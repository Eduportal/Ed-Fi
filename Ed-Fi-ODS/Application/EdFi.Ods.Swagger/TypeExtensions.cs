using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using EdFi.Ods.Swagger.Attributes;
using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Swagger
{
    public static class TypeExtensions
    {
        public static string ToSwaggerType(this Type type)
        {
            return type.ToSwaggerType(null);
        }

        public static string ToSwaggerType(this Type type, IDictionary<string, string> customTypeMappings)
        {
            TypeCategory category;
            Type containedType;
            return type.ToSwaggerType(out category, out containedType, customTypeMappings);
        }


        private static readonly Dictionary<string, string> PrimitiveTypeMap = new Dictionary<string, string>
                {
                    {"Byte", "string"},
                    {"Boolean", "boolean"},
                    {"Int32", "integer"},
                    {"Int64", "integer"},
                    {"Single", "number"},
                    {"Double", "number"},
                    {"Decimal", "number"},
                    {"String", "string"},
                    {"DateTime", "string"}
                };

        internal static string ToSwaggerType(this Type type, out TypeCategory category, out Type containedType, IDictionary<string, string> customTypeMappings = null)
        {
            if (type == typeof(HttpResponseMessage))
            {
                category = TypeCategory.Unkown;
                containedType = null;
                return "void";
            }

            if (type == typeof(IHttpActionResult))
            {
                category = TypeCategory.Unkown;
                containedType = null;
                return "void";
            }

            if (type == null)
            {
                category = TypeCategory.Primitive;
                containedType = null;
                return "void";
            }

            string swaggerTypeName;
            string dotNetTypeName = type.Name;

            if (PrimitiveTypeMap.TryGetValue(dotNetTypeName, out swaggerTypeName) 
                || (customTypeMappings != null && customTypeMappings.TryGetValue(dotNetTypeName, out swaggerTypeName)))
            {
                category = TypeCategory.Primitive;
                containedType = null;
                return swaggerTypeName;
            }

            Type innerTypeOfNullable;
            if (type.IsNullableType(out innerTypeOfNullable))
            {
                return innerTypeOfNullable.ToSwaggerType(out category, out containedType);
            }

            if (type.IsEnum)
            {
                category = TypeCategory.Primitive;
                containedType = null;
                return "string";
            }

            var enumerableItemType = GetGenericItemType(type, typeof(IEnumerable<>));

            if (enumerableItemType != null)
            {
                category = TypeCategory.Container;
                containedType = enumerableItemType;
                return String.Format("array");
            }

            category = TypeCategory.Complex;
            containedType = null;
            return dotNetTypeName;
        }

        //TODO: Evan: why do we need a ConcurrentDictionary for a static/thread-safe flow?
        private static readonly ConcurrentDictionary<Type,Type> GenericItemTypesByType = new ConcurrentDictionary<Type, Type>();  

        private static Type GetGenericItemType(Type type, Type genericType)
        {
            return GenericItemTypesByType.GetOrAdd(type, t =>
                {
                    var enumerable = type.AsGenericType(genericType);

                    if (enumerable == null)
                        return null;

                    return enumerable.GetGenericArguments().First();
                });
        }

        private static readonly Dictionary<string, string> PrimitiveFormatMap = new Dictionary<string, string>
                {
                    {"Byte", "byte"},
                    {"Int32", "int32"},
                    {"Int64", "int64"},
                    {"Single", "float"},
                    {"Double", "double"},
                    {"Decimal", "double"},
                    {"DateTime", "date-time"}
                };

        internal static string ToSwaggerFormat(this Type type)
        {
            string swaggerName;

            if (PrimitiveFormatMap.TryGetValue(type.Name, out swaggerName))
                return swaggerName;

            return null;
        }

        private static readonly ConcurrentDictionary<Type, Tuple<Type, bool>> IsNullableByType = new ConcurrentDictionary<Type, Tuple<Type, bool>>(); 

        internal static bool IsNullableType(this Type type, out Type innerType)
        {
            var result = IsNullableByType.GetOrAdd(type, t =>
                {
                    var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
                    if (isNullable)
                    {
                        var concreteType = type.GetGenericArguments().Single();
                        return new Tuple<Type, bool>(concreteType, true);
                    }

                    return new Tuple<Type, bool>(null, false);
                });

            innerType = result.Item1;
            return result.Item2;
        }

        internal static IList<string> EnumValues(this Type type)
        {
            Type innerType;
            if (type.IsNullableType(out innerType))
                return innerType.EnumValues();

            return type.IsEnum ? type.GetEnumNames() : null;
        }

        public static ModelPropertySpec ToModelPropertySpec(this PropertyInfo property)
        {
            var attributes = property.GetCustomAttributes().ToList();
          
            var descr = attributes.OfType<DescriptionAttribute>().SingleOrDefault();
            var range =  attributes.OfType<RangeAttribute>().SingleOrDefault();
            var min = attributes.OfType<MinLengthAttribute>().SingleOrDefault();
            var max = attributes.OfType<MaxLengthAttribute>().SingleOrDefault();

            TypeCategory category;
            Type containedType;
            var type = property.PropertyType.ToSwaggerType(out category, out containedType);
            ItemsSpec items = null;
            if (containedType != null)
            {
                items = new ItemsSpec() { Ref = containedType.ToSwaggerType() };
            }
            return new ModelPropertySpec
                {
                    Type = type,
                    Items = items,
                    Format = property.PropertyType.ToSwaggerFormat(),
                    Description = descr == null ? null : descr.Description,
                    Required = true,
                    Enum = property.PropertyType.EnumValues(),
                    Maximum = range == null ? (max == null ? null : max.Length.ToString(CultureInfo.InvariantCulture)) : range.Maximum.ToString(),
                    Minimum = range == null ? (min == null ? null : min.Length.ToString(CultureInfo.InvariantCulture)) : range.Minimum.ToString(),
                };
        }


        private static readonly ConcurrentDictionary<Type, Type> GenericTypesByType = new ConcurrentDictionary<Type, Type>(); 

        private static Type AsGenericType(this Type type, Type genericType)
        {
            return GenericTypesByType.GetOrAdd(type, t =>
                {
                    var result = type.GetInterfaces()
                        .Union(new[] { type })
                        .Where(x => x.IsGenericType && x.GetGenericTypeDefinition() == genericType);

                    return result.FirstOrDefault();
                });
        }

        internal static string Description(this IEnumerable<ApiDescription> apiDescriptionGroup)
        {
            var descr = apiDescriptionGroup.First().ActionDescriptor.ControllerDescriptor.ControllerType.GetCustomAttribute<DescriptionAttribute>();
            return descr == null ? null : descr.Description;
        }

        internal static string Summary(this ApiDescription apiDescription)
        {
            var attr = apiDescription.ActionDescriptor.GetCustomAttributes<ApiRequestAttribute>().FirstOrDefault();
            return attr == null ? null : attr.Summary;
        }

        internal static string Notes(this ApiDescription apiDescription)
        {
            var attr = apiDescription.ActionDescriptor.GetCustomAttributes<ApiRequestAttribute>().FirstOrDefault();
            return attr == null ? null : attr.Notes;
        }

        internal static ICollection<ApiResponseMessageSpec> ResponseMessageSpecs(this ApiDescription apiDescription)
        {
            var attrs = apiDescription.ActionDescriptor.GetCustomAttributes<ApiResponseMessageAttribute>();
            return new List<ApiResponseMessageSpec>(attrs.Select(attr => new ApiResponseMessageSpec
            {
                Code = (int)attr.Code,
                Message = attr.Message,
                ResponseModel = attr.ResponseModel == null? "void": attr.ResponseModel.Name
            }));
        }

        internal static ApiMemberAttribute ApiMemberAttribute(this ApiParameterDescription parameterDescription)
        {
            return parameterDescription.ParameterDescriptor.GetCustomAttributes<ApiMemberAttribute>().FirstOrDefault();
        }

        internal static ETagMatch ETag(this ApiParameterDescription parameterDescription)
        {
            var attr =
                parameterDescription.ParameterDescriptor.GetCustomAttributes<ETagMatchAttribute>().FirstOrDefault();
            return attr == null ? ETagMatch.None : attr.ETagMatch;
        }
    }

    public enum TypeCategory
    {
        Unkown,
        Primitive,
        Complex,
        Container
    }
}