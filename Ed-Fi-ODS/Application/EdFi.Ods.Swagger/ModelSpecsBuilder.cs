﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EdFi.Ods.Swagger
{
    public class ModelSpecsBuilder
    {
        private readonly List<Type> _apiTypes;

        public ModelSpecsBuilder()
        {
            _apiTypes = new List<Type>();
        }

        public ModelSpecsBuilder AddType(Type type)
        {
            if (!_apiTypes.Contains(type))
                _apiTypes.Add(type);
            return this;
        }

        public ModelDictionarySpec Build()
        {
            var modelSpecs = new ModelDictionarySpec();
            AddToModelSpecs(_apiTypes, modelSpecs);
            return modelSpecs;
        }

        private void AddToModelSpecs(IEnumerable<Type> apiTypes, Dictionary<string, ModelSpec> modelSpecs)
        {
            foreach (var type in apiTypes)
            {
                TypeCategory category;
                Type containedType;
                var dataType = type.ToSwaggerType(out category, out containedType);

                if (category == TypeCategory.Unkown || category == TypeCategory.Primitive) continue;

                if (modelSpecs.ContainsKey(dataType)) continue;

                var relatedTypes = new List<Type>();
                if (category == TypeCategory.Container)
                {
                    relatedTypes.Add(containedType);
                }
                else
                {
                    if (type.FullName.StartsWith("EdFi"))
                    {
                        var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
                        var propertySpecs = properties
                            .ToDictionary(pi => pi.Name, pi => pi.ToModelPropertySpec());

                        modelSpecs.Add(dataType, new ModelSpec { ID = dataType, Properties = propertySpecs });

                        relatedTypes.AddRange(properties.Select(p => p.PropertyType));   
                    }
                }
                AddToModelSpecs(relatedTypes, modelSpecs);
            }
        }
    }
}