// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using EdFi.TestObjects;

namespace EdFi.Ods.Utilities.LoadGeneration.ReferenceValueBuilders
{
    /// <summary>
    /// Provides a marker interface for distinguishing between a general-purpose
    /// value builder and the numerous value builder implementations that will 
    /// exist to handle building references where key unification is occuring
    /// in the data model.
    /// </summary>
    public interface IContextSpecificReferenceValueBuilder : IValueBuilder { }
}