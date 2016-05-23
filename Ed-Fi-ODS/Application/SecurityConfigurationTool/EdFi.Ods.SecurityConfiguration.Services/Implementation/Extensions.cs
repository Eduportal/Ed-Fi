using System.Collections.Generic;
using System.Data.Entity;

namespace EdFi.Ods.SecurityConfiguration.Services.Implementation
{
    public static class R2VDbContextExtensionMethods
    {
        public static IEnumerable<TEntity> AddRange<TEntity>(this IDbSet<TEntity> dbset, IEnumerable<TEntity> entitiesToAdd) where TEntity : class
        {
            return ((DbSet<TEntity>)dbset).AddRange(entitiesToAdd);
        }

        public static void RemoveRange<TEntity>(this IDbSet<TEntity> dbset, IEnumerable<TEntity> entitiesToDelete) where TEntity : class
        {
            foreach (var entity in entitiesToDelete)
            {
                dbset.Remove(entity);
            }
        }
    }
}