namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System;
    using System.Linq;

    using global::EdFi.Ods.Api.Models.Resources.AcademicSubjectDescriptor;
    using global::EdFi.Ods.Api.Models.Resources.AccountCodeDescriptor;
    using global::EdFi.Ods.Api.Models.Resources.AchievementCategoryDescriptor;
    using global::EdFi.Ods.BulkLoad.Common;
    using global::EdFi.Ods.BulkLoad.Core;
    using global::EdFi.Ods.BulkLoad.Core.Controllers.Base;
    using global::EdFi.Ods.Entities.NHibernate.AdministrativeFundingControlDescriptorAggregate;
    using global::EdFi.Ods.Entities.NHibernate.BehaviorDescriptorAggregate;
    using global::EdFi.Ods.Tests._Bases;

    using NUnit.Framework;

    using Rhino.Mocks;

    using Should;

    public class InterchangeControllerBaseTests
    {
        public class TestController : InterchangeController
        {
            public TestController(OrderedCollectionOfSets<ILoadAggregates> loaders, Func<string, IIndexedXmlFileReader> reader)
                : base(loaders, reader)
            {
            }

            public static OrderedCollectionOfSets<ILoadAggregates> GetRequiredLoaders()
            {
                var collection = new OrderedCollectionOfSets<ILoadAggregates>();
                collection.AddSet()
                    .AddMember(new StubAggregateLoader(typeof (BehaviorDescriptor)))
                    .AddMember(new StubAggregateLoader(typeof (AcademicSubjectDescriptor)));
                collection.AddSet()
                    .AddMember(new StubAggregateLoader(typeof (AccountCodeDescriptor)))
                    .AddMember(new StubAggregateLoader(typeof (AdministrativeFundingControlDescriptor)));
                return collection;
            }

            public static OrderedCollectionOfSets<ILoadAggregates> GetAllLoaders()
            {
                return GetRequiredLoaders()
                    .AddSet().AddMember(new StubAggregateLoader(typeof (AchievementCategoryDescriptor)));
            }
        }

        public class ControllerLoadsEverything : InterchangeController
        {
            public ControllerLoadsEverything(OrderedCollectionOfSets<ILoadAggregates> loaders, Func<string, IIndexedXmlFileReader> reader) : base(loaders, reader)
            {
            }
        }

        [TestFixture]
        public class When_given_a_valid_file_path_valid_aggregate_loaders_divided_into_two_named_sets_and_everything_else
        {
            private LoadResult _result;
            private OrderedCollectionOfSets<ILoadAggregates> _aggregateLoaders;
            private int _expectedSourceElementCount;

            [TestFixtureSetUp]
            public void Setup()
            {
                this._aggregateLoaders = TestController.GetAllLoaders();
                int counter = 1;
                foreach (var loaderSet in this._aggregateLoaders)
                {
                    foreach (var loader in loaderSet)
                    {
                        var stubLoader = (StubAggregateLoader) loader;
                        this._expectedSourceElementCount += counter;
                        stubLoader.SetSourceElementCount(counter++);
                    }
                }

                Func<string, IIndexedXmlFileReader> reader = 
                    (filePath) =>
                        {
                            return new StubReader(filePath);
                        };
                var controller = new TestController(this._aggregateLoaders, reader);
                this._result = controller.LoadAsync(@"path\doesnt\matter").Result;
            }

            [Test]
            public void Should_run_everything_from_set_1_then_everything_from_set_2_then_the_remaining_loader()
            {
                var firstSet = this._aggregateLoaders.RemoveSet(0).Cast<StubAggregateLoader>().ToArray();
                firstSet.Length.ShouldEqual(2); //Sanity Check

                var secondSet = this._aggregateLoaders.RemoveSet(0).Cast<StubAggregateLoader>().ToArray();
                secondSet.Length.ShouldEqual(2); //Sanity Check

                var everythingElse = this._aggregateLoaders.RemoveSet(0).Cast<StubAggregateLoader>().ToArray();
                everythingElse.Length.ShouldEqual(1); //Sanity Check

                firstSet[0].TimeIWasAskedToProcess().ShouldBeLessThan(secondSet[0].TimeIWasAskedToProcess());
                firstSet[0].TimeIWasAskedToProcess().ShouldBeLessThan(secondSet[1].TimeIWasAskedToProcess());
                firstSet[0].TimeIWasAskedToProcess().ShouldBeLessThan(everythingElse[0].TimeIWasAskedToProcess());

                firstSet[1].TimeIWasAskedToProcess().ShouldBeLessThan(secondSet[0].TimeIWasAskedToProcess());
                firstSet[1].TimeIWasAskedToProcess().ShouldBeLessThan(secondSet[1].TimeIWasAskedToProcess());
                firstSet[1].TimeIWasAskedToProcess().ShouldBeLessThan(everythingElse[0].TimeIWasAskedToProcess());

                secondSet[0].TimeIWasAskedToProcess().ShouldBeLessThan(everythingElse[0].TimeIWasAskedToProcess());
                secondSet[1].TimeIWasAskedToProcess().ShouldBeLessThan(everythingElse[0].TimeIWasAskedToProcess());
            }

            [Test]
            public void Should_aggregate_the_source_element_count()
            {
                this._result.SourceElementCount.ShouldEqual(this._expectedSourceElementCount);
            }
        }

        [TestFixture]
        public class When_a_loader_barfs : TestBase
        {
            private LoadResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var barfingLoader = this.Stub<ILoadAggregates>();
                barfingLoader.Stub(x => x.LoadFrom(null)).IgnoreArguments().Throw(new Exception("I am a forced exception"));
                barfingLoader.Stub(x => x.GetAggregateType()).Return(typeof (BehaviorDescriptor));

                var aggregateLoaders = new OrderedCollectionOfSets<ILoadAggregates>().AddSet().AddMember(barfingLoader);

                var controller = new ControllerLoadsEverything(aggregateLoaders, (x) => new StubReader(x));
                this._result = controller.LoadAsync(@"path\doesnt\matter").Result;
            }

            [Test]
            public void Should_report_an_error_result()
            {
                this._result.LoadExceptions.Count.ShouldEqual(1);
                this._result.LoadExceptions[0].Exception.Message.ShouldEqual("I am a forced exception");
            }

            [Test]
            public void Should_report_an_exception()
            {
                this._result.LoadExceptions.Any().ShouldBeTrue();
            }
        }

        [TestFixture]
        public class When_a_loader_doesnt_load_anything : TestBase
        {
            private LoadResult _result;

            [TestFixtureSetUp]
            public void Setup()
            {
                var aggregateLoaders = new OrderedCollectionOfSets<ILoadAggregates>();

                var controller = new ControllerLoadsEverything(aggregateLoaders, (x) => new StubReader(x));
                this._result = controller.LoadAsync(@"path\doesnt\matter").Result;
            }

            [Test]
            public void Should_not_report_exception()
            {
                this._result.LoadExceptions.Any().ShouldBeFalse();
            }
        }
    }
}