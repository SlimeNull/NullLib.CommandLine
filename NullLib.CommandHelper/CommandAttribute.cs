using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullLib.CommandLine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        IArgumentConverter[] argumentConverters;

        public IArgumentConverter[] ArgumentConverters { get => argumentConverters; }
        public CommandAttribute(params Type[] arguConverters)
        {
            this.argumentConverters = arguConverters
                .Select((v) => Activator.CreateInstance(v))
                .OfType<IArgumentConverter>()
                .ToArray();
        }
    }
}
