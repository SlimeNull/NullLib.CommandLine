using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullLib.CommandLine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        readonly IArgumentConverter[] arguConverters;

        private static Type IArgumentConverterType = typeof(IArgumentConverter);
        public static Dictionary<Type, IArgumentConverter> AllConverters;

        public static IArgumentConverter GetArgumentConverter<T>() where T : IArgumentConverter
        {
            Type type = typeof(T);
            return AllConverters.TryGetValue(type, out var result) ? result : AllConverters[type] = Activator.CreateInstance<T>();
        }
        public static IArgumentConverter GetArgumentConverter(Type type)
        {
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

        public IArgumentConverter[] ArgumentConverters { get => arguConverters; }
        public CommandAttribute(params Type[] arguConverters)
        {
            if (AllConverters == null)
                AllConverters = new Dictionary<Type, IArgumentConverter>();

            try
            {
                this.arguConverters = new IArgumentConverter[arguConverters.Length];
                for (int i = 0, end = arguConverters.Length; i < end; i++)
                    this.arguConverters[i] = GetArgumentConverter(arguConverters[i]);
            }
            catch (ArgumentOutOfRangeException e)
            {
                throw new ArgumentOutOfRangeException(nameof(arguConverters), $"Type must be assignable to {nameof(IArgumentConverter)}.");
            }

#if DEBUG
            //Console.WriteLine("AllConvertersCount:" + AllConverters.Count);
#endif
        }
    }
}
