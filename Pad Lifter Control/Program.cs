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
    partial class Program : MyGridProgram
    {
        enum PadState
        {
            Unknown,
            Grabbed,
            Released
        }
        enum DockState
        {
            Unknown,
            Docked,
            Undocked
        }

        static PadState GetPadState(IMyShipConnector connector)
        {
            if (connector.Status == MyShipConnectorStatus.Connected)
                return PadState.Grabbed;
            else
                return PadState.Released;
        }
        static DockState GetDockState(IMyShipConnector connector)
        {
            if (connector.Status == MyShipConnectorStatus.Connected)
                return DockState.Docked;
            else
                return DockState.Undocked;
        }

        // This can't be configurable as it is needed to retrieve configuration:
        private const string Section = "Pad Lifter Control";

        // I don't believe there is any point in making these configurable:
        private const string ActionLock = "Lock";
        private const string ActionUnlock = "Unlock";
        private const string ActionOff = "OnOff_Off";
        private const string ActionOn = "OnOff_On";

        // These could probably be made configurable but I don't know if they should be:
        private const string padTopGears = "Top";
        private const string padTopConnector = "Top Connector";
        private const string padBottomGears = "Bottom";
        private const string padBottomConnnector = "Bottom Connector";

        // These are configurable:
        string tag = "[PLC]";
        string debugTag = "[PLC Debug]";
        int UndockOffSecs = 30;

        ConsoleSurface con;
        MultiSurface status;

        PadState padState;
        DockState dockState;

        GetBlocksClass otherBlocks;

        int unixTimestamp;

        IEnumerator<bool> runQueue;

        IMyShipConnector padConnector;
        IMyShipConnector dockConnector;
        List<IMyLandingGear> myLandingGears;

        Queue<string> cmdqueue;

        public Program()
        {
            padState = PadState.Unknown;
            dockState = DockState.Unknown;
            cmdqueue = new Queue<string>();
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        void MyEcho(string s)
        {
            con.Echo(s);
            if (status != null)
                status.WriteText(s + "\n", true);
        }
        public class StickMsg : IComparable<StickMsg>
        {
            public string message;
            public int created;
            public int age;
            public int timeout => created + age;
            public StickMsg(string msg, int now, int maxage)
            {
                message = msg;
                created = now;
                age = maxage;
            }

            public int CompareTo(StickMsg other) => other.created - created;
        }
        Dictionary<string, StickMsg> stickies = new Dictionary<string, StickMsg>();
        void Emit(string s)
        {
            con.Echo(s);
            if (status != null)
                status.WriteLine(s);
        }
        void StickyMessage(string id, string msg, int age = 30)
        {
            stickies.Remove(id);
            stickies.Add(id, new StickMsg(msg, unixTimestamp, age));
        }
        void StickyMessage(string msg, int age) => StickyMessage(msg, msg, age);
        void StickyMessage(string msg) => StickyMessage(msg, msg);
        void DeleteSticky(string id) => stickies.Remove(id);
        void WriteStickies()
        {
            // I need a COPY of keys because I may be modifing the collection:
            List<string> keys = new List<string>(stickies.Keys);
            keys.Sort((x, y) => stickies[y].CompareTo(stickies[x]));
            foreach (string id in keys)
            {
                StickMsg stickMsg = stickies[id];
                if (stickMsg.timeout < unixTimestamp)
                    stickies.Remove(id);
                else
                    Emit(stickMsg.message);
            }
        }

        #region sidestates
        // Ripped from Small Landing Pad Controller:
        public enum SideStates
        {
            Unknown,
            Unlocked,
            Auto,
            Ready,
            Locked
        }
        void Best(ref SideStates first, SideStates second)
        {
            if (first < second)
                first = second;
        }
        public static SideStates Convert(IMyShipConnector connector)
        {
            switch (connector.Status)
            {
                case MyShipConnectorStatus.Connectable:
                    return SideStates.Ready;
                case MyShipConnectorStatus.Connected:
                    return SideStates.Locked;
                case MyShipConnectorStatus.Unconnected:
                    return SideStates.Unlocked;
            }
            throw new Exception("Out Of Cheese Error");
        }
        public static SideStates Convert(IMyLandingGear gear)
        {
            switch (gear.LockMode)
            {
                case LandingGearMode.Locked:
                    return SideStates.Locked;
                case LandingGearMode.ReadyToLock:
                    return SideStates.Ready;
                case LandingGearMode.Unlocked:
                    if (gear.AutoLock)
                        return SideStates.Auto;
                    return SideStates.Unlocked;
            }
            throw new Exception("Out Of Cheese Error");
        }

        void Best(ref SideStates first, IMyShipConnector second) => Best(ref first, Convert(second));
        void Best(ref SideStates first, IMyLandingGear second) => Best(ref first, Convert(second));

        Color Col(SideStates state)
        {
            switch (state)
            {
                case SideStates.Locked:
                    return Color.Lime;
                case SideStates.Ready:
                    return Color.Yellow;
                case SideStates.Unlocked:
                    return Color.Black;
                case SideStates.Auto:
                    return Color.Blue;
                case SideStates.Unknown:
                    return Color.Red;
            }
            throw new Exception("Out Of Cheese Error");
        }
        // End
        #endregion sidestates

        IMyShipConnector GetPadConnector(string name) => otherBlocks.FirstByName<IMyShipConnector>(name);
        List<IMyLandingGear> GetPadGears(string name) => otherBlocks.ByName<IMyLandingGear>(name);

        void Render()
        {
            WriteStickies();
        }
        IEnumerator<bool> ContinueGrab()
        {
            Utility.RunActions(padConnector, ActionLock);
            yield return true;

            Utility.RunActions(GetPadConnector(padTopConnector), ActionLock);
            yield return true;

            Utility.RunActions(GetPadGears(padTopGears), ActionLock);
            yield return true;

            Utility.RunActions(GetPadGears(padBottomGears), ActionUnlock);
            yield return true;

            Utility.RunActions(GetPadConnector(padBottomConnnector), ActionUnlock);
            yield return true;

            if (GetPadConnector(padTopConnector).Status != MyShipConnectorStatus.Connected)
                StickyMessage("Failed to lock Top Connector!");
            if (GetPadConnector(padBottomConnnector).Status == MyShipConnectorStatus.Connected)
                StickyMessage("Failed to unlock Bottom Connector!");
            // I don't care if top gears are locked or not - the connector will hold secure.
            if (GetPadGears(padBottomGears).Any(g => g.LockMode == LandingGearMode.Locked))
                StickyMessage("Failed to unlock Bottom Landing Gears!");
            padState = PadState.Grabbed;
            yield return false;
        }
        void DoGrab()
        {
            runQueue = ContinueGrab();
        }
        IEnumerator<bool> ContinueRelease()
        {
            Utility.RunActions(GetPadConnector(padBottomConnnector), ActionLock);
            yield return true;

            Utility.RunActions(GetPadGears(padBottomGears), ActionLock);
            yield return true;

            Utility.RunActions(GetPadGears(padTopGears), ActionUnlock);
            yield return true;

            //Utility.RunActions(GetPadConnector(padTopConnector), ActionUnlock);
            Utility.RunActions(padConnector, ActionUnlock);
            yield return true;

            if (padConnector.Status == MyShipConnectorStatus.Connected)
                StickyMessage("Failed to unlock Pad Connector!");
            //if (GetPadConnector(padTopConnector).Status != MyShipConnectorStatus.Connected)
            //    StickyMessage("Failed to lock Top Connector!");
            //if (GetPadConnector(padBottomConnnector).Status == MyShipConnectorStatus.Connected)
            //    StickyMessage("Failed to unlock Bottom Connector!");
            //// I don't care if top gears are locked or not - the connector will hold secure.
            //if (GetPadGears(padBottomGears).Any(g => g.LockMode == LandingGearMode.Locked))
            //    StickyMessage("Failed to unlock Bottom Landing Gears!");
            padState = PadState.Released;
            yield return false;
        }
        void DoRelease()
        {
            runQueue = ContinueRelease();
        }
        void DoDock()
        {
            Utility.RunActions(dockConnector, ActionLock);
            dockState = DockState.Docked;
        }
        IEnumerator<bool> ContinueUndock()
        {
            Utility.RunActions(dockConnector, ActionUnlock);
            yield return true;

            Utility.RunActions(myLandingGears, ActionUnlock);
            yield return true;

            Utility.RunActions(dockConnector, ActionOff);
            DateTime delayUntil = DateTime.Now + TimeSpan.FromSeconds(UndockOffSecs);
            yield return true;

            while (delayUntil > DateTime.Now)
                yield return true;

            Utility.RunActions(dockConnector, ActionOn);
            yield return true;

            dockState = DockState.Undocked;
            yield return false;
        }
        void DoUndock()
        {
            runQueue = ContinueUndock();
        }
        void InvalidCommand(string cmd)
        {
            StickyMessage("Ignoring invalid command: " + cmd);
        }
        void DoIdle()
        {
            if (padConnector == null)
            {
                StickyMessage("No Pad Connector found!");
                return;
            }
            if (dockConnector == null)
            {
                StickyMessage("No Dock Connector found!");
                return;
            }

            if (padState == PadState.Unknown && padConnector != null)
                padState = GetPadState(padConnector);

            if (dockState == DockState.Unknown && dockConnector != null)
                dockState = GetDockState(dockConnector);


            PadState newPadState = GetPadState(padConnector);
            DockState newDockState = GetDockState(dockConnector);

            //FIXME: Can I make a decent guess about what the user wanted to do and do it for them?
            // * If was docked, then regrab pad and undock.
            // * If was undocked and dock is near, regrab pad and dock.
            // * If was undocked and not near dock:
            //   * If was grabbed, release.
            //   * If was realeased, grab
            if (padState == PadState.Grabbed && padConnector.Status != MyShipConnectorStatus.Connected)
            {
                StickyMessage("autofix", "Re-grabbing pad");
                DoGrab();
            }
            else if (padState == PadState.Released && padConnector.Status == MyShipConnectorStatus.Connected)
            {
                StickyMessage("autofix", "Re-releasing pad");
                DoRelease();
            }

            if (dockState == DockState.Docked && dockConnector.Status != MyShipConnectorStatus.Connected)
            {
                StickyMessage("autofix", "Re-docking");
                DoDock();
            }
            else if (dockState == DockState.Undocked && dockConnector.Status == MyShipConnectorStatus.Connected)
            {
                StickyMessage("autofix", "Re-undocking");
                DoUndock();
            }
        }

        void Debug(string msg) => StickyMessage(msg);

        void Main(string argument)
        {
            if (argument != null && argument.Length > 0)
                cmdqueue.Enqueue(argument);

            unixTimestamp = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            string padConnectorName = "Pad Connector";
            string dockConnectorName = "Dock Connector";

            Config.ConfigSection config = Config.Section(Me, Section);
            config.Key("Pad Connector").Get(ref padConnectorName).Comment("Name of the connector which connects to pads");
            config.Key("Dock Connector").Get(ref dockConnectorName).Comment("Name of the connector which connects to a dock");
            config.Key("Tag").Get(ref tag).Comment("Tag for which screen(s) to display information on");
            config.Key("ConsoleTab").Get(ref debugTag).Comment("Tag for debug screen(s)");
            config.Key("UndockOffSecs").Get(ref UndockOffSecs).Comment("How long to turn off the docking connector for after undocking");
            config.Save();

            con = ConsoleSurface.EasyConsole(this, debugTag, Section);
            status = GetBlocks.MultiSurfaceByName(tag, Section);
            status.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            status.ClearText();

            padConnector = GetBlocks.FirstByName<IMyShipConnector>(padConnectorName);
            dockConnector = GetBlocks.FirstByName<IMyShipConnector>(dockConnectorName);
            myLandingGears = GetBlocks.ByType<IMyLandingGear>();

            if (padConnector.OtherConnector != null)
                otherBlocks = GetBlocks.OtherGrid(padConnector.OtherConnector);
            else
                otherBlocks = null;

            if (runQueue != null)
            {
                if (!runQueue.MoveNext() || !runQueue.Current)
                {
                    runQueue.Dispose();
                    runQueue = null;
                }
                return;
            }

            if (cmdqueue.Count > 0)
            {
                Debug("Processing commands!");
                string cmd = cmdqueue.Dequeue();
                Debug("Command: " + cmd);
                switch (cmd)
                {
                    case "Grab":
                        DoGrab();
                        break;
                    case "Release":
                        DoRelease();
                        break;
                    case "Dock":
                        DoDock();
                        break;
                    case "Undock":
                        DoUndock();
                        break;
                    default:
                        InvalidCommand(cmd);
                        break;
                }
                return;
            }

            DoIdle();
            Render();
        }
    }
}