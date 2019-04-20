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
        #region mdk macros
        /*
         * TESTING SCRIPT!!!
         * $MDK_DATETIME$
         */
        #endregion mdk macros

        DateTime first;
        long tick1;
        long tick10;
        long tick100;

        public Program()
        {
            // The constructor, called only once every session and
            // always before any other method is called. Use it to
            // initialize your script. 
            //     
            // The constructor is optional and can be removed if not
            // needed.
            // 
            // It's recommended to set RuntimeInfo.UpdateFrequency 
            // here, which will allow your script to run itself without a 
            // timer block.
            tick1 = 0;
            tick10 = 0;
            tick100 = 0;
            Runtime.UpdateFrequency = UpdateFrequency.Update1 | UpdateFrequency.Update10 | UpdateFrequency.Update100;
        }

        public string Show(long factor, long count, long seconds)
        {
            if (count > 0)
                return (factor * count / seconds).ToString();
            return "(calculating...)";
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

            if (first == default(DateTime))
                first = DateTime.Now;

            if (updateSource.HasFlag(UpdateType.Update1))
                tick1++;
            if (updateSource.HasFlag(UpdateType.Update10))
                tick10++;
            if (updateSource.HasFlag(UpdateType.Update100))
                tick100++;

            long seconds = (long)(DateTime.Now - first).TotalSeconds;

            ConsoleSurface con = ConsoleSurface.EasyConsole(this, "[PLC]", "Pad Lifter Control");
            ConsoleSurface.EchoFunc Echo = con.GetEcho();

            Echo(first.ToString());
            Echo(DateTime.Now + " (" + seconds + ")");

            if (seconds > 0)
            {
                Echo("1: " + Show(1, tick1, seconds));
                Echo("10: " + Show(10, tick10, seconds));
                Echo("100: " + Show(100, tick100, seconds));
            }
            else
            {
                Echo("Please wait, calculating..." + seconds + "(" + DateTime.Now + ")");
            }
        }
    }
}