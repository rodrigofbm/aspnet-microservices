using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.API.Entities;
using Catalog.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Catalog.API.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class CatalogController : ControllerBase
    {
        private readonly IProductRepository _repository;
        private readonly ILogger _logger;

        public CatalogController(IProductRepository repository, ILogger<CatalogController> logger)
        {
            _logger = logger;
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            return Ok(await _repository.GetProducts());
        }
        
        [HttpGet("{id:length(24)}", Name = "GetProduct")]
        public async Task<ActionResult<Product>> GetProduct(string id)
        {
            var product = await _repository.GetById(id);
            if (product == null) return NotFound();

            return product;
        }
        
        [HttpGet("{name}")]
        public async Task<ActionResult<Product>> GetProductByName(string name)
        {
            var product = await _repository.GetByName(name);
            if (product == null) return NotFound();

            return product;
        }
        
        [HttpGet("{category}")]
        public async Task<ActionResult<Product>> GetProductByCategory(string category)
        {
            var product = await _repository.GetByCategory(category);
            if (product == null) return NotFound();

            return product;
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] Product product)
        {
            if (product == null) return BadRequest();
            await _repository.CreateProduct(product);

            return CreatedAtRoute("GetProduct", new { id = product.Id}, product);
        }
        
        [HttpPut]
        public async Task<ActionResult<bool>> Replace(Product product)
        {
            if (product == null) return BadRequest();
            return await _repository.UpdateProduct(product);
        }
        
        [HttpDelete("{id:length(24)}")]
        public async Task<ActionResult<bool>> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("O id é obrigatório");
            
            return await _repository.DeleteProduct(id);
        }
    }
}