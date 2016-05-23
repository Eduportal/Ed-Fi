namespace EdFi.Ods.Admin.UITests.Support.Account
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class FileSystemEmailReader : IEmailReader
    {
        private readonly TestConfiguration _configuration;

        public FileSystemEmailReader(TestSessionContext context)
        {
            this._configuration = context.Configuration;
        }

        private string FindLatestEmailTo(string emailAddress)
        {
            var emailFiles = new DirectoryInfo(this._configuration.LocalEmailDirectory)
                .GetFileSystemInfos("*.eml")
                .OrderByDescending(x => x.CreationTime)
                .ToArray();

            foreach (var file in emailFiles)
            {
                var contents = File.ReadAllText(file.FullName);
                var isMatch = contents.Contains(string.Format("To: {0}", emailAddress));
                if (isMatch)
                {
                    return contents;
                }
            }

            return string.Empty;
        }

        public string FindLink(string email)
        {
            var emailContents = this.FindLatestEmailTo(email);
            var cleanContents = emailContents.Replace("=\r\n", string.Empty).Replace("=0D=0A", " ").Replace("=3D", "=");
            
            string pattern = @"(http[s]?://[^\s]*)";
            var match = Regex.Match(cleanContents, pattern, RegexOptions.Multiline);

            if (!match.Success)
                throw new Exception(string.Format("Unable to find the activation link in this email:\n{0}", cleanContents));

            return match.Groups[1].Value;
        }

        public void ClearMailbox(string email)
        {
        }
    }
}