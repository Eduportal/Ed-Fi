using System;

namespace EdFi.Ods.Common.ExceptionHandling
{
    /// <summary>
    /// Defines a method for translating internal Exceptions to public-facing error messages.
    /// </summary>
    public interface IExceptionTranslator
    {
        /// <summary>
        /// Attempts to translate the specified <see cref="Exception"/> to an error message that hides 
        /// internal details of the service implementation and is palatable for consumers of the API.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> to be translated.</param>
        /// <param name="webServiceError">The web service error response model.</param>
        /// <returns><b>true</b> if the exception was handled; otherwise <b>false</b>.</returns>
        bool TryTranslateMessage(Exception ex, out RESTError webServiceError);
    }
}