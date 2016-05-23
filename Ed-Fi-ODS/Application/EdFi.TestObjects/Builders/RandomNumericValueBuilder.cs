// ****************************************************************************
//  Copyright (C) 2014 Ed-Fi Alliance, LLC. All Rights Reserved.
// ****************************************************************************

using System;

namespace EdFi.TestObjects.Builders
{
    public class RandomNumericValueBuilder : IValueBuilder
    {
        private Random _random = new Random();

        public ValueBuildResult TryBuild(BuildContext buildContext)
        {
            if (!buildContext.TargetType.IsValueType)
                return ValueBuildResult.NotHandled;

            if (buildContext.TargetType != typeof(UInt16)
                && buildContext.TargetType != typeof(UInt32)
                && buildContext.TargetType != typeof(UInt64)
                && buildContext.TargetType != typeof(Byte)
                && buildContext.TargetType != typeof(SByte)
                && buildContext.TargetType != typeof(Int16)
                && buildContext.TargetType != typeof(Int32)
                && buildContext.TargetType != typeof(Int64)
                && buildContext.TargetType != typeof(Decimal)
                && buildContext.TargetType != typeof(Double)
                && buildContext.TargetType != typeof(Single))
                return ValueBuildResult.NotHandled;
            

            var value = GetRandomValue(buildContext.TargetType);
            return ValueBuildResult.WithValue(value, buildContext.LogicalPropertyPath);
        }

        private object GetRandomValue(Type targetType)
        {
            if (targetType == typeof(UInt16))
                return _random.Next(1, UInt16.MaxValue + 1);

            if (targetType == typeof(UInt32))
            {
                byte[] buffer = new byte[sizeof(UInt32)];
                _random.NextBytes(buffer);
                return BitConverter.ToUInt32(buffer, 0);
            }

            if (targetType == typeof(UInt64))
            {
                byte[] buffer = new byte[sizeof(UInt64)];
                _random.NextBytes(buffer);
                return BitConverter.ToUInt64(buffer, 0);
            }

            if (targetType == typeof(Byte))
            {
                byte[] buffer = new byte[sizeof(Byte)];
                _random.NextBytes(buffer);
                return buffer[0];
            }

            if (targetType == typeof(SByte))
            {
                byte[] buffer = new byte[sizeof(UInt32)];
                _random.NextBytes(buffer);
                return (SByte) buffer[0];
            }

            if (targetType == typeof(Int16))
            {
                byte[] buffer = new byte[sizeof(Int16)];
                _random.NextBytes(buffer);
                return BitConverter.ToInt16(buffer, 0);
            }

            if (targetType == typeof(Int32))
                return _random.Next(1, int.MaxValue);

            if (targetType == typeof(Int64))
            {
                byte[] buffer = new byte[sizeof(Int64)];
                _random.NextBytes(buffer);
                return BitConverter.ToInt64(buffer, 0);
            }

            if (targetType == typeof(Decimal))
            {
                byte[] buffer = new byte[sizeof(Int64)];
                _random.NextBytes(buffer);

                var numericPortion = BitConverter.ToInt64(buffer, 0);
                var decimalPortion = _random.Next(1, 10000) / 10000m;
                
                return numericPortion + decimalPortion;
            }

            if (targetType == typeof(Double))
            {
                byte[] buffer = new byte[sizeof(Double)];
                _random.NextBytes(buffer);
                return BitConverter.ToDouble(buffer, 0);
            }

            if (targetType == typeof(Single))
            {
                byte[] buffer = new byte[sizeof(Single)];
                _random.NextBytes(buffer);
                return BitConverter.ToSingle(buffer, 0);
            }

            throw new NotSupportedException(string.Format("'{0}' is not an explicitly supported numeric type.  Did you try to add support for it but forget to add explicit logic for generating a random value of this type?", targetType.Name));
        }

        public void Reset() { }
        public ITestObjectFactory Factory { get; set; }
    }
}