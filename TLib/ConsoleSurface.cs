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
            public class KeyBuilder
            {
                private readonly List<string> parts;
                public const string SURFACEDISPLAYNAME = "[displayname]";
                public const string SURFACEIDNAME = "[idname]";
                public const string SURFACEPOS = "[pos]";

                public KeyBuilder() { parts = new List<string>(); }
                public KeyBuilder(params string[] moreparts) : this() { parts.AddArray(moreparts); }

                public KeyBuilder Add(string part) { parts.Add(part); return this; }

                public string Build(string displayname, string idname, int pos)
                {
                    string ret = "";
                    parts.ForEach(delegate (string s)
                    {
                        if (s == SURFACEIDNAME)
                            ret += idname;
                        else if (s == SURFACEDISPLAYNAME)
                            ret += displayname;
                        else if (s == SURFACEPOS)
                            ret += pos.ToString();
                        else
                            ret += s;
                    });
                    return ret;
                }
            }

            public interface ISurfaceFilter
            {
                List<IMyTextSurface> Surfaces(IMyTextSurfaceProvider provider);
            }

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

            public class SurfaceORFilter : ISurfaceFilter
            {
                private readonly List<ISurfaceFilter> filters;
                private SurfaceORFilter() { filters = new List<ISurfaceFilter>(); }
                public SurfaceORFilter(params ISurfaceFilter[] newfilters) : this()
                {
                    filters.AddArray(newfilters);
                }

                public void Add(ISurfaceFilter filter) => filters.Add(filter);

                public List<IMyTextSurface> Surfaces(IMyTextSurfaceProvider provider)
                {
                    List<IMyTextSurface> surfaces = new List<IMyTextSurface>();
                    foreach (ISurfaceFilter filter in filters)
                    {
                        surfaces.AddList(filter.Surfaces(provider));
                    }
                    return surfaces;
                }
            }
            public static ISurfaceFilter MakeSurfaceOR(params ISurfaceFilter[] filters) => new SurfaceORFilter(filters);

            public static ConsoleSurface.ISurfaceFilter ShowOnScreenFilter(string SectionName) => ConsoleSurface.MakeSurfaceOR(
                    ConsoleSurface.MakeSurfaceConfigFilter(SectionName, "ShowOnScreen_", ConsoleSurface.KeyBuilder.SURFACEDISPLAYNAME),
                    ConsoleSurface.MakeSurfaceConfigFilter(SectionName, "ShowOnScreen_", ConsoleSurface.KeyBuilder.SURFACEIDNAME),
                    ConsoleSurface.MakeSurfaceConfigFilter(SectionName, "ShowOnScreen_", ConsoleSurface.KeyBuilder.SURFACEPOS)
                    );

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

            public void Add(IMyTextSurfaceProvider provider, ISurfaceFilter filter = null)
            {
                if (filter == null)
                    Add(ProviderSurfaces(provider));
                else
                    surfaces.AddList(filter.Surfaces(provider));
            }
            public void Add(List<IMyTextSurfaceProvider> providers, ISurfaceFilter filter = null)
            {
                if (filter == null)
                    providers.ForEach(delegate (IMyTextSurfaceProvider provider) { Add(ProviderSurfaces(provider)); });
                else
                    providers.ForEach(delegate (IMyTextSurfaceProvider provider) { surfaces.AddList(filter.Surfaces(provider)); });
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

            public void Clear() => InitSurfaces();

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
        }
    }
}
