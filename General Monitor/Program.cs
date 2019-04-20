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
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        const string configSection = "General Monitor";
        // The Name of the LCD Panel to display battery power too.        
        string batteriesLCDName = "LCD Battery";
        string batteriesRatesLCDName = "Battery Rates LCD";
        string tanksH2Name = "LCD H2";
        string tanksO2Name = "LCD O2";

        // The Image set to use.           
        string imagePrefix = "Percent ";

        ConsoleSurface console;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        int GeneralPercent(string what, MultiSurface screens, float pct, int lastIndex = -1)
        {
            int index = (int)pct;

            if (index > 100 || index < 0)
                return -1; //error          
            if (index == lastIndex)
                return index;

            string imagename = imagePrefix + index.ToString("000");
            screens.SetCurrentImage(imagename);
            return index;
        }

        void BatteriesRate(List<IMyTextSurface> screens, float diff)
        {
            if (screens.Any())
            {
                foreach (IMyTextSurface lcd in screens)
                {
                    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                    if (diff > 0)
                    {
                        lcd.FontColor = new Color(0, 255, 0);
                        lcd.WriteText("+" + diff.ToString("0.00") + "MW", false);
                    }
                    else
                    {
                        lcd.FontColor = new Color(255, 0, 0);
                        lcd.WriteText(diff.ToString("0.00") + "MW", false);
                    }
                }
            }
        }

        void TanksPercent(string what, MultiSurface screens, List<IMyGasTank> tanks)
        {
            double fill = 0.0f;
            int count = 0;
            foreach (IMyGasTank tank in tanks)
            {
                fill += tank.FilledRatio;
                count++;
            }
            fill /= count;

            GeneralPercent(what, screens, (float)(100.0f * fill));
        }

        void ProcessBatteries(MultiSurface batteriesPercentScreens, MultiSurface batteriesRateScreens, List<IMyBatteryBlock> batteries)
        {
            float curPower = 0.0f;
            float maxPower = 0.0f;
            float curIn = 0.0f;
            float curOut = 0.0f;

            foreach (IMyBatteryBlock batt in batteries)
            {
                curPower += batt.CurrentStoredPower;
                maxPower += batt.MaxStoredPower;
                curIn += batt.CurrentInput;
                curOut += batt.CurrentOutput;
            }

            GeneralPercent("Batteries", batteriesPercentScreens, 100.0f * curPower / maxPower);
            BatteriesRate(batteriesRateScreens, curIn - curOut);
        }

        void Main(string arg)
        {
            Config.ConfigSection config = Config.Section(Me, configSection);
            config.Get("Battery LCD", ref batteriesLCDName);
            config.Get("Battery Rates LCD", ref batteriesRatesLCDName);
            config.Get("Hydrogen LCD", ref tanksH2Name);
            config.Get("Oxygen LCD", ref tanksO2Name);
            config.Get("Image Prefix", ref imagePrefix);
            config.Save();

            console = ConsoleSurface.EasyConsole(this, "General Monitor", configSection);
            console.ClearScreen();
            ConsoleSurface.EchoFunc Echo = console.GetEcho();

            Echo("General Monitor");

            #region mdk macros
            Echo("Version: $MDK_DATETIME$");
            #endregion

            MultiSurface batteriesPercentScreens = GetBlocks.MultiSurfaceByName(batteriesLCDName, configSection);
            MultiSurface batteriesRateScreens = GetBlocks.MultiSurfaceByName(batteriesRatesLCDName, configSection);
            MultiSurface tankH2PercentScreens = GetBlocks.MultiSurfaceByName(tanksH2Name, configSection);
            MultiSurface tankO2PercentScreens = GetBlocks.MultiSurfaceByName(tanksO2Name, configSection);
            batteriesPercentScreens.ClearOnWrite();
            batteriesRateScreens.ClearOnWrite();
            tankH2PercentScreens.ClearOnWrite();
            tankO2PercentScreens.ClearOnWrite();

            List<IMyBatteryBlock> batteries = GetBlocks.ByType<IMyBatteryBlock>();
            List<IMyGasTank> hydrogentanks = GetBlocks.ByType<IMyGasTank>(b => b.IsSameConstructAs(Me) && b.DetailedInfo.Contains("Type: Hydrogen"));
            List<IMyGasTank> oxygentanks = GetBlocks.ByType<IMyGasTank>(b => b.IsSameConstructAs(Me) && b.DetailedInfo.Contains("Type: Oxygen"));

            ProcessBatteries(batteriesPercentScreens, batteriesRateScreens, batteries);
            TanksPercent("H2 Tanks", tankH2PercentScreens, hydrogentanks);
            TanksPercent("O2 Tanks", tankO2PercentScreens, oxygentanks);
        }
    }
}