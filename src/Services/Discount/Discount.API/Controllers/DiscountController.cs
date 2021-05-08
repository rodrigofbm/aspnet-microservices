using System.Threading.Tasks;
using Discount.API.Entities;
using Discount.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Discount.API.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountRepository _repository;

        public DiscountController(IDiscountRepository repository)
        {
            _repository = repository;
        }

        [HttpGet("{productName}", Name = "GetDiscount")]
        public async Task<ActionResult<Coupon>> GetDiscount(string productName)
        {
            var discount = await _repository.GetDiscount(productName);

            return Ok(discount);
        }
        
        [HttpPost]
        public async Task<ActionResult<Coupon>> CreateDiscount(Coupon coupon)
        {
            var discount = await _repository.CreateDiscount(coupon);

            return CreatedAtRoute("GetDiscount", 
                new {productName = coupon.ProductName}, coupon);
        }
        
        [HttpPut]
        public async Task<ActionResult<Coupon>> ReplaceDiscount(Coupon coupon)
        {
            var discount = await _repository.UpdateDiscount(coupon);

            return CreatedAtRoute("GetDiscount", 
                new {productName = coupon.ProductName}, coupon);
        }
        
        [HttpDelete("{productName}")]
        public async Task<IActionResult> DeleteDiscount(string productName)
        {
            var discount = await _repository.DeleteDiscount(productName);

            return NoContent();
        }
    }
}