using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace NullLib.CommandLine
{
    public interface IArgumentConverter
    {
        Type TargetType { get; }
        object Convert(string argument);
        object Convert(object argument);
        bool TryConvert(string argument, out object result);
        bool TryConvert(object argument, out object result);
    }
    public interface IArgumentConverter<TResult> : IArgumentConverter
    {
        new TResult Convert(string argument);
        new TResult Convert(object argument);
        bool TryConvert(string argument, out TResult result);
        bool TryConvert(object argument, out TResult result);
    }

    public static class ArgumentConverterManager
    {
        private static Type IArgumentConverterType = typeof(IArgumentConverter);
        public static Dictionary<Type, IArgumentConverter> AllConverters = new Dictionary<Type, IArgumentConverter>();

        public static IArgumentConverter GetArgumentConverter<T>() where T : IArgumentConverter
        {
            Type type = typeof(T);
            return AllConverters.TryGetValue(type, out var result) ? result : AllConverters[type] = Activator.CreateInstance<T>();
        }
        public static IArgumentConverter GetArgumentConverter(Type type)
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
                    return AllConverters[type] = Activator.CreateInstance(type) as IArgumentConverter;
                else
                    throw new ArgumentOutOfRangeException(nameof(type), $"Type must be assignable to {nameof(IArgumentConverter)}.");
            }
        }
    }

    public class ArgumentConverter : IArgumentConverter
    {
        private Type targetType = typeof(string);
        public virtual Type TargetType { get => targetType; }

        public virtual object Convert(string argument)
        {
            return argument;
        }

        public virtual object Convert(object argument)
        {
            return argument;
        }

        public virtual bool TryConvert(string argument, out object result)
        {
            try
            {
                result = Convert(argument);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }

        public virtual bool TryConvert(object argument, out object result)
        {
            try
            {
                result = Convert(argument);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
    public class ArgumentConverter<TTarget> : IArgumentConverter<TTarget>
    {
        private Type targetType = typeof(TTarget);
        public Type TargetType { get => targetType; }

        public virtual TTarget Convert(string argument)
        {
            return default;
        }

        public virtual TTarget Convert(object argument)
        {
            return Convert(argument as string);
        }

        public virtual bool TryConvert(string argument, out TTarget result)
        {
            try
            {
                result = Convert(argument);
                return true;
            }
            catch
            {
                result = default;
                return false;
            }
        }
        public virtual bool TryConvert(string argument, out object result)
        {
            bool succ = TryConvert(argument, out TTarget resultT);
            result = resultT;
            return succ;
        }

        public virtual bool TryConvert(object argument, out TTarget result)
        {
            return TryConvert(argument as string, out result);
        }

        public virtual bool TryConvert(object argument, out object result)
        {
            if (TryConvert(argument, out TTarget rst))
            {
                result = rst;
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }

        object IArgumentConverter.Convert(string argument)
        {
            return Convert(argument);
        }

        object IArgumentConverter.Convert(object argument)
        {
            return Convert(argument);
        }
    }

    public class ByteConverter : ArgumentConverter<byte>
    {
        public override byte Convert(string argument)
        {
            return byte.Parse(argument);
        }
        public override bool TryConvert(string argument, out byte result)
        {
            return byte.TryParse(argument, out result);
        }
    }
    public class ShortConverter : ArgumentConverter<short>
    {
        public override short Convert(string argument)
        {
            return short.Parse(argument);
        }
        public override bool TryConvert(string argument, out short result)
        {
            return short.TryParse(argument, out result);
        }
    }
    public class IntegerConverter : ArgumentConverter<int>
    {
        public override int Convert(string argument)
        {
            return int.Parse(argument);
        }
        public override bool TryConvert(string argument, out int result)
        {
            return int.TryParse(argument, out result);
        }
    }
    public class LongConverter : ArgumentConverter<long>
    {
        public override long Convert(string argument)
        {
            return long.Parse(argument);
        }
        public override bool TryConvert(string argument, out long result)
        {
            return long.TryParse(argument, out result);
        }
    }
    public class FloatConverter : ArgumentConverter<float>
    {
        public override float Convert(string argument)
        {
            return float.Parse(argument);
        }
        public override bool TryConvert(string argument, out float result)
        {
            return float.TryParse(argument, out result);
        }
    }
    public class DoubleConverter : ArgumentConverter<double>
    {
        public override double Convert(string argument)
        {
            return double.Parse(argument);
        }
        public override bool TryConvert(string argument, out double result)
        {
            return double.TryParse(argument, out result);
        }
    }
    public class DecimalConverter : ArgumentConverter<decimal>
    {
        public override decimal Convert(string argument)
        {
            return decimal.Parse(argument);
        }
        public override bool TryConvert(string argument, out decimal result)
        {
            return decimal.TryParse(argument, out result);
        }
    }
    public class EnumConverter<T> : ArgumentConverter<T> where T : struct
    {
        public override T Convert(string argument)
        {
            return (T)Enum.Parse(typeof(T), argument);
        }
        public override bool TryConvert(string argument, out T result)
        {
            return Enum.TryParse(argument, out result);
        }
    }
    public class ForeachConverter<TConverter> : ArgumentConverter where TConverter : IArgumentConverter
    {
        IArgumentConverter converter;
        public ForeachConverter()
        {
            converter = ArgumentConverterManager.GetArgumentConverter<TConverter>() as IArgumentConverter;
        }
        public override object Convert(string argument)
        {
            Array result = Array.CreateInstance(converter.TargetType, 1);
            result.SetValue(converter.Convert(argument), 0);
            return result;
        }
        public override object Convert(object argument)
        {
            if (argument is string str)
            {
                return Convert(str);
            }
            else if (argument is string[] strs)
            {
                Array result = Array.CreateInstance(converter.TargetType, strs.Length);
                for (int i = 0, end = strs.Length; i < end; i++)
                    result.SetValue(converter.Convert(strs[i]), i);
                return result;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(argument), "Cannot convert argument");
            }
        }
        public override bool TryConvert(string argument, out object result)
        {
            if (converter.TryConvert(argument, out object rst))
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
        public override bool TryConvert(object argument, out object result)
        {
            if (argument is string str)
            {
                return TryConvert(str, out result);
            }
            else if (argument is string[] strs)
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
}
