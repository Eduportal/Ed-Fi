using System;
using System.Globalization;
using EdFi.Ods.Common;
using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Entities.Common
{
    // TODO: Revisit
    public class ETagProvider : IETagProvider
    {
        public string GetETag(object entity)
        {
            if (entity == null)
                return null;

            var versionEntity = entity as IDateVersionedEntity;

            // Handle entities
            if (versionEntity != null)
            {
                if (versionEntity.LastModifiedDate == default(DateTime))
                    return null;

                return versionEntity.LastModifiedDate.ToBinary().ToString(CultureInfo.InvariantCulture);
            }

            // Handle resources
            var resourceEntity = entity as IHasETag;

            if (resourceEntity != null)
            {
                return resourceEntity.ETag;
            }

            // Handle date values
            var dateValue = entity is DateTime ? (DateTime)entity : new DateTime();

            if (dateValue != default(DateTime))
            {
                return dateValue.ToBinary().ToString(CultureInfo.InvariantCulture);
            }

            // Handle guids
            var guidValue = entity is Guid ? (Guid) entity : Guid.Empty;

            if (guidValue != default(Guid))
            {
                return guidValue.ToString("N");
            }

            // Handle strings
            var stringValue = entity is String ? (String)entity : string.Empty;

            if (!string.IsNullOrEmpty(stringValue))
            {
                return stringValue;
            }

            throw new Exception(string.Format("Unsupported type for ETag determination: '{0}'", entity.GetType().Name));
        }

        public DateTime GetDateTime(string etag)
        {
            long result;

            if (!string.IsNullOrWhiteSpace(etag) && long.TryParse(etag, out result))
            {
                try
                {
                    return DateTime.FromBinary(result);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Invalid ETag value.", ex);
                }
            }

            return default(DateTime);
        }
    }
}
