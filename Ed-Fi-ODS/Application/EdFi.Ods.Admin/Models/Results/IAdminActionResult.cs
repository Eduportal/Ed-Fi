namespace EdFi.Ods.Admin.Models.Results
{
    public interface IAdminActionResult
    {
        bool Success { get; set; }
        string Message { get; set; }
    }
}