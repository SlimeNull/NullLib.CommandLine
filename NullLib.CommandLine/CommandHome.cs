using System;
using System.Collections.Generic;
using System.Text;

namespace NullLib.CommandLine
{
    /// <summary>
    /// Basic command class for NullLib.CommandLine commands, provides basic functions for using
    /// </summary>
    public class CommandHome
    {
        /// <summary>
        /// Get the CommandObject which is controlling current instance
        /// </summary>
        public CommandObject CommandObject { get; internal set; }
    }
}
