using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullLib.CommandLine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NCommandAttribute : Attribute
    {
        IArgumentConverter[] argumentConverters;

        public IArgumentConverter[] ArgumentConverters { get => argumentConverters; }
        public NCommandAttribute(params Type[] arguConverters)
        {
            this.argumentConverters = arguConverters
                .Select((v) => Activator.CreateInstance(v))
                .OfType<IArgumentConverter>()
                .ToArray();
        }
    }

    public interface IArgumentConverter
    {
        object Convert(string argument);
        bool TryConvert(string argument, out object result);
    }
    public interface IArgumentConverter<T> : IArgumentConverter
    {
        new T Convert(string argument);
        bool TryConvert(string argument, out T result);
    }
    public abstract class ArgumentConverter : IArgumentConverter
    {
        public virtual object Convert(string argument)
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
    }
    public abstract class ArgumentConverter<T> : IArgumentConverter<T>
    {
        public virtual T Convert(string argument)
        {
            return default(T);
        }
        public virtual bool TryConvert(string argument, out T result)
        {
            try
            {
                result = Convert(argument);
                return true;
            }
            catch
            { 
                result = default(T); 
                return false;
            }
        }
        public virtual bool TryConvert(string argument, out object result)
        {
            bool succ = TryConvert(argument, out T resultT);
            result = resultT;
            return succ;
        }

        object IArgumentConverter.Convert(string argument)
        {
            return Convert(argument);
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
}
