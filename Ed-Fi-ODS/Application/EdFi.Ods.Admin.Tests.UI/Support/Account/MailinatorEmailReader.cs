namespace EdFi.Ods.Admin.UITests.Support.Account
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading;

    using Newtonsoft.Json;

    public class MailinatorEmailReader : IEmailReader
    {
        // Track message Ids and cookie containers for an inbox across readers
        private static Dictionary<string, string> lastMessageIdByInbox = new Dictionary<string, string>();
        private static Dictionary<string, CookieContainer> cookieContainerByInbox = new Dictionary<string, CookieContainer>(); 

        private CookieContainer GetCookieContainer(string inbox)
        {
            CookieContainer cookieContainer;

            if (!cookieContainerByInbox.TryGetValue(inbox, out cookieContainer))
            {
                cookieContainer = new CookieContainer();
                cookieContainerByInbox[inbox] = cookieContainer;
            }

            return cookieContainer;
        }

        private void EnsureInboxInitialized(string email)
        {
            string inbox = GetInboxNameFromEmailAddress(email);

            if (lastMessageIdByInbox.ContainsKey(inbox))
                return;

            var cookieContainer = this.GetCookieContainer(inbox);

            // Use it
            var useItRequest = WebRequest.CreateHttp(
                string.Format("http://www.mailinator.com/useit?box={0}&time={1}", 
                inbox, this.GetUnixTime()));

            useItRequest.CookieContainer = cookieContainer;
            string json = this.GetResponseContent(useItRequest);

            var useIt = JsonConvert.DeserializeObject<UseItResponse>(json);

            // Grab the list of latest emails
            var grabRequest = WebRequest.CreateHttp(
                string.Format("http://www.mailinator.com/grab?inbox={0}&address={1}&time={2}", 
                useIt.inboxname, useIt.address, this.GetUnixTime()));

            grabRequest.CookieContainer = cookieContainer;
            json = this.GetResponseContent(grabRequest);

            var grab = JsonConvert.DeserializeObject<GrabResponse>(json);

            // Find last email sent more than 30 seconds ago (and ignore it and all previous emails)
            var latestEmail = grab.maildir.LastOrDefault(m => m.seconds_ago > 30);

            if (latestEmail != null)
                lastMessageIdByInbox[inbox] = latestEmail.id;
            else
                lastMessageIdByInbox[inbox] = null;
        }

        public string FindLink(string email)
        {
            if (email == null)
                throw new ArgumentNullException("email");

            this.EnsureInboxInitialized(email);

            string inboxName = GetInboxNameFromEmailAddress(email);

            var cookieContainer = this.GetCookieContainer(inboxName);

            // Use it
            var useItRequest = WebRequest.CreateHttp(string.Format("http://www.mailinator.com/useit?box={0}&time={1}", inboxName, this.GetUnixTime()));
            useItRequest.CookieContainer = cookieContainer;
            string json = this.GetResponseContent(useItRequest);

            var useIt = JsonConvert.DeserializeObject<UseItResponse>(json);

            var latestEmail = this.GetLatestEmail(useIt, inboxName);

            // Request rendering of the email
            string messageId = latestEmail.id;
            var renderRequest = WebRequest.CreateHttp(string.Format("http://www.mailinator.com/rendermail.jsp?msgid={0}&time={1}", messageId, this.GetUnixTime()));
            renderRequest.CookieContainer = cookieContainer;
            string html = this.GetResponseContent(renderRequest);

            // Extract the email body
            var emailBodyRegex = new Regex(@"<div class='mailview'[^>]*>(?<Body>.*?)</div>\s*</div>\s*</div>", RegexOptions.Singleline | RegexOptions.Multiline);

            var bodyMatch = emailBodyRegex.Match(html);

            if (bodyMatch.Success)
            {
                string cleanContents = bodyMatch.Value;

                string pattern = @"<a href='(http[s]?://[^']*)'";
                var match = Regex.Match(cleanContents, pattern, RegexOptions.Multiline);

                if (!match.Success)
                    throw new Exception(string.Format("Unable to find the activation link in this email (Inbox: {0}, MessageId: {1}):\r\n{2}", inboxName, messageId, cleanContents));

                // Make note of the email just read, so we don't get it again
                lastMessageIdByInbox[inboxName] = messageId;

                return match.Groups[1].Value;
            }

            throw new Exception(string.Format("Unable to extract body from rendered email  (Inbox: {0}, MessageId: {1}):\r\n{2}", inboxName, messageId, html));
        }

        public void ClearMailbox(string email)
        {
        }

        private MailItem GetLatestEmail(UseItResponse useIt, string inboxName)
        {
            string json;
            MailItem latestEmail;
            DateTime waitStart = DateTime.Now;

            var cookieContainer = this.GetCookieContainer(inboxName);

            while (true)
            {
                // Grab the list of latest emails
                var grabRequest = WebRequest.CreateHttp(
                    string.Format("http://www.mailinator.com/grab?inbox={0}&address={1}&time={2}{3}",
                                  useIt.inboxname, useIt.address, this.GetUnixTime(),
                                  lastMessageIdByInbox[inboxName] == null
                                      ? string.Empty
                                      : "&after=" + lastMessageIdByInbox[inboxName]));

                grabRequest.CookieContainer = cookieContainer;
                json = this.GetResponseContent(grabRequest);

                var grab = JsonConvert.DeserializeObject<GrabResponse>(json);

                // Find latest email
                latestEmail = grab.maildir.LastOrDefault();

                // If an email was found quit looking now
                if (latestEmail != null)
                {
                    Console.WriteLine("New email found at Mailinator.com after {0} seconds.", (DateTime.Now - waitStart).TotalSeconds);
                    break;
                }

                // Check for timeout
                var emailWaitTime = TimeSpan.FromMinutes(2); // TODO: Make configuration value for this?
                if (DateTime.Now - waitStart > emailWaitTime)
                    throw new TimeoutException(
                        string.Format(
                            "Timed out waiting for email to arrive at Mailinator.com. Timeout occurred after {0} seconds.",
                            emailWaitTime.TotalSeconds));

                // If we're still here, wait a bit and try again
                Thread.Sleep(TimeSpan.FromSeconds(15));
            }
            return latestEmail;
        }

        private static string GetInboxNameFromEmailAddress(string email)
        {
            string inbox = email.Split('@')[0];
            return inbox;
        }

        private string GetResponseContent(HttpWebRequest request)
        {
            var response = request.GetResponse();
            var sr = new StreamReader(response.GetResponseStream());
            string content = sr.ReadToEnd();

            return content;
        }

        private long GetUnixTime()
        {
            //create Timespan by subtracting the value provided from the Unix Epoch
            var span = (DateTime.Now - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime());

            //return the total seconds (which is a UNIX timestamp)
            return (long)span.TotalMilliseconds;
        }

        // Mailinator models for deserialization
        class UseItResponse
        {
            public string address { get; set; }
            public string alternatename { get; set; }
            public string inboxname { get; set; }
        }

        class GrabResponse
        {
            public MailItem[] maildir { get; set; }
        }

        class MailItem
        {
            public bool been_read { get; set; }
            public string from { get; set; }
            public string fromfull { get; set; }
            public string id { get; set; }
            public string ip { get; set; }
            public int seconds_ago { get; set; }
            public string snippet { get; set; }
            public string state { get; set; }
            public string subject { get; set; }
            public long time { get; set; }
            public string to { get; set; }
        }
    }
}