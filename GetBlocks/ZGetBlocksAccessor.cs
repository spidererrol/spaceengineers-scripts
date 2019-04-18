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
        private GetBlocksClass getBlocks;
       
        public GetBlocksClass GetBlocks
        {
            get {
                if (getBlocks == null)
                    getBlocks = new GetBlocksClass(this);
                return getBlocks;
            }
            set
            {
                getBlocks = value;
            }
        }
    }
}
