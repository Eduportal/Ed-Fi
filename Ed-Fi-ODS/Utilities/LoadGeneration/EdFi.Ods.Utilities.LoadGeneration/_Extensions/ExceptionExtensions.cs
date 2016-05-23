using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdFi.Ods.Utilities.LoadGeneration._Extensions
{
    public static class ExceptionExtensions
    {
        public static IEnumerable<Exception> GetInnerExceptions(this Exception ex)
        {
            Exception innerEx = ex.InnerException;

            while (innerEx != null)
            {
                yield return innerEx;
                innerEx = innerEx.InnerException;
            }
        }
    }
}
