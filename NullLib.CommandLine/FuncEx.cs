using System;
using System.Collections.Generic;
using System.Text;

namespace NullLib.CommandLine
{
    internal static class FuncEx
    {
        public static StringComparison GetStringComparison(this bool ignoreCases)
        {
            return ignoreCases ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        }
        private const StringComparison ignoreCases = StringComparison.OrdinalIgnoreCase | StringComparison.CurrentCultureIgnoreCase | StringComparison.InvariantCultureIgnoreCase;
        public static bool IsIgnoreCase(this StringComparison stringComparison)
        {
            return ignoreCases.HasFlag(stringComparison);
        }
    }
}
