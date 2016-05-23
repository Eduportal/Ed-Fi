using System;

namespace EdFi.Ods.Tests._Bases
{
    using NUnit.Framework;

    using Rhino.Mocks;

    [TestFixture]
    public abstract class TestFixtureBase
    {
        protected MockRepository mocks;

        private Exception _actualException;
        private bool _actualExceptionInspected;

        protected Exception ActualException
        {
            get
            {
                _actualExceptionInspected = true;
                return _actualException;
            }
            set
            {
                _actualExceptionInspected = false;
                _actualException = value;
            }
        }

        [TestFixtureSetUp]
        public virtual void RunOnceBeforeAny()
        {
            // Initialize NBuilder settings
            //BuilderSetup.SetDefaultPropertyNamer(new NonDefaultNonRepeatingPropertyNamer(new ReflectionUtil()));

            // Create a mock repository for new mocks
            this.mocks = new MockRepository();

            //Arrange
            this.EstablishContext();
            this.Arrange();

            // Stop recording
            this.mocks.ReplayAll();

            //Act
            try
            {
                // Allow execution of code just prior to behavior execution
                this.BeforeBehaviorExecution();

                // Execute the behavior
                try
                {
                    this.ExecuteBehavior();
                    this.Act();
                }
                catch (Exception ex)
                {
                    ActualException = ex;
                }
            }
            finally
            {
                // Allow cleanup surrounding behavior execution, prior to final cleanup
                this.AfterBehaviorExecution();
            }
        }

        [TestFixtureTearDown]
        public virtual void RunOnceAfterAll()
        {
            // Make sure all objects are now in replay mode
            this.mocks.ReplayAll();

            // Make sure all defined mocks are satisfied
            this.mocks.VerifyAll();

            // Make sure exception was inspected.
            if (_actualException != null && !_actualExceptionInspected)
            {
                throw new AssertionException(
                    string.Format(
                        "The exception of type '{0}' was not inspected by the test:\r\n {1}.",
                        _actualException.GetType().Name, _actualException));
            }
        }

        protected virtual void Arrange() { }
        protected virtual void EstablishContext() { }

        protected virtual void BeforeBehaviorExecution() { }
        protected virtual void AfterBehaviorExecution() { }

        /// <summary>
        /// Executes the code to be tested.
        /// </summary>
        protected virtual void Act() { }
        protected virtual void ExecuteBehavior() { }

        protected T Stub<T>() where T : class
        {
            return MockRepository.GenerateStub<T>();
        }

        ///// <summary>
        ///// Make sure that the test fixture isn't skipped because there is no actual test defined.  This happens in some of the
        ///// earlier test fixtures that only test interactions with external dependencies, which causes the test runners to skip
        ///// over them.  Once these fixtures can all be identified this method should be removed.
        ///// </summary>
        //[Test]
        //public void Ensure_test_fixture_execution()
        //{
        //}
    }
}