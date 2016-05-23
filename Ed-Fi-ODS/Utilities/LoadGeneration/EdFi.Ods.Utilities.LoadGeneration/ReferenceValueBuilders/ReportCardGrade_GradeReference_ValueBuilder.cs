// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    public class ReportCardGrade_GradeReference_ValueBuilder : ReferenceValueBuilderBase, IContextSpecificReferenceValueBuilder
    {
        protected override bool IsHandled(BuildContext buildContext)
        {
            return buildContext.Matches("ReportCardGrade", "GradeReference");
        }

        protected override IDictionary<string, object> GetPropertyValueConstraints(BuildContext buildContext)
        {
            // Get the parent
            dynamic reportCard = buildContext.GetParentInstance();

            var constraints = new Dictionary<string, object>
            {
                {"studentUniqueId",                      reportCard.studentReference.studentUniqueId},
                {"gradingPeriodEducationOrganizationId", reportCard.gradingPeriodReference.educationOrganizationId},
                {"gradingPeriodDescriptor",              reportCard.gradingPeriodReference.descriptor},
                {"gradingPeriodBeginDate",               reportCard.gradingPeriodReference.beginDate},
            };

            return constraints;
        }
    }
}