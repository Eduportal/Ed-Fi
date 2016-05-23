// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using EdFi.TestObjects.Builders;
using Newtonsoft.Json.Linq;
using ICustomAttributeProvider = EdFi.TestObjects.ICustomAttributeProvider;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public class EdFiAwareDecimalRangeAttributeProviderDecorator : ICustomAttributeProvider
    {
        private readonly ICustomAttributeProvider _next;

        public EdFiAwareDecimalRangeAttributeProviderDecorator(ICustomAttributeProvider next)
        {
            _next = next;

            InitializeLazyProperties();
        }

        public object[] GetCustomAttributes(MemberInfo memberInfo, Type attributeType, bool inherit)
        {
            if (attributeType != typeof(RangeAttribute))
                return _next.GetCustomAttributes(memberInfo, attributeType, inherit);

            var rangeAttribute = GetRangeAttribute(memberInfo);

            if (rangeAttribute == null)
                return _next.GetCustomAttributes(memberInfo, attributeType, inherit);

            return (new[] { rangeAttribute })
                .Concat(_next.GetCustomAttributes(memberInfo, attributeType, inherit))
                .ToArray();
        }

        public object[] GetCustomAttributes(MemberInfo memberInfo, bool inherit)
        {
            var rangeAttribute = GetRangeAttribute(memberInfo);

            if (rangeAttribute == null)
                return _next.GetCustomAttributes(memberInfo, inherit);
            
            return (new[] {rangeAttribute})
                .Concat(_next.GetCustomAttributes(memberInfo, inherit))
                .ToArray();
        }

        private RangeAttribute GetRangeAttribute(MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;

            if (propertyInfo == null)
                return null;

            Type targetType;

            if (propertyInfo.PropertyType.IsGenericType
                && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // Reassign the nullable type to the underlying target type
                targetType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
            }
            else
            {
                targetType = propertyInfo.ReflectedType;
            }

            // Not a decimal property?  Don't do anything special
            if (!(targetType == typeof(decimal) || targetType == typeof(double))
                || propertyInfo.DeclaringType == null
                //|| memberInfo.MemberType != MemberTypes.Property
                )
            {
                return null;
            }

            var rangeAttribute = CreateRangeAttribute(propertyInfo.DeclaringType.Name, propertyInfo.Name);

            return rangeAttribute;
        }

        private RangeAttribute CreateRangeAttribute(string containingTypeName, string propertyName)
        {
            string key = containingTypeName + "." + propertyName;
            Tuple<int, int> tuple;

            if (!precisionScaleTupleByKey.Value.TryGetValue(key, out tuple))
                return null;

            string maxAsText = new string('9', tuple.Item1 - tuple.Item2) 
                + "." + new string('9', tuple.Item2);

            return new RangeAttribute(0, double.Parse(maxAsText));
        }

        private void InitializeLazyProperties()
        {
            precisionScaleTupleByKey = new Lazy<IDictionary<string, Tuple<int, int>>>(
                () => 
                    new Dictionary<string, Tuple<int, int>>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        {"CalendarDateCalendarEvent.EventDuration", Tuple.Create(3, 2)},
                        {"StudentSpecialEducationProgramAssociation.SpecialEducationHoursPerWeek", Tuple.Create(5, 2)},
                        {"StudentSpecialEducationProgramAssociation.SchoolHoursPerWeek", Tuple.Create(5, 2)},
                        {"CurrentStaffEducationOrgEmploymentAssociation.FullTimeEquivalency", Tuple.Create(5, 2)},
                        {"LocalEducationAgencyFederalFunds.SchoolImprovementReservedFundsPercentage", Tuple.Create(5, 4)},
                        {"LocalEducationAgencyFederalFunds.StateAssessmentAdministrationFunding", Tuple.Create(5, 4)},
                        {"StaffSectionAssociation.PercentageContribution", Tuple.Create(5, 4)},
                        {"StaffEducationOrganizationEmploymentAssociation.FullTimeEquivalency", Tuple.Create(5, 4)},
                        {"ObjectiveAssessment.PercentOfAssessment", Tuple.Create(6, 2)},
                        {"Section.AvailableCreditConversion", Tuple.Create(9, 2)},
                        {"Section.AvailableCredit", Tuple.Create(9, 2)},
                        {"GraduationPlanCreditsByCourse.Credit", Tuple.Create(9, 2)},
                        {"GraduationPlanCreditsByCourse.CreditConversion", Tuple.Create(9, 2)},
                        {"CourseTranscript.AttemptedCreditConversion", Tuple.Create(9, 2)},
                        {"CourseTranscript.AttemptedCredit", Tuple.Create(9, 2)},
                        {"CourseTranscript.EarnedCreditConversion", Tuple.Create(9, 2)},
                        {"CourseTranscript.EarnedCredit", Tuple.Create(9, 2)},
                        {"Course.MinimumAvailableCreditConversion", Tuple.Create(9, 2)},
                        {"Course.MinimumAvailableCredit", Tuple.Create(9, 2)},
                        {"Course.MaximumAvailableCreditConversion", Tuple.Create(9, 2)},
                        {"Course.MaximumAvailableCredit", Tuple.Create(9, 2)},
                        {"StudentAcademicRecord.SessionEarnedCreditConversion", Tuple.Create(9, 2)},
                        {"StudentAcademicRecord.SessionEarnedCredit", Tuple.Create(9, 2)},
                        {"StudentAcademicRecord.SessionAttemptedCreditConversion", Tuple.Create(9, 2)},
                        {"StudentAcademicRecord.SessionAttemptedCredit", Tuple.Create(9, 2)},
                        {"GraduationPlan.TotalRequiredCredit", Tuple.Create(9, 2)},
                        {"GraduationPlan.TotalRequiredCreditConversion", Tuple.Create(9, 2)},
                        {"GraduationPlanCreditsBySubject.Credit", Tuple.Create(9, 2)},
                        {"GraduationPlanCreditsBySubject.CreditConversion", Tuple.Create(9, 2)},
                        {"CourseTranscriptAdditionalCredit.Credit", Tuple.Create(9, 2)},
                        {"StudentAcademicRecord.CumulativeEarnedCreditConversion", Tuple.Create(9, 2)},
                        {"StudentAcademicRecord.CumulativeEarnedCredit", Tuple.Create(9, 2)},
                        {"StudentAcademicRecord.CumulativeAttemptedCreditConversion", Tuple.Create(9, 2)},
                        {"StudentAcademicRecord.CumulativeAttemptedCredit", Tuple.Create(9, 2)},
                        {"StudentLearningStyle.VisualLearning", Tuple.Create(9, 4)},
                        {"StudentLearningStyle.AuditoryLearning", Tuple.Create(9, 4)},
                        {"StudentLearningStyle.TactileLearning", Tuple.Create(9, 4)},
                        {"LeaveEvent.HoursOnLeave", Tuple.Create(18, 2)},
                        {"ReportCard.GPAGivenGradingPeriod", Tuple.Create(18, 4)},
                        {"ReportCard.GPACumulative", Tuple.Create(18, 4)},
                        {"ReportCard.NumberOfDaysAbsent", Tuple.Create(18, 4)},
                        {"ReportCard.NumberOfDaysInAttendance", Tuple.Create(18, 4)},
                        {"StudentAcademicRecord.CumulativeGradePointsEarned", Tuple.Create(18, 4)},
                        {"StudentAcademicRecord.CumulativeGradePointAverage", Tuple.Create(18, 4)},
                        {"StudentAcademicRecord.SessionGradePointsEarned", Tuple.Create(18, 4)},
                        {"StudentAcademicRecord.SessionGradePointAverage", Tuple.Create(18, 4)},
                    }, true);

            // This dictionary created using the following SQL:
            /*
            select c.TABLE_SCHEMA, c.TABLE_NAME, c.COLUMN_NAME, c.NUMERIC_PRECISION, c.NUMERIC_SCALE 
            FROM INFORMATION_SCHEMA.COLUMNS c
            where c.DATA_TYPE = 'decimal'
	            and c.TABLE_SCHEMA in ('edfi', 'extension')
            order by c.NUMERIC_PRECISION, c.NUMERIC_SCALE;
            */
            // Then copy/paste results into code window and perform the following Search/Replace:
            //   Search:    edfi\t(\w+)\t(\w+)\t(\d+)\t(\d+)
            //   Replace:   {"$1.$2", Tuple.Create($3,$4)},

        }

        private Lazy<IDictionary<string, Tuple<int, int>>> precisionScaleTupleByKey;


    }
}