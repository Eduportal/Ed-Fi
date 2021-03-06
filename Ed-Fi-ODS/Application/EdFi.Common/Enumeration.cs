﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace EdFi.Common
{
    public interface IEnumeration
    {
    }

    public interface IEnumeration<TId> : IEnumeration
    {
        TId Id { get; }
    }

    public abstract class Enumeration<TEnum, TId> : IEnumeration<TId> where TEnum : Enumeration<TEnum, TId>
    {
        public abstract TId Id { get; }
        private static readonly object _valueLock = new object();
        private static readonly Dictionary<Type, object> _values = new Dictionary<Type, object>();

        #region EqualityMembers

        protected bool Equals(Enumeration<TEnum, TId> other)
        {
            return EqualityComparer<TId>.Default.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Enumeration<TEnum, TId>)obj);
        }

        public override int GetHashCode()
        {
            return EqualityComparer<TId>.Default.GetHashCode(Id);
        }

        #endregion

        public static TEnumeration[] GetValues<TEnumeration>()
        {
            Type type = typeof(TEnumeration);
            lock (_valueLock)
            {
                if (!_values.ContainsKey(type))
                {
                    _values[type] = type
                        .GetFields(BindingFlags.Static | BindingFlags.Public)
                        .Where(x => x.FieldType == type)
                        .Select(x => x.GetValue(null))
                        .Cast<TEnumeration>()
                        .ToArray();
                }
            }
            return (TEnumeration[])_values[type];
        }

        public static TEnum[] GetValues()
        {
            return GetValues<TEnum>();
        }

        public static TEnum GetById(TId id)
        {
            var values = GetValues().Where(x => Equals(id, x.Id)).ToArray();
            if (values.Length == 0)
                return null;
            if (values.Length > 1)
            {
                string message = string.Format("Found multiple values with the id {0}", id);
                throw new Exception(message);
            }
            return values[0];
        }
    }
}