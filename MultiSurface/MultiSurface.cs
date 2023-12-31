﻿using Sandbox.Game.EntityComponents;
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
using VRage.Game.GUI.TextPanel;

namespace IngameScript
{
    partial class Program
    {
        public partial class MultiSurface : IMyTextSurface, IEnumerable
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
                            ret += pos.ToString("F0");
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

            public class SurfaceConfigParserFilter : ISurfaceFilter
            {
                public delegate bool GetConfigFunc(IMyTextSurfaceProvider block, string display_name_or_id);
                private readonly GetConfigFunc getconfig;
                private readonly KeyBuilder keypattern;

                public string Key(string displayname, string idname, int pos) => keypattern.Build(displayname, idname, pos);

                public SurfaceConfigParserFilter(GetConfigFunc getConfigFunc, KeyBuilder keyBuilder)
                {
                    getconfig = getConfigFunc;
                    keypattern = keyBuilder;
                }

                public List<IMyTextSurface> Surfaces(IMyTextSurfaceProvider block)
                {
                    List<IMyTextSurface> surfaces = new List<IMyTextSurface>();
                    for (int i = 0; i < block.SurfaceCount; i++)
                    {
                        IMyTextSurface surface = block.GetSurface(i);
                        if (getconfig(block, Key(surface.DisplayName, surface.Name, i)))
                            surfaces.Add(surface);
                    }
                    return surfaces;
                }
            }

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

            protected const string ShowOnScreenPrefix = "ShowOnScreen_";
            public static ISurfaceFilter MakeSurfaceConfigParserFilter(SurfaceConfigParserFilter.GetConfigFunc parser, params string[] keyparts) => new SurfaceConfigParserFilter(parser, new KeyBuilder(keyparts));
            public static ISurfaceFilter ShowOnScreenFilter(SurfaceConfigParserFilter.GetConfigFunc parser) => MakeSurfaceOR(
                    MakeSurfaceConfigParserFilter(parser, ShowOnScreenPrefix, KeyBuilder.SURFACEDISPLAYNAME),
                    MakeSurfaceConfigParserFilter(parser, ShowOnScreenPrefix, KeyBuilder.SURFACEIDNAME),
                    MakeSurfaceConfigParserFilter(parser, ShowOnScreenPrefix, KeyBuilder.SURFACEPOS)
                    );

            protected List<IMyTextSurface> surfaces;
            private bool clearOnWrite = false;

            /// <summary>
            /// Conveniance operator to slightly ease converting from LCDs to a MultiSurface.
            /// </summary>
            /// <param name="multiSurface">The MultiSurface to convert</param>
            public static implicit operator List<IMyTextSurface>(MultiSurface multiSurface) => multiSurface.surfaces;

            public MultiSurface() { surfaces = new List<IMyTextSurface>(); }
            public MultiSurface(IMyTextSurface surface) : this() { Add(surface); }
            public MultiSurface(List<IMyTextSurface> newsurfaces) : this() { Add(newsurfaces); }
            public MultiSurface(IMyTextSurfaceProvider provider, string surface) : this() { Add(provider, surface); }
            public MultiSurface(IMyTextSurfaceProvider provider, int surface) : this() { Add(provider, surface); }
            public MultiSurface(List<IMyTextSurfaceProvider> providers, string surface) : this() { Add(providers, surface); }
            public MultiSurface(List<IMyTextSurfaceProvider> providers, int surface) : this() { Add(providers, surface); }
            public MultiSurface(List<IMyTextSurfaceProvider> providers, List<IMyTextSurface> surfaces, ISurfaceFilter filter = null) : this()
            {
                Add(surfaces);
                Add(providers, filter);
            }

            /// <remarks>
            /// Why isn't IMyTextSurfaceProvider and IEnumerable?
            /// </remarks>
            protected static List<IMyTextSurface> ProviderSurfaces(IMyTextSurfaceProvider provider)
            {
                List<IMyTextSurface> ret = new List<IMyTextSurface>();
                for (int i = 0; i < provider.SurfaceCount; i++)
                {
                    ret.Add(provider.GetSurface(i));
                }
                return ret;
            }

            public void Add(IMyTextPanel surface) => Add((IMyTextSurface)surface);
            public void Add(List<IMyTextPanel> surfaces) => Add(surfaces.ConvertAll(p => (IMyTextSurface)p));
            public void Add(IMyTextSurface surface) => surfaces.Add(surface);
            public void Add(List<IMyTextSurface> newsurfaces) => surfaces.AddList(newsurfaces);
            public void Add(IMyTextSurfaceProvider provider, string surface) => surfaces.AddList(ProviderSurfaces(provider).FindAll(s => s.Name == surface));
            public void Add(IMyTextSurfaceProvider provider, int surface) => surfaces.Add(provider.GetSurface(surface));
            public void Add(List<IMyTextSurfaceProvider> providers, string surface) => providers.ForEach(delegate (IMyTextSurfaceProvider p) { Add(p, surface); });
            public void Add(List<IMyTextSurfaceProvider> providers, int surface) => providers.ForEach(delegate (IMyTextSurfaceProvider p) { Add(p, surface); });
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

            public float FontSize { get { return surfaces.First().FontSize; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.FontSize = value; }); } }
            public Color FontColor { get { return surfaces.First().FontColor; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.FontColor = value; }); } }
            public Color BackgroundColor { get { return surfaces.First().BackgroundColor; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.BackgroundColor = value; }); } }
            public byte BackgroundAlpha { get { return surfaces.First().BackgroundAlpha; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.BackgroundAlpha = value; }); } }
            public float ChangeInterval { get { return surfaces.First().ChangeInterval; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.ChangeInterval = value; }); } }
            public string Font { get { return surfaces.First().Font; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.Font = value; }); } }
            public TextAlignment Alignment { get { return surfaces.First().Alignment; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.Alignment = value; }); } }
            public string Script { get { return surfaces.First().Script; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.Script = value; }); } }
            public ContentType ContentType { get { return surfaces.First().ContentType; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.ContentType = value; }); } }

            public Vector2 SurfaceSize
            {
                // This doesn't really make sense as the surfaces could be of differing sizes.
                get { throw new MethodNotAvailable(); }
            }
            public List<Vector2> SurfaceSizes { get { return surfaces.ConvertAll<Vector2>(surface => surface.SurfaceSize); } }

            public Vector2 TextureSize
            {
                // This doesn't really make sense as the surfaces could be of differing sizes.
                get { throw new MethodNotAvailable(); }
            }
            public List<Vector2> TextureSizes { get { return surfaces.ConvertAll<Vector2>(surface => surface.TextureSize); } }

            public bool PreserveAspectRatio { get { return surfaces.First().PreserveAspectRatio; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.PreserveAspectRatio = value; }); } }
            public float TextPadding { get { return surfaces.First().TextPadding; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.TextPadding = value; }); } }
            public Color ScriptBackgroundColor { get { return surfaces.First().ScriptBackgroundColor; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.ScriptBackgroundColor = value; }); } }
            public Color ScriptForegroundColor { get { return surfaces.First().ScriptForegroundColor; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.ScriptForegroundColor = value; }); } }

            public string Name
            {
                // This doesn't really make sense as the surfaces likely have different Names.
                get { throw new MethodNotAvailable(); }
            }

            public string DisplayName
            {
                // This doesn't really make sense as the surfaces likely have different Names.
                get { throw new MethodNotAvailable(); }
            }

            public string CurrentlyShownImage { get { return surfaces.First().CurrentlyShownImage; } }

            public void AddImagesToSelection(List<string> ids, bool checkExistence = false) => surfaces.ForEach(delegate (IMyTextSurface s) { s.AddImagesToSelection(ids, checkExistence); });

            public void AddImageToSelection(string id, bool checkExistence = false) => surfaces.ForEach(delegate (IMyTextSurface s) { s.AddImageToSelection(id, checkExistence); });

            public void ClearImagesFromSelection() => surfaces.ForEach(delegate (IMyTextSurface s) { s.ClearImagesFromSelection(); });

            public void SetCurrentImage(string id, bool checkExistence = false)
            {
                if (CurrentlyShownImage == id)
                    return;
                ClearImagesFromSelection();
                AddImageToSelection(id, checkExistence);
                if (!checkExistence) // I assume that checkExistence==true could legitimatly result in no image.
                {
                    foreach (IMyTextSurface surface in surfaces)
                    {
                        if (surface.CurrentlyShownImage != id)
                        {
                            if (clearOnWrite)
                                ClearText();
                            surface.WriteText("Unable to display " + id + " on this surface\n", true);
                        }
                    }
                }
            }

            public MySpriteDrawFrame DrawFrame()
            {
                // This doesn't really make sense as the surfaces could be of differing sizes.
                throw new MethodNotAvailable();
            }

            /// <summary>
            /// If your code can draw to multiple frames, then this will provide them for you:
            /// </summary>
            /// <returns>A List of MySpriteDraw Frame. One for each IMyTextSurface.</returns>
            public List<MySpriteDrawFrame> DrawFrames() => surfaces.ConvertAll<MySpriteDrawFrame>(s => s.DrawFrame());

            public IEnumerator GetEnumerator() => ((IEnumerable)surfaces).GetEnumerator();

            public void GetFonts(List<string> fonts) => surfaces.First().GetFonts(fonts);

            public void GetScripts(List<string> scripts) => surfaces.First().GetScripts(scripts);

            public void GetSelectedImages(List<string> output) => surfaces.First().GetSelectedImages(output);

            public void GetSprites(List<string> sprites) => surfaces.First().GetSprites(sprites);

            public string GetText() => surfaces.First().GetText();

            public Vector2 MeasureStringInPixels(StringBuilder text, string font, float scale) => surfaces.First().MeasureStringInPixels(text, font, scale);

            public void ReadText(StringBuilder buffer, bool append = false) => surfaces.First().ReadText(buffer, append);

            public void RemoveImageFromSelection(string id, bool removeDuplicates = false) => surfaces.First().RemoveImageFromSelection(id, removeDuplicates);

            public void RemoveImagesFromSelection(List<string> ids, bool removeDuplicates = false) => surfaces.First().RemoveImagesFromSelection(ids, removeDuplicates);

            public bool WriteText(string value, bool append = false)
            {
                bool allOk = true;
                surfaces.ForEach(delegate (IMyTextSurface s)
                {
                    if (!s.WriteText(value, append))
                        allOk = false;
                });
                return allOk;
            }

            public bool WriteText(StringBuilder value, bool append = false)
            {
                if (clearOnWrite)
                    ClearText();
                bool allOk = true;
                surfaces.ForEach(delegate (IMyTextSurface s)
                {
                    if (!s.WriteText(value, append))
                        allOk = false;
                });
                return allOk;
            }

            public bool WriteLine(string value, bool append = true) => WriteText(value + "\n", append);

            public void ClearOnWrite() => clearOnWrite = true;

            public bool ClearText()
            {
                clearOnWrite = false;
                return WriteText("", false);
            }

            public List<string> GetFonts()
            {
                List<string> fonts = new List<string>();
                GetFonts(fonts);
                return fonts;
            }
        }
    }
}
