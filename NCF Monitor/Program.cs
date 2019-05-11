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
        private const string SectionName = "NCF Monitor";

        // NaniteMonitorModified.cs

        string NaniteTag = "NCF Main Base";
        string ConsoleTag = "[NCF Console]";

        void Debug(string msg)
        {
            Echo(msg);
        }

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public IMyFunctionalBlock GetNaniteControl() => GetBlocks.FirstByName<IMyFunctionalBlock>(NaniteTag, b => b.IsSameConstructAs(Me) && b.BlockDefinition.SubtypeName.Contains("NaniteControl"));
        public IMyTerminalBlock GetNaniteOre() => GetBlocks.FirstByName<IMyTerminalBlock>(NaniteTag, b => b.BlockDefinition.SubtypeName.Contains("NaniteOre"));

        public void Main(string realargument)
        {
            Config.ConfigSection config = Config.Section(Me, SectionName);
            config.Key("Tag").Get(ref NaniteTag).Comment("How non-debug blocks are tagged");
            config.Key("Console").Get(ref ConsoleTag).Comment("How debug lcd blocks are tagged");
            config.Save();

            ConsoleSurface con = ConsoleSurface.EasyConsole(this, ConsoleTag, SectionName);
            ConsoleSurface.EchoFunc Echo = con.GetEcho(); // Magically redirect Echo in this function to the con version!

            IMyFunctionalBlock nf = GetNaniteControl();
            IMyTerminalBlock nh = GetNaniteOre();

            #region mdk macros
            Echo("NCF Monitor");
            Echo("Version $MDK_DATETIME$");
            #endregion

            string[] NFStat = { "" };
            if (nf != null)
            {
                NFStat = nf.CustomInfo.Split('\n');
            }

            string[] NHStat = { "" };
            if (nh != null)
            {
                NHStat = nh.CustomInfo.Split('\n');
            }

            MultiSurface ld = GetBlocks.MultiSurfaceByName(NaniteTag, SectionName);
            ld.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            ld.WriteText("", false);
            IMyConveyorSorter sorter = GetBlocks.FirstByName<IMyConveyorSorter>(NaniteTag);

            ld.WriteText("-= Nanite Factory =- \n", false);

            bool isActive = true;
            if (!nf.Enabled)
            {
                isActive = false;
                ld.WriteLine("Turned off in terminal!");
            }

            for (int i = 0; i < NFStat.Length; i++)
            {

                if (NFStat[i].IndexOf("Status") >= 0)
                {
                    isActive = true;
                    string nfStatus = NFStat[i].Split(':')[1];
                    nfStatus = nfStatus.Trim();
                    ld.WriteText("Status: " + nfStatus + "\n", true);
                    Echo("NF Status: '" + nfStatus + "'");
                    switch (nfStatus)
                    {
                        case "Enabled":
                            isActive = false;
                            break;
                    }
                }
                if (NFStat[i].IndexOf("Active Nanites") >= 0)
                {
                    int activeNanites = int.Parse(NFStat[i].Split(':')[1]);
                    ld.WriteText("Active: " + activeNanites.ToString() + "\n", true);
                    if (activeNanites > 0)
                        isActive = true;
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

                if (NFStat[i].IndexOf("Needed parts:") >= 0)
                {
                    ld.WriteText("---------------\nRequired: \n", true);
                    for (int j = i + 1; j < NFStat.Length - 1; j++)
                    {
                        List<string> parts = new List<string>(NFStat[j].Split(':'));
                        int count;
                        if (parts.Count == 2 && int.TryParse(parts[1].Trim(), out count))
                        {
                            ld.WriteLine(parts[0].Trim() + ": " + Utility.DoubleToHUnit(count, format: "N0"));
                        }
                        else
                        {
                            ld.WriteText(NFStat[j] + "\n", true);
                        }
                    }
                    ld.WriteLine("----");
                }

                if (nf == null)
                {
                    ld.WriteText("You don't have Nanite Control Factory,\n or you named it incorrectly\n", false);
                    ld.WriteText("Check the mod's description, to know what's wrong...\n ", true);
                }
                else if (NFStat.Length == 0)
                {
                    ld.WriteLine("Unable to extract status information from Nanite Controller");
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
                int OreStatB = NHStat.Length;
                int OreStatE = NHStat.Length;
                for (int j3 = 0; j3 < NHStat.Length; j3++)
                {
                    if (NHStat[j3].IndexOf("Ores:") >= 0)
                        OreStatB = j3 + 1;
                    if (NHStat[j3].IndexOf("Valid Ore Types:") >= 0 || NHStat[j3] == "")
                        OreStatE = j3;
                }

                ld.WriteText("--<Mining>--\n", true);
                for (int j2 = OreStatB; j2 < OreStatE; j2++)
                {
                    if (NHStat[j2].StartsWith("- "))
                        NHStat[j2] = NHStat[j2].Remove(0, 2);
                    string oreamount = NHStat[j2];
                    int seppos = oreamount.IndexOf(": ");
                    string ore = oreamount.Substring(0, seppos);
                    string sAmount = oreamount.Substring(seppos + 2);
                    int iAmount = int.Parse(sAmount);
                    string hAmount = Utility.DoubleToHUnit(iAmount, format: "N0");
                    ld.WriteText(ore + ": " + hAmount + "\n", true);
                }


            }

            ld.WriteText("----\n", true);

            if (sorter != null)
            {
                if (isActive)
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
            else if (nf.CustomName.Contains("["))
            {
                if (isActive)
                {
                    if (nf.CustomName.Contains("[!"))
                    {
                        nf.CustomName = nf.CustomName.Replace("[!", "[");
                    }
                }
                else
                {
                    if (!nf.CustomName.Contains("[!"))
                    {
                        nf.CustomName = nf.CustomName.Replace("[", "[!");
                    }
                }
            }

            if (sorter != null)
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
            else if (nf.CustomName.Contains("["))
            {
                if (nf.CustomName.Contains("[!"))
                {
                    ld.WriteText("Drain active\n", true);
                }
                else
                {
                    ld.WriteText("Drain INACTIVE\n", true);
                }
            }
            else
            {
                ld.WriteText("No Drain Found!\n", true);
            }

            Echo("Done");
        }
    }
}