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
    partial class Program
    {
        public static class Utility
        {
            private const string MULTIPLIERS = ".kMGTPEZY";
            private static readonly System.Text.RegularExpressions.Regex reHUnit = new System.Text.RegularExpressions.Regex(@"^(\d*\.\d+|\d+)([kMGTPEZY])(.*)$");
            private static readonly System.Text.RegularExpressions.Regex reJumpDriveMaxPower = new System.Text.RegularExpressions.Regex("Max Stored Power: (\\d+\\.?\\d*) (\\w?)Wh", System.Text.RegularExpressions.RegexOptions.Singleline);
            private static readonly System.Text.RegularExpressions.Regex reJumpDriveCurPower = new System.Text.RegularExpressions.Regex("Stored power: (\\d+\\.?\\d*) (\\w?)Wh", System.Text.RegularExpressions.RegexOptions.Singleline);

            public static void UpdateDict<keyType, valType>(IDictionary<keyType, valType> dict, keyType key, valType val)
            {
                if (dict.ContainsKey(key))
                    dict[key] = val;
                else
                    dict.Add(key, val);
            }

            public static double HUnitToDouble(string hunit, bool brokenSi = false)
            {
                System.Text.RegularExpressions.Regex re;
                if (brokenSi) re = new System.Text.RegularExpressions.Regex(reHUnit.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                else re = reHUnit;
                System.Text.RegularExpressions.Match match = re.Match(hunit);
                if (match.Success)
                {
                    double parsedDouble = Double.Parse(match.Groups[1].Value);
                    return parsedDouble * Math.Pow(1000.0, MULTIPLIERS.IndexOf(match.Groups[2].Value));
                }
                else
                {
                    return double.Parse(hunit);
                }
            }

            public static string DoubleToHUnit(double value, double demultiplier = 1, string format = "N")
            {
                string si = MULTIPLIERS;
                while (si.Length > 1)
                {
                    double mul = Math.Pow(1000.0, si.Length - 1);
                    if (value >= mul)
                    {
                        return (value / mul).ToString(format) + si.Last().ToString();
                    }
                }
                return value.ToString(format);
            }

            /// <summary>
            /// Returns the percentage charge the specified jump drive.
            /// </summary>
            /// <param name="drive">Jump drive to calculate</param>
            /// <returns>Percentage of charge in the jump drive</returns>
            public static double JumpDriveChargePercent(IMyJumpDrive drive)
            {
                double maxPower = 0;
                double curPower = 0;
                System.Text.RegularExpressions.Match match = reJumpDriveMaxPower.Match(drive.DetailedInfo);
                double parsedDouble = 0.0;
                if (match.Success)
                {
                    if (Double.TryParse(match.Groups[1].Value, out parsedDouble))
                    {
                        maxPower = parsedDouble * Math.Pow(1000.0, MULTIPLIERS.IndexOf(match.Groups[2].Value));
                    }
                }

                match = reJumpDriveCurPower.Match(drive.DetailedInfo);
                if (match.Success)
                {
                    if (Double.TryParse(match.Groups[1].Value, out parsedDouble))
                    {
                        curPower = parsedDouble * Math.Pow(1000.0, MULTIPLIERS.IndexOf(match.Groups[2].Value));
                    }
                }
                return ((curPower / maxPower) * 100);
            }

            /// <summary>
            /// Run the action on all blocks on the list which match the filter.
            /// </summary>
            /// <typeparam name="MyType">An IMyTerminalBlock sub-type (implied by blocks).</typeparam>
            /// <param name="blocks">List of blocks to apply action to.</param>
            /// <param name="action">String name of the action.</param>
            /// <param name="filter">Filter function.</param>
            public static void RunActions<MyType>(List<MyType> blocks, string action, Func<MyType, bool> filter = null) where MyType : IMyTerminalBlock
            {
                for (int i = 0; i < blocks.Count; i++)
                {
                    if (filter == null || filter.Invoke(blocks[i]))
                    {
                        ITerminalAction off = blocks[i].GetActionWithName(action);
                        off.Apply(blocks[i]);
                    }
                }
            }

            /// <summary>
            /// Run action on the given block.
            /// </summary>
            /// <param name="block">The block to act on.</param>
            /// <param name="action">String name of action to apply.</param>
            public static void RunActions(IMyTerminalBlock block, string action)
            {
                ITerminalAction act = block.GetActionWithName(action);
                act.Apply(block);
            }

            public static IType TryType<IType>(IMyCubeBlock item)
            {
                try
                {
                    return (IType)item;
                }
                catch
                {
                    return default(IType);
                }
            }

            public static bool IsType<IType>(IMyCubeBlock item)
            {
                if (TryType<IType>(item) == null)
                    return false;
                else
                    return true;
            }

            public static string List2String<T>(List<T> list)
            {
                string ret = "";
                for (int i = 0; i < list.Count; i++)
                {
                    string item = list[i].ToString();
                    if (i > 0)
                        ret += ", ";
                    ret += item;
                }
                return ret;
            }

            public static string List2String<T>(T[] list)
            {
                return List2String(new List<T>(list));
            }


        }
    }
}
