namespace EdFi.Ods.Tests._Bases
{
    using System;
    using System.Collections.Generic;

    using global::EdFi.Ods.Common.ExceptionHandling;
    using global::EdFi.Ods.Common.ExceptionHandling.Translators;
    using global::EdFi.Ods.Common.Utils;
    using Rhino.Mocks;

    public class TestBase
    {
        protected T Stub<T>() where T : class
        {
            return MockRepository.GenerateStub<T>();
        }

        protected void InitSystemClock(DateTime time)
        {
            SystemClock.Now = () => time;
        }

        protected IRESTErrorProvider GetErrorProvider()
        {
            return new RESTErrorProvider(BuildExceptionTranslators());
        }

        private static IEnumerable<IExceptionTranslator> BuildExceptionTranslators()
        {
            return new IExceptionTranslator[]
                       {
                           new BadRequestExceptionTranslator(), new SqlServerConstraintExceptionTranslator(),
                           new SqlServerUniqueIndexExceptionTranslator(new DatabaseMetadataProvider()),
                           new EdFiSecurityExceptionTranslator(), new NotFoundExceptionTranslator(), 
                           new NotModifiedExceptionTranslator(), new ConcurencyExceptionTranslator(), 
                       };
        }

        protected TException TestForException<TException>(Action action) where TException : Exception
        {
            try
            {
                action();
                return null;
            }
            catch (TException e)
            {
                return e;
            }
        }
    }
}