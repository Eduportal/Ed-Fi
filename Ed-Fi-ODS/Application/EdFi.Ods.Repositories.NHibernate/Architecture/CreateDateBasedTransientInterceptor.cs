using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate;

namespace EdFi.Ods.Entities.Repositories.NHibernate.Architecture
{
    public class CreateDateBasedTransientInterceptor : EmptyInterceptor
        {
            public override bool? IsTransient(object entity)
            {
                var property = entity.GetType().GetProperty("CreateDate");
    
                if (property != null)
                {
                    bool isTransient = Convert.ToDateTime(property.GetValue(entity)) == default(DateTime);
    
                    return isTransient;
                }
    
                return base.IsTransient(entity);
            }
        }
}
