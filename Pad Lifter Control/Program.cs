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
        /***** FIXME: Lifter will effectively use Pad's batteries when connected! Possibly fit a battery in lifter and set     
                          to always on when "Pad Connector" is connected and always charging when "Dock Connector" is     
                          connected? Possibly fully off at other times as long as there are working reactors? Or just charge      
                          from reactors. Or just change over to batteries with no reactors?     
                          ** I don't want to turn off Pad's batteries because they may not be able to turn on again (eg on game     
                          load)     
  */

        IMyProgrammableBlock self = null;
        IMyTextPanel status = null;
        Int32 unixTimestamp;
        bool retrying = false;
        int stage = 0;
        string stagedcommand = "";

        public void FindMyBlocks<bt>(List<bt> outlist, IMyTerminalBlock refblock, string namefilter = null) where bt : IMyTerminalBlock
        {
            List<IMyTerminalBlock> items = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<bt>(items);
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].CubeGrid != refblock.CubeGrid)
                    continue;
                if (namefilter != null && items[i].CustomName != namefilter)
                    continue;
                outlist.Add((bt)items[i]);
            }
            return;
        }
        public void FindMyBlocks<bt>(List<bt> outlist, string namefilter = null)
        {
            FindMyBlocks(outlist, self as IMyTerminalBlock, namefilter);
        }
        public bt GetMyBlockWithName<bt>(string name, IMyTerminalBlock refblock = null)
        {
            List<bt> blocks = new List<bt>();
            if (refblock == null)
                refblock = self;
            FindMyBlocks(blocks, refblock, name);
            return blocks[0];
        }

        void MyEcho(string s)
        {
            Echo(s);
            if (status != null)
                status.WritePublicText(s + "\n", true);
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

        void Main(string argument)
        {
            if (argument == null)
                argument = "";

            unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;

            List<IMyTerminalBlock> progs = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(progs);
            for (int i = 0; i < progs.Count; i++)
            {
                IMyProgrammableBlock prog = progs[i] as IMyProgrammableBlock;
                if (prog.IsRunning)
                {       // I've only ever seen one prog running at a time. I hope this holds true.     
                    self = prog;
                    break;
                }
            }

            status = GetMyBlockWithName<IMyTextPanel>("Port Text Panel", self);
            if (status != null)
            {
                status.WritePublicText("");
            }
            else
            {
                MyEcho("Failed to find status screen");
            }
            MyEcho(System.DateTime.Now.ToString("dd MMM yyyy h:mm tt\n"));

            IMyShipConnector conn = GetMyBlockWithName<IMyShipConnector>("Pad Connector");
            if (conn == null)
            {
                MyEcho("I can't find 'Pad Connector' connected to me!");
                return;
            }

            IMyShipConnector other = conn.OtherConnector;
            List<IMyLandingGear> topGears = new List<IMyLandingGear>();
            List<IMyLandingGear> bottomGears = new List<IMyLandingGear>();
            bool anyTopLocked = false;
            bool anyBottomLocked = false;
            bool anyTopNear = false;
            bool anyBottomNear = false;
            bool anyTopAuto = false;
            bool anyBottomAuto = false;

            if (other != null)
            {
                List<IMyLandingGear> gears = new List<IMyLandingGear>();
                FindMyBlocks(gears, other);
                for (int i = 0; i < gears.Count; i++)
                {
                    IMyLandingGear gear = gears[i];
                    bool isNear = false;
                    bool isLocked = false;
                    bool isAuto = false;
                    bool isTop = gear.CustomName.Contains("Top");
                    if (isTop)
                        topGears.Add(gear);
                    else
                        bottomGears.Add(gear);
                    string[] details = gear.DetailedInfo.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                    for (int a = 0; a < details.Length; a++)
                    {
                        string[] dets = details[a].Split(':');
                        string command = dets[0];
                        string value = dets[1].Trim();
                        if (command.StartsWith("Lock State"))
                        {
                            if (value.StartsWith("Ready To Lock"))
                            {
                                isNear = true;
                                if (isTop)
                                    anyTopNear = true;
                                else
                                    anyBottomNear = true;
                            }
                            else if (value.StartsWith("Locked") || gear.IsLocked)
                            {
                                isLocked = true;
                                if (isTop)
                                    anyTopLocked = true;
                                else
                                    anyBottomLocked = true;
                            }
                            else if (value.StartsWith("Auto"))
                            {
                                // This doesn't happen, so is a place holder for now.      
                                isAuto = true;
                                if (isTop)
                                    anyTopAuto = true;
                                else
                                    anyBottomAuto = true;
                            }
                        }
                    }
                }
            }

            if (conn.IsConnected)
            {
                if (status != null)
                    status.ShowPublicTextOnScreen();
                if (anyTopLocked)
                    MyEcho("Top: Locked");
                else if (anyTopNear)
                    MyEcho("Top: Ready");
                else if (anyTopAuto)
                    MyEcho("Top: Auto");
                else
                    MyEcho("Top: Off");

                if (anyBottomLocked)
                    MyEcho("Bottom: Locked");
                else if (anyBottomNear)
                    MyEcho("Bottom: Ready");
                else if (anyBottomAuto)
                    MyEcho("Bottom: Auto");
                else
                    MyEcho("Bottom: Off");
            }
            else if (argument == "")
            {
                if (status != null)
                {
                    status.ClearImagesFromSelection();
                    if (conn.Enabled)
                        status.AddImageToSelection("Offline");
                    else
                        status.AddImageToSelection("Arrow");
                    status.ShowTextureOnScreen();
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

            if (!conn.IsConnected)
            {
                status.ShowPublicTextOnScreen();
                MyEcho("Connecting");
                conn.GetActionWithName("Lock").Apply(conn);
            }

            if (!conn.IsConnected)
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
                IMyTimerBlock timer = GetMyBlockWithName<IMyTimerBlock>("Re" + argument + " Timer");
                timer.GetActionWithName("Start").Apply(timer);
                return;
            }
            retrying = false;

            stickies.Remove("Please wait...");

            IMyShipConnector topConnector = GetMyBlockWithName<IMyShipConnector>("Top Connector", other);
            IMyShipConnector bottomConnector = GetMyBlockWithName<IMyShipConnector>("Bottom Connector", other);

            if (argument == "Grab")
            {

                if (anyTopLocked && !anyBottomLocked && stage == 0)
                {
                    StickyMessage("\nALREADY GRABBED!\n");
                    return;
                }

                // Grab Top before release bottom...   

                stagedcommand = argument;

                if (stage == 0)
                {
                    topConnector.GetActionWithName("Lock").Apply(topConnector);
                }
                else if (stage == 1)
                {
                    for (int i = 0; i < topGears.Count; i++)
                    {
                        topGears[i].GetActionWithName("Lock").Apply(topGears[i]);
                    }
                }
                else if (stage == 2)
                {
                    for (int i = 0; i < bottomGears.Count; i++)
                    {
                        bottomGears[i].GetActionWithName("Unlock").Apply(bottomGears[i]);
                    }
                }
                else if (stage == 3)
                {
                    bottomConnector.GetActionWithName("Unlock").Apply(bottomConnector);
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
                    bottomConnector.GetActionWithName("Lock").Apply(bottomConnector);
                }
                else if (stage == 1)
                {
                    for (int i = 0; i < bottomGears.Count; i++)
                    {
                        bottomGears[i].GetActionWithName("Lock").Apply(bottomGears[i]);
                    }
                }
                else if (stage == 2)
                {
                    for (int i = 0; i < topGears.Count; i++)
                    {
                        topGears[i].GetActionWithName("Unlock").Apply(topGears[i]);
                    }
                }
                else if (stage == 3)
                {
                    IMyTimerBlock releaseTimer = GetMyBlockWithName<IMyTimerBlock>("Disconnect Start Timer");
                    if (releaseTimer != null)
                        releaseTimer.GetActionWithName("TriggerNow").Apply(releaseTimer);
                    else
                        topConnector.GetActionWithName("OnOff_Off").Apply(topConnector);
                }
                stage++;
                stage %= 4;

            } // end if "Grab" or "Release"     

            return;
        }


    }
}