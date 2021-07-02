namespace NullLib.CommandLine
{
    public class CommandInvokingInfo
    {
        public string CommandString { get; }
        public CommandSegment[] CommandSegments { get; }
        public Argument GetCommandArguments { get; }
        public object[] CommandParameters { get; }
    }
}