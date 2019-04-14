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
        // NaniteMonitorModified.cs

        string NaniteTag = "Main Base";

        void Debug(string msg)
        {
            Echo(msg);
        }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Main(string realargument)
        {
            Config.ConfigSection config = Config.Section(Me, "NCF Monitor");
            config.Get("Tag", ref NaniteTag);
            config.Save();
            
            var nf = GridTerminalSystem.GetBlockWithName("Nanite Control Factory - " + NaniteTag) as IMyTerminalBlock;
            Echo("Find NF");
            string[] NFStat = { "" };
            if (nf != null)
            {
                NFStat = nf.CustomInfo.Split('\n');
                Echo("Got NF");
            }

            var nh = GridTerminalSystem.GetBlockWithName("NUHOL - " + NaniteTag) as IMyTerminalBlock;
            string[] NHStat = { "" };
            if (nh != null)
            {
                NHStat = nh.CustomInfo.Split('\n');
            }

            var ld = GridTerminalSystem.GetBlockWithName("LCD-NanitePanel_" + NaniteTag) as IMyTextPanel;
            Echo("Got LCD");
            ld.WriteText("", false);
            IMyConveyorSorter sorter = getBlockByName<IMyConveyorSorter>("[" + NaniteTag + "]");

            ld.WriteText("-= Nanite Factory =- \n", false);

            Echo("Parsing NF");
            for (int i = 0; i < NFStat.Length; i++)
            {

                if (NFStat[i].IndexOf("Status") >= 0)
                {
                    ld.WriteText("Status: " + NFStat[i].Split(':')[1] + "\n", true);
                    if (sorter != null)
                    {
                        if (NFStat[i].Split(':')[1].Trim() == "Enabled" || NFStat[i].Split(':')[1].Trim() == "Disabled")
                        {
                            if (!sorter.GetValueBool("DrainAll"))
                            {
                                ITerminalAction act = sorter.GetActionWithName("DrainAll");
                                act.Apply(sorter);
                            }
                        }
                        else
                        {
                            if (sorter.GetValueBool("DrainAll"))
                            {
                                ITerminalAction act = sorter.GetActionWithName("DrainAll");
                                act.Apply(sorter);
                            }
                        }
                    }
                }
                if (NFStat[i].IndexOf("Active Nanites") >= 0)
                {
                    ld.WriteText("Active: " + NFStat[i].Split(':')[1] + "\n", true);
                }
                if (NFStat[i].IndexOf("Current Power") >= 0)
                {
                    ld.WriteText("--<Status>--\nPower: " + NFStat[i].Split(':')[1] + "\n", true);
                }

                if (NFStat[i].IndexOf("Possible Construction") >= 0)
                {
                    ld.WriteText("--<Tasks>--\nTo Construct: " + NFStat[i].Split(':')[1] + " blocks\n", true);
                }
                if (NFStat[i].IndexOf("Possible Deconstruction") >= 0)
                {
                    ld.WriteText("To DeConst: " + NFStat[i].Split(':')[1] + " blocks\n", true);
                }
                if (NFStat[i].IndexOf("Possible Floating") >= 0)
                {
                    ld.WriteText("To Gather: " + NFStat[i].Split(':')[1] + " pieces\n", true);
                }
                if (NFStat[i].IndexOf("Possible Projection") >= 0)
                {
                    ld.WriteText("To Build: " + NFStat[i].Split(':')[1] + " blocks\n", true);
                }
                if (NFStat[i].IndexOf("Possible Mining") >= 0)
                {
                    ld.WriteText("To Mine: " + NFStat[i].Split(':')[1] + " deposites\n", true);
                }

                if (NFStat[i].IndexOf("Missing components") >= 0)
                {
                    ld.WriteText("---------------\nRequiered: \n", true);
                    for (int j = i + 1; j < NFStat.Length - 1; j++)
                    {
                        ld.WriteText("[" + NFStat[j] + "]\n", true);
                    }
                }

                if (NFStat[0] == "")
                {
                    ld.WriteText("You don't have Nanite Control Factory,\n or you named it incorrectly\n", false);
                    ld.WriteText("Check the mod's description, to know what's wrong...\n ", true);
                }

            }

            if (NHStat[0] == "")
            {
                //ld.WritePublicText("--<Mining>--\n", true);     
                //ld.WritePublicText("You don't have Nanite Ultrasonic Hummer Ore Locator\n, or you named it incorrectly\n", true);     
                //ld.WritePublicText("Check the mod's description, to know what's wrong...\n ", true);    
            }
            else
            {
                int OreStatB = 0;
                int OreStatE = 0;
                for (int j3 = 0; j3 < NHStat.Length; j3++)
                {
                    if (NHStat[j3].IndexOf("Ore Detected:") >= 0) { OreStatB = j3 + 2; }
                    if (NHStat[j3].IndexOf("Valid Ore Types:") >= 0) { OreStatE = j3 - 2; }
                }

                ld.WriteText("--<Mining>--\n", true);
                for (int j2 = OreStatB; j2 < OreStatE; j2++)
                {
                    ld.WriteText("[" + NHStat[j2] + "]\n", true);
                }


            }

            ld.WriteText("\n", true);
            if (sorter == null)
            {
                ld.WriteText("No Drain Found!\n", true);
            }
            else
            {
                if (sorter.GetValueBool("DrainAll"))
                {
                    ld.WriteText("Drain active\n", true);
                }
                else
                {
                    ld.WriteText("Drain INACTIVE\n", true);
                }
            }


            Echo("Done");
        }

    }
}