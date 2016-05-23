using System;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace EdFi.Ods.Api.Common
{
    public abstract class ETagMatchAttribute : ParameterBindingAttribute
    {
        private readonly ETagMatch _match;

        protected ETagMatchAttribute(ETagMatch match)
        {
            _match = match;
        }

        public ETagMatch ETagMatch
        {
            get { return _match; }
        }

        public override HttpParameterBinding GetBinding(HttpParameterDescriptor parameter)
        {
            if (parameter.ParameterType == typeof(ETag))
            {
                return new ETagParameterBinding(parameter, _match);
            }
            return parameter.BindAsError("Wrong parameter type");
        }
    }

    /// <summary>
    /// Attribute used to map the if_match header to an ETag parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class IfMatchAttribute : ETagMatchAttribute
    {
        public IfMatchAttribute()
            : base(ETagMatch.IfMatch)
        {
        }
    }

    /// <summary>
    /// Attribute used to map the if_none_match header to an ETag parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public class IfNoneMatchAttribute : ETagMatchAttribute
    {
        public IfNoneMatchAttribute()
            : base(ETagMatch.IfNoneMatch)
        {
        }
    }
}
