using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullLib.CommandLine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandOptionAttribute : Attribute
    {
        readonly IArgumentConverter[] argumentConverters;

        public IArgumentConverter[] ArgumentConverters { get => argumentConverters; }
        public CommandOptionAttribute(params Type[] arguConverters)
        {
            this.argumentConverters = arguConverters
                .Select((v) => Activator.CreateInstance(v))
                .OfType<IArgumentConverter>()
                .ToArray();

            if (this.argumentConverters.Length != arguConverters.Length)
                throw new ArgumentOutOfRangeException("arguConverter", "Type must inherf from 'IArgymentConverter'");
        }
    }
}
