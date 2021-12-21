using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NullLib.CommandLine
{
    public partial class DocGen
    {
        readonly CommandObject commandObject;
        private MethodInfo[] methods;
        private ParameterInfo[][] paramInfos;
        private PropertyInfo[] commandHosts;
        private CommandAttribute[] cmdAttrs;
        private CommandArguAttribute[][] paramAttrs;
        private CommandHostAttribute[] commandHostAttrs;

        public DocGen(CommandObject cmdobj)
        {
            commandObject = cmdobj;

            CommandObjectManager.GetCommandObjectInfo(cmdobj.IntanceType, out methods, out paramInfos, out commandHosts, out cmdAttrs, out paramAttrs, out commandHostAttrs);
        }

        public IEnumerable<string> GetDoc()
        {
            int cmdCount = cmdAttrs.Length;
            int hostCount = commandHostAttrs.Length;

            yield return commandObject.IntanceType.Name;

            for (int i = 0; i < hostCount; i++)
                if (commandHosts[i].GetValue(commandObject.TargetInstance) is CommandObject cmdobj)
                    foreach (string j in new DocGen(cmdobj).GetDoc())
                        yield return $"  {j}";

            for (int i = 0; i < cmdCount; i++)
            {
                CommandAttribute _cmdAttr = cmdAttrs[i];
                CommandArguAttribute[] _paramInfos = paramAttrs[i];


            }
        }

        public string GetDocText()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(commandObject.IntanceType.Name);
            int cmdCount = cmdAttrs.Length;
            int hostCount = commandHostAttrs.Length;

            for (int i = 0; i < hostCount; i++)
            {
                if (commandHosts[i].GetValue(commandObject.TargetInstance) is CommandObject cmdobj)
                {
                    CommandHostAttribute _hostAttr = commandHostAttrs[i];

                    foreach (var def in cmdobj.GenCommandOverview())
                    {
                        sb.Append($"{_hostAttr.GetDifinitionString()} {string.Join(" ", def)}");
                        yield return Enumerable.Repeat(, 1).Concat(def);
                    }
                }
            }

            for (int i = 0; i < cmdCount; i++)
            {
                CommandAttribute _cmdAttr = cmdAttrs[i];
                CommandArguAttribute[] _paramInfos = paramAttrs[i];

                yield return Enumerable.Repeat(_cmdAttr.GetDifinitionString(), 1).Concat(_paramInfos.Select(v => v.GetDifinitionString()));
            }
        }
    }
}
