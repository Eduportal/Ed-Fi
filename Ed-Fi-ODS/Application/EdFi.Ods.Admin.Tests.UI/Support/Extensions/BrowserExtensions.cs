namespace EdFi.Ods.Admin.UITests.Support.Extensions
{
    using global::Coypu;

    public static class BrowserExtensions
    {
        public static bool HasNoAddress(this BrowserSession browser)
        {
            return browser.Location.ToString() == "about:blank";
        }
    }
}