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
using VRage.Game.GUI.TextPanel;

namespace IngameScript
{
    partial class Program
    {
        public class MultiSurface : IMyTextSurface, IEnumerable
        {
            protected List<IMyTextSurface> surfaces;

            public MultiSurface() { }
            public MultiSurface(IMyTextSurface surface) { Add(surface); }
            public MultiSurface(List<IMyTextSurface> newsurfaces) { Add(newsurfaces); }
            public MultiSurface(IMyTextSurfaceProvider provider,string surface) { Add(provider, surface); }
            public MultiSurface(IMyTextSurfaceProvider provider, int surface) { Add(provider, surface); }
            public MultiSurface(List<IMyTextSurfaceProvider> providers,string surface) { Add(providers, surface); }
            public MultiSurface(List<IMyTextSurfaceProvider> providers, int surface) { Add(providers, surface); }

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

            public void Add(IMyTextSurface surface) => surfaces.Add(surface);
            public void Add(List<IMyTextSurface> newsurfaces) => surfaces.AddList(newsurfaces);
            public void Add(IMyTextSurfaceProvider provider, string surface) => surfaces.AddList(ProviderSurfaces(provider).FindAll(s => s.Name == surface));
            public void Add(IMyTextSurfaceProvider provider, int surface) => surfaces.Add(provider.GetSurface(surface));
            public void Add(List<IMyTextSurfaceProvider> providers, string surface) => providers.ForEach(delegate (IMyTextSurfaceProvider p) { Add(p, surface); });
            public void Add(List<IMyTextSurfaceProvider> providers, int surface) => providers.ForEach(delegate (IMyTextSurfaceProvider p) { Add(p, surface); });

            public float FontSize { get { return surfaces.First().FontSize; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.FontSize = value; }); } }
            public Color FontColor { get { return surfaces.First().FontColor; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.FontColor = value; }); } }
            public Color BackgroundColor { get { return surfaces.First().BackgroundColor; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.BackgroundColor = value; }); } }
            public byte BackgroundAlpha { get { return surfaces.First().BackgroundAlpha; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.BackgroundAlpha = value; }); } }
            public float ChangeInterval { get { return surfaces.First().ChangeInterval; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.ChangeInterval = value; }); } }
            public string Font { get { return surfaces.First().Font; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.Font = value; }); } }
            public TextAlignment Alignment { get { return surfaces.First().Alignment; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.Alignment = value; }); } }
            public string Script { get { return surfaces.First().Script; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.Script = value; }); } }
            public ContentType ContentType { get { return surfaces.First().ContentType; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.ContentType = value; }); } }

            public Vector2 SurfaceSize { get { throw new MethodNotAvailable(); } }

            public Vector2 TextureSize { get { throw new MethodNotAvailable(); } }

            public bool PreserveAspectRatio { get { return surfaces.First().PreserveAspectRatio; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.PreserveAspectRatio = value; }); } }
            public float TextPadding { get { return surfaces.First().TextPadding; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.TextPadding = value; }); } }
            public Color ScriptBackgroundColor { get { return surfaces.First().ScriptBackgroundColor; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.ScriptBackgroundColor = value; }); } }
            public Color ScriptForegroundColor { get { return surfaces.First().ScriptForegroundColor; } set { surfaces.ForEach(delegate (IMyTextSurface s) { s.ScriptForegroundColor = value; }); } }

            public string Name { get { throw new MethodNotAvailable(); } }

            public string DisplayName { get { throw new MethodNotAvailable(); } }

            public string CurrentlyShownImage { get { throw new MethodNotAvailable(); } }

            public void AddImagesToSelection(List<string> ids, bool checkExistence = false) => surfaces.ForEach(delegate (IMyTextSurface s) { s.AddImagesToSelection(ids, checkExistence); });

            public void AddImageToSelection(string id, bool checkExistence = false) => surfaces.ForEach(delegate (IMyTextSurface s) { s.AddImageToSelection(id, checkExistence); });

            public void ClearImagesFromSelection() => surfaces.ForEach(delegate (IMyTextSurface s) { s.ClearImagesFromSelection(); });

            public MySpriteDrawFrame DrawFrame()
            {
                throw new MethodNotAvailable();
            }

            public IEnumerator GetEnumerator() => ((IEnumerable)surfaces).GetEnumerator();

            public void GetFonts(List<string> fonts) => surfaces.First().GetFonts(fonts);

            public void GetScripts(List<string> scripts) => surfaces.First().GetScripts(scripts);

            public void GetSelectedImages(List<string> output)
            {
                throw new MethodNotAvailable();
            }

            public void GetSprites(List<string> sprites) => surfaces.First().GetSprites(sprites);

            public string GetText()
            {
                throw new MethodNotAvailable();
            }

            public Vector2 MeasureStringInPixels(StringBuilder text, string font, float scale) => surfaces.First().MeasureStringInPixels(text, font, scale);


            public void ReadText(StringBuilder buffer, bool append = false)
            {
                throw new MethodNotAvailable();
            }

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
                bool allOk = true;
                surfaces.ForEach(delegate (IMyTextSurface s)
                {
                    if (!s.WriteText(value, append))
                        allOk = false;
                });
                return allOk;
            }
        }
    }
}
