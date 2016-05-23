using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using EdFi.Ods.Admin.Security;

namespace EdFi.Ods.Admin.Services
{
    public class EmailService : IEmailService
    {
        private readonly IRouteService _routeService;

        public EmailService(IRouteService routeService)
        {
            _routeService = routeService;
        }

        public void SendConfirmationEmail(string emailAddress, string secret)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(@"An account has been created for email address '" + emailAddress +
                                      "' in Sandbox Admin.");
            messageBuilder.AppendLine();
            messageBuilder.AppendLine(@"Please follow this link to set your password:");
            messageBuilder.AppendLine();
            messageBuilder.AppendLine(_routeService.GetRouteForActivation(secret));

            var body = string.Format(messageBuilder.ToString(), secret);

            var message = new MailMessage
                              {
                                  Subject = "Sandbox Account Activation",
                                  Body = body,
                              };
            message.To.Add(new MailAddress(emailAddress));

            GetSmtpClientWithEnvironmentVariableExpansion()
                .Send(message);
        }

        public void SendForgotPasswordEmail(string emailAddress, string secret)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(@"You have requested a password reset for your account in Sandbox Admin.");
            messageBuilder.AppendLine();
            messageBuilder.AppendLine(@"In order to reset your password, please follow this link:");
            messageBuilder.AppendLine(_routeService.GetRouteForPasswordReset(secret));

            var body = string.Format(messageBuilder.ToString(), secret);

            var message = new MailMessage
                              {
                                  Subject = "Sandbox Account Password Reset",
                                  Body = body,
                              };
            message.To.Add(new MailAddress(emailAddress));

            GetSmtpClientWithEnvironmentVariableExpansion()
                .Send(message);
        }

        private static SmtpClient GetSmtpClientWithEnvironmentVariableExpansion()
        {
            var smtpClient = new SmtpClient();
            var dlpSettings = ConfigurationManager.GetSection("DlpProtectedSettings") as DlpProtectedSettings;
            if (dlpSettings!=null)
                    smtpClient.Credentials = new NetworkCredential(dlpSettings.SendGridCredentials.UserName,dlpSettings.SendGridCredentials.Password);
            
            // Expand any embedded environment variables
            if (smtpClient.PickupDirectoryLocation != null)
            {
                smtpClient.PickupDirectoryLocation = Environment.ExpandEnvironmentVariables(smtpClient.PickupDirectoryLocation);

                // Try to make sure that the specified email directory exists
                try { Directory.CreateDirectory(smtpClient.PickupDirectoryLocation); }
                catch { }
            }

            return smtpClient;
        }
    }
}