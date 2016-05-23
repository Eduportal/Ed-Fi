using System;

namespace EdFi.Ods.Utilities.LoadGeneration
{
    public interface IItemFromApiProvider 
    {
        T GetNext<T>() where T : class;
        object GetNext(Type type);
    }
}