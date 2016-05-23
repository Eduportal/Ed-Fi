using System.Collections.Generic;

namespace EdFi.Common.Messaging
{
    public class Envelope<TCommand> : IEnvelope<TCommand> where TCommand : ICommand
    {
        public Envelope()
        {
            EducationOrganizationIds = new List<int>();
        }

        public Envelope(TCommand message) : this()
        {
            Message = message;
        }

        public TCommand Message { get; set; }
        public string ApiKey { get; set; }
        public int SchoolYear { get; set; }
        public IEnumerable<int> EducationOrganizationIds { get; set; }
        public string ClaimSetName { get; set; }
        public string NamespacePrefix { get; set; }
    }
}
