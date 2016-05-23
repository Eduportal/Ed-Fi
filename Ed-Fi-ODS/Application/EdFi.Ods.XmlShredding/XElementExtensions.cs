using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using EdFi.Common.Extensions;

namespace EdFi.Ods.XmlShredding
{
    public static class XElementExtensions
    {
        public static string ValueOrDefault(this XElement element)
        {
            return (string)element;
        }

        public static XElement ElementOrEmpty(this XElement element, string name)
        {
            return element == null ? null : element.Elements().SingleOrDefault(x => x.Name.LocalName == name);
        }

        public static IEnumerable<XElement> ElementsOrEmpty(this XElement element, string name)
        {
            return element == null ? Enumerable.Empty<XElement>() : element.Elements().Where(x => x.Name.LocalName == name);
        }

        public static IEnumerable<XElement> AllElementsOrEmpty(this XElement thisXElement, IEnumerable<string> targets)
        {
            if (targets == null || !targets.Any()) return new List<XElement>();
            var foundElements = new List<XElement>();
            foreach (var target in targets)
            {
                foundElements.AddRange(thisXElement.ElementsOrEmpty(target).Where(e => e != null && !string.IsNullOrWhiteSpace(e.Value)));
            }
            foreach (var xElement in thisXElement.Descendants())
            {
                var foundInDescendants = xElement.AllElementsOrEmpty(targets);
                foreach (var element in foundInDescendants.Where(element => !foundElements.Contains(element)))
                {
                    foundElements.Add(element);
                }
            }
            return foundElements;
        } 

        public static string ValueOf(this XElement element, string name)
        {
            return element.ElementOrEmpty(name).ValueOrDefault();
        }

        public static int IntValueOf(this XElement thisElement, string name)
        {
            var element = thisElement.ElementOrEmpty(name);

            var elementValue = (int?)element;
            return elementValue.HasValue ? elementValue.Value : default(int);
        }

        public static int? NullableIntValueOf(this XElement element, string name)
        { 
            return (int?)element.ElementOrEmpty(name);
        }

        public static short? Last4CharactersAsNullableShort(this XElement thisXElement)
        {
            if (thisXElement == null) return null;

            var strValue = thisXElement.Value;
            if (string.IsNullOrWhiteSpace(strValue) || strValue.Length <= 4) 
                return string.IsNullOrWhiteSpace(strValue) ? (short?) null : short.Parse(strValue);
            var last4 = strValue.Substring(strValue.IndexOf("-") + 1, 4);
            return short.Parse(last4);
        }

        public static T ValueOrDefault<T>(this XElement thisXElement)
        {
            if (thisXElement == null || string.IsNullOrWhiteSpace(thisXElement.Value)) return default(T);
            var type = typeof (T);
            return (T) Convert.ChangeType(thisXElement.Value, Nullable.GetUnderlyingType(type) ?? type);
        }

        public static short ShortValueOf(this XElement thisElement, string name)
        {
            var element = thisElement.ElementOrEmpty(name);

            short? elementValue = null;
            if (name.Equals("SchoolYear"))
            {
                elementValue = element.Last4CharactersAsNullableShort();
            }
            else
            {
                elementValue = (short?)element;
            }
            return elementValue.HasValue ? elementValue.Value : default(short);
        }

        public static short? NullableShortValueOf(this XElement element, string name)
        {
            return (short?)element.ElementOrEmpty(name);
        }

        public static bool? NullableBoolValueOf(this XElement element, string name)
        {
            return (bool?)element.ElementOrEmpty(name);
        }

        public static bool BoolValueOf(this XElement element, string name)
        {
            return (bool)element.ElementOrEmpty(name);
        }

        public static DateTime? NullableDateValueOf(this XElement element, string name)
        {
            return (DateTime?)element.ElementOrEmpty(name);
        }

        public static DateTime DateValueOf(this XElement element, string name)
        {
            return (DateTime)element.ElementOrEmpty(name);
        }

        public static string AttributeValue(this XElement element, XName name)
        {
            if (element == null)
                return null;

            return (string)element.Attribute(name);
        }

        public static decimal? AttributeNullableDecimalValue(this XElement element, string name)
        {
            if (element == null)
                return null;

            return (decimal?)element.Attribute(name);
        }

        public static decimal DecimalValueOf(this XElement thisXElement, string name)
        {
            var element = thisXElement.ElementOrEmpty(name);
            return element == null || string.IsNullOrWhiteSpace(element.Value) ? default(decimal) : (decimal)element;
        }

        public static decimal? NullableDecimalValueOf(this XElement thisXElement, string name)
        {
            var element = thisXElement.ElementOrEmpty(name);
            return element == null || string.IsNullOrWhiteSpace(element.Value) ? null : (decimal?)element;
        }

        public static TimeSpan TimeSpanValueOf(this XElement thisXElement, string name)
        {
            var element = thisXElement.ElementOrEmpty(name);
            return element == null || string.IsNullOrWhiteSpace(element.Value) ? default(TimeSpan) : XmlConvert.ToDateTime(element.Value, XmlDateTimeSerializationMode.Utc).TimeOfDay;
        }

        public static TimeSpan? NullableTimeSpanValueOf(this XElement thiXElement, string name)
        {
            var element = thiXElement.ElementOrEmpty(name);
            return element == null || string.IsNullOrWhiteSpace(element.Value)
                ? (TimeSpan?) null
                : XmlConvert.ToDateTime(element.Value, XmlDateTimeSerializationMode.Utc).TimeOfDay;
        }
    }
}
