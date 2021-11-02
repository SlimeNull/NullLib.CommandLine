using System;
using System.Collections.Generic;
using System.Numerics;
using System.Globalization;
using System.Text;
using System.Reflection;

namespace NullLib.CommandLine
{
    /// <summary>
    /// Provide methods to convert string or string[] to required parameter type
    /// </summary>
    public interface IArguConverter
    {
        /// <summary>
        /// TargetType of this converter
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Ignore cases when converting
        /// </summary>
        bool IgnoreCases { get; set; }

        /// <summary>
        /// Convert from a string
        /// </summary>
        /// <param name="argu">String to convert</param>
        /// <returns>Conversion result</returns>
        object Convert(string argu);
        /// <summary>
        /// Convert from an object
        /// </summary>
        /// <param name="argu">Object to convert</param>
        /// <returns>Conversion result</returns>
        object Convert(object argu);
        /// <summary>
        /// Convert object back to string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string ConvertBack(object obj);
        /// <summary>
        /// Convert from a string
        /// </summary>
        /// <param name="argu">String to convert</param>
        /// <param name="result">Conversion result</param>
        /// <returns>If the Conversion was successed</returns>
        bool TryConvert(string argu, out object result);
        /// <summary>
        /// Convert form an object
        /// </summary>
        /// <param name="argu">Object to convert</param>
        /// <param name="result">Conversion result</param>
        /// <returns>If the Conversion was successed</returns>
        bool TryConvert(object argu, out object result);
        /// <summary>
        /// Convert object back to string
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryConvertBack(object obj, out string result);
    }
    /// <summary>
    /// Provide methods to convert string or string[] to <typeparamref name="TResult"/>
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    public interface IArguConverter<TResult> : IArguConverter
    {
        /// <summary>
        /// Convert from a string
        /// </summary>
        /// <param name="argu">String to convert</param>
        /// <returns>Conversion result</returns>
        new TResult Convert(string argu);
        /// <summary>
        /// Convert from an object
        /// </summary>
        /// <param name="argu">Object to convert</param>
        /// <returns>Conversion result</returns>
        new TResult Convert(object argu);
        /// <summary>
        /// Convert <typeparamref name="TResult"/> back to string
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        string ConvertBack(TResult obj);
        /// <summary>
        /// Try to convert from a string
        /// </summary>
        /// <param name="argu">String to convert</param>
        /// <param name="result">Conversion result</param>
        /// <returns>If the Conversion was successed</returns>
        bool TryConvert(string argu, out TResult result);
        /// <summary>
        /// Try to convert from a object
        /// </summary>
        /// <param name="argu"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryConvert(object argu, out TResult result);
        /// <summary>
        /// Convert <typeparamref name="TResult"/> back to string
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        bool TryConvertBack(TResult obj, out string result);
    }

    /// <summary>
    /// Provide methods for getting ArgumentConverter without initialize repeated converter
    /// </summary>
    public static class ArguConverterManager
    {
        private static readonly Type IArgumentConverterType = typeof(IArguConverter);
        /// <summary>
        /// Global converter storage
        /// </summary>
        public static Dictionary<Type, IArguConverter> AllConverters { get; } = new Dictionary<Type, IArguConverter>();

        /// <summary>
        /// Get from global storage or initialize a converter
        /// </summary>
        /// <typeparam name="T">Converter type</typeparam>
        /// <returns>Result converter</returns>
        public static IArguConverter GetConverter<T>() where T : IArguConverter
        {
            Type type = typeof(T);
            return AllConverters.TryGetValue(type, out var result) ? result : AllConverters[type] = Activator.CreateInstance<T>();
        }
        /// <summary>
        /// Get from global storage or initialize a converter
        /// </summary>
        /// <param name="type">Converter type</param>
        /// <returns>Result converter</returns>
        public static IArguConverter GetConverter(Type type)
        {
            if (type == null)
                return null;

            if (AllConverters.TryGetValue(type, out var obj))
            {
                return obj;
            }
            else
            {
                if (IArgumentConverterType.IsAssignableFrom(type))
                    return AllConverters[type] = Activator.CreateInstance(type) as IArguConverter;
                else
                    throw new ArgumentOutOfRangeException(nameof(type), $"Type must be assignable to {nameof(IArguConverter)}.");
            }
        }
    }

    /// <summary>
    /// Base class of ArgumentConverter
    /// </summary>
    public abstract class ArguConverterBase : IArguConverter
    {
        private readonly Type targetType = typeof(string);
        /// <summary>
        /// Argument Conversion Target Type
        /// </summary>
        public virtual Type TargetType { get => targetType; }

        /// <summary>
        /// If this converter is cases ignored
        /// </summary>
        public virtual bool IgnoreCases { get; set; }

        public virtual object Convert(string argu)
        {
            return argu;
        }
        public virtual object Convert(object argu)
        {
            return argu;
        }

        public virtual string ConvertBack(object obj)
        {
            return obj.ToString();
        }

        public virtual bool TryConvert(string argu, out object result)
        {
            try
            {
                result = Convert(argu);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        public virtual bool TryConvert(object argu, out object result)
        {
            return TryConvert(argu as string, out result);
        }

        public virtual bool TryConvertBack(object obj, out string result)
        {
            try
            {
                result = ConvertBack(obj);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
    /// <summary>
    /// Base class of ArgumentConverter
    /// </summary>
    /// <typeparam name="TTarget"></typeparam>
    public abstract class ArguConverterBase<TTarget> : IArguConverter<TTarget>
    {
        private static readonly Type targetType = typeof(TTarget);
        public Type TargetType { get => targetType; }

        public virtual bool IgnoreCases { get; set; }

        public virtual TTarget Convert(string argu)
        {
            return default;
        }
        public virtual TTarget Convert(object argu)
        {
            return Convert(argu?.ToString());
        }

        public virtual string ConvertBack(TTarget obj)
        {
            return obj.ToString();
        }
        public virtual string ConvertBack(object obj)
        {
            if (obj is TTarget tobj)
                return ConvertBack(tobj);
            else
                return ConvertBack(default);
        }

        public virtual bool TryConvert(string argu, out TTarget result)
        {
            try
            {
                result = Convert(argu);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
        public virtual bool TryConvert(string argu, out object result)
        {
            bool succ = TryConvert(argu, out TTarget resultT);
            result = resultT;
            return succ;
        }

        public virtual bool TryConvert(object argu, out TTarget result)
        {
            return TryConvert(argu as string, out result);
        }
        public virtual bool TryConvert(object argu, out object result)
        {
            return TryConvert(argu as string, out result);
        }

        public virtual bool TryConvertBack(TTarget obj, out string result)
        {
            try
            {
                result = ConvertBack(obj);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        public virtual bool TryConvertBack(object obj, out string result)
        {
            if (obj is TTarget tobj)
                return TryConvertBack(tobj, out result);
            else
                return TryConvertBack(default, out result);
        }

        object IArguConverter.Convert(string argu)
        {
            return Convert(argu);
        }
        object IArguConverter.Convert(object argu)
        {
            return Convert(argu);
        }
    }

    /// <summary>
    /// Default converter, return value without any conversion
    /// </summary>
    public class ArguConverter : ArguConverterBase<string>
    {
        public override string Convert(string argu)
        {
            return argu;
        }
        public override bool TryConvert(string argu, out string result)
        {
            result = argu;
            return true;
        }
    }

    /// <summary>
    /// Bool converter, return true if "true", false if "false", otherwise, convert failed
    /// </summary>
    public class BoolArguConverter : ArguConverterBase<bool>
    {
        public override bool Convert(string argu)
        {
            return bool.Parse(argu);
        }
        public override bool TryConvert(string argu, out bool result)
        {
            return bool.TryParse(argu, out result);
        }
    }
    /// <summary>
    /// Byte convert, convert by byte.Parse and byte.TryParse
    /// </summary>
    public class ByteArguConverter : ArguConverterBase<byte>
    {
        public override byte Convert(string argu)
        {
            return byte.Parse(argu);
        }
        public override bool TryConvert(string argu, out byte result)
        {
            return byte.TryParse(argu, out result);
        }
    }
    /// <summary>
    /// Char convert, if string has only one char, then return it, otherwise, convert failed
    /// </summary>
    public class CharArguConverter : ArguConverterBase<char>
    {
        public override char Convert(string argu)
        {
            if (argu.Length == 1)
            {
                return argu[0];
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(argu), "Cannot convert argument");
            }
        }
        public override bool TryConvert(string argu, out char result)
        {
            if (argu.Length == 1)
            {
                result = argu[0];
                return true;
            }
            else
            {
                result = char.MinValue;
                return false;
            }
        }
    }
    /// <summary>
    /// Short converter, convert by short.Parse and short.TryParse
    /// </summary>
    public class ShortArguConverter : ArguConverterBase<short>
    {
        public override short Convert(string argu)
        {
            return short.Parse(argu);
        }
        public override bool TryConvert(string argu, out short result)
        {
            return short.TryParse(argu, out result);
        }
    }
    /// <summary>
    /// Int converter, convert by int.Parse and int.TryParse
    /// </summary>
    public class IntArguConverter : ArguConverterBase<int>
    {
        public override int Convert(string argu)
        {
            return int.Parse(argu);
        }
        public override bool TryConvert(string argu, out int result)
        {
            return int.TryParse(argu, out result);
        }
    }
    /// <summary>
    /// Long converter, convert by long.Parse and long.TryParse
    /// </summary>
    public class LongArguConverter : ArguConverterBase<long>
    {
        public override long Convert(string argu)
        {
            return long.Parse(argu);
        }
        public override bool TryConvert(string argu, out long result)
        {
            return long.TryParse(argu, out result);
        }
    }
    /// <summary>
    /// Float converter, convert by float.Parse and float.TryParse
    /// </summary>
    public class FloatArguConverter : ArguConverterBase<float>
    {
        public override float Convert(string argu)
        {
            return float.Parse(argu);
        }
        public override bool TryConvert(string argu, out float result)
        {
            return float.TryParse(argu, out result);
        }
    }
    /// <summary>
    /// Double converter, convert by double.Parse and double.TryParse
    /// </summary>
    public class DoubleArguConverter : ArguConverterBase<double>
    {
        public override double Convert(string argu)
        {
            return double.Parse(argu);
        }
        public override bool TryConvert(string argu, out double result)
        {
            return double.TryParse(argu, out result);
        }
    }
    /// <summary>
    /// BigInt converter, convert by BigInteger.Parse and BigInteger.TryParse
    /// </summary>
    public class BigIntArguConverter : ArguConverterBase<BigInteger>
    {
        public override BigInteger Convert(string argu)
        {
            return BigInteger.Parse(argu);
        }
        public override bool TryConvert(string argu, out BigInteger result)
        {
            return BigInteger.TryParse(argu, out result);
        }
    }
    /// <summary>
    /// Decimal converter, convert by Decimal.Parse and Decimal.TryParse
    /// </summary>
    public class DecimalArguConverter : ArguConverterBase<decimal>
    {
        public override decimal Convert(string argu)
        {
            return decimal.Parse(argu);
        }
        public override bool TryConvert(string argu, out decimal result)
        {
            return decimal.TryParse(argu, out result);
        }
    }
    /// <summary>
    /// Enum converter, convert by Enum.Parse and Enum.TryParse
    /// </summary>
    /// <typeparam name="T">Enum type</typeparam>
    public class EnumArguConverter<T> : ArguConverterBase<T> where T : struct
    {
        public override T Convert(string argu)
        {
            return (T)Enum.Parse(TargetType, argu, IgnoreCases);
        }
        public override bool TryConvert(string argu, out T result)
        {
            return Enum.TryParse(argu, IgnoreCases, out result);
        }
    }
    /// <summary>
    /// Convert from string[], use <typeparamref name="TConverter"/> to convert each element, only use in "params" parameter
    /// </summary>
    /// <typeparam name="TConverter">Converter to use</typeparam>
    public class ForeachArguConverter<TConverter> : ArguConverterBase where TConverter : IArguConverter
    {
        public override Type TargetType => targetType;

        readonly IArguConverter converter;
        private readonly Type targetType;

        public ForeachArguConverter()
        {
            converter = ArguConverterManager.GetConverter<TConverter>();
            targetType = converter.TargetType.MakeArrayType();
        }
        public override object Convert(string argu)
        {
            converter.IgnoreCases = IgnoreCases;
            Array result = Array.CreateInstance(converter.TargetType, 1);
            result.SetValue(converter.Convert(argu), 0);
            return result;
        }
        public override object Convert(object argu)
        {
            converter.IgnoreCases = IgnoreCases;
            if (argu is string str)
            {
                return Convert(str);
            }
            else if (argu is string[] strs)
            {
                Array result = Array.CreateInstance(converter.TargetType, strs.Length);
                for (int i = 0, end = strs.Length; i < end; i++)
                    result.SetValue(converter.Convert(strs[i]), i);
                return result;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(argu), "Cannot convert argument");
            }
        }
        public override bool TryConvert(string argu, out object result)
        {
            converter.IgnoreCases = IgnoreCases;
            if (converter.TryConvert(argu, out object rst))
            {
                Array resultarr = Array.CreateInstance(converter.TargetType, 1);
                resultarr.SetValue(rst, 0);
                result = resultarr;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
        public override bool TryConvert(object argu, out object result)
        {
            converter.IgnoreCases = IgnoreCases;
            if (argu is string str)
            {
                return TryConvert(str, out result);
            }
            else if (argu is string[] strs)
            {
                bool succeed = true;
                Array resultarray = Array.CreateInstance(converter.TargetType, strs.Length);
                for (int i = 0, end = strs.Length; i < end; i++)
                {
                    if (succeed &= converter.TryConvert(strs[i], out object rst))
                        resultarray.SetValue(rst, i);
                    else
                        break;
                }
                result = resultarray;
                return succeed;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
    /// <summary>
    /// Char[] converter, convert string to char[]
    /// </summary>
    public class CharArrayArguConverter : ArguConverterBase<char[]>
    {
        public override char[] Convert(string argu)
        {
            return argu.ToCharArray();
        }
        // do not need to override TryConvert
    }
}
