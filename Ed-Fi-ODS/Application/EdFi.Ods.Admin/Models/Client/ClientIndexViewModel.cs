
namespace EdFi.Ods.Admin.Models.Client
{
    public class ClientIndexViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ApplicationName { get; set; }
        public string Key { get; set; }
        public string Secret { get; set; }
        public string Status { get; set; }
        public string SandboxTypeName { get; set; }
        public bool IsLoading { get { return Status != ApiClientStatus.Online; } }
    }
}