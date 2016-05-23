using System;

namespace EdFi.Ods.Common
{
    public interface IUploadFileSourcingResults : IDisposable
    {
        string FilePathIfValid { get; }
        bool IsFailure { get; }
        string[] ValidationErrorMessages { get; } 
    }
}