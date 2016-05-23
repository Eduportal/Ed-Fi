// *************************************************************************
// ©2012 Ed-Fi Alliance, LLC. All Rights Reserved.
// *************************************************************************

using NUnit.Framework;
using Rhino.Mocks;

namespace EdFi.Ods.XmlShredding.Tests
{
    [TestFixture]
    public abstract class TestFixtureBase
    {
        protected MockRepository mocks;

        [TestFixtureSetUp]
        public virtual void RunOnceBeforeAny()
        {
            // Initialize NBuilder settings
            //BuilderSetup.SetDefaultPropertyNamer(new NonDefaultNonRepeatingPropertyNamer(new ReflectionUtil()));

            // Create a mock repository for new mocks
            mocks = new MockRepository();
            
            EstablishContext();

            // Stop recording
            mocks.ReplayAll();

            try
            {
                // Allow execution of code just prior to test execution
                BeforeExecuteTest();

                // Execute the test
                ExecuteTest();
            }
            finally
            {
                // Allow cleanup surrounding test execution, prior to final cleanup
                AfterExecuteTest();
            }
        }

        [TestFixtureTearDown]
        public virtual void RunOnceAfterAll()
        {
            // Make sure all objects are now in replay mode
            mocks.ReplayAll();

            // Make sure all defined mocks are satisfied
            mocks.VerifyAll();
        }
        
        protected virtual void EstablishContext() { }

        protected virtual void BeforeExecuteTest() { }
        protected virtual void AfterExecuteTest() { }

        /// <summary>
        /// Executes the code to be tested.
        /// </summary>
        protected abstract void ExecuteTest();
    }
}
