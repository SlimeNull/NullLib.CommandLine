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
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class CommandAttribute : Attribute
    {
        readonly IArguConverter[] arguConverters;

        /// <summary>
        /// ArgumentConverters for current command
        /// </summary>
        public IArguConverter[] ArgumentConverters { get => arguConverters; }

        /// <summary>
        /// Initialize a new instance of CommandAttribute with no special IArgumentConverter
        /// If your method only has string parameters, you can use this.
        /// </summary>
        public CommandAttribute()
        {
            arguConverters = new IArguConverter[0];
        }
        /// <summary>
        /// Initialize a new instance of CommandAttribute
        /// </summary>
        /// <param name="arguConverters"></param>
        public CommandAttribute(params Type[] arguConverters)
        {
            try
            {
                int convtrLen = arguConverters is not null ? arguConverters.Length : 0;
                this.arguConverters = new IArguConverter[convtrLen];
                for (int i = 0, end = convtrLen; i < end; i++)
                    this.arguConverters[i] = ArguConverterManager.GetConverter(arguConverters[i]);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw new ArgumentOutOfRangeException(nameof(arguConverters), $"Type must be assignable to {nameof(IArguConverter)}.");
            }
        }

        public void LoadTarget(MethodInfo info)
        {
            if (CommandName == null)
                CommandName = info.Name;
        }

        /// <summary>
        /// Check specified name is the correct name of current command
        /// </summary>
        /// <param name="cmdName">Name for checking</param>
        /// <param name="stringComparison">StringComparison</param>
        /// <returns></returns>
        public bool IsCorrectName(string cmdName, StringComparison stringComparison)
        {
            return cmdName != null && (cmdName.Equals(CommandName, stringComparison) || cmdName.Equals(CommandAlias, stringComparison));
        }
        public string GetDifinitionString(bool alias = false)
        {
            return alias ? CommandAlias : CommandName;
        }

        public IEnumerable<string> ConvertArguObjects(IEnumerable<object> objs)
        {
            IArguConverter curConvtr = ArguConverterManager.GetConverter<ArguConverter>();
            int converterCount = arguConverters.Length;
            return objs.Select((v, i) =>
            {
                if (i < converterCount && arguConverters[i] != null)
                    curConvtr = arguConverters[i];
                return curConvtr.ConvertBack(v);
            });
        }

        /// <summary>
        /// Name of Command, default is same as Method name
        /// </summary>
        public string CommandName { get; set; }
        /// <summary>
        /// Alias of Command, default is null (disabled)
        /// </summary>
        public string CommandAlias { get; set; }
        /// <summary>
        /// Description about current command
        /// </summary>
        public string Description { get; set; }
    }
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public sealed class CommandHostAttribute : Attribute
    {
        private string commandName;
        private string commandAlias;

        /// <summary>
        /// Initialize a new instance of CommandAttribute with no special IArgumentConverter
        /// If your method only has string parameters, you can use this.
        /// </summary>
        public CommandHostAttribute()
        {

        }

        public void LoadTarget(PropertyInfo info)
        {
            if (commandName == null)
                commandName = info.Name;
        }

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
        public string GetDifinitionString(bool alias = false)
        {
            return $"{(alias ? CommandAlias : CommandName)}";
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
        private bool hasDefaltValue;
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
            hasDefaltValue = info.HasDefaultValue;
            defaultValue = info.DefaultValue;
            isParameterArray = info.GetCustomAttribute<ParamArrayAttribute>() != null;
            parameterType = info.ParameterType;
            if (cmdArguName == null)
                cmdArguName = info.Name;
        }
        public bool HasDefaultValue => hasDefaltValue;
        public bool IsParameterArray { get => isParameterArray; }
        public Type ParameterType { get => parameterType; }
        public bool IsCorrectName(string arguName, StringComparison stringComparison)
        {
            return arguName != null && (
                arguName.Equals(cmdArguName, stringComparison) || 
                arguName.Equals(cmdArguAlias));
        }

        public string GetDifinitionString(bool alias = false)
        {
            return $"{(IsParameterArray ? "*" : null)}{(alias ? CommandArguAlias : CommandArguName)}:{ParameterType.Name}";
        }

        /// <summary>
        /// Argument name (default save as current method parameter)
        /// </summary>
        public string CommandArguName { get => cmdArguName; set => cmdArguName = value; }
        /// <summary>
        /// Argument alias (default no alias for this argument)
        /// </summary>
        public string CommandArguAlias { get => cmdArguAlias; set => cmdArguAlias = value; }

        /// <summary>
        /// Description of Argument
        /// </summary>
        public string Description { get => description; set => description = value; }
        /// <summary>
        /// Default value of parameter
        /// </summary>
        public object DefaultValue => defaultValue;
    }
}