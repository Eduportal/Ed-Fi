using System;
using NUnit.Framework;

namespace EdFi.Ods.Tests._Extensions
{
    public static class ExceptionAssertionExtensions
    {
        public static void ShouldBeExceptionType<TExpected>(this Exception exception)
        {
            if (exception == null)
                Assert.That(exception, Is.TypeOf<TExpected>());
            else 
                Assert.That(exception, Is.TypeOf<TExpected>(), GetActualExceptionOutputMessage(exception));
        }

        private static string GetActualExceptionOutputMessage(Exception exception)
        {
            return "Actual Exception: " + exception + "\r\n---------------------------------------------";
        }

        public static void MessageShouldContain(this Exception exception, string expectedSubstring)
        {
            Assert.That(exception.Message, Is.StringContaining(expectedSubstring));
        }

        public static void ShouldBeNull(this Exception exception)
        {
            Assert.That(exception, Is.Null, GetActualExceptionOutputMessage(exception));
        }
    }
}