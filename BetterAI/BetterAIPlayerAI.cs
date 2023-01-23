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

namespace BetterAI
{
    public partial class BetterAIPlayer : Player
    {
        public partial class BetterAIPlayerAI : BetterAIPlayer.PlayerAI
        {

            //public BetterAIPlayerAI() : base()
            //{
            //    AI_MAX_FORT_BORDER_DISTANCE_INSIDE = 0;
            //}
            protected override long getHurryCostValue(City pCity, CityBuildHurryType eHurry)
            {
                long iValue = 0;

                switch (eHurry)
                {
                    case CityBuildHurryType.CIVICS:
                        iValue = pCity.getHurryCivicsCost() * yieldValue(infos.Globals.CIVICS_YIELD);
                        break;
                    case CityBuildHurryType.TRAINING:
                        iValue = pCity.getHurryTrainingCost() * yieldValue(infos.Globals.TRAINING_YIELD);
                        break;
                    case CityBuildHurryType.MONEY:
                        iValue = pCity.getHurryMoneyCost() * yieldValue(infos.Globals.MONEY_YIELD);
                        break;
                    case CityBuildHurryType.ORDERS:
                        iValue = pCity.getHurryOrdersCost() * yieldValue(infos.Globals.ORDERS_YIELD);
                        break;
                    case CityBuildHurryType.POPULATION:
                        iValue = pCity.getHurryPopulationCost() * citizenValue(pCity);
                        break;
                }

/*####### Better Old World AI - Base DLL #######
  ### AI fix                           START ###
  ##############################################*/
                //fix discontent value
                //no use calculating the effect of AI_YIELD_TURNS (100 turns) of discontent, when it only happens once

                //iValue -= cityYieldValue(infos.Globals.DISCONTENT_YIELD, pCity) * pCity.getHurryDiscontent();
                iValue -= discontentValue(pCity) * pCity.getHurryDiscontent() / Constants.YIELDS_MULTIPLIER;
/*####### Better Old World AI - Base DLL #######
  ### AI fix                             END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Alternative Hurry                START ###
  ##############################################*/
                //hurry cost reduced, but no overflow: needs to be reflected in AI evaluation
                if (((BetterAIInfoGlobals)infos.Globals).BAI_HURRY_COST_REDUCED_BY_PRODUCTION == 1)
                {
                    YieldType eBuildYield = (pCity.getBuildYieldType(pCity.getCurrentBuild()));
                    iValue += (pCity.calculateModifiedYield(eBuildYield) * cityYieldValue(eBuildYield, pCity)) / (Constants.YIELDS_MULTIPLIER);
                }
                return iValue;
            }

/*####### Better Old World AI - Base DLL #######
  ### AI fix                           START ###
  ##############################################*/
            public virtual long discontentValue(City pCity)
            {
                YieldType eYield = infos.Globals.DISCONTENT_YIELD;

                //copy-pasted from calculateCityValue START
                //if (eYield == infos.Globals.DISCONTENT_YIELD)
                {
                    //int iModifier = 0;
                    long iValue = yieldValue(eYield);
                    int iNextThreshold = pCity.getYieldThreshold(eYield);

                    if (pCity.hasFamily())
                    {
/*####### Better Old World AI - Base DLL #######
  ### AI fix                           START ###
  ##############################################*/
                        //iValue += getFamilyOpinionValue(pCity.getFamily(), infos.Globals.DISLOYALTY_OPINION) * AI_YIELD_TURNS * 5 / (100 * iNextThreshold);
                        iValue += getFamilyOpinionValue(pCity.getFamily(), infos.Globals.DISLOYALTY_OPINION) * 5 / 100; //without AI_YIELD_TURNS, division by iNextThreshold moved down
 /*####### Better Old World AI - Base DLL #######
   ### AI fix                             END ###
   ##############################################*/
                    }

                    for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                    {
                        if (eLoopYield != eYield)
                        {
                            int iExtraYieldInNextLevel = 0;
                            int iCityYield = cityYield(eLoopYield, pCity);
                            iExtraYieldInNextLevel += infos.Helpers.getDiscontentLevelYieldModifier(pCity.getDiscontentLevel() + 1, eLoopYield) * iCityYield / 100;
                            iExtraYieldInNextLevel -= infos.Helpers.getDiscontentLevelYieldModifier(pCity.getDiscontentLevel(), eLoopYield) * iCityYield / 100;

                            if (iExtraYieldInNextLevel != 0)
                            {
/*####### Better Old World AI - Base DLL #######
  ### AI fix                           START ###
  ##############################################*/
                                //iValue += iExtraYieldInNextLevel * cityYieldValue(eLoopYield, pCity) * AI_YIELD_TURNS / iNextThreshold;
                                iValue += (long)iExtraYieldInNextLevel * cityYieldValue(eLoopYield, pCity); //without AI_YIELD_TURNS, division by iNextThreshold moved down
/*####### Better Old World AI - Base DLL #######
  ### AI fix                             END ###
  ##############################################*/
                            }
                        }
                    }

                    //value derivate from base yield, unmodified
                    //iValue = infos.utils().modify(iValue, pCity.calculateTotalYieldModifier(eYield));

                    iValue /= iNextThreshold;
                    //return infos.utils().modify(iValue, infos.yield(eYield).mbNegative ? -iModifier : iModifier);
                    return iValue;
                }
                //copy-paste END
            }

            public override long calculateCityYieldValue(YieldType eYield, City pCity)
            {
                long iValue = base.calculateCityYieldValue(eYield, pCity);

/*####### Better Old World AI - Base DLL #######
  ### AI fix                           START ###
  ##############################################*/
                if (eYield != infos.Globals.GROWTH_YIELD && eYield != infos.Globals.CIVICS_YIELD && eYield != infos.Globals.TRAINING_YIELD)
                {
                    //why is yieldmodifier ignored in all but 3 cases?
                    iValue = infos.utils().modify(iValue, pCity.calculateTotalYieldModifier(eYield));
                }
/*####### Better Old World AI - Base DLL #######
  ### AI fix                             END ###
  ##############################################*/
                return iValue;
            }

            protected override long bonusValue(BonusType eBonus, ref BonusParameters zParameters)
            {
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers  START ###
  ##############################################*/
                //reducing Courtier values if they can't work by 2/3
                long iPlayerValue = 0;
                foreach (KeyValuePair<CourtierType, GenderType> pair in infos.bonus(eBonus).maeAddCourtier)
                {
                    if (pair.Key != CourtierType.NONE)
                    {
                        BetterAIInfoCourtier eInfoCourtier = (BetterAIInfoCourtier)infos.courtier(pair.Key);
                        if (eInfoCourtier.maeAdjectives.Count > 0)
                        {
                            foreach (TraitType eLoopTrait in eInfoCourtier.maeAdjectives)
                            {
                                if (eLoopTrait != TraitType.NONE)
                                {
                                    if (infos.trait(eLoopTrait).mbNoJob)
                                    {
                                        iPlayerValue -= adjustForInflation(2 * AI_COURTIER_VALUE / 3);
                                        break;
                                    }
                                }
                            }
                        }



                        // XXX
                        //GoalData pGoalData = getActiveStatGoal(infos.Globals.COURTIER_ADDED_STAT);
                        //if (pGoalData != null)
                        //{
                        //    iPlayerValue += bonusValue(infos.Globals.FINISHED_AMBITION_BONUS);
                        //}
                    }
                }

                foreach (KeyValuePair<CourtierType, GenderType> pair in infos.bonus(eBonus).maeAddCourtierOther)
                {
                    if (pair.Key != CourtierType.NONE)
                    {
                        BetterAIInfoCourtier eInfoCourtier = (BetterAIInfoCourtier)infos.courtier(pair.Key);

                        if (eInfoCourtier.maeAdjectives.Count > 0)
                        {
                            foreach (TraitType eLoopTrait in eInfoCourtier.maeAdjectives)
                            {
                                if (eLoopTrait != TraitType.NONE)
                                {
                                    if (infos.trait(eLoopTrait).mbNoJob)
                                    {
                                        iPlayerValue -= adjustForInflation(2 * AI_COURTIER_VALUE / 3);
                                        break;
                                    }
                                }
                            }
                        }


                        //GoalData pGoalData = getActiveStatGoal(infos.Globals.COURTIER_ADDED_STAT);
                        //if (pGoalData != null)
                        //{
                        //    iPlayerValue += bonusValue(infos.Globals.FINISHED_AMBITION_BONUS);
                        //}
                    }
                }

                //if (pOtherPlayer.getTeam() == player.getTeam())
                //{
                //    iValue += iPlayerValue;
                //}
                //else
                //{
                //    iValue -= (iPlayerValue / 2);
                //}
                //translated to
                if (iPlayerValue != 0)
                {
                    Player pOtherPlayer = zParameters.eTargetPlayer != PlayerType.NONE ? game.player(zParameters.eTargetPlayer) : player;
                    if (pOtherPlayer.getTeam() != player.getTeam())
                    {
                        iPlayerValue /= 2;
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers    END ###
  ##############################################*/
                return iPlayerValue + base.bonusValue(eBonus, ref zParameters);
            }

            //copy-paste START

            //protected override void doWaterControlTargets()
            //{
            //    //using var profileScope = new UnityProfileScope("PlayerAI.doWaterControlTargets");

            //    bool validTile(Tile pTile)
            //    {
            //        if (isInvalidTarget(pTile.getID()))
            //        {
            //            return false;
            //        }
            //        if (!pTile.isRevealed(player.getTeam()))
            //        {
            //            return false;
            //        }
            //        if (pTile.isVisible(player.getTeam()) && pTile.hasHostileUnit(player.getTeam()))
            //        {
            //            return false;
            //        }
            //        if (pTile.isRevealedCity(player.getTeam()) && game.isHostileCity(player.getTeam(), TribeType.NONE, pTile.city()))
            //        {
            //            return false;
            //        }
            //        return true;
            //    }

            //    Tile getBestValidTile(Tile pTile, int maxDistance)
            //    {
            //        if (pTile != null)
            //        {
            //            int iter = 0;
            //            while (iter <= maxDistance)
            //            {
            //                using (var tileListScoped = CollectionCache.GetListScoped<int>())
            //                {
            //                    pTile.getTilesAtDistance(iter, tileListScoped.Value);
            //                    foreach (int iTileID in tileListScoped.Value)
            //                    {
            //                        Tile pBestTile = game.tile(iTileID);
            //                        if (pBestTile.getLandSection() == pTile.getLandSection() && validTile(pBestTile))
            //                        {
            //                            return pBestTile;
            //                        }
            //                    }
            //                }
            //                ++iter;
            //            }
            //        }
            //        return null;
            //    }

            //    clearWaterControlTargets();

            //    using (var populatedAreasScoped = CollectionCache.GetHashSetScoped<int>())
            //    using (var areaDistancesScoped = CollectionCache.GetDictionaryScoped<PairStruct<int, int>, LandAreaDistance>())
            //    {
            //        HashSet<int> populatedAreas = populatedAreasScoped.Value;
            //        for (int iI = 0; iI < game.getNumTiles(); ++iI)
            //        {
            //            Tile pAreaTile = game.tile(iI);
            //            if (pAreaTile != null)
            //            {
            //                int iLandSection = pAreaTile.getLandSection();
            //                if (iLandSection != -1)
            //                {
            //                    // assume an area is populated until we have all tiles revealed
            //                    if (!pAreaTile.isRevealed(player.getTeam()) || pAreaTile.hasCityTerritory() || pAreaTile.hasUnit() || pAreaTile.isUrban())
            //                    {
            //                        populatedAreas.Add(iLandSection);
            //                    }
            //                }
            //            }
            //        }

            //        //        Type tPlayer = typeof(Player);
            //        //        MethodInfo MIplayerGetTiles = tPlayer.GetMethod("getTiles", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            //        //        SetList<int> siPlayerTiles = (SetList<int>)MIplayerGetTiles.Invoke(player, null);

            //        Dictionary<PairStruct<int, int>, LandAreaDistance> areaDistances = areaDistancesScoped.Value;
            //        for (int iToTile = 0; iToTile < game.getNumTiles(); ++iToTile)
            //        {
            //            Tile pToTile = game.tile(iToTile);
            //            if (pToTile != null)
            //            {
            //                int iToArea = pToTile.getLandSection();
            //                if (populatedAreas.Contains(iToArea))
            //                {
            //                    if (validTile(pToTile))
            //                    {
            //                        foreach (int iFromTile in ((BetterAIPlayer)player).getTiles())
            //                        {
            //                            if (pToTile.getOwner() != player.getPlayer() || iFromTile > iToTile) // no need for two-way routes
            //                            {
            //                                Tile pFromTile = game.tile(iFromTile);
            //                                if (pFromTile != null)
            //                                {
            //                                    int iFromArea = pFromTile.getLandSection();
            //                                    if (iFromTile != -1 && iFromTile != iToArea)
            //                                    {
            //                                        if (validTile(pFromTile))
            //                                        {
            //                                            PairStruct<int, int> key = PairStruct.Create(Math.Min(iFromArea, iToArea), Math.Max(iFromArea, iToArea));
            //                                            int iDistance = pFromTile.distanceTile(pToTile);
            //                                            if (!areaDistances.TryGetValue(key, out LandAreaDistance minDistance) || iDistance < minDistance.iDistance)
            //                                            {
            //                                                areaDistances[key] = new LandAreaDistance { pFromTile = pFromTile, pToTile = pToTile, iDistance = iDistance };
            //                                            }
            //                                        }
            //                                    }
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }

            //        foreach (KeyValuePair<PairStruct<int, int>, LandAreaDistance> p in areaDistances)
            //        {
            //            int iFromArea = p.Key.First;
            //            int iToArea = p.Key.Second;
            //            Tile pToTile = p.Value.pToTile;
            //            Tile pFromTile = p.Value.pFromTile;
            //            using (var seaRouteFinderScoped = game.mpAICache.GetSeaRouteFinderScoped())
            //            using (var tileListScoped = CollectionCache.GetListScoped<int>())
            //            {
            //                List<int> aiSeaRoute = tileListScoped.Value;
            //                if (seaRouteFinderScoped.Value.FindRoute(pFromTile, pToTile, AI_MAX_WATER_CONTROL_DISTANCE, player, game, aiSeaRoute))
            //                {
            //                    for (int i = 0; i < aiSeaRoute.Count; ++i)
            //                    {
            //                        setWaterControlTarget(aiSeaRoute[i], pToTile.getID(), aiSeaRoute.Count, i == aiSeaRoute.Count - 1);
            //                    }
            //                }
            //            }
            //        }

            //        // expansion target in the same area (needed because sometimes that land route is too long)
            //        foreach (int iToTile in getExpansionTargets())
            //        {
            //            Tile pToTile = getBestValidTile(game.tile(iToTile), AI_MAX_WATER_CONTROL_DISTANCE);
            //            if (pToTile != null)
            //            {
            //                LandAreaDistance minDistance = new LandAreaDistance { pFromTile = null, pToTile = pToTile, iDistance = int.MaxValue };


            //                foreach (int iFromTile in ((BetterAIPlayer)player).getTiles())
            //                {
            //                    Tile pFromTile = game.tile(iFromTile);
            //                    if (pFromTile != null)
            //                    {
            //                        int iFromArea = pFromTile.getLandSection();
            //                        if (iFromTile == pToTile.getLandSection())
            //                        {
            //                            if (validTile(pFromTile))
            //                            {
            //                                int iDistance = pFromTile.distanceTile(minDistance.pToTile);
            //                                if (iDistance < minDistance.iDistance)
            //                                {
            //                                    minDistance.iDistance = iDistance;
            //                                    minDistance.pFromTile = pFromTile;
            //                                }
            //                            }
            //                        }
            //                    }
            //                }

            //                if (minDistance.pFromTile != null)
            //                {
            //                    using (var seaRouteFinderScoped = game.mpAICache.GetSeaRouteFinderScoped())
            //                    using (var tileListScoped = CollectionCache.GetListScoped<int>())
            //                    {
            //                        List<int> aiSeaRoute = tileListScoped.Value;
            //                        if (seaRouteFinderScoped.Value.FindRoute(minDistance.pFromTile, minDistance.pToTile, AI_MAX_WATER_CONTROL_DISTANCE, player, game, aiSeaRoute))
            //                        {
            //                            //if (2 * aiSeaRoute.Count < 3 * minDistance.iDistance)
            //                            if (aiSeaRoute.Count < 2 * minDistance.iDistance + 4)
            //                            {
            //                                for (int i = 0; i < aiSeaRoute.Count; ++i)
            //                                {
            //                                    setWaterControlTarget(aiSeaRoute[i], pToTile.getID(), aiSeaRoute.Count, i == aiSeaRoute.Count - 1);
            //                                }
            //                            }
            //                        }
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}


            //protected override void getWaterUnitTargetNumber(UnitType eUnit, City pCity, out int iTargetNumber, out int iCurrentNumber)
            //{
            //    //using var profileScope = new UnityProfileScope("PlayerAI.getWaterUnitTargetNumber");

            //    InfoUnit pUnitInfo = infos.unit(eUnit);

            ////    Type tPlayer = typeof(Player);
            ////    MethodInfo MIplayerCountUnits = tPlayer.GetMethod("countUnits", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            ////    int iPlayerUnits = (int)MIplayerCountUnits.Invoke(player, new[] { IsVisibleForeignShipDelegate });

            ////    MethodInfo MIplayerGetUnits = tPlayer.GetMethod("getUnits", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            ////    SetList<int> siPlayerTiles = (SetList<int>)MIplayerGetUnits.Invoke(player, null);


            //    using (var cityAreasScoped = CollectionCache.GetHashSetScoped<int>())
            //    using (var PreferedCityAreasScoped = CollectionCache.GetHashSetScoped<int>())
            //    using (var areaCitiesScoped = CollectionCache.GetHashSetScoped<int>())
            //    {
            //        HashSet<int> siCityAreas = cityAreasScoped.Value;
            //        HashSet<int> siPreferedCityAreas = PreferedCityAreasScoped.Value;
            //        HashSet<int> siAreaCities = areaCitiesScoped.Value;

            //        foreach (int iTileId in pCity.getTerritoryTiles())
            //        {
            //            Tile pTile = game.tile(iTileId);
            //            if (pTile.isSaltWater() && pTile.getAreaTileCount() > 2 * infos.Globals.FRESH_WATER_THRESHOLD)
            //            {
            //                siCityAreas.Add(pTile.getArea());

            //                ImprovementType eTileImprovement = pTile.getImprovementFinished();
            //                if (eTileImprovement != ImprovementType.NONE)
            //                {
            //                    foreach (UnitTraitType eLoopTrait in infos.unit(eUnit).maeUnitTrait)
            //                    {
            //                        if (infos.improvement(eTileImprovement).maiUnitTraitXP[(int)eLoopTrait] > 0)
            //                        {
            //                            siPreferedCityAreas.Add(pTile.getArea());
            //                            break;
            //                        }
            //                    }
            //                }
            //            }
            //        }

            //        //this is wrong, tie breaker is actually distance from city to harbor, only non-harbor tie break is area size.
            //        if (siPreferedCityAreas.Count() > 0)
            //        {
            //            siCityAreas.Clear();
            //            siCityAreas = siPreferedCityAreas;
            //        }
            //        int iBestArea = 0;
            //        int iBestSize = 0;
            //        if (siCityAreas.Count() > 1)
            //        {

            //            int iLoopArea = 0;
            //            int iLoopAreaSize = 0;
            //            while (siCityAreas.Count() > 0)
            //            {
            //                iLoopArea = siCityAreas.First();
            //                iLoopAreaSize = game.getWaterAreaCount(iLoopArea);
            //                if (iLoopAreaSize > iBestSize)
            //                {
            //                    iBestArea = iLoopArea;
            //                    iBestSize = iLoopAreaSize;
            //                }
            //                siCityAreas.Remove(iLoopArea);
            //            }
            //            siCityAreas.Add(iBestArea);
            //        }
            //        if (iBestSize == 0)
            //        {
            //            iCurrentNumber = 0;
            //            iTargetNumber = 0;
            //            return;
            //        }


            //        int iTargetScoutShips = 0;
            //        {
            //            int iUnexplored = 0;
            //            for (int i = 0; i < game.getNumTiles(); ++i)
            //            {
            //                Tile pLoopTile = game.tile(i);
            //                if (pLoopTile != null)
            //                {
            //                    //if (pLoopTile.isSaltWater() && siCityAreas.Contains(pLoopTile.getArea()))
            //                    if (pLoopTile.isSaltWater() && pLoopTile.getArea() == iBestArea)
            //                    {
            //                        if (!pLoopTile.isRevealed(player.getTeam()))
            //                        {
            //                            ++iUnexplored;
            //                        }
            //                        if (pLoopTile.hasCityTerritory() && pLoopTile.cityTerritory().getPlayer() == player.getPlayer())
            //                        {
            //                            siAreaCities.Add(pLoopTile.getCityTerritory());
            //                        }
            //                    }
            //                }
            //            }
            //            iTargetScoutShips += iUnexplored / 1000;
            //        }

            //        int iTargetWarships = 0;
            //        if (infos.Helpers.canDamage(eUnit) && pUnitInfo.mbWater)
            //        {
            //            //iTargetWarships += siAreaCities.Count * 2;
            //            iTargetWarships += siAreaCities.Count + 1;
            //        }
            //        iTargetWarships += ((BetterAIPlayer)player).countUnits(IsVisibleForeignShipDelegate);

            //        int iTargetWaterControl = 0;
            //        if (pUnitInfo.miWaterControl > 0)
            //        {
            //            for (int iIndex = 0; iIndex < getNumWaterControlTargets(); ++iIndex)
            //            {
            //                Tile pTile = game.tile(getWaterControlTargetTile(iIndex));

            //                //if (pTile != null && siCityAreas.Contains(pTile.getArea()))
            //                if (pTile != null && pTile.getArea() == iBestArea)
            //                {
            //                    ++iTargetWaterControl;
            //                }
            //            }
            //            iTargetWaterControl /= pUnitInfo.miWaterControl;
            //        }

            //        iCurrentNumber = 0;
            //        foreach (int iUnitId in ((BetterAIPlayer)player).getUnits())
            //        {
            //            Unit pUnit = game.unit(iUnitId);
            //            if (pUnit != null)
            //            {
            //                Tile pTile = pUnit.tile();
            //                //if (pTile.isSaltWater() && siCityAreas.Contains(pTile.getArea()))
            //                if (pTile.isSaltWater() && pTile.getArea() == iBestArea)
            //                {
            //                    if (iTargetWaterControl > 0 && pUnit.waterControl() > 0)
            //                    {
            //                        ++iCurrentNumber;
            //                    }
            //                    else if (iTargetWarships > 0 && isWarship(pUnit))
            //                    {
            //                        ++iCurrentNumber;
            //                    }
            //                    else if (iTargetScoutShips > 0 && game.isWaterUnit(pUnit.getType(), player.getPlayer(), TribeType.NONE))
            //                    {
            //                        ++iCurrentNumber;
            //                    }
            //                }
            //            }
            //        }

            //        //iTargetNumber = iTargetWaterControl + Math.Max(iTargetWarships, iTargetScoutShips);

            //        iTargetNumber = Math.Max((iTargetWaterControl/2 + Math.Max(iTargetWarships, iTargetScoutShips)),(iTargetWaterControl + Math.Max(iTargetWarships, iTargetScoutShips)/2));
            //    }
            //}

            //copy-paste END

        }
    }

}
