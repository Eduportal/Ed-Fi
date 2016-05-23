namespace EdFi.Ods.Admin.UITests.Support.Account
{
    public class Account
    {
        public Account()
        {
            this.Roles = new string[0];
        }

        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string[] Roles { get; set; }
    }
}