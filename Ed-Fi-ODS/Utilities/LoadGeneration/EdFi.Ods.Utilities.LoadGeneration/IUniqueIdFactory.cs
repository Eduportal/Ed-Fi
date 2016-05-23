// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************
using System;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    /// <summary>
    /// Defines a method that creates a new UniqueId using the supplied identity information.
    /// </summary>
    public interface IUniqueIdFactory
    {
        /// <summary>
        /// Creates a new UniqueId using the supplied identity information.
        /// </summary>
        /// <param name="firstName">The new identity's first name.</param>
        /// <param name="lastSurname">The new identity's last name.</param>
        /// <param name="birthDate">The new identity's date of birth.</param>
        /// <param name="sexType">The new identity's gender.</param>
        /// <returns>The newly created UniqueId value.</returns>
        string CreateUniqueId(string firstName, string lastSurname, DateTime? birthDate, string sexType);
    }
}