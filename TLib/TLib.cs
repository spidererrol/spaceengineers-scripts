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
        System.Text.RegularExpressions.Regex reJumpDriveMaxPower = new System.Text.RegularExpressions.Regex(
        "Max Stored Power: (\\d+\\.?\\d*) (\\w?)Wh",
        System.Text.RegularExpressions.RegexOptions.Singleline);
        System.Text.RegularExpressions.Regex reJumpDriveCurPower = new System.Text.RegularExpressions.Regex(
            "Stored power: (\\d+\\.?\\d*) (\\w?)Wh",
        System.Text.RegularExpressions.RegexOptions.Singleline);

  
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

        List<IType> getObjectsByName<IType>(string match, bool thisgrid = true)
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
                    //Debug("++ " + hits[i].CustomName);    
                    if (thing == null)
                        continue;
                    //if (thisgrid && ( thing as IMyTerminalBlock ).CubeGrid != Me.CubeGrid)
                    //    continue;
                    ret.Add(thing);
                }
                catch
                {
                    //Debug("-- " + hits[i].CustomName);    
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

        IType getObjectByName<IType>(string match, bool thisgrid = true) where IType : IMyTerminalBlock
        {
            List<IType> hits = getObjectsByName<IType>(match, thisgrid);
            if (hits.Count == 0)
                return default(IType);
            return hits[0];
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
