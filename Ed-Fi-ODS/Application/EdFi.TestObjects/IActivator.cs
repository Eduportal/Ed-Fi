using System;

namespace EdFi.TestObjects
{
    public interface IActivator
    {
        T CreateInstance<T>();
        object CreateInstance(Type type);
    }
}