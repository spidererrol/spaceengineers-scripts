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

        public Program() {
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
        }

        public void Save() {
            // Called when the program needs to save its state. Use
            // this method to save your state to the Storage field
            // or some other means. 
            // 
            // This method is optional and can be removed if not
            // needed.
        }

        private static readonly System.Text.RegularExpressions.Regex rePosMarks = new System.Text.RegularExpressions.Regex(@"\s*<.*>");
        private static readonly System.Text.RegularExpressions.Regex reTagMarks = new System.Text.RegularExpressions.Regex(@"\s*\[.*\]");
        private static readonly System.Text.RegularExpressions.Regex reAddPos = new System.Text.RegularExpressions.Regex(@"!POS");

        public class BlockName {
            private IMyTerminalBlock terminalBlock;
            private string currentName;
            private bool modified => terminalBlock.CustomName != currentName;
            public string Name {
                get {
                    return currentName;
                }
                set {
                    currentName = value;
                }
            }

            public BlockName(IMyTerminalBlock block) {
                terminalBlock = block;
                currentName = block.CustomName;
            }

            public void ApplyPos(bool clear = false) {
                bool wantPos = Contains(reAddPos);
                bool hasPos = Contains(rePosMarks);
                if (wantPos && hasPos)
                    return;
                if (hasPos && (wantPos || clear)) {
                    Remove(reAddPos);
                    Remove(rePosMarks);
                }
                if (wantPos && !hasPos) {
                    Name += " <" + terminalBlock.Position.ToString() + ">";
                    terminalBlock.ShowOnHUD = true;
                }
            }
            public void ApplyPos(string arg) => ApplyPos(arg == "ClearPOS");

            public void ClearPos() {
                Remove(rePosMarks);
            }

            public void ApplyArg(string arg) {
                if (arg == "ClearPOS")
                    ClearPos();
                if (arg == "ClearHUD")
                    terminalBlock.ShowOnHUD = false;
            }

            public void ClearTags() => Remove(reTagMarks);

            public bool Contains(string contains) => Name.Contains(contains);
            public bool Contains(System.Text.RegularExpressions.Regex contains) => contains.Match(Name).Success;
            public void Replace(string match, string replace) => Name = Name.Replace(match, replace);
            public void Replace(System.Text.RegularExpressions.Regex match, string replace) => Name = match.Replace(Name, replace);
            public void Remove(string match) => Replace(match, "");
            public void Remove(System.Text.RegularExpressions.Regex match) => Replace(match, "");

            public void autoShowOnHUD() {
                if (Name.Contains("[EDC:"))
                    terminalBlock.ShowOnHUD = false;
                else
                    terminalBlock.ShowOnHUD = true;
            }

            public void Save() {
                if (!modified)
                    return;
                terminalBlock.CustomName = currentName;
            }

        }

        public void Main(string argument, UpdateType updateSource) {
            // The main entry point of the script, invoked every time
            // one of the programmable block's Run actions are invoked,
            // or the script updates itself. The updateSource argument
            // describes where the update came from. Be aware that the
            // updateSource is a  bitfield  and might contain more than 
            // one update type.
            // 
            // The method itself is required, but the arguments above
            // can be removed if not needed.

            ConsoleSurface console = ConsoleSurface.EasyConsole(this);
            console.Echo(argument);
            List<IMyAirVent> vents = GetBlocks.ByType<IMyAirVent>();
            foreach (IMyAirVent vent in vents) {
                BlockName name = new BlockName(vent);
                name.ApplyPos(argument);
                name.autoShowOnHUD();

                name.ApplyArg(argument);
                name.Save();
            }
            List<IMyDoor> doors = GetBlocks.ByType<IMyDoor>();
            foreach (IMyDoor door in doors) {
                BlockName name = new BlockName(door);
                name.ApplyPos(argument);
                name.autoShowOnHUD();


                name.ApplyArg(argument);
                name.Save();
            }
            List<IMyTextPanel> lcds = GetBlocks.ByType<IMyTextPanel>();
            foreach (IMyTextPanel lcd in lcds) {
                BlockName name = new BlockName(lcd);
                name.ApplyPos(argument);
                name.autoShowOnHUD();


                name.ApplyArg(argument);
                name.Save();
            }
            console.ClearScreen();
            console.Echo("Finished: " + DateTime.Now);
        }
    }
}
