using System;

namespace EdFi.TestObjects
{
    public class SystemActivator : IActivator
    {
        public T CreateInstance<T>()
        {
            return (T) CreateInstance(typeof(T));
        }

        public object CreateInstance(Type type)
        {
            return Activator.CreateInstance(type);
        }
    }
}