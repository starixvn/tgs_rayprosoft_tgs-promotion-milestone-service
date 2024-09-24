using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Promotion.Milestone.Domain.Models;
using Tgs.Promotion.Milestone.Service.Models;

namespace Tgs.Promotion.Milestone.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MilestoneController(ISender sender, ILogger<MilestoneController> logger) : ControllerBase
    {
        [HttpGet("conditions-Include-completed/{promotionId:guid}/{playerCode}")]
        public IAsyncEnumerable<ConditionIncludeCompleted> GetConditionsIncludeCompletedListAsync(Guid promotionId, string playerCode)
        {
            logger.LogInformation($"conditions-Include-completed {promotionId} {playerCode}", promotionId, playerCode);

            return sender.CreateStream(new GetConditionsIncludeCompletedList(promotionId, playerCode));
        }

        [HttpGet("condition")]
        public Task<PlayerCondition> GetPlayerConditionAsync([FromQuery]GetPlayerConditionRequest request)
        {
            logger.LogInformation("condition {request}", request);

            return sender.Send(new GetPlayerCondition(request.BrandId, request.PlayerCode, request.From,request.To));
        }
    }
}
