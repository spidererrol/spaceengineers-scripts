using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program
    {
        public partial class GetBlocksClass
        {
            public void ActionGroups<IType>(string[] grouplist, string actionname) where IType : IMyTerminalBlock
            {
                for (int i = 0; i < grouplist.Length; i++)
                {
                    List<IType> blocks = OfGroup<IType>(grouplist[i]);
                    Utility.RunActions<IType>(blocks, actionname);
                }
            }

            public void ActionGroups<IType>(string onegroup, string actionname) where IType : IMyTerminalBlock
            {
                string[] grouplist = { onegroup };
                ActionGroups<IType>(grouplist, actionname);
            }

        }
    }
}
