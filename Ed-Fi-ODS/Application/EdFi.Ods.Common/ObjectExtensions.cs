using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace EdFi.Ods.Common
{
    public static class ObjectExtensions
    {
        public static IDictionary<string, object> ToDictionary(this object instance, Func<PropertyDescriptor, object, bool> selector = null)
        {
            // Default selector
            if (selector == null)
                selector = (descriptor, o) => true;

            var dictionary = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            if (instance != null)
            {
                foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(instance))
                {
                    object value = descriptor.GetValue(instance);

                    if (selector(descriptor, value))
                        dictionary.Add(descriptor.Name, value);
                }
            }
            return dictionary;
        }
    }
}
