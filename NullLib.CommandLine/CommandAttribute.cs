using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace NullLib.CommandLine
{
    /// <summary>
    /// Specify a method can be execute by 
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class CommandAttribute : Attribute
    {
        readonly IArgumentConverter[] arguConverters;
        /// <summary>
        /// ArgumentConverters for current command
        /// </summary>
        public IArgumentConverter[] ArgumentConverters { get => arguConverters; }

        /// <summary>
        /// Initialize a new instance of CommandAttribute with no special IArgumentConverter
        /// If your method only has string parameters, you can use this.
        /// </summary>
        public CommandAttribute()
        {
            arguConverters = new IArgumentConverter[0];
        }
        /// <summary>
        /// Initialize a new instance of CommandAttribute
        /// </summary>
        /// <param name="arguConverters"></param>
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

        /// <summary>
        /// Description about current command
        /// </summary>
        public string Description { get; set; }
    }
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class CommandParamAttribute : Attribute
    {
        public string Description { get; set; }
        public string DefaultValue { get; set; }
    }
}
