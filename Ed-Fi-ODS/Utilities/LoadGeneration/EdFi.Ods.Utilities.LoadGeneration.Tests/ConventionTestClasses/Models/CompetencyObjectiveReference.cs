namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ConventionTestClasses.Models {
  public class CompetencyObjectiveReference {
    /* The designated title of the competency objective. */
    public string objective { get; set; }

    /* The grade level for which the competency objective is targeted, */
    public string objectiveGradeLevelDescriptor { get; set; }

    /* The education organization that defined the competency objective */
    public int? educationOrganizationId { get; set; }

    /* Represents a hyperlink to the related competencyObjective resource. */
    public Link link { get; set; }

    }
}
