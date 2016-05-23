using System.Collections.Generic;

namespace EdFi.Ods.Pipelines
{
    public interface IHasPersistentModels<TEntityModel>
        where TEntityModel : class
    {
        IList<TEntityModel> PersistentModels { get; set; }
    }
}