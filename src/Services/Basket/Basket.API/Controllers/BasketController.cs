using System.Threading.Tasks;
using Basket.API.Entities;
using Basket.API.GrpcServices;
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
        private readonly DiscountGrpcService _discountGrpcService;

        public BasketController(IBasketRepository repository, ILogger<BasketController> logger, DiscountGrpcService discountGrpcService)
        {
            _repository = repository;
            _logger = logger;
            _discountGrpcService = discountGrpcService;
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
            // TODO: Communicate with Discount.gRPC
            // TODO: calculate latest price of the product
            var b = await _repository.UpdateBasket(basket);

            foreach (var item in b.Items)
            {
                var discount = await _discountGrpcService.GetDiscount(item.ProductName);
                item.Price -= discount.Amount;
            }

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