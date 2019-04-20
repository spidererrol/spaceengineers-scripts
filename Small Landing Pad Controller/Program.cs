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
        private const string SectionName = "Pad Controller";
        ConsoleSurface console;
        List<string> actionLines;
        //DoorStatus hatchStatus = DoorStatus.Open; // If hatches are closed then I will turn everything off which is fine, if open then I will leave alone.
        Dictionary<long, DoorStatus> hatchStates = new Dictionary<long, DoorStatus>();

        void Main(string argument, UpdateType updateType)
        {
            //float cycleRate = 0.0f;
            Config.ConfigSection config = Config.Section(Me, SectionName);
            //config.Get("CyclePads", cycleRate);
            //config.SetComment("CyclePads", "How may seconds to indicate for each gear. Set to zero to disable");
            config.Default("FullyChargedPercent", 99.0f);
            config.SetComment("FullyChargedPercent", "If batteries are above this level then consider them fully charged");
            config.Default("LowPowerPercent", 10.0f);
            config.SetComment("LowPowerPercent", "If batteries are below this level then power is considered critical");
            config.Save();

            console = ConsoleSurface.EasyConsole(this, consoleTag: "Control Status", sectionName: SectionName);
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
                console.Echo("Setting Maintainance to " + OnOff);
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

            Dictionary<string, SideStates> statesMap = new Dictionary<string, SideStates>
            {
                { "Top", topState },
                { "Bottom", bottomState }
            };

            return statesMap;
        }
    }
}