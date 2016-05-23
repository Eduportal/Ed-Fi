using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection.Emit;

namespace EdFi.Ods.Security.AuthorizationStrategies
{
    /// <summary>
    /// Creates context data from a source entity using the specified types, based on matching property names.
    /// </summary>
    /// <remarks>This class is threadsafe and uses shared static state to persist the dynamically created factory methods.</remarks>
    public class AuthorizationContextDataFactory
    {
        public TContextData CreateContextData<TContextData>(object entity)
            where TContextData : class, new()
        {
            // Can't map anything if source is null
            if (entity == null)
                return null;

            Type sourceType = entity.GetType();
            Type contextDataType = typeof(TContextData);

            string methodName = sourceType.FullName.Replace('.', '_') + "_to_" + contextDataType.FullName.Replace('.', '_');

            var factoryDelegate = FactoryMethodByMethodName.GetOrAdd(methodName,
                mn => CreateDynamicExtractorMethod(mn, sourceType, contextDataType));

            // If no properties were present to be mapped, return a null context
            if (factoryDelegate == null)
                return null;

            var contextData = new TContextData();
            factoryDelegate.Invoke(entity, contextData);

            return contextData;
        }

        private delegate void ExtractDelegate(object source, object target);

        private static readonly ConcurrentDictionary<string, ExtractDelegate> FactoryMethodByMethodName =
            new ConcurrentDictionary<string, ExtractDelegate>();

        private static ExtractDelegate CreateDynamicExtractorMethod(string methodName, Type sourceType, Type targetType)
        {
            var sourceProperties = sourceType.GetProperties().ToDictionary(x => x.Name, x => x);
            var targetProperties = targetType.GetProperties().ToDictionary(x => x.Name, x => x);

            var propertyNamesToMap = targetProperties.Keys.Intersect(sourceProperties.Keys).ToList();

            // If there are no properties to map, return nul
            if (!propertyNamesToMap.Any())
                return null;

            var m = new DynamicMethod(methodName, null, new[] { typeof(object), typeof(object) }, true);
            var il = m.GetILGenerator();

            il.DeclareLocal(targetType);
            il.DeclareLocal(sourceType);

            il.Emit(OpCodes.Nop);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Castclass, targetType);
            il.Emit(OpCodes.Stloc_0);

            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, sourceType);
            il.Emit(OpCodes.Stloc_1);

            foreach (string propertyName in propertyNamesToMap)
            {
                var sourceProperty = sourceProperties[propertyName];
                var targetProperty = targetProperties[propertyName];

                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldloc_1);

                var sourceUnderlyingType = Nullable.GetUnderlyingType(sourceProperty.PropertyType);

                // Non-nullable type source
                il.Emit(OpCodes.Callvirt, sourceProperty.GetGetMethod());

                var targetUnderlyingType = Nullable.GetUnderlyingType(targetProperty.PropertyType);

                if ((targetUnderlyingType != null && sourceUnderlyingType != null) || targetUnderlyingType == null)
                {
                    // Non-nullable type target
                    il.Emit(OpCodes.Callvirt, targetProperty.GetSetMethod());
                }
                else
                {
                    // Nullable type target
                    il.Emit(
                        OpCodes.Newobj,
                        targetProperty.PropertyType.GetConstructor(new Type[] { targetUnderlyingType }));

                    il.Emit(OpCodes.Callvirt, targetProperty.GetSetMethod());
                }

                il.Emit(OpCodes.Nop);
            }

            il.Emit(OpCodes.Ret);

            var d = (ExtractDelegate) m.CreateDelegate(typeof(ExtractDelegate));

            return d;
        }
    }
}