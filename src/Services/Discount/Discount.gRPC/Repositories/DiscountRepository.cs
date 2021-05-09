using System.Threading.Tasks;
using Dapper;
using Discount.gRPC.Entities;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Discount.gRPC.Repositories
{
    public class DiscountRepository : IDiscountRepository
    {
        private readonly IConfiguration _config;

        public DiscountRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<Coupon> GetDiscount(string productName)
        {
           using  var connection = new NpgsqlConnection(_config
               .GetValue<string>("DatabaseSettings:ConnectionString"));

           var coupon = await connection.QueryFirstOrDefaultAsync<Coupon>(
               "SELECT * FROM Coupon WHERE productname = @productname",
               new {productName = productName}
               );
           if (coupon == null)
           {
               return new Coupon
               {
                   ProductName = "No discount",
                   Amount = 0,
                   Description = "No discount"
               };
           }

           return coupon;
        }

        public async Task<bool> CreateDiscount(Coupon coupon)
        {
            using  var connection = new NpgsqlConnection(_config
                .GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected = await connection.ExecuteAsync(
                @"INSERT INTO Coupon (productname, description, amount) 
                VALUES (@productname, @description, @amount)",
                new {productName = coupon.ProductName, description = coupon.Description, amount = coupon.Amount}
            );
            
            return affected != 0;
        }

        public async Task<bool> UpdateDiscount(Coupon coupon)
        {
            using  var connection = new NpgsqlConnection(_config
                .GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected = await connection.ExecuteAsync(
                @"UPDATE Coupon SET productname=@productname, description=@description, amount=@amount
                     WHERE id=@id",
                new
                {
                    productName = coupon.ProductName, 
                    description = coupon.Description, 
                    amount = coupon.Amount, 
                    id = coupon.Id
                }
            );
            
            return affected != 0;
        }

        public async Task<bool> DeleteDiscount(string productName)
        {
            using  var connection = new NpgsqlConnection(_config
                .GetValue<string>("DatabaseSettings:ConnectionString"));

            var affected = await connection.ExecuteAsync(
                @"DELETE FROM Coupon WHERE productname=@productname",
                new {productName = productName}
            );
            
            return affected != 0;
        }
    }
}