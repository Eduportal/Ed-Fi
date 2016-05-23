// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Linq;
using EdFi.Common.Security.Claims;
using EdFi.Ods.Common.Repositories;
using EdFi.Ods.Entities.NHibernate.StudentAggregate;
using EdFi.Ods.Security.Authorization.Repositories;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests._Extensions;
using Rhino.Mocks;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.Security.Authorization.Repositories
{
    public class Feature_Authorizing_a_GetById_operation
    {
        public class When_authorizing_the_operation_on_a_non_existing_entity :
            ScenarioFor<GetEntityByIdAuthorizationDecorator<Student>>
        {
            private Student _actualStudent;

            /// <summary>
            /// Executes the code to be exercised for the scenario.
            /// </summary>
            protected override void Act()
            {
                _actualStudent = TestSubject.GetById(Guid.NewGuid());
            }

            [Assert]
            public void Should_not_throw_an_exception()
            {
                ActualException.ShouldBeNull();
            }

            [Assert]
            public void Should_return_the_Student_as_null()
            {
                _actualStudent.ShouldBeNull();
            }
        }

        public class When_authorizaing_the_operation_on_an_existing_entity :
            ScenarioFor<GetEntityByIdAuthorizationDecorator<Student>>
        {
            private Student _actualStudent;

            /// <summary>
            /// Prepares the state of the scenario (creating stubs, test data, etc.).
            /// </summary>
            protected override void Arrange()
            {
                Supplied(Guid.NewGuid());

                Given<IGetEntityById<Student>>()
                    .Expect(x => x.GetById(Supplied<Guid>()))
                    .Return(Supplied(new Student()));

                Given<IAuthorizationContextProvider>()
                    .Expect(x => x.GetAction())
                    .Return("Action");

                Given<IAuthorizationContextProvider>()
                    .Expect(x => x.GetResource())
                    .Return("Resource");
            }

            /// <summary>
            /// Executes the code to be exercised for the scenario.
            /// </summary>
            protected override void Act()
            {
                _actualStudent = TestSubject.GetById(Supplied<Guid>());
            }

            [Assert]
            public void Should_invoke_authorization_on_the_item()
            {
                Given<IEdFiAuthorizationProvider>()
                    .AssertWasCalled(x => x.AuthorizeSingleItem(
                        Arg<EdFiAuthorizationContext>.Matches(ctx => CompareContexts(ctx))));
            }

            [Assert]
            public void Should_return_the_supplied_student()
            {
                _actualStudent.ShouldBeSameAs(Supplied<Student>());
            }

            private bool CompareContexts(EdFiAuthorizationContext context)
            {
                context.Resource.Single().Value.ShouldEqual("Resource");
                context.Action.Single().Value.ShouldEqual("Action");
                context.Data.ShouldBeSameAs(Supplied<Student>());
                context.Type.ShouldBeSameAs(typeof(Student));
                
                return true;
            }
        }
    }
}