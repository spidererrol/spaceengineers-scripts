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
        #region mdk macros
        const string ReleaseDate = "$MDK_DATETIME$";
        #endregion mdk macros

        public class MainConfig : BaseConfigSection
        {
            public override void ApplyDefaults()
            {
                Default("OpenSeconds", 11);
                Default("Prefix", "[#");
                Default("Suffix", "]");
                Default("ConsoleTo", "##EDC##");
                Default("ConfigSection", "EDC");
                Default("Echo", true);
                Default("Self", true);
            }
            public MainConfig(IMyProgrammableBlock Me, string SectionName = "EDC") : base(Me, SectionName) { }

            public int OpenSeconds => GetInt("OpenSeconds");
            public string Prefix => GetString("Prefix");
            public string Suffix => GetString("Suffix");
            public string ConsoleTo => GetString("ConsoleTo");
            public string ConfigSection => GetString("ConfigSection");
            public bool Echo => GetBool("Echo");
            public bool Self => GetBool("Self");
            public string SealConfigSection => Get("SealConfigSection", ConfigSection + "#Seal");
            public string LeakConfigSection => Get("LeakConfigSection", ConfigSection + "#Leak");
        }
        public MainConfig config;

        public class BlockConfig : KeyPrefixConfigSection
        {
            public static string StatePrefix(RoomState state)
            {
                switch (state)
                {
                    case RoomState.LEAKING:
                        return "Leak";
                    case RoomState.SEALED:
                        return "Seal";
                    default:
                        return "Missing";
                }
            }

            public BlockConfig(RoomState state, IConfigSection section) : base(StatePrefix(state), section) { }

            public string Action => GetString("Action");
            public bool ActionBeforeProperties => GetBool("ActionBeforeProperties");
            public IEnumerable<KeyValuePair<string, MyIniValue>> Properties => GetKeys().Select(f => new KeyValuePair<string, MyIniValue>(f, Get(f)));

            public override void ApplyDefaults() { }

        }

        public enum RoomState
        {
            MISSING,
            LEAKING,
            SEALED,
        };
        public class RoomsStates
        {
            public class StateChanged
            {
                public readonly RoomState state;
                public readonly bool changed;
                public StateChanged(RoomState roomState, bool ischanged)
                {
                    state = roomState;
                    changed = ischanged;
                }
            }

            private IDictionary<string, RoomState> oldStates;
            private IDictionary<string, RoomState> newStates;
            private ISet<string> changed;

            public int Count
            {
                get
                {
                    if (newStates == null && oldStates == null)
                        return 0;
                    if (newStates != null && oldStates == null)
                        return newStates.Count;
                    if (newStates == null && oldStates != null)
                        return oldStates.Count;
                    ISet<string> keys = new HashSet<string>();
                    foreach (string roomname in oldStates.Keys)
                    {
                        keys.Add(roomname);
                    }
                    foreach (string roomname in newStates.Keys)
                    {
                        keys.Add(roomname);
                    }
                    return keys.Count;
                }
            }

            public IList<KeyValuePair<string, RoomState>> ChangedRooms => CalcChanged();

            public IList<KeyValuePair<string, RoomState>> CalcChanged()
            {
                if (changed != null)
                    return newStates.ToList();
                changed = new HashSet<string>();
                List<string> newNames = newStates.Keys.ToList();
                foreach (string rname in newNames)
                {
                    if (oldStates.ContainsKey(rname))
                        if (oldStates[rname] == newStates[rname])
                            newStates.Remove(rname);
                        else
                            changed.Add(rname);
                    else
                        changed.Add(rname);
                }
                return newStates.ToList();
            }

            public StateChanged FinalState(IEnumerable<string> roomnames, bool missingSealed = true)
            {
                bool changed = roomnames.Any(rn => newStates.ContainsKey(rn));
                foreach (string roomname in roomnames)
                {
                    if (newStates.ContainsKey(roomname))
                    {
                        if (newStates[roomname] == RoomState.LEAKING)
                            return new StateChanged(RoomState.LEAKING, changed);
                    }
                    else if (oldStates.ContainsKey(roomname))
                    {
                        if (oldStates[roomname] == RoomState.LEAKING)
                            return new StateChanged(RoomState.LEAKING, changed);
                    }
                    else if (!missingSealed)
                        return new StateChanged(RoomState.MISSING, changed);
                }
                return new StateChanged(RoomState.SEALED, changed);
            }

            public RoomsStates()
            {
                newStates = new Dictionary<string, RoomState>();
            }

            private void Clean()
            {
                if (oldStates == null)
                {
                    oldStates = newStates;
                }
                else
                {
                    foreach (KeyValuePair<string, RoomState> rs in newStates)
                    {
                        oldStates[rs.Key] = rs.Value;
                    }
                }
                newStates = new Dictionary<string, RoomState>();
                changed = null;
            }
            public void Start() => Clean();
            public void Finish() => Clean();
            public void Sealed(string roomname)
            {
                if (!newStates.ContainsKey(roomname))
                    newStates.Add(roomname, RoomState.SEALED);
            }
            public void Leaking(string roomname)
            {
                if (newStates.ContainsKey(roomname))
                    newStates[roomname] = RoomState.LEAKING;
                else
                    newStates.Add(roomname, RoomState.LEAKING);
            }
        }
        public RoomsStates roomsStates = new RoomsStates();

        public class Todo
        {
            public string name;
            public string value;
            public TimeSpan deferUntil;

            public Todo(string name, string value, TimeSpan deferUntil)
            {
                this.name = name;
                this.value = value;
                this.deferUntil = deferUntil;
            }
            public Todo(string name, string value, int seconds) : this(name, value, TimeSpan.FromSeconds(seconds)) { }

            public void Apply(IMyTerminalBlock b)
            {
                if (value == null)
                {
                    b.ApplyAction(name);
                }
                else
                {
                    string type = b.GetProperty(name).TypeName;
                    switch (type)
                    {
                        case "bool":
                            b.SetValueBool(name, bool.Parse(value));
                            break;
                        case "float":
                            b.SetValueFloat(name, float.Parse(value));
                            break;
                        case "color":
                            b.SetValueColor(name, Utility.WebColor(value));
                            break;
                        default:
                            b.SetValue<string>(name, value);
                            break;
                    }
                }
            }
        }

        //Dictionary<Vector3I, DoorStatus> doorState = new Dictionary<Vector3I, DoorStatus>();
        //Dictionary<Vector3I, TimeSpan> doorToClose = new Dictionary<Vector3I, TimeSpan>();
        Dictionary<Vector3I, ISet<Todo>> pendingTasks = new Dictionary<Vector3I, ISet<Todo>>();

        public TimeSpan ticker = TimeSpan.FromSeconds(0);

        public Todo CreateTodo(string name, string value = null) => new Todo(name, value, ticker + TimeSpan.FromSeconds(config.OpenSeconds));

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

        /*
        public void UpdateLights(List<IMyInteriorLight> lights, bool isSealed, bool inverted, string roomname)
        {
            if (!lights.Any())
                return;
            if (inverted)
                isSealed = !isSealed;
            foreach (IMyInteriorLight light in lights)
            {
                LayeredConfigSection sec = new LayeredConfigSection().Add(new string[,]
                {
                    { "SealedColor", "Off"},
                    { "LeakColor","On"},
                }).Add(Config.Section(light, config.ConfigSection)).Add(Config.Section(light, config.ConfigSection + "#" + roomname));
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
                    light.Color = WebColor(sTargetColor);
                }
                foreach (string attrib in new string[] { "BlinkLength", "BlinkIntervalSeconds", "BlinkOffset" })
                {
                    string key = prefix + attrib;
                    if (sec.ContainsKey(key))
                        light.SetValue(attrib, sec.Get(key, 0.0f));
                }
            }
        }
        */

        public string RoomTag(string roomname) => config.Prefix + roomname + config.Suffix;

        public List<IMyTerminalBlock> GetRoomBlocks(string roomname) => GetBlocks.ByName<IMyTerminalBlock>(RoomTag(roomname));

        public IEnumerable<string> TaggedRooms(string CustomName)
        {
            string sTags = System.Text.RegularExpressions.Regex.Escape(config.Prefix) + @"(.+?)(!?)" + System.Text.RegularExpressions.Regex.Escape(config.Suffix);
            System.Text.RegularExpressions.Regex reTags = new System.Text.RegularExpressions.Regex(sTags);

            System.Text.RegularExpressions.MatchCollection matches = reTags.Matches(CustomName);
            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                yield return match.Groups[1].Captures[0].Value;
            }
        }
        public IEnumerable<string> TaggedRooms(IMyTerminalBlock b) => TaggedRooms(b.CustomName);

        public static void SetProperty(IMyTerminalBlock b, string propname, MyIniValue value)
        {
            ITerminalProperty prop = b.GetProperty(propname);
            switch (prop.TypeName.ToLower())
            {
                case "string":
                    b.SetValue<string>(propname, value.ToString());
                    break;
                case "bool":
                    b.SetValueBool(propname, value.ToBoolean());
                    break;
                case "float":
                    b.SetValueFloat(propname, value.ToSingle());
                    break;
                case "color":
                    b.SetValueColor(propname, Utility.WebColor(value.ToString()));
                    break;
                default:
                    throw new Exception("I don't know how to set a property of type '" + prop.TypeName + "'");
            }
        }
        public static void SetProperty(IMyTerminalBlock b, KeyValuePair<string, MyIniValue> prop) => SetProperty(b, prop.Key, prop.Value);

        public readonly DictConfigSection baseDoorConfig = new DictConfigSection(new string[,]
            {
                { "LeakAction", "Close" },
                { "SealAction","Open" },
            });
        public readonly DictConfigSection baseVentConfig = new DictConfigSection();
        public readonly DictConfigSection baseDEFAULTConfig = new DictConfigSection(new string[,]
            {
                { "LeakAction", "On" },
                { "SealAction","Off" },
            });
        public readonly DictConfigSection baseDoorLeakConfig = new DictConfigSection(new string[,]
        {
            { "Monitor","Status=Closed" },
            { "OnStatus","Close" },
        });
        public readonly DictConfigSection baseDEFAULTLeakConfig = new DictConfigSection();
        public readonly DictConfigSection baseDEFAULTSealConfig = new DictConfigSection();

        public void ProcessBlock(IMyTerminalBlock b)
        {
            RoomsStates.StateChanged finalState = roomsStates.FinalState(TaggedRooms(b));
            if (finalState.changed)
            {
                IConfigSection rbc = Config.Section(b, config.ConfigSection);
                IConfigSection baseconf;
                if (Utility.IsType<IMyDoor>(b))
                    baseconf = baseDoorConfig;
                else if (Utility.IsType<IMyAirVent>(b))
                    baseconf = baseVentConfig;
                else
                    baseconf = baseDEFAULTConfig;
                BlockConfig bconfig = new BlockConfig(finalState.state, new LayeredConfigSection().Add(baseconf).Add(rbc));
                if (bconfig.ActionBeforeProperties && bconfig.ContainsKey("Action") && b.HasAction(bconfig.Action))
                    b.ApplyAction(bconfig.Action);
                foreach (KeyValuePair<string, MyIniValue> prop in bconfig.Properties)
                {
                    SetProperty(b, prop);
                }
                if (!bconfig.ActionBeforeProperties && bconfig.ContainsKey("Action") && b.HasAction(bconfig.Action))
                    b.ApplyAction(bconfig.Action);
            }
            else if (finalState.state != RoomState.MISSING)
            {
                string configSectionName = finalState.state == RoomState.SEALED ? config.SealConfigSection : config.LeakConfigSection;
                IConfigSection rbc = Config.Section(b, configSectionName);
                IConfigSection baseconf;
                if (Utility.IsType<IMyDoor>(b) && finalState.state == RoomState.LEAKING)
                    baseconf = baseDoorLeakConfig;
                else if (finalState.state == RoomState.LEAKING)
                    baseconf = baseDEFAULTLeakConfig;
                else
                    baseconf = baseDEFAULTSealConfig;
                IConfigSection bconfig = new LayeredConfigSection().Add(baseconf).Add(rbc);
                if (bconfig.ContainsKey("Monitor") && bconfig.GetString("Monitor").Length > 0)
                {
                    IEnumerable monitorList = bconfig.GetString("Monitor").Split(',');
                    ISet<Todo> todos = new HashSet<Todo>();
                    foreach (string monitorRule in monitorList)
                    {
                        string property = monitorRule.Substring(0, monitorRule.IndexOf('='));
                        ISet<string> values = new HashSet<string>(monitorRule.Substring(monitorRule.IndexOf('=')).Split('|'));
                        string curValue = b.GetProperty(property).ToString();
                        if (!values.Contains(curValue))
                        {
                            string todo = bconfig.GetString("Do" + property);
                            IList<string> commands = todo.Split(',');
                            foreach (string command in commands)
                            {
                                int eq = command.IndexOf('=');
                                Todo newtodo;
                                if (eq == -1)
                                {
                                    newtodo = CreateTodo(command);
                                }
                                else
                                {
                                    string prop = command.Substring(0, eq);
                                    string val = command.Substring(eq);
                                    newtodo = CreateTodo(prop, val);
                                }
                                todos.Add(newtodo);
                            }
                        }
                    }
                    if (todos.Any())
                    {
                        NOPE! this would contiously reset the timestamps!
                        Vector3I bid = b.Position;
                        if (pendingTasks.ContainsKey(bid))
                            pendingTasks.Remove(bid);
                        pendingTasks.Add(bid, todos);
                    }
                }

            }
        }

        public void Main(string argument)
        {
            config = new MainConfig(Me);

            console = ConsoleSurface.EasyConsole(this, config.ConsoleTo, config.ConfigSection, config.Echo, config.Self);

            string sTags = System.Text.RegularExpressions.Regex.Escape(config.Prefix) + @"(.+?)(!?)" + System.Text.RegularExpressions.Regex.Escape(config.Suffix);
            System.Text.RegularExpressions.Regex reTags = new System.Text.RegularExpressions.Regex(sTags);

            Emit("Emergency Decompression Control\n-------------------------------");
            if (doorToClose.Count > 0)
            {
                List<Vector3I> keys = new List<Vector3I>(doorToClose.Keys);
                for (int i = 0; i < keys.Count; i++)
                {
                    if (doorToClose[keys[i]] > ticker + TimeSpan.FromSeconds(config.OpenSeconds * 2))
                    {
                        // Time has gone backwards (probably game loaded) - close door now.
                        doorToClose[keys[i]] = new TimeSpan(0);
                    }
                    else if (doorToClose[keys[i]] < ticker)
                    {
                        // This should have been closed and deleted in the last tick. Clean up as door may have been deleted.
                        doorToClose.Remove(keys[i]);
                    }
                }
            }
            ticker += Runtime.TimeSinceLastRun;
            roomsStates.Start();
            List<IMyAirVent> vents = GetBlocks.ByName<IMyAirVent>(config.Prefix);
            for (int i = 0; i < vents.Count; i++)
            {
                IMyAirVent vent = vents[i];
                RoomState vstate = vent.CanPressurize ? RoomState.SEALED : RoomState.LEAKING;
                if (vent.Depressurize)
                    vstate = RoomState.LEAKING;
                System.Text.RegularExpressions.MatchCollection matches = reTags.Matches(vent.CustomName);
                for (int m = 0; m < matches.Count; m++)
                {
                    System.Text.RegularExpressions.Match match = matches[m];
                    string roomname = match.Groups[1].Captures[0].Value;
                    if (vstate == RoomState.LEAKING)
                        roomsStates.Leaking(roomname);
                    else
                        roomsStates.Sealed(roomname);
                }
            }

            roomsStates.CalcChanged();
            foreach (IMyTerminalBlock block in GetBlocks.ByName<IMyTerminalBlock>(config.Prefix))
            {
                ProcessBlock(block);
            }

            //foreach (KeyValuePair<string, RoomState> room in roomsStates.ChangedRooms)
            //{
            //    List<IMyTerminalBlock> things = GetRoomBlocks(room.Key);
            //    things.ForEach(b => ProcessBlock(b));
            //}
            Emit(roomsStates.Count + " rooms found");
            //jfdkjflwjlf
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