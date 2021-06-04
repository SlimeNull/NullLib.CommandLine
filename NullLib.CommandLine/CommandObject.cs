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

        private void InitializeInstance(bool fastMode)
        {
            if (fastMode)
                CommandObjectManager.GetCommandObjectInfo(instanceType, out methods, out paramInfos, out attributes);
            else
                CommandObjectManager.NewCommandObjectInfo(instanceType, out methods, out paramInfos, out attributes);
        }

        public CommandObject(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            this.instance = instance;
            instanceType = instance.GetType();
            InitializeInstance(true);
        }
        public CommandObject(object instance, bool fastMode)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));
            this.instance = instance;
            instanceType = instance.GetType();
            InitializeInstance(fastMode);
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

        public bool CanExecuteCommand(IArgumentParser[] parsers, CommandLineSegment[] cmdline, bool ignoreCases)
        {
            CommandParser.SplitCommandInfo(cmdline, out var cmdname, out var cmdparams);
            var args = CommandParser.ParseArguments(parsers, cmdparams);
            return CommandInvoker.CanInvoke(methods, paramInfos, attributes, cmdname, args, ignoreCases ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        public bool CanExecuteCommand(IArgumentParser[] parsers, CommandLineSegment[] cmdline)
        {
            return CanExecuteCommand(parsers, cmdline, false);
        }
        public bool CanExecuteCommand(IArgumentParser[] parsers, string cmdline, bool ignoreCases)
        {
            CommandLineSegment[] cmdlineSegs = CommandParser.SplitCommandLine(cmdline);
            return CanExecuteCommand(parsers, cmdlineSegs, ignoreCases);
        }
        public bool CanExecuteCommand(IArgumentParser[] parsers, string cmdline)
        {
            CommandLineSegment[] cmdlineSegs = CommandParser.SplitCommandLine(cmdline);
            return CanExecuteCommand(parsers, cmdlineSegs, false);
        }
        public bool CanExecuteCommand(string cmdline, bool ignoreCases)
        {
            return CanExecuteCommand(CommandParser.DefaultParsers, cmdline, ignoreCases);
        }
        public bool CanExecuteCommand(string cmdline)
        {
            return CanExecuteCommand(CommandParser.DefaultParsers, cmdline, false);
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
    public static class CommandObjectManager
    {
        private static Dictionary<Type, CommandObjectInfo> cmdObjInfos = new Dictionary<Type, CommandObjectInfo>();

        public static bool HasInfo(Type type)
        {
            return cmdObjInfos.ContainsKey(type);
        }
        public static bool RemoveInfo(Type type)
        {
            return cmdObjInfos.Remove(type);
        }
        public static void GetCommandObjectInfo(Type type, out MethodInfo[] methods, out ParameterInfo[][] paramInfos, out CommandAttribute[] attributes)
        {
            if (cmdObjInfos.TryGetValue(type, out var cmdObjInfo))
            {
                methods = cmdObjInfo.Methods;
                paramInfos = cmdObjInfo.ParamInfos;
                attributes = cmdObjInfo.Attributes;
            }
            else
            {
                NewCommandObjectInfo(type, out methods, out paramInfos, out attributes);
                cmdObjInfos[type] = new CommandObjectInfo(methods, paramInfos, attributes);
            }
        }
        public static void NewCommandObjectInfo(Type type, out MethodInfo[] methods, out ParameterInfo[][] paramInfos, out CommandAttribute[] attributes)
        {
            List<MethodInfo> _methods = new List<MethodInfo>();
            List<ParameterInfo[]> _paramInfos = new List<ParameterInfo[]>();
            List<CommandAttribute> _attributes = new List<CommandAttribute>();

            foreach (var method in type.GetMethods())
            {
                CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute != null)
                {
                    ParameterInfo[] __paramInfos = method.GetParameters();
                    _methods.Add(method);
                    _attributes.Add(attribute);
                    _paramInfos.Add(__paramInfos);
                }
            }

            methods = _methods.ToArray();
            paramInfos = _paramInfos.ToArray();
            attributes = _attributes.ToArray();
        }

        class CommandObjectInfo
        {
            public MethodInfo[] Methods;
            public CommandAttribute[] Attributes;
            public ParameterInfo[][] ParamInfos;

            public CommandObjectInfo(Type classType)
            {
                GetCommandObjectInfo(classType, out this.Methods, out this.ParamInfos, out this.Attributes);
            }
            public CommandObjectInfo(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] attributes)
            {
                this.Methods = methods;
                this.ParamInfos = paramInfos;
                this.Attributes = attributes;
            }
        }
    }
}
