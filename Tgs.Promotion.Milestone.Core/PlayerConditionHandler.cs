using MediatR;
using Tgs.Promotion.Milestone.Domain.Interfaces;
using Tgs.Promotion.Milestone.Domain.Models;

namespace Tgs.Promotion.Milestone.Core;

public class PlayerConditionHandler(IMilestoneRepository milestoneRepository)
    : IRequestHandler<GetPlayerCondition, PlayerCondition>
{
    public async Task<PlayerCondition> Handle(GetPlayerCondition request, CancellationToken cancellationToken)
    {
        var (depositAmount, validBetAmount) = await milestoneRepository.GetPlayerConditionAsync(request.BrandId, request.PlayerCode, request.From, request.To, cancellationToken);

        return new PlayerCondition
        {
            PlayerCode = request.PlayerCode, 
            DepositAmount = depositAmount,
            ValidBetAmount = validBetAmount
        };
    }
}