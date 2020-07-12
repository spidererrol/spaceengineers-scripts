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

        /// <summary>
        /// Handle storing configuration on an <see cref="IMyTerminalBlock"/> in <see cref="MyIni"/> format.
        /// </summary>
        public class Config : MyIni
        {
            public static readonly System.Text.RegularExpressions.Regex reWebColor = new System.Text.RegularExpressions.Regex(@"^#?([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$");
            public static Color WebColor(string col)
            {
                switch (col.ToLower())
                {
                    case "aliceblue":
                        return Color.AliceBlue;
                    case "antiquewhite":
                        return Color.AntiqueWhite;
                    case "aqua":
                        return Color.Aqua;
                    case "aquamarine":
                        return Color.Aquamarine;
                    case "azure":
                        return Color.Azure;
                    case "beige":
                        return Color.Beige;
                    case "bisque":
                        return Color.Bisque;
                    case "black":
                        return Color.Black;
                    case "blanchedalmond":
                        return Color.BlanchedAlmond;
                    case "blue":
                        return Color.Blue;
                    case "blueviolet":
                        return Color.BlueViolet;
                    case "brown":
                        return Color.Brown;
                    case "burlywood":
                        return Color.BurlyWood;
                    case "cadetblue":
                        return Color.CadetBlue;
                    case "chartreuse":
                        return Color.Chartreuse;
                    case "chocolate":
                        return Color.Chocolate;
                    case "coral":
                        return Color.Coral;
                    case "cornflowerblue":
                        return Color.CornflowerBlue;
                    case "cornsilk":
                        return Color.Cornsilk;
                    case "crimson":
                        return Color.Crimson;
                    case "cyan":
                        return Color.Cyan;
                    case "darkblue":
                        return Color.DarkBlue;
                    case "darkcyan":
                        return Color.DarkCyan;
                    case "darkgoldenrod":
                        return Color.DarkGoldenrod;
                    case "darkgray":
                        return Color.DarkGray;
                    case "darkgreen":
                        return Color.DarkGreen;
                    case "darkkhaki":
                        return Color.DarkKhaki;
                    case "darkmagenta":
                        return Color.DarkMagenta;
                    case "darkolivegreen":
                        return Color.DarkOliveGreen;
                    case "darkorange":
                        return Color.DarkOrange;
                    case "darkorchid":
                        return Color.DarkOrchid;
                    case "darkred":
                        return Color.DarkRed;
                    case "darksalmon":
                        return Color.DarkSalmon;
                    case "darkseagreen":
                        return Color.DarkSeaGreen;
                    case "darkslateblue":
                        return Color.DarkSlateBlue;
                    case "darkslategray":
                        return Color.DarkSlateGray;
                    case "darkturquoise":
                        return Color.DarkTurquoise;
                    case "darkviolet":
                        return Color.DarkViolet;
                    case "deeppink":
                        return Color.DeepPink;
                    case "deepskyblue":
                        return Color.DeepSkyBlue;
                    case "dimgray":
                        return Color.DimGray;
                    case "dodgerblue":
                        return Color.DodgerBlue;
                    case "firebrick":
                        return Color.Firebrick;
                    case "floralwhite":
                        return Color.FloralWhite;
                    case "forestgreen":
                        return Color.ForestGreen;
                    case "fuchsia":
                        return Color.Fuchsia;
                    case "gainsboro":
                        return Color.Gainsboro;
                    case "ghostwhite":
                        return Color.GhostWhite;
                    case "gold":
                        return Color.Gold;
                    case "goldenrod":
                        return Color.Goldenrod;
                    case "gray":
                        return Color.Gray;
                    case "green":
                        return Color.Green;
                    case "greenyellow":
                        return Color.GreenYellow;
                    case "honeydew":
                        return Color.Honeydew;
                    case "hotpink":
                        return Color.HotPink;
                    case "indianred":
                        return Color.IndianRed;
                    case "indigo":
                        return Color.Indigo;
                    case "ivory":
                        return Color.Ivory;
                    case "khaki":
                        return Color.Khaki;
                    case "lavender":
                        return Color.Lavender;
                    case "lavenderblush":
                        return Color.LavenderBlush;
                    case "lawngreen":
                        return Color.LawnGreen;
                    case "lemonchiffon":
                        return Color.LemonChiffon;
                    case "lightblue":
                        return Color.LightBlue;
                    case "lightcoral":
                        return Color.LightCoral;
                    case "lightcyan":
                        return Color.LightCyan;
                    case "lightgoldenrodyellow":
                        return Color.LightGoldenrodYellow;
                    case "lightgreen":
                        return Color.LightGreen;
                    case "lightgray":
                        return Color.LightGray;
                    case "lightpink":
                        return Color.LightPink;
                    case "lightsalmon":
                        return Color.LightSalmon;
                    case "lightseagreen":
                        return Color.LightSeaGreen;
                    case "lightskyblue":
                        return Color.LightSkyBlue;
                    case "lightslategray":
                        return Color.LightSlateGray;
                    case "lightsteelblue":
                        return Color.LightSteelBlue;
                    case "lightyellow":
                        return Color.LightYellow;
                    case "lime":
                        return Color.Lime;
                    case "limegreen":
                        return Color.LimeGreen;
                    case "linen":
                        return Color.Linen;
                    case "magenta":
                        return Color.Magenta;
                    case "maroon":
                        return Color.Maroon;
                    case "mediumaquamarine":
                        return Color.MediumAquamarine;
                    case "mediumblue":
                        return Color.MediumBlue;
                    case "mediumorchid":
                        return Color.MediumOrchid;
                    case "mediumpurple":
                        return Color.MediumPurple;
                    case "mediumseagreen":
                        return Color.MediumSeaGreen;
                    case "mediumslateblue":
                        return Color.MediumSlateBlue;
                    case "mediumspringgreen":
                        return Color.MediumSpringGreen;
                    case "mediumturquoise":
                        return Color.MediumTurquoise;
                    case "mediumvioletred":
                        return Color.MediumVioletRed;
                    case "midnightblue":
                        return Color.MidnightBlue;
                    case "mintcream":
                        return Color.MintCream;
                    case "mistyrose":
                        return Color.MistyRose;
                    case "moccasin":
                        return Color.Moccasin;
                    case "navajowhite":
                        return Color.NavajoWhite;
                    case "navy":
                        return Color.Navy;
                    case "oldlace":
                        return Color.OldLace;
                    case "olive":
                        return Color.Olive;
                    case "olivedrab":
                        return Color.OliveDrab;
                    case "orange":
                        return Color.Orange;
                    case "orangered":
                        return Color.OrangeRed;
                    case "orchid":
                        return Color.Orchid;
                    case "palegoldenrod":
                        return Color.PaleGoldenrod;
                    case "palegreen":
                        return Color.PaleGreen;
                    case "paleturquoise":
                        return Color.PaleTurquoise;
                    case "palevioletred":
                        return Color.PaleVioletRed;
                    case "papayawhip":
                        return Color.PapayaWhip;
                    case "peachpuff":
                        return Color.PeachPuff;
                    case "peru":
                        return Color.Peru;
                    case "pink":
                        return Color.Pink;
                    case "plum":
                        return Color.Plum;
                    case "powderblue":
                        return Color.PowderBlue;
                    case "purple":
                        return Color.Purple;
                    case "red":
                        return Color.Red;
                    case "rosybrown":
                        return Color.RosyBrown;
                    case "royalblue":
                        return Color.RoyalBlue;
                    case "saddlebrown":
                        return Color.SaddleBrown;
                    case "salmon":
                        return Color.Salmon;
                    case "sandybrown":
                        return Color.SandyBrown;
                    case "seagreen":
                        return Color.SeaGreen;
                    case "seashell":
                        return Color.SeaShell;
                    case "sienna":
                        return Color.Sienna;
                    case "silver":
                        return Color.Silver;
                    case "skyblue":
                        return Color.SkyBlue;
                    case "slateblue":
                        return Color.SlateBlue;
                    case "slategray":
                        return Color.SlateGray;
                    case "snow":
                        return Color.Snow;
                    case "springgreen":
                        return Color.SpringGreen;
                    case "steelblue":
                        return Color.SteelBlue;
                    case "tan":
                        return Color.Tan;
                    case "teal":
                        return Color.Teal;
                    case "thistle":
                        return Color.Thistle;
                    case "tomato":
                        return Color.Tomato;
                    case "turquoise":
                        return Color.Turquoise;
                    case "violet":
                        return Color.Violet;
                    case "wheat":
                        return Color.Wheat;
                    case "white":
                        return Color.White;
                    case "whitesmoke":
                        return Color.WhiteSmoke;
                    case "yellow":
                        return Color.Yellow;
                    case "yellowgreen":
                        return Color.YellowGreen;
                }
                System.Text.RegularExpressions.MatchCollection matches = reWebColor.Matches(col);
                foreach (System.Text.RegularExpressions.Match match in matches)
                {
                    string red = match.Groups[1].Captures[0].Value;
                    string green = match.Groups[2].Captures[0].Value;
                    string blue = match.Groups[3].Captures[0].Value;
                    return new Color(Convert.ToInt32(red, 16), Convert.ToInt32(green, 16), Convert.ToInt32(blue, 16));
                }
                return Color.Transparent;
            }
            public static string WebColor(Color col)
            {
                if (col == Color.AliceBlue) return "AliceBlue";
                if (col == Color.AntiqueWhite) return "AntiqueWhite";
                if (col == Color.Aqua) return "Aqua";
                if (col == Color.Aquamarine) return "Aquamarine";
                if (col == Color.Azure) return "Azure";
                if (col == Color.Beige) return "Beige";
                if (col == Color.Bisque) return "Bisque";
                if (col == Color.Black) return "Black";
                if (col == Color.BlanchedAlmond) return "BlanchedAlmond";
                if (col == Color.Blue) return "Blue";
                if (col == Color.BlueViolet) return "BlueViolet";
                if (col == Color.Brown) return "Brown";
                if (col == Color.BurlyWood) return "BurlyWood";
                if (col == Color.CadetBlue) return "CadetBlue";
                if (col == Color.Chartreuse) return "Chartreuse";
                if (col == Color.Chocolate) return "Chocolate";
                if (col == Color.Coral) return "Coral";
                if (col == Color.CornflowerBlue) return "CornflowerBlue";
                if (col == Color.Cornsilk) return "Cornsilk";
                if (col == Color.Crimson) return "Crimson";
                if (col == Color.Cyan) return "Cyan";
                if (col == Color.DarkBlue) return "DarkBlue";
                if (col == Color.DarkCyan) return "DarkCyan";
                if (col == Color.DarkGoldenrod) return "DarkGoldenrod";
                if (col == Color.DarkGray) return "DarkGray";
                if (col == Color.DarkGreen) return "DarkGreen";
                if (col == Color.DarkKhaki) return "DarkKhaki";
                if (col == Color.DarkMagenta) return "DarkMagenta";
                if (col == Color.DarkOliveGreen) return "DarkOliveGreen";
                if (col == Color.DarkOrange) return "DarkOrange";
                if (col == Color.DarkOrchid) return "DarkOrchid";
                if (col == Color.DarkRed) return "DarkRed";
                if (col == Color.DarkSalmon) return "DarkSalmon";
                if (col == Color.DarkSeaGreen) return "DarkSeaGreen";
                if (col == Color.DarkSlateBlue) return "DarkSlateBlue";
                if (col == Color.DarkSlateGray) return "DarkSlateGray";
                if (col == Color.DarkTurquoise) return "DarkTurquoise";
                if (col == Color.DarkViolet) return "DarkViolet";
                if (col == Color.DeepPink) return "DeepPink";
                if (col == Color.DeepSkyBlue) return "DeepSkyBlue";
                if (col == Color.DimGray) return "DimGray";
                if (col == Color.DodgerBlue) return "DodgerBlue";
                if (col == Color.Firebrick) return "Firebrick";
                if (col == Color.FloralWhite) return "FloralWhite";
                if (col == Color.ForestGreen) return "ForestGreen";
                if (col == Color.Fuchsia) return "Fuchsia";
                if (col == Color.Gainsboro) return "Gainsboro";
                if (col == Color.GhostWhite) return "GhostWhite";
                if (col == Color.Gold) return "Gold";
                if (col == Color.Goldenrod) return "Goldenrod";
                if (col == Color.Gray) return "Gray";
                if (col == Color.Green) return "Green";
                if (col == Color.GreenYellow) return "GreenYellow";
                if (col == Color.Honeydew) return "Honeydew";
                if (col == Color.HotPink) return "HotPink";
                if (col == Color.IndianRed) return "IndianRed";
                if (col == Color.Indigo) return "Indigo";
                if (col == Color.Ivory) return "Ivory";
                if (col == Color.Khaki) return "Khaki";
                if (col == Color.Lavender) return "Lavender";
                if (col == Color.LavenderBlush) return "LavenderBlush";
                if (col == Color.LawnGreen) return "LawnGreen";
                if (col == Color.LemonChiffon) return "LemonChiffon";
                if (col == Color.LightBlue) return "LightBlue";
                if (col == Color.LightCoral) return "LightCoral";
                if (col == Color.LightCyan) return "LightCyan";
                if (col == Color.LightGoldenrodYellow) return "LightGoldenrodYellow";
                if (col == Color.LightGreen) return "LightGreen";
                if (col == Color.LightGray) return "LightGray";
                if (col == Color.LightPink) return "LightPink";
                if (col == Color.LightSalmon) return "LightSalmon";
                if (col == Color.LightSeaGreen) return "LightSeaGreen";
                if (col == Color.LightSkyBlue) return "LightSkyBlue";
                if (col == Color.LightSlateGray) return "LightSlateGray";
                if (col == Color.LightSteelBlue) return "LightSteelBlue";
                if (col == Color.LightYellow) return "LightYellow";
                if (col == Color.Lime) return "Lime";
                if (col == Color.LimeGreen) return "LimeGreen";
                if (col == Color.Linen) return "Linen";
                if (col == Color.Magenta) return "Magenta";
                if (col == Color.Maroon) return "Maroon";
                if (col == Color.MediumAquamarine) return "MediumAquamarine";
                if (col == Color.MediumBlue) return "MediumBlue";
                if (col == Color.MediumOrchid) return "MediumOrchid";
                if (col == Color.MediumPurple) return "MediumPurple";
                if (col == Color.MediumSeaGreen) return "MediumSeaGreen";
                if (col == Color.MediumSlateBlue) return "MediumSlateBlue";
                if (col == Color.MediumSpringGreen) return "MediumSpringGreen";
                if (col == Color.MediumTurquoise) return "MediumTurquoise";
                if (col == Color.MediumVioletRed) return "MediumVioletRed";
                if (col == Color.MidnightBlue) return "MidnightBlue";
                if (col == Color.MintCream) return "MintCream";
                if (col == Color.MistyRose) return "MistyRose";
                if (col == Color.Moccasin) return "Moccasin";
                if (col == Color.NavajoWhite) return "NavajoWhite";
                if (col == Color.Navy) return "Navy";
                if (col == Color.OldLace) return "OldLace";
                if (col == Color.Olive) return "Olive";
                if (col == Color.OliveDrab) return "OliveDrab";
                if (col == Color.Orange) return "Orange";
                if (col == Color.OrangeRed) return "OrangeRed";
                if (col == Color.Orchid) return "Orchid";
                if (col == Color.PaleGoldenrod) return "PaleGoldenrod";
                if (col == Color.PaleGreen) return "PaleGreen";
                if (col == Color.PaleTurquoise) return "PaleTurquoise";
                if (col == Color.PaleVioletRed) return "PaleVioletRed";
                if (col == Color.PapayaWhip) return "PapayaWhip";
                if (col == Color.PeachPuff) return "PeachPuff";
                if (col == Color.Peru) return "Peru";
                if (col == Color.Pink) return "Pink";
                if (col == Color.Plum) return "Plum";
                if (col == Color.PowderBlue) return "PowderBlue";
                if (col == Color.Purple) return "Purple";
                if (col == Color.Red) return "Red";
                if (col == Color.RosyBrown) return "RosyBrown";
                if (col == Color.RoyalBlue) return "RoyalBlue";
                if (col == Color.SaddleBrown) return "SaddleBrown";
                if (col == Color.Salmon) return "Salmon";
                if (col == Color.SandyBrown) return "SandyBrown";
                if (col == Color.SeaGreen) return "SeaGreen";
                if (col == Color.SeaShell) return "SeaShell";
                if (col == Color.Sienna) return "Sienna";
                if (col == Color.Silver) return "Silver";
                if (col == Color.SkyBlue) return "SkyBlue";
                if (col == Color.SlateBlue) return "SlateBlue";
                if (col == Color.SlateGray) return "SlateGray";
                if (col == Color.Snow) return "Snow";
                if (col == Color.SpringGreen) return "SpringGreen";
                if (col == Color.SteelBlue) return "SteelBlue";
                if (col == Color.Tan) return "Tan";
                if (col == Color.Teal) return "Teal";
                if (col == Color.Thistle) return "Thistle";
                if (col == Color.Tomato) return "Tomato";
                if (col == Color.Turquoise) return "Turquoise";
                if (col == Color.Violet) return "Violet";
                if (col == Color.Wheat) return "Wheat";
                if (col == Color.White) return "White";
                if (col == Color.WhiteSmoke) return "WhiteSmoke";
                if (col == Color.Yellow) return "Yellow";
                if (col == Color.YellowGreen) return "YellowGreen";
                if (col.A > 0)
                    return string.Format("#{0:x2}{1:x2}{2:x2}{3:x2}", col.R, col.G, col.B, col.A);
                return string.Format("#{0:x2}{1:x2}{2:x2}", col.R, col.G, col.B);
            }

            /// <summary>
            /// This represents a single section within a <see cref="IngameScript.Config"/>.
            /// </summary>
            public class ConfigSection : IConfigSection
            {
                private readonly Config parent;
                private readonly string section;

                /// <summary>
                /// Constructor - get a section from an existing <see cref="IngameScript.Config"/>
                /// </summary>
                /// <param name="myParent">Parent <see cref="IngameScript.Config"/></param>
                /// <param name="mySection">Section to use within the config.</param>
                public ConfigSection(Config myParent, string mySection)
                {
                    parent = myParent;
                    section = mySection;
                }
                /// <summary>
                /// Constructor - get a section directly from a block.
                /// </summary>
                /// <param name="start">An <see cref="IMyTerminalBlock"/> to get the config from.</param>
                /// <param name="mySection">Section to use within the config.</param>
                public ConfigSection(IMyTerminalBlock start, string mySection)
                {
                    parent = new Config(start);
                    section = mySection;
                }
                /// <summary>
                /// Constructor - get a section from an ini string.
                /// </summary>
                /// <param name="start">String form of configuration</param>
                /// <param name="mySection">Section to use within the config.</param>
                public ConfigSection(string start, string mySection)
                {
                    parent = new Config(start);
                    section = mySection;
                }
                /// <summary>
                /// Constructor - get a section from an existing <see cref="MyIni"/>
                /// </summary>
                /// <remarks>
                /// The supplied MyIni will be copied and updates to this ConfigSection will NOT update the MyIni.
                /// </remarks>
                /// <param name="start"><see cref="MyIni"/> to initialise the configuration</param>
                /// <param name="mySection">Section to use within the config.</param>
                public ConfigSection(MyIni start, string mySection)
                {
                    parent = new Config(start);
                    section = mySection;
                }

                public bool IsReadOnly() => false;

                public ConfigSectionKey Key(string key) => new ConfigSectionKey(this, key);

                /// <returns>Returns the <see cref="IngameScript.Config"/> that this section is part of.</returns>
                public Config Config => parent;

                /// <summary>
                /// Check if a specific key exists in this section.
                /// </summary>
                /// <param name="key">Key to check for</param>
                /// <returns>Wether or not the key exists</returns>
                public bool ContainsKey(string key) => parent.ContainsKey(section, key);

                /// <summary>
                /// Set a key to a specified value.
                /// </summary>
                /// <param name="key">Name of key to set.</param>
                /// <param name="value">Value to set.</param>
                public void Set(string key, string value) => parent.Set(section, key, value);
                public void Set(string key, bool value) => parent.Set(section, key, value);
                public void Set(string key, int value) => parent.Set(section, key, value);
                public void Set(string key, float value) => parent.Set(section, key, value);
                public void Set(string key, Color value) => parent.Set(section, key, WebColor(value));

                public void SetComment(string key, string comment) => parent.SetComment(section, key, comment);
                public string GetComment(string key) => parent.GetComment(section, key);

                /// <summary>
                /// Retrieve a key, setting it to a default if it is missing.
                /// </summary>
                /// <param name="key">Key to retrieve</param>
                /// <param name="defaultvalue">Default value if key is missing.</param>
                /// <returns>value of key or default value.</returns>
                public string Get(string key, string defaultvalue)
                {
                    if (!ContainsKey(key))
                        Set(key, defaultvalue);
                    return parent.Get(section, key).ToString();
                }
                public int Get(string key, int defaultvalue)
                {
                    if (!ContainsKey(key))
                        Set(key, defaultvalue);
                    return parent.Get(section, key).ToInt32();
                }
                public float Get(string key, float defaultvalue)
                {
                    if (!ContainsKey(key))
                        Set(key, defaultvalue);
                    return parent.Get(section, key).ToSingle();
                }
                public bool Get(string key, bool defaultvalue)
                {
                    if (!ContainsKey(key))
                        Set(key, defaultvalue);
                    return parent.Get(section, key).ToBoolean(defaultvalue);
                }
                public Color Get(string key, Color defaultvalue)
                {
                    if (!ContainsKey(key))
                        Set(key, defaultvalue);
                    return GetColor(key);
                }
                public MyIniValue Get(string key) => parent.Get(section, key);
                public string GetString(string key) => Get(key).ToString();
                public int GetInt(string key) => Get(key).ToInt32();
                public float GetFloat(string key) => Get(key).ToSingle();
                public bool GetBool(string key) => Get(key).ToBoolean();
                public Color GetColor(string key) => WebColor(GetString(key));

                /// <summary>
                /// Shorthand for <c>value = section.Get(key,value);</c>
                /// </summary>
                /// <param name="key">key to update</param>
                /// <param name="value">value to use as default and to update</param>
                public void Get(string key, ref string value) => value = Get(key, value);
                public void Get(string key, ref int value) => value = Get(key, value);
                public void Get(string key, ref float value) => value = Get(key, value);
                public void Get(string key, ref bool value) => value = Get(key, value);
                public void Get(string key, ref Color value) => value = Get(key, value);

                /// <summary>
                /// Set key to a default value if it has not been set already.
                /// </summary>
                /// <param name="key">key to set</param>
                /// <param name="value">default value to use</param>
                public void Default(string key, string value) => Get(key, value);
                public void Default(string key, int value) => Get(key, value);
                public void Default(string key, float value) => Get(key, value);
                public void Default(string key, bool value) => Get(key, value);
                public void Default(string key, Color value) => Get(key, value);

                public void Delete(string key) => parent.Delete(section, key);

                /// <summary>
                /// Save ENTIRE config to block.
                /// </summary>
                /// <seealso cref="Config.Save(IMyTerminalBlock)"/>
                /// <param name="block">block to save configuration to.</param>
                public void Save(IMyTerminalBlock block) => parent.Save(block);
                /// <summary>
                /// Save ENTIRE config to the block used to create the config.
                /// </summary>
                /// <seealso cref="Config.Save()"/>
                /// <exception cref="NoBlockSpecified">Thrown if this config was not created using an <see cref="IMyTerminalBlock"/></exception>
                public void Save() => parent.Save();

                public List<string> GetKeys()
                {
                    List<MyIniKey> iniKeys = new List<MyIniKey>();
                    parent.GetKeys(iniKeys);
                    return iniKeys.FindAll(ik => ik.Section == section).ConvertAll(ik => ik.Name);
                }

            }

            private readonly IMyTerminalBlock termBlock;
            private string original;
            private bool suppressComments;
            private readonly HashSet<string> deleteComments;

            /// <summary>
            /// Removing comments is currently a little hackish
            /// </summary>
            /// <param name="set">set Suppress mode or not</param>
            /// <returns>Previous suppress mode</returns>
            public bool SuppressComments(bool set = true)
            {
                bool ret = suppressComments;
                if (ret != set)
                    deleteComments.Clear();
                suppressComments = set;
                return ret;
            }

            /// <summary>
            /// Create a <see cref="ConfigSection"/> directly from a config source.
            /// </summary>
            /// <param name="start">A <see cref="IMyTerminalBlock"/>, <see cref="MyIni"/>, or string</param>
            /// <param name="section">The name of the section to use.</param>
            /// <returns>A <see cref="ConfigSection"/></returns>
            public static ConfigSection Section(IMyTerminalBlock start, string section) => new Config(start).Section(section);
            public static ConfigSection Section(MyIni start, string section) => new Config(start).Section(section);
            public static ConfigSection Section(string start, string section) => new Config(start).Section(section);

            /// <summary>
            /// Parse configuration from a terminal block.
            /// Additionally block is stored to be used with <see cref="Save()"/> later on.
            /// </summary>
            /// <param name="block">The <see cref="IMyTerminalBlock"/> to retrieve the config from.</param>
            public Config(IMyTerminalBlock block) : this()
            {
                termBlock = block;
                Load(block);
            }

            /// <summary>
            /// Parse configuration from a string.
            /// </summary>
            /// <param name="defaultini">string containing config.</param>
            public Config(string defaultini) : this()
            {
                Load(defaultini);
            }
            /// <summary>
            /// Parse configuration from an existing <see cref="MyIni"/>
            /// </summary>
            /// <remarks>
            /// The <see cref="MyIni"/> will be copied and will not recieve updates.
            /// </remarks>
            /// <param name="defaultini"><see cref="MyIni"/> to base config on.</param>
            public Config(MyIni defaultini) : this()
            {
                Load(defaultini);
            }
            /// <summary>
            /// Create an empty configuration.
            /// </summary>
            /// <remarks>
            /// You can then use <see cref="Load(IMyTerminalBlock)"/> to load a config if desired.
            /// </remarks>
            public Config() : base()
            {
                suppressComments = false;
                deleteComments = new HashSet<string>();
            }

            /// <summary>
            /// Load configuration from a block. If there is no configuration on the block then
            /// first saves any existing configuration to the block.
            /// </summary>
            /// <remarks>
            /// Will clear any existing configuration first.
            /// </remarks>
            /// <param name="block"></param>
            public void Load(IMyTerminalBlock block)
            {
                if (block.CustomData.Length == 0)
                {
                    block.CustomData = this.ToString();
                }
                this.Clear();
                this.TryParse(block.CustomData);
                original = this.ToString();
            }
            /// <summary>
            /// Load configuration from an existing <see cref="MyIni"/>
            /// </summary>
            /// <remarks>
            /// Will copy the config from the <see cref="MyIni"/>. Updates will not be sent to the original
            /// ini.
            /// </remarks>
            /// <param name="defaultini"><see cref="MyIni"/> to load from.</param>
            public void Load(MyIni defaultini)
            {
                this.Clear();
                this.TryParse(defaultini.ToString());
                original = this.ToString();
            }
            /// <summary>
            /// Load configuration from a string.
            /// </summary>
            /// <param name="defaultini">string containing config.</param>
            public void Load(string defaultini)
            {
                this.Clear();
                this.TryParse(defaultini);
                original = this.ToString();
            }

            /// <summary>
            /// Reload the config from the current block, discarding any changes or default settings.
            /// </summary>
            /// <exception cref="NoBlockSpecified">Thrown if this config was not created using an <see cref="IMyTerminalBlock"/></exception>
            public void Reload() {
                if (termBlock != null)
                    Load(termBlock);
                else
                    throw new NoBlockSpecified("Trying to reload without having a block to load from!");
            }
            public void Reload(IMyTerminalBlock block) => Load(block);
            public void Reload(MyIni ini) => Load(ini);
            public void Reload(string ini) => Load(ini);

            /// <summary>
            /// Save the current configuration to the given block.
            /// </summary>
            /// <param name="block"></param>
            public void Save(IMyTerminalBlock block)
            {
                string output = this.ToString();
                if (deleteComments.Any())
                    output = string.Join("\n", new List<string>(output.Split('\n')).FindAll(l => !deleteComments.Contains(l.Length > 1 ? l.Substring(1) : l)));
                if (output != original)
                {
                    block.CustomData = output;
                    original = output;
                }
            }
            public class NoBlockSpecified : Exception
            {
                public NoBlockSpecified(string msg) : base(msg) { }
            }
            /// <summary>
            /// Save to the block that this config was created with.
            /// </summary>
            /// <exception cref="NoBlockSpecified">Thrown if this config was not created using an <see cref="IMyTerminalBlock"/></exception>
            public void Save()
            {
                if (termBlock != null)
                    Save(termBlock);
                else
                    throw new NoBlockSpecified("Trying to save without having a block to save to!");
            }

            /// <summary>
            /// Retrieve (create) a <see cref="ConfigSection"/>.
            /// </summary>
            /// <param name="section">Name of section to connect to</param>
            /// <returns>Object representing the specified section.</returns>
            public ConfigSection Section(string section)
            {
                return new ConfigSection(this, section);
            }

            public void DeleteComment(string section, string key, string comment) => deleteComments.Add(comment);
            public void DeleteComment(MyIniKey key, string comment) => DeleteComment(key.Section, key.Name, comment);
            public new void SetComment(string section, string key, string comment)
            {
                if (suppressComments)
                    DeleteComment(section, key, comment);
                else
                    base.SetComment(section, key, comment);
            }
            public new void SetComment(MyIniKey key, string comment)
            {
                if (suppressComments)
                    DeleteComment(key, comment);
                else
                    base.SetComment(key, comment);
            }
        }
    }
}
