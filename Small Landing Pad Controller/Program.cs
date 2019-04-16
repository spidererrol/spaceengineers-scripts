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
        // Release: $MDK_DATETIME$
        #endregion
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        private const double PowerFactor = 0.99; // Used to ensure that I get something to charge with.
        private const string MaintainanceTag = "Maintainance";
        private const string MaintainanceAccessName = "Maintainance Access";
        ConsoleSurface console;
        List<string> actionLines;
        //DoorStatus hatchStatus = DoorStatus.Open; // If hatches are closed then I will turn everything off which is fine, if open then I will leave alone.
        Dictionary<long, DoorStatus> hatchStates = new Dictionary<long, DoorStatus>();

        void Main(string argument, UpdateType updateType)
        {
            //float cycleRate = 0.0f;
            Config.ConfigSection config = Config.Section(Me, "Pad Controller");
            //config.Get("CyclePads", cycleRate);
            //config.SetComment("CyclePads", "How may seconds to indicate for each gear. Set to zero to disable");
            config.Default("FullyChargedPercent", 99.0f);
            config.SetComment("FullyChargedPercent", "If batteries are above this level then consider them fully charged");
            config.Default("LowPowerPercent", 10.0f);
            config.SetComment("LowPowerPercent", "If batteries are below this level then power is considered critical");
            config.Save();

            console = ConsoleSurface.EasyConsole(this, consoleTag: "Control Status", sectionName: "Pad Controller");
            console.ClearScreen();
            actionLines = new List<string>();

            //double unixTime = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;

            ManagePower(config);
            ManageHatches(config);
            Dictionary<string, SideStates> states = ManageLights(config);
            if (argument != null && argument.Trim().Length > 0 && !(updateType.HasFlag(UpdateType.Update1) || updateType.HasFlag(UpdateType.Update10) || updateType.HasFlag(UpdateType.Update100)))
                ProcessAction(config, states, argument);

            // Keep at end!
            actionLines.ForEach(line => console.Echo(line));
            actionLines.Clear();
        }

        private void ProcessAction(Config.ConfigSection config, Dictionary<string, SideStates> states, string argument)
        {
            if (argument != "Top" && argument != "Bottom")
            {
                console.Echo("ERROR: Called with unknown command: " + argument);
                return;
            }
            SideStates state = states[argument];
            string lockAction = "Lock";
            if (state == SideStates.Locked)
                lockAction = "Unlock";
            RunActions(getBlocksByName<IMyShipConnector>(argument), lockAction);
            RunActions(getBlocksByName<IMyLandingGear>(argument), lockAction);
        }

        private void ManageHatches(Config.ConfigSection config)
        {
            List<IMyDoor> hatches = GetBlocksOfType<IMyDoor>();
            foreach (IMyDoor hatch in hatches)
            {
                long entityId = hatch.EntityId;
                if (!hatchStates.ContainsKey(entityId))
                {
                    hatchStates.Add(entityId, hatch.Status);
                    continue;
                }
                DoorStatus was = hatchStates[entityId];
                DoorStatus now = hatch.Status;
                if (now == DoorStatus.Opening)
                    now = DoorStatus.Closed; // This delays on until after opened to avoid the "Fridge Light" problem... (except it doesn't).
                if (now == DoorStatus.Closing)
                    now = DoorStatus.Closed;
                if (was == now)
                    continue;
                string OnOff = ((now == DoorStatus.Open) ? "On" : "Off");
                getBlocksByName<IMyLightingBlock>(MaintainanceTag).FindAll(b => !b.CustomName.Contains(MaintainanceAccessName)).ForEach(b => RunActions(b, "OnOff_" + OnOff));
                getBlocksByName<IMyTextPanel>(MaintainanceTag).FindAll(b => !b.CustomName.Contains(MaintainanceAccessName)).ForEach(b => RunActions(b, "OnOff_" + OnOff));
                getBlocksByName<IMyButtonPanel>(MaintainanceTag).FindAll(b => !b.CustomName.Contains(MaintainanceAccessName)).ForEach(b => RunActions(b, "OnOff_" + OnOff));
                hatchStates[entityId] = now;
            }
        }

        private void UpdateBattery(IMyBatteryBlock batt, ChargeMode mode)
        {
            if (batt.ChargeMode != mode)
            {
                batt.ChargeMode = mode;
                actionLines.Add("Setting Battery " + batt.CustomName + " to " + mode.ToString());
            }
        }

        private void UpdateReactors(List<IMyReactor> reactors, bool enable)
        {
            if (reactors.Any(r => r.Enabled != enable))
            {
                actionLines.Add((enable ? "Enabling" : "Disabling") + " reactors");
                RunActions(reactors, "OnOff_" + (enable ? "On" : "Off"));
            }
        }

        private void ManagePower(Config.ConfigSection config)
        {
            List<IMySolarPanel> solarPanels = GetBlocksOfType<IMySolarPanel>();
            List<IMyReactor> reactors = GetBlocksOfType<IMyReactor>();
            List<IMyBatteryBlock> batteries = GetBlocksOfType<IMyBatteryBlock>();

            List<IMySolarPanel> extSolar = GetBlocksOfType<IMySolarPanel>(b => !b.IsSameConstructAs(Me));
            List<IMyReactor> extReactor = GetBlocksOfType<IMyReactor>(b => !b.IsSameConstructAs(Me));
            List<IMyBatteryBlock> extBatteries = GetBlocksOfType<IMyBatteryBlock>(b => !b.IsSameConstructAs(Me));

            float externalMaxPower = extSolar.Sum(p => p.MaxOutput) + extReactor.Sum(p => p.MaxOutput) + extBatteries.Sum(p => p.MaxOutput) + solarPanels.Sum(p => p.MaxOutput); // Treat local solar the same as exteral.
            float externalCurPower = extSolar.Sum(p => p.CurrentOutput) + extReactor.Sum(p => p.CurrentOutput) + extBatteries.Sum(p => p.CurrentOutput) + solarPanels.Sum(p => p.CurrentOutput); // Treat local solar the same as exteral.

            float batsMax = batteries.Sum(p => p.MaxStoredPower);
            float batsCur = batteries.Sum(p => p.CurrentStoredPower);

            float batsOut = batteries.Sum(p => p.CurrentOutput);
            float batsIn = batteries.Sum(p => p.CurrentInput);

            float fulllevel = config.Get("FullyChargedPercent").ToSingle() / 100.0f;
            float critlevel = config.Get("LowPowerPercent").ToSingle() / 100.0f;

            string batteryStates = "";
            foreach (IMyBatteryBlock batt in batteries)
            {
                string battState = "[";
                switch (batt.ChargeMode)
                {
                    case ChargeMode.Auto:
                        battState += "~"; // "A" looks to much like "R" in the default font.
                        break;
                    case ChargeMode.Discharge:
                        battState += "D";
                        break;
                    case ChargeMode.Recharge:
                        battState += "R";
                        break;
                }
                float battLevel = (100.0f * batt.CurrentStoredPower) / batt.MaxStoredPower;
                battState += ":" + battLevel.ToString("N0") + "%]";
                batteryStates += battState;
            }
            console.Echo("Batteries:" + batteryStates);

            string reactorStates = "";
            foreach (IMyReactor reactor in reactors)
            {
                reactorStates += "[" + (reactor.Enabled ? "On" : "Off") + "]";
            }
            console.Echo("Reactors:" + reactorStates);

            int providerBatts = 0;
            List<IMyBatteryBlock> fullBats = batteries.FindAll(batt => batt.CurrentStoredPower / batt.MaxStoredPower >= fulllevel);
            foreach (IMyBatteryBlock batt in fullBats)
            {
                UpdateBattery(batt, ChargeMode.Auto);
                providerBatts++;
            }

            if (fullBats.Count == batteries.Count)
            {
                // Everything full.
                UpdateReactors(reactors, false);
                return;
            }

            List<IMyBatteryBlock> critBats = batteries.FindAll(batt => batt.CurrentStoredPower / batt.MaxStoredPower <= critlevel);
            if (critBats.Count > 0)
            {
                UpdateReactors(reactors, true);
                if (critBats.Any(batt => batt.ChargeMode == ChargeMode.Recharge))
                    return; // At least one battery is already charging. No need to change anything.

                // Ensure all non-critical batteries are providing power:
                foreach (IMyBatteryBlock batt in batteries.Except(critBats))
                    UpdateBattery(batt, ChargeMode.Auto);

                if ((externalMaxPower - (externalCurPower - batsIn)) * PowerFactor <= batsOut)
                {
                    critBats.ForEach(batt => UpdateBattery(batt, ChargeMode.Auto));
                    return;
                }

                // Recharge the first critical battery:
                UpdateBattery(critBats.First(), ChargeMode.Recharge);
                return;
            }

            // No batteries are full nor critical:
            if (batteries.Any(b => b.ChargeMode == ChargeMode.Auto))
                UpdateReactors(reactors, false);

            // If I've no spare power, then retain the status quo:
            if ((externalMaxPower - (externalCurPower - batsIn)) * PowerFactor <= batsOut)
                return;

            // Someone is already charging, let them charge:
            if (batteries.Any(b => b.ChargeMode == ChargeMode.Recharge))
                return;

            // Choose any non-"full" battery and charge it:
            UpdateBattery(batteries.First(batt => batt.CurrentStoredPower / batt.MaxStoredPower < fulllevel), ChargeMode.Recharge);
        }

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
                // I could and probably should use constant colours here but some seem a bit weird to me when I want these nice distinct colours.
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

        private Dictionary<string, SideStates> ManageLights(Config.ConfigSection config)
        {
            IMyShipConnector topConnector = getBlockByName<IMyShipConnector>("Top");
            IMyShipConnector bottomConnector = getBlockByName<IMyShipConnector>("Bottom");
            List<IMyLandingGear> topGears = getBlocksByName<IMyLandingGear>("Top");
            List<IMyLandingGear> bottomGears = getBlocksByName<IMyLandingGear>("Bottom");
            SideStates topState = SideStates.Unknown;
            SideStates bottomState = SideStates.Unknown;

            Best(ref topState, topConnector);
            Best(ref bottomState, bottomConnector);
            topGears.ForEach(g => Best(ref topState, g));
            bottomGears.ForEach(g => Best(ref bottomState, g));

            Color topColor = Col(topState);
            Color bottomColor = Col(bottomState);

            foreach (IMyLightingBlock light in getBlocksByName<IMyLightingBlock>("Top"))
            {
                light.Color = topColor;
            }

            foreach (IMyLightingBlock light in getBlocksByName<IMyLightingBlock>("Bottom"))
            {
                light.Color = bottomColor;
            }

            Dictionary<string, SideStates> statesMap = new Dictionary<string, SideStates>();

            statesMap.Add("Top", topState);
            statesMap.Add("Bottom", bottomState);

            return statesMap;
        }


        /*


                // Set this to true to flash through the states of each pad. You might want a fast repeat though.  
                // You may get problems when the batteries switch over with a faster repeat.  
                bool doFlash = false;




                Int32 unixTimestamp;
                Int32 tick = 0;

                IMyTextPanel status = null;
                bool hatchOpen = false;

                IMyBatteryBlock curCharging = null;

                void Main(string argument)
                {
                    IMyProgrammableBlock self = null;

                    List<IMyTerminalBlock> progs = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyProgrammableBlock>(progs);
                    for (int i = 0; i < progs.Count; i++)
                    {
                        IMyProgrammableBlock prog = progs[i] as IMyProgrammableBlock;
                        if (prog.IsRunning)
                        {
                            self = prog;
                            break;
                        }
                    }

                    GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(progs);
                    for (int i = 0; i < progs.Count; i++)
                    {
                        IMyTextPanel prog = progs[i] as IMyTextPanel;
                        if (prog.CubeGrid == self.CubeGrid && prog.CustomName == "Control Status")
                        {
                            status = prog;
                            break;
                        }
                    }

                    if (status != null)
                    {
                        status.ShowPublicTextOnScreen();
                        status.WritePublicText("");
                    }

                    bool newHatch = true;
                    List<IMyTerminalBlock> hatches = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyDoor>(hatches);
                    for (int i = 0; i < hatches.Count; i++)
                    {
                        IMyDoor hatch = hatches[i] as IMyDoor;
                        if (hatch.CubeGrid == self.CubeGrid && !hatch.Open)
                            newHatch = false;
                    }

                    if (newHatch != hatchOpen)
                    {
                        hatchOpen = newHatch;
                        if (status != null)
                            status.RequestEnable(hatchOpen);
                        List<IMyTerminalBlock> mb = new List<IMyTerminalBlock>();
                        GridTerminalSystem.GetBlocksOfType<IMyButtonPanel>(mb);
                        for (int i = 0; i < mb.Count; i++)
                        {
                            if (mb[i].CubeGrid != self.CubeGrid)
                                continue;
                            if (mb[i].CustomName == "Maintainance Access")
                                continue;
                            if (!mb[i].CustomName.Contains("Maintainance"))
                                continue;
                            IMyButtonPanel b = mb[i] as IMyButtonPanel;
                            //b.RequestEnable(hatchOpen);  
                            if (hatchOpen)
                                b.GetActionWithName("OnOff_On").Apply(b);
                            else
                                b.GetActionWithName("OnOff_Off").Apply(b);
                        }
                        GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(mb);
                        for (int i = 0; i < mb.Count; i++)
                        {
                            if (mb[i].CubeGrid != self.CubeGrid)
                                continue;
                            if (mb[i].CustomName == "Maintainance Access")
                                continue;
                            if (!mb[i].CustomName.Contains("Maintainance"))
                                continue;
                            IMyLightingBlock b = mb[i] as IMyLightingBlock;
                            b.RequestEnable(hatchOpen);
                        }
                        GridTerminalSystem.GetBlocksOfType<IMyTextPanel>(mb);
                        for (int i = 0; i < mb.Count; i++)
                        {
                            if (mb[i].CubeGrid != self.CubeGrid)
                                continue;
                            if (mb[i].CustomName == "Maintainance Access")
                                continue;
                            if (!mb[i].CustomName.Contains("Maintainance"))
                                continue;
                            IMyTextPanel b = mb[i] as IMyTextPanel;
                            b.RequestEnable(hatchOpen);
                        }
                    }

                    if (doFlash)
                    {
                        unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                        tick = tick + 1;
                        tick = tick % 32768;
                    }

                    doBatteries();
                    doControl(self, argument);
                }

                void MyEcho(string s)
                {
                    Echo(s);
                    if (status != null)
                    {
                        status.WritePublicText(s + "\n", true);
                    }
                }

                void doControl(IMyProgrammableBlock self, string argument)
                {
                    List<IMyTerminalBlock> lights = new List<IMyTerminalBlock>();
                    List<IMyLightingBlock> mylights = new List<IMyLightingBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyLightingBlock>(lights);
                    for (int i = 0; i < lights.Count; i++)
                    {
                        IMyLightingBlock light = lights[i] as IMyLightingBlock;
                        if (light.CubeGrid == self.CubeGrid)
                            mylights.Add(light);
                    }

                    bool topOn = true;
                    bool bottomOn = true;
                    Color topColor = new Color(255, 0, 0);
                    Color bottomColor = new Color(0, 0, 255);

                    IMyShipConnector topConnector = null;
                    IMyShipConnector bottomConnector = null;
                    List<IMyTerminalBlock> connectors = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyShipConnector>(connectors);
                    for (int i = 0; i < connectors.Count; i++)
                    {
                        if (connectors[i].CubeGrid != self.CubeGrid)
                            continue;
                        if (connectors[i].CustomName.Contains("Top"))
                            topConnector = connectors[i] as IMyShipConnector;
                        if (connectors[i].CustomName.Contains("Bottom"))
                            bottomConnector = connectors[i] as IMyShipConnector;
                    }

                    List<IMyTerminalBlock> gears = new List<IMyTerminalBlock>();
                    List<Color> topColors = new List<Color>();
                    List<Color> bottomColors = new List<Color>();
                    GridTerminalSystem.GetBlocksOfType<IMyLandingGear>(gears);
                    bool anyTopLocked = false;
                    bool anyBottomLocked = false;
                    bool anyTopNear = false;
                    bool anyBottomNear = false;
                    bool anyTopAuto = false;
                    bool anyBottomAuto = false;
                    bool doLock = false;
                    bool first = true;
                    for (int i = 0; i < gears.Count; i++)
                    {
                        IMyLandingGear gear = gears[i] as IMyLandingGear;
                        if (gear.CubeGrid == self.CubeGrid)
                        {
                            bool isNear = false;
                            bool isLocked = false;
                            bool isAuto = false;
                            bool isTop = gear.CustomName.Contains("Top");
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
                                    else if (value.StartsWith("Auto Lock"))
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
                            bool changeLock = false;
                            List<Color> cur = null;
                            if (gear.CustomName.Contains("Top"))
                            {
                                cur = topColors;
                                if (argument != null && argument.Contains("Top"))
                                    changeLock = true;
                            }
                            else if (gear.CustomName.Contains("Bottom"))
                            {
                                cur = bottomColors;
                                if (argument != null && argument.Contains("Bottom"))
                                    changeLock = true;
                            }
                            else
                            {
                                continue;
                            }
                            if (isLocked)
                            {
                                cur.Add(Color.Lime); // I would have called this green, but whatever.  
                            }
                            else if (isNear)
                            {
                                cur.Add(Color.Yellow);
                            }
                            else if (isAuto)
                            {
                                cur.Add(Color.Blue);
                            }
                            else
                            {
                                cur.Add(Color.Black);
                            }
                            if (changeLock)
                            {
                                if (first)
                                {
                                    doLock = !isLocked;
                                    IMyShipConnector connchange = null;
                                    if (gear.CustomName.Contains("Top"))
                                        connchange = topConnector;
                                    else if (gear.CustomName.Contains("Bottom"))
                                        connchange = bottomConnector;
                                    if (connchange != null)
                                    {
                                        if (doLock)
                                        {
                                            connchange.GetActionWithName("Lock").Apply(connchange);
                                        }
                                        else
                                        {
                                            connchange.GetActionWithName("Unlock").Apply(connchange);
                                        }
                                    }
                                }
                                if (doLock)
                                {
                                    gear.GetActionWithName("Lock").Apply(gear);
                                }
                                else
                                {
                                    gear.GetActionWithName("Unlock").Apply(gear);
                                }
                                first = false;
                            }
                        }
                    }

                    if (argument != null && argument.Trim().Length > 0)
                    {
                        bool anyLocked = false;
                        if (argument.Contains("Top"))
                            anyLocked = anyTopLocked;
                        if (argument.Contains("Bottom"))
                            anyLocked = anyBottomLocked;
                        if (anyLocked == doLock)
                        {
                            for (int i = 0; i < gears.Count; i++)
                            {
                                IMyLandingGear gear = gears[i] as IMyLandingGear;
                                if (gear.CubeGrid != self.CubeGrid)
                                    continue;
                                if (!gear.CustomName.Contains(argument))
                                    continue;
                                if (!doLock)
                                {
                                    gear.GetActionWithName("Lock").Apply(gear);
                                }
                                else
                                {
                                    gear.GetActionWithName("Unlock").Apply(gear);
                                }
                            }
                        }
                    }

                    if (doFlash)
                    {
                        if (topColors.Count > 0)
                        {
                            int topi = tick % topColors.Count;
                            topColor = topColors[topi];
                            if (topColor == Color.Black)
                                topOn = false;
                        }
                        else
                        {
                            topColor = Color.Red;
                        }

                        if (bottomColors.Count > 0)
                        {
                            int bottomi = tick % bottomColors.Count;
                            bottomColor = bottomColors[bottomi];
                            if (bottomColor == Color.Black)
                                bottomOn = false;
                        }
                        else
                        {
                            bottomColor = Color.Red;
                        }
                    }
                    else
                    {
                        topColor = Color.Black;
                        topOn = false;
                        if (anyTopLocked)
                        {
                            topColor = Color.Lime;
                            topOn = true;
                        }
                        else if (anyTopNear)
                        {
                            topColor = Color.Yellow;
                            topOn = true;
                        }
                        else if (anyTopAuto)
                        {
                            topColor = Color.Blue;
                            topOn = true;
                        }
                        bottomColor = Color.Black;
                        bottomOn = false;
                        if (anyBottomLocked)
                        {
                            bottomColor = Color.Lime;
                            bottomOn = true;
                        }
                        else if (anyBottomNear)
                        {
                            bottomColor = Color.Yellow;
                            bottomOn = true;
                        }
                        else if (anyBottomAuto)
                        {
                            bottomColor = Color.Blue;
                            bottomOn = true;
                        }
                    }

                    for (int i = 0; i < mylights.Count; i++)
                    {
                        if (!mylights[i].CustomName.Contains("Indicator"))
                            continue;
                        if (mylights[i].CustomName.Contains("Maintainance"))
                        {
                            if (!hatchOpen)
                            {
                                mylights[i].GetActionWithName("OnOff_Off").Apply(mylights[i]);
                                mylights[i].SetValue("Color", Color.Red);
                                continue;
                            }
                        }
                        if (mylights[i].CustomName.Contains("Top"))
                        {
                            if (topOn)
                            {
                                mylights[i].GetActionWithName("OnOff_On").Apply(mylights[i]);
                                mylights[i].SetValue("Color", topColor);
                            }
                            else
                            {
                                mylights[i].GetActionWithName("OnOff_Off").Apply(mylights[i]);
                            }
                        }
                        if (mylights[i].CustomName.Contains("Bottom"))
                        {
                            if (bottomOn)
                            {
                                mylights[i].GetActionWithName("OnOff_On").Apply(mylights[i]);
                                mylights[i].SetValue("Color", bottomColor);
                            }
                            else
                            {
                                mylights[i].GetActionWithName("OnOff_Off").Apply(mylights[i]);
                            }
                        }
                    }
                }

                void doBatteries()
                {
                    List<IMyTerminalBlock> allpower = new List<IMyTerminalBlock>();
                    bool haveExternalPower = false;

                    // 1 - try solar panels (always consider them "external" if they are supplying power): 
                    GridTerminalSystem.GetBlocksOfType<IMySolarPanel>(allpower);
                    if (allpower.Count > 0)
                    {
                        for (int i = 0; i < allpower.Count; i++)
                        {
                            IMySolarPanel sp = allpower[i] as IMySolarPanel;
                            if (!sp.DetailedInfo.Contains("Max Output: 0 "))
                            {
                                //MyEcho("Panel " + sp.CustomName + " is acceptable"); 
                                haveExternalPower = true;
                                break;
                            }
                        }
                    }
                    if (!haveExternalPower)
                    {
                        // 2 - try external batteries: 
                        GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(allpower);
                        for (int i = 0; i < allpower.Count; i++)
                        {
                            if (allpower[i].CubeGrid == Me.CubeGrid)
                                continue;
                            haveExternalPower = true;
                            break;
                        }
                    }
                    List<IMyReactor> myreactors = new List<IMyReactor>();
                    if (!haveExternalPower)
                    {
                        // 3 - try external reactors:  
                        GridTerminalSystem.GetBlocksOfType<IMyReactor>(allpower);
                        for (int i = 0; i < allpower.Count; i++)
                        {
                            if (allpower[i].CubeGrid == Me.CubeGrid)
                            {
                                myreactors.Add((IMyReactor)(allpower[i]));
                                continue;
                            }
                            haveExternalPower = true;
                            //break; // I want to continue scanning to fill myreactors. 
                        }
                    }
                    if (myreactors.Count <= 0)
                    {
                        GridTerminalSystem.GetBlocksOfType<IMyReactor>(allpower);
                        for (int i = 0; i < allpower.Count; i++)
                        {
                            if (allpower[i].CubeGrid == Me.CubeGrid)
                            {
                                myreactors.Add((IMyReactor)(allpower[i]));
                            }
                        }
                    }

                    List<IMyTerminalBlock> allbatteries = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(allbatteries);
                    List<IMyBatteryBlock> mybatteries = new List<IMyBatteryBlock>();
                    IMyBatteryBlock best = null;
                    double bestcharge = 0; // I'm assuming equal capacities here to save maths.       
                    bool bestcharging = false;
                    double bestmax = 0;
                    IMyBatteryBlock worst = null;
                    double worstcharge = 0; // I'm assuming equal capacities here to save maths.       
                    bool worstcharging = false;
                    double worstmax = 0;
                    double totalcharge = 0;
                    double totalmax = 0;
                    double curCharge = 0;
                    double curMax = 0;
                    double rate = 0;
                    for (int b = 0; b < allbatteries.Count; b++)
                    {
                        IMyBatteryBlock cur = allbatteries[b] as IMyBatteryBlock;
                        if (cur.CubeGrid != Me.CubeGrid)
                        {
                            continue;
                        }
                        string[] details = cur.DetailedInfo.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);
                        double charge = 0; double maxCharge = 0, output = 0, max = 0, maxDraw = 0, input = 0;
                        bool charging = false;

                        mybatteries.Add(cur);

                        for (int a = 0; a < details.Length; a++)
                        {
                            string[] dets = details[a].Split(':');
                            string command = dets[0];
                            dets = dets[1].Trim().Split(' ');
                            if (command.StartsWith("Max Stored Power"))
                                maxCharge = float.Parse(dets[0]) * UnitMultiple(dets[1]);
                            else if (command.StartsWith("Stored power"))
                                charge = float.Parse(dets[0]) * UnitMultiple(dets[1]);
                            else if (command.StartsWith("Fully recharged"))   // in X hours, means we're trying to draw power.           
                                charging = true;
                            else if (command.StartsWith("Max Output"))
                                max = float.Parse(dets[0]) * UnitMultiple(dets[1]);
                            else if (command.StartsWith("Current Output"))
                                output = float.Parse(dets[0]) * UnitMultiple(dets[1]);
                            else if (command.StartsWith("Max Required Input"))
                            {
                                maxDraw = float.Parse(dets[0]) * UnitMultiple(dets[1]);
                            }
                            else if (command.StartsWith("Current Input"))
                            {
                                input += float.Parse(dets[0]) * UnitMultiple(dets[1]);
                            }
                        }
                        if (curCharging == cur)
                        {
                            curCharge = charge;
                            curMax = maxCharge;
                            if (charge < maxCharge && haveExternalPower && !charging)
                                cur.GetActionWithName("Recharge").Apply(cur);
                        }
                        else if (charging)
                        {
                            cur.GetActionWithName("Recharge").Apply(cur);
                        }
                        totalcharge += charge;
                        totalmax += maxCharge;
                        rate += input - output;
                        MyEcho("Scanning " + cur.CustomName + " = " + ((int)charge));
                        if (charge == maxCharge)
                        {
                            worst = best;
                            worstcharge = bestcharge;
                            worstcharging = bestcharging;
                            worstmax = bestmax;
                            best = cur;
                            bestcharge = charge;
                            bestcharging = charging;
                            bestmax = maxCharge;
                        }
                        else if (best == null)
                        {
                            best = cur;
                            bestcharge = charge;
                            bestcharging = charging;
                            bestmax = maxCharge;
                        }
                        else if (best != null)
                        {
                            if (bestcharge < charge)
                            {
                                worst = best;
                                worstcharge = bestcharge;
                                worstcharging = bestcharging;
                                worstmax = bestmax;
                                best = cur;
                                bestcharge = charge;
                                bestcharging = charging;
                                bestmax = maxCharge;
                            }
                        }
                        if (cur != best)
                        {
                            worst = cur;
                            worstcharge = charge;
                            worstcharging = charging;
                            worstmax = maxCharge;
                        }
                    }

                    string ratestring = prettyUnit(Math.Abs(rate));
                    if (rate > 0)
                        ratestring = "+" + ratestring;
                    else if (rate < 0)
                        ratestring = "-" + ratestring;
                    MyEcho(
                            "Total: "
                            + ((int)totalcharge)
                            + "/"
                            + ((int)totalmax)
                            + " ("
                            + ((int)((totalcharge * 100) / totalmax))
                            + "%) "
                            + ratestring
                    );

                    if (!haveExternalPower)
                        MyEcho("No charging source available");
                    else
                        MyEcho("Charging source is available");

                    if (curCharging == best && bestcharge >= bestmax)
                        curCharging = null;

                    if ((bestcharge == bestmax && worstcharge == worstmax) || !haveExternalPower)
                    {
                        if (haveExternalPower)
                            MyEcho("System Fully Charged");
                        if (bestcharging)
                            best.GetActionWithName("Recharge").Apply(best);
                        if (worstcharging)
                            worst.GetActionWithName("Recharge").Apply(worst);
                    }
                    else if (curCharging != null && curCharge < curMax)
                    {
                        MyEcho("Continue Charging " + curCharging.CustomName);
                    }
                    else
                    {
                        curCharging = worst;
                    }

                    int bestpct = (int)((bestcharge * 100) / bestmax);
                    int worstpct = (int)((worstcharge * 100) / worstmax);

                    MyEcho("Best = " + best.CustomName + " (" + bestpct + "%)");
                    MyEcho("Worst = " + worst.CustomName + " (" + worstpct + "%)");

                    if (bestcharging)
                        MyEcho("Recharging " + best.CustomName);
                    else
                        MyEcho("Draining " + best.CustomName);

                    if (worstcharging)
                        MyEcho("Recharging " + worst.CustomName);
                    else
                        MyEcho("Draining " + worst.CustomName);

                    double readyPower = 0;
                    if (!bestcharging)
                        readyPower += bestcharge;
                    if (!worstcharging)
                        readyPower += worstcharge;

                    bool reactorOn = true;
                    string rState = "Unknown";
                    if (readyPower < 20)
                    {
                        rState = "On [Low Charge]";
                    }
                    else if ((bestcharging || worstcharging) && !(bestcharging && worstcharging))
                    {
                        reactorOn = false;
                        rState = "Off";
                    }
                    else
                    {
                        rState = "On";
                    }
                    bool reactorOK = false;
                    for (int i = 0; i < myreactors.Count; i++)
                    {
                        IMyReactor r = myreactors[i];
                        bool cur = r.Enabled;
                        //MyEcho(r.CustomName); 
                        if (!cur || (r.IsFunctional && r.IsWorking))
                            reactorOK = true;
                        if (cur == reactorOn)
                            continue;
                        if (reactorOn)
                        {
                            MyEcho("Enabling reactor " + r.CustomName);
                            r.GetActionWithName("OnOff_On").Apply(r);
                        }
                        else
                        {
                            MyEcho("Shutting down reactor " + r.CustomName);
                            r.GetActionWithName("OnOff_Off").Apply(r);
                        }
                    }
                    if (!reactorOK)
                        rState = "Not Working";
                    MyEcho("Reactor(s): " + rState);

                    if (!hatchOpen && bestpct <= 1)
                    {
                        bool reactorWorking = false;
                        for (int i = 0; i < myreactors.Count; i++)
                        {
                            if (myreactors[i].IsFunctional && myreactors[i].IsWorking)
                            {
                                reactorWorking = true;
                                break;
                            }
                        }

                        if (!reactorWorking)
                            openHatch();
                    }

                }

                void openHatch()
                {
                    List<IMyTerminalBlock> hatches = new List<IMyTerminalBlock>();
                    GridTerminalSystem.GetBlocksOfType<IMyDoor>(hatches);
                    for (int i = 0; i < hatches.Count; i++)
                    {
                        IMyDoor hatch = hatches[i] as IMyDoor;
                        if (hatch.CubeGrid == Me.CubeGrid && !hatch.Open)
                            hatch.GetActionWithName("Open_On").Apply(hatch);
                    }
                }

                string prettyUnit(double din)
                {
                    int i;
                    string u = "";
                    if (din > 1000000000)
                    {
                        din /= 1000000000;
                        u = "GW";
                    }
                    else if (din > 1000000)
                    {
                        din /= 1000000;
                        u = "MW";
                    }
                    else if (din > 1000)
                    {
                        din /= 1000;
                        u = "KW";
                    }
                    else
                    {
                        u = "W";
                    }
                    i = (int)din;
                    return i + u;
                }

                float UnitMultiple(string unit)
                {
                    if (unit.StartsWith("W")) return 0.001f;
                    if (unit.StartsWith("kW")) return 1f;
                    if (unit.StartsWith("MW")) return 1000f;
                    if (unit.StartsWith("GW")) return 1000000f;
                    return 0.001f;
                }
        //*/
    }
}