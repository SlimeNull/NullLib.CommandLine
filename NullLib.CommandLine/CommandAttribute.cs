using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NullLib.CommandLine
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public static Dictionary<Type, IArgumentConverter> AllConverters;

        readonly IArgumentConverter[] argumentConverters;

        public IArgumentConverter[] ArgumentConverters { get => argumentConverters; }
        public CommandAttribute(params Type[] arguConverters)
        {
            if (AllConverters == null)
                AllConverters = new Dictionary<Type, IArgumentConverter>();

            this.argumentConverters = arguConverters
                .Select((v) => AllConverters.TryGetValue(v, out var obj) ? obj : AllConverters[v] = Activator.CreateInstance(v) as IArgumentConverter)
                .ToArray();

            if (this.argumentConverters.Length != arguConverters.Length)
                throw new ArgumentOutOfRangeException(nameof(arguConverters), "Type must inherf from 'IArgymentConverter'");

#if DEBUG
            //Console.WriteLine("AllConvertersCount:" + AllConverters.Count);
#endif
        }
    }
}
