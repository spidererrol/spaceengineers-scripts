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
         * Running Lights
         * Version: $MDK_DATETIME$
         */
        #endregion mdk macros

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

            Runtime.UpdateFrequency = UpdateFrequency.Update100;
        }

        //FIXME: departing needs to be set per-suffix!
        public bool departing = true;

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

            ConsoleSurface console = ConsoleSurface.EasyConsole(this);
            console.ClearScreen();

            console.Echo("Running Lights");
            console.Echo("--------------");

            List<IMyShipConnector> connectors = GetBlocks.ByName<IMyShipConnector>("Vehicle Parking Connector");
            foreach (IMyShipConnector connector in connectors)
            {
                System.Text.RegularExpressions.Regex reSuffix = new System.Text.RegularExpressions.Regex(@"Vehicle Parking Connector (.*)$",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                System.Text.RegularExpressions.MatchCollection matches = reSuffix.Matches(connector.CustomName);
                if (matches.Count <= 0 || matches[0].Groups.Count <= 1)
                {
                    console.Warn("Skipping " + connector.CustomName + " as cannot find suffix");
                    continue;
                }
                string suffix = matches[0].Groups[1].Value;

                console.Echo("Processing " + suffix);

                // IMyShipConnector connector = GetBlocks.FirstByName<IMyShipConnector>("Vehicle Parking Connector A");
                IMySensorBlock sensor = GetBlocks.FirstByName<IMySensorBlock>("Vehicle Parking Sensor " + suffix);

                if (sensor == null)
                {
                    console.Err("Cannot find 'Vehicle Parking Sensor " + suffix + "'");
                    return;
                }

                bool on = sensor.IsActive && connector.Status != MyShipConnectorStatus.Connected;

                if (connector.Status == MyShipConnectorStatus.Connected)
                    departing = true;
                else if (!on)
                    departing = false;

                // "Corner Light - Double 35"
                List<IMyLightingBlock> lights = GetBlocks.ByName<IMyLightingBlock>("Running Lights " + suffix);
                if (lights.Count < 2)
                {
                    console.Err("I need at least 2 lights (first and last) to auto setup");
                }
                bool init = lights.Count == 2;
                Func<IMyTerminalBlock, int> GetPos;
                Func<IMyLightingBlock, int> NotPos1;
                Func<IMyLightingBlock, int> NotPos2;
                if (lights[0].Position.X != lights[1].Position.X)
                {
                    GetPos = l => l.Position.X;
                    NotPos1 = l => l.Position.Y;
                    NotPos2 = l => l.Position.Z;
                }
                else if (lights[0].Position.Y != lights[1].Position.Y)
                {
                    GetPos = l => l.Position.Y;
                    NotPos1 = l => l.Position.X;
                    NotPos2 = l => l.Position.Z;
                }
                else if (lights[0].Position.Z != lights[1].Position.Z)
                {
                    GetPos = l => l.Position.Z;
                    NotPos1 = l => l.Position.X;
                    NotPos2 = l => l.Position.Y;
                }
                else
                {
                    console.Err("Cannot determine direction of lights for '" + suffix + "'!");
                    continue;
                }
                int baseOffset = int.MaxValue;
                int baseMaxOffset = int.MinValue;
                int basePos1 = NotPos1(lights[0]);
                int basePos2 = NotPos2(lights[0]);
                foreach (IMyLightingBlock light in lights)
                {
                    if (GetPos(light) < baseOffset)
                        baseOffset = GetPos(light);
                    if (GetPos(light) > baseMaxOffset)
                        baseMaxOffset = GetPos(light);
                }
                if (init)
                {
                    console.Warn("Detecting lights for '" + suffix + "'");
                    lights = GetBlocks.ByType<IMyLightingBlock>();
                }
                foreach (IMyLightingBlock light in lights)
                {
                    if (light == null)
                        continue;

                    if (init && (NotPos1(light) != basePos1 || NotPos2(light) != basePos2))
                        continue;

                    if (GetPos(light) > baseMaxOffset || GetPos(light) < baseOffset)
                        continue;

                    int c = 1 + GetPos(light) - baseOffset;

                    if (init)
                        light.CustomName = "Running Lights " + suffix + " " + c.ToString("D");

                    bool direction = departing;
                    if (GetPos(connector) > baseOffset)
                        direction = !direction;
                    if (!direction)
                        c = 1 + lights.Count - c;
                    if (on != light.Enabled)
                        light.ApplyAction("OnOff");
                    light.BlinkIntervalSeconds = 1.0f;
                    light.BlinkLength = 10f;
                    light.BlinkOffset = 10f * (c % 10);

                }
            }
        }
    }
}