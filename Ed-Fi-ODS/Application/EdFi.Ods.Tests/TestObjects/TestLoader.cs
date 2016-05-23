namespace EdFi.Ods.Tests.TestObjects
{
    using System;
    using System.Threading.Tasks;

    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.BulkLoad.Core.Controllers;

    public class TestLoader : IInterchangeController
    {
        private readonly string _type;
        private Func<LoadResult> _testSuppliedBehavior; 

        public TestLoader(string type)
        {
            this._type = type;
        }

        public void SetLoadFromBehavior(Func<LoadResult> given)
        {
            this._testSuppliedBehavior = given;
        }

        public async Task<LoadResult> LoadAsync(string fromFile)
        {
            return this._testSuppliedBehavior();
        }
    }
}