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

namespace IngameScript {
    partial class Program : MyGridProgram {

        #region mdk macros
        /*
         * Emergency Decompression Control Mk.1
         * Version: $MDK_DATETIME$
         */
        public const string VerString = "$MDK_DATETIME$";
        #endregion mdk macros

        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Save() {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        class CompartmentCounts {
            public int pcomps = 0;
            public int dcomps = 0;
            public int ocomps = 0;
        }

        class CompartmentStatus {
            public VentStatus status;
            public int ventcount = 0;
            public float pressure = 0;

            public CompartmentStatus(VentStatus st, float pres) {
                status = st;
                ventcount = 1;
                pressure = pres;
            }
            public CompartmentStatus(VentStatus st, IMyAirVent pres) {
                status = st;
                ventcount = 1;
                pressure = pres.GetOxygenLevel();
            }
            public CompartmentStatus(VentStatus st) {
                status = st;
            }

            public void AddPressure(float level) {
                if (ventcount++ == 0) {
                    pressure = level;
                } else {
                    pressure *= ventcount++;
                    pressure += level;
                    pressure /= ventcount;
                }
            }
            public void AddPressure(IMyAirVent vent) => AddPressure(vent.GetOxygenLevel());

            public void AddStatus(VentStatus newStatus) {
                foreach (VentStatus vs in new List<VentStatus>() { VentStatus.Depressurized, VentStatus.Depressurizing, VentStatus.Pressurizing, VentStatus.Pressurized }) {
                    if (status == vs) return;
                    if (newStatus == vs) {
                        status = newStatus;
                        return;
                    }
                }
            }

            public void Add(VentStatus addStatus, float addPressure) {
                AddStatus(addStatus);
                AddPressure(addPressure);
            }
            public void Add(VentStatus addStatus, IMyAirVent addPressure) {
                AddStatus(addStatus);
                AddPressure(addPressure);
            }

            public override string ToString() {
                string more = "";
                if (status == VentStatus.Pressurizing && ventcount > 0) {
                    float level = pressure;
                    level *= 100;
                    more = " " + level.ToString("F1") + "%";
                }
                return status.ToString() + more;
            }

            public static implicit operator VentStatus(CompartmentStatus c) => c.status;
            public static implicit operator CompartmentStatus(VentStatus v) => new CompartmentStatus(v);
        }

        string tagSequence;
        string tagSep;
        string tagEnd;
        bool thisGrid;
        int doorTimeout;

        Dictionary<string, CompartmentStatus> compartments = new Dictionary<string, CompartmentStatus>();
        struct DoorStateTime {
            public DoorStatus status;
            public DateTime expires;
        }
        Dictionary<long, DoorStateTime> doorstates = new Dictionary<long, DoorStateTime>();

        ConsoleSurface console;

        CompartmentCounts GetCompartments(IMyTerminalBlock block) {
            string remaining = block.CustomName;
            //console.Echo("Door: " + remaining);
            string prev = "";
            CompartmentCounts counts = new CompartmentCounts();
            while (remaining.Contains(tagSequence) && prev != remaining) {
                prev = remaining;
                string _junk = GetUntil(ref remaining, tagSequence);
                string compartment = GetUntil(ref remaining, tagEnd);
                string options = "";
                if (compartment.Contains(tagSep)) {
                    options = compartment;
                    compartment = GetUntil(ref options, tagSep);
                }

                List<string> compkeys;

                if (compartment == "*") {
                    compkeys = compartments.Keys.ToList();
                } else {
                    compkeys = new List<string> {
                        compartment
                    };
                }

                foreach (string compkey in compkeys) {

                    if (!compartments.ContainsKey(compkey)) {
                        console.Warn("There is no Vent for compartment '" + compkey + "', defined on block '" + block.CustomName + "'");
                        continue;
                    }

                    switch (compartments[compkey].status) {
                        case VentStatus.Depressurized:
                        case VentStatus.Depressurizing:
                            counts.dcomps++;
                            break;
                        case VentStatus.Pressurized:
                            counts.pcomps++;
                            break;
                        default:
                            counts.ocomps++;
                            break;
                    }
                }
            }
            if (prev == remaining) {
                console.Err("Failed to find changes: " + remaining);
            }
            return counts;
        }

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

        bool PropCheck(IMyTerminalBlock block, IConfigSection section, ITerminalProperty prop) {
            switch (prop.TypeName) {
                case "Boolean":
                    bool bVal = prop.AsBool().GetValue(block);
                    return bVal == section.Get(prop.Id, bVal);
                case "Color":
                    Color cVal = prop.AsColor().GetValue(block);
                    return cVal == section.Get(prop.Id, cVal);
                case "Single":
                    float fVal = prop.AsFloat().GetValue(block);
                    return fVal == section.Get(prop.Id, fVal);
                case "StringBuilder":
                    StringBuilder sbVal = prop.As<StringBuilder>().GetValue(block);
                    string sVal = sbVal.ToString();
                    return sVal == section.Get(prop.Id, sVal);
                default:
                    throw new Exception("I don't know how to handle property type " + prop.TypeName + " for property " + prop.Id);
            }
        }

        void SetProp(IMyTerminalBlock block, IConfigSection section, ITerminalProperty prop) {
            switch (prop.TypeName) {
                case "Boolean":
                    bool bVal = prop.AsBool().GetValue(block);
                    prop.AsBool().SetValue(block, section.Get(prop.Id, bVal));
                    break;
                case "Color":
                    Color cVal = prop.AsColor().GetValue(block);
                    prop.AsColor().SetValue(block, section.Get(prop.Id, cVal));
                    break;
                case "Single":
                    float fVal = prop.AsFloat().GetValue(block);
                    prop.AsFloat().SetValue(block, section.Get(prop.Id, fVal));
                    break;
                case "StringBuilder":
                    StringBuilder sbVal = prop.As<StringBuilder>().GetValue(block);
                    string sVal = sbVal.ToString();
                    sbVal = new StringBuilder(section.Get(prop.Id, sVal));
                    prop.As<StringBuilder>().SetValue(block, sbVal);
                    break;
                default:
                    throw new Exception("I don't know how to handle property type " + prop.TypeName + " for property " + prop.Id);
            }
        }

        public void Main(string argument, UpdateType updateSource) {
            DateTime now = DateTime.Now;
            Config config = new Config(Me);
            IConfigSection edcConfig = config.Section("EDC");
            string tagStart = edcConfig.Get("tagStart", "[");
            string tagName = edcConfig.Get("tagName", "EDC");
            tagSequence = tagStart + tagName;
            tagSep = edcConfig.Get("tagSep", ":");
            tagEnd = edcConfig.Get("tagEnd", "]");
            tagSequence += tagSep; // I always want the seperator after the start of the tag :)
            thisGrid = !edcConfig.Get("allGrids", false);
            doorTimeout = edcConfig.Get("doorTimeout", 15);
            edcConfig.SetComment("doorTimeout", "Seconds a door will be left alone after a person has operated it");
            edcConfig.Save();

            console = ConsoleSurface.EasyConsole(this, tagStart + tagName + tagEnd, "Console");
            console.ClearScreen();
            console.Echo("Emergency Decompression Control Mk.1");
            console.Echo("Build: " + VerString);

            //Dictionary<string, VentStatus> prevcompartments = compartments;
            compartments = new Dictionary<string, CompartmentStatus>();

            List<IMyAirVent> vents = GetBlocks.ByName<IMyAirVent>(tagSequence, thisGrid);
            foreach (IMyAirVent vent in vents) {
                string compartment = vent.CustomName;
                compartment = compartment.Remove(0, compartment.IndexOf(tagSequence) + tagSequence.Length);
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
                if (compartments.ContainsKey(compartment)) {
                    compartments[compartment].Add(vStatus, vent);
                } else {
                    compartments.Add(compartment, new CompartmentStatus(vStatus, vent));
                }
            }
            foreach (string compkey in compartments.Keys) {
                CompartmentStatus cs = compartments[compkey];
                console.Echo(compkey + " : " + cs.ToString());
            }

            List<IMySoundBlock> sounders = GetBlocks.ByName<IMySoundBlock>(tagSequence, thisGrid);
            foreach (IMySoundBlock sounder in sounders) {
                CompartmentCounts counts = GetCompartments(sounder);
                if (counts.ocomps > 0 || counts.dcomps > 0) {
                    //console.Echo(sounder.CustomName + " playing");
                    // if (!sounder.DetailedInfo.Contains("Loop timer")) // DetailedInfo is always blank despite showing things on the UI!
                    if (!sounder.CustomData.Contains("ACTIVE:[")) {
                        sounder.LoopPeriod = 30 * 60; // 30 minutes.
                        sounder.Play();
                        sounder.CustomData += "ACTIVE:[" + now.AddMinutes(30).ToString() + "]";
                    }
                    //TODO: Reset to inactive if the stored time has passed (so it will reset the playback).
                } else if (counts.pcomps > 0) {
                    //if (sounder.DetailedInfo.Contains("Loop timer") && sounder.LoopPeriod > 1) { // DetailedInfo is always blank despite showing things on the UI!
                    if (sounder.CustomData.Contains("ACTIVE:[")) {
                        //console.Warn(" stopping");
                        sounder.LoopPeriod = 1.0f;
                        sounder.Stop(); // This doesn't actually seem to do anything, hence the LoopPeriod above.
                        int start = sounder.CustomData.IndexOf("ACTIVE:[");
                        if (start < 0) // This should never happen.
                            start = 0;
                        int end = sounder.CustomData.IndexOf(']', start);
                        if (end < 0)
                            end = sounder.CustomData.Length - 1;
                        sounder.CustomData = sounder.CustomData.Remove(start, end - start);
                    }
                    //console.Echo(sounder.CustomName + " stopped");
                }
            }

            List<IMyTerminalBlock> blocks = GetBlocks.ByName<IMyTerminalBlock>(tagSequence, thisGrid);
            foreach (IMyTerminalBlock block in blocks) {
                if (Utility.IsType<IMyAirVent>(block)) continue;
                if (Utility.IsType<IMySoundBlock>(block)) continue;
                if (Utility.IsType<IMyDoor>(block)) continue;
                CompartmentCounts counts = GetCompartments(block);
                // I have thoughts for customisable Good/Bad states, but then I have to use them to detect states too.
                Config lconf = new Config(block);
                IConfigSection section;
                lconf.Section("EDC").Default("Save Config", false);
                lconf.Section("EDC").SetComment("Save Config", "Set this to 'true' to save out all properties with their current values");
                if (counts.ocomps > 0 || counts.dcomps > 0) {
                    section = lconf.Section("DEPRESSURIZED");
                    if (Utility.IsType<IMyLightingBlock>(block))
                        section.Set("OnOff", section.Get("OnOff", true));
                } else if (counts.pcomps > 0) {
                    section = lconf.Section("PRESSURIZED");
                    if (Utility.IsType<IMyLightingBlock>(block))
                        section.Set("OnOff", section.Get("OnOff", false));
                } else {
                    section = lconf.Section("UNKNOWN");
                }
                lconf.Save();

                List<ITerminalProperty> properties = new List<ITerminalProperty>();
                block.GetProperties(properties);
                bool matches = true;

                foreach (ITerminalProperty prop in properties) {
                    if (prop.Id == "Name") // Don't check Name here as it's just asking for trouble.
                        continue;
                    matches = matches && PropCheck(block, section, prop);
                }

                if (lconf.Section("EDC").Get("Save Config", false))
                    lconf.Save();

                if (!matches) {
                    foreach (ITerminalProperty prop in properties) {
                        if (prop.Id == "Name") // Don't modify Name here as it's just asking for trouble.
                            continue;
                        SetProp(block, section, prop);
                    }
                }
            }

            List<IMyDoor> doors = GetBlocks.ByName<IMyDoor>(tagSequence, thisGrid);
            foreach (IMyDoor door in doors) {
                if (!door.Enabled) {
                    console.Warn("Door '" + door.CustomName + "' is disabled!");
                    continue;
                }
                CompartmentCounts counts = GetCompartments(door);

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
                if (counts.dcomps == 0 && counts.ocomps == 0) doorStatus = DoorStatus.Open; // Everything is pressurized.
                if (counts.pcomps == 0 && counts.dcomps == 0) doorStatus = DoorStatus.Open; // Everything can and is trying to pressurize.
                                                                                            //if (pcomps == 0 && ocomps == 0) // This condition is dangerous as it will be hit during the initial depressurisation and will prevent repressurization if one compartment is fixed but the next is still broken.

                //FIXME: How to handle external (dcomps+pcomps+ocpomps==1) doors?
                // If dcomps or ocomps then close (as normal)
                // If pcomps and prevDoorStatus == closed only open if another compartment changed to (and is now staying at) decompressed at the same time?
                // For now:
                if (counts.pcomps == 1 && counts.dcomps == 0 && counts.ocomps == 0 && doorStatus == DoorStatus.Open)
                    doorStatus = prevDoorStatus; // This should let it stay open if already open or manually opened.
                if (counts.pcomps == 0 && counts.dcomps == 0 && counts.ocomps == 1 && doorStatus == DoorStatus.Open)
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
