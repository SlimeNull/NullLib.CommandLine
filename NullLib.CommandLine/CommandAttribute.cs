using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
        private string commandName;
        private string commandAlias;

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
                    this.arguConverters[i] = ArguConverterManager.GetConverter(arguConverters[i]);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException(nameof(arguConverters), $"Type must be assignable to {nameof(IArgumentConverter)}.");
            }
        }

        public void LoadTarget(MethodInfo info)
        {
            if (commandName == null)
                commandName = info.Name;
        }

        /// <summary>
        /// Infomation of calling corresponding Command, if it's not being calling, value is null
        /// </summary>
        public CommandInvokingInfo InvokingInfo { get; internal set; }

        /// <summary>
        /// Check specified name is the correct name of current command
        /// </summary>
        /// <param name="cmdName">Name for checking</param>
        /// <param name="stringComparison">StringComparison</param>
        /// <returns></returns>
        public bool IsCorrectName(string cmdName, StringComparison stringComparison)
        {
            return cmdName != null && (cmdName.Equals(commandName, stringComparison) || cmdName.Equals(commandAlias, stringComparison));
        }

        public IEnumerable<string> ConvertArguObjects(IEnumerable<object> objs)
        {
            IArgumentConverter curConvtr = ArguConverterManager.GetConverter<ArguConverter>();
            int converterCount = arguConverters.Length;
            return objs.Select((v, i) =>
            {
                if (i < converterCount && arguConverters[i] != null)
                    curConvtr = arguConverters[i];
                return curConvtr.ConvertBack(v);
            });
        }
        public string ConvertArguObject(int index, object obj)
        {
            for (int convtrCount = arguConverters.Length; index >= 0 && (index >= convtrCount || arguConverters[index] == null); index--) ;
            if (index < 0)
                return ArguConverterManager.GetConverter<ArguConverter>().ConvertBack(obj);
            else
                return arguConverters[index].ConvertBack(obj);
        }

        /// <summary>
        /// Name of Command, default is same as Method name
        /// </summary>
        public string CommandName { get => commandName; set => commandName = value; }

        /// <summary>
        /// Alias of Command, default is null (disabled)
        /// </summary>
        public string CommandAlias { get => commandAlias; set => commandAlias = value; }
        /// <summary>
        /// Description about current command
        /// </summary>
        public string Description { get; set; }
    }
    /// <summary>
    /// Commnad parameter infomation
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    public sealed class CommandArguAttribute : Attribute
    {
        private string cmdArguName;
        private string cmdArguAlias;
        private string description = null;
        private object defaultValue;
        private bool isParameterArray;
        private Type parameterType;

        public CommandArguAttribute() { }
        public CommandArguAttribute(string desc)
        {
            Description = desc;
        }

        public CommandArguAttribute(ParameterInfo param)
        {
            LoadTarget(param);
        }

        public void LoadTarget(ParameterInfo info)
        {
            defaultValue = info.DefaultValue;
            isParameterArray = info.GetCustomAttribute<ParamArrayAttribute>() != null;
            parameterType = info.ParameterType;
            if (cmdArguName == null)
                cmdArguName = info.Name;
        }
        public bool HasDefaultValue => defaultValue != null;
        public bool IsParameterArray { get => isParameterArray; }
        public Type ParameterType { get => parameterType; }
        public bool IsCorrectName(string arguName, StringComparison stringComparison)
        {
            return arguName != null && (arguName.Equals(cmdArguName, stringComparison) || arguName.Equals(cmdArguAlias));
        }

        public string CommandArguName { get => cmdArguName; set => cmdArguName = value; }
        public string CommandArguAlias { get => cmdArguAlias; set => cmdArguAlias = value; }

        /// <summary>
        /// Description of Parameter
        /// </summary>
        public string Description { get => description; set => description = value; }
        /// <summary>
        /// Default value of Parameter
        /// </summary>
        public object DefaultValue => defaultValue;
    }
}