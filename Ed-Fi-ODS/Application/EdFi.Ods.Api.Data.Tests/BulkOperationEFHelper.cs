using System;
using System.Data.Entity.Validation;
using EdFi.Ods.Common;
using EdFi.Ods.Api.Data.Contexts;
using EdFi.Ods.Api.Models.Resources.Enums;

namespace EdFi.Ods.Api.Data.Tests
{
    public static class BulkOperationEfHelper
    {
        public static BulkOperation InsertSampleBulkOperation()
        {
            var bulkOperation = BuildBulkOperation();

            using (var context = new BulkOperationDbContext())
            {
                context.BulkOperations.Add(bulkOperation);
                try
                {
                    context.SaveChanges();
                }
                catch (DbEntityValidationException e)
                {
                    WriteEfValidationErrorsToConsole(e);

                    throw;
                }
            }

            return bulkOperation;
        }

        public static void WriteEfValidationErrorsToConsole(DbEntityValidationException e)
        {
            foreach (var eve in e.EntityValidationErrors)
            {
                Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                    eve.Entry.Entity.GetType().Name,
                    eve.Entry.State);
                foreach (var ve in eve.ValidationErrors)
                {
                    Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);
                }
            }
        }

        private static BulkOperation BuildBulkOperation()
        {
            var random = new Random();

            Func<int, byte[]> randomBytes = (count) =>
            {
                var bytes = new byte[count];
                random.NextBytes(bytes);
                return bytes;
            };

            var bulkOperation = new BulkOperation
            {
                Id = Guid.NewGuid().ToString(),
                DatabaseName = "My DB",
                ResetDistrictData = "Yes please!",
                Status = BulkOperationStatus.Incomplete,
                UploadFiles = new[]
                {
                    new UploadFile
                    {
                        Id = Guid.NewGuid().ToString(),
                        Format = InterchangeFileFormat.TextXml.Name,
                        InterchangeType = InterchangeType.EducationOrganization.Name,
                        Size = 100,
                        Status = UploadFileStatus.Initialized,
                        UploadFileChunks = new[]
                        {
                            new UploadFileChunk
                            {
                                Id = Guid.NewGuid().ToString(),
                                Offset = 0,
                                Size = 30,
                                Chunk = randomBytes(30)
                            },
                            new UploadFileChunk
                            {
                                Id = Guid.NewGuid().ToString(),
                                Offset = 30,
                                Size = 70,
                                Chunk = randomBytes(70)
                            }
                        }
                    },
                    new UploadFile
                    {
                        Id = Guid.NewGuid().ToString(),
                        Format = InterchangeFileFormat.ApplicationXml.Name,
                        InterchangeType = InterchangeType.Descriptors.Name,
                        Size = 200,
                        Status = UploadFileStatus.Initialized,
                    }
                }
            };
            return bulkOperation;
        }
    }
}