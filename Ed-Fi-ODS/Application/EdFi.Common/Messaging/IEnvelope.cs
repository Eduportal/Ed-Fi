using System.Collections.Generic;

namespace EdFi.Common.Messaging
{
    public interface IEnvelope<TCommand> where TCommand : ICommand
    {
        TCommand Message { get; set; }
        string ApiKey { get; set; }
        int SchoolYear { get; set; }
        IEnumerable<int> EducationOrganizationIds { get; set; }
        string ClaimSetName { get; set; }
        string NamespacePrefix { get; set; }
    }
}