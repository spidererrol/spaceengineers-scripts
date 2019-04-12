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
        /// How many seconds to leave a door open when a user has opened it?
        /// </summary>
        public int OpenSeconds = 11;

        Dictionary<string, int> rooms = new Dictionary<string, int>();
        Dictionary<Vector3I, DoorStatus> doorState = new Dictionary<Vector3I, DoorStatus>();
        Dictionary<Vector3I, TimeSpan> doorToClose = new Dictionary<Vector3I, TimeSpan>();

        public TimeSpan ticker = TimeSpan.FromSeconds(0);


        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        //public void Save() {
        //    // Called when the program needs to save its state. Use
        //    // this method to save your state to the Storage field
        //    // or some other means. 
        //    // 
        //    // This method is optional and can be removed if not
        //    // needed.
        //}

        public void InitSurface()
        {
            IMyTextSurface surface = Me.GetSurface(0);
            surface.ContentType = ContentType.TEXT_AND_IMAGE;
            surface.WriteText("", false);
        }

        public void Emit(string msg)
        {
            IMyTextSurface surface = Me.GetSurface(0);
            surface.WriteText(msg + "\n", true);
            Echo(msg);
        }

        System.Text.RegularExpressions.Regex reWebColor = new System.Text.RegularExpressions.Regex(@"^#?([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$");

        public Color webColor(string col)
        {
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

        public string webColor(Color col)
        {
            string scol = "#";
            scol += col.R.ToString("X2");
            scol += col.G.ToString("X2");
            scol += col.B.ToString("X2");
            return scol;
        }

        public void updateLights(List<IMyInteriorLight> lights, bool isSealed, bool inverted)
        {
            if (!lights.Any())
                return;
            if (inverted)
                isSealed = !isSealed;
            foreach (IMyInteriorLight light in lights)
            {
                Config.ConfigSection sec = Config.Section(light,"EDC");
                string sSealedColor = sec.Get("SealedColor", "Off");
                string sLeakColor = sec.Get("LeakColor", "On");
                sec.Save();
                string sTargetColor = isSealed ? sSealedColor : sLeakColor;
                sTargetColor = sTargetColor.ToLower();
                if (sTargetColor == "on")
                    RunActions(light, "OnOff_On");
                else if (sTargetColor == "off")
                    RunActions(light, "OnOff_Off");
                else
                {
                    RunActions(light, "OnOff_On");
                    light.Color = webColor(sTargetColor);
                }
            }
        }

        public void Main(string argument)
        {
            Config.ConfigSection config = Config.Section(Me,"EDC");
            OpenSeconds = config.Get("OpenSeconds", OpenSeconds);
            string prefix = config.Get("Prefix", "[#");
            string suffix = config.Get("Suffix", "]");
            config.Save();

            string sTags = System.Text.RegularExpressions.Regex.Escape(prefix) + @"(.+?)(!?)" + System.Text.RegularExpressions.Regex.Escape(suffix);
            System.Text.RegularExpressions.Regex reTags = new System.Text.RegularExpressions.Regex(sTags);

            InitSurface();
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
                List<IMyDoor> blks = getObjectsByName<IMyDoor>("Shutter");
                for (int i = 0; i < blks.Count; i++)
                {
                    blks[i].CustomName = blks[i].CustomName + " " + prefix + "ControlRoom" + suffix;
                }
            }
            Dictionary<string, int> newrooms = new Dictionary<string, int>();
            List<IMyAirVent> vents = getObjectsByName<IMyAirVent>(prefix);
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
                updateLights(getObjectsByName<IMyInteriorLight>(prefix + roomKey + suffix), isSealed, false);
                updateLights(getObjectsByName<IMyInteriorLight>(prefix + roomKey + "!" + suffix), isSealed, true);
            }
            Emit(roomKeys.Count + " rooms found");
            List<IMyDoor> doors = getObjectsByName<IMyDoor>(prefix);
            for (int i = 0; i < doors.Count; i++)
            {
                IMyDoor door = doors[i];
                System.Text.RegularExpressions.MatchCollection matches = reTags.Matches(door.CustomName);
                int shouldClose = 0;
                bool sealChanged = false;
                bool doOpen = false;
                for (int m = 0; m < matches.Count; m++)
                {
                    System.Text.RegularExpressions.Match match = matches[m];
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
                Config.ConfigSection doorini = Config.Section(door,"EDC");
                doOpen = !doorini.Get("StayClosed", !doOpen);
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
                    RunActions(door, "Open_Off");
                    doorState[door.Position] = DoorStatus.Closing;
                }
                else if (doOpen && shouldClose == 0 && door.Status == DoorStatus.Closed)
                {
                    Emit("Opening " + door.CustomName);
                    RunActions(door, "Open_On");
                    doorState[door.Position] = DoorStatus.Opening;
                }
            }
            rooms = newrooms;
        }

    }
}