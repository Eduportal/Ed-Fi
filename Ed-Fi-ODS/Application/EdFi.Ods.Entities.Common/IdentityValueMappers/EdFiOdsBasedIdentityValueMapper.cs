// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;
using System.Data;
using System.Data.SqlClient;
using EdFi.Common.Database;
using EdFi.Ods.Common.Specifications;

namespace EdFi.Ods.Entities.Common.IdentityValueMappers
{
    /// <summary>
    /// Provides an implementation of the person identifier value mappers that treats all incoming
    /// UniqueId values as pure data elements and implements no integration with an external UniqueId 
    /// system (e.g EduId or other identity solution).
    /// </summary>
    public class EdFiOdsBasedUsiValueMapper : IUniqueIdToUsiValueMapper
    {
        private readonly IDatabaseConnectionStringProvider _odsDatabaseConnectionStringProvider;

        public EdFiOdsBasedUsiValueMapper(IDatabaseConnectionStringProvider odsDatabaseConnectionStringProvider)
        {
            _odsDatabaseConnectionStringProvider = odsDatabaseConnectionStringProvider;
        }

        public PersonIdentifiersValueMap GetUsi(string personType, string uniqueId)
        {
            return GetPersonIdentifiersValueMap(personType, personType + "UniqueId", uniqueId);
        }

        public PersonIdentifiersValueMap GetUniqueId(string personType, int usi)
        {
            return GetPersonIdentifiersValueMap(personType, personType + "USI", usi);
        }

        private PersonIdentifiersValueMap GetPersonIdentifiersValueMap(string personType, string searchField, object searchValue)
        {
            // Validate Person type
            if (!PersonEntitySpecification.IsPersonEntity(personType))
            {
                throw new ArgumentException(
                    string.Format("Invalid person type '{0}'. Valid person types are: {1}", personType,
                        "'" + String.Join("','", PersonEntitySpecification.ValidPersonTypes) + "'"));
            }

            using (var conn = new SqlConnection(_odsDatabaseConnectionStringProvider.GetConnectionString()))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText =
                    string.Format(@"
                        select person.{0}USI as Usi, person.{0}UniqueId as UniqueId
                        from edfi.{0} person 
                        where person.{1} = @value",
                        personType, searchField);

                cmd.Parameters.AddWithValue("@value", searchValue);

                using (var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    if (!rdr.Read())
                        return new PersonIdentifiersValueMap();

                    int usiCol = rdr.GetOrdinal("Usi");
                    int uniqueIdCol = rdr.GetOrdinal("UniqueId");

                    var result = new PersonIdentifiersValueMap
                    {
                        Usi = rdr.IsDBNull(usiCol) ? default(int) : rdr.GetInt32(usiCol),
                        UniqueId = rdr.IsDBNull(uniqueIdCol) ? default(string) : rdr.GetString(uniqueIdCol)
                    };

                    return result;
                }
            }
        }
    }
}