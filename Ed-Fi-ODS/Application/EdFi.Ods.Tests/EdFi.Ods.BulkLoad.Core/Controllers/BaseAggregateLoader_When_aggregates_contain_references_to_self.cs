namespace EdFi.Ods.Tests.EdFi.Ods.BulkLoad.Core.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using global::EdFi.Ods.Api.Models.Resources.LearningObjective;
    using global::EdFi.Ods.Api.Models.Resources.LearningStandard;
    using global::EdFi.Ods.Api.Models.Resources.LocalEducationAgency;
    using global::EdFi.Ods.Entities.Common;

    using NUnit.Framework;

    using Should;

    /// <summary>
    /// The tests below were created to ensure naming conventions for self referencing resources are followed
    /// If the tests below fail, there will likely also be an issue with AggregateLoaderBase as it is using the
    /// same 'Parent' + propertyname naming convention
    /// </summary>
    [TestFixture]
    public class BaseAggregateLoader_When_aggregates_contain_references_to_self
    {
        private void AssertSelfReferencingResourceFollowsConvention(Type type)
        {
            var interfaceType = type.GetInterface("I" + type.Name);
            var natKeys = interfaceType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(NaturalKeyMemberAttribute))).ToList();
            var parentKeyProperties = new List<PropertyInfo>();
            foreach (var naturalKeyProperty in natKeys)
            {
                parentKeyProperties.AddRange(interfaceType.GetProperties()
                        .Where(
                            prop => prop.Name == "Parent" + naturalKeyProperty.Name)
                        .ToList());
            }
            parentKeyProperties.Count.ShouldEqual(natKeys.Count);            
        }

        [Test(Description = "These tests ensure naming conventions for self referencing resources are not inadvertently changed")]
        public void should_have_parent_properties_following_naming_convention()
        {
            this.AssertSelfReferencingResourceFollowsConvention(typeof(LearningObjective));
            this.AssertSelfReferencingResourceFollowsConvention(typeof(LearningStandard));
            this.AssertSelfReferencingResourceFollowsConvention(typeof(LocalEducationAgency));
        }
    }
}
