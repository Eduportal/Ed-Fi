using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;

namespace EdFi.Ods.IntegrationTests._Extensions
{
    public static class DbContextExtensions
    {
         public static void SaveChangesForTest(this DbContext context)
         {
             CatchAndWriteDbValidationExceptions(() => context.SaveChanges());
         }

        public static void CatchAndWriteDbValidationExceptions(Action action)
        {
            try
            {
                action();
            }
            catch (DbEntityValidationException exception)
            {
                var errorTexts = exception.EntityValidationErrors.SelectMany(x => x.ValidationErrors).Select(x => x.ErrorMessage);
                foreach (var error in errorTexts)
                {
                    Console.WriteLine(error);
                }
                throw;
            }
        }
    }
}