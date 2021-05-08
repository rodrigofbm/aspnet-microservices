using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.API.Data;
using Catalog.API.Entities;
using MongoDB.Driver;

namespace Catalog.API.Repositories
{
    public class ProductRepository :  IProductRepository
    {
        private readonly ICatalogContext _context;
        public ProductRepository(ICatalogContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetProducts()
        {
            var products = await _context
                .Products
                .Find(p => true)
                .ToListAsync();

            return products;
        }

        public async Task<Product> GetById(string id)
        {
            return await _context
                .Products
                .Find(p => p.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Product> GetByName(string name)
        {
            return await _context
                .Products
                .Find(p => p.Name == name)
                .FirstOrDefaultAsync();
        }

        public async Task<Product> GetByCategory(string category)
        {
            return await _context
                .Products
                .Find(p => p.Category == category)
                .FirstOrDefaultAsync();
        }

        public async Task CreateProduct(Product product)
        {
            await _context.Products.InsertOneAsync(product);
        }

        public async Task<bool> UpdateProduct(Product product)
        {
            var updated = await _context.Products
                .ReplaceOneAsync(filter: p => p.Id == product.Id, replacement: product);

            return updated.IsAcknowledged && updated.ModifiedCount > 0;
        }

        public async Task<bool> DeleteProduct(string id)
        {
            var deleted = await _context.Products.DeleteOneAsync(filter: p => p.Id == id);
            return deleted.IsAcknowledged && deleted.DeletedCount > 0;
        }
    }
}