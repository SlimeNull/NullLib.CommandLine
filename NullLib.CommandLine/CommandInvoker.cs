using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace NullLib.CommandLine
{
    public static class CommandInvoker
    {
        #region PreProcess
        private static bool IsVarlenMethod(ParameterInfo[] paramInfos)
        {
            return paramInfos.Length > 0 && paramInfos[paramInfos.Length - 1].GetCustomAttribute<ParamArrayAttribute>() != null;
        }
        private static bool TryAssignNamedArgument(ParameterInfo[] paramInfos, ref IArgument[] args, IArgument toAssign, StringComparison stringComparison)
        {
            bool assigned = false;
            for(int j = 0, jend = paramInfos.Length; j < jend; j++)
            {
                ParameterInfo param = paramInfos[j];
                if(param.Name.Equals(toAssign.Name, stringComparison))
                {
                    args[j] = toAssign;
                    assigned = true;
                    break;
                }
            }

            return assigned;
        }
        private static bool TryAssignNormalArgument(ref IArgument[] args, IArgument toAssign, ref int startIndex, int endIndex)
        {
            bool assigned = false;
            for(; startIndex < endIndex; startIndex++)
            {
                if(args[startIndex] == null)
                {
                    args[startIndex] = new Argument(toAssign.Content);
                    assigned = true;
                    break;
                }
            }
            return assigned;
        }
        private static bool FillOptionalArguments(ParameterInfo[] paramInfos, ref IArgument[] args)
        {
            for(int i = 0, iend = args.Length; i < iend; i++)
            {
                if(args[i] == null)
                {
                    args[i] = new Argument(paramInfos[i].Name);
                    if(paramInfos[i].HasDefaultValue)
                        args[i].ValueObj = paramInfos[i].DefaultValue;
                    else
                        return false;
                }
            }

            return true;
        }

        private static bool TryFormatNormalArgument(ParameterInfo[] paramInfos, IArgument[] args, StringComparison stringComparison, out IArgument[] result)
        {
            if(args.Length > paramInfos.Length)
            {
                result = null;
                return false;
            }

            result = new IArgument[paramInfos.Length];

            int normalParamIndex = 0;
            for(int i = 0, iend = args.Length; i < iend; i++)
            {
                IArgument argu = args[i];
                if(string.IsNullOrWhiteSpace(argu.Name))
                {
                    if(!TryAssignNormalArgument(ref result, argu, ref normalParamIndex, result.Length))
                        return false;
                }
                else
                {
                    if(!TryAssignNamedArgument(paramInfos, ref result, argu, stringComparison))
                        return false;
                }
            }

            return true;
        }
        private static bool TryFormatVarlenArguments(ParameterInfo[] paramInfos, IArgument[] args, StringComparison stringComparison, out IArgument[] result)
        {
            result = new IArgument[paramInfos.Length];
            List<string> arrparams = new();

            int normalParamIndex = 0;
            for(int i = 0, iend = args.Length; i < iend; i++)
            {
                IArgument argu = args[i];
                if(string.IsNullOrWhiteSpace(argu.Name))
                {
                    if(!TryAssignNormalArgument(ref result, argu, ref normalParamIndex, result.Length - 1))
                        arrparams.Add(argu.Content);
                }
                else
                {
                    if(!TryAssignNamedArgument(paramInfos, ref result, argu, stringComparison))
                        return false;
                }
            }

            result[result.Length - 1] = new Argument() { Name = paramInfos[paramInfos.Length - 1].Name, ValueObj = arrparams.ToArray() };

            return true;
        }

        public static bool TryFormatArguments(ParameterInfo[] paramInfos, IArgument[] args, StringComparison stringComparison, out IArgument[] result)
        {
            if(IsVarlenMethod(paramInfos))
            {
                if(!TryFormatVarlenArguments(paramInfos, args, stringComparison, out result))
                    return false;
            }
            else
            {
                if(!TryFormatNormalArgument(paramInfos, args, stringComparison, out result))
                    return false;
            }

            return FillOptionalArguments(paramInfos, ref result);
        }
        public static bool TryFormatArguments(ParameterInfo[] paramInfos, IArgument[] args, out IArgument[] result)
        {
            return TryFormatArguments(paramInfos, args, StringComparison.Ordinal, out result);
        }
        public static bool TryConvertArguments(ParameterInfo[] paramInfos, IArgumentConverter[] converters, ref IArgument[] args, StringComparison stringComparison)
        {
            const StringComparison ignoreCasesComparison = StringComparison.OrdinalIgnoreCase | StringComparison.CurrentCultureIgnoreCase | StringComparison.InvariantCultureIgnoreCase;
            bool ignoreCases = ignoreCasesComparison.HasFlag(stringComparison);

            IEnumerator enumerator = converters.GetEnumerator();
            IArgumentConverter curConvtr = ArgumentConverterManager.GetConverter<ArguConverter>();
            for(int i = 0, end = args.Length; i < end; i++)
            {
                if(enumerator.MoveNext() && enumerator.Current != null)
                    curConvtr = enumerator.Current as IArgumentConverter;
                if(!paramInfos[i].ParameterType.IsAssignableFrom(curConvtr.TargetType))
                    return false;
                IArgument curArgu = args[i];
                if(!curConvtr.TargetType.IsInstanceOfType(curArgu.ValueObj))
                {
                    curConvtr.IgnoreCases = ignoreCases;
                    if(!curConvtr.TryConvert(curArgu.ValueObj, out var valueObj))
                        return false;
                    curArgu.ValueObj = valueObj;
                }
            }
            return true;
        }

        private static void FormatNormalArgument(ParameterInfo[] paramInfos, IArgument[] args, StringComparison stringComparison, out IArgument[] result)
        {
            if(args.Length > paramInfos.Length)
                throw new CommandParameterFormatException();

            result = new IArgument[paramInfos.Length];

            int normalParamIndex = 0;
            for(int i = 0, iend = args.Length; i < iend; i++)
            {
                IArgument argu = args[i];
                if(string.IsNullOrWhiteSpace(argu.Name))
                {
                    if(!TryAssignNormalArgument(ref result, argu, ref normalParamIndex, result.Length))
                        throw new CommandArgumentAssignException(i, argu);
                }
                else
                {
                    if(!TryAssignNamedArgument(paramInfos, ref result, argu, stringComparison))
                        throw new CommandArgumentAssignException(i, argu);
                }
            }
        }
        private static void FormatVarlenArgument(ParameterInfo[] paramInfos, IArgument[] args, StringComparison stringComparison, out IArgument[] result)
        {
            result = new IArgument[paramInfos.Length];
            List<string> arrparams = new();

            int normalParamIndex = 0;
            for(int i = 0, iend = args.Length; i < iend; i++)
            {
                IArgument argu = args[i];
                if(string.IsNullOrWhiteSpace(argu.Name))
                {
                    if(!TryAssignNormalArgument(ref result, argu, ref normalParamIndex, result.Length - 1))
                        arrparams.Add(argu.Content);
                }
                else
                {
                    if(!TryAssignNamedArgument(paramInfos, ref result, argu, stringComparison))
                        throw new CommandArgumentAssignException(i, argu);
                }
            }

            result[result.Length - 1] = new Argument() { Name = paramInfos[paramInfos.Length - 1].Name, ValueObj = arrparams.ToArray() };
        }
        public static IArgument[] FormatArguments(MethodInfo method, ParameterInfo[] paramInfos, IArgument[] args, StringComparison stringComparison)
        {
            IArgument[] result;
            try
            {
                if(IsVarlenMethod(paramInfos))
                    FormatVarlenArgument(paramInfos, args, stringComparison, out result);
                else
                    FormatNormalArgument(paramInfos, args, stringComparison, out result);
            }
            catch(CommandArgumentAssignException e)
            {
                throw new CommandParameterFormatException(method, null, e);
            }

            FillOptionalArguments(paramInfos, ref result);
            return result;
        }
        public static IArgument[] FormatArguments(MethodInfo method, ParameterInfo[] paramInfos, IArgument[] args)
        {
            return FormatArguments(method, paramInfos, args, StringComparison.Ordinal);
        }
        public static IArgument[] ConvertArguments(MethodInfo method, ParameterInfo[] paramInfos, IArgumentConverter[] converters, IArgument[] args, StringComparison stringComparison)
        {
            const StringComparison ignoreCasesComparison = StringComparison.OrdinalIgnoreCase | StringComparison.CurrentCultureIgnoreCase | StringComparison.InvariantCultureIgnoreCase;
            bool ignoreCases = ignoreCasesComparison.HasFlag(stringComparison);

            IEnumerator enumerator = converters.GetEnumerator();
            IArgumentConverter curConvtr = ArgumentConverterManager.GetConverter<ArguConverter>();
            for(int i = 0, end = args.Length; i < end; i++)
            {
                if(enumerator.MoveNext() && enumerator.Current != null)
                    curConvtr = enumerator.Current as IArgumentConverter;
                if(!paramInfos[i].ParameterType.IsAssignableFrom(curConvtr.TargetType))
                    throw new CommandParameterConvertException(method, "Parameter type not match argument type.");
                IArgument curArgu = args[i];
                if(!curConvtr.TargetType.IsInstanceOfType(curArgu.ValueObj))
                {
                    curConvtr.IgnoreCases = ignoreCases;
                    try
                    {
                        curArgu.ValueObj = curConvtr.Convert(curArgu.ValueObj);
                    }
                    catch(Exception e)
                    {
                        throw new CommandParameterConvertException(method, null, e);
                    }
                }
            }
            return args;
        }

        public static object[] GetArgumentObjects(IList<IArgument> args)
        {
            object[] result = new object[args.Count];
            for(int i = 0, end = result.Length; i < end; i++)
                result[i] = args[i].ValueObj;
            return result;
        }
        #endregion

        #region InvokerOverloads
        public static bool TryInvoke(MethodInfo method, ParameterInfo[] paramInfos, CommandAttribute attribute, object instance, IArgument[] args, out object result)
        {
            return TryInvoke(method, paramInfos, attribute, instance, args, StringComparison.Ordinal, out result);
        }                  // not root
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

        public static object Invoke(MethodInfo method, ParameterInfo[] paramInfos, CommandAttribute attribute, object instance, IArgument[] args)
        {
            return Invoke(method, paramInfos, attribute, instance, args, StringComparison.Ordinal);
        }    // not root
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

        public static bool TryInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] attributes, object instance, string methodName, IArgument[] args, out object result)
        {
            return TryInvoke(methods, paramInfos, attributes, instance, methodName, args, StringComparison.Ordinal, out result);
        }               // not root
        public static bool TryInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, object instance, string methodName, IArgument[] args, StringComparison stringComparison, out object result)
        {
            CommandAttribute[] attributes = new CommandAttribute[methods.Length];
            for(int i = 0, end = methods.Length; i < end; i++)
                attributes[i] = methods[i].GetCustomAttribute<CommandAttribute>();
            return TryInvoke(methods, paramInfos, attributes, instance, methodName, args, stringComparison, out result);
        }   // not root
        public static bool TryInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, object instance, string methodName, IArgument[] args, out object result)
        {
            return TryInvoke(methods, paramInfos, instance, methodName, args, StringComparison.Ordinal, out result);
        }                  // not root
        public static bool TryInvoke(MethodInfo[] methods, object instance, string methodName, IArgument[] args, StringComparison stringComparison, out object result)
        {
            ParameterInfo[][] paramInfos = new ParameterInfo[methods.Length][];
            for(int i = 0, end = methods.Length; i < end; i++)
                paramInfos[i] = methods[i].GetParameters();
            return TryInvoke(methods, paramInfos, instance, methodName, args, stringComparison, out result);
        }             // not root
        public static bool TryInvoke(MethodInfo[] methods, object instance, string methodName, IArgument[] args, out object result)
        {
            return TryInvoke(methods, instance, methodName, args, StringComparison.Ordinal, out result);
        }                                                // not root

        public static object Invoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] attributes, object instance, string methodName, IArgument[] args)
        {
            return Invoke(methods, paramInfos, attributes, instance, methodName, args, StringComparison.Ordinal);
        }                                   // not root
        public static object Invoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, object instance, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            CommandAttribute[] attributes = new CommandAttribute[methods.Length];
            for(int i = 0, end = methods.Length; i < end; i++)
                attributes[i] = methods[i].GetCustomAttribute<CommandAttribute>();
            return Invoke(methods, paramInfos, attributes, instance, methodName, args, stringComparison);
        }   // not root
        public static object Invoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, object instance, string methodName, IArgument[] args)
        {
            return Invoke(methods, paramInfos, instance, methodName, args, StringComparison.Ordinal);
        }            // not root
        public static object Invoke(MethodInfo[] methods, object instance, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            ParameterInfo[][] paramInfos = new ParameterInfo[methods.Length][];
            for(int i = 0, end = methods.Length; i < end; i++)
                paramInfos[i] = methods[i].GetParameters();
            return Invoke(methods, paramInfos, instance, methodName, args, stringComparison);
        }       // not root
        public static object Invoke(MethodInfo[] methods, object instance, string methodName, IArgument[] args)
        {
            return Invoke(methods, instance, methodName, args, StringComparison.Ordinal);
        }                                          // not root

        public static bool CanInvoke(MethodInfo method, ParameterInfo[] paramInfos, CommandAttribute attribute, string methodName, IArgument[] args)
        {
            return CanInvoke(method, paramInfos, attribute, methodName, args, StringComparison.Ordinal);
        }
        public static bool CanInvoke(MethodInfo method, ParameterInfo[] paramInfos, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            return CanInvoke(method, paramInfos, method.GetCustomAttribute<CommandAttribute>(), methodName, args, stringComparison);
        }
        public static bool CanInvoke(MethodInfo method, ParameterInfo[] paramInfos, string methodName, IArgument[] args)
        {
            return CanInvoke(method, paramInfos, method.GetCustomAttribute<CommandAttribute>(), methodName, args, StringComparison.Ordinal);
        }
        public static bool CanInvoke(MethodInfo method, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            return CanInvoke(method, method.GetParameters(), method.GetCustomAttribute<CommandAttribute>(), methodName, args, stringComparison);
        }
        public static bool CanInvoke(MethodInfo method, string methodName, IArgument[] args)
        {
            return CanInvoke(method, method.GetParameters(), method.GetCustomAttribute<CommandAttribute>(), methodName, args, StringComparison.Ordinal);
        }

        public static bool CanInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] attributes, string methodName, IArgument[] args)
        {
            return CanInvoke(methods, paramInfos, attributes, methodName, args, StringComparison.Ordinal);
        }
        public static bool CanInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            CommandAttribute[] attributes = new CommandAttribute[methods.Length];
            for(int i = 0, end = methods.Length; i < end; i++)
                attributes[i] = methods[i].GetCustomAttribute<CommandAttribute>();
            return CanInvoke(methods, paramInfos, attributes, methodName, args, stringComparison);
        }
        public static bool CanInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, string methodName, IArgument[] args)
        {
            return CanInvoke(methods, paramInfos, methodName, args, StringComparison.Ordinal);
        }
        public static bool CanInvoke(MethodInfo[] methods, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            ParameterInfo[][] paramInfos = new ParameterInfo[methods.Length][];
            for(int i = 0, end = methods.Length; i < end; i++)
                paramInfos[i] = methods[i].GetParameters();
            return CanInvoke(methods, paramInfos, methodName, args, stringComparison);
        }
        public static bool CanInvoke(MethodInfo[] methods, string methodName, IArgument[] args)
        {
            return CanInvoke(methods, methodName, args, StringComparison.Ordinal);
        }
        #endregion

        #region InvokerRoots
        public static bool TryInvoke(MethodInfo method, ParameterInfo[] paramInfos, CommandAttribute attribute, object instance, IArgument[] args, StringComparison stringComparison, out object result)
        {
            result = null;
            if(!TryFormatArguments(paramInfos, args, stringComparison, out var formatedArgs))
                return false;
            if(!TryConvertArguments(paramInfos, attribute.ArgumentConverters, ref formatedArgs, stringComparison))
                return false;
            object[] methodParamObjs = GetArgumentObjects(formatedArgs);
            result = method.Invoke(instance, methodParamObjs);
            return true;
        }
        public static object Invoke(MethodInfo method, ParameterInfo[] paramInfos, CommandAttribute attribute, object instance, IArgument[] args, StringComparison stringComparison)
        {
            IArgument[] formatedArgs = FormatArguments(method, paramInfos, args, stringComparison);
            IArgument[] convertedArgs = ConvertArguments(method, paramInfos, attribute.ArgumentConverters, formatedArgs, stringComparison);
            object[] methodParamObjs = GetArgumentObjects(convertedArgs);
            return method.Invoke(instance, methodParamObjs);
        }
        public static bool TryInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] attributes, object instance, string methodName, IArgument[] args, StringComparison stringComparison, out object result)
        {
            result = null;
            for(int i = 0, end = methods.Length; i < end; i++)
            {
                MethodInfo method = methods[i];
                if(method.Name.Equals(methodName, stringComparison) && TryInvoke(method, paramInfos[i], attributes[i], instance, args, stringComparison, out result))
                    return true;
            }
            return false;
        }
        public static object Invoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] attributes, object instance, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            for(int i = 0, end = methods.Length; i < end; i++)
            {
                MethodInfo method = methods[i];
                if(method.Name.Equals(methodName, stringComparison))
                {
                    ParameterInfo[] _paramInfos = paramInfos[i];
                    CommandAttribute attribute = attributes[i];
                    if(attribute == null)
                        throw new ArgumentOutOfRangeException(nameof(method), "Specified method is not supported. Please add 'CommandOption' Attribute first.");
                    if (TryFormatArguments(_paramInfos, args, stringComparison, out var formatedArgs))
                    {
                        if (TryConvertArguments(_paramInfos, attribute.ArgumentConverters, ref formatedArgs, stringComparison))
                        {
                            object[] methodParamObjs = GetArgumentObjects(formatedArgs);
                            return method.Invoke(instance, methodParamObjs);
                        }
                    }
                }
            }

            throw new CommandEntryPointNotFoundException(methodName);
        }
        public static bool CanInvoke(MethodInfo method, ParameterInfo[] paramInfos, CommandAttribute attribute, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            return
                method.Name.Equals(methodName, stringComparison) &&
                TryFormatArguments(paramInfos, args, stringComparison, out var formatedArgs) &&
                TryConvertArguments(paramInfos, attribute.ArgumentConverters, ref formatedArgs, stringComparison);
        }
        public static bool CanInvoke(MethodInfo[] methods, ParameterInfo[][] paramInfos, CommandAttribute[] attributes, string methodName, IArgument[] args, StringComparison stringComparison)
        {
            for(int i = 0, end = methods.Length; i < end; i++)
                if(CanInvoke(methods[i], paramInfos[i], attributes[i], methodName, args, stringComparison))
                    return true;
            return false;
        }
        #endregion
    }
}
