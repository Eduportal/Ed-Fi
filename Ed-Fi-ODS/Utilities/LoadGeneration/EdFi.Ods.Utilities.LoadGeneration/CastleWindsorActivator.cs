// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using EdFi.Common.InversionOfControl;
using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public class CastleWindsorActivator : IActivator
    {
        public T CreateInstance<T>()
        {
            return IoC.Resolve<T>();
        }

        public object CreateInstance(Type type)
        {
            return IoC.Resolve(type);
        }
    }
}