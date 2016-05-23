using System.Collections.Generic;

namespace EdFi.Common.Security.Authorization
{
    public class ParameterizedFilterBuilder
    {
        private readonly IDictionary<string, IDictionary<string, object>> _filtersByName 
            = new Dictionary<string, IDictionary<string, object>>();

        public FilterDescriptor Filter(string filterName)
        {
            if (!_filtersByName.ContainsKey(filterName))
                _filtersByName[filterName] = new Dictionary<string, object>();

            return new FilterDescriptor(filterName, _filtersByName);
        }

        public IDictionary<string, IDictionary<string, object>> Value
        {
            get { return _filtersByName; }
        }
    }

    public class FilterDescriptor
    {
        private readonly string _filterName;
        private readonly IDictionary<string, IDictionary<string, object>> _filters;

        public FilterDescriptor(string filterName, IDictionary<string, IDictionary<string, object>> filters)
        {
            _filterName = filterName;
            _filters = filters;
        }

        public FilterDescriptor Set(string parameterName, object parameterValue)
        {
            _filters[_filterName][parameterName] = parameterValue;

            return this;
        }

        public IDictionary<string, IDictionary<string, object>> Value
        {
            get { return _filters; }
        }
    }
}