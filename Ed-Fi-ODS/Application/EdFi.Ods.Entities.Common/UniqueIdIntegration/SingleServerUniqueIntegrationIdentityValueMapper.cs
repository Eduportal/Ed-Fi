using System;
using System.Collections.Concurrent;
using System.Data.SqlClient;
using EdFi.Common.Database;
using EdFi.Ods.Common.Specifications;
using EdFi.Ods.Entities.Common.IdentityValueMappers;

namespace EdFi.Ods.Entities.Common.UniqueIdIntegration
{
    /// <summary>
    /// Implements both identity value mappers (supporting USIs and Ids) in a single implementation 
    /// optimized for a single server deployment (where the ODS and UniqueId integration artifacts reside on 
    /// the same SQL Server instance).
    /// </summary>
    /// <remarks>
    /// The implementation assumes that the <b>UniqueIdIntegrationContext</b> database and the
    /// <b>EdFi_ODS</b> currently in context for the request are deployed to the <i>same</i> SQL Server instance
    /// and the ODS person tables are accessible to the credentials provided for <b>UniqueIdIntegrationContext</b>.
    /// 
    /// If the deployment environment will involve multiple servers for the ODS(s) and UniqueId integration
    /// databases, then use the <see cref="EdFiOdsBasedUsiValueMapper"/> in conjunction with
    /// the <see cref="UniqueIdIntegrationBasedIdValueMapper"/>.
    /// </remarks>
    public class SingleServerUniqueIntegrationIdentityValueMapper : IUniqueIdToIdValueMapper, IUniqueIdToUsiValueMapper
    {
        private readonly IDatabaseConnectionStringProvider _uniqueIdIntegrationDatabaseConnectionStringProvider;
        private readonly IDatabaseConnectionStringProvider _odsDatabaseConnectionStringProvider;

        public SingleServerUniqueIntegrationIdentityValueMapper(
            IDatabaseConnectionStringProvider uniqueIdIntegrationDatabaseConnectionStringProvider,
            IDatabaseConnectionStringProvider odsDatabaseConnectionStringProvider)
        {
            _uniqueIdIntegrationDatabaseConnectionStringProvider = uniqueIdIntegrationDatabaseConnectionStringProvider;
            _odsDatabaseConnectionStringProvider = odsDatabaseConnectionStringProvider;
        }

        public PersonIdentifiersValueMap GetId(string personType, string uniqueId)
        {
            return Get(personType, uniqueId);
        }

        public PersonIdentifiersValueMap GetUniqueId(string personType, Guid id)
        {
            return Get(personType, id);
        }

        public PersonIdentifiersValueMap GetUniqueId(string personType, int usi)
        {
            return Get(personType, usi);
        }

        public PersonIdentifiersValueMap GetUsi(string personType, string uniqueId)
        {
            return Get(personType, uniqueId);
        }

        private PersonIdentifiersValueMap Get(string personTypeName, string uniqueId)
        {
            var results = new PersonIdentifiersValueMap();

            if (!PersonEntitySpecification.IsPersonEntity(personTypeName))
                throw new ArgumentException(
                    string.Format("Invalid person type '{0}'. Valid person types are: {1}", personTypeName,
                        "'" + String.Join("','", PersonEntitySpecification.ValidPersonTypes) + "'"));

            string uniqueIdIntegrationConnectionString = _uniqueIdIntegrationDatabaseConnectionStringProvider.GetConnectionString();

            string uniqueIdIntegrationDatabaseName = GetUniqueIdIntegrationDatabaseName(uniqueIdIntegrationConnectionString);

            using (var connection = new SqlConnection(uniqueIdIntegrationConnectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText =
                        string.Format(@"select mapping.Id as Id, person.{2}USI as Usi, person.Id as Id, mapping.UniqueId as UniqueId
                                            from {0}.uid.UniqueIdPersonMapping mapping
                                                left outer join {1}.edfi.{2} person on mapping.Id = person.Id
                                                where mapping.UniqueId = @uniqueId", 
                                                    uniqueIdIntegrationDatabaseName,
                                                    GetOdsDatabaseName(),
                                                    personTypeName);

                    command.Parameters.AddWithValue("@uniqueId", uniqueId);
                    connection.Open();
                    var rdr = command.ExecuteReader();
                    if (!rdr.Read()) return results;
                    results.Usi = rdr["Usi"] == DBNull.Value ? default(int) : Convert.ToInt32(rdr["Usi"]);
                    results.Id = rdr["Id"] == DBNull.Value ? default(Guid) : new Guid(rdr["Id"].ToString());
                    results.UniqueId = rdr["UniqueId"] == DBNull.Value ? default(string) : rdr["UniqueId"].ToString();
                }
            }

            return results;
        }

        private PersonIdentifiersValueMap Get(string personTypeName, Guid id)
        {
            var results = new PersonIdentifiersValueMap();

            if (!PersonEntitySpecification.IsPersonEntity(personTypeName))
                throw new ArgumentException(
                    string.Format("Invalid person type '{0}'. Valid person types are: {1}", personTypeName,
                        "'" + String.Join("','", PersonEntitySpecification.ValidPersonTypes) + "'"));

            string uniqueIdIntegrationConnectionString = _uniqueIdIntegrationDatabaseConnectionStringProvider.GetConnectionString();

            using (var connection = new SqlConnection(uniqueIdIntegrationConnectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText =
                        string.Format(@"select mapping.Id as Id, person.{2}USI as Usi, person.Id as Id, mapping.UniqueId as UniqueId
                                            from {0}.uid.UniqueIdPersonMapping mapping
                                                left outer join {1}.edfi.{2} person on mapping.Id = person.Id
                                                where mapping.Id = @id",
                                GetUniqueIdIntegrationDatabaseName(uniqueIdIntegrationConnectionString),
                                GetOdsDatabaseName(),
                                personTypeName);

                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var rdr = command.ExecuteReader();
                    if (!rdr.Read()) return results;
                    results.Usi = rdr["Usi"] == DBNull.Value ? default(int) : Convert.ToInt32(rdr["Usi"]);
                    results.Id = rdr["Id"] == DBNull.Value ? default(Guid) : new Guid(rdr["Id"].ToString());
                    results.UniqueId = rdr["UniqueId"] == DBNull.Value ? default(string) : rdr["UniqueId"].ToString();
                }
            }
            
            return results;
        }

        private PersonIdentifiersValueMap Get(string personTypeName, int usi)
        {
            var results = new PersonIdentifiersValueMap();

            if (!PersonEntitySpecification.IsPersonEntity(personTypeName))
                throw new ArgumentException(
                    string.Format("Invalid person type '{0}'. Valid person types are: {1}", personTypeName,
                        "'" + String.Join("','", PersonEntitySpecification.ValidPersonTypes) + "'"));

            string uniqueIdIntegrationConnectionString = _uniqueIdIntegrationDatabaseConnectionStringProvider.GetConnectionString();

            using (var connection = new SqlConnection(uniqueIdIntegrationConnectionString))
            {
                using (var command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText =
                        string.Format(@"select mapping.Id as Id, person.{2}USI as Usi, person.Id as Id, mapping.UniqueId as UniqueId
                                            from {0}.uid.UniqueIdPersonMapping mapping
                                                left outer join {1}.edfi.{2} person on mapping.Id = person.Id
                                                where person.{2}USI = @usi",
                                GetUniqueIdIntegrationDatabaseName(uniqueIdIntegrationConnectionString),
                                GetOdsDatabaseName(), 
                                personTypeName);

                    command.Parameters.AddWithValue("@usi", usi);
                    connection.Open();
                    var rdr = command.ExecuteReader();
                    if (!rdr.Read()) return results;
                    results.Usi = rdr["Usi"] == DBNull.Value ? default(int) : Convert.ToInt32(rdr["Usi"]);
                    results.Id = rdr["Id"] == DBNull.Value ? default(Guid) : new Guid(rdr["Id"].ToString());
                    results.UniqueId = rdr["UniqueId"] == DBNull.Value ? default(string) : rdr["UniqueId"].ToString();
                }
            }
            
            return results;
        }

        private static readonly ConcurrentDictionary<int, string> UniqueIdDbNameByConnectionStringHashCode
            = new ConcurrentDictionary<int, string>();

        private static string GetUniqueIdIntegrationDatabaseName(string uniqueIdIntegrationConnectionString)
        {
            string dbName =
                UniqueIdDbNameByConnectionStringHashCode.GetOrAdd(
                    uniqueIdIntegrationConnectionString.GetHashCode(),
                    cs => new SqlConnectionStringBuilder(uniqueIdIntegrationConnectionString).InitialCatalog);

            return dbName;
        }

        private readonly ConcurrentDictionary<int, string> _odsDatabaseNameByConnectionString
            = new ConcurrentDictionary<int, string>();

        private string GetOdsDatabaseName()
        {
            string connectionString = _odsDatabaseConnectionStringProvider.GetConnectionString();

            string dbName =
                _odsDatabaseNameByConnectionString.GetOrAdd(
                    connectionString.GetHashCode(),
                    hc => new SqlConnectionStringBuilder(connectionString).InitialCatalog);
            
            return dbName;
        }
    }
}