using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;
using DaggerfallConnect;
using System;
using System.Linq;
using DaggerfallWorkshop.Utility;

public class TestAllLocationsByBuildings
{
    [Test]
    public void LocationsByBuildings()
    {
        // pre-load all locations
        List<Location> locations = new List<Location>();
        MapsFile mapFile = DaggerfallUnity.Instance.ContentReader.MapFileReader;
        DFRegion.LocationTypes[] citiesLocationTypes = new DFRegion.LocationTypes[]
        {
            DFRegion.LocationTypes.HomeFarms,
            DFRegion.LocationTypes.HomePoor,
            DFRegion.LocationTypes.HomeWealthy,
            DFRegion.LocationTypes.Tavern,
            DFRegion.LocationTypes.TownCity,
            DFRegion.LocationTypes.TownHamlet,
            DFRegion.LocationTypes.TownVillage,
        };

        for (int regionId = 0; regionId <= mapFile.RegionCount; ++regionId)
        {
            DFRegion regionData = mapFile.GetRegion(regionId);
            for (int i = 0; i < regionData.LocationCount; ++i)
            {
                DFRegion.RegionMapTable dfMapData = regionData.MapTable[i];
                DFRegion.LocationTypes type = dfMapData.LocationType;
                if (Array.IndexOf(citiesLocationTypes, type) < 0)
                    continue; // skip location if not a city

                DFLocation dfLocation = mapFile.GetLocation(regionId, i);
                Location location = new Location();
                location.Name = regionData.Name + "/" + regionData.MapNames[i];
                location.BldType = dfLocation.Exterior.Buildings.Select(b => b.BuildingType).ToArray();
                location.Sectors = dfLocation.Exterior.Buildings.Select(b => b.Sector).ToArray();

                location.Blocks = new List<BlockInfo>();
                BlocksFile blocksFile = DaggerfallUnity.Instance.ContentReader.BlockFileReader;
                for (int y = 0; y < dfLocation.Exterior.ExteriorData.Height; y++)
                {
                    for (int x = 0; x < dfLocation.Exterior.ExteriorData.Width; x++)
                    {
                        string blockName = mapFile.GetRmbBlockName(ref dfLocation, x, y);
                        string fixedBlockName = blocksFile.CheckName(blockName);
                        DFBlock block = blocksFile.GetBlock(fixedBlockName);
                        if (block.Type != DFBlock.BlockTypes.Rmb)
                            continue;
                        RMBLayout.BuildingSummary[] dfBuildings = RMBLayout.GetBuildingData(block);

                        BlockInfo blockInfo = new BlockInfo();
                        blockInfo.Name = fixedBlockName;
                        blockInfo.BuildingCount = dfBuildings.Length;
                        blockInfo.BldType = dfBuildings.Select(b => b.BuildingType).ToArray();
                        location.Blocks.Add(blockInfo);
                    }
                }
                locations.Add(location);
            }

            // verify buildings mapping for all locations
            foreach (var loc in locations)
            {
                int[] mappedBuildings = MapLocationBuildingsToBlockBuildings(loc);
                int mappedBuildingsCount = mappedBuildings.Count(b => b >= 0);

                string locationBuildingsStr = string.Join("-", loc.BldType.Select(b => ((int)b).ToString("X2")).ToArray());
                string mappedBuildingsStr = string.Join("-", mappedBuildings.Select(b => b < 0 ? "XX" : ((int)loc.BldType[b]).ToString("X2")).ToArray());

                // all location buildings must be mapped to corresponding block buildings
                Assert.AreEqual(loc.BldType.Length, mappedBuildingsCount,
                    loc.Name + "\r\n" +
                    locationBuildingsStr + "\r\n" +
                    mappedBuildingsStr);
            }
        }
    }

    private class BlockInfo
    {
        public string Name;
        public int BuildingCount;
        public DFLocation.BuildingTypes[] BldType;
    }

    private class Location
    {
        public string Name;
        public short[] Sectors;
        public DFLocation.BuildingTypes[] BldType;
        public List<BlockInfo> Blocks;
    }

    static DFLocation.BuildingTypes[] residentials = new DFLocation.BuildingTypes[] 
    {
        DFLocation.BuildingTypes.HouseForSale,
        DFLocation.BuildingTypes.House1,
        DFLocation.BuildingTypes.House2,
        DFLocation.BuildingTypes.House3,
        DFLocation.BuildingTypes.House4,
        DFLocation.BuildingTypes.House5,
        DFLocation.BuildingTypes.House6,
    };

    static bool IsEqualBld(DFLocation.BuildingTypes bld1, DFLocation.BuildingTypes bld2)
    {
        // house for sale (0x01) can redefine regular residences (0x11, 0x12, e.t.c.)
        DFLocation.BuildingTypes c1 = (Array.IndexOf(residentials, bld1) >= 0) ? DFLocation.BuildingTypes.House1 : bld1;
        DFLocation.BuildingTypes c2 = (Array.IndexOf(residentials, bld2) >= 0) ? DFLocation.BuildingTypes.House1 : bld2;
        return c1 == c2;
    }

    private static int[] MapLocationBuildingsToBlockBuildings(Location loc)
    {
        int[] result = new int[loc.Blocks.Sum(b => b.BuildingCount)];
        for (int i = 0; i < result.Length; ++i)
            result[i] = -1;

        int blockIdx = 0;
        int pos = 0;
        for (int i = 0; i < loc.BldType.Length; )
        {
            if (blockIdx >= loc.Blocks.Count)
                break;

            // estimate forward mapping
            int forwardScore = 0;
            int forwardMatchCount = 0;
            {
                int testLocBldIdx = i;
                for (int j = 0; j < loc.Blocks[blockIdx].BuildingCount; ++j)
                {
                    if (testLocBldIdx >= loc.BldType.Length)
                        break;

                    DFLocation.BuildingTypes loc_bld = loc.BldType[testLocBldIdx];
                    DFLocation.BuildingTypes block_bld = loc.Blocks[blockIdx].BldType[j];
                    bool isResidence = Array.IndexOf(residentials, loc_bld) >= 0;
                    if (IsEqualBld(loc_bld, block_bld))
                    {
                        ++testLocBldIdx;
                        ++forwardMatchCount;
                        if (!isResidence)
                            forwardScore += 10000; // not a residence gets extra score
                        else
                        {
                            forwardScore += 100;
                            if (loc_bld == block_bld)
                                forwardScore += 1;
                        }
                    }
                }
            }

            // estimate backward mapping
            int backwardScore = 0;
            int backwardMatchCount = 0;
            {
                int testLocBldIdx = i;
                for (int j = loc.Blocks[blockIdx].BuildingCount - 1; j >= 0; --j)
                {
                    if (testLocBldIdx >= loc.BldType.Length)
                        break;

                    DFLocation.BuildingTypes loc_bld = loc.BldType[testLocBldIdx];
                    DFLocation.BuildingTypes block_bld = loc.Blocks[blockIdx].BldType[j];
                    bool isResidence = Array.IndexOf(residentials, loc_bld) >= 0;
                    if (IsEqualBld(loc_bld, block_bld))
                    {
                        ++testLocBldIdx;
                        ++backwardMatchCount;
                        if (!isResidence)
                            backwardScore += 10000; // not a residence gets extra score
                        else
                        {
                            backwardScore += 100;
                            if (loc_bld == block_bld)
                                backwardScore += 1;
                        }
                    }
                }
            }

            if (forwardScore >= backwardScore)
            {
                // apply forward mapping
                for (int blockBldIdx = 0; blockBldIdx < loc.Blocks[blockIdx].BuildingCount; ++blockBldIdx)
                {
                    if (i >= loc.BldType.Length)
                        break;
                    DFLocation.BuildingTypes loc_bld = loc.BldType[i];
                    DFLocation.BuildingTypes block_bld = loc.Blocks[blockIdx].BldType[blockBldIdx];
                    if (IsEqualBld(loc_bld, block_bld))
                    {
                        result[pos + blockBldIdx] = i;
                        ++i;
                    }
                }
            }
            else
            {
                // apply backward mapping
                for (int blockBldIdx = loc.Blocks[blockIdx].BuildingCount - 1; blockBldIdx >= 0; --blockBldIdx)
                {
                    if (i >= loc.BldType.Length)
                        break;
                    DFLocation.BuildingTypes loc_bld = loc.BldType[i];
                    DFLocation.BuildingTypes block_bld = loc.Blocks[blockIdx].BldType[blockBldIdx];
                    if (IsEqualBld(loc_bld, block_bld))
                    {
                        result[pos + blockBldIdx] = i;
                        ++i;
                    }
                }
            }

            pos += loc.Blocks[blockIdx].BuildingCount;
            ++blockIdx;
        }

        return result;
    }
}
