using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {

        #region mdk macros
        /*
         * Emergency Decompression Control Mk.1
         * Version: $MDK_DATETIME$
         */
        public const string VerString = "$MDK_DATETIME$";
        #endregion mdk macros

        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        public void Save() {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        string tagStart;
        string tagSep;
        string tagEnd;
        bool thisGrid;
        int doorTimeout;

        ConsoleSurface console;

        string GetUntil(ref string remaining, params string[] terms) {
            int best = int.MaxValue;
            string bestterm = null;
            foreach (string term in terms) {
                int hit = remaining.IndexOf(term);
                if (hit < 0)
                    continue;
                if (hit < best) {
                    bestterm = term;
                    best = hit;
                }
            }
            if (bestterm == null)
                return null;
            string result = remaining.Substring(0, best);
            remaining = remaining.Remove(0, best + bestterm.Length);
            return result;
        }

        Dictionary<string, VentStatus> compartments = new Dictionary<string, VentStatus>();
        struct DoorStateTime
        {
            public DoorStatus status;
            public DateTime expires;
        }
        Dictionary<long, DoorStateTime> doorstates = new Dictionary<long, DoorStateTime>();

        public void Main(string argument, UpdateType updateSource) {
            DateTime now = DateTime.Now;
            Config config = new Config(Me);
            IConfigSection edcConfig = config.Section("EDC");
            string rawTagStart = edcConfig.Get("tagStart", "[");
            string tagName = edcConfig.Get("tagName", "EDC");
            tagStart = rawTagStart + tagName;
            tagSep = edcConfig.Get("tagSep", ":");
            tagEnd = edcConfig.Get("tagEnd", "]");
            tagStart += tagSep; // I always want the seperator after the start of the tag :)
            thisGrid = !edcConfig.Get("allGrids", false);
            doorTimeout = edcConfig.Get("doorTimeout", 15);
            edcConfig.SetComment("doorTimeout", "Seconds a door will be left alone after a person has operated it");
            edcConfig.Save();

            console = ConsoleSurface.EasyConsole(this, rawTagStart + tagName + tagEnd, "Console");
            console.ClearScreen();
            console.Echo("Emergency Decompression Control Mk.1");
            console.Echo("Build: " + VerString);

            //Dictionary<string, VentStatus> prevcompartments = compartments;
            compartments = new Dictionary<string, VentStatus>();

            List<IMyAirVent> vents = GetBlocks.ByName<IMyAirVent>(tagStart, thisGrid);
            foreach (IMyAirVent vent in vents) {
                string compartment = vent.CustomName;
                compartment = compartment.Remove(0, compartment.IndexOf(tagStart) + tagStart.Length);
                int termpos = compartment.IndexOf(tagSep);
                if (termpos < 0)
                    termpos = compartment.IndexOf(tagEnd);
                if (termpos < 0) {
                    console.Err("Tag is not closed in " + vent.CustomName);
                    continue;
                }
                compartment = compartment.Remove(termpos);
                //console.Warn("Name: '" + compartment + "'");

                VentStatus vStatus = vent.Status;
                if (vStatus != VentStatus.Pressurized && vent.CanPressurize && !vent.Depressurize)
                    vStatus = VentStatus.Pressurizing;
                console.Echo(compartment + " : " + vStatus.ToString());
                Utility.UpdateDict(compartments, compartment, vStatus);
            }

            List<IMyDoor> doors = GetBlocks.ByName<IMyDoor>(tagStart, thisGrid);
            foreach (IMyDoor door in doors) {
                if (!door.Enabled) {
                    console.Warn("Door '" + door.CustomName + "' is disabled!");
                    continue;
                }
                string remaining = door.CustomName;
                //console.Echo("Door: " + remaining);
                string prev = "";
                int pcomps = 0; // Pressurized
                int dcomps = 0; // Depressurized/ing
                int ocomps = 0; // Pressurizing = Close both d and p doors.
                while (remaining.Contains(tagStart) && prev != remaining) {
                    prev = remaining;
                    string _junk = GetUntil(ref remaining, tagStart);
                    string compartment = GetUntil(ref remaining, tagEnd);
                    string options = "";
                    if (compartment.Contains(tagSep)) {
                        options = compartment;
                        compartment = GetUntil(ref options, tagSep);
                    }

                    /*
                    console.Echo(" Part");
                    console.Echo("  Comp:" + compartment);
                    console.Echo("  Opts:" + options);
                    console.Echo("  Remain:" + remaining);
                    */

                    if (!compartments.ContainsKey(compartment)) {
                        console.Warn("There is no Vent for compartment '" + compartment + "', defined on door '" + door.CustomName + "'");
                        continue;
                    }

                    switch (compartments[compartment]) {
                        case VentStatus.Depressurized:
                        case VentStatus.Depressurizing:
                            dcomps++;
                            break;
                        case VentStatus.Pressurized:
                            pcomps++;
                            break;
                        default:
                            ocomps++;
                            break;
                    }
                }
                if (prev == remaining) {
                    console.Err("Failed to find changes: " + remaining);
                }

                DoorStatus doorStatus = door.Status;
                if (doorStatus == DoorStatus.Closing)
                    doorStatus = DoorStatus.Closed;
                if (doorStatus == DoorStatus.Opening)
                    doorStatus = DoorStatus.Open;

                if (doorstates.ContainsKey(door.GetId())) {
                    DoorStateTime dst = doorstates[door.GetId()];
                    if (dst.expires > now) {
                        console.Echo("Manual override active for door '" + door.CustomName + "'");
                        continue;
                    }
                    if (dst.status != doorStatus) {
                        // Door has been changed by something (or hopefully someone) else.
                        Utility.UpdateDict(doorstates, door.GetId(), new DoorStateTime() { status = doorStatus, expires = now.AddSeconds(doorTimeout) });
                        console.Echo("Door '" + door.CustomName + "' has been manually " + doorStatus.ToString());
                        continue;
                    }
                }

                DoorStatus prevDoorStatus = doorStatus;
                doorStatus = DoorStatus.Closed; // If I don't know what to do, close the door to be safe!
                if (dcomps == 0 && ocomps == 0) doorStatus = DoorStatus.Open; // Everything is pressurized.
                if (pcomps == 0 && dcomps == 0) doorStatus = DoorStatus.Open; // Everything can and is trying to pressurize.
                                                                              //if (pcomps == 0 && ocomps == 0) // This condition is dangerous as it will be hit during the initial depressurisation and will prevent repressurization if one compartment is fixed but the next is still broken.

                //FIXME: How to handle external (dcomps+pcomps+ocpomps==1) doors?
                // If dcomps or ocomps then close (as normal)
                // If pcomps and prevDoorStatus == closed only open if another compartment changed to (and is now staying at) decompressed at the same time?
                // For now:
                if (pcomps == 1 && dcomps == 0 && ocomps == 0 && doorStatus == DoorStatus.Open)
                    doorStatus = prevDoorStatus; // This should let it stay open if already open or manually opened.
                if (pcomps == 0 && dcomps == 0 && ocomps == 1 && doorStatus == DoorStatus.Open)
                    doorStatus = prevDoorStatus; // This should let it stay open if already open or manually opened.

                if (doorStatus != prevDoorStatus) {
                    if (doorStatus == DoorStatus.Open)
                        door.OpenDoor();
                    if (doorStatus == DoorStatus.Closed)
                        door.CloseDoor();
                }

                // Keep this at end as it should reflect the state I just set:
                Utility.UpdateDict(doorstates, door.GetId(), new DoorStateTime() { status = doorStatus, expires = now });
            }

        }
    }
}
