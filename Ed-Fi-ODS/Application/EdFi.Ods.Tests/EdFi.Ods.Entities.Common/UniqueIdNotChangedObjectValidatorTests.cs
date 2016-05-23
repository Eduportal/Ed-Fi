namespace EdFi.Ods.Tests.EdFi.Ods.Entities.Common
{
    using System;
    using System.Linq;

    using global::EdFi.Ods.Entities.Common.Caching;
    using global::EdFi.Ods.Entities.Common.Validation;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    [TestFixture]
    public class When_validating_student_resource_with_changed_unique_id : TestBase
    {
        [Test]
        public void Should_return_validation_error()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Api.Models.Resources.Student.Student {Id = Guid.NewGuid(), StudentUniqueId = uniqueId + "XYZ"};
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.Empty)).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeFalse();
            result.Count.ShouldEqual(1);
            result.First().ErrorMessage.ShouldEqual("A person's UniqueId cannot be modified.");
        }    
    }

    [TestFixture]
    public class When_validating_staff_resource_with_changed_unique_id : TestBase
    {
        [Test]
        public void Should_return_validation_error()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Api.Models.Resources.Staff.Staff { Id = Guid.NewGuid(), StaffUniqueId = uniqueId + "XYZ" };
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.Empty)).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeFalse();
            result.Count.ShouldEqual(1);
            result.First().ErrorMessage.ShouldEqual("A person's UniqueId cannot be modified.");
        }
    }

    [TestFixture]
    public class When_validating_parent_resource_with_changed_unique_id : TestBase
    {
        [Test]
        public void Should_return_validation_error()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Api.Models.Resources.Parent.Parent { Id = Guid.NewGuid(), ParentUniqueId = uniqueId + "XYZ" };
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.Empty)).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeFalse();
            result.Count.ShouldEqual(1);
            result.First().ErrorMessage.ShouldEqual("A person's UniqueId cannot be modified.");
        }
    }

    [TestFixture]
    public class When_validating_student_entity_with_changed_unique_id : TestBase
    {
        [Test]
        public void Should_return_validation_error()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Entities.NHibernate.StudentAggregate.Student { Id = Guid.NewGuid(), StudentUniqueId = uniqueId + "XYZ" };
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.Empty)).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeFalse();
            result.Count.ShouldEqual(1);
            result.First().ErrorMessage.ShouldEqual("A person's UniqueId cannot be modified.");
        }
    }

    [TestFixture]
    public class When_validating_staff_entity_with_changed_unique_id : TestBase
    {
        [Test]
        public void Should_return_validation_error()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Entities.NHibernate.StaffAggregate.Staff { Id = Guid.NewGuid(), StaffUniqueId = uniqueId + "XYZ" };
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.Empty)).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeFalse();
            result.Count.ShouldEqual(1);
            result.First().ErrorMessage.ShouldEqual("A person's UniqueId cannot be modified.");
        }
    }

    [TestFixture]
    public class When_validating_parent_entity_with_changed_unique_id : TestBase
    {
        [Test]
        public void Should_return_validation_error()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Entities.NHibernate.ParentAggregate.Parent { Id = Guid.NewGuid(), ParentUniqueId = uniqueId + "XYZ" };
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.NewGuid())).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeFalse();
            result.Count.ShouldEqual(1);
            result.First().ErrorMessage.ShouldEqual("A person's UniqueId cannot be modified.");
        }
    }




    [TestFixture]
    public class When_validating_student_resource_with_unchanged_unique_id : TestBase
    {
        [Test]
        public void Should_pass_validation()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Api.Models.Resources.Student.Student { Id = Guid.NewGuid(), StudentUniqueId = uniqueId };
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.Empty)).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_validating_staff_resource_with_unchanged_unique_id : TestBase
    {
        [Test]
        public void Should_pass_validation()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Api.Models.Resources.Staff.Staff { Id = Guid.NewGuid(), StaffUniqueId = uniqueId };
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.Empty)).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_validating_parent_resource_with_unchanged_unique_id : TestBase
    {
        [Test]
        public void Should_pass_validation()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Api.Models.Resources.Parent.Parent { Id = Guid.NewGuid(), ParentUniqueId = uniqueId };
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.NewGuid())).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_validating_student_entity_with_unchanged_unique_id : TestBase
    {
        [Test]
        public void Should_pass_validation()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Entities.NHibernate.StudentAggregate.Student { Id = Guid.NewGuid(), StudentUniqueId = uniqueId };
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.Empty)).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_validating_staff_entity_with_unchanged_unique_id : TestBase
    {
        [Test]
        public void Should_pass_validation()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Entities.NHibernate.StaffAggregate.Staff { Id = Guid.NewGuid(), StaffUniqueId = uniqueId };
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.Empty)).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_validating_parent_entity_with_unchanged_unique_id : TestBase
    {
        [Test]
        public void Should_pass_validation()
        {
            const string uniqueId = "ABC123";
            var student = new global::EdFi.Ods.Entities.NHibernate.ParentAggregate.Parent { Id = Guid.NewGuid(), ParentUniqueId = uniqueId };
            var mockCache = this.Stub<IPersonUniqueIdToIdCache>();
            var validator = new UniqueIdNotChangedObjectValidator(mockCache);
            mockCache.Stub(x => x.GetUniqueId(string.Empty, Guid.Empty)).IgnoreArguments().Return(uniqueId);
            var result = validator.ValidateObject(student);
            result.IsValid().ShouldBeTrue();
        }
    }

}
