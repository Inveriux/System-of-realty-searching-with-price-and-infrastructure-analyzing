using InfrastructureService.Services.Scoring.Configuration;

namespace InfrastructureService.Services.Scoring.Policies;

public sealed class HealthcareCategoryScoringPolicy : GenericCategoryScoringPolicyBase
{
    public override string CategoryCode => "healthcare";
    protected override bool PreferRoutingScore => true;

    protected override GenericCategoryScoringRulesOptions GetRules(InfrastructureScoringProfileOptions profile)
        => profile.Healthcare;
}
