using System;
using EdFi.Common.Database;
using EdFi.Ods.Common.Context;

namespace EdFi.Ods.Common.Database
{
    public class YearSpecificOdsDatabaseNameProvider : IDatabaseNameProvider
    {
        private readonly ISchoolYearContextProvider schoolYearContextProvider;

        public YearSpecificOdsDatabaseNameProvider(ISchoolYearContextProvider schoolYearContextProvider)
        {
            this.schoolYearContextProvider = schoolYearContextProvider;
        }

        public string GetDatabaseName()
        {
            //Convention: "EdFi_Ods_" + school year.
            int schoolYear = schoolYearContextProvider.GetSchoolYear();

            if (schoolYear == 0)
                throw new InvalidOperationException("The year-specific ODS database name cannot be derived because the school year was not set in the current context.");

            return string.Format("EdFi_Ods_{0}", schoolYear);
        }
    }
}