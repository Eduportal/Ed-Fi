namespace EdFi.Ods.Admin.UITests.Support.Account
{
    public interface IEmailReader
    {
        string FindLink(string email);
        void ClearMailbox(string email);
    }
}