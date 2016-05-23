using System.Collections.Generic;
using System.Linq;
using EdFi.Ods.Common;

namespace EdFi.Ods.BulkLoad.Core
{
    public class SequencedInterchangeComparer : Comparer<InterchangeType>
    {
        public override int Compare(InterchangeType interchange1, InterchangeType interchange2)
        {
            var typeList = InterchangeType.RequiredLoadOrder.ToList();
            return typeList.IndexOf(interchange1) - typeList.IndexOf(interchange2);
        }
    }
}