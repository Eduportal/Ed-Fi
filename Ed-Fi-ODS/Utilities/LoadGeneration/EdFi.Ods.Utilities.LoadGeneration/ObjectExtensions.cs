// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public static class ObjectExtensions
    {
        public static int ToInt32(this object value)
        {
            return Convert.ToInt32(value);
        }
    }
}