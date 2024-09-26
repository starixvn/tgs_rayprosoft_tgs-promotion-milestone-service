using Microsoft.EntityFrameworkCore;
using Tgs.Payment.Clients;
using Tgs.Promotion.Milestone.Domain.Entities;
using Tgs.Promotion.Milestone.Domain.Interfaces;
using Tgs.Promotion.Milestone.Domain.Models;

namespace Tgs.Promotion.Milestone.Repository;

public class MilestoneRepository(tgsContext context) : IMilestoneRepository
{
    public IAsyncEnumerable<PlayerCondition> GetConditionsAsync(Guid brandId, DateTime from, DateTime to,
        DateTime today,
        CancellationToken cancellationToken)
    {
        var deposits = context.FiveMinutesPaymentStatistics
            .Where(x => x.BrandId == brandId)
            .Where(x => x.TimePoint >= from && x.TimePoint < to)
            .Where(x => x.UserType == (short)UserType.Player)
            .GroupBy(x => x.UserId, (playerCode, list) => new PlayerCondition
            {
                PlayerCode = playerCode,
                DepositAmount = list.Sum(s => s.DepositAmount) ?? 0M
            }).ToAsyncEnumerable();

        var validBets = context.PlayerAggregates
            .Where(x => x.BrandId == brandId)
            .Where(x => x.ReportDateTime >= from && x.ReportDateTime < to)
            .GroupBy(x => x.PlayerCode, (playerCode, list) => new PlayerCondition
            {
                PlayerCode = playerCode,
                ValidBetAmount = list.Sum(s => s.OriginalTurnover)
            }).ToAsyncEnumerable();

        return deposits.Concat(validBets).GroupBy(x => x.PlayerCode).SelectAwait(async x => new PlayerCondition
        {
            PlayerCode = x.Key,
            DepositAmount = await x.SumAsync(s => s.DepositAmount, cancellationToken),
            ValidBetAmount = await x.SumAsync(s => s.ValidBetAmount, cancellationToken)
        });
    }

    public IAsyncEnumerable<ValidMilestone> GetRequirementsFulfilledListAsync(Guid promotionId, string playerCode,
        CancellationToken cancellationToken)
    {
        return context.PromotionMilestones
            .Where(x => x.PromotionId == promotionId && x.PlayerCode == playerCode)
            .Include(x => x.PromotionDistribute)
            .Select(x => new ValidMilestone
            {
                PromotionId = x.PromotionId,
                PlayerCode = x.PlayerCode,
                MilestoneLevel = x.MilestoneLevel,
                Bonus = x.Bonus,
                BucketAmount = x.BucketAmount,
                DepositAmount = x.DepositAmount,
                ValidBetAmount = x.ValidBetAmount,
                CreatedOn = x.CreatedOn,
                SentOn = x.PromotionDistribute == null ? null : x.PromotionDistribute.SentOn
            }).ToAsyncEnumerable();
    }

    public async Task<ValidMilestone?> GetRequirementsFulfilledByIdAsync(Guid promotionId, Guid milestoneId,
        CancellationToken cancellationToken)
    {
        return await context.PromotionMilestones
            .Where(x => x.PromotionId == promotionId && x.Id == milestoneId)
            .Include(x => x.PromotionDistribute)
            .Select(x => new ValidMilestone
            {
                PromotionId = x.PromotionId,
                PlayerCode = x.PlayerCode,
                MilestoneLevel = x.MilestoneLevel,
                Bonus = x.Bonus,
                BucketAmount = x.BucketAmount,
                DepositAmount = x.DepositAmount,
                ValidBetAmount = x.ValidBetAmount,
                CreatedOn = x.CreatedOn,
                SentOn = x.PromotionDistribute == null ? null : x.PromotionDistribute.SentOn
            }).FirstOrDefaultAsync(cancellationToken);
    }

    public async Task CreateMilestoneAsync(CreatePromotionMilestone model, CancellationToken cancellationToken)
    {
        await context.PromotionMilestones.AddAsync(new PromotionMilestone
            {
                Id = model.Id,
                PromotionId = model.PromotionId,
                BrandId = model.BrandId,
                PlayerCode = model.PlayerCode,
                MilestoneLevel = model.MilestoneLevel,
                Bonus = model.Bonus,
                BucketAmount = model.BucketAmount,
                DepositAmount = model.DepositAmount,
                ValidBetAmount = model.ValidBetAmount,
                CreatedOn = DateTime.UtcNow
            }, cancellationToken)
            .ConfigureAwait(false);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateMilestoneReferenceIdAsync(Guid id, Guid distributeId, CancellationToken cancellationToken)
    {
        await context.PromotionMilestones.Where(x => x.Id == id).ExecuteUpdateAsync(
                x => x.SetProperty(p => p.PromotionDistributeId, distributeId), cancellationToken)
            .ConfigureAwait(false);

        await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<(Guid? id, Guid? promotionDistributeId)> GetMilestoneByLevelAsync(Guid brandId, Guid promotionId,
        string playerCode, int milestoneLevel, CancellationToken cancellationToken)
    {
        var milestone = await context.PromotionMilestones.FirstOrDefaultAsync(x =>
            x.BrandId == brandId && x.PromotionId == promotionId && x.PlayerCode == playerCode &&
            x.MilestoneLevel == milestoneLevel, cancellationToken).ConfigureAwait(false);

        return (milestone?.Id, milestone?.PromotionDistributeId);
    }

    public async Task<(decimal depositAmount, decimal validBetAmount)> GetPlayerConditionAsync(Guid brandId,
        string playerCode, DateTime from, DateTime to, CancellationToken cancellationToken)
    {
        var playerDeposit = await context.FiveMinutesPaymentStatistics
            .Where(x => x.BrandId == brandId)
            .Where(x => x.UserType == (short)UserType.Player)
            .Where(x => x.UserId == playerCode)
            .Where(x => x.TimePoint >= from && x.TimePoint < to)
            .GroupBy(x => x.UserId, (playerCode, list) => new
            {
                PlayerCode = playerCode,
                DepositAmount = list.Sum(s => s.DepositAmount) ?? 0M
            }).FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var validBetAmount = await context.GameWagerStatDailyCloses.Where(x => x.BrandId == brandId)
            .Where(x => x.PlayerCode == playerCode)
            .Where(x => x.StatOn >= from && x.StatOn < to)
            .GroupBy(x => x.PlayerCode, (playerCode, list) => new
            {
                PlayerCode = playerCode,
                ValidBetAmount = list.Sum(s => s.Turnover)
            }).FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        return (playerDeposit?.DepositAmount ?? 0M, validBetAmount?.ValidBetAmount ?? 0M);
    }
}