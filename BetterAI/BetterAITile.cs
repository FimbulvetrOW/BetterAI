using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenCrowns.AppCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using static TenCrowns.GameCore.Text.TextExtensions;
using Constants = TenCrowns.GameCore.Constants;
using Enum = System.Enum;
using TenCrowns.ClientCore;
using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using UnityEngine;
using UnityEngine.UI;
using static TenCrowns.ClientCore.ClientUI;
using static BetterAI.BetterAIInfos;
using BetterAI;
using Random = Mohawk.SystemCore.Random;

namespace BetterAI
{
    public class BetterAITile : Tile
    {
/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
        public virtual CityBiomeType getCityBiome()
        {
            BetterAICity pCityTerritory = (BetterAICity)cityTerritory();

            if (pCityTerritory != null)
            {
                return pCityTerritory.getCityBiome();
            }
            else return CityBiomeType.NONE;
        }

        //lines 3077-3121
        public override void setTerrain(TerrainType eNewValue)
        {
            TerrainType eOldTerrain = getTerrain();
            base.setTerrain(eNewValue);
            if (eOldTerrain != eNewValue)
            {
                BetterAICity pCityTerritory = (BetterAICity)cityTerritory();
                pCityTerritory?.setTerrainChanged();
            }
        }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/


        //canHaveImprovement: lines 4805-5098
        public virtual bool canCityTileHaveImprovement(ImprovementType eImprovement, TeamType eTeamTerritory = TeamType.NONE, bool bTestEnabled = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bUpgradeImprovement = false, bool bForceImprovement = false)
        {
            BetterAIInfoImprovement eInfoImprovement = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            BetterAICity pCityTerritory = (BetterAICity)cityTerritory();


            if (!isImprovementValid(eImprovement, bTestEnabled))
            {
                return false;
            }

            if (!bForceImprovement)
            {
                ImprovementType eImprovementPrereq = eInfoImprovement.meImprovementPrereq;

                if (eImprovementPrereq != ImprovementType.NONE)
                {
                    //if (pCityTerritory == null)
                    //{
                    //    return false;
                    //}

                    int iCount = pCityTerritory.getFinishedImprovementCount(eImprovementPrereq);

                    //if (iCount == 0)
                    //{
                    //    return false;
                    //}

                    if ((iCount == 1) && !bUpgradeImprovement)
                    {
                        if (getImprovement() == eImprovementPrereq)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        //canHaveImprovement: lines 4805-5098
        public virtual bool canGeneralTileHaveImprovement(ImprovementType eImprovement, TeamType eTeamTerritory = TeamType.NONE, bool bTestEnabled = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bUpgradeImprovement = false, bool bForceImprovement = false)
        {
            if (getTerrain() == TerrainType.NONE)
            {
                return false;
            }

            if (impassable())
            {
                return false;
            }

            //moved to canCityTileHaveImprovement
            //if (!isImprovementValid(eImprovement, false))
            //{
            //    return false;
            //}

            BetterAIInfoImprovement eInfoImprovement = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            if (isUrban() && !(eInfoImprovement.mbUrban))
            {
                return false;
            }

            if (eTeamTerritory != TeamType.NONE)
            {
                if ((getTeam() != eTeamTerritory) && !(getTeam() == TeamType.NONE && getOwnerTribe() == TribeType.NONE && !eInfoImprovement.mbTerritoryOnly))
                {
                    return false;
                }
            }
            else if (eInfoImprovement.mbTerritoryOnly)
            {
                if (!hasCityTerritory())
                {
                    return false;
                }
            }

            if (hasImprovementFinished())
            {
                if (improvement().mbPermanent)
                {
                    return false;
                }
            }

            if (bTestAdjacent)
            {
                if (eInfoImprovement.mbRequiresUrban)
                {
                    if (!urbanEligible())
                    {
                        return false;
                    }
                }
            }

            if (bTestEnabled)
            {
                if (hasCity())
                {
                    return false;
                }

                ImprovementClassType eImprovementClass = eInfoImprovement.meClass;
                ReligionType eReligionPrereq = eInfoImprovement.meReligionPrereq;
                if (bTestAdjacent)
                {
                    if (bTestReligion && eReligionPrereq != ReligionType.NONE)
                    {
                        if (eInfoImprovement.mbNoAdjacentReligion)
                        {
                            if (adjacentToOtherImprovementReligion(eReligionPrereq))
                            {
                                return false;
                            }
                        }
                    }

                    if (!bUpgradeImprovement)
                    {
                        ImprovementType eAdjacentImprovementPrereq = eInfoImprovement.meAdjacentImprovementPrereq;

                        if (eAdjacentImprovementPrereq != ImprovementType.NONE)
                        {
                            if (!adjacentToCityImprovementFinished(eAdjacentImprovementPrereq))
                            {
                                return false;
                            }
                        }
                    }

                    if (!bUpgradeImprovement)
                    {
                        ImprovementClassType eAdjacentImprovementClassPrereq = eInfoImprovement.meAdjacentImprovementClassPrereq;

                        if (eAdjacentImprovementClassPrereq != ImprovementClassType.NONE)
                        {
                            if (!adjacentToCityImprovementClassFinished(eAdjacentImprovementClassPrereq))
                            {
                                return false;
                            }
                        }
                    }

                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        if (infos().improvementClass(eImprovementClass).mbNoAdjacent && !notAdjacentToImprovementClass(eImprovementClass))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        //lines 4805-5098
        public override bool canHaveImprovement(ImprovementType eImprovement, TeamType eTeamTerritory = TeamType.NONE, bool bTestEnabled = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bUpgradeImprovement = false, bool bForceImprovement = false)
        {
            //split into 3:
            //tile.canGeneralTileHaveImprovement (not tied to city)
            //city.canCityHaveImprovement (tied only to city)
            //tile.canCityTileHaveImprovement (tied to city and tile)

/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ##############################################*/
            if (!(canGeneralTileHaveImprovement(eImprovement, eTeamTerritory, bTestEnabled, bTestAdjacent, bTestReligion, bUpgradeImprovement, bForceImprovement)))
            {
                return false;
            }

            //BetterAIInfoImprovement eInfoImprovement = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            BetterAICity pCityTerritory = (BetterAICity)cityTerritory();
            if (pCityTerritory != null)
            {
                bool bPrimaryUnlock = true;
                if (!(pCityTerritory.canCityHaveImprovement(eImprovement, ref bPrimaryUnlock, eTeamTerritory, bTestEnabled, bTestReligion, bUpgradeImprovement, bForceImprovement)))
                {
                    return false;
                }

                if (!(canCityTileHaveImprovement(eImprovement, eTeamTerritory, bTestEnabled, bTestAdjacent, bTestReligion, bUpgradeImprovement, bForceImprovement)))
                {
                    return false;
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/

            return true;
        }

        //lines 10897-10962
        public override void getExpansionTiles(List<PairStruct<int, CityTerritory>> aExpansionTiles, int iRange, CityTerritory cityTerritory, TeamType eVisibilityTeam, Dictionary<int, CityTerritory> dTerritoryTiles)
        {
            using (var tileQueueScoped = CollectionCache.GetArrayDequeScoped<PairStruct<int, CityTerritory>>())
            {
                ArrayDeque<PairStruct<int, CityTerritory>> aTileQueue = tileQueueScoped.Value;

                using (var listTilesScoped = CollectionCache.GetListScoped<int>())
                {
                    List<int> aiTiles = listTilesScoped.Value;
                    getTilesInRange(iRange, aiTiles);
                    foreach (int iTileID in aiTiles)
                    {
                        aTileQueue.AddBack(PairStruct.Create(iTileID, cityTerritory));
                    }
                }

                while (aTileQueue.Count > 0)
                {
                    PairStruct<int, CityTerritory> front = aTileQueue.RemoveFront();
                    int iLoopTileID = front.First;
                    Tile pLoopTile = game().tile(iLoopTileID);
                    if (pLoopTile != null)
                    {
                        using (var listChangeScoped = CollectionCache.GetListScoped<PairStruct<int, CityTerritory>>())
                        {
                            List<PairStruct<int, CityTerritory>> aTiles = listChangeScoped.Value;
                            pLoopTile.getOwnerChangeTiles(front.Second, aTiles, eVisibilityTeam, dTerritoryTiles);
                            foreach (PairStruct<int, CityTerritory> p in aTiles)
                            {
/*####### Better Old World AI - Base DLL #######
  ### Border growth fix                START ###
  ##############################################*/
                                if (!game().tile(p.First).hasOwner())
/*####### Better Old World AI - Base DLL #######
  ### Border growth fix                  END ###
  ##############################################*/
                                {
                                    if (!dTerritoryTiles.TryGetValue(p.First, out CityTerritory existingTerritory))
                                    {
                                        aTileQueue.AddBack(p);
                                    }
                                    dTerritoryTiles[p.First] = p.Second;
                                    aExpansionTiles.Add(p);
                                }
                            }

                            for (DirectionType eDir = 0; eDir < DirectionType.NUM_TYPES; ++eDir)
                            {
                                Tile pAdjacent = pLoopTile.tileAdjacent(eDir);
                                if (pAdjacent != null && !pAdjacent.hasRevealedCityTerritory(eVisibilityTeam) && !dTerritoryTiles.ContainsKey(pAdjacent.getID()))
                                {
                                    int iConnectedCitySite = pAdjacent.getConnectedCitySiteActive(eVisibilityTeam);
                                    if (iConnectedCitySite != -1 && iConnectedCitySite != front.Second.iCitySiteID)
                                    {
                                        using (var tileListScoped = CollectionCache.GetListScoped<int>())
                                        {
                                            CityTerritory? minorCityTerritory = game().tile(iConnectedCitySite).checkMinorCity(dTerritoryTiles, eVisibilityTeam, tileListScoped.Value);
                                            if (minorCityTerritory != null)
                                            {
                                                foreach (int iMinorCityTileId in tileListScoped.Value)
                                                {
                                                    PairStruct<int, CityTerritory> p = PairStruct.Create(iMinorCityTileId, minorCityTerritory.Value);
                                                    aTileQueue.AddBack(p);
                                                    aExpansionTiles.Add(p);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


    }
}
