using System;

namespace EdFi.Ods.Common
{
    public interface IHasIdentifier
    {
        Guid Id { get; set; }
    }
}
