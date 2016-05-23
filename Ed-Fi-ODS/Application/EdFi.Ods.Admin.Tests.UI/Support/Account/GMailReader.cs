namespace EdFi.Ods.Admin.UITests.Support.Account
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading;

    using AE.Net.Mail;

    public class GMailReader : IEmailReader
    {
        public void ClearMailbox(string email)
        {
            using (var client = new ImapClient("imap.gmail.com", email, "***REMOVEDBROKEN***", AuthMethods.Login , 993, true))
            {
                var messageCount = client.GetMessageCount();
                if (messageCount > 0)
                {
                    var messages = client.GetMessages(0, messageCount - 1);
                    foreach (var mailMessage in messages)
                    {
                        client.DeleteMessage(mailMessage);
                    }
                }
                client.Disconnect();
            }
        }

        public string FindLink(string email)
        {
            var link = string.Empty;
            using (
                var client = new ImapClient("imap.gmail.com", email, "***REMOVEDBROKEN***", AuthMethods.Login, 993,
                    true))
            {
                var waitStart = DateTime.Now;
                while (true)
                {
                    client.SelectMailbox("INBOX");
                    var messageCount = client.GetMessageCount();
                    if (messageCount > 0)
                    {
                        var messages = client.GetMessages(0, messageCount - 1, false);
                        var lastestMessage = messages[messageCount - 1];
                        link = this.FindLinkStringInMessage(lastestMessage, email);
                        foreach (var mailMessage in messages) 
                        {
                            client.DeleteMessage(mailMessage);
                        }
 
                        if(!string.IsNullOrWhiteSpace(link)) break;
                    }

                     // Check for timeout
                    var emailWaitTime = TimeSpan.FromMinutes(5); // TODO: Make configuration value for this?
                    if (DateTime.Now - waitStart > emailWaitTime)
                        throw new TimeoutException(
                            string.Format(
                                "Timed out waiting for email to arrive at gmail. Timeout occurred after {0} seconds.",
                                emailWaitTime.TotalSeconds));

                    // If we're still here, wait a bit and try again
                    Thread.Sleep(TimeSpan.FromSeconds(15));
                }
               client.Disconnect();
            }

            return link;
        }

        private string FindLinkStringInMessage(MailMessage message, string email)
        {
            var linkServerBaseUrl = TestSessionContext.Current.Configuration.ServerBaseUrl;

            if (message.Body == null) return string.Empty;

            var pattern = string.Format(@"({0}[^\s]*)", linkServerBaseUrl);

            var match = Regex.Match(message.Body, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase);

           return match.Success ? match.Groups[1].Value : string.Empty;
        }
    }
}