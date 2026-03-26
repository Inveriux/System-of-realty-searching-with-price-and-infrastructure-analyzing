using InfrastructureService.Services.Scoring.Configuration;

namespace InfrastructureService.Services.Scoring.Policies;

public sealed class LeisureCategoryScoringPolicy : GenericCategoryScoringPolicyBase
{
    public override string CategoryCode => "leisure";

    protected override GenericCategoryScoringRulesOptions GetRules(InfrastructureScoringProfileOptions profile)
        => profile.Leisure;
}
