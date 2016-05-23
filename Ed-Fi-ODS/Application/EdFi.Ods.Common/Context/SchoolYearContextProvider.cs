using System.Security.Principal;
using EdFi.Common.Context;

namespace EdFi.Ods.Common.Context
{
    public class SchoolYearContextProvider : ISchoolYearContextProvider
    {
        private const string SchoolYearContextKey = "SchoolYearContextProvider.SchoolYear";
        
        private readonly IContextStorage contextStorage;

        public SchoolYearContextProvider(IContextStorage contextStorage)
        {
            this.contextStorage = contextStorage;
        }

        public int GetSchoolYear()
        {
            return contextStorage.GetValue<int>(SchoolYearContextKey);
        }

        public void SetSchoolYear(int schoolYear)
        {
            contextStorage.SetValue(SchoolYearContextKey, schoolYear);
        }
    }
}