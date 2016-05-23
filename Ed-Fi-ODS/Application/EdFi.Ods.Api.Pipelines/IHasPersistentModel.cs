namespace EdFi.Ods.Pipelines
{
    public interface IHasPersistentModel<TEntityModel>
        where TEntityModel : class
    {
        TEntityModel PersistentModel { get; set; }
    }
}