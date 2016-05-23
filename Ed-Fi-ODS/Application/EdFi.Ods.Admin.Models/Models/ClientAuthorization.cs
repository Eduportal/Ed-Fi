using System;

namespace EdFi.Ods.Admin.Models
{

    public class ClientAuthorizationCode
    {
        private const double MinutesToExpire = 10;

        public ClientAuthorizationCode()
        {
            Id = Guid.NewGuid();
            Expiration = DateTime.UtcNow.AddMinutes(MinutesToExpire);
        }

        public Guid Id { get; set; }

        public ApiClient ApiClient { get; set; }

        public DateTime Expiration { get; set; }

        public string Scope { get; set; }

        public override string ToString()
        {
            return Id.ToString("N");
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow.Subtract(Expiration) > TimeSpan.Zero;
        }
    }

    public class ClientAccessToken
    {
        private const double MinutesToExpire = 10;

        public Guid Id { get; set; }

        public ApiClient ApiClient { get; set; }

        public DateTime Expiration { get; set; }

        public string Scope { get; set; }

        public ClientAccessToken()
        {
            Id = Guid.NewGuid();
            Expiration = DateTime.UtcNow.AddMinutes(MinutesToExpire);
        }

        internal void ResetExpiration()
        {
            Expiration = DateTime.UtcNow.AddMinutes(MinutesToExpire);
        }

        public override string ToString()
        {
            return Id.ToString("N");
        }

        public bool IsExpired()
        {
            return DateTime.UtcNow.Subtract(Expiration) > TimeSpan.Zero;
        }
    }
}
