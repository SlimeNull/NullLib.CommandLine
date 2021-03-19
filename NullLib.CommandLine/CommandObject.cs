using System;
using System.Linq;
using System.Reflection;

namespace NullLib.CommandLine
{
    public class CommandObject<T>
    {
        readonly T instance;
        MethodInfo[] methods;
        string[] methodNames;
        string[] methodUpperNames;

        private void InitializeInstance()
        {
            if (instance != null)
            {
                this.methods = typeof(T).GetMethods();
                this.methodNames = methods.Select((v) => v.Name.ToString()).ToArray();
                this.methodUpperNames = this.methods.Select((v) => v.Name.ToUpper()).ToArray();
            }
        }

        public CommandObject()
        {
            this.instance = Activator.CreateInstance<T>();

            InitializeInstance();
        }
        public CommandObject(T instance)
        {
            this.instance = instance;

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
        public static object[] ConvertArguments(MethodInfo method, object[] arguments)
        {
            object[] result = new object[arguments.Length];
            ParameterInfo[] paramInfos = method.GetParameters();

            if (method.GetCustomAttribute(typeof(CommandOptionAttribute)) is CommandOptionAttribute attribute)
            {
                var converters = attribute.ArgumentConverters;
                for (int i = 0, len = converters.Length; i < len; i++)
                {
                    Type objType = arguments[i].GetType();
                    if (paramInfos[i].ParameterType != objType)
                        if (objType == typeof(string))
                            result[i] = converters[i].Convert(arguments[i] as string);
                        else
                            throw new ArgumentOutOfRangeException("arguments", $"Object of type 'string' is requeired. Index: {i}");
                    else
                        result[i] = arguments;
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException("method", "Method must has attribute 'SetCommand'.");
            }

            return result;
        }
        public static object ExecuteCommand(T instance, MethodInfo method, string[] arguments)
        {
            object[] paramsForCalling = ConvertArguments(method, arguments);
            return method.Invoke(instance, paramsForCalling);
        }
        public static object ExecuteCommand(T instance, MethodInfo method, object arguments)
        {
            object[] paramsForCalling = GetArguments(method, arguments);
            paramsForCalling = ConvertArguments(method, paramsForCalling);
            return method.Invoke(instance, paramsForCalling);
        }

        public bool TryGetMethodInfo(string methodName, bool ignoreCases, out MethodInfo result)
        {
            string[] nameToSearch;

            if (ignoreCases)
            {
                methodName = methodName.ToUpper();
                nameToSearch = methodUpperNames;
            }
            else
            {
                nameToSearch = methodNames;
            }

            result = null;
            for (int i = 0, len = methods.Length; i < len; i++)
                if (string.Equals(methodName, nameToSearch[i]))
                    result = methods[i];

            return result != null;
        }

        public object ExecuteCommand(string methodName, string[] arguments, bool ignoreCases)
        {
            if (TryGetMethodInfo(methodName, ignoreCases, out MethodInfo toCall))
                return ExecuteCommand(instance, toCall, arguments);

            throw new EntryPointNotFoundException("Method not found.");
        }
        public object ExecuteCommand(string methodName, string[] arguments)
        {
            return ExecuteCommand(methodName, arguments, false);
        }
        public object ExecuteCommand(string methodName, object arguments, bool ignoreCases)
        {
            if (TryGetMethodInfo(methodName, ignoreCases, out MethodInfo toCall))
                return ExecuteCommand(instance, toCall, arguments);

            throw new EntryPointNotFoundException("Method not found.");
        }
        public object ExecuteCommand(string methodName, object arguments)
        {
            return ExecuteCommand(methodName, arguments, false);
        }
        public object ExecuteCommand(string[] commandLine, bool ignoreCases)
        {
            if (commandLine.Length < 1)
                throw new ArgumentOutOfRangeException("commandLine", "Length of this array must greater than 0");

            string methodName = commandLine[0];
            string[] arguments = new string[commandLine.Length - 1];
            Array.Copy(commandLine, 1, arguments, 0, arguments.Length);

            return ExecuteCommand(methodName, arguments, ignoreCases);
        }
        public object ExecuteCommand(string[] commandLine)
        {
            return ExecuteCommand(commandLine, false);
        }
        public object ExecuteCommand(MethodInfo method, string[] arguments)
        {
            return ExecuteCommand(instance, method, arguments);
        }
        public object ExecuteCommand(MethodInfo method, object arguments)
        {
            return ExecuteCommand(instance, method, arguments);
        }
        public bool TryExecuteCommand(string methodName, string[] arguments, bool ignoreCases, out object result)
        {
            try
            {
                result = ExecuteCommand(methodName, arguments, ignoreCases);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        public bool TryExecuteCommand(string methodName, string[] arguments,out object result)
        {
            return TryExecuteCommand(methodName, arguments, false, out result);
        }
        public bool TryExecuteCommand(string methodName, object arguments, bool ignoreCases, out object result)
        {
            try
            {
                result = ExecuteCommand(methodName, arguments, ignoreCases);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        public bool TryExecuteCommand(string methodName, object arguments, out object result)
        {
            return TryExecuteCommand(methodName, arguments, false, out result);
        }
        public bool TryExecuteCommand(string[] commandLine, bool ignoreCases, out object result)
        {
            try
            {
                result = ExecuteCommand(commandLine, ignoreCases);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        public bool TryExecuteCommand(string[] commandLine, out object result)
        {
            return TryExecuteCommand(commandLine, false, out result);
        }
        public bool TryExecuteCommand(MethodInfo method, string[] arguments, out object result)
        {
            try
            {
                result = ExecuteCommand(method, arguments);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
        public bool TryExecuteCommand(MethodInfo method, object arguments, out object result)
        {
            try
            {
                result = ExecuteCommand(method, arguments);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }
}
