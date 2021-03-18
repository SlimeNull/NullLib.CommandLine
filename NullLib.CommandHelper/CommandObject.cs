using System;
using System.Linq;
using System.Reflection;

namespace NullLib.CommandLine
{
    public class CommandObject<T>
    {
        T instance;
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

        public static object ExecuteCommand(T instance, MethodInfo method, string[] arguments)
        {
            object[] paramsForCall = arguments.Select((v) => (object)v).ToArray();

            NCommandAttribute attribute = method.GetCustomAttribute(typeof(NCommandAttribute)) as NCommandAttribute;
            if (attribute != null)
            {
                var converters = attribute.ArgumentConverters;
                for (int i = 0, len = converters.Length; i < len; i++)
                    paramsForCall[i] = converters[i].Convert(arguments[i]);
            }

            return method.Invoke(instance, paramsForCall);
        }

        public object ExecuteCommand(string[] commandLine, bool ignoreCases)
        {
            if (commandLine.Length < 1)
                throw new ArgumentOutOfRangeException("commandLine");

            string commandName = commandLine[0];
            string[] commandParams = new string[commandLine.Length - 1];
            Array.Copy(commandLine, 1, commandParams, 0, commandParams.Length);

            if (ignoreCases)
            {
                commandName = commandName.ToUpper();
                for (int i = 0, len = methods.Length; i < len; i++)
                {
                    if (string.Equals(commandName, methodUpperNames[i]))
                    {
                        return ExecuteCommand(instance, methods[i], commandParams);
                    }
                }
            }
            else
            {
                for (int i = 0, len = methods.Length; i < len; i++)
                {
                    if (string.Equals(commandName, methodNames[i]))
                    {
                        return ExecuteCommand(instance, methods[i], commandParams);
                    }
                }
            }

            throw new EntryPointNotFoundException("Method not found.");
        }
        public object ExecuteCommand(string[] commandLine)
        {
            return ExecuteCommand(commandLine, false);
        }
        public object ExecuteCommand(MethodInfo method, string[] arguments)
        {
            return ExecuteCommand(instance, method, arguments);
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
    }
}
