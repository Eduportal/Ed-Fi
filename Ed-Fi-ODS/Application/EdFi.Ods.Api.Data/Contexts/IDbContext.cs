using System;

namespace EdFi.Ods.Api.Data.Contexts
{
    public interface IDbContext : IDisposable
    {
        int SaveChanges();
    }
}