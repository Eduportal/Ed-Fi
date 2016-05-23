using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace EdFi.Ods.Admin.Security
{
    public class DlpProtectedSettings : ConfigurationSection
    {
        [ConfigurationProperty("sendGridCredentials")]
        public SendGridCredentials SendGridCredentials
        {
            get
            {
                return (SendGridCredentials)this["sendGridCredentials"];
            }
            set
            {
                this["sendGridCredentials"] = value;
            }
        }
    }

    public class SendGridCredentials: ConfigurationElement
    {
        [ConfigurationProperty("userName")]
        public String UserName
        {
            get
            {
                return (String)this["userName"];
            }
            set
            {
                this["userName"] = value;
            }
        }

        [ConfigurationProperty("password")]
        public String Password
        {
            get
            {
                return (String)this["password"];
            }
            set
            {
                this["password"] = value;
            }
        }
    }
}