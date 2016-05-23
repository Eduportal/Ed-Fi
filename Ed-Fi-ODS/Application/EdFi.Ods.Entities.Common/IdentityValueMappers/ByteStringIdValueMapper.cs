using System;
using System.Text;

namespace EdFi.Ods.Entities.Common.IdentityValueMappers
{
    public class ByteStringIdValueMapper : IUniqueIdToIdValueMapper
    {
        public PersonIdentifiersValueMap GetId(string personType, string uniqueId)
        {
            var byteArray = Encoding.ASCII.GetBytes(uniqueId);
            Array.Resize(ref byteArray, 16);

            return new PersonIdentifiersValueMap
            {
                Id = new Guid(byteArray),
                UniqueId = uniqueId
            };
        }

        public PersonIdentifiersValueMap GetUniqueId(string personType, Guid id)
        {
            return new PersonIdentifiersValueMap
            {
                Id = id,
                UniqueId = Encoding.ASCII.GetString(id.ToByteArray()).TrimEnd('\0')
            };
        }
    }
}
