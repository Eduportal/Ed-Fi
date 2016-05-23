using System.Linq;

namespace EdFi.Ods.Entities.Common.Validation 
{
    internal static class CrossSiteScriptingValidation
    {
        private static readonly char[] DangerousCharacters = {'<', '&'};
        private static readonly char[] DangerousSecondCharacters = { '!', '/', '?' };
        private static readonly string[] DangerousStrings =
        {
            "javascript:", "onafterprint", "onbeforeprint", "onbeforeunload", "onerror", "onhashchange", "onload", "onmessage", 
            "onoffline", "ononline", "onpagehide", "onpageshow", "onpopstate", "onresize", "onstorage", "onunload", "onblur", 
            "onchange", "oncontextmenu", "onfocus", "oninput", "oninvalid", "onreset", "onsearch", "onselect", "onsubmit", 
            "onkeydown", "onkeypress", "onkeyup", "onclick", "ondblclick", "ondrag", "ondragend", "ondragenter", "ondragleave", 
            "ondragover", "ondragstart", "ondrop", "onmousedown", "onmousemove", "onmouseout", "onmouseover", "onmouseup", 
            "onmousewheel", "onscroll", "onwheel", "oncopy", "oncut", "onpaste", "onabort", "oncanplay", "oncanplaythrough", 
            "oncuechange", "ondurationchange", "onemptied", "onended", "onerror", "onloadeddata", "onloadedmetadata", 
            "onloadstart", "onpause", "onplay", "onplaying", "onprogress", "onratechange", "onseeked", "onseeking", "onstalled",
            "onsuspend", "ontimeupdate", "onvolumechange", "onwaiting", "onerror", "onshow", "ontoggle", "seeksegmenttime"
        };

        /// <summary>
        /// This extends the standard functionality to include a few more XSS filters
        /// For an exhaustive list of XSS attacks visit https://www.owasp.org/index.php/XSS_Filter_Evasion_Cheat_Sheet
        /// </summary>
        /// <param name="s">The string to check for XSS attacks</param>
        /// <returns>Whether string contains any XSS vulnerabilities</returns>
        internal static bool IsCrossSiteScriptDanger(string s)
        {
            s = s.ToLower();
            return IsDangerousString(s) || DangerousStrings.Any(ds => s.Contains(ds));
        }

        /// <summary>
        /// Algorithmically similar method to System.Web.CrossSiteScriptingValidation.IsDangerousString
        /// </summary>
        /// <param name="s">string to validate</param>
        /// <returns>Whether the string contains '&#' or open bracket with certain following characters</returns>
        private static bool IsDangerousString(string s)
        {
            for (var i = 0;; i++)
            {
                i = s.IndexOfAny(DangerousCharacters, i);
                if (i == -1 || i == s.Length - 1) return false;
                if (s[i] == '&' && s[i + 1] == '#') return true;
                if (s[i] == '<' && (char.IsLetter(s[i+1]) || DangerousSecondCharacters.Any(c => c == s[i+1]))) return true;
            }
        }
    }
}