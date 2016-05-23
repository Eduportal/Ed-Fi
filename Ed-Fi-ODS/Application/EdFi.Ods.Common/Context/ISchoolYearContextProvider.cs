namespace EdFi.Ods.Common.Context
{
    public interface ISchoolYearContextProvider
    {
        int  GetSchoolYear();
        void SetSchoolYear(int schoolYear);
    }
}