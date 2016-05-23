// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using EdFi.TestObjects;
using log4net;

namespace EdFi.Ods.Utilities.LoadGeneration.ValueBuilders
{
    public class SchoolYearValueBuilder : IValueBuilder
    {
        private ILog _logger = LogManager.GetLogger(typeof(SchoolYearValueBuilder));

        private readonly IApiSdkFacade _apiSdkFacade;
        private readonly IApiSdkReflectionProvider _apiSdkReflectionProvider;

        public SchoolYearValueBuilder(IApiSdkFacade apiSdkFacade, IApiSdkReflectionProvider apiSdkReflectionProvider)
        {
            _apiSdkFacade = apiSdkFacade;
            _apiSdkReflectionProvider = apiSdkReflectionProvider;
        }

        private int _schoolYear;

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType != typeof(int)
                || !buildContext.LogicalPropertyPath.EndsWith("SchoolYear", StringComparison.InvariantCultureIgnoreCase))
                return ValueBuildResult.NotHandled;

            return ValueBuildResult.WithValue(GetSchoolYear(), null);
        }

        private int GetSchoolYear()
        {
            if (_schoolYear != 0)
                return _schoolYear;

            Type modelType;

            if (!_apiSdkReflectionProvider.TryGetModelType("SchoolYearType", out modelType))
                throw new Exception("Unable to find the model type for 'SchoolYearType' in the REST API SDK.");

            _logger.Info("Retrieving school years.");
            var results = _apiSdkFacade.GetAll(modelType);

            int maxSchoolYear = 0;
            int maxCurrentSchoolYear = 0;

            foreach (dynamic schoolYearType in results)
            {
                if (schoolYearType.schoolYear > maxSchoolYear)
                    maxSchoolYear = schoolYearType.schoolYear;

                if (schoolYearType.currentSchoolYear && schoolYearType.schoolYear > maxCurrentSchoolYear)
                    maxCurrentSchoolYear = schoolYearType.schoolYear;
            }

            if (maxCurrentSchoolYear > 0)
                _schoolYear = maxCurrentSchoolYear;
            else if (maxSchoolYear > 0)
                _schoolYear = maxSchoolYear;
            else
                throw new Exception("No school years were returned from the REST API.");

            return _schoolYear;
        }

        public void Reset() {}
        public ITestObjectFactory Factory { get; set; }
    }
}
