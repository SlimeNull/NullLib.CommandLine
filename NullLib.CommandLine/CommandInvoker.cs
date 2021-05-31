using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NullLib.CommandLine
{
    public static class CommandInvoker
    {
        public static bool TryFormatArguments(ParameterInfo[] paramInfos, IArgument[] args, StringComparison stringComparison, out INamedArgument[] result)
        {
            result = null;
            if (paramInfos.Length != args.Length)
                return false;

            result = new INamedArgument[args.Length];
            ParameterInfo lastParamInfo = null;
            if (args.Length < 1 || (lastParamInfo = paramInfos[args.Length - 1]).GetCustomAttribute(typeof(ParamArrayAttribute)) == null)
            {
                for (int i = 0, iend = args.Length; i < iend; i++)
                {
                    IArgument argu = args[i];
                    if (argu is INamedArgument namedArgu)
                    {
                        bool assigned = false;
                        for (int j = 0, jend = paramInfos.Length; j < jend; j++)
                        {
                            ParameterInfo param = paramInfos[j];
                            if (param.Name.Equals(namedArgu.Name, stringComparison))
                            {
                                result[j] = namedArgu;
                                assigned = true;
                                break;
                            }
                        }

                        if (!assigned)
                            return false;
                    }
                    else
                    {
                        bool assigned = false;
                        for (int j = 0, end = args.Length; j < end; j++)
                        {
                            if (result[j] == null)
                            {
                                ParameterInfo curParamInfo = paramInfos[j];
                                result[j] = new NamedArgument(curParamInfo.Name, args[j].Content);
                                assigned = true;
                                break;
                            }
                        }

                        if (!assigned)
                            return false;
                    }
                }
            }
            else
            {
                List<IArgument> arrparams = new List<IArgument>();

                for (int i = 0, iend = args.Length; i < iend; i++)
                {
                    IArgument argu = args[i];
                    if (argu is INamedArgument namedArgu)
                    {
                        bool assigned = false;
                        for (int j = 0, jend = paramInfos.Length - 1; j < jend; j++)
                        {
                            ParameterInfo param = paramInfos[j];
                            if (param.Name.Equals(namedArgu.Name, stringComparison))
                            {
                                result[j] = namedArgu;
                                assigned = true;
                                break;
                            }
                        }

                        if (!assigned)
                            return false;
                    }
                    else
                    {
                        bool assigned = false;
                        for (int j = 0, end = args.Length; j < end; j++)
                        {
                            if (result[j] == null)
                            {
                                ParameterInfo curParamInfo = paramInfos[j];
                                result[j] = new NamedArgument(curParamInfo.Name, args[j].Content);
                                assigned = true;
                                break;
                            }
                        }

                        if (!assigned)
                            arrparams.Add(argu);
                    }
                }

                result[result.Length - 1] = new NamedArgument() { Name = lastParamInfo.Name, ValueObj = arrparams.ToArray() };
            }

            return true;
        }
        public static bool TryFormatArguments(ParameterInfo[] paramInfos, IArgument[] args, out INamedArgument[] result)
        {
            return TryFormatArguments(paramInfos, args, StringComparison.Ordinal, out result);
        }
        public static bool TryConvertArguments(IArgumentConverter[] converters, INamedArgument[] arguments, out INamedArgument[] result)
        {
            result = null;
            if (converters.Length != arguments.Length)
                return false;

            result = new INamedArgument[arguments.Length];
            for (int i = 0, end = arguments.Length; i < end; i++)
            {
                INamedArgument curArgu = arguments[i];
                IArgumentConverter curConvtr = converters[i];
                if (!curConvtr.TryConvert(curArgu.Content, out var valueObj))
                    return false;
                curArgu.ValueObj = valueObj;
                result[i] = curArgu;
            }
            return true;
        }

        public static INamedArgument[] FormatArguments(ParameterInfo[] paramInfos, IArgument[] args, StringComparison stringComparison)
        {
            if (TryFormatArguments(paramInfos, args, stringComparison, out var result))
                return result;
            else
                throw new ArgumentOutOfRangeException(nameof(args), "Arguments not match ParameterInfos");
        }
        public static INamedArgument[] FormatArguments(ParameterInfo[] paramInfos, IArgument[] args)
        {
            return FormatArguments(paramInfos, args, StringComparison.Ordinal);
        }
        public static INamedArgument[] ConvertArguments(IArgumentConverter[] converters, INamedArgument[] arguments)
        {
            if (TryConvertArguments(converters, arguments, out var result))
                return result;
            else
                throw new ArgumentOutOfRangeException(nameof(arguments), "Arguments cannot be converted by specified converters");
        }

        public static bool TryInvoke(MethodInfo method, ParameterInfo[] paramInfos, object instance, IArgument[] args, StringComparison stringComparison, out object result)
        {
            return TryInvoke(method, paramInfos, method.GetCustomAttribute<CommandAttribute>(), instance, args, stringComparison, out result);
        }
        public static bool TryInvoke(MethodInfo method, ParameterInfo[] paramInfos, object instance, IArgument[] args, out object result)
        {
            return TryInvoke(method, paramInfos, instance, args, StringComparison.Ordinal, out result);
        }
        public static bool TryInvoke(MethodInfo method, object instance, IArgument[] args, StringComparison stringComparison, out object result)
        {
            return TryInvoke(method, method.GetParameters(), instance, args, stringComparison, out result);
        }
        public static bool TryInvoke(MethodInfo method, object instance, IArgument[] args, out object result)
        {
            return TryInvoke(method, method.GetParameters(), instance, args, StringComparison.Ordinal, out result);
        }

        public static object Invoke(MethodInfo method, ParameterInfo[] paramInfos, object instance, IArgument[] args, StringComparison stringComparison)
        {
            return Invoke(method, paramInfos, method.GetCustomAttribute<CommandAttribute>(), instance, args, stringComparison);
        }
        public static object Invoke(MethodInfo method, ParameterInfo[] paramInfos, object instance, IArgument[] args)
        {
            return Invoke(method, paramInfos, instance, args, StringComparison.Ordinal);
        }
        public static object Invoke(MethodInfo method, object instance, IArgument[] args, StringComparison stringComparison)
        {
            return Invoke(method, method.GetParameters(), instance, args, stringComparison);
        }
        public static object Invoke(MethodInfo method, object instance, IArgument[] args)
        {
            return Invoke(method, method.GetParameters(), instance, args, StringComparison.Ordinal);
        }

        public static bool TryInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, object instance, string methodName, IArgument[] args, StringComparison stringComparison, out object result)
        {
            CommandAttribute[] attributes = new CommandAttribute[methods.Length];
            for (int i = 0, end = methods.Length; i < end; i++)
                attributes[i] = methods[i].GetCustomAttribute<CommandAttribute>();
            return TryInvoke(methods, paramInfos, attributes, instance, methodName, args, stringComparison, out result);
        }
        public static bool TryInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, object instance, string methodName, IArgument[] args, out object result)
        {
            return TryInvoke(methods, paramInfos, instance, methodName, args, StringComparison.Ordinal, out result);
        }
        public static bool TryInvoke(MethodInfo[] methods, object instance, string methodName, IArgument[] args, StringComparison stringComparison, out object result)
        {
            ParameterInfo[][] paramInfos = new ParameterInfo[methods.Length][];
            for (int i = 0, end = methods.Length; i < end; i++)
                paramInfos[i] = methods[i].GetParameters();
            return TryInvoke(methods, paramInfos, instance, methodName, args, stringComparison, out result);
        }
        public static bool TryInvoke(MethodInfo[] methods, object instance, string methodName, IArgument[] args, out object result)
        {
            return TryInvoke(methods, instance, methodName, args, StringComparison.Ordinal, out result);
        }

        public static object Invoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, object instance, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            CommandAttribute[] attributes = new CommandAttribute[methods.Length];
            for (int i = 0, end = methods.Length; i < end; i++)
                attributes[i] = methods[i].GetCustomAttribute<CommandAttribute>();
            return Invoke(methods, paramInfos, attributes, instance, methodName, args, stringComparison);
        }
        public static object Invoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, object instance, string methodName, IArgument[] args)
        {
            return Invoke(methods, paramInfos, instance, methodName, args, StringComparison.Ordinal);
        }
        public static object Invoke(MethodInfo[] methods, object instance, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            ParameterInfo[][] paramInfos = new ParameterInfo[methods.Length][];
            for (int i = 0, end = methods.Length; i < end; i++)
                paramInfos[i] = methods[i].GetParameters();
            return Invoke(methods, paramInfos, instance, methodName, args, stringComparison);
        }
        public static object Invoke(MethodInfo[] methods, object instance, string methodName, IArgument[] args)
        {
            return Invoke(methods, instance, methodName, args, StringComparison.Ordinal);
        }


        public static bool TryInvoke(MethodInfo method, ParameterInfo[] paramInfos, CommandAttribute attribute, object instance, IArgument[] args, StringComparison stringComparison, out object result)
        {
            result = null;

            if (!TryFormatArguments(paramInfos, args, stringComparison, out INamedArgument[] formatedArgs))
                return false;
            if (!TryConvertArguments(attribute.ArgumentConverters, formatedArgs, out INamedArgument[] convertedArgs))
                return false;
            object[] methodParamObjs = GetArgumentObjects(convertedArgs);
            try
            {
                result = method.Invoke(instance, methodParamObjs);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public static bool TryInvoke(MethodInfo method, ParameterInfo[] paramInfos, CommandAttribute attribute, object instance, IArgument[] args, out object result)
        {
            return TryInvoke(method, paramInfos, attribute, instance, args, StringComparison.Ordinal, out result);
        }

        public static object Invoke(MethodInfo method, ParameterInfo[] paramInfos, CommandAttribute attribute, object instance, IArgument[] args, StringComparison stringComparison)
        {
            INamedArgument[] formatedArgs = FormatArguments(paramInfos, args, stringComparison);
            INamedArgument[] convertedArgs = ConvertArguments(attribute.ArgumentConverters, formatedArgs);
            object[] methodParamObjs = GetArgumentObjects(convertedArgs);
            return method.Invoke(instance, methodParamObjs);
        }
        public static object Invoke(MethodInfo method, ParameterInfo[] paramInfos, CommandAttribute attribute, object instance, IArgument[] args)
        {
            return Invoke(method, paramInfos, attribute, instance, args, StringComparison.Ordinal);
        }

        public static bool TryInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] attributes, object instance, string methodName, IArgument[] args, StringComparison stringComparison, out object result)
        {
            result = null;
            for (int i = 0, end = methods.Length; i < end; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name.Equals(methodName, stringComparison) && TryInvoke(method, paramInfos[i], attributes[i], instance, args, stringComparison, out result))
                    return true;
            }
            return false;
        }
        public static bool TryInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] attributes, object instance, string methodName, IArgument[] args, out object result)
        {
            return TryInvoke(methods, paramInfos, attributes, instance, methodName, args, StringComparison.Ordinal, out result);
        }

        public static object Invoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] attributes, object instance, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            for (int i = 0, end = methods.Length; i < end; i++)
            {
                MethodInfo method = methods[i];
                if (method.Name.Equals(methodName, stringComparison))
                {
                    ParameterInfo[] _paramInfos = paramInfos[i];
                    CommandAttribute attribute = attributes[i];
                    if (attribute == null)
                        throw new ArgumentOutOfRangeException(nameof(method), "Specified method is not supported. Please add 'CommandOption' Attribute first.");
                    if (TryFormatArguments(_paramInfos, args, out var formatedArgs))
                    {
                        INamedArgument[] convertedArgs = ConvertArguments(attribute.ArgumentConverters, formatedArgs);
                        object[] methodParamObjs = GetArgumentObjects(convertedArgs);
                        return method.Invoke(instance, methodParamObjs);
                    }
                }
            }

            throw new EntryPointNotFoundException("Cannot find matched method.");
        }
        public static object Invoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] attributes, object instance, string methodName, IArgument[] args)
        {
            return Invoke(methods, paramInfos, attributes, instance, methodName, args, StringComparison.Ordinal);
        }

        public static object[] GetArgumentObjects(IList<INamedArgument> args)
        {
            object[] result = new object[args.Count];
            for (int i = 0, end = result.Length; i < end; i++)
                result[i] = args[i].ValueObj;
            return result;
        }
    }
}
