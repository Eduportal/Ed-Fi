using System;

namespace EdFi.Ods.BulkLoad.Core
{
    public class LoadException
    {
        public string Element { get; set; }
        public Exception Exception { get; set; }

        public static LoadException ForEmptyInterchange(string interchangeType, string fileId)
        {
            var message = string.Format(
                @"No aggregates for Interchange {0} were found in file {1}.  Please confirm the declared Interchange Type ({0}) is valid.",
                interchangeType, fileId);
            return new LoadException{Element = string.Empty, Exception = new ArgumentException(message)};
        }
    }
}