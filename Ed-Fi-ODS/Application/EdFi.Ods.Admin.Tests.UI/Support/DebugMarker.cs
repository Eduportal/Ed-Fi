namespace EdFi.Ods.Admin.UITests.Support
{
    using NUnit.Framework;

    public class DebugMarker
    {
        public bool IsDebug { get; private set; }

        public void SetDebug()
        {
            this.IsDebug = true;
        }

        public void Fail()
        {
            Assert.Fail("Debug mode has been used to prevent the browser from closing.  Please turn off debug mode before committing.");
        }
    }
}