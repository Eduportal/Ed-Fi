using System;
using System.Collections.Generic;

namespace EdFi.Ods.Common
{
    public interface IHasIdentifiers<TId>
    {
        // Before changing return type (i.e. to IList<T>), make sure this works for ServiceStack request model binding
        List<Guid> Ids { get; set; }
    }
}