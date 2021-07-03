using System;

namespace NullLib.CommandLine
{
    public class CommandUnResolvedEventArgs : EventArgs
    {
        public bool Handled { get; set; }
        public CommandUnResolvedEventArgs(string str, CommandSegment[] segments, IArgument[] arguments, string cmdname, CommandSegment[] argsSegments)
        {
            this.CommandString = str;
            this.CommandSegments = segments;
            this.CommandArguments = arguments;
            this.CommandName = cmdname;
            this.CommandArgsSegments = argsSegments;
        }
        public string CommandString { get; }
        public string CommandName { get; }
        public CommandSegment[] CommandSegments { get; }
        public CommandSegment[] CommandArgsSegments { get; }
        public IArgument[] CommandArguments { get; }
    }
}