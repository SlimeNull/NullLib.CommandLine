using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace NullLib.CommandLine
{
    /// <summary>
    /// CommandObject is used for calling method by command line.
    /// </summary>
    public class CommandObject
    {
        protected readonly object instance;
        Type instanceType;
        MethodInfo[] methods;
        ParameterInfo[][] paramInfos;
        CommandAttribute[] attributes;

        public virtual object TargetInstance => instance;

        private void InitializeInstance()
        {
            List<MethodInfo> methods = new List<MethodInfo>();
            List<ParameterInfo[]> paramInfos = new List<ParameterInfo[]>();
            List<CommandAttribute> attributes = new List<CommandAttribute>();

            foreach (var method in instanceType.GetMethods())
            {
                CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute != null)
                {
                    ParameterInfo[] _paramInfos = method.GetParameters();
                    methods.Add(method);
                    attributes.Add(attribute);
                    paramInfos.Add(_paramInfos);
                }
            }

            this.methods = methods.ToArray();
            this.attributes = attributes.ToArray();
            this.paramInfos = paramInfos.ToArray();
        }

        public CommandObject(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            this.instance = instance;
            instanceType = instance.GetType();
            InitializeInstance();
        }

        public static object[] GetArguments(MethodInfo method, object instance)
        {
            Type arguObjType = instance.GetType();
            ParameterInfo[] methodParams = method.GetParameters();
            object[] paramsForCalling = new object[methodParams.Length];

            for (int i = 0, len = methodParams.Length; i < len; i++)
            {
                string paramName = methodParams[i].Name;

                FieldInfo fieldInfo = arguObjType.GetField(paramName);
                if (fieldInfo != null)
                {
                    paramsForCalling[i] = fieldInfo.GetValue(instance);
                    continue;
                }
                PropertyInfo propertyInfo = arguObjType.GetProperty(paramName);
                if (propertyInfo != null)
                {
                    paramsForCalling[i] = propertyInfo.GetValue(instance);
                    continue;
                }

                throw new ArgumentOutOfRangeException("arguments", $"Can not find field or property '{paramName}'");
            }

            return paramsForCalling;
        }

        public object ExecuteCommand(IArgumentParser[] parsers, CommandLineSegment[] cmdline, bool ignoreCases)
        {
            CommandParser.SplitCommandInfo(cmdline, out var cmdname, out var arguments);
            IArgument[] args = CommandParser.ParseArguments(parsers, arguments);
            return CommandInvoker.Invoke(methods, paramInfos, attributes, instance, cmdname, args, ignoreCases ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        public object ExecuteCommand(IArgumentParser[] parsers, CommandLineSegment[] cmdline)
        {
            return ExecuteCommand(parsers, cmdline, false);
        }
        public object ExecuteCommand(IArgumentParser[] parsers, string cmdline, bool ignoreCases)
        {
            CommandLineSegment[] cmdinfo = CommandParser.SplitCommandLine(cmdline);
            return ExecuteCommand(parsers, cmdinfo, ignoreCases);
        }
        public object ExecuteCommand(IArgumentParser[] parsers, string cmdline)
        {
            return ExecuteCommand(parsers, cmdline, false);
        }
        public object ExecuteCommand(string cmdline, bool ignoreCases)
        {
            return ExecuteCommand(CommandParser.DefaultParsers, cmdline, ignoreCases);
        }
        public object ExecuteCommand(string cmdline)
        {
            return ExecuteCommand(CommandParser.DefaultParsers, cmdline, false);
        }
        public bool TryExecuteCommand(IArgumentParser[] parsers, CommandLineSegment[] cmdline, bool ignoreCases, out object result)
        {
            CommandParser.SplitCommandInfo(cmdline, out var cmdname, out var arguments);
            IArgument[] args = CommandParser.ParseArguments(parsers, arguments);
            return CommandInvoker.TryInvoke(methods, paramInfos, attributes, instance, cmdname, args, ignoreCases ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal, out result);
        }
        public bool TryExecuteCommand(IArgumentParser[] parsers, CommandLineSegment[] cmdline, out object result)
        {
            return TryExecuteCommand(parsers, cmdline, false, out result);
        }
        public bool TryExecuteCommand(IArgumentParser[] parsers, string cmdline, bool ignoreCases, out object result)
        {
            CommandLineSegment[] cmdinfo = CommandParser.SplitCommandLine(cmdline);
            return TryExecuteCommand(parsers, cmdinfo, ignoreCases, out result);
        }
        public bool TryExecuteCommand(IArgumentParser[] parsers, string cmdline, out object result)
        {
            return TryExecuteCommand(parsers, cmdline, false, out result);
        }
        public bool TryExecuteCommand(string cmdline, bool ignoreCases, out object result)
        {
            return TryExecuteCommand(CommandParser.DefaultParsers, cmdline, ignoreCases, out result);
        }
        public bool TryExecuteCommand(string cmdline, out object result)
        {
            return TryExecuteCommand(CommandParser.DefaultParsers, cmdline, false, out result);
        }
    }
    public class CommandObject<T> : CommandObject where T : class
    {
        /// <summary>
        /// Initialize an CommandObject instance, and set the TargetInstance property as a new instance initialized by the default constructor of <typeparamref name="T"/>
        /// </summary>
        public CommandObject() :
            base(Activator.CreateInstance<T>()) { }
        /// <summary>
        /// Initialize an CommandObject instance, and set the param <paramref name="instance"/> as TargetInstance
        /// </summary>
        /// <param name="instance"></param>
        public CommandObject(T instance) :
            base(instance) { }
        public new T TargetInstance { get => instance as T; }
    }
}
