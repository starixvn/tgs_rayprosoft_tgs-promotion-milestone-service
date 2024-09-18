using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tgs.Promotion.Milestone.Domain.Models;
using Tgs.Promotion.Milestone.Service.Models;

namespace Tgs.Promotion.Milestone.Service.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MilestoneController(ISender sender) : ControllerBase
    {
        [HttpGet("valid-list/{promotionId:guid}/{playerCode}")]
        public IAsyncEnumerable<ValidMilestone> GetMilestoneRequirementsFulfilledListAsync(Guid promotionId, string playerCode)
        {
            return sender.CreateStream(new GetMilestoneRequirementsFulfilledList(promotionId, playerCode));
        }

        [HttpGet("condition")]
        public Task<PlayerCondition> GetPlayerConditionAsync([FromQuery]GetPlayerConditionRequest request)
        {
            return sender.Send(new GetPlayerCondition(request.BrandId, request.PlayerCode, request.From,request.To));
        }
    }
}
