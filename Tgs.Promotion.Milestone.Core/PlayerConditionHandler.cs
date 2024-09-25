using MediatR;
using Tgs.Promotion.Clients;
using Tgs.Promotion.Milestone.Domain.Interfaces;
using Tgs.Promotion.Milestone.Domain.Models;

namespace Tgs.Promotion.Milestone.Core;

public class PlayerConditionHandler(IPromotionServiceClient promotionServiceClient, IMilestoneRepository milestoneRepository)
    : IRequestHandler<GetPlayerCondition, PlayerCondition>
{
    public async Task<PlayerCondition> Handle(GetPlayerCondition request, CancellationToken cancellationToken)
    {
        var promotion = await promotionServiceClient.GetPromotionMainInfoAsync(request.PromotionId, cancellationToken);

        var (depositAmount, validBetAmount) = await milestoneRepository.GetPlayerConditionAsync(request.BrandId,
            request.PlayerCode, promotion.EventDuring.From, promotion.EventDuring.To, cancellationToken);

        return new PlayerCondition
        {
            PlayerCode = request.PlayerCode,
            DepositAmount = depositAmount,
            ValidBetAmount = validBetAmount
        };
    }
}