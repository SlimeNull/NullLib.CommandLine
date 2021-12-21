using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Linq;
using System.Reflection;

namespace System.CommandLine.Execution
{
    public class CommandObject
    {
        internal readonly object instance;
        internal Type instanceType;
        RootCommand rootCommand;
        public CommandObject(object instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            this.instance = instance;
            this.instanceType = instance.GetType();

            instanceType.GetMethods().Select(m =>
            {
                Command command = new Command(m.Name);
                IEnumerable<Symbol> symbols = m.GetParameters().Select<ParameterInfo, Symbol>(p =>
                {
                    if (p.HasDefaultValue)
                    {
                        Option opt = Activator.CreateInstance(typeof(Option<>).MakeGenericType(p.ParameterType), p.Name) as Option;
                        opt.SetDefaultValue(p.DefaultValue);
                        return opt;
                    }
                    else
                    {
                        Argument arg = Activator.CreateInstance(typeof(Argument<>).MakeGenericType(p.ParameterType), p.Name) as Argument;
                        return arg;
                    }
                });

                foreach (Symbol symbol in symbols)
                    command.Add(symbol);

                    

                return command;
            });
        }
    }
}
