// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using EdFi.Ods.Security.AuthorizationStrategies;
using EdFi.Ods.Tests._Bases;
using EdFi.Ods.Tests._Extensions;
using NUnit.Framework;
using Should;

namespace EdFi.Ods.Tests.EdFi.Ods.Security.AuthorizationStrategies
{
    public class When_generating_aliases_for_filters : TestFixtureBase
    {
        // Supplied values

        // Actual values
        private HashSet<string> _actualAliases;

        // Dependencies
        protected override void Act()
        {
            // Execute code under test
            var generator = new AliasGenerator();
            generator.Reset();

            _actualAliases = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            int i = 0;

            while (i++ < 100000)
            {
                string nextAlias = generator.GetNextAlias();

                if (!_actualAliases.Add(nextAlias))
                    Assert.Fail("Duplicate alias generated: '{0}'", nextAlias);
            }

            Assert.Fail("Possible infinite loop detected in alias generation.  Stopped at 100,000 aliases.");
        }

        [Assert]
        public void Should_generate_17576_unique_aliases_before_throwing_an_InvalidOperationException()
        {
            _actualAliases.Count.ShouldEqual(26*26*26);
            ActualException.ShouldBeExceptionType<InvalidOperationException>();
        }
    }
}