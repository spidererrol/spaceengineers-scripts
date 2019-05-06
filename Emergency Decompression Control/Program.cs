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
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        /// <summary>
        /// Section name to use when reading config from Custom Data.
        /// </summary>
        public const string SectionName = "EDC";

        /// <summary>
        /// How many seconds to leave a door open when a user has opened it?
        /// </summary>
        /// <remarks>
        /// This can now be updated via the "Custom Data" configuration.
        /// </remarks>
        public int OpenSeconds = 11;

        Dictionary<string, int> rooms = new Dictionary<string, int>();
        Dictionary<Vector3I, DoorStatus> doorState = new Dictionary<Vector3I, DoorStatus>();
        Dictionary<Vector3I, TimeSpan> doorToClose = new Dictionary<Vector3I, TimeSpan>();

        public TimeSpan ticker = TimeSpan.FromSeconds(0);

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        ConsoleSurface console;

        public void Save() => console = null;

        public void Emit(string msg)
        {
            if (console == null)
                console = new ConsoleSurface(this, 0);
            console.Echo(msg);
        }


        System.Text.RegularExpressions.Regex reWebColor = new System.Text.RegularExpressions.Regex(@"^#?([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$");

        public Color webColor(string col)
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

        public void updateLights(List<IMyInteriorLight> lights, bool isSealed, bool inverted)
        {
            if (!lights.Any())
                return;
            if (inverted)
                isSealed = !isSealed;
            foreach (IMyInteriorLight light in lights)
            {
                Config.ConfigSection sec = Config.Section(light, SectionName);
                string sSealedColor = sec.Get("SealedColor", "Off");
                string sLeakColor = sec.Get("LeakColor", "On");
                sec.Save();
                string prefix = isSealed ? "Sealed" : "Leak";
                string sTargetColor = isSealed ? sSealedColor : sLeakColor;
                sTargetColor = sTargetColor.ToLower();
                if (sTargetColor == "on")
                    Utility.RunActions(light, "OnOff_On");
                else if (sTargetColor == "off")
                    Utility.RunActions(light, "OnOff_Off");
                else
                {
                    Utility.RunActions(light, "OnOff_On");
                    light.Color = webColor(sTargetColor);
                }
                foreach (string attrib in new string[] { "BlinkLength", "BlinkIntervalSeconds", "BlinkOffset" })
                {
                    string key = prefix + attrib;
                    if (sec.ContainsKey(key))
                        light.SetValue(attrib, sec.Get(key, 0.0f));
                }
            }
        }

        public void Main(string argument)
        {
            Config.ConfigSection config = Config.Section(Me, SectionName);
            config.Get("OpenSeconds", ref OpenSeconds);
            string prefix = config.Get("Prefix", "[#");
            string suffix = config.Get("Suffix", "]");
            string consoletag = config.Get("ConsoleTo", "##EDC##");
            bool useecho = config.Get("Echo", true);
            bool useself = config.Get("Self", true);
            config.Save();

            console = ConsoleSurface.EasyConsole(this, consoletag, SectionName, useecho, useself);

            string sTags = System.Text.RegularExpressions.Regex.Escape(prefix) + @"(.+?)(!?)" + System.Text.RegularExpressions.Regex.Escape(suffix);
            System.Text.RegularExpressions.Regex reTags = new System.Text.RegularExpressions.Regex(sTags);

            Emit("Emergency Decompression Control\n-------------------------------");
            if (doorToClose.Count > 0)
            {
                List<Vector3I> keys = new List<Vector3I>(doorToClose.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    if (doorToClose[keys[i]] > ticker + TimeSpan.FromSeconds(OpenSeconds * 2))
                    {
                        // Time has gone backwards (probably game loaded) - close door now.
                        doorToClose[keys[i]] = new TimeSpan(0);
                        ;
                    }
                    else if (doorToClose[keys[i]] < ticker)
                    {
                        // This should have been closed and deleted in the last tick. Clean up as door may have been deleted.
                        doorToClose.Remove(keys[i]);
                    }
                }
            }
            ticker += Runtime.TimeSinceLastRun;
            if (argument == "test")
            {
                List<IMyDoor> blks = GetBlocks.ByName<IMyDoor>("Shutter");
                for (int i = 0; i < blks.Count; i++)
                {
                    blks[i].CustomName = blks[i].CustomName + " " + prefix + "ControlRoom" + suffix;
                }
            }
            Dictionary<string, int> newrooms = new Dictionary<string, int>();
            List<IMyAirVent> vents = GetBlocks.ByName<IMyAirVent>(prefix);
            for (int i = 0; i < vents.Count; i++)
            {
                IMyAirVent vent = vents[i];
                int vstate = vent.CanPressurize ? 0 : 1;
                if (vent.Depressurize)
                    vstate = 1;
                System.Text.RegularExpressions.MatchCollection matches = reTags.Matches(vent.CustomName);
                for (int m = 0; m < matches.Count; m++)
                {
                    System.Text.RegularExpressions.Match match = matches[m];
                    string roomname = match.Groups[1].Captures[0].Value;
                    if (newrooms.ContainsKey(roomname))
                    {
                        newrooms[roomname] += vstate;
                    }
                    else
                    {
                        newrooms.Add(roomname, vstate);
                    }
                }
            }
            if (rooms.Count == 0)
            {
                // Shortcut for first run:
                rooms = newrooms;
                return;
            }
            List<string> roomKeys = new List<string>(rooms.Keys);
            for (int i = 0; i < roomKeys.Count; i++)
            {
                string roomKey = roomKeys[i];
                if (!newrooms.ContainsKey(roomKey))
                    continue;
                bool wasSealed = rooms[roomKey] == 0;
                bool isSealed = newrooms[roomKey] == 0;
                if (isSealed)
                    Emit(roomKey + " is presurised");
                else
                    Emit(roomKey + " is leaking");
                if (wasSealed == isSealed)
                    continue;
                /*if (isSealed)
                {
                    Emit(roomKey + " has represurised");
                    //RunActions(getObjectsByName<IMyInteriorLight>(prefix + roomKey + suffix), "OnOff_Off");
                    //RunActions(getObjectsByName<IMyInteriorLight>(prefix + roomKey + "!" + suffix), "OnOff_On");
                }
                else
                {
                    //RunActions(getObjectsByName<IMyInteriorLight>(prefix + roomKey + suffix), "OnOff_On");
                    //RunActions(getObjectsByName<IMyInteriorLight>(prefix + roomKey + "!" + suffix), "OnOff_Off");
                }*/
                updateLights(GetBlocks.ByName<IMyInteriorLight>(prefix + roomKey + suffix), isSealed, false);
                updateLights(GetBlocks.ByName<IMyInteriorLight>(prefix + roomKey + "!" + suffix), isSealed, true);
            }
            Emit(roomKeys.Count + " rooms found");
            List<IMyDoor> doors = GetBlocks.ByName<IMyDoor>(prefix);
            for (int i = 0; i < doors.Count; i++)
            {
                IMyDoor door = doors[i];
                System.Text.RegularExpressions.MatchCollection matches = reTags.Matches(door.CustomName);
                int shouldClose = 0;
                bool sealChanged = false;
                bool doOpen = false;
                bool stayClosed = false;
                for (int m = 0; m < matches.Count; m++)
                {
                    System.Text.RegularExpressions.Match match = matches[m];
                    if (match.Groups[2].Captures[0].Value == "!")
                        stayClosed = true;
                    string roomname = match.Groups[1].Captures[0].Value;
                    if (!newrooms.ContainsKey(roomname))
                    {
                        Emit("No vent found for room " + roomname);
                        newrooms.Add(roomname, 1); // Treat it as "unsealed".
                    }
                    if (!rooms.ContainsKey(roomname))
                    {
                        Emit("No history found for room " + roomname);
                        rooms.Add(roomname, 1); // Start it up "unsealed".
                    }
                    if (argument == "test")
                    { // This will only persist for one cycle so is not really a "lockdown".
                        newrooms[roomname]++;
                    }
                    bool wasSealed = rooms[roomname] == 0;
                    bool isSealed = newrooms[roomname] == 0;
                    if (!isSealed)
                        shouldClose++;
                    if (wasSealed == isSealed)
                        continue;
                    sealChanged = true;
                    if (match.Groups[2].Captures[0].Value != "!")
                        doOpen = true;
                }
                Config.ConfigSection doorini = Config.Section(door, SectionName);
                if (doOpen)
                    doOpen = !doorini.Get("StayClosed", stayClosed);
                doorini.Save();

                if (!doorState.ContainsKey(door.Position))
                    doorState.Add(door.Position, door.Status);
                if (doorState[door.Position] == DoorStatus.Closing || doorState[door.Position] == DoorStatus.Opening)
                    doorState[door.Position] = door.Status;
                if (doorState[door.Position] == DoorStatus.Closed && door.Status == DoorStatus.Open)
                {
                    // User has opened the door, defer closing it again until timeout has passed.
                    if (doorToClose.ContainsKey(door.Position))
                    {
                        if (doorToClose[door.Position] > ticker)
                        {
                            continue; // Skip this door for now.
                        }
                        else
                        {
                            doorToClose.Remove(door.Position);
                            sealChanged = true; // Operate the door.
                        }
                    }
                    else
                    {
                        doorToClose.Add(door.Position, ticker + TimeSpan.FromSeconds(OpenSeconds));
                        continue;
                    }

                }
                if (!sealChanged)
                    continue;
                if (shouldClose > 0 && (door.Status == DoorStatus.Open))
                {
                    Emit("Closing " + door.CustomName);
                    Utility.RunActions(door, "Open_Off");
                    doorState[door.Position] = DoorStatus.Closing;
                }
                else if (doOpen && shouldClose == 0 && door.Status == DoorStatus.Closed)
                {
                    Emit("Opening " + door.CustomName);
                    Utility.RunActions(door, "Open_On");
                    doorState[door.Position] = DoorStatus.Opening;
                }
            }
            rooms = newrooms;
        }

    }
}