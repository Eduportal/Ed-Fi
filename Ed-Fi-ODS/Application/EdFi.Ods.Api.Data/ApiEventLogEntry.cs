using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using EdFi.Ods.Api.Common;

namespace EdFi.Ods.Api.Data
{
    public class ApiEventLogEntry
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [Required]
        public string Event { get; set; }
        public string HttpMethod { get; set; }
        public string Uri { get; set; }
        public string Message { get; set; }
        [Required]
        public string AggregateName { get; set; }
        [Required]
        public string AggregateKey { get; set; }
        [Required]
        public string ApplicationKey { get; set; }
        [Required]
        public string ETag { get; set; }
        [Required]
        public DateTimeOffset Timestamp { get; set; }
    }
}
