using System.Collections.Generic;

namespace EdFi.Ods.CodeGen.DatabaseSchema
{
    public class DatabaseSchemaProviderDecoratorBase : IDatabaseSchemaProvider
    {
        private readonly IDatabaseSchemaProvider _decoratedProvider;

        public DatabaseSchemaProviderDecoratorBase(IDatabaseSchemaProvider decoratedProvider)
        {
            _decoratedProvider = decoratedProvider;
        }

        public virtual List<Table> LoadTables()
        {
            return _decoratedProvider.LoadTables();
        }

        public virtual List<Index> GetIndices()
        {
            return _decoratedProvider.GetIndices();
        }

        public virtual void EnsureAllConnectionsClosed()
        {
            _decoratedProvider.EnsureAllConnectionsClosed();
        }
    }
}