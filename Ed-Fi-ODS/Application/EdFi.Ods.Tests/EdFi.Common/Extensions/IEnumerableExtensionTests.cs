using System;
using System.Collections.Generic;
using EdFi.Common.Extensions;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests._Extensions;
using Should;

namespace EdFi.Ods.Tests.EdFi.Common.Extensions
{
    public class Feature_Inserting_items_into_enumerable_collections_based_on_existing_items
    {
        public class When_inserting_an_item_before_the_second_item_of_a_two_item_list : TestFixtureBase
        {
            // Supplied values
            object object1 = new object();
            object object2 = new object();
            object insertObject = new object();

            // Actual values
            private IEnumerable<object> _actualResult;

            protected override void Act()
            {
                // Execute code under test
                var list = new List<object> {object1, object2};

                _actualResult = list.InsertBefore(object2, insertObject);
            }

            [Assert]
            public void Should_insert_the_item_between_the_existing_items()
            {
                _actualResult.ShouldEqual(new [] { object1, insertObject, object2});
            }
        }
        
        public class When_inserting_an_item_after_the_first_item_of_a_two_item_list : TestFixtureBase
        {
            // Supplied values
            object object1 = new object();
            object object2 = new object();
            object insertObject = new object();

            // Actual values
            private IEnumerable<object> _actualResult;

            protected override void Act()
            {
                // Execute code under test
                var list = new List<object> {object1, object2};

                _actualResult = list.InsertAfter(object1, insertObject);
            }

            [Assert]
            public void Should_insert_the_item_between_the_existing_items()
            {
                _actualResult.ShouldEqual(new [] { object1, insertObject, object2});
            }
        }

        public class When_inserting_an_item_after_the_second_item_of_a_two_item_list : TestFixtureBase
        {
            // Supplied values
            object object1 = new object();
            object object2 = new object();
            object insertObject = new object();

            // Actual values
            private IEnumerable<object> _actualResult;

            protected override void Act()
            {
                // Execute code under test
                var list = new List<object> { object1, object2 };

                _actualResult = list.InsertAfter(object2, insertObject);
            }

            [Assert]
            public void Should_insert_the_item_as_the_last_item_in_the_list()
            {
                _actualResult.ShouldEqual(new[] { object1, object2, insertObject });
            }
        }

        public class When_inserting_an_item_before_the_first_item_of_a_two_item_list : TestFixtureBase
        {
            // Supplied values
            object object1 = new object();
            object object2 = new object();
            object insertObject = new object();

            // Actual values
            private IEnumerable<object> _actualResult;

            protected override void Act()
            {
                // Execute code under test
                var list = new List<object> { object1, object2 };

                _actualResult = list.InsertBefore(object1, insertObject);
            }

            [Assert]
            public void Should_insert_the_item_as_the_first_item_in_the_list()
            {
                _actualResult.ShouldEqual(new[] { insertObject, object1, object2 });
            }
        }

        public class When_inserting_an_item_before_the_only_item_in_a_single_item_list : TestFixtureBase
        {
            // Supplied values
            object object1 = new object();
            object insertObject = new object();

            // Actual values
            private IEnumerable<object> _actualResult;

            protected override void Act()
            {
                // Execute code under test
                var list = new List<object> { object1 };

                _actualResult = list.InsertBefore(object1, insertObject);
            }

            [Assert]
            public void Should_insert_the_item_as_the_first_item_in_the_list()
            {
                _actualResult.ShouldEqual(new[] { insertObject, object1 });
            }
        }

        public class When_inserting_an_item_after_the_only_item_in_a_single_item_list : TestFixtureBase
        {
            // Supplied values
            object object1 = new object();
            object insertObject = new object();

            // Actual values
            private IEnumerable<object> _actualResult;

            protected override void Act()
            {
                // Execute code under test
                var list = new List<object> { object1 };

                _actualResult = list.InsertAfter(object1, insertObject);
            }

            [Assert]
            public void Should_insert_the_item_as_the_last_item_in_the_list()
            {
                _actualResult.ShouldEqual(new[] { object1, insertObject});
            }
        }

        public class When_inserting_an_item_before_an_item_that_does_not_exist_in_list : TestFixtureBase
        {
            // Supplied values
            object object1 = new object();
            object object2 = new object();
            object nonMemberObject = new object();
            object insertObject = new object();

            // Actual values
            private IEnumerable<object> _actualResult;

            protected override void Act()
            {
                // Execute code under test
                var list = new List<object> { object1, object2 };

                _actualResult = list.InsertBefore(nonMemberObject, insertObject);
            }

            [Assert]
            public void Should_throw_an_exception()
            {
                ActualException.ShouldBeExceptionType<ArgumentException>();
                ActualException.MessageShouldContain("Item could not be inserted because the insertion point item was not found in the collection.");
            }
        }
    }
}
