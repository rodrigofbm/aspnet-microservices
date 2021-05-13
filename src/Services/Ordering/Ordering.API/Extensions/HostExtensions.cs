using System;
using System.Reflection;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Ordering.API.Extensions
{
    public static class HostExtensions
    {
        public static IHost MigrateDatabase<TContext>(this IHost host, Action<TContext, IServiceProvider> seeder,
            int? retry = 0) where TContext : DbContext
        {
           var retryForAvailability = retry.Value;

           using (var scope = host.Services.CreateScope())
           {
               var services = scope.ServiceProvider;
               var logger = services.GetRequiredService<ILogger<TContext>>();
               var context = services.GetRequiredService<TContext>();

               try
               {
                    logger.LogInformation("Migration database associated with context {DbContextName}",
                        nameof(TContext));
                    InvokeSeeder(seeder, context, services);
                    logger.LogInformation("Migrated database associated with context {DbContextName}",
                        nameof(TContext));
               }
               catch (SqlException ex)
               {
                   logger.LogError(ex, "Migrated database associated with context {DbContextName}",
                       nameof(TContext));
                   if (retryForAvailability < 50)
                   {
                       retryForAvailability++;
                       System.Threading.Thread.Sleep(2000);
                       MigrateDatabase<TContext>(host, seeder, retryForAvailability);
                   }
               }

               return host;
           }
        }

        private static void InvokeSeeder<TContext>(Action<TContext, IServiceProvider> seeder, TContext context,
            IServiceProvider services) where TContext : DbContext
        {
            context.Database.Migrate();
            seeder(context, services);
        }
    }
}