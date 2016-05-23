namespace EdFi.Ods.Admin.UITests.Support
{
    using System;

    public class TestUrlBuilder
    {
        private readonly TestSessionContext _context;

        public TestUrlBuilder(TestSessionContext context)
        {
            this._context = context;
        }

        public string BuildUrl(string relativeUrl)
        {
            Console.WriteLine("Base URL:  {0}", this._context.Configuration.ServerBaseUrl);
            var baseAddress = this._context.Configuration.ServerBaseUrl.TrimEnd('/');
            return relativeUrl.Replace("~", baseAddress);
        }
    }
}