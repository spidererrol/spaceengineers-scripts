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

namespace IngameScript {
    partial class Program {
        public static class Utility {
            private const string MULTIPLIERS = ".kMGTPEZY";
            private static readonly System.Text.RegularExpressions.Regex reHUnit = new System.Text.RegularExpressions.Regex(@"^(\d*\.\d+|\d+)\s*([kMGTPEZY])(.*)$");
            private static readonly System.Text.RegularExpressions.Regex reJumpDriveMaxPower = new System.Text.RegularExpressions.Regex("Max Stored Power: (\\d+\\.?\\d*) (\\w?)Wh", System.Text.RegularExpressions.RegexOptions.Singleline);
            private static readonly System.Text.RegularExpressions.Regex reJumpDriveCurPower = new System.Text.RegularExpressions.Regex("Stored power: (\\d+\\.?\\d*) (\\w?)Wh", System.Text.RegularExpressions.RegexOptions.Singleline);

            public class Options {
                internal Dictionary<string, string> optarg;
                internal Dictionary<string, bool> optbool;
                internal List<string> args;
                public Options() {
                    optarg = new Dictionary<string, string>();
                    optbool = new Dictionary<string, bool>();
                    args = new List<string>();
                }

                public string Option(string key, string def) => optarg.ContainsKey(key) ? optarg[key] : def;
                public bool Option(string key, bool def) => optbool.ContainsKey(key) ? optbool[key] : def;
                public List<string> Arg => args;
            }

            public class OptParse {
                public Options optionsObj;
                public string pending;
                public OptParse(string arguments) {
                    pending = arguments;
                    optionsObj = new Options();
                }

                public int Length => pending.Length;
                public Dictionary<string, string> optarg => optionsObj.optarg;
                public Dictionary<string, bool> optbool => optionsObj.optbool;
                public List<string> args => optionsObj.args;

                public void AddOption(string key, string value) => optarg.Add(key, value);
                public void AddOption(string key, bool value) => optbool.Add(key, value);
                public void AddArg(string arg) => args.Add(arg);

                public bool DoMatch(System.Text.RegularExpressions.Regex regex, Action<System.Text.RegularExpressions.Match> action) {
                    if (!regex.IsMatch(pending))
                        return false;
                    System.Text.RegularExpressions.Match match = regex.Match(pending);
                    pending = regex.Replace(pending, "");
                    action(match);
                    return true;
                }

                public void Remove(System.Text.RegularExpressions.Regex regex) => pending = regex.Replace(pending, "");
                public bool RequireRemove(System.Text.RegularExpressions.Regex regex) {
                    if (!regex.IsMatch(pending))
                        return false;
                    Remove(regex);
                    return true;
                }

            }

            private static readonly System.Text.RegularExpressions.Regex reSpace = new System.Text.RegularExpressions.Regex(@"^\s+");
            private static readonly System.Text.RegularExpressions.Regex reOptArgQ = new System.Text.RegularExpressions.Regex(@"^--([\w-]+)=" + "\"" + @"(.*)" + "\"");
            private static readonly System.Text.RegularExpressions.Regex reOptArg = new System.Text.RegularExpressions.Regex(@"^--([\w-]+)=(.*)");
            private static readonly System.Text.RegularExpressions.Regex reNotBoolOpt = new System.Text.RegularExpressions.Regex(@"^--no-([\w-]+)\s");
            private static readonly System.Text.RegularExpressions.Regex reBoolOpt = new System.Text.RegularExpressions.Regex(@"^--([\w-]+)\s");
            private static readonly System.Text.RegularExpressions.Regex reArgQ = new System.Text.RegularExpressions.Regex("^\"([^\"]+)\"");
            private static readonly System.Text.RegularExpressions.Regex reArg = new System.Text.RegularExpressions.Regex(@"^(\S+)");

            public static string get(System.Text.RegularExpressions.Match match, int n) => match.Groups[n].Captures[0].Value;

            public static Options ParseArgs(string arguments) {
                OptParse parser = new OptParse(arguments);
                bool first = true;
                while (parser.Length > 0) {
                    if (first) {
                        first = false;
                    } else {
                        if (!parser.RequireRemove(reSpace)) {
                            throw new Exception("Expecting space near '" + parser.pending + "' with length " + parser.Length);
                        }
                    }

                    if (parser.DoMatch(reOptArgQ, match => parser.AddOption(get(match, 0), get(match, 1))))
                        continue;
                    if (parser.DoMatch(reOptArg, match => parser.AddOption(get(match, 0), get(match, 1))))
                        continue;
                    if (parser.DoMatch(reNotBoolOpt, match => parser.AddOption(get(match, 0), false)))
                        continue;
                    if (parser.DoMatch(reBoolOpt, match => parser.AddOption(get(match, 0), true)))
                        continue;
                    if (parser.DoMatch(reArgQ, match => parser.AddArg(get(match, 0))))
                        continue;
                    if (parser.DoMatch(reArg, match => parser.AddArg(get(match, 0))))
                        continue;
                    throw new Exception("I don't know what to do with " + parser.pending);
                }
                return parser.optionsObj;
            }

            public static void UpdateDict<keyType, valType>(IDictionary<keyType, valType> dict, keyType key, valType val) {
                if (dict.ContainsKey(key))
                    dict[key] = val;
                else
                    dict.Add(key, val);
            }

            public static double HUnitToDouble(string hunit, bool brokenSi = false) {
                System.Text.RegularExpressions.Regex re;
                if (brokenSi) re = new System.Text.RegularExpressions.Regex(reHUnit.ToString(), System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                else re = reHUnit;
                System.Text.RegularExpressions.Match match = re.Match(hunit);
                if (match.Success) {
                    double parsedDouble;
                    if (double.TryParse(match.Groups[1].Value, out parsedDouble)) {
                        return parsedDouble * Math.Pow(1000.0, MULTIPLIERS.IndexOf(match.Groups[2].Value));
                    } else {
                        throw new Exception("Cannot parse '" + match.Groups[1].Value + "' as a double in " + hunit);
                    }
                } else {
                    double parsedDouble;
                    if (double.TryParse(hunit, out parsedDouble)) {
                        return parsedDouble;
                    } else {
                        throw new Exception("Cannot parse '" + hunit + "' as a double");
                    }
                }
            }

            public static string DoubleToHUnit(double value, double demultiplier = 1, string format = "N", string seperator = "") {
                string si = MULTIPLIERS;
                while (si.Length > 1) {
                    double mul = Math.Pow(1000.0, si.Length - 1);
                    if (value >= mul) {
                        return (value / mul).ToString(format) + seperator + si.Last().ToString();
                    }
                    si = si.Substring(0, si.Length - 1);
                }
                return value.ToString(format);
            }

            /// <summary>
            /// Returns the percentage charge the specified jump drive.
            /// </summary>
            /// <param name="drive">Jump drive to calculate</param>
            /// <returns>Percentage of charge in the jump drive</returns>
            public static double JumpDriveChargePercent(IMyJumpDrive drive) {
                double maxPower = 0;
                double curPower = 0;
                System.Text.RegularExpressions.Match match = reJumpDriveMaxPower.Match(drive.DetailedInfo);
                double parsedDouble = 0.0;
                if (match.Success) {
                    if (Double.TryParse(match.Groups[1].Value, out parsedDouble)) {
                        maxPower = parsedDouble * Math.Pow(1000.0, MULTIPLIERS.IndexOf(match.Groups[2].Value));
                    }
                }

                match = reJumpDriveCurPower.Match(drive.DetailedInfo);
                if (match.Success) {
                    if (Double.TryParse(match.Groups[1].Value, out parsedDouble)) {
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
            public static void RunActions<MyType>(List<MyType> blocks, string action, Func<MyType, bool> filter = null) where MyType : IMyTerminalBlock {
                for (int i = 0; i < blocks.Count; i++) {
                    if (filter == null || filter.Invoke(blocks[i])) {
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
            public static void RunActions(IMyTerminalBlock block, string action) {
                ITerminalAction act = block.GetActionWithName(action);
                act.Apply(block);
            }

            public static IType TryType<IType>(IMyCubeBlock item) {
                try {
                    return (IType)item;
                } catch {
                    return default(IType);
                }
            }

            public static bool IsType<IType>(IMyCubeBlock item) {
                if (TryType<IType>(item) == null)
                    return false;
                else
                    return true;
            }

            public static string List2String<T>(IList<T> list) {
                string ret = "";
                for (int i = 0; i < list.Count; i++) {
                    string item = list[i].ToString();
                    if (i > 0)
                        ret += ", ";
                    ret += item;
                }
                return ret;
            }

            public static string List2String<T>(T[] list) {
                return List2String(new List<T>(list));
            }
        }
    }
}
