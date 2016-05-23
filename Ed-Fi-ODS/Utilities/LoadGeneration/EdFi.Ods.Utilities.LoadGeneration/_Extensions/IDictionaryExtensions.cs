// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Castle.Components.DictionaryAdapter;

namespace EdFi.Ods.Utilities.LoadGeneration._Extensions
{
    public static class IDictionaryExtensions
    {
        public static string ToDebugString<TKey, TValue>(this IDictionary<TKey, TValue> source)
        {
            return string.Join(", ", source.Select(x => string.Format("[{0}={1}]", x.Key, x.Value)));
        }
    }
}