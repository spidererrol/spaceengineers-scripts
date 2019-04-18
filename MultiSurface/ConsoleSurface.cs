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
        public class ConsoleSurface : MultiSurface
        {
            // This is a copy from GetBlocks because I don't want to require that just for this:
            private static List<IType> GetObjectsByName<IType>(Program prog, string match, bool thisgrid = true)
            {
                List<IMyTerminalBlock> hits = new List<IMyTerminalBlock>();
                if (match == "")
                    throw new Exception("You mustn't call GetObjectsByName() with an empty string!");
                if (thisgrid)
                    prog.GridTerminalSystem.SearchBlocksOfName(match, hits, block => block.IsSameConstructAs(prog.Me));
                else
                    prog.GridTerminalSystem.SearchBlocksOfName(match, hits);
                List<IType> ret = new List<IType>();
                for (int i = 0; i < hits.Count; i++)
                {
                    IType thing;
                    try
                    {
                        thing = (IType)hits[i];
                        if (thing == null)
                            continue;
                        ret.Add(thing);
                    }
                    catch
                    {
                        continue;
                    }
                }
                return ret;
            }

            /// <summary>
            /// This provides an easy way to get a console which can be Echo, on the Programming Block screen and/or on other screens.
            /// If either of <paramref name="consoleTag"/> or <paramref name="sectionName"/> are missing or null then other screens will not be used.
            /// </summary>
            /// <remarks>
            /// You shouldn't put <paramref name="consoleTag"/> on the current Programming Block if you have <paramref name="onSelf"/> set as you will likely
            /// get dual output.
            /// </remarks>
            /// <param name="prog">(REQUIRED) should be <c>this</c></param>
            /// <param name="consoleTag">What other blocks must have in their CustomName in order to use their screen(s)</param>
            /// <param name="sectionName">The configuration section name to use when enabling individual screens from CustomData</param>
            /// <param name="useEcho">Use <c>Echo(...)</c> to output to the Programming Block interface</param>
            /// <param name="onSelf">Use the screen(s) on this Programming Block. Will use CustomData if sectionName is set, otherwise force screen 0.</param>
            /// <returns></returns>
            public static ConsoleSurface EasyConsole(Program prog, string consoleTag = null, string sectionName = null, bool useEcho = true, bool onSelf = true)
            {
                IMyProgrammableBlock Me = prog.Me;
                ConsoleSurface console;
                ISurfaceFilter filter = ShowOnScreenFilter(sectionName);
                if (consoleTag != null && sectionName != null)
                {
                    List<IMyTextSurfaceProvider> providers = GetObjectsByName<IMyTextSurfaceProvider>(prog, consoleTag);
                    console = new ConsoleSurface(prog, providers, filter, useEcho);

                    // Things which only have one surface (eg Text Panels):
                    List<IMyTextSurface> panels = GetObjectsByName<IMyTextSurface>(prog, consoleTag);
                    console.Add(panels);
                }
                else
                {
                    console = new ConsoleSurface(prog, useEcho);
                }
                if (onSelf && sectionName != null)
                {
                    Config.ConfigSection conf = Config.Section(Me, sectionName);
                    conf.Default(ShowOnScreenPrefix + Me.GetSurface(0).DisplayName, true);
                    conf.Save();
                    console.Add(Me, filter);
                }
                else if (onSelf)
                {
                    console.Add(Me, 0);
                }
                return console;
            }

            public delegate void EchoFunc(string msg);

            protected readonly Program program;
            protected readonly bool doEcho;
            private readonly List<EchoFunc> echos;
            private bool ready;

            public ConsoleSurface() : base() { ready = false; echos = new List<EchoFunc>(); }

            // These may conflict with IMyProgrammingBlock etc:
            //public ConsoleSurface(IMyTextSurface surface) : base(surface) { }
            //public ConsoleSurface(List<IMyTextSurface> newsurfaces) : base(newsurfaces) { }
            //public ConsoleSurface(IMyTextSurfaceProvider provider, string surface) : base(provider, surface) { }
            //public ConsoleSurface(IMyTextSurfaceProvider provider, int surface) : base(provider, surface) { }
            //public ConsoleSurface(List<IMyTextSurfaceProvider> providers, string surface) : base(providers, surface) { }
            //public ConsoleSurface(List<IMyTextSurfaceProvider> providers, int surface) : base(providers, surface) { }

            public ConsoleSurface(Program program, bool doecho = true, EchoFunc echo = null) : this()
            {
                if (echo != null)
                    echos.Add(echo);
                doEcho = doecho;
                this.program = program;
            }
            public ConsoleSurface(Program program, int surfaceno, bool doecho = true, EchoFunc echo = null) : this(program, doecho, echo)
            {
                Add(program.Me, surfaceno);
            }
            public ConsoleSurface(Program program, string surfacename, bool doecho = true, EchoFunc echo = null) : this(program, doecho, echo)
            {
                Add(program.Me, surfacename);
            }
            public ConsoleSurface(Program program, List<IMyTextSurfaceProvider> providers, ISurfaceFilter filter, bool doecho = true, EchoFunc echo = null) : this(program, doecho, echo)
            {
                Add(providers, filter);
            }

            protected void InitSurfaces()
            {
                foreach (IMyTextSurface surface in surfaces)
                {
                    surface.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                    surface.WriteText("", false);
                }
                ready = true;
            }

            public void ClearScreen() => InitSurfaces();

            public void Echo(string msg)
            {
                if (!ready)
                    InitSurfaces();
                if (doEcho)
                    program.Echo(msg);
                if (echos.Count > 0)
                {
                    foreach (EchoFunc echo in echos)
                    {
                        echo(msg);
                    }
                }
                if (surfaces.Count > 0)
                {
                    foreach (IMyTextSurface surface in surfaces)
                    {
                        surface.WriteText(msg + "\n", true);
                    }
                }
            }

            public void DumpPA(IMyTerminalBlock block)
            {
                List<ITerminalProperty> props = new List<ITerminalProperty>();
                List<ITerminalAction> acts = new List<ITerminalAction>();
                block.GetProperties(props);
                block.GetActions(acts);
                Echo("Block: " + block.CustomName);
                Echo("Is A: " + block.BlockDefinition.TypeIdString + "." + block.BlockDefinition.SubtypeName);
                Echo("Aka: " + block.GetType().ToString());
                Echo("Properties:");
                for (int i = 0; i < props.Count; i++)
                {
                    Echo(" " + props[i].TypeName + " " + props[i].Id);
                }
                Echo("Actions:");
                for (int i = 0; i < acts.Count; i++)
                {
                    Echo(" " + acts[i].Id);
                }
                Echo("----");
            }


        }
    }
}
