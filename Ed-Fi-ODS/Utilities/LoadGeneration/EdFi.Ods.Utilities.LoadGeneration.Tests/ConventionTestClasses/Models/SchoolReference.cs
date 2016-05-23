namespace EdFi.Ods.Utilities.LoadGeneration.Tests.ConventionTestClasses.Models
{
    public class SchoolReference
    {
        /* School Identity Column */
        public int? schoolId { get; set; }

        /* Represents a hyperlink to the related school resource. */
        public Link link { get; set; }

    }
}
