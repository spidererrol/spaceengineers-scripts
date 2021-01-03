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

        public class DoorQ {

            static TimeSpan timeout = new TimeSpan(0, 0, 6); // 6 seconds

            public readonly GetBlocksClass GetBlocks;
            public DateTime entered;
            public string doorname;
            public string airlockname;
            public bool decompress;

            public IMyDoor door => GetBlocks.FirstByName<IMyDoor>(doorname);
            public IMyAirVent airlock => GetBlocks.FirstByName<IMyAirVent>(airlockname);

            public string Name => airlockname + "|" + doorname;

            public bool due => (DateTime.Now - entered) > timeout
                        ? true
                        : decompress && airlock.Status == VentStatus.Depressurized
                        ? true
                        : (!decompress) && airlock.Status == VentStatus.Pressurized
                        ? true
                        : false;

            public DoorQ(GetBlocksClass getBlocks, IMyDoor door, IMyAirVent vent, bool decomp) {
                GetBlocks = getBlocks;
                entered = DateTime.Now;
                doorname = door.CustomName;
                airlockname = vent.CustomName;
            }

            public void OpenDoor() => door.OpenDoor();
        }
        public List<DoorQ> doorQ = new List<DoorQ>();

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

            ConsoleSurface console = ConsoleSurface.EasyConsole(this, "[Airlock Control]");
            //console.ClearScreen();

            if (updateSource.HasFlag(UpdateType.Trigger)) {
                console.Echo("Request: " + argument);
                Utility.Options options = Utility.ParseArgs(argument);
                string airlock_zone = options.Arg[0];
                string player_zone = options.Arg[1];
                console.Echo(" Airlock: " + airlock_zone);
                console.Echo(" Switch: " + player_zone);

                IMyAirVent airlock = GetBlocks.FirstByName<IMyAirVent>(airlock_zone);
                IMyAirVent player = GetBlocks.FirstByName<IMyAirVent>(player_zone);

                if (airlock == null) {
                    console.Echo("Failed to find airvent: " + airlock_zone);
                    return;
                }
                if (player == null) {
                    console.Echo("Failed to find airvent: " + player_zone);
                    return;
                }

                console.Echo("Got vents");

                List<IMyDoor> doors = GetBlocks.ByName<IMyDoor>(airlock_zone);
                List<IMyDoor> notdoors = doors.FindAll(d => !d.CustomName.Contains(player_zone));
                IMyDoor door = doors.Find(d => d.CustomName.Contains(player_zone));

                console.Echo("Filtered doors");

                notdoors.ForEach(d => d.CloseDoor());

                console.Echo("Other doors closed");

                if (player.Status == airlock.Status) {
                    console.Echo(" READY!");
                    door.OpenDoor();
                    return;
                }

                console.Echo(" QUEUED!");
                if (player.Status == VentStatus.Depressurized || player.Status == VentStatus.Depressurizing) {
                    airlock.Depressurize = true;
                    doorQ.Add(new DoorQ(GetBlocks, door, airlock, true));
                } else {
                    airlock.Depressurize = false;
                    doorQ.Add(new DoorQ(GetBlocks, door, airlock, false));
                }
            }

            doorQ.FindAll(q => q.due).ForEach(q => {
                console.Echo("Activate: " + q.Name);
                q.OpenDoor();
                doorQ.Remove(q);
            });
        }
    }
}
