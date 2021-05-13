using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
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
        private readonly IMapper _mapper;
        private readonly DiscountGrpcService _discountGrpcService;
        private readonly IPublishEndpoint _publishEndpoint;

        public BasketController(IBasketRepository repository, ILogger<BasketController> logger, IMapper mapper, DiscountGrpcService discountGrpcService, IPublishEndpoint publishEndpoint)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
            _discountGrpcService = discountGrpcService;
            _publishEndpoint = publishEndpoint;
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
            foreach (var item in basket.Items)
            {
                var discount = await _discountGrpcService.GetDiscount(item.ProductName);
                item.Price -= discount.Amount;
            }

            await _repository.UpdateBasket(basket);
            
            return CreatedAtRoute("GetBasket", new {userName = basket.UserName}, basket);
        }
        
        [HttpDelete("{userName}")]
        public IActionResult DeleteBasket(string userName)
        {
            _repository.DeleteBasket(userName);

            return NoContent();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> Checkout(BasketCheckout basketCheckout)
        {
            // get existing basket with total price by username
            var basket = await _repository.GetBasket(basketCheckout.UserName);
            if (basket == null) return BadRequest("Basket not found");
            var totalPrice = basket.TotalPrice;
            
            // Create basketCheckoutEvent -- Set total price on basket checkout event
            var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
            eventMessage.TotalPrice = totalPrice;
            
            // send checkout event to rabbitmq
            await _publishEndpoint.Publish(eventMessage);
            
            // remove the basket
            _repository.DeleteBasket(basketCheckout.UserName);
            
            return Accepted();
        }
    }
}