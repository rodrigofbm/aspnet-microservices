using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Discount.gRPC.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, int? retry = 0)
        {
            var retryForAvailability = retry.Value;

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var config = services.GetRequiredService<IConfiguration>();
                var loggerFactory = services.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger<Program>();
                
                try
                {
                    logger.LogInformation("Migration postgresql database");
                    var connection = new NpgsqlConnection(
                        config.GetValue<string>("DatabaseSettings:ConnectionString"));
                    connection.Open();

                    using var command = new NpgsqlCommand { Connection = connection };
                    command.CommandText = "DROP TABLE IF EXISTS coupon";
                    command.ExecuteNonQuery();
                    command.CommandText = @"create table Coupon (
	                    ID serial primary key not null,
	                    ProductName varchar(24) not null,
	                    Description text,
	                    Amount int
                    );";
                    command.ExecuteNonQuery();
                    command.CommandText = @"insert into coupon (productname, description, amount) 
                        values('Iphone X', 'iPhone Discount', 150);";
                    command.ExecuteNonQuery();
                    command.CommandText = @"insert into coupon (productname, description, amount) 
                        values('Samsung 10', 'Samsung Discount', 150);";
                    command.ExecuteNonQuery();
                    
                    logger.LogInformation("Migrated postgresql database");
                }
                catch (NpgsqlException e)
                {
                    logger.LogError(e.Message, e);
                    if (retryForAvailability < 50)
                    {
                        retryForAvailability++;
                        System.Threading.Thread.Sleep(2000);
                        MigrateDatabase<TContext>(host, retryForAvailability);
                    }
                }
            }

            return host;
        }
    }
}