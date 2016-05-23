using System;

namespace EdFi.Identity.Models
{
using EdFi.Common.Identity;

    /// <summary>
    /// Sample implementation of IUniqueIdentity
    /// <remarks>
    /// This implementation is not for production or test use, it only demonstrates the use of the Unique Identity interfaces.
    /// </remarks>
    /// </summary>
    public class SampleUniqueIdentity : IUniqueIdentity
    {
        public IIdentity Get(string uniqueId)
        {
            return new Identity()
                       {
                           BirthDate = new DateTime(2000, 1, 1),
                           BirthGender = Gender.Male,
                           FamilyNames = "Doe",
                           GivenNames = "John",
                           Identifiers = new IIdentifier[] { },
                           UniqueId = uniqueId,
                           Weight = 10.0
                       };
        }

        public IIdentity[] Get(IIdentity identity)
        {
            return new IIdentity[] { identity };
        }

        public IIdentity Post(IIdentity command)
        {
            command.UniqueId = Guid.NewGuid().ToString("N");
            return command;
        }
    }
}
