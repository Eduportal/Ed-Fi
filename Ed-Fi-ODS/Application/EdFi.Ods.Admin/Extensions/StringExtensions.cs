using System;
using System.Net.Mail;

namespace EdFi.Ods.Admin.Extensions
{
    public static class StringExtensions
    {
        public static bool IsValidEmailAddress(this string address)
        {
            try
            {
                var ma = new MailAddress(address);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}