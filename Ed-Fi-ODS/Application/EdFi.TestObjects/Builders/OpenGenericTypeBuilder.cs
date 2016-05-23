using System;
using System.Collections.Generic;
using System.Linq;

namespace EdFi.TestObjects.Builders
{
    public class OpenGenericTypeBuilder : IValueBuilder
    {
        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (buildContext.TargetType.IsGenericTypeDefinition)
            {
                var typeArgs = buildContext.TargetType.GetGenericArguments();

                var concreteTypes = new List<Type>();

                foreach (var typeArg in typeArgs)
                {
                    var typeConstraint = typeArg.GetGenericParameterConstraints().FirstOrDefault();

                    if (typeConstraint == null)
                    {
                        // Just use an integer... it's simple.
                        concreteTypes.Add(typeof(int));
                    }
                    else if (typeConstraint.IsAbstract)
                    {
                        // Find derived types
                        throw new Exception(string.Format("Generic constraint on type '{0}' uses type '{1}', which is abstract and not currently supported.",
                                                          buildContext.TargetType.FullName, typeConstraint.FullName));
                    }
                    else
                    {
                        concreteTypes.Add(typeConstraint);
                    }
                }

                var closedGenericType = buildContext.TargetType.MakeGenericType(concreteTypes.ToArray());

                return ValueBuildResult.WithValue(this.Factory.Create(buildContext.LogicalPropertyPath, closedGenericType, buildContext.PropertyValueConstraints, buildContext.ObjectGraphLineage), buildContext.LogicalPropertyPath);
            }

            return ValueBuildResult.NotHandled;
        }

        public void Reset()
        {
        }

        public ITestObjectFactory Factory { get; set; }
    }
}