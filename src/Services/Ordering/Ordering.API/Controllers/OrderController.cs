using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Features.Orders.Commands.CheckoutOrder;
using Ordering.Application.Features.Orders.Commands.DeleteOrder;
using Ordering.Application.Features.Orders.Commands.UpdateOrder;
using Ordering.Application.Features.Orders.Queries.GetOrdersList;

namespace Ordering.API.Controllers
{
    public class OrderController : BaseController
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{username}", Name = "GetOrder")]
        public async Task<ActionResult<OrderVm>> GetOrderByUserName(string username)
        {
            var query = new GetOrderListQuery(username);
            var orders = await _mediator.Send(query);

            return Ok(orders);
        }
        
        [HttpPost]
        public async Task<ActionResult<int>> CheckoutOrder(CheckoutOrderCommand command)
        {
            var result = await _mediator.Send(command);

            return Ok(result);
        }

        [HttpPut(Name = "UpdateOrder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType()]
        public async Task<ActionResult> ReplaceOrder(UpdateOrderCommand command)
        {
            await _mediator.Send(command);

            return NoContent();
        }
        
        [HttpDelete("{orderId}",Name = "DeleteOrder")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType()]
        public async Task<ActionResult> RemoveOrder(int orderId)
        {
            var command = new DeleteOrderCommand {Id = orderId};
            await _mediator.Send(command);

            return NoContent();
        }
    }
}