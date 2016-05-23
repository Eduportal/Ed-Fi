namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core
{
    using System;
    using System.Linq;

    using global::EdFi.Ods.Api.Models.Resources.LearningObjective;
    using global::EdFi.Ods.Api.Models.Resources.LevelDescriptor;
    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.Pipelines.Common;
    using global::EdFi.Ods.Pipelines.Put;
    using global::EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    [TestFixture]
    public class When_loading_aggregate_without_references_to_self
    {
        [Test]
        public void Should_load_all_aggregates_in_any_order()
        {
            //Arrange
            var file = new TestIndexedFile(@"
                                <LevelDescriptor>
                                    <LevelDescriptorId>1</LevelDescriptorId>
                                    <CodeValue>01</CodeValue>
                                </LevelDescriptor>
                                <LevelDescriptor>
                                    <LevelDescriptorId>2</LevelDescriptorId>
                                    <CodeValue>02</CodeValue>
                                </LevelDescriptor>
                                <LevelDescriptor>
                                    <LevelDescriptorId>3</LevelDescriptorId>
                                    <CodeValue>03</CodeValue>
                                </LevelDescriptor>
                                            ");
            var factory = new TestMapFromXmlResourceFactory<LevelDescriptor>();
            var putResult = new PutResult { ResourceId = Guid.NewGuid(), ResourceWasCreated = true };
            var step =
                new TestOnlyStep
                    <LevelDescriptor,
                        global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(putResult);
            var sut = new TestAggregateLoader<LevelDescriptor,
                global::EdFi.Ods.Entities.NHibernate.LevelDescriptorAggregate.LevelDescriptor>(factory, step);
            //Act
            var task = sut.LoadFrom(file);
            task.Wait();
            var result = task.Result;

            //Assert
            step.PersistedResources.Count.ShouldEqual(3);
            step.PersistedResources.Where(x => x.CodeValue == "01").ShouldNotBeNull();
            step.PersistedResources.Where(x => x.CodeValue == "02").ShouldNotBeNull();
            step.PersistedResources.Where(x => x.CodeValue == "03").ShouldNotBeNull();
            result.LoadExceptions.Any().ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_loading_aggregate_with_references_to_self
    {
        [Test]
        public void Should_load_aggregates_in_proper_order()
        {
            //Arrange
            var file = new TestIndexedFile(@"
                                            <LearningObjective>
                                                <Objective>1.2</Objective>
                                                <AcademicSubjectDescriptor>1.2Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>1.2GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ParentObjective>1.1</ParentObjective>
                                                <ParentAcademicSubjectDescriptor>1.1Subject</ParentAcademicSubjectDescriptor>
                                                <ParentObjectiveGradeLevelDescriptor>1.1GradeLevel</ParentObjectiveGradeLevelDescriptor>
                                            </LearningObjective>
                                            <LearningObjective>
                                                <Objective>2.0</Objective>
                                                <AcademicSubjectDescriptor>2.0Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>2.0GradeLevel</ObjectiveGradeLevelDescriptor>
                                            </LearningObjective>
                                            <LearningObjective>
                                                <Objective>1.1</Objective>
                                                <AcademicSubjectDescriptor>1.1Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>1.1GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ParentObjective>1.0</ParentObjective>
                                                <ParentAcademicSubjectDescriptor>1.0Subject</ParentAcademicSubjectDescriptor>
                                                <ParentObjectiveGradeLevelDescriptor>1.0GradeLevel</ParentObjectiveGradeLevelDescriptor>
                                            </LearningObjective>
                                            <LearningObjective>
                                                <Objective>1.0</Objective>
                                                <AcademicSubjectDescriptor>1.0Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>1.0GradeLevel</ObjectiveGradeLevelDescriptor>
                                            </LearningObjective>
                                            ");
            var factory = new TestMapFromXmlResourceFactory<LearningObjective>();
            var putResult = new PutResult { ResourceId = Guid.NewGuid(), ResourceWasCreated = true };
            var step =
                new TestOnlyStep
                    <LearningObjective,
                        global::EdFi.Ods.Entities.NHibernate.LearningObjectiveAggregate.LearningObjective>(putResult);
            var sut = new TestAggregateLoader<LearningObjective,
                global::EdFi.Ods.Entities.NHibernate.LearningObjectiveAggregate.LearningObjective>(factory, step);
            //Act
            var result = sut.LoadFrom(file).ContinueWith(t => t.Result).Result;

            //Assert
            step.PersistedResources.Count.ShouldEqual(4);
            step.PersistedResources[2].Objective.ShouldEqual("1.1");
            step.PersistedResources[3].Objective.ShouldEqual("1.2");
            result.LoadExceptions.Any().ShouldBeFalse();
        }
    }

    [TestFixture]
    public class When_multipleAggregates_fail_to_load : TestBase
    {
        private LoadAggregateResult _result;

        [TestFixtureSetUp]
        public void Setup()
        {
            //Arrange
            var file = new TestIndexedFile(@"
                                            <LearningObjective>
                                                <Objective>1.2</Objective>
                                                <AcademicSubjectDescriptor>1.2Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>1.2GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ParentObjective>1.1</ParentObjective>
                                                <ParentAcademicSubjectDescriptor>1.1Subject</ParentAcademicSubjectDescriptor>
                                                <ParentObjectiveGradeLevelDescriptor>1.1GradeLevel</ParentObjectiveGradeLevelDescriptor>
                                                <ETag>1.2</ETag>
                                            </LearningObjective>
                                            <LearningObjective>
                                                <Objective>2.0</Objective>
                                                <AcademicSubjectDescriptor>2.0Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>2.0GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ETag>2.0</ETag>
                                            </LearningObjective>
                                            <LearningObjective>
                                                <Objective>1.1</Objective>
                                                <AcademicSubjectDescriptor>1.1Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>1.1GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ParentObjective>1.0</ParentObjective>
                                                <ParentAcademicSubjectDescriptor>1.0Subject</ParentAcademicSubjectDescriptor>
                                                <ParentObjectiveGradeLevelDescriptor>1.0GradeLevel</ParentObjectiveGradeLevelDescriptor>
                                                <ETag>1.1</ETag>
                                            </LearningObjective>
                                            <LearningObjective>
                                                <Objective>1.0</Objective>
                                                <AcademicSubjectDescriptor>1.0Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>1.0GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ETag>1.0</ETag>
                                            </LearningObjective>
                                            ");
            var factory = new TestMapFromXmlResourceFactory<LearningObjective>();

            var step = this.Stub<IStep<PutContext<LearningObjective,
                global::EdFi.Ods.Entities.NHibernate.LearningObjectiveAggregate.LearningObjective>, PutResult>>();

            step.Expect(
                s =>
                    s.Execute(Arg<PutContext<LearningObjective,
                        global::EdFi.Ods.Entities.NHibernate.LearningObjectiveAggregate.LearningObjective>>.Is
                            .Anything, Arg<PutResult>.Is.Anything))
                .IgnoreArguments()
                .Throw(new Exception("Something bad happens when you call me."));

            var sut = new TestAggregateLoader<LearningObjective,
                global::EdFi.Ods.Entities.NHibernate.LearningObjectiveAggregate.LearningObjective>(factory, step);
            //Act
            this._result = sut.LoadFrom(file).ContinueWith(t => t.Result).Result;
        }

        [Test]
        public void Should_attempt_to_load_children_regardless_of_failure()
        {
            this._result.LoadExceptions.Count().ShouldEqual(4);
        }

        [Test]
        public void Should_include_the_count_of_source_elements_attempted()
        {
            this._result.SourceElementCount.ShouldEqual(4);
        }
    }

    [TestFixture]
    public class When_dependency_fails_to_load
    {
        [Test]
        public void Should_attempt_to_load_children_regardless_of_failure()
        {
            //Arrange
            var file = new TestIndexedFile(@"
                                            <LearningObjective>
                                                <Objective>1.2</Objective>
                                                <AcademicSubjectDescriptor>1.2Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>1.2GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ParentObjective>1.1</ParentObjective>
                                                <ParentAcademicSubjectDescriptor>1.1Subject</ParentAcademicSubjectDescriptor>
                                                <ParentObjectiveGradeLevelDescriptor>1.1GradeLevel</ParentObjectiveGradeLevelDescriptor>
                                                <ETag>1.2</ETag>
                                            </LearningObjective>
                                            <LearningObjective>
                                                <Objective>2.0</Objective>
                                                <AcademicSubjectDescriptor>2.0Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>2.0GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ETag>2.0</ETag>
                                            </LearningObjective>
                                            <LearningObjective>
                                                <Objective>1.1</Objective>
                                                <AcademicSubjectDescriptor>1.1Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>1.1GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ParentObjective>1.0</ParentObjective>
                                                <ParentAcademicSubjectDescriptor>1.0Subject</ParentAcademicSubjectDescriptor>
                                                <ParentObjectiveGradeLevelDescriptor>1.0GradeLevel</ParentObjectiveGradeLevelDescriptor>
                                                <ETag>1.1</ETag>
                                            </LearningObjective>
                                            <LearningObjective>
                                                <Objective>1.0</Objective>
                                                <AcademicSubjectDescriptor>1.0Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>1.0GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ETag>1.0</ETag>
                                            </LearningObjective>
                                            ");
            var factory = new TestMapFromXmlResourceFactory<LearningObjective>();
            var putResult = new[]
            {
                new PutResult {ResourceId = Guid.NewGuid(), ResourceWasCreated = true, ETag = "1.2"},
                new PutResult {ResourceId = Guid.NewGuid(), ResourceWasCreated = true, ETag = "1.1"},
                new PutResult {ResourceId = Guid.NewGuid(), ResourceWasCreated = true, ETag = "2.0"},
                new PutResult {ResourceId = Guid.NewGuid(), ResourceWasCreated = false, ETag = "1.0", Exception = new Exception("PG13 Failure")},
            };

            var step =
                new TestOnlyStep
                    <LearningObjective,
                        global::EdFi.Ods.Entities.NHibernate.LearningObjectiveAggregate.LearningObjective>(putResult);
            var sut = new TestAggregateLoader<LearningObjective,
                global::EdFi.Ods.Entities.NHibernate.LearningObjectiveAggregate.LearningObjective>(factory, step);
            //Act
            var result = sut.LoadFrom(file).ContinueWith(t => t.Result).Result;

            //Assert
            step.PersistedResources.Count.ShouldEqual(3);
            step.PersistedResources.Any(pr => pr.Objective == "1.1").ShouldBeTrue();
            step.PersistedResources.Any(pr => pr.Objective == "1.2").ShouldBeTrue();
            step.PersistedResources.Any(pr => pr.Objective == "2.0").ShouldBeTrue();
            result.LoadExceptions.Any().ShouldBeTrue();
        }
    }

    [TestFixture]
    public class When_dependencies_are_not_part_of_load
    {
        [Test]
        public void Should_attempt_to_load_children()
        {
            //Arrange
            var file = new TestIndexedFile(@"
                                            <LearningObjective>
                                                <Objective>1.2</Objective>
                                                <AcademicSubjectDescriptor>1.2Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>1.2GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ParentObjective>1.1</ParentObjective>
                                                <ParentAcademicSubjectDescriptor>1.1Subject</ParentAcademicSubjectDescriptor>
                                                <ParentObjectiveGradeLevelDescriptor>1.1GradeLevel</ParentObjectiveGradeLevelDescriptor>
                                            </LearningObjective>
                                            <LearningObjective>
                                                <Objective>1.1</Objective>
                                                <AcademicSubjectDescriptor>1.1Subject</AcademicSubjectDescriptor>
                                                <ObjectiveGradeLevelDescriptor>1.1GradeLevel</ObjectiveGradeLevelDescriptor>
                                                <ParentObjective>1.0</ParentObjective>
                                                <ParentAcademicSubjectDescriptor>1.0Subject</ParentAcademicSubjectDescriptor>
                                                <ParentObjectiveGradeLevelDescriptor>1.0GradeLevel</ParentObjectiveGradeLevelDescriptor>
                                            </LearningObjective>
                                            ");
            var factory = new TestMapFromXmlResourceFactory<LearningObjective>();
            var putResult = new PutResult { ResourceId = Guid.NewGuid(), ResourceWasCreated = true };
            var step =
                new TestOnlyStep
                    <LearningObjective,
                        global::EdFi.Ods.Entities.NHibernate.LearningObjectiveAggregate.LearningObjective>(putResult);
            var sut = new TestAggregateLoader<LearningObjective,
                global::EdFi.Ods.Entities.NHibernate.LearningObjectiveAggregate.LearningObjective>(factory, step);
            //Act
            var result = sut.LoadFrom(file).ContinueWith(t => t.Result).Result;

            //Assert
            step.PersistedResources.Count.ShouldEqual(2);
            result.LoadExceptions.Any().ShouldBeFalse();
        }
    }
}
