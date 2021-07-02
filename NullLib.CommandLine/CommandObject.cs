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
        internal readonly object instance;
        readonly Type instanceType;
        MethodInfo[] methods;
        ParameterInfo[][] paramInfos;
        CommandAttribute[] methodAttributes;
        CommandArguAttribute[][] paramAttributes;

        /// <summary>
        /// Operation's target instance of current CommandObject
        /// </summary>
        public virtual object TargetInstance => instance;

        private void InitializeInstance()
        {
            CommandObjectManager.GetCommandObjectInfo(instanceType, out methods, out paramInfos, out methodAttributes, out paramAttributes);
        }

        /// <summary>
        /// Initialize a CommandObject with specified instance
        /// </summary>
        /// <param name="instance"></param>
        public CommandObject(object instance)
        {
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));

            instanceType = instance.GetType();
            InitializeInstance();
        }

        /// <summary>
        /// Find arguments from specified instance for calling specified method
        /// </summary>
        /// <param name="method">Method for calling</param>
        /// <param name="instance">Instance for finding</param>
        /// <returns></returns>
        public static object[] FindArguments(MethodInfo method, object instance)
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

        public object ExecuteCommand(IArgumentParser[] parsers, CommandSegment[] cmdline, bool ignoreCases)
        {
            CommandParser.SplitCommandInfo(cmdline, out var cmdname, out var arguments);
            IArgument[] args = CommandParser.ParseArguments(parsers, arguments);
            return CommandInvoker.Invoke(methods, methodAttributes, paramAttributes, instance, cmdname, args, ignoreCases ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        public object ExecuteCommand(IArgumentParser[] parsers, CommandSegment[] cmdline)
        {
            return ExecuteCommand(parsers, cmdline, false);
        }
        public object ExecuteCommand(IArgumentParser[] parsers, string cmdline, bool ignoreCases)
        {
            CommandParser.SplitCommandLine(cmdline, out var cmdinfo);
            return ExecuteCommand(parsers, cmdinfo, ignoreCases);
        }
        public object ExecuteCommand(IArgumentParser[] parsers, string cmdline)
        {
            return ExecuteCommand(parsers, cmdline, false);
        }
        public object ExecuteCommand(CommandSegment[] cmdline, bool ignoreCases)
        {
            return ExecuteCommand(CommandParser.DefaultParsers, cmdline, ignoreCases);
        }
        public object ExecuteCommand(CommandSegment[] cmdline)
        {
            return ExecuteCommand(CommandParser.DefaultParsers, cmdline, false);
        }
        public object ExecuteCommand(string cmdline, bool ignoreCases)
        {
            return ExecuteCommand(CommandParser.DefaultParsers, cmdline, ignoreCases);
        }
        /// <summary>
        /// ExecuteCommand with <typeref name="cmdline"/>, defalt parsers, not ignore cases
        /// </summary>
        /// <param name="cmdline"></param>
        /// <returns></returns>
        public object ExecuteCommand(string cmdline)
        {
            return ExecuteCommand(CommandParser.DefaultParsers, cmdline, false);
        }

        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="parsers"></param>
        /// <param name="cmdline"></param>
        /// <param name="ignoreCases"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(IArgumentParser[] parsers, CommandSegment[] cmdline, bool ignoreCases)
        {
            CommandParser.SplitCommandInfo(cmdline, out var cmdname, out var cmdparams);
            var args = CommandParser.ParseArguments(parsers, cmdparams);
            return CommandInvoker.CanInvoke(methods, methodAttributes, paramAttributes, cmdname, args, ignoreCases ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="parsers"></param>
        /// <param name="cmdline"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(IArgumentParser[] parsers, CommandSegment[] cmdline)
        {
            return CanExecuteCommand(parsers, cmdline, false);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="parsers"></param>
        /// <param name="cmdline"></param>
        /// <param name="ignoreCases"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(IArgumentParser[] parsers, string cmdline, bool ignoreCases)
        {
            CommandParser.SplitCommandLine(cmdline, out var cmdlineSegs);
            return CanExecuteCommand(parsers, cmdlineSegs, ignoreCases);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="parsers"></param>
        /// <param name="cmdline"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(IArgumentParser[] parsers, string cmdline)
        {
            CommandParser.SplitCommandLine(cmdline, out var cmdlineSegs);
            return CanExecuteCommand(parsers, cmdlineSegs, false);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="cmdline"></param>
        /// <param name="ignoreCases"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(CommandSegment[] cmdline, bool ignoreCases)
        {
            return CanExecuteCommand(CommandParser.DefaultParsers, cmdline, ignoreCases);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="cmdline"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(CommandSegment[] cmdline)
        {
            return CanExecuteCommand(CommandParser.DefaultParsers, cmdline, false);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="cmdline"></param>
        /// <param name="ignoreCases"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(string cmdline, bool ignoreCases)
        {
            return CanExecuteCommand(CommandParser.DefaultParsers, cmdline, ignoreCases);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="cmdline"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(string cmdline)
        {
            return CanExecuteCommand(CommandParser.DefaultParsers, cmdline, false);
        }

        /// <summary>
        /// Try to execute specified cmdline
        /// </summary>
        /// <param name="parsers"></param>
        /// <param name="cmdline"></param>
        /// <param name="ignoreCases"></param>
        /// <param name="result"></param>
        /// <exception cref="CommandEntryPointNotFoundException">Cannot find appropriate method to invoke</exception>
        /// <exception cref="CommandParameterFormatException">Cannot format cmdline to required parameters</exception>
        /// <exception cref="CommandParameterConvertException">Cannot convert cmdline argument to required parameter type</exception>
        /// <exception cref="TargetInvocationException"></exception>
        /// <returns></returns>
        public bool TryExecuteCommand(IArgumentParser[] parsers, CommandSegment[] cmdline, bool ignoreCases, out object result)
        {
            CommandParser.SplitCommandInfo(cmdline, out var cmdname, out var arguments);
            IArgument[] args = CommandParser.ParseArguments(parsers, arguments);
            return CommandInvoker.TryInvoke(methods, methodAttributes, paramAttributes, instance, cmdname, args, ignoreCases ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal, out result);
        }
        public bool TryExecuteCommand(IArgumentParser[] parsers, CommandSegment[] cmdline, out object result)
        {
            return TryExecuteCommand(parsers, cmdline, false, out result);
        }
        public bool TryExecuteCommand(IArgumentParser[] parsers, string cmdline, bool ignoreCases, out object result)
        {
            CommandParser.SplitCommandLine(cmdline, out var cmdinfo);
            return TryExecuteCommand(parsers, cmdinfo, ignoreCases, out result);
        }
        public bool TryExecuteCommand(IArgumentParser[] parsers, string cmdline, out object result)
        {
            return TryExecuteCommand(parsers, cmdline, false, out result);
        }
        public bool TryExecuteCommand(CommandSegment[] cmdline, bool ignoreCases, out object result)
        {
            return TryExecuteCommand(CommandParser.DefaultParsers, cmdline, ignoreCases, out result);
        }
        public bool TryExecuteCommand(CommandSegment[] cmdline, out object result)
        {
            return TryExecuteCommand(CommandParser.DefaultParsers, cmdline, false, out result);
        }
        public bool TryExecuteCommand(string cmdline, bool ignoreCases, out object result)
        {
            return TryExecuteCommand(CommandParser.DefaultParsers, cmdline, ignoreCases, out result);
        }
        public bool TryExecuteCommand(string cmdline, out object result)
        {
            return TryExecuteCommand(CommandParser.DefaultParsers, cmdline, false, out result);
        }


        protected string GenCommandDefinition(IArgumentParser parser, int index, out string cmdName, out string[] args)
        {
            cmdName = methods[index].Name;
            string[] defaultValues = methodAttributes[index].ConvertArguObjects(paramAttributes[index].Select(v => v.DefaultValue)).ToArray();
            args = paramAttributes[index].Select((v, i) => parser.FormatArgu(v.CommandArguName, defaultValues[i])).ToArray();
            return $"{cmdName} {string.Join(" ", args)}";
        }
        //protected bool GenMethodOverviewInfo(string name, StringComparison stringComparison, out string result)
        //{
        //    for (int i = 0, end = methods.Length; i < end; i++)
        //    {
        //        if (methods[i].Name.Equals(name, stringComparison))
        //        {
        //        }
        //    }
        //}
    }
    /// <summary>
    /// CommandObject is used for calling method by command line.
    /// </summary>
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

        /// <summary>
        /// Operation's target instance of current CommnadObject
        /// </summary>
        public new T TargetInstance { get => instance as T; }
    }
    /// <summary>
    /// CommandObject information manager
    /// </summary>
    public static class CommandObjectManager
    {
        private static readonly Dictionary<Type, CommandObjectInfo> cmdObjInfos = new();

        public static IEnumerable<Type> Keys => cmdObjInfos.Keys;
        public static bool HasInfo(Type type)
        {
            return cmdObjInfos.ContainsKey(type);
        }
        public static bool RemoveInfo(Type type)
        {
            return cmdObjInfos.Remove(type);
        }
        public static void GetCommandObjectInfo(Type type, out MethodInfo[] methods, out ParameterInfo[][] paramInfos, out CommandAttribute[] methodAttributes, out CommandArguAttribute[][] paramAttributes)
        {
            if (cmdObjInfos.TryGetValue(type, out var cmdObjInfo))
            {
                methods = cmdObjInfo.Methods;
                paramInfos = cmdObjInfo.ParamInfos;
                methodAttributes = cmdObjInfo.MethodAttributes;
                paramAttributes = cmdObjInfo.ParamAttributes;
            }
            else
            {
                NewCommandObjectInfo(type, out methods, out paramInfos, out methodAttributes, out paramAttributes);
                cmdObjInfos[type] = new CommandObjectInfo(methods, paramInfos, methodAttributes, paramAttributes);
            }
        }
        public static void NewCommandObjectInfo(Type type, out MethodInfo[] methods, out ParameterInfo[][] paramInfos, out CommandAttribute[] methodAttributes, out CommandArguAttribute[][] paramAttributes)
        {
            List<MethodInfo> _methods = new();
            List<ParameterInfo[]> _paramInfos = new();
            List<CommandAttribute> _cmdAttrs = new();
            List<CommandArguAttribute[]> _arguAttrs = new();

            foreach (var method in type.GetMethods())
            {
                CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute != null)
                {
                    attribute.LoadTarget(method);
                    ParameterInfo[] __paramInfos = method.GetParameters();
                    CommandArguAttribute[] __arguAttributes = new CommandArguAttribute[__paramInfos.Length];
                    
                    for (int i = 0, end = __paramInfos.Length; i < end; i++)
                    {
                        ParameterInfo curParam = __paramInfos[i];
                        CommandArguAttribute curAttr = curParam.GetCustomAttribute<CommandArguAttribute>() ?? new CommandArguAttribute();
                        __arguAttributes[i] = curAttr;
                        curAttr.LoadTarget(curParam);
                    }

                    _methods.Add(method);
                    _cmdAttrs.Add(attribute);
                    _paramInfos.Add(__paramInfos);
                    _arguAttrs.Add(__arguAttributes);
                }
            }

            methods = _methods.ToArray();
            paramInfos = _paramInfos.ToArray();
            methodAttributes = _cmdAttrs.ToArray();
            paramAttributes = _arguAttrs.ToArray();
        }

        struct CommandObjectInfo
        {
            public MethodInfo[] Methods;
            public ParameterInfo[][] ParamInfos;
            public CommandAttribute[] MethodAttributes;
            public CommandArguAttribute[][] ParamAttributes;

            public CommandObjectInfo(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] methodAttributes, CommandArguAttribute[][] paramAttributes)
            {
                this.Methods = methods;
                this.ParamInfos = paramInfos;
                this.MethodAttributes = methodAttributes;
                this.ParamAttributes = paramAttributes;
            }
        }
    }
}
