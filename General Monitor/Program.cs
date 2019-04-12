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
        string lcdName = "LCD Battery";
        string lcd2Name = "Battery Rates LCD";
        string lcdH2 = "LCD H2";
        string lcdO2 = "LCD O2";

        // The Image set to use.           
        string imagePrefix = "Percent ";

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
        }

        int percentScreen(List<IMyTextPanel> screens, float pct, int lastIndex = -1)
        {
            if (screens.Any())
            {
                int index = (int)pct;

                if (index > 100 || index < 0)
                    return -1; //error          
                if (index == lastIndex)
                    return index;

                string imagename = imagePrefix + index.ToString("000");
                foreach (IMyTextPanel lcd in screens)
                {
                    lcd.ContentType = ContentType.TEXT_AND_IMAGE;
                    if (imagename != lcd.CurrentlyShownImage)
                    {
                        lcd.AddImageToSelection(imagename);
                        lcd.RemoveImageFromSelection(lcd.CurrentlyShownImage);
                    }
                    if (lcd.CurrentlyShownImage == null)
                    {
                        lcd.AddImageToSelection(imagename);
                    }
                }
                return index;
            }
            return lastIndex;
        }

        void rateScreen(List<IMyTextPanel> screens, float diff)
        {
            if (screens.Any())
            {
                foreach (IMyTextPanel lcd in screens)
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

        void calcTanks(List<IMyTextPanel> screens, List<IMyGasTank> tanks)
        {
            double fill = 0.0f;
            int count = 0;
            foreach (IMyGasTank tank in tanks)
            {
                fill += tank.FilledRatio;
                count++;
            }
            fill /= count;

            percentScreen(screens, (float)(100.0f * fill));
        }

        void Main(string arg)
        {

            MyIni config = getConfig(Me);
            lcdName = getConfig(config, configSection, "Battery LCD", lcdName);
            lcd2Name = getConfig(config, configSection, "Battery Rates LCD", lcd2Name);
            lcdH2 = getConfig(config, configSection, "Hydrogen LCD", lcdH2);
            lcdO2 = getConfig(config, configSection, "Oxygen LCD", lcdO2);
            imagePrefix = getConfig(config, configSection, "Image Prefix", imagePrefix);
            Me.CustomData = config.ToString();

            List<IMyTextPanel> screens = getObjectsByName<IMyTextPanel>(lcdName);
            List<IMyTextPanel> screens2 = getObjectsByName<IMyTextPanel>(lcd2Name);
            List<IMyTextPanel> screensH2 = getObjectsByName<IMyTextPanel>(lcdH2);
            List<IMyTextPanel> screensO2 = getObjectsByName<IMyTextPanel>(lcdO2);

            // No shorthand methods for these:
            List<IMyBatteryBlock> batteries = new List<IMyBatteryBlock>();
            List<IMyGasTank> hydrogentanks = new List<IMyGasTank>();
            List<IMyGasTank> oxygentanks = new List<IMyGasTank>();
            GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(batteries, b => b.IsSameConstructAs(Me));
            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(hydrogentanks, b => b.IsSameConstructAs(Me) && b.DetailedInfo.Contains("Type: Hydrogen"));
            GridTerminalSystem.GetBlocksOfType<IMyGasTank>(oxygentanks, b => b.IsSameConstructAs(Me) && b.DetailedInfo.Contains("Type: Oxygen"));

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

            percentScreen(screens, 100.0f * curPower / maxPower);
            rateScreen(screens2, curIn - curOut);
            calcTanks(screensH2, hydrogentanks);
            calcTanks(screensO2, oxygentanks);

            screens.Clear();
            screens2.Clear();
            screensH2.Clear();
            screensO2.Clear();
            batteries.Clear();
            hydrogentanks.Clear();
        }
    }
}