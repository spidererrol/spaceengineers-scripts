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
            /// <summary>
            /// Get a MultiSurface which convers all matching surfaces. Note: surfaceFilter actually defaults to the ShowOnScreen config filter.
            /// </summary>
            /// <param name="match">string to match in block names</param>
            /// <param name="configSection">configuration to scan in CustomData of matching blocks</param>
            /// <param name="thisgrid">only match blocks on this connected grid</param>
            /// <param name="surfaceFilter">If null will default to the ShowOnScreen config filter</param>
            /// <returns>a MultiSurface</returns>
            public MultiSurface MultiSurfaceByName(string match, string configSection, bool thisgrid = true, MultiSurface.ISurfaceFilter surfaceFilter = null)
            {
                List<IMyTextSurfaceProvider> providers = ByName<IMyTextSurfaceProvider>(match, thisgrid);
                List<IMyTextSurface> surfaces = ByName<IMyTextSurface>(match, thisgrid);
                if (surfaceFilter == null)
                    surfaceFilter = MultiSurface.ShowOnScreenFilter(configSection);
                return new MultiSurface(providers, surfaces, surfaceFilter);
            }


        }
    }
}
