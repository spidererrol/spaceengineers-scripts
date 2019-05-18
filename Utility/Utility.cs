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
        public static class Utility
        {
            private const string MULTIPLIERS = ".kMGTPEZY";
            private static readonly System.Text.RegularExpressions.Regex reHUnit = new System.Text.RegularExpressions.Regex(@"^(\d*\.\d+|\d+)\s*([kMGTPEZY])(.*)$");
            private static readonly System.Text.RegularExpressions.Regex reJumpDriveMaxPower = new System.Text.RegularExpressions.Regex("Max Stored Power: (\\d+\\.?\\d*) (\\w?)Wh", System.Text.RegularExpressions.RegexOptions.Singleline);
            private static readonly System.Text.RegularExpressions.Regex reJumpDriveCurPower = new System.Text.RegularExpressions.Regex("Stored power: (\\d+\\.?\\d*) (\\w?)Wh", System.Text.RegularExpressions.RegexOptions.Singleline);

            public static void UpdateDict<keyType, valType>(IDictionary<keyType, valType> dict, keyType key, valType val)
            {
                if (dict.ContainsKey(key))
                    dict[key] = val;
                else
                    dict.Add(key, val);
            }

            public static double HUnitToDouble(string hunit, bool brokenSi = false)
            {
                System.Text.RegularExpressions.Regex re;
                if (brokenSi) re = new System.Text.RegularExpressions.Regex(reHUnit.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                else re = reHUnit;
                System.Text.RegularExpressions.Match match = re.Match(hunit);
                if (match.Success)
                {
                    double parsedDouble;
                    if (double.TryParse(match.Groups[1].Value, out parsedDouble))
                    {
                        return parsedDouble * Math.Pow(1000.0, MULTIPLIERS.IndexOf(match.Groups[2].Value));
                    }
                    else
                    {
                        throw new Exception("Cannot parse '" + match.Groups[1].Value + "' as a double in " + hunit);
                    }
                }
                else
                {
                    double parsedDouble;
                    if (double.TryParse(hunit, out parsedDouble))
                    {
                        return parsedDouble;
                    }
                    else
                    {
                        throw new Exception("Cannot parse '" + hunit + "' as a double");
                    }
                }
            }

            public static string DoubleToHUnit(double value, double demultiplier = 1, string format = "N", string seperator = "")
            {
                string si = MULTIPLIERS;
                while (si.Length > 1)
                {
                    double mul = Math.Pow(1000.0, si.Length - 1);
                    if (value >= mul)
                    {
                        return (value / mul).ToString(format) + seperator + si.Last().ToString();
                    }
                    si = si.Substring(0, si.Length - 1);
                }
                return value.ToString(format);
            }

            /// <summary>
            /// Returns the percentage charge the specified jump drive.
            /// </summary>
            /// <param name="drive">Jump drive to calculate</param>
            /// <returns>Percentage of charge in the jump drive</returns>
            public static double JumpDriveChargePercent(IMyJumpDrive drive)
            {
                double maxPower = 0;
                double curPower = 0;
                System.Text.RegularExpressions.Match match = reJumpDriveMaxPower.Match(drive.DetailedInfo);
                double parsedDouble = 0.0;
                if (match.Success)
                {
                    if (Double.TryParse(match.Groups[1].Value, out parsedDouble))
                    {
                        maxPower = parsedDouble * Math.Pow(1000.0, MULTIPLIERS.IndexOf(match.Groups[2].Value));
                    }
                }

                match = reJumpDriveCurPower.Match(drive.DetailedInfo);
                if (match.Success)
                {
                    if (Double.TryParse(match.Groups[1].Value, out parsedDouble))
                    {
                        curPower = parsedDouble * Math.Pow(1000.0, MULTIPLIERS.IndexOf(match.Groups[2].Value));
                    }
                }
                return ((curPower / maxPower) * 100);
            }

            /// <summary>
            /// Run the action on all blocks on the list which match the filter.
            /// </summary>
            /// <typeparam name="MyType">An IMyTerminalBlock sub-type (implied by blocks).</typeparam>
            /// <param name="blocks">List of blocks to apply action to.</param>
            /// <param name="action">String name of the action.</param>
            /// <param name="filter">Filter function.</param>
            public static void RunActions<MyType>(List<MyType> blocks, string action, Func<MyType, bool> filter = null) where MyType : IMyTerminalBlock
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    if (filter == null || filter.Invoke(blocks[i]))
                    {
                        ITerminalAction off = blocks[i].GetActionWithName(action);
                        off.Apply(blocks[i]);
                    }
                }
            }

            /// <summary>
            /// Run action on the given block.
            /// </summary>
            /// <param name="block">The block to act on.</param>
            /// <param name="action">String name of action to apply.</param>
            public static void RunActions(IMyTerminalBlock block, string action)
            {
                ITerminalAction act = block.GetActionWithName(action);
                act.Apply(block);
            }

            public static IType TryType<IType>(IMyCubeBlock item)
            {
                try
                {
                    return (IType)item;
                }
                catch
                {
                    return default(IType);
                }
            }

            public static bool IsType<IType>(IMyCubeBlock item)
            {
                if (TryType<IType>(item) == null)
                    return false;
                else
                    return true;
            }

            public static string List2String<T>(IList<T> list)
            {
                string ret = "";
                for (int i = 0; i < list.Count; i++)
                {
                    string item = list[i].ToString();
                    if (i > 0)
                        ret += ", ";
                    ret += item;
                }
                return ret;
            }

            public static string List2String<T>(T[] list)
            {
                return List2String(new List<T>(list));
            }

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
        }
    }
}
