namespace EdFi.Ods.Admin.Services
{
    public interface IRouteService
    {
        string GetRouteForPasswordReset(string marker);
        string GetRouteForActivation(string marker);
    }
}