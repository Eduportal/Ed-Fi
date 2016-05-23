// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    public class ReferenceLinkPropertySkipper : PropertySkipperBase
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            // Add criteria that "link" must be on a containing type that ends with "Reference"
            return base.IsHandled(buildContext)
                && buildContext.GetContainingTypeName().EndsWith("Reference", StringComparison.InvariantCultureIgnoreCase);
        }

        protected override string PropertyName
        {
            get { return "link"; }
        }
    }
}