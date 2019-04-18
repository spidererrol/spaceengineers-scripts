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
        // TLib.cs - Legacy function names. Everything here has been moved into classes to ease incorporating just the bits you need.

        // Legacy aliases for functions now in Utility:
        public void UpdateDict<keyType, valType>(IDictionary<keyType, valType> dict, keyType key, valType val) => Utility.UpdateDict<keyType, valType>(dict, key, val);
        public double HUnitToDouble(string hunit, bool brokenSi = false) => Utility.HUnitToDouble(hunit, brokenSi);
        public string DoubleToHUnit(double value, double demultiplier = 1, string format = "N") => Utility.DoubleToHUnit(value, demultiplier, format);
        public double JumpDriveChargePercent(IMyJumpDrive drive) => Utility.JumpDriveChargePercent(drive);
        public void RunActions<MyType>(List<MyType> blocks, string action, Func<MyType, bool> filter = null) where MyType : IMyTerminalBlock => Utility.RunActions<MyType>(blocks, action, filter);
        public void RunActions(IMyTerminalBlock block, string action) => Utility.RunActions(block, action);
        public IType tryType<IType>(IMyCubeBlock item) => Utility.TryType<IType>(item);
        public bool isType<IType>(IMyCubeBlock item) => Utility.IsType<IType>(item);
        public string list2String<T>(List<T> list) => Utility.List2String<T>(list);
        public string list2String<T>(T[] list) => Utility.List2String<T>(list);

        // Legacy to functions in GetBlocks:
        List<IType> getBlocksByName<IType>(string match, bool thisgrid = true) => GetBlocks.ByName<IType>(match, thisgrid);
        List<IType> GetBlocksOfType<IType>(GetBlocksClass.BlockFilter blockFilter) where IType : IMyTerminalBlock => GetBlocks.ByType<IType>(blockFilter);
        List<IType> GetBlocksOfType<IType>(bool thisgrid = true) where IType : IMyTerminalBlock => GetBlocks.ByType<IType>(thisgrid);
        List<IMyBlockGroup> findGroups(string groupname) => GetBlocks.GroupsByName(groupname);
        List<IType> FindBlocksOfGroup<IType>(string groupname, bool thisgrid = true) => GetBlocks.OfGroups<IType>(groupname, thisgrid);
        List<IType> GetBlocksOfGroup<IType>(string groupname, bool thisgrid = true) => GetBlocks.OfGroup<IType>(groupname, thisgrid);
        IType getBlockByName<IType>(string match, bool thisgrid = true) where IType : IMyTerminalBlock => GetBlocks.FirstByName<IType>(match, thisgrid);
        List<IMyThrust> GetAllThrusters(bool thisgrid = true) => GetBlocks.AllThrusters(thisgrid);
        List<IMyGasTank> GetAllTanks(string gastype = null, bool thisgrid = true) => GetBlocks.AllTanks(gastype, thisgrid);
        List<IMyJumpDrive> GetAllJumpDrives(bool thisgrid = true) => GetBlocks.AllJumpDrives(thisgrid);
        public List<IMyBatteryBlock> GetAllBatteries(bool thisgrid = true) => GetBlocks.AllBatteries(thisgrid);
        public List<IMyLandingGear> GetAllLandingGears(bool thisgrid = true) => GetBlocks.AllLandingGears(thisgrid);
        public List<IMyTerminalBlock> OfGroup(string groupname, bool thisgrid = true) => GetBlocks.OfGroup<IMyTerminalBlock>(groupname, thisgrid);
        public List<IMyOxygenFarm> GetAllFarms(bool thisgrid = true) => GetBlocks.AllFarms(thisgrid);
        public List<IMyGasGenerator> GetAllGenerators(bool thisgrid = true) => GetBlocks.AllGenerators(thisgrid);

        // Legacy to functions in GetBlocks (with MultiSurface):
        MultiSurface GetMultiSurfaceByName(string match, string configSection, bool thisgrid = true, MultiSurface.ISurfaceFilter surfaceFilter = null) => GetBlocks.MultiSurfaceByName(match, configSection, thisgrid, surfaceFilter);

        // Legacy to functions in GetBlocks (with Utility):
        public void ActionGroups(string[] grouplist, string actionname) => GetBlocks.ActionGroups<IMyTerminalBlock>(grouplist, actionname);
        public void ActionGroups<IType>(string[] grouplist, string actionname) where IType : IMyTerminalBlock => GetBlocks.ActionGroups<IType>(grouplist, actionname);
        public void ActionGroups(string onegroup, string actionname) => GetBlocks.ActionGroups<IMyTerminalBlock>(onegroup, actionname);
        public void ActionGroups<IType>(string onegroup, string actionname) where IType : IMyTerminalBlock => GetBlocks.ActionGroups<IType>(onegroup, actionname);

        // This won't really work anywhere as-is, but there is a new method ConsoleSurface.DumpPA(block).
        public void dumpPA(IMyTerminalBlock block)
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



        #endregion TLib.cs
    }
}
