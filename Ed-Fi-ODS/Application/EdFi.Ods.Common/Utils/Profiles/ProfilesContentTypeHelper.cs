// ****************************************************************************
//  Copyright (C) 2015 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Text.RegularExpressions;
using EdFi.Common.Inflection;

namespace EdFi.Ods.Common.Utils.Profiles
{
    public static class ProfilesContentTypeHelper
    {
        public static string CreateContentType(
            string resourceCollectionName,
            string profileName,
            ContentTypeUsage usage)
        {
            return string.Format(
                "application/vnd.ed-fi{0}.{1}.{2}.{3}+json",
                string.Empty,
                CompositeTermInflector.MakeSingular(resourceCollectionName).ToLower(),
                profileName.ToLower(),
                usage.ToString().ToLower());
        }

        private static readonly Regex ProfileRegex = new Regex(@"^application/vnd\.ed-fi(\.(?<Implementation>[\w\-]+))?\.(?<Resource>\w+)\.(?<Profile>[\w\-]+)\.(?<Usage>(readable|writable))\+json$", RegexOptions.Compiled);

        public static ProfileContentTypeDetails GetContentTypeDetails(this string contentType)
        {
            ProfileContentTypeDetails details;

            if (TryGetContentTypeDetails(contentType, out details))
                return details;
            else
                return null;
        }

        public static bool IsEdFiContentType(this string contentType)
        {
            if (contentType.StartsWith("application/vnd.ed-fi."))
            {
                return true;
            }

            return false;
        }

        public static bool TryGetContentTypeDetails(this string contentType, out ProfileContentTypeDetails details)
        {
            details = null;

            var match = ProfileRegex.Match(contentType);

            if (!match.Success)
                return false;

            details = new ProfileContentTypeDetails
            {
                Implementation = match.Groups["Implementation"].Value,
                Resource = match.Groups["Resource"].Value,
                Profile = match.Groups["Profile"].Value,
                Usage =
                    (ContentTypeUsage) Enum.Parse(typeof(ContentTypeUsage), match.Groups["Usage"].Value, true),
            };

            return true;
        }
    }
}