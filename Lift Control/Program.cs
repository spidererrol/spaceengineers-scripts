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
        // This file contains your actual script.
        //
        // You can either keep all your code here, or you can create separate
        // code files to make your program easier to navigate while coding.
        //
        // In order to add a new utility class, right-click on your project, 
        // select 'New' then 'Add Item...'. Now find the 'Space Engineers'
        // category under 'Visual C# Items' on the left hand side, and select
        // 'Utility Class' in the main area. Name it in the box below, and
        // press OK. This utility class will be merged in with your code when
        // deploying your final script.
        //
        // You can also simply create a new utility class manually, you don't
        // have to use the template if you don't want to. Just do so the first
        // time to see what a utility class looks like.
        // 
        // Go to:
        // https://github.com/malware-dev/MDK-SE/wiki/Quick-Introduction-to-Space-Engineers-Ingame-Scripts
        //
        // to learn more about ingame scripts.

        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set Runtime.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.

            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        public void Save()
        {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        ConsoleSurface console;
        private bool EnableDebug = true;
        Dictionary<string, double> prevPos = new Dictionary<string, double>();

        void DebugInit()
        {
            console = ConsoleSurface.EasyConsole(this);
            console.Add(GetBlocks.ByName<IMyTextPanel>("[LIFT]"));
        }

        void Debug(string msg)
        {
            if (EnableDebug)
                console.Echo("D:" + msg);
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.

            DebugInit();

            List<IMyPistonBase> pistons = GetBlocks.ByName<IMyPistonBase>("[LIFT:", false);
            HashSet<string> lifts = new HashSet<string>();
            foreach(IMyPistonBase piston in pistons)
            {
                System.Text.RegularExpressions.Regex LiftID = new System.Text.RegularExpressions.Regex(@"\[LIFT:([^\]]+)\]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                System.Text.RegularExpressions.MatchCollection matches = LiftID.Matches(piston.CustomName);
                if (matches.Count <= 0)
                    continue;
                if (matches[0].Groups.Count < 1)
                    continue;
                lifts.Add(matches[0].Groups[1].Value);
            }

            Debug("Lifts:");

            /* TODO:
             * Scan the doors to gather positions and allow main to be called with args of:
             * "F0" ... "Fn" for floor n
             * or
             * "P0.0" ... "Pn.m" for position n.m
             * Then adjust all piston max and/or mins and direction of each piston to achieve the requested floor.
             * Also allow "F-n" or "Bn" to map the same as "Fn" - allow for downward facing pistons.
             * "F0" is smallest position. "Fn", "F-n", and "Bn" are longest position.
             * Request queuing? Corner LCD position markers?
             */

            foreach(string lift in lifts)
            {
                Debug(" " + lift);
                pistons = GetBlocks.ByName<IMyPistonBase>("[LIFT:" + lift + "]", false);
                double position = 0;
                foreach(IMyPistonBase piston in pistons)
                {
                    position += piston.CurrentPosition;
                }
                string pos = position.ToString("F1"); // fixed to 1 decimal place.
                double prev = Double.PositiveInfinity;
                if (prevPos.ContainsKey(lift))
                    prev = prevPos[lift];
                else
                    prevPos.Add(lift, position);
                prevPos[lift] = position;
                Debug(" " + lift + ":" + position + "/" + prev);
                List<IMyDoor> floordoors = GetBlocks.ByName<IMyDoor>("[LIFT:" + lift + ":" , false);
                List<double> floors = new List<double>();
                foreach (IMyDoor door in floordoors)
                {
                    System.Text.RegularExpressions.Regex DoorPos = new System.Text.RegularExpressions.Regex(@"\[LIFT:[^\]]*:([^\]]+)\]", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    System.Text.RegularExpressions.MatchCollection matches = DoorPos.Matches(door.CustomName);
                    if (matches.Count <= 0 || matches[0].Groups.Count <= 1)
                        continue;
                    string sDoorPos = matches[0].Groups[1].Value;
                    double dDoorPos;
                    //Debug(" sDoorPos:" + sDoorPos);
                    if (!double.TryParse(sDoorPos, out dDoorPos))
                        continue;

                    floors.InsertInOrder(dDoorPos);
                }
                int curfloor = -1;
                bool above = false;
                for(int i = 0; i < floors.Count; i++)
                {
                    double dFloor = floors[i];
                    string sFloor = dFloor.ToString("F1");
                    //Debug(" sFloor:" + i + "=" + sFloor);
                    if (sFloor == pos)
                    {
                        curfloor = i;
                        above = false;
                        break;
                    }
                    if (position > dFloor)
                    {
                        curfloor = i;
                        above = true;
                    }
                }
                string debugPrefix = " Position: ";
                if (above)
                {
                    Debug(debugPrefix + curfloor + "<=>" + (curfloor + 1));
                } else
                {
                    Debug(debugPrefix + curfloor);
                }
                List<IMyTextPanel> textpanels = GetBlocks.ByName<IMyTextPanel>("[LIFT:" + lift + "]");
                foreach(IMyTextPanel panel in textpanels)
                {
                    string paneltext = panel.GetText();
                    List<string> lines = new List<string>(paneltext.Split('\n'));
                    int presetmode = 0;
                    for (int i = 1; i < lines.Count; i++) // First line doesn't count.
                    {
                        string line = lines[i];
                        if (line.Trim() == "")
                        {
                            presetmode = i;
                        }
                    }
                    if (presetmode > 0)
                    {
                        int offset = presetmode + (curfloor * 2) + 1;
                        if (above)
                            offset++;
                        Debug(" :" + offset + ":" + lines[offset] + ":");
                        lines[0] = lines[offset];
                        panel.WriteText(string.Join("\n",lines));
                    } else
                    {
                        Debug(" TODO: I don't yet support non-preset LCDs");
                    }
                }
                if (prev != position)
                    continue;

                Debug("  OPEN");
                List<IMyDoor> doors = GetBlocks.ByName<IMyDoor>("[LIFT:" + lift + ":" + pos + "]", false);
                if (doors.Count > 0) // Only add the car door if we have a matching landing door - don't open the door just because we are jammed.
                    doors.AddList(GetBlocks.ByName<IMyDoor>("[LIFT:" + lift + "]", false));
                foreach(IMyDoor door in doors)
                {
                    if (door.Status != DoorStatus.Closed)
                        continue;
                    door.OpenDoor();
                }
            }

        }
    }
}
