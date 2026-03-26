using InfrastructureService.Services.Scoring.Configuration;

namespace InfrastructureService.Services.Scoring.Policies;

public sealed class CommerceCategoryScoringPolicy : GenericCategoryScoringPolicyBase
{
    public override string CategoryCode => "commerce";

    protected override GenericCategoryScoringRulesOptions GetRules(InfrastructureScoringProfileOptions profile)
        => profile.Commerce;
}
