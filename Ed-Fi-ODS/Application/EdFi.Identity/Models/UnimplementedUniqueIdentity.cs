using System;
using System.Threading.Tasks;
using EdFi.Common.Identity;
using FluentValidation;
using FluentValidation.Results;

namespace EdFi.Identity.Models
{
    /// <summary>
    /// A Null implementation of the Unique Identity interface
    /// <remarks>
    /// This implmentation should be used when no integration with a Unique Identity system is 
    /// desired. It triggers the appropriate HTTP 501 response in the Identity API
    /// </remarks>
    /// </summary>
    public class UnimplementedUniqueIdentity : IUniqueIdentity
    {
        IIdentity IUniqueIdentity.Get(string uniqueId)
        {
            throw new NotImplementedException();
        }

        IIdentity[] IUniqueIdentity.Get(IIdentity identity)
        {
            throw new NotImplementedException();
        }

        IIdentity IUniqueIdentity.Post(IIdentity command)
        {
            throw new NotImplementedException();
        }
    }
}
