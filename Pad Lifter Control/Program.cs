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
        private const string Section = "Pad Lifter Control";
        string tag = "[PLC]";
        string debugTag = "[PLC Debug]";

        ConsoleSurface con;
        MultiSurface status;
        Int32 unixTimestamp;
        bool retrying = false;
        int stage = 0;
        string stagedcommand = "";

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        void MyEcho(string s)
        {
            con.Echo(s);
            if (status != null)
                status.WriteText(s + "\n", true);
        }
        Dictionary<string, Int32> stickies = new Dictionary<string, Int32>();
        void StickyMessage(string msg)
        {
            stickies.Remove(msg);
            stickies.Add(msg, unixTimestamp + 10);
        }
        void WriteStickies()
        {
            List<string> keys = new List<string>(stickies.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                MyEcho(keys[i]);
                Int32 timeout = stickies[keys[i]];
                if (timeout < unixTimestamp)
                    stickies.Remove(keys[i]);
            }
        }

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

        void Main(string argument)
        {
            if (argument == null)
                argument = "";

            Config.ConfigSection config = Config.Section(Me, Section);
            config.Get("Tag", tag);
            config.SetComment("Tag", "Tag to identify related blocks");
            config.Get("ConsoleTag", debugTag);
            config.SetComment("ConsoleTag", "Tag to output detailed information to");
            config.Save();

            con = ConsoleSurface.EasyConsole(this, debugTag, Section);
            ConsoleSurface.EchoFunc Echo = con.GetEcho();

            unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            status = GetBlocks.MultiSurfaceByName(tag, Section);
            if (status != null)
            {
                status.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
                status.WriteText("");
            }
            else
            {
                Echo("Failed to find status screen");
            }
            MyEcho(System.DateTime.Now.ToString("dd MMM yyyy h:mm tt\n"));

            IMyShipConnector conn = GetBlocks.FirstByName<IMyShipConnector>("Pad Connector");
            if (conn == null)
            {
                MyEcho("I can't find 'Pad Connector' connected to me!");
                return;
            }

            IMyShipConnector other = conn.OtherConnector;
            List<IMyLandingGear> topGears = new List<IMyLandingGear>();
            List<IMyLandingGear> bottomGears = new List<IMyLandingGear>();
            SideStates topState = SideStates.Unknown;
            SideStates bottomState = SideStates.Unknown;

            GetBlocksClass GetOther = null;

            if (other != null)
            {
                GetOther = GetBlocks.OtherGrid(other);

                List<IMyLandingGear> gears = GetOther.ByType<IMyLandingGear>();
                foreach (IMyLandingGear gear in gears)
                {
                    bool isTop = gear.CustomName.Contains("Top");
                    if (isTop)
                        Best(ref topState, gear);
                    else
                        Best(ref bottomState, gear);
                }
            }

            if (conn.Status == MyShipConnectorStatus.Connected)
            {
                MyEcho("Top: " + topState.ToString());
                MyEcho("Bottom: " + bottomState.ToString());
            }
            else if (argument == "")
            {
                if (status != null)
                {
                    //status.ClearImagesFromSelection();
                    string targImage;
                    if (conn.Enabled)
                        targImage = "Offline";
                    else
                        targImage = "Arrow";

                    //status.AddImageToSelection(targImage);
                    status.WriteText("", false);
                    if (status.CurrentlyShownImage == null || status.CurrentlyShownImage == "")
                        status.WriteLine(targImage);
                    status = null; // Disable text output for the rest of this round.
                }
            }
            MyEcho("");
            WriteStickies();
            MyEcho("");

            if (stage > 0)
            {
                MyEcho("Please wait..." + stage);
                argument = stagedcommand;
            }

            if (argument != "Grab" && argument != "Release")
                return;

            MyEcho("Command: " + argument);

            if (conn.Status == MyShipConnectorStatus.Connectable)
            {
                MyEcho("Connecting");
                conn.GetActionWithName("Lock").Apply(conn);
            }

            if (conn.Status != MyShipConnectorStatus.Connected || GetOther == null)
            {
                if (retrying)
                {
                    stickies.Remove("Please wait...");
                    StickyMessage("Failed to connect");
                    retrying = false;
                    return;
                }
                StickyMessage("Please wait...");
                retrying = true;
                IMyTimerBlock timer = GetBlocks.FirstByName<IMyTimerBlock>("Re" + argument + " Timer");
                timer.GetActionWithName("Start").Apply(timer);
                return;
            }
            retrying = false;

            stickies.Remove("Please wait...");

            IMyShipConnector topConnector = GetOther.FirstByName<IMyShipConnector>("Top Connector");
            IMyShipConnector bottomConnector = GetOther.FirstByName<IMyShipConnector>("Bottom Connector");

            if (argument == "Grab")
            {

                if (topState == SideStates.Locked && bottomState != SideStates.Locked && stage == 0)
                {
                    StickyMessage("\nALREADY GRABBED!\n");
                    return;
                }

                // Grab Top before release bottom...   

                stagedcommand = argument;

                if (stage == 0)
                {
                    // Should actually be locked already otherwise how am I connected to it?
                    Utility.RunActions(topConnector, "Lock");
                }
                else if (stage == 1)
                {

                    Utility.RunActions(topGears, "Lock");
                }
                else if (stage == 2)
                {
                    Utility.RunActions(bottomGears, "Unlock");
                }
                else if (stage == 3)
                {
                    Utility.RunActions(bottomConnector, "Unlock");
                }
                stage++;
                stage %= 4;
            }
            else if (argument == "Release")
            {

                // Lock bottom before release top. 

                stagedcommand = argument;

                if (stage == 0)
                {
                    Utility.RunActions(bottomConnector, "Lock");
                }
                else if (stage == 1)
                {
                    Utility.RunActions(bottomGears, "Lock");
                }
                else if (stage == 2)
                {
                    Utility.RunActions(topGears, "Unlock");
                }
                else if (stage == 3)
                {
                    // SE now makes connectors inactive for a short while after unlock so this is no longer needed:

                    //IMyTimerBlock releaseTimer = GetBlocks.FirstByName<IMyTimerBlock>("Disconnect Start Timer");
                    //if (releaseTimer != null)
                    //    Utility.RunActions(releaseTimer, "TriggerNow");
                    //else
                    //    Utility.RunActions(topConnector, "OnOff_Off");
                }
                stage++;
                stage %= 4;

            } // end if "Grab" or "Release"     

            return;
        }


    }
}