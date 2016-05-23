// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Linq;
using Castle.DynamicProxy;
using EdFi.Ods.Utilities.LoadGeneration._Extensions;
using EdFi.TestObjects;
using log4net;
using Newtonsoft.Json;

namespace EdFi.Ods.Utilities.LoadGeneration.Interceptors
{
    public class ValueBuilderLoggingInterceptor : IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            if (invocation.Method.Name != "TryBuild")
            {
                invocation.Proceed();
                return;
            }

            ILog logger = LogManager.GetLogger(invocation.TargetType);

            //BuildContext buildContext = null;
            //string contextDisplayBefore = null;
            //string correlationId = null;

            //if (logger.IsDebugEnabled)
            //{
            //    correlationId = Guid.NewGuid().ToString("n");

            //    buildContext = (BuildContext) invocation.Arguments[0];

            //    contextDisplayBefore = buildContext.ToString();

            //    logger.DebugFormat("BuildContext (before): {0} {{CorrelationId: {1}}}", contextDisplayBefore, correlationId);
            //}

            // Allow call to proceed
            invocation.Proceed();

            if (logger.IsDebugEnabled)
            {
                //string contextDisplayAfter = buildContext.ToString();

                //if (contextDisplayAfter != contextDisplayBefore)
                //    logger.DebugFormat("BuildContext MODIFIED (after): {0} {{CorrelationId: {1}}}", contextDisplayAfter, correlationId);

                var result = invocation.ReturnValue as ValueBuildResult;

                // Log source of every value being built
                if (result != null && result.ShouldSetValue)
                {
                    //ILog logger = LogManager.GetLogger(invocation.TargetType);
                    //var buildContext = buildContext as BuildContext;
                    var buildContext = (BuildContext) invocation.Arguments[0];

                    logger.DebugFormat("Value builder '{0}' handled the request for property path '{1}' and built a value of '{2}'.",
                        invocation.TargetType.Name, 
                        buildContext.LogicalPropertyPath,
                        result.Value == null 
                            ? "[null]" 
                            : (result.Value.GetType().IsCustomClass() 
                                ? "\r\n" + JsonConvert.SerializeObject(result.Value, Formatting.Indented) 
                                : result.Value));
                }
            }
        }

        //private static string GetContextDisplayText(BuildContext buildContext)
        //{
        //    return string.Join(", ", buildContext.PropertyValueConstraints.Select(x => string.Format("[{0}={1}]", x.Key, x.Value)));
        //}
    }
}