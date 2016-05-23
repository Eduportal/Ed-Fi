namespace EdFi.Ods.Tests.EdFi.Ods.Common.ExceptionHandling
{
    using System;
    using System.Collections.Generic;
    using System.Net;

    using global::EdFi.Ods.Common.ExceptionHandling;
    using global::EdFi.Ods.Common.ExceptionHandling.Translators;
    using global::EdFi.Ods.Tests._Bases;

    using NHibernate.Exceptions;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    public class When_an_insert_conflicts_with_a_foreign_key_constraint_with_a_single_column : TestFixtureBase
    {
        private Exception suppliedInsertException;
        private RESTError actualError;

        protected override void EstablishContext()
        {
            this.suppliedInsertException =
                NHibernateExceptionBuilder.CreateException(
                "could not execute batch command.[SQL: SQL not available]",
                "The INSERT statement conflicted with the FOREIGN KEY constraint \"FK_StudentAddress_AddressType_AddressTypeId\". The conflict occurred in database \"EdFi_Ods\", table \"edfi.AddressType\", column 'AddressTypeId'.\r\nThe statement has been terminated.\r\n");
        }

        protected override void ExecuteBehavior()
        {
            var translator = new SqlServerConstraintExceptionTranslator();
            translator.TryTranslateMessage(this.suppliedInsertException, out this.actualError);
        }

        [Test]
        public virtual void Should_respond_with_a_409_Conflict()
        {
            Assert.That(this.actualError.Code, Is.EqualTo((int) HttpStatusCode.Conflict));
            Assert.That(this.actualError.Type, Is.EqualTo("Conflict"));
        }

        [Test]
        public virtual void Should_translate_the_message_to_indicate_that_a_related_resource_does_not_have_the_value_specified_in_the_current_request_but_does_not_provide_column_level_details()
        {
            Assert.That(this.actualError.Message, Is.EqualTo("The value supplied for the related 'addressType' resource does not exist."));
        }
    }    
    
    public class When_an_update_conflicts_with_a_foreign_key_constraint_with_a_single_column : TestFixtureBase
    {
        private Exception suppliedUpdateException;
        private RESTError actualError;

        protected override void EstablishContext()
        {
            this.suppliedUpdateException = NHibernateExceptionBuilder.CreateException(
                "could not update: [something-a-rather][SQL: SQL not available]",
                "The UPDATE statement conflicted with the FOREIGN KEY constraint \"FK_Student_LimitedEnglishProficiencyType_LimitedEnglishProficiencyTypeId\". The conflict occurred in database \"EdFi_Ods\", table \"edfi.LimitedEnglishProficiencyType\", column 'LimitedEnglishProficiencyTypeId'.\r\nThe statement has been terminated.\r\n");
        }

        protected override void ExecuteBehavior()
        {
            var translator = new SqlServerConstraintExceptionTranslator();
            translator.TryTranslateMessage(this.suppliedUpdateException, out this.actualError);
        }

        [Test]
        public virtual void Should_respond_with_a_409_Conflict()
        {
            Assert.That(this.actualError.Code, Is.EqualTo((int) HttpStatusCode.Conflict));
            Assert.That(this.actualError.Type, Is.EqualTo("Conflict"));
        }

        [Test]
        public virtual void Should_translate_the_message_to_indicate_that_a_related_resource_does_not_have_the_value_specified_in_the_current_request_but_does_not_provide_column_details()
        {
            Assert.That(this.actualError.Message, Is.EqualTo("The value supplied for the related 'limitedEnglishProficiencyType' resource does not exist."));
        }
    }

    public class When_a_delete_conflicts_with_a_reference_constraint_with_multiple_columns : TestFixtureBase
    {
        private Exception suppliedUpdateException;
        private RESTError actualError;

        protected override void EstablishContext()
        {
            this.suppliedUpdateException = NHibernateExceptionBuilder.CreateException(
                "could not delete: [something-a-rather][SQL: SQL not available]",
                "The DELETE statement conflicted with the REFERENCE constraint \"FK_DisciplineAction_DisciplineIncident_SchoolId\". The conflict occurred in database \"EdFi_Ods\", table \"edfi.DisciplineAction\".\r\nThe statement has been terminated.");
        }

        protected override void ExecuteBehavior()
        {
            var translator = new SqlServerConstraintExceptionTranslator();
            translator.TryTranslateMessage(this.suppliedUpdateException, out this.actualError);
        }

        [Test]
        public virtual void Should_respond_with_a_409_Conflict()
        {
            Assert.That(this.actualError.Code, Is.EqualTo((int)HttpStatusCode.Conflict));
            Assert.That(this.actualError.Type, Is.EqualTo("Conflict"));
        }

        [Test]
        public virtual void Should_translate_the_message_to_indicate_that_a_related_resource_does_not_have_the_value_specified_in_the_current_request_but_does_not_provide_column_details()
        {
            Assert.That(this.actualError.Message, Is.EqualTo("The resource (or a subordinate entity of the resource) cannot be deleted because it is a dependency of the 'disciplineAction' entity."));
        }
    }

    public class When_a_delete_conflicts_with_a_reference_constraint_with_a_single_column : TestFixtureBase
    {
        private Exception suppliedUpdateException;
        private RESTError actualError;

        protected override void EstablishContext()
        {
            this.suppliedUpdateException = NHibernateExceptionBuilder.CreateException(
                "could not delete: [something-a-rather][SQL: SQL not available]",
                "The DELETE statement conflicted with the REFERENCE constraint \"FK_CourseTranscript_CourseAttemptResultType_CourseAttemptResultTypeId\". The conflict occurred in database \"EdFi_Ods\", table \"edfi.CourseTranscript\", column 'CourseAttemptResultTypeId'.\r\nThe statement has been terminated.");
        }

        protected override void ExecuteBehavior()
        {
            var translator = new SqlServerConstraintExceptionTranslator();
            translator.TryTranslateMessage(this.suppliedUpdateException, out this.actualError);
        }

        [Test]
        public virtual void Should_respond_with_a_409_Conflict()
        {
            Assert.That(this.actualError.Code, Is.EqualTo((int)HttpStatusCode.Conflict));
            Assert.That(this.actualError.Type, Is.EqualTo("Conflict"));
        }

        [Test]
        public virtual void Should_translate_the_message_to_indicate_that_a_related_resource_does_not_have_the_value_specified_in_the_current_request_and_provide_column_details()
        {
            Assert.That(this.actualError.Message, Is.EqualTo("The resource (or a subordinate entity of the resource) cannot be deleted because it is a dependency of the 'courseAttemptResultTypeId' value of the 'courseTranscript' entity."));
        }
    }


    public class When_an_insert_conflicts_with_a_non_primary_key_unique_index_with_a_single_column : TestFixtureBase
    {
        private Exception suppliedUpdateException;
        private IDatabaseMetadataProvider suppliedMetadataProvider;
        
        private RESTError actualError;

        protected override void EstablishContext()
        {
            this.suppliedUpdateException = NHibernateExceptionBuilder.CreateException(
                "could not insert: [EdFi.Ods.Entities.AcademicHonorsTypeAggregate.AcademicHonorsType][SQL: INSERT INTO edfi.AcademicHonorsType (LastModifiedDate, CreateDate, Id, CodeValue, Description) VALUES (?, ?, ?, ?, ?); select SCOPE_IDENTITY()]",
                "Cannot insert duplicate key row in object 'edfi.AcademicHonorsType' with unique index 'SomeIndexName'.\r\nThe statement has been terminated.");

            this.suppliedMetadataProvider = this.mocks.StrictMock<IDatabaseMetadataProvider>();
            SetupResult.For(this.suppliedMetadataProvider.GetIndexDetails("SomeIndexName"))
                       .Return(new IndexDetails
                       {
                           IndexName = "SomeIndexName",
                           TableName = "SomeTableName",
                           ColumnNames = new List<string> { "Column1" }
                       });
        }

        protected override void ExecuteBehavior()
        {
            var translator = new SqlServerUniqueIndexExceptionTranslator(this.suppliedMetadataProvider);
            translator.TryTranslateMessage(this.suppliedUpdateException, out this.actualError);
        }

        [Test]
        public virtual void Should_respond_with_a_409_Conflict()
        {
            Assert.That(this.actualError.Code, Is.EqualTo((int) HttpStatusCode.Conflict));
            Assert.That(this.actualError.Type, Is.EqualTo("Conflict"));
        }

        [Test]
        public virtual void Should_translate_message_to_indicate_which_single_property_on_which_entity_was_not_unique()
        {
            Assert.That(this.actualError.Message, Is.EqualTo("The value supplied for property 'column1' of entity 'someTableName' is not unique."));
        }
    }

    public class When_an_insert_conflicts_with_a_non_primary_key_unique_index_with_multiple_columns : TestFixtureBase
    {
        private Exception suppliedUpdateException;
        private IDatabaseMetadataProvider suppliedMetadataProvider;
        private RESTError actualError;

        protected override void EstablishContext()
        {
            this.suppliedUpdateException = NHibernateExceptionBuilder.CreateException(
                "could not insert: [EdFi.Ods.Entities.AcademicHonorsTypeAggregate.AcademicHonorsType][SQL: INSERT INTO edfi.AcademicHonorsType (LastModifiedDate, CreateDate, Id, CodeValue, Description) VALUES (?, ?, ?, ?, ?); select SCOPE_IDENTITY()]",
                "Cannot insert duplicate key row in object 'edfi.AcademicHonorsType' with unique index 'SomeIndexName'.\r\nThe statement has been terminated.");

            this.suppliedMetadataProvider = this.mocks.DynamicMock<IDatabaseMetadataProvider>();
            SetupResult.For(this.suppliedMetadataProvider.GetIndexDetails("SomeIndexName"))
                       .Return(new IndexDetails
                           {
                               IndexName = "SomeIndexName",
                               TableName = "SomeTableName",
                               ColumnNames = new List<string> {"Column1", "Column2"}
                           });
        }

        protected override void ExecuteBehavior()
        {
            var translator = new SqlServerUniqueIndexExceptionTranslator(this.suppliedMetadataProvider);
            translator.TryTranslateMessage(this.suppliedUpdateException, out this.actualError);
        }

        [Test]
        public virtual void Should_respond_with_a_409_Conflict()
        {
            Assert.That(this.actualError.Code, Is.EqualTo((int) HttpStatusCode.Conflict));
            Assert.That(this.actualError.Type, Is.EqualTo("Conflict"));
        }

        [Test]
        public virtual void Should_translate_message_to_indicate_which_properties_on_which_entity_were_not_unique()
        {
            Assert.That(this.actualError.Message, Is.EqualTo("The values supplied for properties 'column1', 'column2' of entity 'someTableName' are not unique."));
        }
    }

    [TestFixture]
    public class When_an_ArgumentException_is_thrown : TestFixtureBase
    {
        private Exception suppliedException;
        private RESTError actualError;

        protected override void EstablishContext()
        {
            this.suppliedException = new ArgumentException("Some error message");
        }

        protected override void ExecuteBehavior()
        {
            var translator = new BadRequestExceptionTranslator();
            translator.TryTranslateMessage(this.suppliedException, out this.actualError);
        }

        [Test]
        public virtual void Should_respond_with_a_400_BadRequest()
        {
            Assert.That(this.actualError.Code, Is.EqualTo((int)HttpStatusCode.BadRequest));
            Assert.That(this.actualError.Type, Is.EqualTo("Bad Request"));
        }

        [Test]
        public virtual void Should_provide_a_message_that_is_a_straight_passthrough_of_the_exception_message()
        {
            Assert.That(this.actualError.Message, Is.EqualTo("Some error message"));
        }
    }

    public class DuplicateNaturalKeyExceptionTranslatorTests
    {
        [TestFixture]
        public class When_a_regular_exception_is_thrown : TestFixtureBase
        {
            private Exception exception;
            private bool result;

            protected override void EstablishContext()
            {
                this.exception = new Exception();
            }

            protected override void ExecuteBehavior()
            {
                var translator = new DuplicateNaturalKeyExceptionTranslator();
                RESTError actualError;
                this.result = translator.TryTranslateMessage(this.exception, out actualError);
            }

            [Test]
            public void Should_not_handle_this_exception()
            {
                this.result.ShouldBeFalse();
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
                this.exception = new GenericADOException("Generic exception message", null);
            }

            protected override void ExecuteBehavior()
            {
                var translator = new DuplicateNaturalKeyExceptionTranslator();
                this.wasHandled = translator.TryTranslateMessage(this.exception, out this.actualError);
            }

            [Test]
            public void Should_not_handle()
            {
                this.wasHandled.ShouldBeFalse();
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
                this.exception = NHibernateExceptionBuilder.CreateException("Some generic SQL Exception message", slightlyWrongMessage);
            }

            protected override void ExecuteBehavior()
            {
                var translator = new DuplicateNaturalKeyExceptionTranslator();
                this.wasHandled = translator.TryTranslateMessage(this.exception, out this.actualError);
            }

            [Test]
            public void Should_not_handle()
            {
                this.wasHandled.ShouldBeFalse();
            }
        }

        [TestFixture]
        public class When_an_nHibernate_ADO_exception_is_thrown_with_an_inner_SQL_exception_primary_key_violation : TestFixtureBase
        {
            private Exception exception;
            private bool result;
            RESTError actualError;
            
            protected override void EstablishContext()
            {
                const string mess = "Violation of PRIMARY KEY constraint 'PK_Session'. Cannot insert duplicate key in object 'edfi.Session'. The duplicate key value is (900007, 9, 2014). \r\nThe statement has been terminated.";
                this.exception = NHibernateExceptionBuilder.CreateException("Generic SQL Exception message...", mess);
            }

            protected override void ExecuteBehavior()
            {
                var translator = new DuplicateNaturalKeyExceptionTranslator();
                this.result = translator.TryTranslateMessage(this.exception, out this.actualError);
            }

            [Test]
            public void Should_handle_this_exception()
            {
                this.result.ShouldBeTrue();
            }

            [Test]
            public void Should_translate_the_exception_to_a_409_error()
            {
                this.actualError.Code.ShouldEqual(409);
            }

            [Test]
            public void Should_set_the_exception_type_to_conflict()
            {
                this.actualError.Type.ShouldEqual("Conflict");
            }

            [Test]
            public void Should_set_a_reasonable_message()
            {
                this.actualError.Message.ShouldEqual("A natural key conflict occurred when attempting to create a new resource with a duplicate key. This is likely caused by multiple resources with the same key in the same file. Exactly one of these resources was inserted.");
            }
        }

        [TestFixture]
        public class When_an_nHibernate_ADO_exception_is_thrown_with_an_inner_SQL_exception_primary_key_violation_and_a_backwards_PK_name : TestFixtureBase
        {
            private Exception exception;
            private bool result;
            RESTError actualError;
            
            protected override void EstablishContext()
            {
                var mess = "Violation of PRIMARY KEY constraint 'BackwardsPkName_PK'. Cannot insert duplicate key in object 'edfi.Session'. The duplicate key value is (900007, 9, 2014). \r\nThe statement has been terminated.";
                this.exception = NHibernateExceptionBuilder.CreateException("Generic exception message", mess);
            }

            protected override void ExecuteBehavior()
            {
                var translator = new DuplicateNaturalKeyExceptionTranslator();
                this.result = translator.TryTranslateMessage(this.exception, out this.actualError);
            }

            [Test]
            public void Should_handle_this_exception()
            {
                this.result.ShouldBeTrue();
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
                this.exception = new GenericADOException("Generic exception message", innerexception);
            }

            protected override void ExecuteBehavior()
            {
                var translator = new DuplicateNaturalKeyExceptionTranslator();
                this.result = translator.TryTranslateMessage(this.exception, out this.actualError);
            }

            [Test]
            public void Should_not_handle_this_exception()
            {
                this.result.ShouldBeFalse();
            }
        }
    }

    //[TestFixture]
    //public class When_an_update_conflicts_with_a_foreign_key_constraint_on_a_type_table_x
    //{
    //    [Test]
    //    public void Should_translate_the_message_to_indicate_that_a_related_resource_does_not_have_the_value_specified_in_the_current_request_but_does_not_provide_column_details()
    //    {
    //        var suppliedException = NHibernateExceptionBuilder.CreateException(
    //            "could not update: [something-a-rather][SQL: SQL not available]",
    //            "The UPDATE statement conflicted with the FOREIGN KEY constraint \"FK_Student_LimitedEnglishProficiencyType_LimitedEnglishProficiencyTypeId\". The conflict occurred in database \"EdFi_Ods\", table \"edfi.LimitedEnglishProficiencyType\", column 'LimitedEnglishProficiencyTypeId'.\r\nThe statement has been terminated.\r\n");

    //        string actualMessage;
    //        var translator = new UpdateConflictWithForeignKeyConstraintTranslator();
    //        translator.TryTranslateMessage(null, suppliedException, out actualMessage);

    //        Assert.That(actualMessage, Is.EqualTo("The value supplied for the related 'LimitedEnglishProficiencyType' resource does not exist."));
    //    }        
    //}


}
