using System.Threading.Tasks;
using Basket.API.Entities;
using Basket.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Basket.API.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class BasketController : ControllerBase
    {
        private readonly IBasketRepository _repository;
        private readonly ILogger<BasketController> _logger;

        public BasketController(IBasketRepository repository, ILogger<BasketController> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        [HttpGet("{userName}", Name = "GetBasket")]
        public async Task<ActionResult<ShoppingCart>> GetBasket(string userName)
        {
            var basket = await _repository.GetBasket(userName);
            
            return Ok(basket ?? new ShoppingCart(userName));
        }
        
        [HttpPost]
        public async Task<ActionResult<ShoppingCart>> UpdateBasket(ShoppingCart basket)
        {
            var b = await _repository.UpdateBasket(basket);

            return CreatedAtRoute("GetBasket", new {userName = basket.UserName}, b);
        }
        
        [HttpDelete("{userName}")]
        public IActionResult DeleteBasket(string userName)
        {
            _repository.DeleteBasket(userName);

            return NoContent();
        }
    }
}