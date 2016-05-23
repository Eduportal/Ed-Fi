// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    /// <summary>
    /// Provides an extensible mechanism for handling special cases in the Ed-Fi data model where key unification is occurring by calling
    /// into an inner list of <see cref="IContextSpecificReferenceValueBuilder"/> implementations that are individually given a chance
    /// to satisfy the construction of a context-specific reference.
    /// </summary>
    public class ContextSpecificReferenceValueBuilder : IValueBuilder
    {
        private readonly IContextSpecificReferenceValueBuilder[] _contextSpecificReferenceValueBuilders;

        public ContextSpecificReferenceValueBuilder(IContextSpecificReferenceValueBuilder[] contextSpecificReferenceValueBuilders)
        {
            this._contextSpecificReferenceValueBuilders = contextSpecificReferenceValueBuilders;
        }

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (!buildContext.TargetType.Name.EndsWith("Reference"))
                return ValueBuildResult.NotHandled;

            foreach (var valueBuilder in _contextSpecificReferenceValueBuilders)
            {
                var buildResult = valueBuilder.TryBuild(buildContext);
                
                if (buildResult.Handled)
                    return buildResult;
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset() {}

        private ITestObjectFactory _factory;

        public ITestObjectFactory Factory
        {
            get { return _factory; }
            set
            {
                _factory = value;

                foreach (var builder in _contextSpecificReferenceValueBuilders)
                    builder.Factory = value;
            }
        }
    }
}