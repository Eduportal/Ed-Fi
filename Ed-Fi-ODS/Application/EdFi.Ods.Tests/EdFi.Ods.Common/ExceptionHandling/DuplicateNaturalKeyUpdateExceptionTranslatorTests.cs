using System;
using EdFi.Ods.Common.ExceptionHandling;
using EdFi.Ods.Common.ExceptionHandling.Translators;
using EdFi.Ods.Tests._Bases;
using NHibernate;
using NHibernate.Exceptions;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.Common.ExceptionHandling
{
    public class DuplicateNaturalKeyUpdateExceptionTranslatorTests
    {
        [TestFixture]
        public class When_a_regular_exception_is_thrown : TestFixtureBase
        {
            private Exception exception;
            private bool result;

            protected override void EstablishContext()
            {
                exception = new Exception();
            }

            protected override void ExecuteBehavior()
            {
                var translator = new DuplicateNaturalKeyUpdateExceptionTranslator();
                RESTError actualError;
                result = translator.TryTranslateMessage(exception, out actualError);
            }

            [Test]
            public void Should_not_handle_this_exception()
            {
                result.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_a_generic_ADO_exception_is_thrown_without_an_inner_exception
            : TestFixtureBase
        {
            private Exception exception;
            private bool wasHandled;
            RESTError actualError;

            protected override void EstablishContext()
            {
                exception = new GenericADOException("Generic exception message", null);
            }

            protected override void ExecuteBehavior()
            {
                var translator = new DuplicateNaturalKeyUpdateExceptionTranslator();
                wasHandled = translator.TryTranslateMessage(exception, out actualError);
            }

            [Test]
            public void Should_not_handle()
            {
                wasHandled.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_a_generic_ADO_exception_is_thrown_with_an_inner_exception_with_the_wrong_message
            : TestFixtureBase
        {
            private Exception exception;
            private bool wasHandled;
            RESTError actualError;

            protected override void EstablishContext()
            {
                const string slightlyWrongMessage = "VViolation of PRIMARY KEY constraint 'PK_Session'. Cannot insert duplicate key in object 'edfi.Session'. The duplicate key value is (900007, 9, 2014). The statement has been terminated.";
                exception = NHibernateExceptionBuilder.CreateException("Some generic SQL Exception message", slightlyWrongMessage);
            }

            protected override void ExecuteBehavior()
            {
                var translator = new DuplicateNaturalKeyUpdateExceptionTranslator();
                wasHandled = translator.TryTranslateMessage(exception, out actualError);
            }

            [Test]
            public void Should_not_handle()
            {
                wasHandled.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_an_nHibernate_StaleObjectState_exception_is_thrown : TestFixtureBase
        {
            private Exception exception;
            private bool result;
            RESTError actualError;
            
            protected override void EstablishContext()
            {
                exception = new StaleObjectStateException("Some entity", "some object key");
            }

            protected override void ExecuteBehavior()
            {
                var translator = new DuplicateNaturalKeyUpdateExceptionTranslator();
                result = translator.TryTranslateMessage(exception, out actualError);
            }

            [Test]
            public void Should_handle_this_exception()
            {
                result.ShouldBeTrue();
            }

            [Test]
            public void Should_translate_the_exception_to_a_409_error()
            {
                actualError.Code.ShouldEqual(409);
            }

            [Test]
            public void Should_set_the_exception_type_to_conflict()
            {
                actualError.Type.ShouldEqual("Conflict");
            }

            [Test]
            public void Should_set_a_reasonable_message()
            {
                actualError.Message.ShouldEqual("A natural key conflict occurred when attempting to update a new resource with a duplicate key. This is likely caused by multiple resources with the same key in the same file. Exactly one of these resources was updated.");
            }
        }

        [TestFixture]
        public class When_an_nHibernate_ADO_exception_is_thrown_with_an_inner_exception_of_the_wrong_type : TestFixtureBase
        {
            private Exception exception;
            private bool result;
            RESTError actualError;
            
            protected override void EstablishContext()
            {
                var mess = "Violation of PRIMARY KEY constraint 'PK_Session'. Cannot insert duplicate key in object 'edfi.Session'. The duplicate key value is (900007, 9, 2014). The statement has been terminated.";
                var innerexception = new Exception(mess);
                exception = new GenericADOException("Generic exception message", innerexception);
            }

            protected override void ExecuteBehavior()
            {
                var translator = new DuplicateNaturalKeyUpdateExceptionTranslator();
                result = translator.TryTranslateMessage(exception, out actualError);
            }

            [Test]
            public void Should_not_handle_this_exception()
            {
                result.ShouldBeFalse();
            }
        }
    }
}