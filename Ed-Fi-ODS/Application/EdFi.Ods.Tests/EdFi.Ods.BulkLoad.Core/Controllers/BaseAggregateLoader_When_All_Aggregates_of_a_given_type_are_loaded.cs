namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System;
    using System.Linq;

    using global::EdFi.Ods.Api.Models.Resources.LevelDescriptor;
    using global::EdFi.Ods.Pipelines.Put;

    using NUnit.Framework;

    using Should;

    [TestFixture]
    public class BaseAggregateLoader_When_All_Aggregates_of_a_given_type_are_loaded
    {
        [Test]
        public void Given_All_Aggregates_Are_Created_Should_Show_Expected_Number_Loaded()
        {
            //Arrange
            const int shouldLoad = 6;
            var file = new TestIndexedFile(shouldLoad);
            var factory = new TestLevelDescriptorResourceFactory();
            var putResult = new PutResult {ResourceId = Guid.NewGuid(), ResourceWasCreated = true, ResourceWasPersisted = true};
            var step = new TestOnlyStep<LevelDescriptor,global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(putResult);
            var sut =
                new TestAggregateLoader
                    <LevelDescriptor, global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(factory, step);
            //Act
            var result = sut.LoadFrom(file).ContinueWith(t => t.Result).Result;
            result.LoadExceptions.Any().ShouldBeFalse();
            result.RecordsLoaded.ShouldEqual(shouldLoad);
        }

        [Test]
        public void Given_All_Aggregates_Are_Persisted_But_Not_Created_And_Not_Updated_Should_Show_Expected_Number_Loaded()
        {
            //Arrange
            const int shouldLoad = 6;
            var file = new TestIndexedFile(shouldLoad);
            var factory = new TestLevelDescriptorResourceFactory();
            var putResult = new PutResult {ResourceId = Guid.NewGuid(), ResourceWasPersisted = true};
            var step = new TestOnlyStep<LevelDescriptor,global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(putResult);
            var sut =
                new TestAggregateLoader
                    <LevelDescriptor, global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(factory, step);
            //Act
            var result = sut.LoadFrom(file).ContinueWith(t => t.Result).Result;
            result.LoadExceptions.Any().ShouldBeFalse();
            result.RecordsLoaded.ShouldEqual(shouldLoad);
        }

        [Test]
        public void Given_No_Aggregates_Exist_In_File_Should_Return_Zero_Count_And_All_Loaded()
        {
            //Arrange
            var file = new TestIndexedFile();
            var factory = new TestLevelDescriptorResourceFactory();
            var putResult = new PutResult { ResourceId = Guid.NewGuid(), ResourceWasCreated = true};
            var step = new TestOnlyStep<LevelDescriptor, global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(putResult);
            var sut =
                new TestAggregateLoader
                    <LevelDescriptor, global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(factory, step);
            //Act
            var result = sut.LoadFrom(file).ContinueWith(t => t.Result).Result;
            
            //Assert
            result.LoadExceptions.Any().ShouldBeFalse();
            result.RecordsLoaded.ShouldEqual(0);
            result.SourceElementCount.ShouldEqual(0);
        }

        [Test]
        public void Given_At_Least_One_Aggregate_Fails_To_Load_Should_Indicate_Not_All_Aggregates_Loaded()
        {
             //Arrange
            var file = new TestIndexedFile(2);
            var factory = new TestLevelDescriptorResourceFactory();
            var putResult = new PutResult {Exception = new Exception("Geez guys - why do I have to fail")};
            var step = new TestOnlyStep<LevelDescriptor, global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(putResult);
            var sut =
                new TestAggregateLoader
                    <LevelDescriptor, global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(factory, step);
            //Act
            var result = sut.LoadFrom(file).ContinueWith(t => t.Result).Result;
            result.LoadExceptions.Any().ShouldBeTrue();
        }

        [Test]
        public void Given_Aggregate_Fails_While_Processing_Pipeline_Should_Indicate_Exception()
        {
            //Arrange
            var file = new TestIndexedFile(1);
            var factory = new TestLevelDescriptorResourceFactory();
            var putResult = new PutResult { Exception = new Exception("Procesing pipeline exception") };
            var step = new TestOnlyStep<LevelDescriptor, global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(putResult);
            var sut =
                new TestAggregateLoader
                    <LevelDescriptor, global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(factory, step);
            //Act
            var result = sut.LoadFrom(file).ContinueWith(t => t.Result).Result;
            result.LoadExceptions.Length.ShouldEqual(1);
            result.LoadExceptions.First().Exception.Message.ShouldContain("Procesing pipeline exception");
        }

        [Test]
        public void Given_Aggregate_Fails_While_Building_Resource_Should_Indicate_Exception()
        {
            //Arrange
            var file = new TestIndexedFile(1);
            var factory = new TestLevelDescriptorResourceFactory(new Exception("Building resource exception"));
            var putResult = new PutResult { ResourceId = Guid.NewGuid(), ResourceWasCreated = true };
            var step = new TestOnlyStep<LevelDescriptor, global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(putResult);
            var sut =
                new TestAggregateLoader
                    <LevelDescriptor, global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(factory, step);
            //Act
            var result = sut.LoadFrom(file).ContinueWith(t => t.Result).Result;
            result.LoadExceptions.Length.ShouldEqual(1);
            result.LoadExceptions.First().Exception.Message.ShouldContain("Building resource exception");
        }
    }
}