using System;
using System.Linq.Expressions;
using EdFi.Ods.Admin.Extensions;
using EdFi.Ods.Admin.Models;
using EdFi.Ods.Admin.Models.Results;
using EdFi.Ods.Admin.Security;
using EdFi.Ods.Common.Utils.Extensions;
using WebMatrix.WebData;
using log4net;

namespace EdFi.Ods.Admin.Services
{
    public class UserAccountManager : IUserAccountManager
    {
        private readonly ILog _log = LogManager.GetLogger(typeof (UserAccountManager));

        private const int UsernameStrength = 15;

        private readonly IClientAppRepo _clientAppRepository;
        private readonly IEmailService _emailService;
        private readonly IPasswordService _passwordService;
        private readonly ISecurityService _securityService;

        public UserAccountManager(IClientAppRepo clientAppRepository, IEmailService emailService, IPasswordService passwordService, ISecurityService securityService)
        {
            _clientAppRepository = clientAppRepository;
            _emailService = emailService;
            _passwordService = passwordService;
            _securityService = securityService;
        }

        public CreateLoginResult Create(CreateLoginModel model)
        {
            var validationResult = ValidateCreateModel(model);
            if (validationResult != null)
                return validationResult;

            if (!WebSecurity.UserExists(model.Email))
                return CreateNewUser(model);

            if (WebSecurity.IsConfirmed(model.Email))
            {
                return new CreateLoginResult
                           {
                               Message = "User already exists with this email address.",
                               UserStatus = UserStatus.AlreadyExists
                           }
                    .AddFailingField(x => x.Email);
            }
            return new CreateLoginResult
                       {
                           Message = "A user already exists with this email address, but email address has not been confirmed.\n\n" +
                                     "Use the link below to send the confirmation link again.",
                           UserStatus = UserStatus.NeedsEmailConfirmation
                       }
                .AddFailingField(x => x.Email);
        }

        private CreateLoginResult CreateNewUser(CreateLoginModel model)
        {
            var randomPassword = Guid.NewGuid().ToString();

            try
            {
                string confirmationSecret = WebSecurity.CreateUserAndAccount(
                    model.Email,
                    randomPassword,
                    new {FullName = model.Name},
                    true);

                _clientAppRepository.SetDefaultVendorOnUserFromEmailAndName(model.Email, model.Name);

                _emailService.SendConfirmationEmail(model.Email, confirmationSecret);
                return new CreateLoginResult
                           {
                               Success = true,
                               UserStatus = UserStatus.Created,
                           };
            }
            catch (Exception e)
            {
                _log.Error("CreateNewUser", e);
                var message = string.Format("An exception was thrown when attempting to create the user:\n{0}", e.Message)
                                    .Replace("\r\n", "\n").Replace("\n", "<br/>");
                return CreateLoginResult.Fail.WithMessage(message);
            }
        }


        private TResult ValidateRequired<TResult, TModel>(Expression<Func<TModel, string>> propertyExpression, TModel model)
            where TResult : AdminActionResult<TResult, TModel>
        {
            var func = propertyExpression.Compile();
            var value = func(model);
            if (string.IsNullOrWhiteSpace(value))
            {
                var propName = propertyExpression.MemberName();
                var result = Activator.CreateInstance<TResult>();
                result.Message = string.Format("{0} is required.", propName);
                result.Success = false;
                result.AddFailingField(propertyExpression);
                return result;
            }
            return null;
        }

        private CreateLoginResult ValidateCreateModel(CreateLoginModel model)
        {
            var failedRequiredField =
                ValidateRequired<CreateLoginResult, CreateLoginModel>(x => x.Email, model) ??
                ValidateRequired<CreateLoginResult, CreateLoginModel>(x => x.Name, model);

            if (failedRequiredField != null)
            {
                failedRequiredField.UserStatus = UserStatus.Failed;
                return failedRequiredField;
            }

            if (!model.Email.IsValidEmailAddress())
                return CreateLoginResult
                    .Fail
                    .WithMessage(string.Format("'{0}' is not a valid email address", model.Email))
                    .AddFailingField(x => x.Email);

            return null;
        }

        private PasswordResetResult ValidateResetModel(PasswordResetModel model)
        {
            var failedRequiredField =
                ValidateRequired<PasswordResetResult, PasswordResetModel>(x => x.Password, model) ??
                ValidateRequired<PasswordResetResult, PasswordResetModel>(x => x.ConfirmPassword, model);

            if (failedRequiredField != null)
                return failedRequiredField;

            var confirmationMismatch = !model.Password.Equals(model.ConfirmPassword);
            if (confirmationMismatch)
            {
                var mismatchResult = new PasswordResetResult
                                         {
                                             Message = "Password and Confirm Password must match.",
                                             Success = false,
                                         };
                mismatchResult.AddFailingField(x => x.Password).AddFailingField(x => x.ConfirmPassword);

                return mismatchResult;
            }

            if (!_passwordService.PasswordIsStrong(model.Password))
                return PasswordResetResult.BadPassword;

            var validateResetRequest = _passwordService.ValidateRequest(model.UserName, model.Marker);
            if (validateResetRequest.Success)
                return null;
            return validateResetRequest;
        }

        public PasswordResetResult ResetPassword(PasswordResetModel model)
        {
            try
            {
                var validationResult = ValidateResetModel(model);
                if (validationResult != null)
                    return validationResult;

                WebSecurityService.UpdatePasswordAndActivate(model.UserName, model.Password);

                return PasswordResetResult.Successful;
            }
            catch (Exception e)
            {
                _log.Error("ResetPassword", e);
                return DatabaseUnreachableResult<PasswordResetResult>();
            }
        }

        private ChangePasswordResult ValidateChangeModel(ChangePasswordModel model)
        {
            var failedRequiredField =
                ValidateRequired<ChangePasswordResult, ChangePasswordModel>(x => x.CurrentPassword, model) ??
                ValidateRequired<ChangePasswordResult, ChangePasswordModel>(x => x.NewPassword, model) ??
                ValidateRequired<ChangePasswordResult, ChangePasswordModel>(x => x.ConfirmPassword, model);

            if (failedRequiredField != null)
                return failedRequiredField;

            var confirmationMismatch = !model.NewPassword.Equals(model.ConfirmPassword);
            if (confirmationMismatch)
            {
                var mismatchResult = new ChangePasswordResult
                                         {
                                             Message = "New Password and Confirm Password must match.",
                                             Success = false,
                                         };
                mismatchResult.AddFailingField(x => x.NewPassword).AddFailingField(x => x.ConfirmPassword);

                return mismatchResult;
            }

            if (!_passwordService.PasswordIsStrong(model.NewPassword))
                return new ChangePasswordResult {Success = false, Message = "NewPassword is invalid"}
                    .AddFailingField(x => x.NewPassword);

            return null;
        }

        public ChangePasswordResult ChangePassword(ChangePasswordModel model)
        {
            var validationResult = ValidateChangeModel(model);
            if (validationResult != null)
                return validationResult;

            try
            {
                var currentPasswordOkay = WebSecurity.Login(model.UserName, model.CurrentPassword, false);
                if (!currentPasswordOkay)
                {
                    var badPasswordResult = new ChangePasswordResult
                                                {
                                                    Success = false,
                                                    Message = "The Current Password supplied is incorrect"
                                                };
                    badPasswordResult.AddFailingField(x => x.CurrentPassword);

                    return badPasswordResult;
                }
                WebSecurityService.UpdatePasswordAndActivate(model.UserName, model.NewPassword);
                return ChangePasswordResult.Successful;
            }
            catch (Exception e)
            {
                _log.Error("ChangePassword", e);
                return DatabaseUnreachableResult<ChangePasswordResult>();
            }
        }

        private ForgotPasswordResetResult ValidateForgotModel(ForgotPasswordModel model)
        {
            var failedRequiredField =
                ValidateRequired<ForgotPasswordResetResult, ForgotPasswordModel>(x => x.Email, model);

            if (failedRequiredField != null)
                return failedRequiredField;

            var userByEmail = _clientAppRepository.GetUser(model.Email);
            if (userByEmail == null)
            {
                return new ForgotPasswordResetResult
                           {
                               Success = false,
                               Message = string.Format("There are no accounts with the email address '{0}'", model.Email)
                           }.AddFailingField(x => x.Email);
            }

            return null;
        }

        public ForgotPasswordResetResult ForgotPassword(ForgotPasswordModel model)
        {
            var userProfile = _clientAppRepository.GetUser(model.Email);
            if (userProfile == null)
                return ForgotPasswordResetResult.BadEmail(model.Email);

            return SendEmail(model, (email, confirmationSecret) => _emailService.SendForgotPasswordEmail(email, confirmationSecret));
        }

        public ForgotPasswordResetResult ResendConfirmation(ForgotPasswordModel model)
        {
            var userName = model.Email;
            var badUserName = !WebSecurity.UserExists(userName);
            if (badUserName)
                return ForgotPasswordResetResult.BadEmail(userName);

            var isConfirmed = WebSecurity.IsConfirmed(userName);
            if (isConfirmed)
            {
                var message = string.Format("The account with email address '{0}' has already been confirmed.  Use the password reset if the password has been lost.", userName);
                return new ForgotPasswordResetResult {Success = false, Message = message};
            }
            var secret = _clientAppRepository.GetTokenFromUserName(userName);

            try
            {
                _emailService.SendConfirmationEmail(userName, secret);
            }
            catch (Exception e)
            {
                _log.Error("SendEmail", e);
                return EmailDown<ForgotPasswordResetResult>();
            }

            return ForgotPasswordResetResult.Successful;
        }

        private ForgotPasswordResetResult SendEmail(ForgotPasswordModel model, Action<string, string> sendForgotPassword)
        {
            string secret;
            User user;
            try
            {
                var validationResult = ValidateForgotModel(model);
                if (validationResult != null)
                    return validationResult;
                user = _clientAppRepository.GetUser(model.Email);
                secret = _passwordService.SetPasswordResetSecret(model.Email);
            }
            catch (Exception e)
            {
                _log.Error("SendEmail", e);
                return DatabaseUnreachableResult<ForgotPasswordResetResult>();
            }

            try
            {
                sendForgotPassword(user.Email, secret);
            }
            catch (Exception e)
            {
                _log.Error("SendEmail", e);
                return EmailDown<ForgotPasswordResetResult>();
            }

            return ForgotPasswordResetResult.Successful;
        }

        private T DatabaseUnreachableResult<T>() where T : IAdminActionResult
        {
            var result = Activator.CreateInstance<T>();
            result.Success = false;
            result.Message = "The database is unreachable.  Please try again in a few minutes.";
            return result;
        }

        private static TResult EmailDown<TResult>() where TResult : IAdminActionResult
        {
            var result = Activator.CreateInstance<TResult>();
            result.Success = false;
            result.Message = "It looks like our email system is down.  Please try again in a few minutes.";
            return result;
        }
    }
}