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
        #region TLib.cs
        const string MULTIPLIERS = ".kMGTPEZY";
        readonly System.Text.RegularExpressions.Regex reJumpDriveMaxPower = new System.Text.RegularExpressions.Regex("Max Stored Power: (\\d+\\.?\\d*) (\\w?)Wh", System.Text.RegularExpressions.RegexOptions.Singleline);
        readonly System.Text.RegularExpressions.Regex reJumpDriveCurPower = new System.Text.RegularExpressions.Regex("Stored power: (\\d+\\.?\\d*) (\\w?)Wh", System.Text.RegularExpressions.RegexOptions.Singleline);
        readonly System.Text.RegularExpressions.Regex reHUnit = new System.Text.RegularExpressions.Regex(@"^(\d*\.\d+|\d+)([kMGTPEZY])(.*)$");

        public void UpdateDict<keyType, valType>(IDictionary<keyType, valType> dict, keyType key, valType val)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = val;
            }
            else
            {
                dict.Add(key, val);
            }
        }

        List<IType> getBlocksByName<IType>(string match, bool thisgrid = true)
        {
            List<IMyTerminalBlock> hits = new List<IMyTerminalBlock>();
            if (match == "")
                throw new Exception("You mustn't call getObjectsByName() with an empty string!");
            if (thisgrid)
                GridTerminalSystem.SearchBlocksOfName(match, hits, myGridOnly);
            else
                GridTerminalSystem.SearchBlocksOfName(match, hits);
            List<IType> ret = new List<IType>();
            for (int i = 0; i < hits.Count; i++)
            {
                IType thing;
                try
                {
                    thing = (IType)hits[i];
                    if (thing == null)
                        continue;
                    ret.Add(thing);
                }
                catch
                {
                    continue;
                }
            }
            return ret;
        }

        public delegate bool BlockFilter(IMyTerminalBlock block);

        List<IType> GetBlocksOfType<IType>(BlockFilter blockFilter = null) where IType : IMyTerminalBlock
        {
            List<IMyTerminalBlock> hits = new List<IMyTerminalBlock>();
            if (blockFilter == null)
                blockFilter = myGridOnly;
            GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(hits, block => blockFilter(block));
            List<IType> ret = new List<IType>();
            for (int i = 0; i < hits.Count; i++)
            {
                IType thing;
                try
                {
                    thing = (IType)hits[i];
                    if (thing == null)
                        continue;
                    ret.Add(thing);
                }
                catch
                {
                    continue;
                }
            }
            return ret;

        }

        List<IMyBlockGroup> findGroups(string groupname)
        {
            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
            GridTerminalSystem.GetBlockGroups(groups);
            return groups.FindAll(delegate (IMyBlockGroup g)
            {
                return g.Name.Contains(groupname);
            });
        }

        List<IType> FindBlocksOfGroup<IType>(string groupname, bool thisgrid = true)
        {
            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
            GridTerminalSystem.GetBlockGroups(groups);
            groups = groups.FindAll(delegate (IMyBlockGroup g)
            {
                return g.Name.Contains(groupname);
            });
            List<IType> allblocks = new List<IType>();
            for (int i = 0; i < groups.Count; i++)
            {
                IMyBlockGroup group = groups[i];
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                if (group != null)
                    group.GetBlocks(blocks, myGridOnly);
                allblocks.AddList<IType>(blocks.FindAll(delegate (IMyTerminalBlock b)
                {
                    IType output;
                    try
                    {
                        output = (IType)b;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }).ConvertAll<IType>(delegate (IMyTerminalBlock b)
                {
                    IType output;
                    try
                    {
                        output = (IType)b;
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                    return output;
                }));
            }
            return allblocks;
        }

        List<IType> GetBlocksOfGroup<IType>(string groupname, bool thisgrid = true)
        {
            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(groupname);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            if (group != null)
                group.GetBlocks(blocks, myGridOnly);
            return blocks.FindAll(delegate (IMyTerminalBlock b)
            {
                IType output;
                try
                {
                    output = (IType)b;
                    return true;
                }
                catch
                {
                    return false;
                }
            }).ConvertAll<IType>(delegate (IMyTerminalBlock b)
            {
                IType output;
                try
                {
                    output = (IType)b;
                }
                catch (Exception e)
                {
                    throw e;
                }
                return output;
            });
        }

        /// <summary>
        /// Get the first block of a given type with a name that matches <paramref name="match"/>.
        /// </summary>
        /// <typeparam name="IType">Type of block to search for. It must be based on <see cref="IMyTerminalBlock"/></typeparam>
        /// <param name="match">String that must be found in the block name</param>
        /// <param name="thisgrid">If it should be on this grid or not.</param>
        /// <returns></returns>
        IType getBlockByName<IType>(string match, bool thisgrid = true) where IType : IMyTerminalBlock
        {
            List<IType> hits = getBlocksByName<IType>(match, thisgrid);
            if (hits.Count == 0)
                return default(IType);
            return hits[0];
        }

        /// <summary>
        /// Get a MultiSurface which convers all matching surfaces. Note: surfaceFilter actually defaults to the ShowOnScreen config filter.
        /// </summary>
        /// <param name="match">string to match in block names</param>
        /// <param name="configSection">configuration to scan in CustomData of matching blocks</param>
        /// <param name="thisgrid">only match blocks on this connected grid</param>
        /// <param name="surfaceFilter">If null will default to the ShowOnScreen config filter</param>
        /// <returns>a MultiSurface</returns>
        MultiSurface GetMultiSurfaceByName(string match, string configSection, bool thisgrid = true, MultiSurface.ISurfaceFilter surfaceFilter = null)
        {
            List<IMyTextSurfaceProvider> providers = getBlocksByName<IMyTextSurfaceProvider>(match, thisgrid);
            List<IMyTextSurface> surfaces = getBlocksByName<IMyTextSurface>(match, thisgrid);
            if (surfaceFilter == null)
                surfaceFilter = MultiSurface.ShowOnScreenFilter(configSection);
            return new MultiSurface(providers, surfaces, surfaceFilter);
        }

        bool myGridOnly(IMyTerminalBlock block)
        {
            //return block.CubeGrid == Me.CubeGrid;
            return block.IsSameConstructAs(Me);
        }

        List<IMyThrust> GetAllThrusters(bool thisgrid = true)
        {
            List<IMyTerminalBlock> tmp = new List<IMyTerminalBlock>();
            List<IMyThrust> ret = new List<IMyThrust>();
            if (thisgrid)
            {
                GridTerminalSystem.GetBlocksOfType<IMyThrust>(tmp, myGridOnly);
            }
            else
            {
                GridTerminalSystem.GetBlocksOfType<IMyThrust>(tmp);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                ret.Add((IMyThrust)tmp[i]);
            }
            return ret;
        }

        /// <summary>
        /// Retrieve all tanks.
        /// </summary>
        /// <param name="gastype">Limit to the given gas types ("Oxygen"/"Hydrogen"). If Gas type is not null, the PB API Extender is required!</param>
        /// <param name="thisgrid">Limit to the same grid as the programming block</param>
        /// <returns>list of tanks</returns>
        List<IMyGasTank> GetAllTanks(string gastype = null, bool thisgrid = true)
        {
            List<IMyTerminalBlock> tmp = new List<IMyTerminalBlock>();
            List<IMyGasTank> ret = new List<IMyGasTank>();
            if (thisgrid)
            {
                GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tmp, myGridOnly);
            }
            else
            {
                GridTerminalSystem.GetBlocksOfType<IMyGasTank>(tmp);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                if (gastype != null && tmp[i].GetValue<string>("GasType") != gastype)
                    continue;
                ret.Add((IMyGasTank)tmp[i]);
            }
            return ret;
        }

        /// <summary>
        /// Retrieve all Jump Drives.
        /// </summary>
        /// <param name="thisgrid">Limit to the same grid as the programming block</param>
        /// <returns>list of jump drives</returns>
        List<IMyJumpDrive> GetAllJumpDrives(bool thisgrid = true)
        {
            List<IMyTerminalBlock> tmp = new List<IMyTerminalBlock>();
            if (thisgrid)
            {
                GridTerminalSystem.GetBlocksOfType<IMyJumpDrive>(tmp, myGridOnly);
            }
            else
            {
                GridTerminalSystem.GetBlocksOfType<IMyJumpDrive>(tmp);
            }
            return tmp.ConvertAll<IMyJumpDrive>(delegate (IMyTerminalBlock b)
            {
                return (IMyJumpDrive)b;
            });
        }

        public double HUnitToDouble(string hunit, bool brokenSi = false)
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

        public string DoubleToHUnit(double value, double demultiplier = 1, string format = "N")
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
        double JumpDriveChargePercent(IMyJumpDrive drive)
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
        void RunActions<MyType>(List<MyType> blocks, string action, Func<MyType, bool> filter = null) where MyType : IMyTerminalBlock
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
        void RunActions(IMyTerminalBlock block, string action)
        {
            ITerminalAction act = block.GetActionWithName(action);
            act.Apply(block);
        }

        void ActionGroups(string[] grouplist, string actionname)
        {
            for (int i = 0; i < grouplist.Length; i++)
            {
                List<IMyTerminalBlock> blocks = GetBlocksOfGroup(grouplist[i]);
                RunActions(blocks, actionname);
            }
        }

        void ActionGroups(string onegroup, string actionname)
        {
            string[] grouplist = { onegroup };
            ActionGroups(grouplist, actionname);
        }

        List<IMyBatteryBlock> GetAllBatteries(bool thisgrid = true)
        {
            List<IMyTerminalBlock> tmp = new List<IMyTerminalBlock>();
            List<IMyBatteryBlock> ret = new List<IMyBatteryBlock>();
            if (thisgrid)
            {
                GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(tmp, myGridOnly);
            }
            else
            {
                GridTerminalSystem.GetBlocksOfType<IMyBatteryBlock>(tmp);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                ret.Add((IMyBatteryBlock)tmp[i]);
            }
            return ret;
        }

        List<IMyLandingGear> GetAllLandingGears(bool thisgrid = true)
        {
            List<IMyTerminalBlock> tmp = new List<IMyTerminalBlock>();
            List<IMyLandingGear> ret = new List<IMyLandingGear>();
            if (thisgrid)
            {
                GridTerminalSystem.GetBlocksOfType<IMyLandingGear>(tmp, myGridOnly);
            }
            else
            {
                GridTerminalSystem.GetBlocksOfType<IMyLandingGear>(tmp);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                ret.Add((IMyLandingGear)tmp[i]);
            }
            return ret;
        }

        List<IMyTerminalBlock> GetBlocksOfGroup(string groupname, bool thisgrid = true)
        {
            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(groupname);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            if (group != null)
                group.GetBlocks(blocks, myGridOnly);
            return blocks;
        }

        List<IMyOxygenFarm> GetAllFarms(bool thisgrid = true)
        {
            List<IMyTerminalBlock> tmp = new List<IMyTerminalBlock>();
            List<IMyOxygenFarm> ret = new List<IMyOxygenFarm>();
            if (thisgrid)
            {
                GridTerminalSystem.GetBlocksOfType<IMyOxygenFarm>(tmp, myGridOnly);
            }
            else
            {
                GridTerminalSystem.GetBlocksOfType<IMyOxygenFarm>(tmp);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                if (tmp[i].BlockDefinition.SubtypeName.Contains("Nanite")) // Nanite Control Factory is an oxygen farm. Don't touch it!
                    continue;
                ret.Add((IMyOxygenFarm)tmp[i]);
            }
            return ret;
        }

        List<IMyGasGenerator> GetAllGenerators(bool thisgrid = true)
        {
            List<IMyTerminalBlock> tmp = new List<IMyTerminalBlock>();
            List<IMyGasGenerator> ret = new List<IMyGasGenerator>();
            if (thisgrid)
            {
                GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(tmp, myGridOnly);
            }
            else
            {
                GridTerminalSystem.GetBlocksOfType<IMyGasGenerator>(tmp);
            }
            for (int i = 0; i < tmp.Count; i++)
            {
                ret.Add((IMyGasGenerator)tmp[i]);
            }
            return ret;
        }

        IType tryType<IType>(IMyCubeBlock item)
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

        bool isType<IType>(IMyCubeBlock item)
        {
            if (tryType<IType>(item) == null)
                return false;
            else
                return true;
        }


        void dumpPA(IMyTerminalBlock block)
        {
            List<ITerminalProperty> props = new List<ITerminalProperty>();
            List<ITerminalAction> acts = new List<ITerminalAction>();
            block.GetProperties(props);
            block.GetActions(acts);
            Echo("Block: " + block.CustomName);
            Echo("Is A:" + block.BlockDefinition.TypeIdString + "." + block.BlockDefinition.SubtypeName);
            Echo("Properties:");
            for (int i = 0; i < props.Count; i++)
            {
                Echo(" " + props[i].TypeName + " " + props[i].Id);
            }
            Echo("Actions:");
            for (int i = 0; i < acts.Count; i++)
            {
                Echo(" " + acts[i].Id);
            }
            Echo("----");
        }

        string list2String<T>(List<T> list)
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

        string list2String<T>(T[] list)
        {
            return list2String(new List<T>(list));
        }
        #endregion TLib.cs
    }
}
