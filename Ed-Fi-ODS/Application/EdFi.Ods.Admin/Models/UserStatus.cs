namespace EdFi.Ods.Admin.Models
{
    public enum UserStatus
    {
        Created,
        AlreadyExists,
        Deactivated,
        NeedsEmailConfirmation,
        Failed
    }
}