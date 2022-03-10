using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace NullLib.CommandLine
{
    public class CommandException : Exception
    {
        public CommandException() : base() { }
        public CommandException(string message) : base(message) { }
        public CommandException(string message, Exception innerException) : base(message, innerException) { }
        protected CommandException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
    public class CommandEntryPointNotFoundException : CommandException
    {
        public string MethodRequired { get; }

        public CommandEntryPointNotFoundException() : base() { }
        public CommandEntryPointNotFoundException(string methodRequired) : base()
        {
            MethodRequired = methodRequired;
        }
        public CommandEntryPointNotFoundException(string methodRequired, string message) : base(message)
        {
            MethodRequired = methodRequired;
        }
    }

    public class CommandOverrideNotFoundException : CommandEntryPointNotFoundException
    {
        public CommandOverrideNotFoundException() : base() { }
        public CommandOverrideNotFoundException(string methodRequired) : base(methodRequired) { }
        public CommandOverrideNotFoundException(string methodRequired, string message) : base(methodRequired, message) { }
    }

    public class CommandParameterFormatException : CommandException
    {
        public MethodInfo Method { get; }
        public CommandParameterFormatException() : base() { }
        public CommandParameterFormatException(MethodInfo method) : base()
        {
            Method = method;
        }
        public CommandParameterFormatException(MethodInfo method, string message) : base(message)
        {
            Method = method;
        }
        public CommandParameterFormatException(MethodInfo method, string message, Exception innerException) : base(message, innerException)
        {
            Method = method;
        }
    }
    public class CommandParameterConvertException : CommandException
    {
        public MethodInfo Method { get; }
        public CommandParameterConvertException() : base() { }
        public CommandParameterConvertException(MethodInfo method) : base()
        {
            Method = method;
        }
        public CommandParameterConvertException(MethodInfo method, string message) : base(message)
        {
            Method = method;
        }
        public CommandParameterConvertException(MethodInfo method, string message, Exception innerException) : base(message, innerException)
        {
            Method = method;
        }
    }
    public class CommandArgumentAssignException : CommandException
    {
        protected readonly IArgument argument;
        public IArgument Argument => argument;
        public int ArgumentIndex { get; }
        public CommandArgumentAssignException() : base() { }
        public CommandArgumentAssignException(int arguIndex, IArgument argu) : base()
        {
            ArgumentIndex = arguIndex;
            argument = argu;
        }
        public CommandArgumentAssignException(int arguIndex, IArgument argu, string message) : base(message)
        {
            ArgumentIndex = arguIndex;
            argument = argu;
        }
        public CommandArgumentAssignException(int arguIndex, IArgument argu, string message, Exception innerException) : base(message, innerException)
        {
            ArgumentIndex = arguIndex;
            argument = argu;
        }
    }
    public class CommandArguConverterNotFoundException : CommandException
    {
        public Type ParameterType { get; }

        public CommandArguConverterNotFoundException(Type parameterType) : base("Command argument converter not found.")
        {
            ParameterType = parameterType;
        }
    }
}
