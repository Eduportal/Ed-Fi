namespace EdFi.Ods.Tests.EdFi.Ods.Common.ExceptionHandling
{
    using System;
    using System.Data.SqlClient;
    using System.Reflection;

    using NHibernate.Exceptions;

    public static class NHibernateExceptionBuilder
    {
        public static GenericADOException CreateException(string nhibernateMessage, string sqlMessage)
        {
            return new GenericADOException(nhibernateMessage, CreateSqlException(sqlMessage, null));
        }

        private static SqlException CreateSqlException(string message, Exception innerException)
        {
            var exception = GetSqlExceptionConstructor().Invoke(new object[]
                {
                    message, null, innerException, default(Guid)
                });

            return (SqlException) exception;
        }

        private static ConstructorInfo sqlExceptionConstructor;

        private static ConstructorInfo GetSqlExceptionConstructor()
        {
            if (sqlExceptionConstructor == null)
            {
                var constructorTypes = new[]
                    {
                        typeof(string), typeof(SqlErrorCollection), typeof(Exception), typeof(Guid)
                    };

                sqlExceptionConstructor = typeof(SqlException).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, constructorTypes, null);
            }

            return sqlExceptionConstructor;
        }
    }
}