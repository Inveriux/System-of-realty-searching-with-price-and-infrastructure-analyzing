using InfrastructureService.Services.Scoring.Configuration;

namespace InfrastructureService.Services.Scoring.Policies;

public sealed class EducationCategoryScoringPolicy : GenericCategoryScoringPolicyBase
{
    public override string CategoryCode => "education";

    protected override GenericCategoryScoringRulesOptions GetRules(InfrastructureScoringProfileOptions profile)
        => profile.Education;
}
