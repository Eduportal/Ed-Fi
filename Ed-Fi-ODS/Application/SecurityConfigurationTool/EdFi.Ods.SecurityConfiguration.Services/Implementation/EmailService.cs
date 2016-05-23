using System.Net.Mail;
using System.Text;
using EdFi.Ods.SecurityConfiguration.Services.Model;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfigurationService _configurationService;

        public EmailService(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
        }

        public void SendActivationRequestEmail(EmailParameters parameters)
        {
            var smtpClient = new SmtpClient();

            var message = new MailMessage
            {
                Subject = "Ed-Fi API Key and Secret",
                Body = CreateBody(parameters),
            };

            message.To.Add(new MailAddress(parameters.RecipientEmailAddress));

            smtpClient.Send(message);

        }

        private string CreateBody(EmailParameters parameters)
        {
            var siteAddress = _configurationService.GetKeyRetrievalSite();
            siteAddress = siteAddress.TrimEnd('/');

            var template = new StringBuilder();

            template.AppendFormat(
                "An Ed-Fi ODS API Key and Secret has been created for {0}. This email has been sent to retrieve the key and secret associated with the below list of Education Organizations.",
                parameters.ApplicationName);

            template.AppendLine();
            template.AppendLine();

            foreach (var edOrg in parameters.EducationOrganization)
            {
                template.AppendFormat("- {0}", edOrg);
                template.AppendLine();
                template.AppendLine();
            }
            template.AppendLine();

            template.AppendLine("Please note that you must have the activation code in order to access the key and secret. ");
            template.AppendLine();
            template.AppendFormat("Click here: {0}/#/activate/{1}", siteAddress, parameters.ChallengeId);
            template.AppendLine();
            template.AppendLine();

            return template.ToString();
        }
    }
}