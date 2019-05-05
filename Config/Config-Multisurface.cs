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
        public partial class MultiSurface
        {
            public class SurfaceConfigFilter : ISurfaceFilter
            {
                private readonly string section;
                private readonly KeyBuilder keypattern;

                public string Section => section;

                public string Key(string displayname, string idname, int pos) => keypattern.Build(displayname, idname, pos);

                public SurfaceConfigFilter(string configSection, KeyBuilder configKey)
                {
                    section = configSection;
                    keypattern = configKey;
                }

                public List<IMyTextSurface> Surfaces(IMyTextSurfaceProvider block)
                {
                    List<IMyTextSurface> surfaces = new List<IMyTextSurface>();
                    Config.ConfigSection config = Config.Section((IMyTerminalBlock)block, section);
                    for (int i = 0; i < block.SurfaceCount; i++)
                    {
                        IMyTextSurface surface = block.GetSurface(i);
                        if (config.Get(Key(surface.DisplayName, surface.Name, i), false))
                            surfaces.Add(surface);
                    }
                    return surfaces;
                }
            }

            public static ISurfaceFilter MakeSurfaceConfigFilter(string section, params string[] keyparts) => new SurfaceConfigFilter(section, new KeyBuilder(keyparts));
            public static ISurfaceFilter ShowOnScreenFilter(string SectionName) => MakeSurfaceOR(
                    MakeSurfaceConfigFilter(SectionName, ShowOnScreenPrefix, KeyBuilder.SURFACEDISPLAYNAME),
                    MakeSurfaceConfigFilter(SectionName, ShowOnScreenPrefix, KeyBuilder.SURFACEIDNAME),
                    MakeSurfaceConfigFilter(SectionName, ShowOnScreenPrefix, KeyBuilder.SURFACEPOS)
                    );
        }
    }
}
