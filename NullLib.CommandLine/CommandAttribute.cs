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
        public IArgumentConverter[] ArgumentConverters { get => arguConverters; }
        public CommandAttribute(params Type[] arguConverters)
        {
            try
            {
                this.arguConverters = new IArgumentConverter[arguConverters.Length];
                for (int i = 0, end = arguConverters.Length; i < end; i++)
                    this.arguConverters[i] = ArgumentConverterManager.GetConverter(arguConverters[i]);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException(nameof(arguConverters), $"Type must be assignable to {nameof(IArgumentConverter)}.");
            }

#if DEBUG
            //Console.WriteLine("AllConvertersCount:" + AllConverters.Count);
#endif
        }
    }
}
