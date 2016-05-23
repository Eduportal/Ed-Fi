namespace EdFi.Ods.Common
{
    public interface IValidateAndSourceFiles
    {
        IUploadFileSourcingResults ValidateMakeLocalAndFindPath(string operationId, string uploadFileId);
    }
}