using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using EdFi.Ods.CodeGen;
using EdFi.Ods.CodeGen.DatabaseSchema;
using EdFi.Ods.CodeGen.Models;
using EdFi.Ods.CodeGen.Models.ProfileMetadata;
using EdFi.Ods.Common.Utils.Profiles;
using EdFi.Ods.Tests._Bases;
using Rhino.Mocks;
using Should;
using Property = EdFi.Ods.CodeGen.Models.ProfileMetadata.PropertyDefinition;

namespace EdFi.Ods.Tests.EdFi.Ods.CodeGen.DatabaseSchema
{
    public class When_profile_definition_includes_a_resource_level_property_and_collection : TestFixtureBase
    {
        private bool _actualTopLevelPropertyShouldBeGenerated;
        private bool _actualTopLevelObjectShouldBeGenerated;
        private bool _actualTopLevelCollectionShouldBeGenerated;
        private bool _actualUnspecifiedPropertyShouldBeGenerated;
        private bool _actualUnspecifiedObjectShouldBeGenerated;
        private bool _actualUnspecifiedCollectionShouldBeGenerated;

        private ICodeGenHelper _codeGenHelper;

        protected override void Arrange()
        {
            var resourceModel = Stub<T4ResourceModel>();
            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Anything)).Return(new CodeGenClassModel());

            _codeGenHelper = Stub<ICodeGenHelper>();
            _codeGenHelper.Stub(x => x.GetTable(Arg<string>.Is.Anything)).Return(new Table { FKTables = new List<FKTable>() });
            _codeGenHelper.Stub(x => x.DomainModel).Return(resourceModel);
        }

        protected override void Act()
        {
            var profileContext = ProfileContexts.GetResourceLevelReadIncludeOnly();

            var helper = new ProfilesCodeGenHelper(profileContext, _codeGenHelper);

            const string resourceName = "Resource";

            _actualTopLevelPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "TopProperty", null);
            _actualTopLevelObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "TopObject", null);
            _actualTopLevelCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "TopCollectionOnes", null);

            _actualUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedProperty", null);
            _actualUnspecifiedObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedObject", null);
            _actualUnspecifiedCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedCollectionOnes", null); 
        }

        [Assert]
        public void Should_generate_explicitly_included_property()
        {
            _actualTopLevelPropertyShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_explicitly_included_object()
        {
            _actualTopLevelObjectShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_explicitly_included_collection()
        {
            _actualTopLevelCollectionShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_NOT_generate_any_of_the_unspecified_members()
        {
            _actualUnspecifiedPropertyShouldBeGenerated.ShouldBeFalse();
            _actualUnspecifiedObjectShouldBeGenerated.ShouldBeFalse();
            _actualUnspecifiedCollectionShouldBeGenerated.ShouldBeFalse();
        }
    }

    public class When_profile_definition_includes_all_resource_level_members : TestFixtureBase
    {
        private bool _actualTopLevelPropertyShouldBeGenerated;
        private bool _actualTopLevelObjectShouldBeGenerated;
        private bool _actualTopLevelCollectionShouldBeGenerated;
        private bool _actualUnspecifiedPropertyShouldBeGenerated;
        private bool _actualUnspecifiedObjectShouldBeGenerated;
        private bool _actualUnspecifiedCollectionShouldBeGenerated;

        private ICodeGenHelper _codeGenHelper;

        protected override void Arrange()
        {
            var resourceModel = Stub<T4ResourceModel>();
            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Anything)).Return(new CodeGenClassModel());

            _codeGenHelper = Stub<ICodeGenHelper>();
            _codeGenHelper.Stub(x => x.GetTable(Arg<string>.Is.Anything)).Return(new Table { FKTables = new List<FKTable>() });
            _codeGenHelper.Stub(x => x.DomainModel).Return(resourceModel);
        }

        protected override void Act()
        {
            var profileContext = ProfileContexts.GetResourceLevelReadIncludeAll();

            var helper = new ProfilesCodeGenHelper(profileContext, _codeGenHelper);

            const string resourceName = "Resource";

            _actualTopLevelPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "PropertyOne", null);
            _actualTopLevelObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "TopObject", null);
            _actualTopLevelCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "CollectionOnes", null);

            _actualUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedProperty", null);
            _actualUnspecifiedObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedObject", null);
            _actualUnspecifiedCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedCollectionOnes", null);
        }

        [Assert]
        public void Should_generate_explicitly_included_property()
        {
            _actualTopLevelPropertyShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_explicitly_included_object()
        {
            _actualTopLevelObjectShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_explicitly_included_collection()
        {
            _actualTopLevelCollectionShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_also_generate_unspecified_members()
        {
            _actualUnspecifiedPropertyShouldBeGenerated.ShouldBeTrue();
            _actualUnspecifiedObjectShouldBeGenerated.ShouldBeTrue();
            _actualUnspecifiedCollectionShouldBeGenerated.ShouldBeTrue();
        }
    }

    public class When_profile_defintion_includes_a_resource_level_collection_and_an_embedded_collection_with_members : TestFixtureBase
    {
        private ICodeGenHelper _codeGenHelper;
        private bool _embeddedCollectionPropertyShouldBeGenerated;
        private bool _embeddedCollectionObjectShouldBeGenerated;
        private bool _embeddedObjectPropertyShouldBeGenerated;
        private bool _embeddedCollectionCollectionShouldBeGenerated;
        private bool _embeddedCollectionUnspecifiedPropertyShouldBeGenerated;

        protected override void Arrange()
        {
            var resourceModel = Stub<T4ResourceModel>();
            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("Resource"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ChildTables = new List<FKTable>
                {
                    new FKTable
                    {
                     IsPrimaryTable = true,
                     ThisTable = "Resource",
                     OtherTable = "TopCollectionOne"
                    }
                },
                OneToOneChildFKs = new List<FKTable>
                {
                    new FKTable
                    {
                        IsPrimaryTable = true,
                        ThisTable = "Resource",
                        OtherTable = "TopObject"
                    }
                }
            });

            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("TopCollectionOne"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ParentFkTable = new FKTable
                {
                    IsPrimaryTable = false,
                    ThisTable = "TopCollectionOne",
                    OtherTable = "Resource"
                },
                ChildTables = new List<FKTable>
                {
                    new FKTable
                    {
                        IsPrimaryTable = true,
                        ThisTable = "TopCollectionOne",
                        OtherTable = "SubCollectionTwo"
                    }
                }
            });

            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("TopObject"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ParentFkTable = new FKTable
                {
                    IsPrimaryTable = false,
                    ThisTable = "TopObject",
                    OtherTable = "Resource"
                }
            });

            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("TopCollectionTwo"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ParentFkTable = new FKTable
                {
                    IsPrimaryTable = false,
                    ThisTable = "TopCollectionTwo",
                    OtherTable = "TopCollectionOne"
                }
            });

            _codeGenHelper = Stub<ICodeGenHelper>();
            _codeGenHelper.Stub(x => x.GetTable(Arg<string>.Is.Anything)).Return(new Table { FKTables = new List<FKTable>() });
            _codeGenHelper.Stub(x => x.DomainModel).Return(resourceModel);
        }

        protected override void Act()
        {
            var profileContext = ProfileContexts.GetResourceChildCollectionReadIncludeOnly();

            var helper = new ProfilesCodeGenHelper(profileContext, _codeGenHelper);

            const string resourceName = "Resource";

            _embeddedCollectionPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "CollectionOneProperty", null);
            _embeddedCollectionObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "CollectionOneObject", null);
            _embeddedObjectPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopObject", "ObjectProperty1", null);
            _embeddedCollectionCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "SubCollectionTwos", null);
            _embeddedCollectionUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "UnspecifiedProperty", null);
        }

        [Assert]
        public void Should_generate_explicity_included_collection_embedded_property()
        {
            _embeddedCollectionPropertyShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_explicity_included_collection_embedded_object()
        {
            _embeddedCollectionObjectShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_explicity_included_object_property_embedded_object()
        {
            _embeddedObjectPropertyShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_explicity_included_collection_embedded_collection()
        {
            _embeddedCollectionCollectionShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_NOT_generate_unspecified_collection_property()
        {
            _embeddedCollectionUnspecifiedPropertyShouldBeGenerated.ShouldBeFalse();
        }
    }

    public class When_profile_definition_includes_a_resource_level_collection_and_all_embedded_members : TestFixtureBase
    {
        private ICodeGenHelper _codeGenHelper;
        private bool _embeddedCollectionPropertyShouldBeGenerated;
        private bool _embeddedCollectionObjectShouldBeGenerated;
        private bool _embeddedCollectionCollectionShouldBeGenerated;
        private bool _embeddedCollectionUnspecifiedPropertyShouldBeGenerated;
        private bool _embeddedCollectionUnspecifiedObjectShouldBeGenerated;
        private bool _embeddedCollectionUnspecifiedCollectionShouldBeGenerated;

        protected override void Arrange()
        {
            var resourceModel = Stub<T4ResourceModel>();

            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("Resource"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ChildTables = new List<FKTable>
                {
                    new FKTable
                    {
                     IsPrimaryTable = true,
                     ThisTable = "Resource",
                     OtherTable = "TopCollectionOne"
                    }
                }
            });

            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("TopCollectionOne"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ParentFkTable = new FKTable
                {
                    IsPrimaryTable = false,
                    ThisTable = "TopCollectionOne",
                    OtherTable = "Resource"
                },
                ChildTables = new List<FKTable>
                {
                    new FKTable
                    {
                        IsPrimaryTable = true,
                        ThisTable = "TopCollectionOne",
                        OtherTable = "SubCollectionTwo"
                    }
                }
            });

            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("TopCollectionTwo"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ParentFkTable = new FKTable
                {
                    IsPrimaryTable = false,
                    ThisTable = "TopCollectionTwo",
                    OtherTable = "TopCollectionOne"
                }
            });


            _codeGenHelper = Stub<ICodeGenHelper>();
            _codeGenHelper.Stub(x => x.GetTable(Arg<string>.Is.Anything)).Return(new Table { FKTables = new List<FKTable>() });
            _codeGenHelper.Stub(x => x.DomainModel).Return(resourceModel);
        }

        protected override void Act()
        {
            var profileContext = ProfileContexts.GetResourceCollectionReadIncludeAll();

            var helper = new ProfilesCodeGenHelper(profileContext, _codeGenHelper);

            const string resourceName = "Resource";

            _embeddedCollectionPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "CollectionOneProperty", null);
            _embeddedCollectionObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "CollectionOneObject", null);
            _embeddedCollectionCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "SubCollectionTwos", null);

            _embeddedCollectionUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "UnspecifiedProperty", null);
            _embeddedCollectionUnspecifiedObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "UnspecifiedProperty", null);
            _embeddedCollectionUnspecifiedCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "UnspecifiedCollection", null);
        }

        [Assert]
        public void Should_generate_included_collection_embedded_property()
        {
            _embeddedCollectionPropertyShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_included_collection_embedded_object()
        {
            _embeddedCollectionObjectShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_included_collection_embedded_collection()
        {
            _embeddedCollectionCollectionShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_unspecified_collection_property()
        {
            _embeddedCollectionUnspecifiedPropertyShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_unspecified_collection_object()
        {
            _embeddedCollectionUnspecifiedObjectShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_unspecified_collection_collection()
        {
            _embeddedCollectionUnspecifiedCollectionShouldBeGenerated.ShouldBeTrue();
        }
    }

    public class When_profile_definition_excludes_a_resource_level_property_and_collection : TestFixtureBase
    {
        private bool _actualTopLevelPropertyShouldBeGenerated;
        private bool _actualTopLevelObjectShouldBeGenerated;
        private bool _actualTopLevelCollectionShouldBeGenerated;
        private bool _actualUnspecifiedPropertyShouldBeGenerated;
        private bool _actualUnspecifiedObjectShouldBeGenerated;
        private bool _actualUnspecifiedCollectionShouldBeGenerated;
        

        private ICodeGenHelper _codeGenHelper;

        protected override void Arrange()
        {
            var resourceModel = Stub<T4ResourceModel>();
            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Anything)).Return(new CodeGenClassModel());

            _codeGenHelper = Stub<ICodeGenHelper>();
            _codeGenHelper.Stub(x => x.GetTable(Arg<string>.Is.Anything)).Return(new Table { FKTables = new List<FKTable>() });
            _codeGenHelper.Stub(x => x.DomainModel).Return(resourceModel);
        }

        protected override void Act()
        {
            var profileContext = ProfileContexts.GetResourceLevelReadExcludeOnly();

            var helper = new ProfilesCodeGenHelper(profileContext, _codeGenHelper);

            const string resourceName = "Resource";

            _actualTopLevelPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "TopProperty", null);
            _actualTopLevelObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "TopObject", null);
            _actualTopLevelCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "TopCollectionOnes", null);
            
            _actualUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedProperty", null);
            _actualUnspecifiedObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedObject", null);
            _actualUnspecifiedCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedCollectionOnes", null);
        }

        [Assert]
        public void Should_NOT_generate_explicitly_excluded_property()
        {
            _actualTopLevelPropertyShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_NOT_generate_explicitly_excluded_object()
        {
            _actualTopLevelObjectShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_NOT_generate_explicitly_excluded_collection()
        {
            _actualTopLevelCollectionShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_generate_unspecified_property()
        {
            _actualUnspecifiedPropertyShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_unspecified_object()
        {
            _actualUnspecifiedObjectShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_unspecified_collection()
        {
            _actualUnspecifiedCollectionShouldBeGenerated.ShouldBeTrue();
        }
    }

    public class When_profile_definition_excludes_all_resource_level_members : TestFixtureBase
    {
        private bool _actualTopLevelPropertyShouldBeGenerated;
        private bool _actualTopLevelObjectShouldBeGenerated;
        private bool _actualTopLevelCollectionShouldBeGenerated;
        private bool _actualUnspecifiedPropertyShouldBeGenerated;
        private bool _actualUnspecifiedObjectShouldBeGenerated;
        private bool _actualUnspecifiedCollectionShouldBeGenerated;

        private ICodeGenHelper _codeGenHelper;

        protected override void Arrange()
        {
            var resourceModel = Stub<T4ResourceModel>();
            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Anything)).Return(new CodeGenClassModel());

            _codeGenHelper = Stub<ICodeGenHelper>();
            _codeGenHelper.Stub(x => x.GetTable(Arg<string>.Is.Anything)).Return(new Table { FKTables = new List<FKTable>() });
            _codeGenHelper.Stub(x => x.DomainModel).Return(resourceModel);
        }

        protected override void Act()
        {
            var profileContext = ProfileContexts.GetResourceLevelReadExcludeAll();

            var helper = new ProfilesCodeGenHelper(profileContext, _codeGenHelper);

            const string resourceName = "Resource";

            _actualTopLevelPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "PropertyOne", null);
            _actualTopLevelObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "ObjectOne", null);
            _actualTopLevelCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "CollectionOnes", null);

            _actualUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedProperty", null);
            _actualUnspecifiedObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedObject", null);
            _actualUnspecifiedCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, resourceName, "UnspecifiedCollectionOnes", null);
        }

        [Assert]
        public void Should_NOT_generate_explicitly_excluded_properties()
        {
            _actualTopLevelPropertyShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_NOT_generate_explicitly_excluded_objects()
        {
            _actualTopLevelObjectShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_NOT_generate_explicitly_excluded_collections()
        {
            _actualTopLevelCollectionShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_NOT_generate_any_unspecified_members()
        {
            _actualUnspecifiedPropertyShouldBeGenerated.ShouldBeFalse();
            _actualUnspecifiedObjectShouldBeGenerated.ShouldBeFalse();
            _actualUnspecifiedCollectionShouldBeGenerated.ShouldBeFalse();
        }
    }

    public class When_profile_defintion_excludes_a_resource_level_collection_and_an_embedded_collection_with_members : TestFixtureBase
    {
        private ICodeGenHelper _codeGenHelper;
        private bool _embeddedCollectionPropertyShouldBeGenerated;
        private bool _embeddedCollectionObjectShouldBeGenerated;
        private bool _embeddedObjectPropertyShouldBeGenerated;
        private bool _embeddedCollectionCollectionShouldBeGenerated;
        private bool _embeddedCollectionUnspecifiedPropertyShouldBeGenerated;

        protected override void Arrange()
        {
            var resourceModel = Stub<T4ResourceModel>();
            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("Resource"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ChildTables = new List<FKTable>
                {
                    new FKTable
                    {
                     IsPrimaryTable = true,
                     ThisTable = "Resource",
                     OtherTable = "TopCollectionOne"
                    }
                },
                OneToOneChildFKs = new List<FKTable>
                {
                    new FKTable
                    {
                        IsPrimaryTable = true,
                        ThisTable = "Resource",
                        OtherTable = "TopObject"
                    }
                }
            });

            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("TopCollectionOne"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ParentFkTable = new FKTable
                {
                    IsPrimaryTable = false,
                    ThisTable = "TopCollectionOne",
                    OtherTable = "Resource"
                },
                ChildTables = new List<FKTable>
                {
                    new FKTable
                    {
                        IsPrimaryTable = true,
                        ThisTable = "TopCollectionOne",
                        OtherTable = "SubCollectionTwo"
                    }
                }
            });

            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("TopObject"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ParentFkTable = new FKTable
                {
                    IsPrimaryTable = false,
                    ThisTable = "TopObject",
                    OtherTable = "Resource"
                }
            });

            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("TopCollectionTwo"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ParentFkTable = new FKTable
                {
                    IsPrimaryTable = false,
                    ThisTable = "TopCollectionTwo",
                    OtherTable = "TopCollectionOne"
                }
            });

            _codeGenHelper = Stub<ICodeGenHelper>();
            _codeGenHelper.Stub(x => x.GetTable(Arg<string>.Is.Anything)).Return(new Table { FKTables = new List<FKTable>() });
            _codeGenHelper.Stub(x => x.DomainModel).Return(resourceModel);
        }

        protected override void Act()
        {
            var profileContext = ProfileContexts.GetResourceChildCollectionReadExcludeOnly();

            var helper = new ProfilesCodeGenHelper(profileContext, _codeGenHelper);

            const string resourceName = "Resource";

            _embeddedCollectionPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "CollectionOneProperty", null);
            _embeddedCollectionObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "CollectionOneObject", null);
            _embeddedObjectPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopObject", "ObjectProperty1", null);
            _embeddedCollectionCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "SubCollectionTwos", null);
            _embeddedCollectionUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "UnspecifiedProperty", null);
        }

        [Assert]
        public void Should_NOT_generate_explicity_excluded_collection_embedded_property()
        {
            _embeddedCollectionPropertyShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_NOT_generate_explicity_excluded_collection_embedded_object()
        {
            _embeddedCollectionObjectShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_NOT_generate_explicity_excluded_object_embedded_property()
        {
            _embeddedObjectPropertyShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_NOT_generate_explicity_excluded_collection_embedded_collection()
        {
            _embeddedCollectionCollectionShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_generate_unspecified_collection_property()
        {
            _embeddedCollectionUnspecifiedPropertyShouldBeGenerated.ShouldBeTrue();
        }
    }

    public class When_profile_defintion_excludes_a_resource_level_collection_and_all_embedded_members : TestFixtureBase
    {
        private ICodeGenHelper _codeGenHelper;
        private bool _embeddedCollectionPropertyShouldBeGenerated;
        private bool _embeddedCollectionObjectShouldBeGenerated;
        private bool _embeddedCollectionCollectionShouldBeGenerated;
        private bool _embeddedCollectionUnspecifiedPropertyShouldBeGenerated;
        private bool _embeddedCollectionUnspecifiedObjectShouldBeGenerated;
        private bool _embeddedCollectionUnspecifiedCollectionShouldBeGenerated;

        protected override void Arrange()
        {
            var resourceModel = Stub<T4ResourceModel>();
            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("Resource"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ChildTables = new List<FKTable>
                {
                    new FKTable
                    {
                     IsPrimaryTable = true,
                     ThisTable = "Resource",
                     OtherTable = "TopCollectionOne"
                    }
                }
            });

            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("TopCollectionOne"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ParentFkTable = new FKTable
                {
                    IsPrimaryTable = false,
                    ThisTable = "TopCollectionOne",
                    OtherTable = "Resource"
                },
                ChildTables = new List<FKTable>
                {
                    new FKTable
                    {
                        IsPrimaryTable = true,
                        ThisTable = "TopCollectionOne",
                        OtherTable = "SubCollectionTwo"
                    }
                }
            });

            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Equal("TopCollectionTwo"))).Return(new CodeGenClassModel
            {
                IsBaseTable = false,
                ParentFkTable = new FKTable
                {
                    IsPrimaryTable = false,
                    ThisTable = "TopCollectionTwo",
                    OtherTable = "TopCollectionOne"
                }
            });

            _codeGenHelper = Stub<ICodeGenHelper>();
            _codeGenHelper.Stub(x => x.GetTable(Arg<string>.Is.Anything)).Return(new Table { FKTables = new List<FKTable>() });
            _codeGenHelper.Stub(x => x.DomainModel).Return(resourceModel);
        }

        protected override void Act()
        {
            var profileContext = ProfileContexts.GetResourceCollectionReadExcludeAll();

            var helper = new ProfilesCodeGenHelper(profileContext, _codeGenHelper);

            const string resourceName = "Resource";

            _embeddedCollectionPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "CollectionOneProperty", null);
            _embeddedCollectionObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "CollectionOneObject", null);
            _embeddedCollectionCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "SubCollectionTwos", null);

            _embeddedCollectionUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "UnspecifiedProperty", null);
            _embeddedCollectionUnspecifiedObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "UnspecifiedProperty", null);
            _embeddedCollectionUnspecifiedCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceName, "TopCollectionOne", "UnspecifiedCollection", null);
        }

        [Assert]
        public void Should_NOT_generate_excluded_collection_embedded_property()
        {
            _embeddedCollectionPropertyShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_NOT_generate_excluded_collection_embedded_object()
        {
            _embeddedCollectionObjectShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_NOT_generate_excluded_collection_embedded_collection()
        {
            _embeddedCollectionCollectionShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_generate_unspecified_collection_property()
        {
            _embeddedCollectionUnspecifiedPropertyShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_generate_unspecified_collection_object()
        {
            _embeddedCollectionUnspecifiedObjectShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_generate_unspecified_collection_collection()
        {
            _embeddedCollectionUnspecifiedCollectionShouldBeGenerated.ShouldBeFalse();
        }
    }

    public class When_profile_definition_has_multiple_resources_each_including_all_resource_level_members : TestFixtureBase
    {
        private bool _actualResourceOneTopLevelShouldBeGenerated;
        private bool _actualResourceOneTopLevelObjectShouldBeGenerated;
        private bool _actualResourceOneTopLevelCollectionShouldBeGenerated;
        private bool _actualResourceTwoTopLevelShouldBeGenerated;
        private bool _actualResourceTwoTopLevelObjectShouldBeGenerated;
        private bool _actualResourceTwoTopLevelCollectionShouldBeGenerated;
        private bool _actualResourceOneUnspecifiedPropertyShouldBeGenerated;
        private bool _actualResourceOneUnspecifiedObjectShouldBeGenerated;
        private bool _actualResourceOneUnspecifiedCollectionShouldBeGenerated;
        private bool _actualResourceTwoUnspecifiedPropertyShouldBeGenerated;
        private bool _actualResourceTwoUnspecifiedObjectShouldBeGenerated;
        private bool _actualResourceTwoUnspecifiedCollectionShouldBeGenerated;

        private ICodeGenHelper _codeGenHelper;

        protected override void Arrange()
        {
            var resourceModel = Stub<T4ResourceModel>();
            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Anything)).Return(new CodeGenClassModel());

            _codeGenHelper = Stub<ICodeGenHelper>();
            _codeGenHelper.Stub(x => x.GetTable(Arg<string>.Is.Anything)).Return(new Table { FKTables = new List<FKTable>() });
            _codeGenHelper.Stub(x => x.DomainModel).Return(resourceModel);
        }

        protected override void Act()
        {
            var profileContext = ProfileContexts.GetMultipleResourcesIncludeAll();

            var helper = new ProfilesCodeGenHelper(profileContext, _codeGenHelper);

            const string resourceOne = "ResourceOne";
            const string resourceTwo = "ResourceTwo";

            _actualResourceOneTopLevelShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "TopProperty", null);
            _actualResourceOneTopLevelObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "TopObject", null);
            _actualResourceOneTopLevelCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "TopCollectionOnes", null);

            _actualResourceTwoTopLevelShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "TopProperty", null);
            _actualResourceTwoTopLevelObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "TopObject", null);
            _actualResourceTwoTopLevelCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "TopCollectionOnes", null);
            
            // Too verbose?
            _actualResourceOneUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "UnspecifiedProperty", null);
            _actualResourceOneUnspecifiedObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "UnspecifiedObject", null);
            _actualResourceOneUnspecifiedCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "UnspecifiedCollectionOnes", null);

            _actualResourceTwoUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "UnspecifiedProperty", null);
            _actualResourceTwoUnspecifiedObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "UnspecifiedObject", null);
            _actualResourceTwoUnspecifiedCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "UnspecifiedCollectionOnes", null);
        }

        [Assert]
        public void Should_generate_all_members_for_the_specified_resourceone()
        {
            _actualResourceOneTopLevelShouldBeGenerated.ShouldBeTrue();
            _actualResourceOneTopLevelObjectShouldBeGenerated.ShouldBeTrue();
            _actualResourceOneTopLevelCollectionShouldBeGenerated.ShouldBeTrue();
            _actualResourceOneUnspecifiedPropertyShouldBeGenerated.ShouldBeTrue();
            _actualResourceOneUnspecifiedObjectShouldBeGenerated.ShouldBeTrue();
            _actualResourceOneUnspecifiedCollectionShouldBeGenerated.ShouldBeTrue();
        }

        [Assert]
        public void Should_generate_all_members_for_the_specified_resourcetwo()
        {
            _actualResourceTwoTopLevelShouldBeGenerated.ShouldBeTrue();
            _actualResourceTwoTopLevelObjectShouldBeGenerated.ShouldBeTrue();
            _actualResourceTwoTopLevelCollectionShouldBeGenerated.ShouldBeTrue();
            _actualResourceTwoUnspecifiedPropertyShouldBeGenerated.ShouldBeTrue();
            _actualResourceTwoUnspecifiedObjectShouldBeGenerated.ShouldBeTrue();
            _actualResourceTwoUnspecifiedCollectionShouldBeGenerated.ShouldBeTrue();
        }
    }

    public class When_profile_definition_has_multiple_resources_each_excluding_all_resource_level_members : TestFixtureBase
    {
        private bool _actualResourceOneTopLevelShouldBeGenerated;
        private bool _actualResourceOneTopLevelObjectShouldBeGenerated;
        private bool _actualResourceOneTopLevelCollectionShouldBeGenerated;
        private bool _actualResourceTwoTopLevelShouldBeGenerated;
        private bool _actualResourceTwoTopLevelObjectShouldBeGenerated;
        private bool _actualResourceTwoTopLevelCollectionShouldBeGenerated;
        private bool _actualResourceOneUnspecifiedPropertyShouldBeGenerated;
        private bool _actualResourceOneUnspecifiedObjectShouldBeGenerated;
        private bool _actualResourceOneUnspecifiedCollectionShouldBeGenerated;
        private bool _actualResourceTwoUnspecifiedPropertyShouldBeGenerated;
        private bool _actualResourceTwoUnspecifiedObjectShouldBeGenerated;
        private bool _actualResourceTwoUnspecifiedCollectionShouldBeGenerated;

        private ICodeGenHelper _codeGenHelper;

        protected override void Arrange()
        {
            var resourceModel = Stub<T4ResourceModel>();
            resourceModel.Stub(x => x.GetClassModel(Arg<string>.Is.Anything)).Return(new CodeGenClassModel());

            _codeGenHelper = Stub<ICodeGenHelper>();
            _codeGenHelper.Stub(x => x.GetTable(Arg<string>.Is.Anything)).Return(new Table { FKTables = new List<FKTable>() });
            _codeGenHelper.Stub(x => x.DomainModel).Return(resourceModel);
        }

        protected override void Act()
        {
            var profileContext = ProfileContexts.GetMultipleResourcesExcludeAll();

            var helper = new ProfilesCodeGenHelper(profileContext, _codeGenHelper);

            const string resourceOne = "ResourceOne";
            const string resourceTwo = "ResourceTwo";

            _actualResourceOneTopLevelShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "TopProperty", null);
            _actualResourceOneTopLevelObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "TopObject", null);
            _actualResourceOneTopLevelCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "TopCollectionOnes", null);

            _actualResourceTwoTopLevelShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "TopProperty", null);
            _actualResourceTwoTopLevelObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "TopObject", null);
            _actualResourceTwoTopLevelCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "TopCollectionOnes", null);

            // Too verbose?
            _actualResourceOneUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "UnspecifiedProperty", null);
            _actualResourceOneUnspecifiedObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "UnspecifiedObject", null);
            _actualResourceOneUnspecifiedCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceOne, resourceOne, "UnspecifiedCollectionOnes", null);

            _actualResourceTwoUnspecifiedPropertyShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "UnspecifiedProperty", null);
            _actualResourceTwoUnspecifiedObjectShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "UnspecifiedObject", null);
            _actualResourceTwoUnspecifiedCollectionShouldBeGenerated = helper.ShouldGenerateMember(resourceTwo, resourceTwo, "UnspecifiedCollectionOnes", null);
        }

        [Assert]
        public void Should_NOT_generate_all_members_for_the_specified_resourceone()
        {
            _actualResourceOneTopLevelShouldBeGenerated.ShouldBeFalse();
            _actualResourceOneTopLevelObjectShouldBeGenerated.ShouldBeFalse();
            _actualResourceOneTopLevelCollectionShouldBeGenerated.ShouldBeFalse();
            _actualResourceOneUnspecifiedPropertyShouldBeGenerated.ShouldBeFalse();
            _actualResourceOneUnspecifiedObjectShouldBeGenerated.ShouldBeFalse();
            _actualResourceOneUnspecifiedCollectionShouldBeGenerated.ShouldBeFalse();
        }

        [Assert]
        public void Should_NOT_generate_all_members_for_the_specified_resourcetwo()
        {
            _actualResourceTwoTopLevelShouldBeGenerated.ShouldBeFalse();
            _actualResourceTwoTopLevelObjectShouldBeGenerated.ShouldBeFalse();
            _actualResourceTwoTopLevelCollectionShouldBeGenerated.ShouldBeFalse();
            _actualResourceTwoUnspecifiedPropertyShouldBeGenerated.ShouldBeFalse();
            _actualResourceTwoUnspecifiedObjectShouldBeGenerated.ShouldBeFalse();
            _actualResourceTwoUnspecifiedCollectionShouldBeGenerated.ShouldBeFalse();
        }
    }

    static class ProfileContexts
    {
        private static XmlSerializer _serializer = new XmlSerializer(typeof(Profile));

        public static ProfileContext GetResourceLevelReadIncludeOnly()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='Resource'>
            <ReadContentType memberSelection='IncludeOnly'>
                <Property name='TopProperty'/>
                <Object name='TopObject' />
                <Collection name='TopCollectionOnes' memberSelection='IncludeAll'/>
            </ReadContentType>
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Readable);
        }

        public static ProfileContext GetResourceLevelReadIncludeAll()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='Resource'>
            <ReadContentType memberSelection='IncludeAll'/>
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Readable);
        }

        public static ProfileContext GetResourceChildCollectionReadIncludeOnly()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='Resource'>
            <ReadContentType memberSelection='IncludeOnly'>
                <Property name='TopProperty'/>
                <Object name='TopObject' memberSelection='IncludeOnly'>
                    <Property name='ObjectProperty1'/>
                </Object>
                <Collection name='TopCollectionOnes' memberSelection='IncludeOnly'>
                    <Property name='CollectionOneProperty' />
                    <Object name='CollectionOneObject' />
                    <Collection name='SubCollectionTwos' memberSelection='IncludeAll' />
                </Collection>
            </ReadContentType>
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Readable);
        }

        public static ProfileContext GetResourceCollectionReadIncludeAll()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='Resource'>
            <ReadContentType memberSelection='IncludeOnly'>
                <Collection name='TopCollectionOnes' memberSelection='IncludeAll' />
            </ReadContentType>
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Readable);
        }

        public static ProfileContext GetDescendantCollectionReadIncludeOnly()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='Resource'>
            <ReadContentType memberSelection='IncludeOnly'>
                <Collection name='TopCollectionOnes' memberSelection='IncludeOnly'>
                    <Property name='CollectionOneProperty' />
                    <Object name='CollectionOneObject' />
                    <Collection name='SubCollectionTwos' memberSelection='IncludeOnly'>
                        <Property name='CollectionOneProperty' />
                        <Object name='CollectionOneObject' />
                    </Collection>
                </Collection>
            </ReadContentType>
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Readable);
        }

        public static ProfileContext GetResourceLevelReadExcludeAll()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='Resource'>
            <ReadContentType memberSelection='ExcludeAll' />               
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Readable);
        }

        public static ProfileContext GetResourceLevelReadExcludeOnly()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='Resource'>
            <ReadContentType memberSelection='ExcludeOnly'>
                <Property name='TopProperty'/>
                <Object name='TopObject' />
                <Collection name='TopCollectionOnes' memberSelection='ExcludeAll'/>
            </ReadContentType>
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Readable);
        }

        public static ProfileContext GetResourceChildCollectionReadExcludeOnly()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='Resource'>
            <ReadContentType memberSelection='ExcludeOnly'>
                <Property name='TopProperty'/>
                <Object name='TopObject' memberSelection='ExcludeOnly'>
                    <Property name='ObjectProperty1'/>
                </Object>
                <Collection name='TopCollectionOnes' memberSelection='ExcludeOnly'>
                    <Property name='CollectionOneProperty' />
                    <Object name='CollectionOneObject' />
                    <Collection name='SubCollectionTwos' memberSelection='ExcludeAll' />
                </Collection>
            </ReadContentType>
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Readable);
        }

        public static ProfileContext GetResourceCollectionReadExcludeAll()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='Resource'>
            <ReadContentType memberSelection='IncludeOnly'>
                <Collection name='TopCollectionOnes' memberSelection='ExcludeAll' />
            </ReadContentType>
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Readable);
        }

        public static ProfileContext GetDescendantCollectionReadExcludeOnly()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='Resource'>
            <ReadContentType memberSelection='ExcludeOnly'>
                <Collection name='TopCollectionOnes' memberSelection='ExcludeOnly'>
                    <Property name='CollectionOneProperty' />
                    <Object name='CollectionOneObject' />
                    <Collection name='SubCollectionTwos' memberSelection='ExcludeOnly'>
                        <Property name='CollectionOneProperty' />
                        <Object name='CollectionOneObject' />
                    </Collection>
                </Collection>
            </ReadContentType>
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Readable);
        }

        public static ProfileContext GetMultipleResourcesIncludeAll()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='ResourceOne'>
            <WriteContentType memberSelection='IncludeAll'/>
        </Resource>
        <Resource name='ResourceTwo'>
            <WriteContentType memberSelection='IncludeAll'/>
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Writable);
        }

        public static ProfileContext GetMultipleResourcesExcludeAll()
        {
            string profileXml = @"
    <Profile name='TestProfile'>
        <Resource name='ResourceOne'>
            <ReadContentType memberSelection='ExcludeAll'/>
        </Resource>
        <Resource name='ResourceTwo'>
            <ReadContentType memberSelection='ExcludeAll'/>
        </Resource>
    </Profile>
";
            return CreateProfileContext(profileXml, ContentTypeUsage.Readable);
        }

        private static ProfileContext CreateProfileContext(string profileXml, ContentTypeUsage contentTypeUsage)
        {
            return new ProfileContext
            {
                ContentTypeUsage = contentTypeUsage,
                Profile = (Profile) _serializer.Deserialize(new StringReader(profileXml))
            };
        }
    }
}
