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
        public partial class GetBlocksClass
        {
            private readonly IDictionary<string, object> propCache;
            private readonly IMyGridTerminalSystem GridTerminalSystem;
            private readonly IMyTerminalBlock Me;
            private GetBlocksClass(IMyGridTerminalSystem gts, IMyTerminalBlock refblock)
            {
                GridTerminalSystem = gts;
                Me = refblock;
                propCache = new Dictionary<string, object>();
            }
            public GetBlocksClass(Program program) : this(program.GridTerminalSystem, program.Me) { }

            public void ClearCache() => propCache.Clear();

            public GetBlocksClass OtherGrid(IMyTerminalBlock refblock) => new GetBlocksClass(GridTerminalSystem, refblock);

            public delegate bool BlockFilter(IMyTerminalBlock block);
            private BlockFilter UseMyGridFilter(bool thisgrid) => thisgrid ? myGridOnly : default(BlockFilter);

            public List<IType> ByName<IType>(string match, bool thisgrid = true) => ByName<IType>(match, UseMyGridFilter(thisgrid));
            public List<IType> ByName<IType>(string match, BlockFilter blockFilter)
            {
                List<IMyTerminalBlock> hits = new List<IMyTerminalBlock>();
                if (match == "")
                    throw new Exception("You mustn't call GetBlocks.ByName() with an empty string!");
                if (blockFilter != null)
                    GridTerminalSystem.SearchBlocksOfName(match, hits, b => blockFilter(b));
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

            public List<IMyTerminalBlock> GetAllBlocks()
            {
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocks(blocks);
                return blocks;
            }

            public List<IMyTerminalBlock> GetAllThisGrid()
            {
                List<IMyTerminalBlock> hits = new List<IMyTerminalBlock>();
                GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(hits, myGridOnly);
                return hits;
            }

            private delegate object cacheGetter(string cacheid);
            private IType Cached<IType>(string cacheid, cacheGetter get)
            {
                if (!propCache.ContainsKey(cacheid))
                    propCache.Add(cacheid, get(cacheid));
                return (IType)propCache[cacheid];
            }
            private IList<IType> CachedList<IType>(string cacheid, cacheGetter get) => Cached<List<IType>>(cacheid, get);

            public IList<IMyTerminalBlock> Everything => CachedList<IMyTerminalBlock>("Everything", id => GetAllBlocks());
            public IList<IMyTerminalBlock> EverythingThisGrid => CachedList<IMyTerminalBlock>("EverythingThisGrid", id => GetAllThisGrid());

            public List<IType> ByType<IType>(bool thisgrid = true) where IType : IMyTerminalBlock => ByType<IType>(UseMyGridFilter(thisgrid));
            public List<IType> ByType<IType>(BlockFilter blockFilter) where IType : IMyTerminalBlock
            {
                List<IMyTerminalBlock> hits = new List<IMyTerminalBlock>();
                if (blockFilter == null)
                    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(hits);
                else
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

            public List<IMyBlockGroup> GroupsByName(string groupname)
            {
                List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
                GridTerminalSystem.GetBlockGroups(groups);
                return groups.FindAll(delegate (IMyBlockGroup g)
                {
                    return g.Name.Contains(groupname);
                });
            }

            /// <summary>
            /// This will find all blocks of specified type in multiple groups with groupname.
            /// </summary>
            /// <typeparam name="IType">Type of the returned blocks</typeparam>
            /// <param name="groupname">String which must be contained in the name of matching groups</param>
            /// <param name="thisgrid">If blocks should be on this grid.</param>
            /// <returns></returns>
            public List<IType> OfGroups<IType>(string groupname, bool thisgrid = true)
            {
                List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
                GridTerminalSystem.GetBlockGroups(groups);
                groups = groups.FindAll(g => g.Name.Contains(groupname));
                List<IType> allblocks = new List<IType>();
                foreach (IMyBlockGroup group in groups)
                {
                    if (group == null)
                        continue;
                    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                    if (thisgrid)
                        group.GetBlocks(blocks, myGridOnly);
                    else
                        group.GetBlocks(blocks);
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
                    }).ConvertAll<IType>(b => (IType)b));
                }
                return allblocks;
            }

            /// <summary>
            /// Return all blocks of specified type that are in the specified group.
            /// </summary>
            /// <typeparam name="IType">Type to limit to</typeparam>
            /// <param name="groupname">Name of group to search</param>
            /// <param name="thisgrid">Only search the current grid</param>
            /// <returns>List of matching blocks</returns>
            public List<IType> OfGroup<IType>(string groupname, bool thisgrid = true)
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
            public IType FirstByName<IType>(string match, bool thisgrid = true) where IType : IMyTerminalBlock => FirstByName<IType>(match, UseMyGridFilter(thisgrid));
            public IType FirstByName<IType>(string match, BlockFilter blockFilter) where IType : IMyTerminalBlock
            {
                List<IType> hits = ByName<IType>(match, blockFilter);
                if (hits.Count == 0)
                    return default(IType);
                return hits.First();
            }
            public IType FirstByType<IType>(bool thisgrid = true) where IType : IMyTerminalBlock => FirstByType<IType>(UseMyGridFilter(thisgrid));
            public IType FirstByType<IType>(BlockFilter blockFilter) where IType : IMyTerminalBlock {
                List<IType> hits = ByType<IType>(blockFilter);
                if (hits.Count == 0)
                    return default(IType);
                return hits.First();
            }
            // Don't use this - there is no control over which grid nor the type so potential for conflicts are high:
            //public IType ExactByName<IType>(string name) => (IType)GridTerminalSystem.GetBlockWithName(name);

            bool myGridOnly(IMyTerminalBlock block)
            {
                //return block.CubeGrid == Me.CubeGrid;
                return block.IsSameConstructAs(Me);
            }


            /// <summary>
            /// Retrieve all tanks.
            /// </summary>
            /// <param name="gastype">Limit to the given gas types ("Oxygen"/"Hydrogen").</param>
            /// <param name="thisgrid">Limit to the same grid as the programming block</param>
            /// <returns>list of tanks</returns>
            public List<IMyGasTank> AllTanks(string gastype = null, bool thisgrid = true)
            {
                if (gastype == null)
                    return ByType<IMyGasTank>(thisgrid);
                return ByType<IMyGasTank>(thisgrid).FindAll(g => g.DetailedInfo.Contains("Type: " + gastype));
            }

            /// <summary>
            /// Retrieve all Jump Drives.
            /// </summary>
            /// <param name="thisgrid">Limit to the same grid as the programming block</param>
            /// <returns>list of jump drives</returns>
            public List<IMyJumpDrive> AllJumpDrives(bool thisgrid = true) => ByType<IMyJumpDrive>(thisgrid);
            public List<IMyThrust> AllThrusters(bool thisgrid = true) => ByType<IMyThrust>(thisgrid);
            public List<IMyBatteryBlock> AllBatteries(bool thisgrid = true) => ByType<IMyBatteryBlock>(thisgrid);
            public List<IMyLandingGear> AllLandingGears(bool thisgrid = true) => ByType<IMyLandingGear>(thisgrid);
            public List<IMyGasGenerator> AllGenerators(bool thisgrid = true) => ByType<IMyGasGenerator>(thisgrid);

            /// <summary>
            /// Get All Oxygen Farms but excludes the Nanite facility which is secretly an Oxygen Farm (for some reason).
            /// </summary>
            /// <param name="thisgrid">Limit to current grid</param>
            /// <returns>List of Oxygen Farms</returns>
            public List<IMyOxygenFarm> AllFarms(bool thisgrid = true) => ByType<IMyOxygenFarm>(thisgrid).FindAll(f => !f.BlockDefinition.SubtypeName.Contains("Nanite"));

        }
    }
}
