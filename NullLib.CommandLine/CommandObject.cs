using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NullLib.CommandLine
{
    /// <summary>
    /// CommandObject is used for calling method by command line.
    /// </summary>
    public class CommandObject
    {
        internal readonly object instance;
        readonly Type instanceType;
        private MethodInfo[] methods;
        private ParameterInfo[][] paramInfos;
        private PropertyInfo[] commandHosts;
        private CommandAttribute[] cmdAttrs;
        private CommandArguAttribute[][] paramAttrs;
        private CommandHostAttribute[] commandHostAttrs;

        /// <summary>
        /// Operation's target instance of current CommandObject
        /// </summary>
        public object TargetInstance => instance;
        /// <summary>
        /// Type of TargetInstance
        /// </summary>
        public Type IntanceType => instanceType;

        /// <summary>
        /// When calling ExecuteCommand(...), if cannot find entry point method for command, CommandUnresolved will be triggered
        /// </summary>
        public event EventHandler<CommandUnResolvedEventArgs> CommandUnresolved;

        private void InitializeInstance()
        {
            CommandObjectManager.GetCommandObjectInfo(instanceType, out methods, out paramInfos, out commandHosts, out cmdAttrs, out paramAttrs, out commandHostAttrs);
        }
        private void OnCommandUnresolved(CommandUnResolvedEventArgs args)
        {
            CommandUnresolved?.Invoke(this, args);
        }

        /// <summary>
        /// Initialize a CommandObject with specified instance
        /// </summary>
        /// <param name="instance"></param>
        public CommandObject(object instance)
        {
            this.instance = instance ?? throw new ArgumentNullException(nameof(instance));

            if (instance is CommandHome ncmd)
                ncmd.CommandObject = this;

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

        private bool CanExecuteCommandHost(string cmdname, IArgument[] args, StringComparison stringComparison)
        {
            for (int i = 0, iend = commandHostAttrs.Length; i < iend; i++)
            {
                CommandHostAttribute hostattr = commandHostAttrs[i];
                if (hostattr.IsCorrectName(cmdname, stringComparison))
                {
                    if (commandHosts[i].GetValue(instance) is CommandObject host)
                    {
                        CommandParser.SplitCommandInfo(args, out var _cmdname, out var _args);

                        if (host.CanExecuteCommand(_cmdname, _args, stringComparison))
                            return true;
                    }
                }
            }

            return false;
        }
        private bool TryExecuteCommandHost(string cmdname, IArgument[] args, StringComparison stringComparison, out object result)
        {
            for (int i = 0, iend = commandHostAttrs.Length; i < iend; i++)
            {
                CommandHostAttribute hostattr = commandHostAttrs[i];
                if (hostattr.IsCorrectName(cmdname, stringComparison))
                {
                    if (commandHosts[i].GetValue(instance) is CommandObject host)
                    {
                        CommandParser.SplitCommandInfo(args, out var _cmdname, out var _args);

                        if (host.TryExecuteCommand(_cmdname, _args, stringComparison, out result))
                            return true;
                    }
                }
            }

            result = null;
            return false;
        }
        internal object ExecuteCommand(string cmdname, IArgument[] args, StringComparison stringComparison,
            string cmdlinestr = null, CommandSegment[] cmdlineSegments = null, CommandSegment[] argsSegments = null)
        {
            object result;
            if (TryExecuteCommandHost(cmdname, args, stringComparison, out result))
                return result;
            if (CommandInvoker.TryInvoke(methods, cmdAttrs, paramAttrs, instance, cmdname, args, stringComparison, out result))
                return result;

            CommandUnResolvedEventArgs eArgs = new CommandUnResolvedEventArgs(cmdlinestr, cmdlineSegments, args, cmdname, argsSegments);
            OnCommandUnresolved(eArgs);
            if (!eArgs.Handled)
                throw new CommandEntryPointNotFoundException(cmdname);
            return null;
        }
        private object ExecuteCommand(IArguParser[] parsers, string cmdlinestr, CommandSegment[] cmdline, bool ignoreCases)
        {
            CommandParser.SplitCommandInfo(cmdline, out var cmdname, out var argsSegments);
            IArgument[] args = CommandParser.ParseArguments(parsers, argsSegments);
            StringComparison stringComparison = ignoreCases.GetStringComparison();

            return ExecuteCommand(cmdname, args, stringComparison, cmdlinestr, cmdline, argsSegments);
        }

        /// <summary>
        /// Execute command by specified parsers, commandline segments
        /// </summary>
        /// <param name="parsers"></param>
        /// <param name="cmdlineSegs"></param>
        /// <param name="ignoreCases">Ignore cases or not</param>
        /// <returns></returns>
        public object ExecuteCommand(IArguParser[] parsers, CommandSegment[] cmdlineSegs, bool ignoreCases)
        {
            CommandParser.SplitCommandInfo(cmdlineSegs, out var cmdname, out var argsSegments);
            IArgument[] args = CommandParser.ParseArguments(parsers, argsSegments);
            StringComparison stringComparison = ignoreCases.GetStringComparison();
            return ExecuteCommand(cmdname, args, stringComparison,
                null, cmdlineSegs, argsSegments);
        }
        /// <summary>
        /// Execute command by specified parsers, commandline segments, not ignore cases
        /// </summary>
        /// <param name="parsers"></param>
        /// <param name="cmdlineSegs"></param>
        /// <returns></returns>
        public object ExecuteCommand(IArguParser[] parsers, CommandSegment[] cmdlineSegs)
        {
            return ExecuteCommand(parsers, cmdlineSegs, false);
        }
        /// <summary>
        /// Execute command by specified parsers, commandline string
        /// </summary>
        /// <param name="parsers"></param>
        /// <param name="cmdline"></param>
        /// <param name="ignoreCases">Ignore cases or not</param>
        /// <returns></returns>
        public object ExecuteCommand(IArguParser[] parsers, string cmdline, bool ignoreCases)
        {
            CommandParser.SplitCommandLine(cmdline, out var cmdSegs);
            return ExecuteCommand(parsers, cmdline, cmdSegs, ignoreCases);
        }
        public object ExecuteCommand(IArguParser[] parsers, string cmdline)
        {
            return ExecuteCommand(parsers, cmdline, false);
        }
        public object ExecuteCommand(CommandSegment[] cmdline, bool ignoreCases)
        {
            return ExecuteCommand(CommandParser.DefaultParsers, null, cmdline, ignoreCases);
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

        internal bool CanExecuteCommand(string cmdname, IArgument[] args, StringComparison stringComparison)
        {
            if (CanExecuteCommandHost(cmdname, args, stringComparison))
                return true;
            return CommandInvoker.CanInvoke(cmdAttrs, paramAttrs, cmdname, args, stringComparison);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="parsers"></param>
        /// <param name="cmdlineSegs"></param>
        /// <param name="ignoreCases"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(IArguParser[] parsers, CommandSegment[] cmdlineSegs, bool ignoreCases)
        {
            CommandParser.SplitCommandInfo(cmdlineSegs, out var cmdname, out var cmdparams);
            var args = CommandParser.ParseArguments(parsers, cmdparams);
            StringComparison stringComparison = ignoreCases.GetStringComparison();

            return CanExecuteCommand(cmdname, args, stringComparison);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="parsers"></param>
        /// <param name="cmdlineSegs"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(IArguParser[] parsers, CommandSegment[] cmdlineSegs)
        {
            return CanExecuteCommand(parsers, cmdlineSegs, false);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="parsers"></param>
        /// <param name="cmdline"></param>
        /// <param name="ignoreCases"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(IArguParser[] parsers, string cmdline, bool ignoreCases)
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
        public bool CanExecuteCommand(IArguParser[] parsers, string cmdline)
        {
            CommandParser.SplitCommandLine(cmdline, out var cmdlineSegs);
            return CanExecuteCommand(parsers, cmdlineSegs, false);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="cmdlineSegs"></param>
        /// <param name="ignoreCases"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(CommandSegment[] cmdlineSegs, bool ignoreCases)
        {
            return CanExecuteCommand(CommandParser.DefaultParsers, cmdlineSegs, ignoreCases);
        }
        /// <summary>
        /// Check if specified cmdline can be executed
        /// </summary>
        /// <param name="cmdlineSegs"></param>
        /// <returns></returns>
        public bool CanExecuteCommand(CommandSegment[] cmdlineSegs)
        {
            return CanExecuteCommand(CommandParser.DefaultParsers, cmdlineSegs, false);
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

        internal bool TryExecuteCommand(string cmdname, IArgument[] args, StringComparison stringComparison, out object result)
        {
            if (TryExecuteCommandHost(cmdname, args, stringComparison, out result))
                return true;
            return CommandInvoker.TryInvoke(methods, cmdAttrs, paramAttrs, instance, cmdname, args, stringComparison, out result);
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
        public bool TryExecuteCommand(IArguParser[] parsers, CommandSegment[] cmdline, bool ignoreCases, out object result)
        {
            CommandParser.SplitCommandInfo(cmdline, out var cmdname, out var arguments);
            IArgument[] args = CommandParser.ParseArguments(parsers, arguments);
            StringComparison stringComparison = ignoreCases.GetStringComparison();

            return TryExecuteCommand(cmdname, args, stringComparison, out result);
        }
        public bool TryExecuteCommand(IArguParser[] parsers, CommandSegment[] cmdline, out object result)
        {
            return TryExecuteCommand(parsers, cmdline, false, out result);
        }
        public bool TryExecuteCommand(IArguParser[] parsers, string cmdline, bool ignoreCases, out object result)
        {
            CommandParser.SplitCommandLine(cmdline, out var cmdinfo);
            return TryExecuteCommand(parsers, cmdinfo, ignoreCases, out result);
        }
        public bool TryExecuteCommand(IArguParser[] parsers, string cmdline, out object result)
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

        public IEnumerable<IEnumerable<string>> GenCommandDetails(string cmdname, StringComparison stringComparison)
        {
            int cmdCount = cmdAttrs.Length;
            int hostCount = commandHostAttrs.Length;

            for (int i = 0; i < hostCount; i++)
            {
                if (commandHosts[i].GetValue(instance) is CommandObject cmdobj)
                {
                    CommandHostAttribute _hostAttr = commandHostAttrs[i];
                    if (_hostAttr.IsCorrectName(cmdname, stringComparison))
                    {
                        foreach (var def in cmdobj.GenCommandOverview())
                        {
                            yield return new string[] { _hostAttr.CommandName, ":" };
                            yield return new string[] { "  ", _hostAttr.GetDifinitionString() }.Concat(def);
                        }
                        yield break;
                    }
                }
            }

            for (int i = 0; i < cmdCount; i++)
            {
                CommandAttribute _cmdAttr = cmdAttrs[i];
                CommandArguAttribute[] _paramInfos = paramAttrs[i];

                if (_cmdAttr.IsCorrectName(cmdname, stringComparison))
                {
                    yield return new string[] { _cmdAttr.GetDifinitionString() }.Concat(_paramInfos.Select(v => v.GetDifinitionString()));
                    if (!string.IsNullOrWhiteSpace(_cmdAttr.Description))
                        yield return new string[] { "  -", _cmdAttr.Description };
                    foreach (var _paramInfo in _paramInfos)
                        if (!string.IsNullOrWhiteSpace(_paramInfo.Description))
                            yield return new string[] { "    -" }.Concat(new string[] { _paramInfo.CommandArguName, ":", _paramInfo.Description });
                }
            }
        }
        public string GenCommandDetailsText(string cmdname, StringComparison stringComparison)
        {
            return string.Join("\n", GenCommandDetails(cmdname, stringComparison).Select(v => string.Join(" ", v)));
        }
        public IEnumerable<IEnumerable<string>> GenCommandOverview()
        {
            int cmdCount = cmdAttrs.Length;
            int hostCount = commandHostAttrs.Length;

            for (int i = 0; i < hostCount; i++)
            {
                if (commandHosts[i].GetValue(instance) is CommandObject cmdobj)
                {
                    CommandHostAttribute _hostAttr = commandHostAttrs[i];

                    foreach (var def in cmdobj.GenCommandOverview())
                    {
                        yield return Enumerable.Repeat(_hostAttr.GetDifinitionString(), 1).Concat(def);
                    }
                }
            }

            for (int i = 0; i < cmdCount; i++)
            {
                CommandAttribute _cmdAttr = cmdAttrs[i];
                CommandArguAttribute[] _paramInfos = paramAttrs[i];

                yield return Enumerable.Repeat(_cmdAttr.GetDifinitionString(), 1).Concat(_paramInfos.Select(v => v.GetDifinitionString()));
            }
        }
        public string GenCommandOverviewText()
        {
            return string.Join("\n", GenCommandOverview().Select(v => string.Join(" ", v)));
        }
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
            base(Activator.CreateInstance<T>())
        { }
        /// <summary>
        /// Initialize an CommandObject instance, and set the param <paramref name="instance"/> as TargetInstance
        /// </summary>
        /// <param name="instance"></param>
        public CommandObject(T instance) :
            base(instance)
        { }

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
        private static Type cmdObjType = typeof(CommandObject);
        private static readonly Dictionary<Type, CommandObjectInfo> cmdObjInfos = new();

        /// <summary>
        /// Get types whose CommandObject info was intialized
        /// </summary>
        public static IEnumerable<Type> Keys => cmdObjInfos.Keys;
        /// <summary>
        /// Check if CommandObject info is exist by specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool HasInfo(Type type)
        {
            return cmdObjInfos.ContainsKey(type);
        }
        /// <summary>
        /// Remove existed CommandObject info by specified type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool RemoveInfo(Type type)
        {
            return cmdObjInfos.Remove(type);
        }
        /// <summary>
        /// Get from initialized CommandObject info or intialize new one when info not found
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methods"></param>
        /// <param name="paramInfos"></param>
        /// <param name="commandHosts"></param>
        /// <param name="methodAttributes"></param>
        /// <param name="paramAttributes"></param>
        /// <param name="commandHostAttributes"></param>
        public static void GetCommandObjectInfo(Type type,
            out MethodInfo[] methods, out ParameterInfo[][] paramInfos, out PropertyInfo[] commandHosts,
            out CommandAttribute[] methodAttributes, out CommandArguAttribute[][] paramAttributes, out CommandHostAttribute[] commandHostAttributes)
        {
            if (cmdObjInfos.TryGetValue(type, out var cmdObjInfo))
            {
                methods = cmdObjInfo.Methods;
                paramInfos = cmdObjInfo.ParamInfos;
                commandHosts = cmdObjInfo.CommandHosts;
                methodAttributes = cmdObjInfo.MethodAttributes;
                paramAttributes = cmdObjInfo.ParamAttributes;
                commandHostAttributes = cmdObjInfo.CommandHostAttributes;
            }
            else
            {
                NewCommandObjectInfo(type, out methods, out paramInfos, out commandHosts, out methodAttributes, out paramAttributes, out commandHostAttributes);
                cmdObjInfos[type] = new CommandObjectInfo(methods, paramInfos, commandHosts, methodAttributes, paramAttributes, commandHostAttributes);
            }
        }
        /// <summary>
        /// Initialize new CommandObject info for specified type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methods"></param>
        /// <param name="paramInfos"></param>
        /// <param name="commandHosts"></param>
        /// <param name="methodAttributes"></param>
        /// <param name="paramAttributes"></param>
        /// <param name="commandHostAttributes"></param>
        public static void NewCommandObjectInfo(Type type,
            out MethodInfo[] methods, out ParameterInfo[][] paramInfos, out PropertyInfo[] commandHosts,
            out CommandAttribute[] methodAttributes, out CommandArguAttribute[][] paramAttributes, out CommandHostAttribute[] commandHostAttributes)
        {
            List<MethodInfo> _methods = new();
            List<ParameterInfo[]> _paramInfos = new();
            List<PropertyInfo> _commandHosts = new();
            List<CommandAttribute> _cmdAttrs = new();
            List<CommandArguAttribute[]> _arguAttrs = new();
            List<CommandHostAttribute> _commandHostAttributes = new();

            foreach (var property in type.GetProperties())
            {
                CommandHostAttribute attribute = property.GetCustomAttribute<CommandHostAttribute>();
                if (attribute is not null)
                {
                    if (cmdObjType.IsAssignableFrom(property.PropertyType))
                    {
                        attribute.LoadTarget(property);

                        _commandHosts.Add(property);
                        _commandHostAttributes.Add(attribute);
                    }
                }
            }

            foreach (var method in type.GetMethods())
            {
                CommandAttribute attribute = method.GetCustomAttribute<CommandAttribute>();
                if (attribute is not null)
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
            commandHosts = _commandHosts.ToArray();
            methodAttributes = _cmdAttrs.ToArray();
            paramAttributes = _arguAttrs.ToArray();
            commandHostAttributes = _commandHostAttributes.ToArray();
        }
        struct CommandObjectInfo
        {
            public MethodInfo[] Methods;
            public ParameterInfo[][] ParamInfos;
            public PropertyInfo[] CommandHosts;
            public CommandAttribute[] MethodAttributes;
            public CommandArguAttribute[][] ParamAttributes;
            public CommandHostAttribute[] CommandHostAttributes;

            public CommandObjectInfo(
                MethodInfo[] methods, ParameterInfo[][] paramInfos, PropertyInfo[] commandHosts,
                CommandAttribute[] methodAttributes, CommandArguAttribute[][] paramAttributes, CommandHostAttribute[] commandHostAttributes)
            {
                this.Methods = methods;
                this.ParamInfos = paramInfos;
                this.CommandHosts = commandHosts;
                this.MethodAttributes = methodAttributes;
                this.ParamAttributes = paramAttributes;
                this.CommandHostAttributes = commandHostAttributes;
            }
        }
    }
}
