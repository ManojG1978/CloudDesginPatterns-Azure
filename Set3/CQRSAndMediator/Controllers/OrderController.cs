using System.Threading.Tasks;
using CQRSAndMediator.RequestModels.CommandRequestModels;
using CQRSAndMediator.RequestModels.QueryRequestModels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CQRSAndMediator.Controllers
{
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;
        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("makeorder")]
        public async Task<ActionResult> MakeOrder([FromBody] MakeOrderRequestModel requestModel)
        {
            var response = await _mediator.Send(requestModel);
            return Ok(response);
        }

        [HttpGet("getorder")]
        public async Task<ActionResult> OrderDetails([FromQuery] GetOrderByIdRequestModel requestModel)
        {
            var response = await _mediator.Send(requestModel);
            return Ok(response);
        }
        
    }
}