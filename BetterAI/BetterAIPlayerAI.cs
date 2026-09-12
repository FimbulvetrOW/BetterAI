using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TenCrowns.AppCore;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using UnityEngine;
using UnityEngine.UI;
using static BetterAI.BetterAIInfos;
using static BetterAI.BetterAIUnit;
using static TenCrowns.ClientCore.ClientUI;
using static TenCrowns.GameCore.NoiseGenerator;
using static TenCrowns.GameCore.Text.TextExtensions;
using static TenCrowns.GameCore.Unit;
using Constants = TenCrowns.GameCore.Constants;
using Enum = System.Enum;

namespace BetterAI
{
    public partial class BetterAIPlayer : Player
    {
        public partial class BetterAIPlayerAI : BetterAIPlayer.PlayerAI
        {
            protected int AI_GROWTH_CITY_SPECIALIZATION_MODIFIER => ((BetterAIInfoGlobals)infos.Globals).AI_GROWTH_CITY_SPECIALIZATION_MODIFIER;
            protected int AI_CIVICS_CITY_SPECIALIZATION_MODIFIER => ((BetterAIInfoGlobals)infos.Globals).AI_CIVICS_CITY_SPECIALIZATION_MODIFIER;
            protected int AI_TRAINING_CITY_SPECIALIZATION_MODIFIER => ((BetterAIInfoGlobals)infos.Globals).AI_TRAINING_CITY_SPECIALIZATION_MODIFIER;
            protected int AI_FAMILY_OPINION_VALUE_PER => ((BetterAIInfoGlobals)infos.Globals).AI_FAMILY_OPINION_VALUE_PER;

            protected int AI_EXPANSION_OVERRIDES_ZERO_WAR_CHANCE => ((BetterAIInfoGlobals)infos.Globals).AI_EXPANSION_OVERRIDES_ZERO_WAR_CHANCE;

            protected virtual int AI_CITY_GOVERNOR_VALUE => ((BetterAIInfoGlobals)infos.Globals).AI_CITY_GOVERNOR_VALUE;
            protected virtual int AI_DEATHTRAIT_PROB_EVAL_DEPTH => ((BetterAIInfoGlobals)infos.Globals).AI_DEATHTRAIT_PROB_EVAL_DEPTH;

            [SkipCheckSaveConsistency] protected BetterAIPlayerCache BAI_mpAICache = new BetterAIPlayerCache();

            public override void init(Game pGame, Player pPlayer, Tribe pTribe)
            {
                base.init(pGame, pPlayer, pTribe);
                BAI_mpAICache = new BetterAIPlayerCache();
            }

            public override void initClient(Game pGame, Player pPlayer, Tribe pTribe)
            {
                base.initClient(pGame, pPlayer, pTribe);
                BAI_mpAICache = new BetterAIPlayerCache();
            }

            //lines 8034-8058
            protected override long getHurryCostValue(City pCity, CityBuildHurryType eHurry)
            {
                long iValue = base.getHurryCostValue(pCity, eHurry);

                if (player == null) return iValue;

/*####### Better Old World AI - Base DLL #######
  ### Alternative Hurry                START ###
  ##############################################*/
                //hurry cost reduced, but no overflow: needs to be reflected in AI evaluation
                YieldType eBuildYield = (pCity.getBuildYieldType(pCity.getCurrentBuild()));
                int iCityYieldLost = pCity.calculateCurrentYield(eBuildYield);  //whether or not next Turns production reduced the cost of Hurrying, it's not going to City Production next turn
                int iStockpileYieldGained = iCityYieldLost;                     //but if not it's going to the stockpile instead
                iCityYieldLost += pCity.getYieldOverflow(eBuildYield);          //whether or not Overflow reduced the cost of Hurrying, it's gone afterwards
                iValue += (iCityYieldLost * cityYieldValue(eBuildYield, pCity)) / (Constants.YIELDS_MULTIPLIER);
                if (((BetterAIInfoGlobals)infos.Globals).BAI_HURRY_COST_REDUCED < 3)
                {
                    iValue -= (iStockpileYieldGained * yieldValue(eBuildYield)) / (Constants.YIELDS_MULTIPLIER);
                }
/*####### Better Old World AI - Base DLL #######
  ### Alternative Hurry                  END ###
  ##############################################*/

                return iValue;
            }

/*####### Better Old World AI - Base DLL #######
  ### City Yields                      START ###
  ##############################################*/
            //lines 1184-1194
            public override void refreshCachedValues()
            {
                base.refreshCachedValues();
                BAI_mpAICache.clear();
            }

            //restoring v1.0.70024 version of calculateYieldValue
            public override long calculateYieldValue(YieldType eYield, int iExtraStockpile, int iExtraRate)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.calculateYieldValue");

                if (infos.yield(eYield).meSubtractFromYield != YieldType.NONE)
                {
                    if (iExtraStockpile == 0 && iExtraRate == 0)
                    {
                        return -(yieldValue(infos.yield(eYield).meSubtractFromYield));
                    }
                    else
                    {
                        return -(calculateYieldValue(infos.yield(eYield).meSubtractFromYield, iExtraStockpile, iExtraRate));
                    }
                }

                long iValue = getBaseYieldValue(eYield);
                int iValueModifier = 0;

                if (infos.yield(eYield).mbGlobal && player != null)
                {
                    int iRate = getNetYieldAfterUnits(eYield) + iExtraRate;

                    if (eYield == infos.Globals.ORDERS_YIELD)
                    {
                        int iRateWhole = iRate / Constants.YIELDS_MULTIPLIER;
                        int iTargetOrders = Math.Max(1, getTargetOrders());

                        if (iRateWhole < iTargetOrders)
                        {
                            iValueModifier += Math.Min(AI_ORDER_SHORTAGE_PER_TURN_MODIFIER, AI_ORDER_SHORTAGE_PER_TURN_MODIFIER * (iTargetOrders - iRateWhole) / Math.Max(1, iRateWhole));
                        }
                    }
                    else
                    {
                        int iStockpile = player.getYieldStockpile(eYield) + iExtraStockpile * Constants.YIELDS_MULTIPLIER;

                        //separated effects of isSavingYields(eYield) and Rate <= 0
                        if (iRate <= 0)
                        {
                            iValueModifier += 50;
                            if (isSavingYields(eYield))
                            {
                                iValueModifier += 50;
                            }

                        }
                        else if (isSavingYields(eYield))
                        {
                            iValueModifier += 25;
                        }

                        if (eYield == infos.Globals.TRAINING_YIELD && iStockpile > infos.Helpers.getMaxTraining() / 2)
                        {
                            iValueModifier = 100 * (infos.Helpers.getMaxTraining() / 2 - iStockpile) / infos.Helpers.getMaxTraining();
                        }
                        else if (eYield == infos.Globals.CIVICS_YIELD && iStockpile > infos.Helpers.getMaxCivics() / 2)
                        {
                            iValueModifier = 100 * (infos.Helpers.getMaxCivics() / 2 - iStockpile) / infos.Helpers.getMaxCivics();
                        }
                        else
                        {
                            int iModifiedYieldStockpile = getModifiedYieldStockpileWhole(eYield) + iExtraStockpile;

                            //moved to above after all, and split between isSavingYields(eYield) in Rate <= 0
                            //if (iRate <= 0 || isSavingYields(eYield))
                            //{
                            //    iValueModifier += 50;
                            //}

                            if (iModifiedYieldStockpile > 0)
                            {
                                if (iRate < 0)
                                {
                                    iValueModifier += AI_YIELD_SHORTAGE_PER_TURN_MODIFIER * Math.Max(0, AI_STOCKPILE_TURN_BUFFER - infos.Helpers.turnsLeft(iModifiedYieldStockpile, 0, -iRate));
                                }
                                else
                                {
                                    int iFutureStockpile = iModifiedYieldStockpile + AI_STOCKPILE_TURN_BUFFER * iRate / Constants.YIELDS_MULTIPLIER;
                                    if (iFutureStockpile > AI_NUM_GOODS_TARGET)
                                    {
                                        iValueModifier += Math.Max(-80, 50 * (AI_NUM_GOODS_TARGET - iFutureStockpile) / AI_NUM_GOODS_TARGET);
                                    }
                                }
                            }
                            else
                            {
                                if (iRate <= 0)
                                {
                                    iValueModifier += AI_YIELD_SHORTAGE_PER_TURN_MODIFIER * AI_STOCKPILE_TURN_BUFFER;
                                }
                                else
                                {
                                    iValueModifier += AI_YIELD_SHORTAGE_PER_TURN_MODIFIER * Math.Min(AI_STOCKPILE_TURN_BUFFER, infos.Helpers.turnsLeft(-iModifiedYieldStockpile, 0, iRate)) / 4;
                                }
                            }
                        }
                    }

                    foreach (GoalData pGoalData in ((BetterAIPlayer)player).getGoalDataList())
                    {
                        if (!(pGoalData.mbFinished))
                        {
                            if (infos.goal(pGoalData.meType).maiYieldCount[eYield] > 0 || infos.goal(pGoalData.meType).maiYieldProducedData[eYield] > 0)
                            {
                                iValueModifier += 50;
                            }
                        }
                    }
                }

                return infos.utils().modify(iValue, iValueModifier);
            }

            //lines 4294-4322
            public virtual bool cityNeedsGrowth(City pCity)
            {
                if (getNeedSettlers(pCity) > 0)

                {
                    return true;
                }

                foreach (GoalData pGoalData in ((BetterAIPlayer)player).getGoalDataList())
                {
                    if (!(pGoalData.mbFinished))
                    {
                        if (infos.goal(pGoalData.meType).miPopulation > 0)
                        {
                            return true;
                        }
                        else if (infos.goal(pGoalData.meType).miCitizens > 0)
                        {
                            return true;
                        }
                    }
                }

                for (ReligionType eLoopReligion = 0; eLoopReligion < infos.religionsNum(); eLoopReligion++)
                {
                    if (!(game.canFoundReligion(eLoopReligion, bTestPrereqs: true)))
                    {
                        continue;
                    }
                    int iRequiredCitizens = infos.religion(eLoopReligion).miRequiresCitizens;
                    if (iRequiredCitizens > 0 && iRequiredCitizens < player.countCitizensTotal())
                    {
                        return true;
                        //break;
                    }
                }

                if (pCity.getCitizens() <= 2)
                {
                    return true;
                }

                if (pCity.isHurryPopulation() || pCity.isHurryPopulation(infos.Globals.UNIT_BUILD)
                    || pCity.isHurryPopulation(infos.Globals.SPECIALIST_BUILD) || pCity.isHurryPopulation(infos.Globals.PROJECT_BUILD))
                {
                    return true;
                }

                return false;
            }

            public virtual void cacheCityYieldSpecializationModifiers()
            {
                if (getCities().Count <= 1) return;

                YieldType[] aProductionYields = new YieldType[] { infos.Globals.CIVICS_YIELD, infos.Globals.TRAINING_YIELD, infos.Globals.GROWTH_YIELD };

                using (var cityLsYieldListScoped = CollectionCache.GetListScoped<(int, int, int)>())
                using (var landSectionYieldsScoped = CollectionCache.GetDictionaryScoped<int, int>())
                using (var landSectionCitiesScoped = CollectionCache.GetDictionaryScoped<int, int>())
                {
                    List<(int, int, int)> cityLsYieldList = cityLsYieldListScoped.Value;
                    Dictionary<int, int> landSectionYields = landSectionYieldsScoped.Value;
                    Dictionary<int, int> landSectionCities = landSectionCitiesScoped.Value;

                    foreach (YieldType eLoopYield in aProductionYields)
                    {
                        //int iYieldRatePlayer = 0;
                        //int iNumPlayerCities = 0;
                        int iYieldRateAverage;
                        int iSpecializationModifier = 0;
                        int iBaseSpecializationModifier = 0;

                        if (eLoopYield == infos.Globals.TRAINING_YIELD)
                        {
                            iBaseSpecializationModifier = AI_TRAINING_CITY_SPECIALIZATION_MODIFIER;
                        }
                        else if (eLoopYield == infos.Globals.CIVICS_YIELD)
                        {
                            iBaseSpecializationModifier = AI_CIVICS_CITY_SPECIALIZATION_MODIFIER;
                        }
                        else if (eLoopYield == infos.Globals.GROWTH_YIELD)
                        {
                            iBaseSpecializationModifier = AI_GROWTH_CITY_SPECIALIZATION_MODIFIER;
                        }
                        if (iBaseSpecializationModifier == 0) continue;

                        bool bSameLandSection = (eLoopYield == infos.Globals.TRAINING_YIELD || eLoopYield == infos.Globals.GROWTH_YIELD);
                        int iLandSection = -1;

                        foreach (int iCityID in getCities())
                        {
                            City pLoopCity = game.city(iCityID);

                            if (pLoopCity != null)
                            {
                                int iYieldRateCity = pLoopCity.calculateModifiedYield(eLoopYield);
                                cityLsYieldList.Add((iCityID, iLandSection, iYieldRateCity));

                                if (bSameLandSection)
                                {
                                    iLandSection = pLoopCity.tile().getLandSection();
                                }
                                if (!(landSectionYields.ContainsKey(iLandSection)))
                                {
                                    landSectionYields[iLandSection] = iYieldRateCity;
                                    landSectionCities[iLandSection] = 1;
                                }
                                else
                                {
                                    landSectionYields[iLandSection] += iYieldRateCity;
                                    landSectionCities[iLandSection] += 1;
                                }
                            }
                        }

                        foreach ((int iCityID, int iLandSection, int iYieldRate) cityLsYieldTriple in cityLsYieldList)
                        {
                            iYieldRateAverage = (landSectionYields[cityLsYieldTriple.iLandSection] + landSectionCities[cityLsYieldTriple.iLandSection] - 1) / landSectionCities[cityLsYieldTriple.iLandSection];  //rounded up
                            if (cityLsYieldTriple.iYieldRate > iYieldRateAverage)
                            {
                                iSpecializationModifier = iBaseSpecializationModifier;
                                iSpecializationModifier *= cityLsYieldTriple.iYieldRate;
                                iSpecializationModifier /= iYieldRateAverage;
                                infos.utils().modify(iSpecializationModifier, -(100 / landSectionCities[cityLsYieldTriple.iLandSection]));
                            }

                            lock (gameCacheLock)
                            {
                                BAI_mpAICache.setCityYieldSpecializationModifier(eLoopYield, cityLsYieldTriple.iCityID, iSpecializationModifier);
                            }
                        }

                    }
                }
            }

            public virtual int cityYieldSpecializationModifier(City pCity, YieldType eYield)
            {
                if (pCity != null && (eYield == infos.Globals.CIVICS_YIELD || eYield == infos.Globals.TRAINING_YIELD || eYield == infos.Globals.GROWTH_YIELD))
                {
                    lock (gameCacheLock)
                    {
                        if (BAI_mpAICache.getCityYieldSpecializationModifier(eYield, pCity.getID(), out int iValue))
                        {
                            return iValue;
                        }
                        else
                        {
                            BAI_mpAICache.setCityYieldSpecializationModifier(eYield, pCity.getID(), 0);
                            return 0;
                        }
                    }
                }
                return 0;
            }

            public virtual int getMinYieldTurnsLeft(YieldType eYield, bool bHolyCitiesOnly = false)
            {
                int iValue = int.MaxValue;
                foreach (int iCityID in getCities())
                {
                    City pLoopCity = game.city(iCityID);
                    if (pLoopCity != null && (!bHolyCitiesOnly || pLoopCity.isReligionHolyCityAny()) && pLoopCity.calculateCurrentYield(eYield, true) > 0)
                    {
                        int iTurnsLeft = pLoopCity.getYieldTurnsLeft(eYield);
                        if (iTurnsLeft < iValue)
                        {
                            iValue = iTurnsLeft;
                        }
                    }

                }
                return iValue;
            }


            //lines 1394-1418
            protected override void cacheCityYieldValues()
            {
                base.cacheCityYieldValues();
                cacheCityYieldSpecializationModifiers();
                //attaching to cacheCityYieldValues
                cacheCharacterTurnsRemaining();
            }

            //lines 2715-2927

            protected override void doExpansionTargets(bool bCanDeclareWar, int iPriorityTargets)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.doExpansionTargets");

                if (player != null)
                {
                    foreach (int iExpansionTarget in getExpansionTargets())
                    {
                        Tile pTile = game.tile(iExpansionTarget);
                        if (pTile != null && pTile.isVisible(Team))
                        {
                            Player pStealingPlayer = null;

                            if (pTile.isCitySiteActive(Team))
                            {
                                Unit pBlockUnit = pTile.connectedNoFoundUnit(Team);
                                if (pBlockUnit != null)
                                {
                                    pStealingPlayer = pBlockUnit.player();
                                }
                            }
                            else if (pTile.hasRevealedCityTerritory(Team))
                            {
                                City pCity = pTile.revealedCityTerritory(Team);
                                if (game.getTurn() - pCity.getFoundedTurn() <= 1)
                                {
                                    pStealingPlayer = pCity.player();
                                }
                            }

                            if ((pStealingPlayer != null) &&
                                (pStealingPlayer.getTeam() != Team) &&
                                !(game.isHostile(Team, Tribe, pStealingPlayer.getTeam(), TribeType.NONE)))
                            {
                                if (pTile.getRecentAttacks(pStealingPlayer.getTeam()) < (pTile.getRecentAttacks(Team) / 2))
                                {
                                    if (pStealingPlayer.doEventTrigger(infos.Globals.PLAYER_STOLE_CITY_SITE_EVENTTRIGGER, lTriggerSubjects: new() { getPlayer(), pTile }))
                                    {
                                        pTile.clearRecentAttacks();
                                    }
                                }
                            }
                        }
                    }
                    using (var citySitesScoped = CollectionCache.GetListScoped<int>())
                    {
                        game.getCitySites(citySitesScoped.Value);
                        foreach (int iCitySiteTileID in citySitesScoped.Value)
                        {
                            Tile pCitySite = game.tile(iCitySiteTileID);
                            if (pCitySite != null && pCitySite.isValidFoundLocation(Team, TeamType.NONE))
                            {
                                Unit pBlockUnit = pCitySite.connectedNoFoundUnit(TeamType.NONE);
                                if (pBlockUnit != null && pBlockUnit.getPlayer() == getPlayer())
                                {
                                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < game.getNumPlayers(); ++eLoopPlayer)
                                    {
                                        Player pLoopPlayer = game.player(eLoopPlayer);
                                        if (shouldLeaveTileForTeam(pCitySite, pLoopPlayer.getTeam()))
                                        {
                                            pLoopPlayer.doEventTrigger(infos.Globals.AI_FORFEIT_CITY_SITE_EVENTTRIGGER, lTriggerSubjects: new() { getPlayer(), pCitySite });
                                        }
                                    }
                                }
                            }
                        }
                    }

                }

                using (var valueListScoped = CollectionCache.GetListScoped<(Tile, ExpansionValue)>())
                {
                    List<(Tile, ExpansionValue)> azExpansionValues = valueListScoped.Value;
                    bool bFoundWarPreparingTarget = false;

                    //bool bDeclareWarToExpand = (getExpansionTargets().Count == 0 && getValidCitySites().Count == 0 && areAllCitiesDefended() && bCanDeclareWar);
                    bool bDeclareWarToExpand = (getValidCitySites().Count == 0 && areAllCitiesDefended() && bCanDeclareWar);

                    for (int iI = 0; iI < game.getNumTiles(); ++iI)
                    {
                        Tile pLoopTile = game.tile(iI);
                        if (pLoopTile != null)
                        {
                            (PlayerType eTilePlayer, TribeType eTileTribe) = getTileTargetTerritory(pLoopTile);
                            Player pTilePlayer = eTilePlayer != PlayerType.NONE ? game.player(eTilePlayer) : null;
                            TeamType eTileTeam = pTilePlayer != null ? pTilePlayer.getTeam() : TeamType.NONE;

                            long iValue = tileExpansionValue(pLoopTile, bDeclareWarToExpand || isWarPreparing(eTilePlayer));
                            if (iValue > 0)
                            {
                                if (game.areAllied(eTileTeam, eTileTribe, Team, Tribe))
                                {
                                    continue; // don't break alliance just to expand - only diplomacy AI does this
                                }
                                if (!game.isHostile(eTileTeam, eTileTribe, Team, Tribe))
                                {
                                    if (player == null)
                                    {
                                        continue;
                                    }

                                    if (!bDeclareWarToExpand && !isWarPreparing(eTilePlayer))
                                    {
                                        continue;
                                    }

/*####### Better Old World AI - Base DLL #######
  ### AI expansion even with 0 war chance START#
  ##############################################*/
                                    if (eTilePlayer != PlayerType.NONE)
                                    {
                                        if (player.canDeclareWar(pTilePlayer))
                                        {
                                            if (getWarOfferPercent(eTilePlayer) + AI_EXPANSION_OVERRIDES_ZERO_WAR_CHANCE == 0)
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }

                                    if (eTileTribe != TribeType.NONE)
                                    {
                                        if (player.canDeclareWarTribe(eTileTribe))
                                        {
                                            if (getWarOfferPercent(eTileTribe) + AI_EXPANSION_OVERRIDES_ZERO_WAR_CHANCE == 0)
                                            {
                                                continue;
                                            }
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
/*####### Better Old World AI - Base DLL #######
  ### AI expansion even with 0 war chance END ##
  ##############################################*/

                                }

                                if (player != null)
                                {
                                    if (isAtWarWithPlayer() && !getEnemyPlayers().Contains(eTilePlayer)) // only target major enemies, if they exist
                                    {
                                        continue;
                                    }
                                }

                                ExpansionValue zNewValue = new ExpansionValue();
                                zNewValue.iFoundValue = iValue;
                                zNewValue.iAttackPercent = getDiplomacyAttackPercentPerTurn(eTileTeam, eTilePlayer, eTileTribe);
                                zNewValue.iPriority = 0;
                                if (pLoopTile.hasCityTerritory())
                                {
                                    zNewValue.iPriority += getPriorityCityTiles(pLoopTile.cityTerritory());
                                }
                                else if (pLoopTile.isCitySiteActive())
                                {
                                    zNewValue.iPriority += isPriorityTarget(iI) ? 1 : 0;
                                }

                                azExpansionValues.Add((pLoopTile, zNewValue));

                                if (eTilePlayer != PlayerType.NONE && getWarPreparingPlayer() == eTilePlayer)
                                {
                                    bFoundWarPreparingTarget = true;
                                }
                            }
                        }
                    }

                    azExpansionValues.Sort(ExpansionValue.Compare);

                    if (getWarPreparingPlayer() != PlayerType.NONE)
                    {
                        if (!bFoundWarPreparingTarget)
                        {
                            clearWarPreparingPlayer();
                        }
                        else
                        {
                            setWarPreparingTurns(getWarPreparingTurns() - 1);
                        }
                    }

                    using (var teamsChecked = CollectionCache.GetHashSetScoped<TeamType>())
                    using (var tribesChecked = CollectionCache.GetHashSetScoped<TribeType>())
                    using (var expansionTargetsScoped = CollectionCache.GetHashSetScoped<int>())
                    {
                        HashSet<TeamType> seTeamsChecked = teamsChecked.Value;
                        HashSet<TribeType> seTribesChecked = tribesChecked.Value;

                        for (int iExpansionIndex = 0; iExpansionIndex < azExpansionValues.Count; ++iExpansionIndex)
                        {
                            Tile pLoopTile = azExpansionValues[iExpansionIndex].Item1;
                            expansionTargetsScoped.Value.Add(pLoopTile.getID());

                            (PlayerType eTilePlayer, TribeType eTileTribe) = getTileTargetTerritory(pLoopTile);
                            TeamType eTileTeam = eTilePlayer != PlayerType.NONE ? game.player(eTilePlayer).getTeam() : TeamType.NONE;

                            bool bWarPrepare = game.isHostile(Team, Tribe, eTileTeam, eTileTribe) || isWarPreparing(eTilePlayer);
                            if (!bWarPrepare && iExpansionIndex < iPriorityTargets)
                            {
                                if (!seTeamsChecked.Contains(eTileTeam) || !seTribesChecked.Contains(eTileTribe))
                                {
                                    seTeamsChecked.Add(eTileTeam);
                                    seTribesChecked.Add(eTileTribe);
                                    bWarPrepare = game.randomPercent(azExpansionValues[iExpansionIndex].Item2.iAttackPercent);
                                }
                            }

                            if (bWarPrepare)
                            {

                                if (!getExpansionTargets().Contains(pLoopTile.getID()))
                                {
                                    if (player != null)
                                    {
                                        if (pLoopTile.isRevealedCity(Team))
                                        {
                                            player.pushDebugLogData(new GameLogData("New target: " + pLoopTile.city().getName(), GameLogType.AI_DEBUG, "", "", "", "", game.getTurn(), game.getTeamTurn()));
                                        }
                                        else if (pLoopTile.hasImprovementTribeSite(Team))
                                        {
                                            player.pushDebugLogData(new GameLogData("New target: " + game.textManager().TEXT(pLoopTile.revealedImprovement(Team).mName) + " at " + pLoopTile.ToString(), GameLogType.AI_DEBUG, "", "", "", "", game.getTurn(), game.getTeamTurn()));
                                        }
                                        else
                                        {
                                            player.pushDebugLogData(new GameLogData("New target: Tile at " + pLoopTile.ToString(), GameLogType.AI_DEBUG, "", "", "", "", game.getTurn(), game.getTeamTurn()));
                                        }
                                    }
                                }

                                if (!game.isHostile(Team, Tribe, eTileTeam, eTileTribe))
                                {
                                    if (eTileTribe != TribeType.NONE)
                                    {
                                        if (player != null)
                                        {
                                            player.declareWarTribe(eTileTribe);
                                        }
                                        if (Tribe != TribeType.NONE)
                                        {
                                            MohawkAssert.Assert(false, "tribe war declarations should be made in doPlayerDiplomacy");
                                        }
                                    }
                                    else if (eTilePlayer != PlayerType.NONE && getWarPreparingPlayer() == PlayerType.NONE)
                                    {
                                        setWarPreparingPlayer(eTilePlayer);
                                        setWarPreparingTurns(AI_WAR_PREPARING_TURNS);
                                    }
                                }
                                
                            }
                        }
                        setExpansionTargets(expansionTargetsScoped.Value);
                    }
                }
            }



            //lines 4141-4240
            public override long calculateCityYieldValue(YieldType eYield, City pCity)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.calculateCityYieldValue");

                if (infos.yield(eYield).meSubtractFromYield != YieldType.NONE)
                {
                    return -(cityYieldValue(infos.yield(eYield).meSubtractFromYield, pCity));
                }

                int iModifier = 0;

                //long iValue = yieldValue(eYield);
                long iValue = getBaseYieldValue(eYield);

                if (pCity != null && player != null)
                {
                    int iYieldRate = pCity.calculateModifiedYield(eYield);


                    if (eYield == infos.Globals.GROWTH_YIELD)
                    {

                        //if (isExpandPriority())
                        // isExpandPriority is global, we should look at how many Settlers we need right here
                        bool bNeedsGrowth = false;
                        if (getNeedSettlers(pCity) > 0 || pCity.isYieldBuildCurrent(eYield))

                        {
                            iModifier += 100;
                            bNeedsGrowth = true;
                        }

                        //iModifier += infos.utils().modify(AI_GROWTH_CITY_MODIFIER, pCity.calculateTotalYieldModifier(eYield));
                        iModifier += AI_GROWTH_CITY_MODIFIER;


                        //growth doesn't immediately mean specialists, this is too much
                        //if (pCity.hasFamily())
                        //{
                        //    iModifier += game.familyClass(pCity.getFamily()).miSpecialistsOpinion * AI_FAMILY_OPINION_VALUE / 1000;
                        //}

                        foreach (GoalData pGoalData in ((BetterAIPlayer)player).getGoalDataList())
                        {
                            if (!(pGoalData.mbFinished))
                            {
                                if (infos.goal(pGoalData.meType).miPopulation > 0)
                                {
                                    iModifier += 50;
                                    bNeedsGrowth = true;
                                }
                                else if (infos.goal(pGoalData.meType).miCitizens > 0)
                                {
                                    iModifier += 50;
                                    bNeedsGrowth = true;
                                }
                            }
                        }

                        for (ReligionType eLoopReligion = 0; eLoopReligion < infos.religionsNum(); eLoopReligion++)
                        {
                            if (!(game.canFoundReligion(eLoopReligion, bTestPrereqs: true)))
                            {
                                continue;
                            }
                            int iRequiredCitizens = infos.religion(eLoopReligion).miRequiresCitizens;
                            if (iRequiredCitizens > 0 && iRequiredCitizens < player.countCitizensTotal())
                            {
                                iModifier += 25;
                                bNeedsGrowth = true;
                                break;
                            }
                        }

                        if (pCity.getCitizens() <= 2)
                        {
                            bNeedsGrowth = true;
                        }

                        if (pCity.isHurryPopulation() || pCity.isHurryPopulation(infos.Globals.UNIT_BUILD))
                        {
                            iModifier += (50 * 10) / (10 + pCity.getHurryPopulationCount());
                            bNeedsGrowth = true;
                        }

                        if (pCity.isHurryPopulation() || pCity.isHurryPopulation(infos.Globals.SPECIALIST_BUILD))
                        {
                            iModifier += (40 * 10) / (10 + pCity.getHurryPopulationCount());
                            bNeedsGrowth = true;
                        }

                        if (pCity.isHurryPopulation() || pCity.isHurryPopulation(infos.Globals.PROJECT_BUILD))
                        {
                            iModifier += (25 * 10) / (10 + pCity.getHurryPopulationCount());
                            bNeedsGrowth = true;
                        }


                        //long iValueCitizenFraction = (citizenValue(pCity, false) / pCity.getYieldThresholdWhole(infos.Globals.GROWTH_YIELD));
                        //if (iValue < iValueCitizenFraction)
                        //{
                        //    iValue = iValueCitizenFraction;
                        //}
                        //else if (!bNeedsGrowth)
                        if (!bNeedsGrowth)
                        {
                            if (pCity.hasFamily())
                            {
                                iModifier -= (75 * (pCity.getCitizens() - 2)) / pCity.getCitizens();  //overpopulation
                                //iValue += ((pCity.getCitizens() - 2) * citizenValue(pCity, false)) / (pCity.getYieldThresholdWhole(infos.Globals.GROWTH_YIELD) * pCity.getCitizens()); //this causes unexpected city effect calculation
                            }
                        }

                        iValue = infos.utils().modify(iValue, (iValue > 0) ? iModifier : -(iModifier));
                        iModifier = 0;

                    }
                    else if (eYield == infos.Globals.HAPPINESS_YIELD)
                    {
                        iValue += getHappinessLevelValue((iYieldRate > 0 ? 1 : -1), pCity) / pCity.getYieldThresholdWhole(infos.Globals.HAPPINESS_YIELD);
                    }
                    else if (eYield == infos.Globals.CULTURE_YIELD)
                    {
                        if (iYieldRate < 250)
                        {
                            if (iYieldRate < 50)
                            {
                                //if (cityYield(eYield, pCity) == 0)
                                if (iYieldRate == 0)
                                {
                                    //iModifier += 100;
                                    iModifier += 50;
                                }
                                iModifier += 25 - iYieldRate;

                                if (infos.Globals.HURRY_CULTURE != CultureType.NONE)
                                {
                                    if (infos.Helpers.isCultureHigher(infos.Globals.HURRY_CULTURE, pCity.getCulture()))
                                    {
                                        iModifier += 25 - iYieldRate;
                                    }
                                }

                            }
                            iModifier += 25 - iYieldRate / 10;
                        }

                        {
                            bool bHolyCity = pCity.isReligionHolyCityAny();
                            CultureType eHighestWonderCulture = ((BetterAIGame)game).getHighestWonderCulture((BetterAIPlayer)player, bIncludeHolyCityValid: bHolyCity);
                            bool bHolyCitiesOnly = bHolyCity ? (eHighestWonderCulture > ((BetterAIGame)game).getHighestWonderCulture((BetterAIPlayer)player, bIncludeHolyCityValid: false)) : false;

                            if (eHighestWonderCulture != CultureType.NONE && pCity.getCulture() < eHighestWonderCulture)
                            {
                                if (((BetterAIPlayer)player).countMinCultureCities(infos.Helpers.getNextCulture(pCity.getCulture()), bHolyCitiesOnly: bHolyCitiesOnly) == 0)
                                {
                                    //now to find out if this city is the city closest to the next level
                                    int iTurnsLeft = pCity.getYieldTurnsLeft(eYield);
                                    if (iTurnsLeft <= ((11  * getMinYieldTurnsLeft(eYield, bHolyCitiesOnly: bHolyCitiesOnly)) / 10) ) //less or equal to 110% of the minimum
                                    {
                                        iModifier += 50;
                                    }
                                }
                            }
                        }


                        foreach (GoalData pGoalData in ((BetterAIPlayer)player).getGoalDataList())
                        {
                            if (!(pGoalData.mbFinished))
                            {
                                for (CultureType eCulture = 0; eCulture < infos.culturesNum(); ++eCulture)
                                {
                                    if (infos.goal(pGoalData.meType).maiCultureCount[eCulture] > 0 && pCity.getCulture() < eCulture)
                                    {
                                        iModifier += 50;
                                    }
                                }
                            }
                        }
                    }
                    else if (eYield == infos.Globals.CIVICS_YIELD)
                    {
                        //iModifier += infos.utils().modify(AI_CIVICS_CITY_MODIFIER, pCity.calculateTotalYieldModifier(eYield));
                        iModifier += AI_CIVICS_CITY_MODIFIER;
                    }
                    else if (eYield == infos.Globals.TRAINING_YIELD)
                    {
                        //int iModifierModifier = isPeaceful() ? AI_TRAINING_CITY_MODIFIER : 3 * AI_TRAINING_CITY_MODIFIER / 2;
                        iModifier += isPeaceful() ? AI_TRAINING_CITY_MODIFIER : 3 * AI_TRAINING_CITY_MODIFIER / 2;

                        //check for special unit resources for cities with above-average output
                        int iRarestUnitResourcePer100CitySites = int.MaxValue;
                        ResourceType eRarestUnitResource = ResourceType.NONE;
                        using var resourcesScoped = CollectionCache.GetListScoped<ResourceType>();
                        List<ResourceType> CheckedResources = resourcesScoped.Value;

                        foreach (int iTileID in pCity.getTerritoryTiles())
                        {
                            Tile pLoopTile = game.tile(iTileID);
                            if (pLoopTile != null)
                            {
                                ResourceType eTileResource = pLoopTile.getResource();
                                if (eTileResource != ResourceType.NONE)
                                {
                                    int iResourceRarity = 100 * game.getResourceCount(eTileResource) / (game.getCitySiteCount() + 1);
                                    if (iResourceRarity < iRarestUnitResourcePer100CitySites && !CheckedResources.Contains(eTileResource))
                                    {
                                        CheckedResources.Add(eTileResource);

                                        if (((BetterAIInfoGlobals)infos.Globals).dUnitsWithResourceRequirement.ContainsKey(eTileResource))
                                        {
                                            foreach (UnitType eResourceUnit in ((BetterAIInfoGlobals)infos.Globals).dUnitsWithResourceRequirement[eTileResource])
                                            {
                                                if (infos.unit(eResourceUnit).meProductionType == infos.Globals.TRAINING_YIELD && player.canEverBuildUnit(eResourceUnit) && !player.isUnitObsolete(eResourceUnit))
                                                {
                                                    eRarestUnitResource = eTileResource;
                                                    iRarestUnitResourcePer100CitySites = iResourceRarity;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        if (eRarestUnitResource != ResourceType.NONE)
                        {
                            infos.utils().modify(iModifier, Math.Max(0, 50 - 2 * iRarestUnitResourcePer100CitySites)); //extra value if resource is rarer than 1 per 4 city sites
                        }
                    }

                }
                iValue = infos.utils().modify(iValue, (iValue > 0) ? iModifier : -(iModifier));
                iValue += yieldValue(eYield) - getBaseYieldValue(eYield);  // yield value modifiers and city yield value modifiers effects separated, no longer multiply each other

                return iValue;
            }
/*####### Better Old World AI - Base DLL #######
  ### AI: City Yield Values              END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
            //ToDo: characterSwapValuePerTurn and getOverlapTurns
            //lines 5757-5775
            protected override long leaderValue(Character pCharacter)
            {
                long iValue = 0;

                if (pCharacter != null && player != null)
                {
                    for (RatingType eRating = 0; eRating < infos.ratingsNum(); ++eRating)
                    {
                        iValue += ratingValue(eRating, pCharacter.getRating(eRating), pCharacter, bLeader: true, bLeaderSpouse: false, bSuccessor: false, CourtierType.NONE, CouncilType.NONE, -1, UnitType.NONE);
                    }

                    //foreach (TraitType eLoopTrait in pCharacter.getTraits())
                    //{
                    //    iValue += traitValue(eLoopTrait, pCharacter, true, bLeader: true, bLeaderSpouse: false, bSuccessor: false, CouncilType.NONE, -1, UnitType.NONE);
                    //}
                    iValue += getTotalCharacterTraitsValue(pCharacter, bLeader: true, bLeaderSpouse: false, bSuccessor: false, CouncilType.NONE, iCityGovernor: -1, UnitType.NONE, iCityAgent: -1);

                }

                return iValue;
            }

            //lines 5777-5795
            protected override long leaderSpouseValue(Character pCharacter)
            {
                long iValue = 0;

                if (pCharacter != null && player != null)
                {
                    for (RatingType eRating = 0; eRating < infos.ratingsNum(); ++eRating)
                    {
                        iValue += ratingValue(eRating, pCharacter.getRating(eRating), pCharacter, bLeader: false, bLeaderSpouse: true, bSuccessor: false, CourtierType.NONE, CouncilType.NONE, -1, UnitType.NONE);
                    }

                    //foreach (TraitType eLoopTrait in pCharacter.getTraits())
                    //{
                    //    iValue += traitValue(eLoopTrait, pCharacter, true, bLeader: false, bLeaderSpouse: true, bSuccessor: false, CouncilType.NONE, -1, UnitType.NONE);
                    //}
                    iValue += getTotalCharacterTraitsValue(pCharacter, bLeader: false, bLeaderSpouse: true, bSuccessor: false, CouncilType.NONE, iCityGovernor: -1, UnitType.NONE, iCityAgent: -1);
                }

                return iValue;
            }

            //lines 5797-5815
            protected override long heirValue(Character pCharacter)
            {
                long iValue = 0;

                if (pCharacter != null && player != null)
                {
                    for (RatingType eRating = 0; eRating < infos.ratingsNum(); ++eRating)
                    {
                        iValue += ratingValue(eRating, pCharacter.getRating(eRating), pCharacter, bLeader: false, bLeaderSpouse: false, bSuccessor: true, CourtierType.NONE, CouncilType.NONE, -1, UnitType.NONE);
                    }

                    //foreach (TraitType eLoopTrait in pCharacter.getTraits())
                    //{
                    //    iValue += traitValue(eLoopTrait, pCharacter, true, bLeader: false, bLeaderSpouse: false, bSuccessor: true, CouncilType.NONE, -1, UnitType.NONE);
                    //}
                    iValue += getTotalCharacterTraitsValue(pCharacter, bLeader: false, bLeaderSpouse: false, bSuccessor: true, CouncilType.NONE, iCityGovernor: -1, UnitType.NONE, iCityAgent: -1);

                }

                return iValue;
            }

            //lines 5850-5921
            public override long governorValue(Character pCharacter, City pCity, bool bIncludeCost, bool bSubtractCurrent)
            {
                long iValue = 0;

                if (pCharacter != null && player != null)
                {
                    if (pCharacter.isLeader())
                    {
                        long iLeaderValue = 0;
                        for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                        {
                            int iYieldAmount = infos.yield(eLoopYield).miLeaderGovernor;
                            if (iYieldAmount != 0)
                            {
                                iLeaderValue += infos.utils().modify(iYieldAmount, pCity.calculateTotalYieldModifierForGovernor(eLoopYield, pCharacter)) * cityYieldValue(eLoopYield, pCity);
                            }
                        }
                        iValue += (iLeaderValue * getTurnsLeftEstimateX10(pCharacter, bGeneral: false, bJob: true) / Constants.YIELDS_MULTIPLIER) / 10;
                    }

                    iValue += (getCharacterXPValue(pCharacter, pCity.culture().miXP * getTurnsLeftEstimateX10(pCharacter, bGeneral: false, bJob: true))) / 10;

                    for (RatingType eRating = 0; eRating < infos.ratingsNum(); ++eRating)
                    {
                        iValue += ratingValue(eRating, pCharacter.getRating(eRating), pCharacter, false, false, false, eCouncil: CouncilType.NONE, iCityGovernor: pCity.getID(), eUnitGeneral: UnitType.NONE);
                    }

                    //foreach (TraitType eLoopTrait in pCharacter.getTraits())
                    //{
                    //    iValue += traitValue(eLoopTrait, pCharacter, true, false, false, false, eCouncil: CouncilType.NONE, iCityGovernor: pCity.getID(), eUnitGeneral: UnitType.NONE);
                    //}
                    iValue += getTotalCharacterTraitsValue(pCharacter, bLeader: false, bLeaderSpouse: false, bSuccessor: false, CouncilType.NONE, iCityGovernor: pCity.getID(), UnitType.NONE, iCityAgent: -1);

                    iValue = infos.utils().modify(iValue, getJobValueModifierForTutor(pCharacter), true);

                    if (pCharacter.hasFamily())
                    {
                        int iFamilyOpinion = game.familyClass(pCharacter.getFamily()).miGovernorOpinion;
                        if (pCharacter.isFamilyHead())
                        {
                            iFamilyOpinion += infos.job(infos.Globals.GOVERNOR_JOB).miOpinion + player.getJobOpinionRate(infos.Globals.GOVERNOR_JOB);
                        }

                        if (iFamilyOpinion != 0)
                        {
                            iValue += getFamilyOpinionValue(pCharacter.getFamily(), iFamilyOpinion);
                        }
                    }

                    if (bSubtractCurrent)
                    {
                        if (pCharacter.isJob())
                        {
                            iValue -= jobValue(pCharacter);
                        }
                        if (pCity.isGoverned())
                        {
                            iValue = characterSwapValuePerTurn(pCity.governor(), pCharacter, governorValue(pCity.governor(), pCity, false, false), iValue, false) * getOverlapTurns(pCity.governor(), pCharacter, false);
                        }
                    }


                    if (bIncludeCost)
                    {
                        for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); eLoopYield++)
                        {
                            iValue -= pCity.getMakeGovernorCost(eLoopYield) * yieldValue(eLoopYield);
                        }
                    }
                }

                return iValue;
            }

            //courtiers get nothing from traits

            //lines 5938-6001

            protected override long councilValue(Character pCharacter, CouncilType eCouncil, bool bIncludeCost, bool bSubtractCurrent)
            {
                long iValue = 0;

                if (pCharacter != null && player != null)
                {
                    iValue += (getCharacterXPValue(pCharacter, infos.council(eCouncil).miXP * getTurnsLeftEstimateX10(pCharacter, bGeneral: false, bJob: true))) / 10;

                    for (RatingType eLoopRating = 0; eLoopRating < infos.ratingsNum(); ++eLoopRating)
                    {
                        iValue += ratingValue(eLoopRating, pCharacter.getRating(eLoopRating), pCharacter, false, false, false, eCouncil: eCouncil, iCityGovernor: -1, eUnitGeneral: UnitType.NONE);
                    }

                    //foreach (TraitType eLoopTrait in pCharacter.getTraits())
                    //{
                    //    iValue += traitValue(eLoopTrait, pCharacter, true, false, false, false, eCouncil: eCouncil, iCityGovernor: -1, eUnitGeneral: UnitType.NONE);
                    //}
                    iValue += getTotalCharacterTraitsValue(pCharacter, bLeader: false, bLeaderSpouse: false, bSuccessor: false, eCouncil: eCouncil, iCityGovernor: -1, UnitType.NONE, iCityAgent: -1);

                    iValue = infos.utils().modify(iValue, getJobValueModifierForTutor(pCharacter), true);

                    int iOpinion = pCharacter.getFamilyOpinionCouncil(eCouncil);
                    if (pCharacter.hasFamily())
                    {
                        if (pCharacter.isFamilyHead())
                        {
                            iOpinion += infos.council(eCouncil).miOpinion;
                        }

                        iOpinion -= getLoneCouncilOpinion(pCharacter, eCouncil);

                        iOpinion += pCharacter.getFamilyOpinionCouncil(eCouncil);
                    }
                    if (iOpinion != 0)
                    {
                        iValue += getFamilyOpinionValue(pCharacter.getFamily(), iOpinion);
                    }

                    if (bSubtractCurrent)
                    {
                        if (pCharacter.isJob())
                        {
                            iValue -= jobValue(pCharacter);
                        }
                        if (player.hasCouncilCharacter(eCouncil))
                        {
                            Character pCouncil = player.councilCharacter(eCouncil);
                            iValue = characterSwapValuePerTurn(pCharacter, pCharacter, councilValue(pCouncil, eCouncil, false, false), iValue, false) * getOverlapTurns(pCouncil, pCharacter, false);
                        }
                    }

                    if (bIncludeCost)
                    {
                        if (infos.council(eCouncil).meAssignMission != MissionType.NONE)
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); eLoopYield++)
                            {
                                iValue -= player.getMissionCost(infos.council(eCouncil).meAssignMission, eLoopYield, pCharacter) * yieldValue(eLoopYield);
                            }
                        }
                    }
                }

                return iValue;
            }

            //lines 6003-6042
            public override long agentValue(Character pCharacter, City pCity, bool bIncludeCost, bool bSubtractCurrent)
            {
                long iValue = base.agentValue(pCharacter, pCity, bIncludeCost, bSubtractCurrent);

                //foreach (TraitType eLoopTrait in pCharacter.getTraits())
                //{
                //    iValue += traitValue(eLoopTrait, pCharacter, true, false, false, false, eCouncil: CouncilType.NONE, iCityGovernor: -1, eUnitGeneral: UnitType.NONE, iCityAgent: pCity.getID());
                //}
                iValue += getTotalCharacterTraitsValue(pCharacter, bLeader: false, bLeaderSpouse: false, bSuccessor: false, CouncilType.NONE, iCityGovernor: -1, UnitType.NONE, iCityAgent: pCity.getID());

                return iValue;
            }


            //lines 7140-7162
            protected override void doBuyTilePlanning()
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.doBuyTilePlanning");
                //
                //if (player == null)
                //{
                //    return;
                //}
                //
                //foreach (int iCityID in getCities())
                //{
                //    City pLoopCity = game.city(iCityID);
                //
                //    for (YieldType eYield = 0; eYield < infos.yieldsNum(); ++eYield)
                //    {
                //        (Tile pBuyTile, long iBuyValue) = getBestBuyTile(pLoopCity, eYield);
                //        if (pBuyTile != null)
                //        {
                //            mpAICache.addExpense(pBuyTile, eYield, pLoopCity, iBuyValue, game);
                //        }
                //    }
                //}

                using var profileScope = new UnityProfileScope("PlayerAI.doBuyTilePlanning");

                if (player == null)
                {
                    return;
                }

                foreach (int iCityID in getCities())
                {
                    City pLoopCity = game.city(iCityID);
                    if (pLoopCity != null)
                    {
                        for (YieldType eYield = 0; eYield < infos.yieldsNum(); ++eYield)
                        {
/*####### Better Old World AI - Base DLL #######
  ### AI: BuyTileUnits Planning         START  #
  ##############################################*/
                            if (pLoopCity.canBuyTileUnlocked(eYield))
/*####### Better Old World AI - Base DLL #######
  ### AI: BuyTileUnits Planning           END  #
  ##############################################*/
                            {
                                (Tile pBuyTile, long iBuyValue) = getBestBuyTile(pLoopCity, eYield);
                                if (pBuyTile != null)
                                {
                                    mpAICache.addExpense(pBuyTile, eYield, pLoopCity, iBuyValue, game);
                                }
                            }
                        }
                    }

                }

/*####### Better Old World AI - Base DLL #######
  ### AI: BuyTileUnits Planning         START  #
  ##############################################*/
                foreach (int iUnitID in getUnits())
                {
                    Unit pLoopUnit = game.unit(iUnitID);
                    if (pLoopUnit != null)
                    {
                        for (YieldType eYield = 0; eYield < infos.yieldsNum(); ++eYield)
                        {
                            if (pLoopUnit.hasBuyTileYield(eYield))
                            {
                                (Tile pBuyTile, City pBuyCity, long iBuyValue) = getBestUnitBuyTile(pLoopUnit, eYield, bSkipIfUnlockedInCity: true);
                                if (pBuyTile != null && pBuyCity != null)
                                {
                                    mpAICache.addExpense(pBuyTile, eYield, pBuyCity, iBuyValue, game);
                                }
                            }
                        }
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### AI: BuyTileUnits Planning           END  #
  ##############################################*/

            }

            //public virtual (Tile, long) getBestBuyTile(City pCity, YieldType eYield)
            //lines 12409-12451
            //modified for unit
            public virtual (Tile, City, long) getBestUnitBuyTile(Unit pUnit, YieldType eYield, bool bSkipIfUnlockedInCity = true)
            {
                using var profileScope = new UnityProfileScope("PlayerAI.getBestUnitBuyTile");

                long iBestValue = 0;
                City pBestCity = null; //+City
                Tile pBestTile = null;

/*####### Better Old World AI - Base DLL #######
  ### AI: BuyTileUnits Planning         START  #
  ##############################################*/
                if (player == null || (bSkipIfUnlockedInCity && !pUnit.hasBuyTileYield(eYield)))
                {
                    return (pBestTile, pBestCity, iBestValue);
                }

                foreach (int iCityID in getCities())
                {
                    City pLoopCity = game.city(iCityID);
                    if (pLoopCity != null)
                    {
                        (Tile pLoopCityTile, long iTileValue) = getBestUnitBuyTileInCity(pUnit, eYield, pCity: pLoopCity, bSkipIfUnlockedInCity: true);

                        if (iTileValue > 0)
                        {
                            if (iTileValue > iBestValue)
                            {
                                iBestValue = iTileValue;
                                pBestCity = pLoopCity;   //+City
                                pBestTile = pLoopCityTile;
                            }
                        }
                    }

                    
                }
                return (pBestTile, pBestCity, iBestValue); //+City

            }

            public virtual (Tile, long) getBestUnitBuyTileInCity(Unit pUnit, YieldType eYield, City pCity, bool bSkipIfUnlockedInCity = true, bool bUnitInCity = false)
            {

                long iBestValue = 0;
                Tile pBestTile = null;
                if (pCity != null)
                {
                    if ((!bSkipIfUnlockedInCity && (pCity.canBuyTileUnlocked(eYield) || pUnit.hasBuyTileYield(eYield)))
                        || (bSkipIfUnlockedInCity && !pCity.canBuyTileUnlocked(eYield) && pUnit.hasBuyTileYield(eYield)))
/*####### Better Old World AI - Base DLL #######
  ### AI: BuyTileUnits Planning           END  #
  ##############################################*/
                    {
                        if (!mpAICache.isCityTilesCached(pCity.getID()))
                        {
                            updateCityTiles(pCity);
                        }

                        using (var tileListScoped = CollectionCache.GetListScoped<int>())
                        {
                            foreach (int iTileID in getCityExpandedTiles(pCity.getID()))
                            {
                                tileListScoped.Value.Add(iTileID);
                            }
                            foreach (int iTileID in tileListScoped.Value)
                            {
                                Tile pLoopTile = game.tile(iTileID);

/*####### Better Old World AI - Base DLL #######
  ### AI: BuyTileUnits Planning         START  #
  ##############################################*/
                                //if (!pLoopTile.hasCityTerritory() && pLoopCity.canBuyTile(pLoopTile, eYield, player, null, false) && isBuyEligible(pLoopTile))
                                if (!pLoopTile.hasCityTerritory() && pCity.canBuyTile(pLoopTile, eYield, player, pUnit, false) && isBuyEligible(pLoopTile))
                                {
                                    long iTileValue = getExpansionTileValue(pLoopTile, 0, pCity);
                                    iTileValue -= pCity.getBuyTileCost(pLoopTile, eYield) * yieldValue(eYield);
                                    //iTileValue -= (infos.Globals.UNIT_BUY_TILE_COST + 1) * yieldValue(infos.Globals.ORDERS_YIELD); // estimate orders spent
                                    long iOrders = infos.Globals.UNIT_BUY_TILE_COST;
                                    if (bUnitInCity || pUnit.getTileID() != iTileID)
                                    {
                                        if (getCityExpandedTiles(pCity.getID()).Contains(iTileID))
                                        {
                                            //if (pUnit.canPathTo(pMoveTile, iMaxSteps, pPathfinder))
                                            //{
                                            //    iSteps = pPathfinder.getNumStepsTo(pMoveTile);
                                            //}
                                            iOrders += 1;
                                        }
                                        else
                                        {
                                            if (((BetterAIUnitAI)pUnit.AI).isClosestCity(pCity))
                                            {
                                                iOrders += 2;
                                            }
                                            else
                                            {
                                                iOrders += 4;
                                            }
                                        }
                                    }
                                    iTileValue -= iOrders * yieldValue(infos.Globals.ORDERS_YIELD); // estimate orders spent
/*####### Better Old World AI - Base DLL #######
  ### AI: BuyTileUnits Planning           END  #
  ##############################################*/

                                    if (iTileValue > 0)
                                    {
                                        if (iTileValue > iBestValue || (iTileValue == iBestValue && iTileID > (pBestTile?.getID() ?? -1)))
                                        {
                                            iBestValue = iTileValue;
                                            pBestTile = pLoopTile;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                return (pBestTile, iBestValue);
            }



            //lines 6951-6973
            protected override bool shouldRespectCitySiteOwnership(Player pOtherPlayer)
            {
                if (player == null)
                {
                    return false;
                }
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't treat humans differently START #
  ##############################################*/
                /*
                if (!pOtherPlayer.isHuman())
                {
                    return false;
                }
                */
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't treat humans differently END ###
  ##############################################*/
                if (game.isHostile(Team, Tribe, pOtherPlayer.getTeam(), TribeType.NONE))
                {
                    return false;
                }
                if (pOtherPlayer.getTeam() != Team)
                {
                    if (game.opponentLevel().mbCompetitive)
                    {
                        return false;
                    }
                    if (isOtherPlayerSneaky(pOtherPlayer))
                    {
                        return false;
                    }
                }
                return true;
            }

            //lines 7025-7082
            protected override bool shouldClaimCitySite(Tile pTile)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.shouldClaimCitySite");

                if (player == null)
                {
                    return false;
                }

                if (!canEverSettle())
                {
                    return false;
                }

                if (!pTile.isCitySiteActive()) // slight cheat, but humans can usually tell if the site has been settled by other means (score, surrounding tiles, etc)
                {
                    return false;
                }

                if (!player.canFoundCity())
                {
                    return false;
                }

                if (!((BetterAIPlayer)player).getStartingTiles().Contains(pTile.getID()))
                {
                    if (!isFoundCitySafe(pTile))
                    {
                        return false;
                    }

/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much     START ###
  ##############################################*/
                    //if you clear a camp and then don't guard it, that's on you.
                    /*
                    if (shouldLeaveTileForOtherPlayer(pTile))
                    {
                        return false;
                    }
                    */

                    //this part can stay, to make sure we are only checking not-yet-used starting tiles
                    if (pTile.getCitySite() == CitySiteType.ACTIVE_RESERVED)
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much       END ###
  ##############################################*/
                    {
                        // don't claim reserved sites if we already started with extra cities
                        if (player.doesStartWithCities())
                        {
                            // at least for a grace period
                            if (game.getTurn() < (game.opponentLevel().mbCompetitive ? AI_PLAY_TO_WIN_GRACE_TURNS : AI_GRACE_TURNS))
                            {

/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much     START ###
  ##############################################*/
                                //this section is now in the base game (1.0.65965)
                                //grace turns only apply for non-sneaky players, so the owner needs to be found and checked
                                for (PlayerType eLoopPlayer = 0; eLoopPlayer < game.getNumPlayers(); ++eLoopPlayer)
                                {
                                    BetterAIPlayer pLoopPlayer = (BetterAIPlayer)game.player(eLoopPlayer);

                                    if (eLoopPlayer != getPlayer())
                                    {
                                        if (pTile.isCitySiteActive(player.getTeam()) && pLoopPlayer.getStartingTiles().Contains(pTile.getID()))
                                        {
                                            if (!isOtherPlayerSneaky(pLoopPlayer))
                                            {
                                                return false;
                                            }
                                            break;
                                        }
                                    }
                                }
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much       END ###
  ##############################################*/
                                
                            }
                        }
                    }
                }

                return true;
            }

            public virtual void getCityWaterUnitAreas(UnitType eUnit, City pCity, HashSet<int> siCityAreas)
            {
                InfoUnit pUnitInfo = infos.unit(eUnit);

                //amphibious units start on land and can (probably) walk to all water areas
                if (!pUnitInfo.mbWater) // && game.isWaterUnit(eUnit, getPlayer(), Tribe) == true (no need to check again)
                {
                    foreach (int iTileId in getTiles())
                    {
                        Tile pTile = game.tile(iTileId);
                        if (pTile.isSaltWater() && pTile.getAreaTileCount() > 2 * infos.Globals.FRESH_WATER_THRESHOLD)
                        {
                            siCityAreas.Add(pTile.getArea());
                        }
                    }
                }
                else if (pCity == null)
                {
                    if (player != null)
                    {
                        foreach (int iCityID in player.getCities())
                        {
                            City pLoopCity = game.city(iCityID);
                            if (pLoopCity != null)
                            {
                                getCityWaterUnitAreas(eUnit, pLoopCity, siCityAreas);
                            }
                        }
                    }
                    else if (tribe != null)
                    {
                        foreach (int iCityID in tribe.getCities())
                        {
                            City pLoopCity = game.city(iCityID);
                            if (pLoopCity != null)
                            {

                                getCityWaterUnitAreas(eUnit, pLoopCity, siCityAreas);
                            }
                        }
                    }
                }
                else
                {
                    foreach (int iTileId in pCity.getTerritoryTiles())
                    {
                        Tile pTile = game.tile(iTileId);
                        if (pTile.isSaltWater())
                        {
                            if (game.getWaterAreaCount(pTile.getArea()) > 2 * infos.Globals.FRESH_WATER_THRESHOLD)
                            {
                                siCityAreas.Add(pTile.getArea());
                            }
                        }
                    }
                }

                return;
            }

/*####### Better Old World AI - Base DLL #######
  ### AI: Less ships                   START ###
  ##############################################*/
            //lines 8702-8799
            //unused, to be reviewed later
            protected virtual void getWaterUnitTargetNumber(UnitType eUnit, City pCity, out int iTargetNumber, out int iCurrentNumber)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.getWaterUnitTargetNumber");

                InfoUnit pUnitInfo = infos.unit(eUnit);

                bool bWaterControlShip = (pUnitInfo.miWaterControl > 0);
                bool bWarShip = (infos.Helpers.canDamage(eUnit) && pUnitInfo.mbWater);
                bool bScoutShip = (game.isWaterUnit(eUnit, getPlayer(), Tribe) && pUnitInfo.miReveal > 0);

                using (var waterControlDestinationsScoped = CollectionCache.GetHashSetScoped<int>())
                using (var cityAreasScoped = CollectionCache.GetHashSetScoped<int>())
                using (var areaCitiesScoped = CollectionCache.GetHashSetScoped<int>())
                {
                    HashSet<int> siWaterControlDestinations = waterControlDestinationsScoped.Value;
                    HashSet<int> siCityAreas = cityAreasScoped.Value;
                    HashSet<int> siAreaCities = areaCitiesScoped.Value;

                    getCityWaterUnitAreas(eUnit, pCity, siCityAreas);

                    if (siCityAreas.Count == 0)
                    {
                        iTargetNumber = 0;
                        iCurrentNumber = 0;
                        return;
                    }

                    int iTargetScoutShips = 0;

                    //if (player != null)
                    if (player != null && bScoutShip)  //in case there are water units unfit for scouting
                    {
                        int iUnexplored = 0;
                        for (int i = 0; i < game.getNumTiles(); ++i)
                        {
                            Tile pLoopTile = game.tile(i);
                            if (pLoopTile != null)
                            {
                                if (pLoopTile.isSaltWater() && siCityAreas.Contains(pLoopTile.getArea()))
                                {
                                    if (!pLoopTile.isRevealed(Team))
                                    {
                                        ++iUnexplored;
                                    }
                                    if (pLoopTile.hasCityTerritory() && isOwnCity(pLoopTile.cityTerritory()))
                                    {
                                        siAreaCities.Add(pLoopTile.getCityTerritory());
                                    }
                                }
                            }
                        }
                        iTargetScoutShips += iUnexplored / 1000;
                    }
                    iTargetScoutShips = Math.Min(3, iTargetScoutShips);

                    int iTargetWarships = 0;
                    if (bWarShip)
                    {
                        //iTargetWarships += siAreaCities.Count * 2;
                        iTargetWarships += (siAreaCities.Count * 2) - 1;
                    }
                    iTargetWarships += countUnits(IsVisibleForeignShipDelegate);


                    //int iTargetWaterControl = 0;
                    int iTargetWaterControlTiles = 0;
                    if (bWaterControlShip)
                    {

                        for (int iIndex = 0; iIndex < getNumWaterControlTargets(); ++iIndex)
                        {
                            //Tile pTile = game.tile(getWaterControlTargetTile(iIndex));
                            mpAICache.getWaterControlTarget(iIndex, out int iTileID, out int iTargetTileID, out _, out _);
                            Tile pTile = game.tile(iTileID);

                            if (pTile != null && siCityAreas.Contains(pTile.getArea()))
                            {
                                //++iTargetWaterControl;
                                ++iTargetWaterControlTiles;
                                siWaterControlDestinations.Add(iTargetTileID);
                            }
                        }

                        //iTargetWaterControl /= pUnitInfo.miWaterControl;
                        //iTargetWaterControl = (iTargetWaterControl + 2 * pUnitInfo.miWaterControl) / (2 * pUnitInfo.miWaterControl + 1);
                    }

                    iCurrentNumber = 0;
                    int iCurrentWaterControlLength = 0;
                    int iCurrentWarShips = 0;
                    int iCurrentScoutShips = 0;

                    foreach (int iUnitId in getUnits())
                    {
                        Unit pUnit = game.unit(iUnitId);
                        if (pUnit != null)
                        {
                            Tile pTile = pUnit.tile();
                            if (pTile.isSaltWater() && siCityAreas.Contains(pTile.getArea()))
                            {
                                if (iTargetWaterControlTiles > 0 && pUnit.waterControl() > 0)
                                {
                                    iCurrentWaterControlLength += 2 * pUnit.waterControl() + 1;
                                }
                                else if (iTargetWarships > 0 && isWarship(pUnit))
                                {
                                    ++iCurrentWarShips;
                                }
                                else if (iTargetScoutShips > 0 && pUnit.reveal() > 0)
                                {
                                    ++iCurrentScoutShips;
                                }
                            }
                        }
                    }

                    //iTargetNumber = iTargetWaterControl + Math.Max(iTargetWarships, iTargetScoutShips);
                    iTargetNumber = Math.Max(0, ((iTargetWaterControlTiles - iCurrentWaterControlLength) + 2 * pUnitInfo.miWaterControl) / (2 * pUnitInfo.miWaterControl + 1));
                    iCurrentNumber = (iCurrentWaterControlLength + 2 * pUnitInfo.miWaterControl) / (2 * pUnitInfo.miWaterControl + 1);
                    iTargetNumber += iCurrentNumber;
                    iTargetNumber = Math.Max(iTargetNumber, siWaterControlDestinations.Count());

                    if (iTargetWarships - iCurrentWarShips > iTargetScoutShips - iCurrentScoutShips)
                    {
                        iTargetNumber += iTargetWarships;
                        iCurrentNumber += iCurrentWarShips;
                    }
                    else
                    {
                        iTargetNumber += iTargetScoutShips;
                        iCurrentNumber += iCurrentScoutShips;
                    }
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### AI: Less ships                     END ###
  ##############################################*/

            //lines 9401-9463
            protected override int calculateTargetMilitaryUnitNumber()
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.calculateTargetUnitNumber");

                if (player == null)
                {
                    return 0;
                }

/*####### Better Old World AI - Base DLL #######
  ### AI: Don't go crazy with mil units START ##
  ##############################################*/
                //int iUnits = Math.Min(Math.Max(10, 3 * getCities().Count), getNetOrdersAfterUnitsWhole() / 2);
                //int iUnits = Math.Max(10, 5 * getCities().Count / 2);
                int iUnits = Math.Min(3 * getCities().Count, getNetOrdersAfterUnitsWhole() / 2);
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't go crazy with mil units  END ###
  ##############################################*/

                for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos.improvementsNum(); ++eLoopImprovement)
                {
                    if (isFort(eLoopImprovement))
                    {
                        iUnits += player.getActiveImprovementCount(eLoopImprovement);
                    }
                }
                if (player != null)
                {
                    for (int iTileID = 0; iTileID < game.getNumTiles(); ++iTileID)
                    {
                        Tile pLoopTile = game.tile(iTileID);
                        if (pLoopTile != null)
                        {
                            //if (!pLoopTile.hasCityTerritory() && pLoopTile.hasRevealedImprovement(player.getTeam()) && pLoopTile.hasActiveImprovement())
                            if (!pLoopTile.hasCityTerritory() && pLoopTile.isVisible(player.getTeam()) && pLoopTile.hasActiveImprovement())
                            {
                                if (isValidFortTile(pLoopTile) && isFort(pLoopTile.getImprovement()))
                                {
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't go crazy with mil units START ##
  ##############################################*/
                                    //check if the Fort belongs to someone else already
                                    bool bBlocked = false;
                                    if (pLoopTile.hasUnit())
                                    {
                                        using (var unitListScoped = CollectionCache.GetListScoped<int>())
                                        {
                                            pLoopTile.getAliveUnits(unitListScoped.Value);

                                            foreach (int iUnitID in unitListScoped.Value)
                                            {
                                                Unit pLoopUnit = game.unit(iUnitID);

                                                if (!(pLoopUnit.isHiddenFrom(player.getTeam())))
                                                {
                                                    //if (game.isHostileUnit(player.getTeam(), TribeType.NONE, pLoopUnit) && pLoopTile.canUnitDefend(pLoopUnit.getType()))
                                                    if (pLoopUnit.getPlayer() != getPlayer() && pLoopTile.canUnitDefend(pLoopUnit.getType()))
                                                    {
                                                        if (pLoopUnit.info().mbBlocks)
                                                        {
                                                            bBlocked = true;
                                                            break;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    if (!bBlocked)
                                    {
                                        ++iUnits;
                                    }
                                }
                                //for a tribe site you need a settler, not a military unit
                                //else if (pLoopTile.getImprovementTribeSite(player.getTeam()) == TribeType.NONE && pLoopTile.getTribeSettlementOrRuins(player.getTeam()) != TribeType.NONE)
                                //{
                                //    ++iUnits;
                                //}
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't go crazy with mil units  END ###
  ##############################################*/
                            }
                        }
                    }
                }

                int iModifier = 50;
                if (isPeaceful() && !game.isCompetitiveGameMode())
                {
                    int iMaxWarModifier = 0;
                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < game.getNumPlayers(); ++eLoopPlayer)
                    {
                        Player pLoopPlayer = game.player(eLoopPlayer);
                        if (pLoopPlayer.isAlive() && !game.areTeamsAllied(pLoopPlayer.getTeam(), Team) && game.isTeamContact(Team, pLoopPlayer.getTeam()))
                        {
                            int iPlayerWarModifier = 0;
                            PowerType eStrength = player.calculatePowerOf(eLoopPlayer);
                            if (eStrength != PowerType.NONE)
                            {
                                iPlayerWarModifier += infos.power(eStrength).miWarModifier;
                            }
                            ProximityType eProximity = player.calculateProximityPlayer(eLoopPlayer);
                            if (eProximity != ProximityType.NONE)
                            {
                                iPlayerWarModifier += infos.proximity(eProximity).miWarModifier;
                            }
                            OpinionPlayerType eOpinion = pLoopPlayer.calculatePlayerOpinionOfUs(getPlayer());
                            if (eOpinion != OpinionPlayerType.NONE)
                            {
                                iPlayerWarModifier += infos.opinionPlayer(eOpinion).miWarPercent;
                            }
                            if (pLoopPlayer.isHuman())
                            {
                                iPlayerWarModifier += game.opponentLevel().miWarModifier;
                            }
                            iMaxWarModifier = Math.Max(iMaxWarModifier, iPlayerWarModifier);
                        }
                    }

                    iModifier = Math.Max(-50, Math.Min(50, iMaxWarModifier - 100));
                }

                iUnits = infos.utils().modify(iUnits, iModifier);

                if (player != null)
                {
                    int iCharacterModifier = 0;
                    if (player.isHuman())
                    {
                        for (CouncilType eCouncil = 0; eCouncil < infos.councilsNum(); ++eCouncil)
                        {
                            if (infos.council(eCouncil).mbTraitsAffectAutobuild && player.hasCouncilCharacter(eCouncil))
                            {
                                iCharacterModifier += player.councilCharacter(eCouncil).getUnitBuildModifier();
                            }
                        }
                    }
                    else if (player.hasLeader())
                    {
                        iCharacterModifier += player.leader().getUnitBuildModifier();
                    }

                    iUnits = infos.utils().modify(iUnits, Math.Max(-50, Math.Min(100, iCharacterModifier)));
                }

/*####### Better Old World AI - Base DLL #######
  ### AI: Don't go crazy with mil units START ##
  ##############################################*/
                //return iUnits;
                return Math.Max(1 + (3 * getCities().Count), iUnits);
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't go crazy with mil units  END ###
  ##############################################*/
            }

            //moved to correct relative position
            //lines 9770-10056
            protected override long calculateEffectUnitValue(EffectUnitType eEffectUnit, UnitType eForUnit, bool bIncludeIndirectEffects)
            {
                long iValue = base.calculateEffectUnitValue(eEffectUnit, eForUnit, bIncludeIndirectEffects);
                
                BetterAIInfoEffectUnit pEffectUnitInfo = (BetterAIInfoEffectUnit)infos.effectUnit(eEffectUnit);

/*####### Better Old World AI - Base DLL #######
  ### Enlist Replacement Attack Heal   START ###
  ##############################################*/
                iValue += (pEffectUnitInfo.miHealAttack * AI_UNIT_HEAL_VALUE * 5 / 300); //less than healAlways, more than Heal
/*####### Better Old World AI - Base DLL #######
  ### Enlist Replacement Attack Heal     END ###
  ##############################################*/

                return iValue;
            }


            //unused, not sure if I will ever use this
            public virtual long productionCostValue(YieldType eYield, int iCost, City pCity)
            {
                long iValue = 0;
                iCost *= Constants.YIELDS_MULTIPLIER; //production costs are whole
                bool bNeedsGrowth = cityNeedsGrowth(pCity);

                int iCurrentCivicsYield = pCity.calculateCurrentYield(infos.Globals.CIVICS_YIELD, bTestBuild: false, bAccountForCurrentBuild: false); 
                int iCurrentTrainingYield = pCity.calculateCurrentYield(infos.Globals.TRAINING_YIELD, bTestBuild: false, bAccountForCurrentBuild: false);
                int iCurrentGrowthYield = pCity.calculateCurrentYield(infos.Globals.GROWTH_YIELD, bTestBuild: false, bAccountForCurrentBuild: false);

                int iEquivalentCivicsYield;
                int iEquivalentTrainingYield;
                int iEquivalentGrowthYield;

                if (eYield == infos.Globals.GROWTH_YIELD)
                {
                    iEquivalentCivicsYield = (iCost * iCurrentCivicsYield) / iCurrentGrowthYield;
                    iEquivalentTrainingYield = (iCost * iCurrentTrainingYield) / iCurrentGrowthYield;
                    iEquivalentGrowthYield = iCost;
                    iValue += iEquivalentGrowthYield * Math.Max(0, cityYieldValue(infos.Globals.GROWTH_YIELD, pCity));
                }
                else
                {
                    if (eYield == infos.Globals.CIVICS_YIELD)
                    {
                        iEquivalentTrainingYield = (iCost * iCurrentTrainingYield) / iCurrentCivicsYield;
                        iEquivalentGrowthYield = (iCost * iCurrentGrowthYield) / iCurrentCivicsYield;
                        iEquivalentCivicsYield = iCost;
                        iValue -= yieldValue(infos.Globals.TRAINING_YIELD);
                    }
                    else if (eYield == infos.Globals.TRAINING_YIELD)
                    {
                        iEquivalentCivicsYield = (iCost * iCurrentCivicsYield) / iCurrentTrainingYield;
                        iEquivalentGrowthYield = (iCost * iCurrentGrowthYield) / iCurrentTrainingYield;
                        iEquivalentTrainingYield = iCost;
                        iValue -= yieldValue(infos.Globals.CIVICS_YIELD);
                    }
                    else return (long)0;

                    iValue += iEquivalentGrowthYield * Math.Max(cityYieldValue(infos.Globals.GROWTH_YIELD, pCity), (bNeedsGrowth ? 0 : -1 * (citizenValue(pCity, false) / pCity.getYieldThresholdWhole(infos.Globals.GROWTH_YIELD))));
                }

                iValue += iEquivalentCivicsYield * cityYieldValue(infos.Globals.CIVICS_YIELD, pCity);
                iValue += iEquivalentTrainingYield * cityYieldValue(infos.Globals.TRAINING_YIELD, pCity);

                return (iValue / Constants.YIELDS_MULTIPLIER);  //.. and for values we actually need whole numbers
            }

            //lines 9609-10024
            protected override long calculateImprovementValueForTile(Tile pTile, City pCity, ImprovementType eImprovement)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.calculateImprovementValueForTile");

                if (player == null)
                {
                    return -1;
                }

                if (infos == null)
                {
                    UnityEngine.Debug.Log("infos is null");
                    return -1;
                }

                if (game == null)
                {
                    UnityEngine.Debug.Log("game is null");
                    return -1;
                }

                if (pTile == null)
                {
                    UnityEngine.Debug.Log("infos is null");
                    return -1;
                }

                if (pTile.getImprovement() != eImprovement && !((BetterAITile)pTile).canHaveImprovement(eImprovement, pCity, 
                    bTestEnabled: false, bTestTerritory: false, bTestAdjacent: false, bTestReligion: false, bForceImprovement: true, bTestCulture: false, bTestImprovement: false))
                {
                    return -1;
                }

                BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos.improvement(eImprovement);

                if (pImprovementInfo == null)
                {
                    UnityEngine.Debug.Log("Improvement Info is null");
                    return -1;
                }

                if (pImprovementInfo.mbUrban && !pTile.isUrban() && pTile.hasResource())
                {
                    return -1;
                }

                // Don't even evaluate the Jerwan in a bad location
                {
                    foreach (ImprovementType eAdjacentImprovementSpecialist in pImprovementInfo.maeAdjacentImprovementSpecialists)
                    {
                        int iNumEligible = 0;
                        for (DirectionType eDir = 0; eDir < DirectionType.NUM_TYPES; ++eDir)
                        {
                            Tile pAdjacent = pTile.tileAdjacent(eDir);
                            if (pAdjacent != null)
                            {
                                if (pAdjacent.getImprovement() == eAdjacentImprovementSpecialist || ((BetterAITile)pAdjacent).canHaveImprovement(eAdjacentImprovementSpecialist, pCity, 
                                    bTestEnabled: false, bTestTerritory: false, bTestAdjacent: false, bTestReligion: false, bTestCulture: false, bTestImprovement: false))
                                {
                                    ++iNumEligible;
                                }
                            }
                        }
                        if (pImprovementInfo.maeAdjacentImprovementSpecialists.Count > 0 && iNumEligible < 4)
                        {
                            return -1;
                        }
                    }
                }

                {
                    // take advantage of the free specialist, don't build other improvements
                    SpecialistType eFreeSpecialist = pTile.getFreeSpecialist(eImprovement);
                    if (eFreeSpecialist == SpecialistType.NONE && pTile.hasFreeSpecialist())
                    {
                        return -1;
                    }
                }

                // don't build Wonders where they are in danger of being captured
                if (infos.improvement(eImprovement).mbWonder && pCity != null && isCityInDanger(pCity))
                {
                    return -1;
                }

                bool bRemove = pTile.getImprovement() == eImprovement;

                ImprovementClassType eImprovementClass = pImprovementInfo.meClass;

                long iValue = 0;

                int iExtraCivicsProduction = 0;
                //beging scope here
                using (var effectCityExtraCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
                using (var effectCityExtraYieldsScoped = CollectionCache.GetListScoped<int>())
                {
                    Dictionary<EffectCityType, int> dEffectCityExtraCounts = effectCityExtraCountsScoped.Value;
                    List<int> extraYields = effectCityExtraYieldsScoped.Value;
                    for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                    {
                        extraYields.Add(0);
                    }

                    if (!bRemove && pTile.getImprovement() != ImprovementType.NONE)
                    {
                        addImprovementCityEffectCounts(pTile.getImprovement(), pTile, dEffectCityExtraCounts, true);
                    }

                    {
                        EffectCityType eEffectCity = pImprovementInfo.meEffectCity;

                        if (eEffectCity != EffectCityType.NONE)
                        {
                            iValue += effectCityValue(eEffectCity, pCity, bRemove);
                            cityEffectExtraYieldFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts, extraYields);
                            iValue += cityEffectExtraUnlockValueFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts);
                        }
                    }

                    if (pCity != null && pCity.getImprovementCount(eImprovement) <= (3 + (bRemove ? 1 : 0)))
                    {
                        long iBestLandUnitValue = 0;
                        long iBestWaterUnitValue = 0;
                        long iBestExistingLandUnitValue = 1;
                        long iBestExistingWaterUnitValue = 1;
                        UnitType eBestLandUnit = UnitType.NONE;
                        UnitType eBestWaterUnit = UnitType.NONE;
                        for (UnitType eLoopUnit = 0; eLoopUnit < infos.unitsNum(); ++eLoopUnit)
                        {
                            if (player.canEverBuildUnit(eLoopUnit))
                            {
                                if (infos.unit(eLoopUnit).meImprovementPrereq == eImprovement)
                                {
                                    using (var areasScoped = CollectionCache.GetHashSetScoped<int>())
                                    {
                                        if (infos.unit(eLoopUnit).mbWater)
                                        {
                                            pCity.getSaltWaterAreas(areasScoped.Value);
                                        }
                                        else
                                        {
                                            areasScoped.Value.Add(-1);
                                        }
                                        foreach (int iAreaID in areasScoped.Value)
                                        {
                                            long iUnitValue = getUnitBuildValue(eLoopUnit, pCity, iAreaID);
                                            if (iAreaID == -1) //land
                                            {
                                                if (iUnitValue > iBestLandUnitValue)
                                                {
                                                    iBestLandUnitValue = iUnitValue;
                                                    eBestLandUnit = eLoopUnit;
                                                }
                                            }
                                            else //water
                                            {
                                                if (iUnitValue > iBestWaterUnitValue)
                                                {
                                                    iBestWaterUnitValue = iUnitValue;
                                                    eBestWaterUnit = eLoopUnit;
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (pCity.canBuildUnit(eLoopUnit, bBuyGoods: true, bTestEnabled: true, bTestGoods: false))
                                {
                                    using (var areasScoped = CollectionCache.GetHashSetScoped<int>())
                                    {
                                        if (infos.unit(eLoopUnit).mbWater)
                                        {
                                            pCity.getSaltWaterAreas(areasScoped.Value);
                                        }
                                        else
                                        {
                                            areasScoped.Value.Add(-1);
                                        }
                                        foreach (int iAreaID in areasScoped.Value)
                                        {
                                            long iUnitValue = getUnitBuildValue(eLoopUnit, pCity, iAreaID);
                                            if (iAreaID == -1) //land
                                            {
                                                if (iUnitValue > iBestExistingLandUnitValue)
                                                {
                                                    iBestExistingLandUnitValue = iUnitValue;
                                                }
                                            }
                                            else //water
                                            {
                                                if (iUnitValue > iBestExistingWaterUnitValue)
                                                {
                                                    iBestExistingWaterUnitValue = iUnitValue;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        {
                            long iLandUnitUnlockValue = 0;
                            long iWasterUnitUnlockValue = 0;
                            if (iBestLandUnitValue > iBestExistingLandUnitValue)
                            {
                                int iMod = cityYieldSpecializationModifier(pCity, infos.unit(eBestLandUnit).meProductionType);
                                long iDiffValue = (100 * iBestExistingLandUnitValue) / iBestLandUnitValue;  // < 100 (existing value in % of unlocked value)
                                iDiffValue *= iDiffValue * iDiffValue;                                      // < 1000000
                                iDiffValue /= 10000;                                                        // < 100 (%^3: example existing is 59% of new, then iDiffValue = 59%^3 = 20)
                                iDiffValue = (100 - iDiffValue) * iBestLandUnitValue * (100 + iMod);        // < 100 * iBestUnitValue * >=100
                                iDiffValue /= 100;                                                          // ~ 100 * iBestUnitValue
                                iLandUnitUnlockValue = (iDiffValue * AI_YIELD_TURNS * pCity.calculateModifiedYield(infos.unit(eBestLandUnit).meProductionType)) / (player.getUnitBuildCost(eBestLandUnit, pCity) * Constants.YIELDS_MULTIPLIER * 100);

                            }

                            if (iBestWaterUnitValue > iBestExistingWaterUnitValue)
                            {
                                int iMod = cityYieldSpecializationModifier(pCity, infos.unit(eBestWaterUnit).meProductionType);
                                long iDiffValue = (100 * iBestExistingWaterUnitValue) / iBestWaterUnitValue;  // < 100 (existing value in % of unlocked value)
                                iDiffValue *= iDiffValue * iDiffValue;                                        // < 1000000
                                iDiffValue /= 10000;                                                          // < 100
                                iDiffValue = (100 - iDiffValue) * iBestWaterUnitValue * (100 + iMod);         // < 100 * iBestUnitValue * >=100
                                iDiffValue /= 100;                                                            // ~ 100 * iBestUnitValue
                                iWasterUnitUnlockValue = (iDiffValue * AI_YIELD_TURNS * pCity.calculateModifiedYield(infos.unit(eBestWaterUnit).meProductionType)) / (player.getUnitBuildCost(eBestWaterUnit, pCity) * Constants.YIELDS_MULTIPLIER * 200); // * 200: because water units are worth less than land
                            }

                            //cities aren't producing military units all the time
                            iLandUnitUnlockValue /= 2;
                            iWasterUnitUnlockValue /= 2;

                            if (iLandUnitUnlockValue + iWasterUnitUnlockValue > 0)
                            {
                                iValue += (Math.Max(iLandUnitUnlockValue, iWasterUnitUnlockValue + iLandUnitUnlockValue / 2)) / Math.Max(1, pCity.getImprovementCount(eImprovement) + (bRemove ? 0 : 1));
                            }
                        }

                    }

                    if (pImprovementInfo.miUnitTurns > 0)
                    {
                        long iUnitValue = 0;
                        long iWaterUnitValue = 0;
                        int iWaterDice = 0;
                        int iDiceTotal = 0;
                        for (UnitType eFreeUnit = 0; eFreeUnit < infos.unitsNum(); ++eFreeUnit)
                        {
                            int iDie = pImprovementInfo.maiUnitDie[eFreeUnit];
                            if (iDie > 0)
                            {
                                iDiceTotal += iDie;
                                if (isBuildUnitValid(pCity, pTile.getArea(), eFreeUnit, false, false, false))
                                {
                                    //if (isWarship(eFreeUnit))
                                    if (pTile.isWater() && infos.unit(eFreeUnit).mbWater)
                                    {
                                        iWaterDice += iDie;
                                        iWaterUnitValue += unitValue(eFreeUnit, pCity, pTile.getArea(), false) * iDie;
                                    }
                                    else
                                    {
                                        iUnitValue += unitValue(eFreeUnit, pCity, -1, false) * iDie;
                                    }
                                }


                            }
                        }
                        if (iDiceTotal > 0)
                        {
                            int iAdditionalShips = (AI_YIELD_TURNS * iWaterDice) / (pImprovementInfo.miUnitTurns * iDiceTotal);
                            if (iWaterUnitValue > 0 && getWaterUnitTargetNumber(pTile.getArea()) < iAdditionalShips)
                            {
                                iWaterUnitValue *= getWaterUnitTargetNumber(pTile.getArea());
                                iWaterUnitValue /= iAdditionalShips;
                            }
                            iValue = ((iUnitValue + iWaterUnitValue) * AI_YIELD_TURNS) / (iDiceTotal * pImprovementInfo.miUnitTurns);
                        }
                    }

                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        {
                            EffectCityType eEffectCity = infos.improvementClass(eImprovementClass).meEffectCity;

                            if (eEffectCity != EffectCityType.NONE)
                            {
                                iValue += effectCityValue(eEffectCity, pCity, bRemove);
                                cityEffectExtraYieldFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts, extraYields);
                                iValue += cityEffectExtraUnlockValueFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts);
                            }
                        }

                        if (pTile.hasResource())
                        {
                            {
                                EffectCityType eEffectCity = infos.improvementClass(eImprovementClass).maeResourceCityEffect[pTile.getResource()];

                                if (eEffectCity != EffectCityType.NONE)
                                {
                                    iValue += adjustForInflation(AI_RESOURCE_EXTRA_VALUE);
                                    iValue += effectCityValue(eEffectCity, pCity, bRemove);
                                    cityEffectExtraYieldFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts, extraYields);
                                    iValue += cityEffectExtraUnlockValueFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts);
                                }
                            }
                        }

                        if (pImprovementInfo.meReligionPrereq != ReligionType.NONE)
                        {
                            for (TheologyType eLoopTheology = 0; eLoopTheology < infos.theologiesNum(); eLoopTheology++)
                            {
                                EffectCityType eEffectCity = infos.improvementClass(eImprovementClass).maeTheologyCityEffect[eLoopTheology];

                                if (eEffectCity != EffectCityType.NONE)
                                {
                                    long iEffectValue = effectCityValue(eEffectCity, pCity, bRemove);
                                    cityEffectExtraYieldFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts, extraYields);
                                    iValue += cityEffectExtraUnlockValueFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts);
                                    if (game.isReligionTheology(pImprovementInfo.meReligionPrereq, eLoopTheology))
                                    {
                                        iValue += iEffectValue;
                                    }
                                    else if (game.canEstablishTheology(pImprovementInfo.meReligionPrereq, eLoopTheology))
                                    {
                                        iValue += iEffectValue / 2;
                                    }
                                }
                            }
                        }
                    }

                    if (!bRemove && player.getWorldReligionCount() == 0)
                    {
                        int iPlayerImprovements = player.getActiveImprovementCount(eImprovement);
                        int iPlayerImprovementClasses = eImprovementClass != ImprovementClassType.NONE ? player.getActiveImprovementClassCount(eImprovementClass) : 0;
                        for (ReligionType eLoopReligion = 0; eLoopReligion < infos.religionsNum(); ++eLoopReligion)
                        {
                            if (!game.isReligionFounded(eLoopReligion))
                            {
                                bool bImprovementNeeded = false;
                                if (infos.religion(eLoopReligion).maiRequiresImprovement[eImprovement] > iPlayerImprovements)
                                {
                                    bImprovementNeeded = true;
                                }

                                if (eImprovementClass != ImprovementClassType.NONE)
                                {
                                    if (infos.religion(eLoopReligion).maiRequiresImprovementClass[eImprovementClass] > iPlayerImprovementClasses)
                                    {
                                        bImprovementNeeded = true;
                                    }
                                }
                                if (bImprovementNeeded || (pImprovementInfo.meSpecialist != SpecialistType.NONE && game.isReligionSpecialist(pImprovementInfo.meSpecialist)))
                                {
                                    iValue += stateReligionValue(eLoopReligion, false, false);
                                }
                            }
                        }
                    }


                    if (pCity != null)
                    {
                        for (DirectionType eDir = 0; eDir < DirectionType.NUM_TYPES; ++eDir)
                        {
                            Tile pAdjacent = pTile.tileAdjacent(eDir);
                            if (pAdjacent != null && (pAdjacent.getTeam() == Team || !pAdjacent.hasOwner()))
                            {
                                using (var improvementListScoped = CollectionCache.GetListScoped<ImprovementType>())
                                {
                                    List<ImprovementType> aeImprovements = improvementListScoped.Value;
                                    bool bHasImprovement = false;
                                    if (pAdjacent.hasImprovement())
                                    {
                                        if (pAdjacent.getTeam() == Team)
                                        {
                                            bHasImprovement = true;
                                            aeImprovements.Add(pAdjacent.getImprovement());
                                        }
                                    }
                                    else if (pAdjacent.hasResource())
                                    {
                                        long iBestOutput = 0;
                                        ImprovementType eBestImprovement = ImprovementType.NONE;
                                        for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos.improvementsNum(); ++eLoopImprovement)
                                        {
                                            if (infos.Helpers.isImprovementResourceValid(eLoopImprovement, pAdjacent.getResource()))
                                            {
                                                long iOutputValue = 0;
                                                for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                                                {
                                                    int iYield = pAdjacent.yieldOutput(eLoopImprovement, SpecialistType.NONE, eLoopYield, pCity, bCityEffects: true, bBaseOnly: false);
                                                    if (iYield != 0)
                                                    {
                                                        iOutputValue += iYield * cityYieldValue(eLoopYield, pCity);
                                                        //no need to modify this, it's just for ranking
                                                    }
                                                }
                                                if (iOutputValue > iBestOutput)
                                                {
                                                    iBestOutput = iOutputValue;
                                                    eBestImprovement = eLoopImprovement;
                                                }
                                            }
                                        }
                                        if (eBestImprovement != ImprovementType.NONE)
                                        {
                                            bHasImprovement = true;
                                            aeImprovements.Add(eBestImprovement);
                                        }
                                    }
                                    else
                                    {
                                        //using var profileScopeAdjacent = new UnityProfileScope("AdjacentImprovementModifier");

                                        if (pImprovementInfo.maeAdjacentImprovementSpecialists.Count > 0)
                                        {
                                            aeImprovements.AddRange(pImprovementInfo.maeAdjacentImprovementSpecialists);
                                        }
                                        else
                                        {
                                            foreach (KeyValuePair<ImprovementType, int> p in pImprovementInfo.maiAdjacentImprovementModifier)
                                            {
                                                aeImprovements.Add(p.Key);
                                            }
                                            foreach (KeyValuePair<ImprovementClassType, int> p in pImprovementInfo.maiAdjacentImprovementClassModifier)
                                            {
                                                InfoImprovementClass improvementClass = infos.improvementClass(p.Key);
                                                infos.Helpers.getImprovementClassImprovements(improvementClass.meType, aeImprovements);
                                            }
                                            foreach ((ImprovementClassType, YieldType, int) p in pImprovementInfo.maaiAdjacentImprovementClassYield)
                                            {
                                                InfoImprovementClass improvementClass = infos.improvementClass(p.Item1);
                                                infos.Helpers.getImprovementClassImprovements(improvementClass.meType, aeImprovements);
                                            }
                                            foreach ((ImprovementType, YieldType, int) p in pImprovementInfo.maaiAdjacentImprovementYield)
                                            {
                                                aeImprovements.Add(p.Item1);
                                            }
                                            if (pImprovementInfo.meClass != ImprovementClassType.NONE)
                                            {
                                                foreach (KeyValuePair<ImprovementClassType, int> p in infos.improvementClass(pImprovementInfo.meClass).maiAdjacentImprovementClassModifier)
                                                {
                                                    InfoImprovementClass improvementClass = infos.improvementClass(p.Key);
                                                    infos.Helpers.getImprovementClassImprovements(improvementClass.meType, aeImprovements);
                                                }
                                            }

                                            // remove duplicates
                                            using (var improvementSetScoped = CollectionCache.GetHashSetScoped<ImprovementType>())
                                            {
                                                HashSet<ImprovementType> seImprovementsDone = improvementSetScoped.Value;
                                                for (int i = aeImprovements.Count - 1; i >= 0; --i)
                                                {
                                                    ImprovementType eLoopImprovement = aeImprovements[i];
                                                    if (seImprovementsDone.Contains(eLoopImprovement))
                                                    {
                                                        aeImprovements.RemoveAt(i);
                                                    }
                                                    seImprovementsDone.Add(eLoopImprovement);
                                                }
                                            }
                                        }
                                    }

                                    if (aeImprovements.Count > 0)
                                    {
                                        long iAdjacentValue = 0;
                                        foreach (ImprovementType eLoopImprovement in aeImprovements)
                                        {
                                            if (pAdjacent.getImprovement() == eLoopImprovement || pAdjacent.isImprovementValid(eLoopImprovement, pAdjacent.cityTerritory()))
                                            {
                                                for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); eLoopYield++)
                                                {
                                                    City pAdjacentCity = pAdjacent.cityTerritory() ?? pCity;
                                                    int iModifier = pCity?.calculateTotalYieldModifier(eLoopYield) ?? 0;
                                                    int iAdjacentModifier = (pAdjacentCity == pCity) ? iModifier : (pAdjacentCity?.calculateTotalYieldModifier(eLoopYield) ?? 0);

                                                    if (pAdjacentCity.getTeam() == Team)
                                                    {
                                                        int iYieldToAdjacent = infos.utils().modify(getYieldToAdjacent(eLoopYield, eLoopImprovement, eImprovement, pTile.getResource()), iModifier);
                                                        if (iYieldToAdjacent != 0)
                                                        {
                                                            iAdjacentValue += iYieldToAdjacent * cityYieldValue(eLoopYield, pCity);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        iAdjacentValue /= aeImprovements.Count;

                                        if (!bHasImprovement)
                                        {
                                            iAdjacentValue /= 2;
                                        }
                                        iAdjacentValue /= Constants.YIELDS_MULTIPLIER;
                                        iValue += iAdjacentValue * AI_YIELD_TURNS;
                                    }
                                }
                            }
                        }
                    }


                    //SpecialistType eImprovementSpecialist = ((bRemove || pTile.isSpecialistValid(pTile.getSpecialist(), eImprovement)) ? pTile.getSpecialist() : SpecialistType.NONE);
                    SpecialistType eImprovementSpecialist;
                    if (bRemove || pTile.isSpecialistValid(pTile.getSpecialist(), eImprovement))
                    {
                        eImprovementSpecialist = pTile.getSpecialist();
                    }
                    else
                    {
                        eImprovementSpecialist = pTile.getFreeSpecialist(eImprovement);

                        if (pTile.getSpecialist() != SpecialistType.NONE && pTile.cityTerritory() != null && pCity == pTile.cityTerritory()) //specialists can only exist in city territory, so this is probably unnecessary
                        {
                            addSpecialistCityEffectCounts(pTile.getSpecialist(), pTile, dEffectCityExtraCounts, bRemove: true);
                        }
                    }

                    for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); eLoopYield++)
                    {
/*####### Better Old World AI - Base DLL #######
  ### AI: proper yield modifiers       START ###
  ##############################################*/
                        //long iTileOutputValue = pTile.yieldOutput(eImprovement, eImprovementSpecialist, eLoopYield, pCityEffects: null, bBaseOnly: false);
                        //long iTileOutputValue = pTile.yieldOutput(eImprovement, SpecialistType.NONE, eLoopYield, pCityEffects: null, bBaseOnly: false);
                        long iTileOutputValue = ((BetterAITile)pTile).yieldOutputForGovernor(eImprovement, SpecialistType.NONE, eLoopYield, pCity, bCityEffects: false, bBaseOnly: false, bCost: true, pCity.governor(), bTheology: true, newImprovements: null, dEffectCityExtraCounts);

                        //if (infos.yield(eLoopYield).miPerImprovement != 0)
                        //{
                        //    iTileOutputValue += infos.utils().modify(infos.yield(eLoopYield).miPerImprovement, pCity.calculateTotalYieldModifier(eLoopYield));
                        //}
                        iTileOutputValue += infos.yield(eLoopYield).miPerImprovement;

                        if (iTileOutputValue != 0)
                        {
                            //iTileOutputValue *= cityYieldValue(eLoopYield, pCity);
                            //iTileOutputValue /= Constants.YIELDS_MULTIPLIER;
                            //iValue += iTileOutputValue * AI_YIELD_TURNS;

                            int iExtraModifier = 0;
                            foreach (KeyValuePair<EffectCityType, int> p in dEffectCityExtraCounts)
                            {
                                iExtraModifier += infos.effectCity(p.Key).maiYieldModifier[eLoopYield] * p.Value;
                            }

                            iTileOutputValue = infos.utils().modify((iTileOutputValue + extraYields[(int)eLoopYield]), (pCity?.calculateTotalYieldModifier(eLoopYield) ?? 0) + iExtraModifier);
                            
                            if (eLoopYield == infos.Globals.CIVICS_YIELD)
                            {
                                if (eImprovementSpecialist != SpecialistType.NONE)
                                {
                                    iExtraCivicsProduction = ((BetterAITile)pTile).yieldOutputForGovernor(eImprovement, eImprovementSpecialist, eLoopYield, pCity, bCityEffects: false, bBaseOnly: false, bCost: true, pCity.governor(), bTheology: true, newImprovements: null, dEffectCityExtraCounts);
                                    iExtraCivicsProduction += infos.yield(eLoopYield).miPerImprovement;
                                    iExtraCivicsProduction = infos.utils().modify((iExtraCivicsProduction + extraYields[(int)eLoopYield]), (pCity?.calculateTotalYieldModifier(eLoopYield) ?? 0) + iExtraModifier);
                                }
                                else
                                {
                                    iExtraCivicsProduction = (int)iTileOutputValue;
                                }
                            }

                            iValue += iTileOutputValue * cityYieldValue(eLoopYield, pCity) * AI_YIELD_TURNS / Constants.YIELDS_MULTIPLIER;
                        }
/*####### Better Old World AI - Base DLL #######
  ### AI: proper yield modifiers         END ###
  ##############################################*/
                    }

                    //full value for existing specialists, and free specialists that get added automatically
                    //needs to be removed from improvementValueTile, section bSubtractCurrent
                    if (eImprovementSpecialist != SpecialistType.NONE)
                    {
                        bool bIncludeBorderExpansion = !bRemove && !pTile.isUrban() && !(pTile.getSpecialist() != SpecialistType.NONE) && !pImprovementInfo.mbUrban; //true only for a new free specialist
                        iValue += specialistValue(eImprovementSpecialist, pCity, pTile, pTile.getImprovement(), bIncludeCost: false, bIncludeUnlock: false, bIncludeBorderExpansion: bIncludeBorderExpansion);
                    }

                }
                //end scope here

                // encourage new improvements that allow good specialists
/*####### Better Old World AI - Base DLL #######
  ### AI: less value for specialist    START ###
  ##############################################*/
                if (pCity != null && pImprovementInfo.meSpecialist != SpecialistType.NONE && (pTile.getSpecialist() == SpecialistType.NONE || !pTile.isSpecialistValid(pTile.getSpecialist(), eImprovement)))
                {
                    //There is value in having the option to build another specialist. 
                    //This value is reduced if there are already other specialist build options present, or if citizens can also be used to rush productions.

                    long iSpecialistBuildValue = getSpecialistBuildValue(pImprovementInfo.meSpecialist, pTile, pCity, 
                        Math.Max(1, pCity.calculateModifiedYield(infos.Globals.CIVICS_YIELD) + iExtraCivicsProduction), 
                        player.getSpecialistBuildCost(pImprovementInfo.meSpecialist, pCity, eImprovement), eImprovement, bIncludeUnlock: true);

                    long iCurrentlyAvailableOptionBuildValue;
                    using var buildListScoped = CollectionCache.GetListScoped<BuildValue>();
                    List<BuildValue> azBuildValues = buildListScoped.Value;
                    int iNumBuildsDivisor;
                    if (pCity.isHurryPopulation() || pCity.isHurryPopulation(infos.Globals.UNIT_BUILD) && pCity.isHurryPopulation(infos.Globals.SPECIALIST_BUILD) && pCity.isHurryPopulation(infos.Globals.PROJECT_BUILD))
                    {
                        iNumBuildsDivisor = 6;  //arbitrary
                    }
                    else
                    {
                        iNumBuildsDivisor = (2 + (pCity.isHurryPopulation(infos.Globals.UNIT_BUILD) ? 3 : 0) + (pCity.isHurryPopulation(infos.Globals.SPECIALIST_BUILD) ? 1 : 0) + (pCity.isHurryPopulation(infos.Globals.PROJECT_BUILD) ? 1 : 0));  //arbitrary
                    }
                    int iNumBuilds = 1 + ((pCity.getCitizens() * 2 - 1) / iNumBuildsDivisor);  //arbitrary

                    //This part probably costs a lot of CPU cycles, since this means all cities cache all specialist values too when caching improvement values
                    getBestBuild(pCity, infos.Globals.SPECIALIST_BUILD, bBuyGoods: true, bTestEnabled: false, bTestGoods: false, iNumBuilds: iNumBuilds, azBuildValues, bIgnoreDanger: true);

                    long iSumBuildValue = 0;
                    int iModifier = 0;
                    long iSubValue = iSpecialistBuildValue / 20;  //arbitrary
                    int iRemoveCount = (bRemove ? 1 : 0);

                    if (azBuildValues.Count > iRemoveCount)
                    {
                        iCurrentlyAvailableOptionBuildValue = azBuildValues[0].iValue;
                        foreach (BuildValue eLoopBuildValue in azBuildValues)
                        {
                            iSumBuildValue += eLoopBuildValue.iValue;
                        }

                        if (azBuildValues.Count < iNumBuilds)
                        {
                            iModifier = (30 + 10 * (iNumBuilds - azBuildValues.Count - 1 + iRemoveCount)) / iNumBuildsDivisor;  //15% extra for 1 citizen more than builds if there is no option to hurry with population, +5% for every citizen beyond that  //arbitrary
                        }

                    }
                    else
                    {
                        iSumBuildValue = ( iSpecialistBuildValue * (19 - Math.Min(9, (iNumBuilds - 1 + iRemoveCount) * 2)) ) / 20;  //the higher the number of citizens, the lower iSumBuildValue gets, which increases value (max 1/2)  //arbitrary
                    }
                    iSubValue += Math.Max(0, iSpecialistBuildValue - (iSumBuildValue / Math.Max(1, azBuildValues.Count - iRemoveCount))) / 2;  //arbitrary

                    iValue += infos.utils().modify(iSubValue, iModifier);
                }
/*####### Better Old World AI - Base DLL #######
  ### AI: less value for specialist      END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement       START ###
  ##############################################*/
                //if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE || pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                //{
                //    UnityEngine.Debug.Log("BonusAdjacentImprovement: AI eval start");
                //}
                BetterAITile BAI_pTile = (BetterAITile)pTile;
                BetterAICity BAI_pCity = (BetterAICity)pCity;

                ImprovementType eBonusImprovement = ImprovementType.NONE;

                long iBonusImprovementValue = int.MinValue;
                bool bImprovementSpreadsBorders = ((BetterAIInfoHelpers)(infos.Helpers)).improvementSpreadsBorders(eImprovement, game, player, pTile);

                if (!(pImprovementInfo.mbMakesAdjacentPassableLandTileValidForBonusImprovement))
                {
                    if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE)
                    {
                        if (BAI_pCity == null) //invalid
                        {
                            UnityEngine.Debug.Log("Invalid: City is null, meBonusAdjacentImprovement is not NONE");
                            return -1;
                        }

                        if (BAI_pTile.canCityTileAddImprovementAdjacent(BAI_pCity, pImprovementInfo.meBonusAdjacentImprovement, bImprovementSpreadsBorders))
                        {
                            eBonusImprovement = pImprovementInfo.meBonusAdjacentImprovement;
                        }
                    }
                    else if (pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                    {
                        if (BAI_pCity == null) //invalid
                        {
                            UnityEngine.Debug.Log("Invalid: City is null, meBonusAdjacentImprovementClass is not NONE");
                            return -1;
                        }

                        BAI_pTile.canCityTileAddImprovementClassAdjacent(BAI_pCity, pImprovementInfo.meBonusAdjacentImprovementClass, bImprovementSpreadsBorders, out eBonusImprovement);
                    }

                    if (eBonusImprovement != ImprovementType.NONE && BAI_pCity.canCityHaveImprovement(eBonusImprovement, bTestReligion: true, bForceImprovement: true))
                    {
                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                        {
                            BetterAITile pAdjacentTile = (BetterAITile)pTile.tileAdjacent(eLoopDirection);

                            if (pAdjacentTile != null)
                            {
                                if (pAdjacentTile.getImprovement() == ImprovementType.NONE
                                    && (!pAdjacentTile.hasResource() || infos.Helpers.isImprovementResourceValid(eBonusImprovement, pAdjacentTile.getResource()))
                                    && pAdjacentTile.cityTerritory() == pTile.cityTerritory() || (bImprovementSpreadsBorders && pAdjacentTile.cityTerritory() == null)
                                    && pAdjacentTile.canGeneralTileHaveImprovement(eBonusImprovement, bForceImprovement: true)
                                    && pAdjacentTile.canCityTileHaveImprovement(BAI_pCity, eBonusImprovement, bForceImprovement: true))
                                {
                                    long iLoopValue = calculateImprovementValueForTile(pAdjacentTile, pCity, eBonusImprovement);
                                    if (iLoopValue > iBonusImprovementValue)
                                    {
                                        iBonusImprovementValue = iLoopValue;
                                    }
                                }
                            }
                        }
                    }

                }
                else
                {
                    //ImprovementType eTestImprovement = ImprovementType.NONE;
                    if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE)
                    {
                        if (BAI_pCity == null) //invalid
                        {
                            UnityEngine.Debug.Log("Invalid: City is null, meBonusAdjacentImprovement is not NONE");
                            return -1;
                        }

                        eBonusImprovement = pImprovementInfo.meBonusAdjacentImprovement;

                        if (!(BAI_pCity.canCityHaveImprovement(pImprovementInfo.meBonusAdjacentImprovement, bTestTerritory: false, bTestReligion: true, bForceImprovement: true)))
                        {
                            UnityEngine.Debug.Log("AI considering an invalid improvement (meBonusAdjacentImprovement)");
                            return -1;
                        }
                    }

                    if (eBonusImprovement == ImprovementType.NONE && pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                    {
                        if (BAI_pCity == null) //invalid
                        {
                            //UnityEngine.Debug.Log("Invalid: City is null, meBonusAdjacentImprovementClass is not NONE");
                            return -1;
                        }

                        for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos.improvementsNum(); eLoopImprovement++)
                        {
                            if (infos.improvement(eLoopImprovement).meClass == pImprovementInfo.meBonusAdjacentImprovementClass)
                            {
                                if (BAI_pCity.canCityHaveImprovement(eLoopImprovement, bTestTerritory: false, bTestReligion: true, bForceImprovement: true))
                                {
                                    eBonusImprovement = eLoopImprovement;
                                    break;
                                }
                            }

                            if (eBonusImprovement == ImprovementType.NONE)
                            {
                                //System.Diagnostics.StackTrace t = new System.Diagnostics.StackTrace();
                                //UnityEngine.Debug.Log("AI considering an invalid improvement (meBonusAdjacentImprovementClass) " + t.ToString());
                                return -1;
                            }
                        }
                    }

                    if (eBonusImprovement != ImprovementType.NONE)
                    {
                        //only land improvements get mbMakesAdjacentPassableLandTileValidForBonusImprovement
                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                        {
                            BetterAITile pAdjacentTile = (BetterAITile)pTile.tileAdjacent(eLoopDirection);

                            if (pAdjacentTile != null)
                            {
                                if (pAdjacentTile.isLand() && !(pAdjacentTile.impassable())
                                    && pAdjacentTile.getImprovement() == ImprovementType.NONE
                                    && (!pAdjacentTile.hasResource() || infos.Helpers.isImprovementResourceValid(eBonusImprovement, pAdjacentTile.getResource()))
                                    && pAdjacentTile.cityTerritory() == pTile.cityTerritory() || (bImprovementSpreadsBorders && pAdjacentTile.cityTerritory() == null)
                                    && pAdjacentTile.canGeneralTileHaveImprovement(eBonusImprovement, bForceImprovement: true)
                                    && pAdjacentTile.canCityTileHaveImprovement(BAI_pCity, eBonusImprovement, bForceImprovement: true))
                                {
                                    long iLoopValue = calculateImprovementValueForTile(pAdjacentTile, pCity, eBonusImprovement);
                                    if (iLoopValue > iBonusImprovementValue)
                                    {
                                        iBonusImprovementValue = iLoopValue;
                                    }
                                }
                            }
                        }
                    }

                }

                if (eBonusImprovement != ImprovementType.NONE && iBonusImprovementValue != int.MinValue)
                {
                    iValue += iBonusImprovementValue;
                }
                
                //if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE || pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                //{
                //    UnityEngine.Debug.Log("BonusAdjacentImprovement: AI eval end");
                //}
/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement         END ###
  ##############################################*/

                if (pTile.getImprovement() == eImprovement && pTile.isPillaged())
                {
                    for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                    {
                        iValue += (infos.Helpers.getBuildCost(eImprovement, eLoopYield, pTile) - player.getRepairCost(eLoopYield, pTile)) * yieldValue(eLoopYield);
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### Max vegetation remove value 0    START ###
  ##############################################*/
                {
                    long iSubValue = 0;
                    if (pImprovementInfo.mbNoVegetation && pTile.hasVegetation())
                    {
                        for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                        {
                            //getYieldRemove part moved to improvementValue - bIcludeCost
                            //int iYieldAmount = player.getYieldRemove(pTile, eLoopYield, true, pCity);

                            int iYieldAmount = -getYieldLostFromClear(pTile, pCity, eLoopYield, pTile.getVegetation());
                            if (pTile.vegetation().meVegetationRemove != VegetationType.NONE)
                            {
                                iYieldAmount -= getYieldLostFromClear(pTile, pCity, eLoopYield, pTile.vegetation().meVegetationRemove);
                            }
                            iSubValue += iYieldAmount * yieldValue(eLoopYield);
                        }

                        //extra order cost from vegetation's iBuildCost is in improvementValue - bIcludeCost
                        //iSubValue -= pTile.vegetation().miBuildCost * yieldValue(infos.Globals.ORDERS_YIELD);

                        iValue += Math.Min(0, iSubValue);
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### Max vegetation remove value 0      END ###
  ##############################################*/

                iValue += getImprovementFamilyOpinionValue(pCity, eImprovement);

                if (!bRemove && (pImprovementInfo.mbUrban || pImprovementInfo.mbRoadFree))
                {
                    if (pCity != null)
                    {
                        if (cityNeedsTradeNetworkImprovement(pCity, pTile))
                        {
                            iValue += effectCityValue(infos.Globals.CONNECTED_EFFECTCITY, pCity, false);

                            if (pCity.hasFamily() && game.familyClass(pCity.getFamily()).miConnectedOpinion != 0)
                            {
                                iValue += getFamilyOpinionValue(pCity.getFamily(), game.familyClass(pCity.getFamily()).miConnectedOpinion);
                            }
                        }
                    }
                    else
                    {
                        iValue += adjustForInflation(AI_TRADE_NETWORK_VALUE_ESTIMATE);
                    }
                }

                if (pImprovementInfo.meEffectPlayer != EffectPlayerType.NONE)
                {
                    iValue += effectPlayerValue(pImprovementInfo.meEffectPlayer, player.getStateReligion(), bRemove);
                }

                if (isFort(eImprovement))
                {
                    iValue += getFortValue(eImprovement, pTile);
                }

                // 1-tile canal when no connection is there. todo: also for shortcuts
                if (pImprovementInfo.mbCanal)
                {
                    bool bFound = false;
                    for (DirectionType eDir = 0; !bFound && eDir < DirectionType.NUM_TYPES - 1; ++eDir)
                    {
                        Tile pAdjacent = pTile.tileAdjacent(eDir);
                        if (pAdjacent != null && pAdjacent.isWater())
                        {
                            for (DirectionType eDir2 = eDir + 1; !bFound && eDir2 < DirectionType.NUM_TYPES; ++eDir2)
                            {
                                Tile pAdjacent2 = pTile.tileAdjacent(eDir2);
                                if (pAdjacent2 != null && pAdjacent2.isWater())
                                {
                                    if (!mpAICache.isTileReachableFromWater(pAdjacent.getID(), pAdjacent2.getID()))
                                    {
                                        iValue += adjustForInflation(AI_CANAL_VALUE);
                                        bFound = true;
                                    }
                                }
                            }
                        }
                    }
                }

                //worthless
                //if (pImprovementInfo.mbIgnoreZOC)
                //{
                //    iValue += adjustForInflation(AI_UNIT_ZOC_VALUE);
                //}


                return Math.Max(0, iValue);
            }


            public virtual void addImprovementCityEffectCounts(ImprovementType eImprovement, Tile pTile, Dictionary<EffectCityType, int> dEffectCityCounts, bool bRemove = false)
            {
                InfoImprovement pImprovementInfo = infos.improvement(eImprovement);
                ImprovementClassType eImprovementClass = pImprovementInfo.meClass;
                int iExtraCount = (bRemove ? -1 : 1);

                {
                    EffectCityType eEffectCity = pImprovementInfo.meEffectCity;

                    if (eEffectCity != EffectCityType.NONE)
                    {
                        dEffectCityCounts[eEffectCity] = dEffectCityCounts.GetOrDefault(eEffectCity, 0) + iExtraCount;
                    }
                }

                if (eImprovementClass != ImprovementClassType.NONE)
                {
                    InfoImprovementClass improvementClass = infos.improvementClass(eImprovementClass);
                    {
                        EffectCityType eEffectCity = improvementClass.meEffectCity;

                        if (eEffectCity != EffectCityType.NONE)
                        {
                            dEffectCityCounts[eEffectCity] = dEffectCityCounts.GetOrDefault(eEffectCity, 0) + iExtraCount;
                        }
                    }

                    if (pTile.hasResource())
                    {
                        {
                            EffectCityType eEffectCity = improvementClass.maeResourceCityEffect[pTile.getResource()];

                            if (eEffectCity != EffectCityType.NONE)
                            {
                                dEffectCityCounts[eEffectCity] = dEffectCityCounts.GetOrDefault(eEffectCity, 0) + iExtraCount;
                            }
                        }
                    }

                    if (pImprovementInfo.meReligionPrereq != ReligionType.NONE)
                    {
                        for (TheologyType eLoopTheology = 0; eLoopTheology < infos.theologiesNum(); eLoopTheology++)
                        {
                            EffectCityType eEffectCity = improvementClass.maeTheologyCityEffect[eLoopTheology];

                            if (eEffectCity != EffectCityType.NONE)
                            {
                                if (game.isReligionTheology(pImprovementInfo.meReligionPrereq, eLoopTheology))
                                {
                                    dEffectCityCounts[eEffectCity] = dEffectCityCounts.GetOrDefault(eEffectCity, 0) + iExtraCount;
                                }
                            }
                        }
                    }

                }

                if (pImprovementInfo.meEffectPlayer != EffectPlayerType.NONE)
                {
                    EffectCityType eEffectCity = infos.effectPlayer(pImprovementInfo.meEffectPlayer).meCapitalEffectCity;
                    if (eEffectCity != EffectCityType.NONE)
                    {
                        dEffectCityCounts[eEffectCity] = dEffectCityCounts.GetOrDefault(eEffectCity, 0) + iExtraCount;
                    }

                    eEffectCity = infos.effectPlayer(pImprovementInfo.meEffectPlayer).meEffectCity;
                    if (eEffectCity != EffectCityType.NONE)
                    {
                        dEffectCityCounts[eEffectCity] = dEffectCityCounts.GetOrDefault(eEffectCity, 0) + iExtraCount;
                    }

                    eEffectCity = infos.effectPlayer(pImprovementInfo.meEffectPlayer).meEffectCityExtra;
                    if (eEffectCity != EffectCityType.NONE)
                    {
                        dEffectCityCounts[eEffectCity] = dEffectCityCounts.GetOrDefault(eEffectCity, 0) + iExtraCount;
                    }
                }

            }

            public virtual void addSpecialistCityEffectCounts(SpecialistType eSpecialist, Tile pTile, Dictionary<EffectCityType, int> dEffectCityCounts, bool bRemove = false)
            {
                InfoSpecialist specialist = infos.specialist(eSpecialist);
                SpecialistClassType eSpecialistClass = specialist.meClass;
                int iExtraCount = (bRemove ? -1 : 1);

                {
                    EffectCityType eEffectCity = specialist.meEffectCity;
                    if (eEffectCity != EffectCityType.NONE)
                    {
                        dEffectCityCounts[eEffectCity] = dEffectCityCounts.GetOrDefault(eEffectCity, 0) + iExtraCount;
                    }
                }

                {
                    EffectCityType eEffectCity = specialist.meEffectCityExtra;
                    if (eEffectCity != EffectCityType.NONE)
                    {
                        dEffectCityCounts[eEffectCity] = dEffectCityCounts.GetOrDefault(eEffectCity, 0) + iExtraCount;
                    }
                }

                if (eSpecialistClass != SpecialistClassType.NONE)
                {
                    EffectCityType eEffectCity = infos.specialistClass(eSpecialistClass).meEffectCity;
                    if (eEffectCity != EffectCityType.NONE)
                    {
                        dEffectCityCounts[eEffectCity] = dEffectCityCounts.GetOrDefault(eEffectCity, 0) + iExtraCount;
                    }
                }

                if (pTile != null && pTile.hasResource())
                {
                    EffectCityType eResourceEffectCity = infos.specialistClass(eSpecialistClass).maeResourceCityEffect[pTile.getResource()];

                    if (eResourceEffectCity != EffectCityType.NONE)
                    {
                        dEffectCityCounts[eResourceEffectCity] = dEffectCityCounts.GetOrDefault(eResourceEffectCity, 0) + iExtraCount;
                    }
                }

            }
            public virtual void cityEffectExtraYieldFromExtraCityEffects(EffectCityType eEffectCity, City pCity, Dictionary<EffectCityType, int> dEffectCityExtraCounts, List<int> extraYields)
            {
                InfoEffectCity infoEffectCity = infos.effectCity(eEffectCity);

                foreach ((EffectCityType, YieldType, int) zTriple in infoEffectCity.maaiEffectCityYieldRate)
                {
                    int iCount = dEffectCityExtraCounts.GetOrDefault(zTriple.Item1, 0);
                    if (iCount > 0)
                    {
                        extraYields[(int)zTriple.Item2] += iCount * zTriple.Item3;
                    }
                }

                foreach (KeyValuePair<EffectCityType, int> p in dEffectCityExtraCounts)
                {
                    foreach ((EffectCityType, YieldType, int) zTriple in infos.effectCity(p.Key).maaiEffectCityYieldRate)
                    {
                        int iCount = dEffectCityExtraCounts.GetOrDefault(zTriple.Item1, 0);
                        if (zTriple.Item1 == eEffectCity && iCount > 0)
                        {
                            extraYields[(int)zTriple.Item2] += iCount * zTriple.Item3;
                        }
                    }
                }
            }
            public virtual long cityEffectExtraUnlockValueFromExtraCityEffects(EffectCityType eEffectCity, City pCity, Dictionary<EffectCityType, int> dEffectCityExtraCounts, bool bRemove = false)
            {
                long iValue = 0;

                if (pCity == null)
                {
                    return 0;
                }

                int iExtraCount = bRemove ? -1 : 1;
                int iAdditionalExtraCount;
                BetterAIInfoEffectCity pEffectCityInfo = (BetterAIInfoEffectCity)infos.effectCity(eEffectCity);

                if (dEffectCityExtraCounts != null && dEffectCityExtraCounts.Count() != 0)
                {
                    if (pEffectCityInfo.mbEnablesGovernor && game.isCharacters())
                    {
                        iAdditionalExtraCount = 0;
                        foreach (KeyValuePair<EffectCityType, int> p in dEffectCityExtraCounts)
                        {
                            if (((BetterAIInfoEffectCity)infos.effectCity(p.Key)).mbEnablesGovernor)
                            {
                                iAdditionalExtraCount += p.Value;
                            }
                        }

                        //And this, hopefully, enables the AI to upgrade a Garrison to a Stronghold
                        if ((((BetterAICity)pCity).isEnablesGovernor() != ((BetterAICity)pCity).isEnablesGovernor(iExtraCount))
                            != (((BetterAICity)pCity).isEnablesGovernor(iAdditionalExtraCount) != ((BetterAICity)pCity).isEnablesGovernor(iExtraCount + iAdditionalExtraCount)))
                        {
                            iValue += AI_CITY_GOVERNOR_VALUE * (iAdditionalExtraCount < 0 ? 1 : -1);
                        }
                    }

                    //add here: all other unlock conditions


                }

                return iValue;
            }

            //unused
            public virtual long cityEffectExtraValueFromExtraCityEffects(EffectCityType eEffectCity, City pCity, Dictionary<EffectCityType, int> dEffectCityExtraCounts)
            {
                long iValue = 0;
                InfoEffectCity infoEffectCity = infos.effectCity(eEffectCity);

                foreach ((EffectCityType, YieldType, int) zTriple in infoEffectCity.maaiEffectCityYieldRate)
                {
                    int iCount = dEffectCityExtraCounts.GetOrDefault(zTriple.Item1, 0);
                    if (iCount > 0)
                    {
                        iValue += infos.utils().modify((iCount * zTriple.Item3 * cityYieldValue(zTriple.Item2, pCity)), pCity.calculateTotalYieldModifier(zTriple.Item2)) * AI_YIELD_TURNS / Constants.YIELDS_MULTIPLIER;
                    }
                }

                foreach (KeyValuePair<EffectCityType, int> p in dEffectCityExtraCounts)
                {
                    foreach ((EffectCityType, YieldType, int) zTriple in infos.effectCity(p.Key).maaiEffectCityYieldRate)
                    {
                        int iCount = dEffectCityExtraCounts.GetOrDefault(zTriple.Item1, 0);
                        if (zTriple.Item1 == eEffectCity && iCount > 0)
                        {
                            iValue += infos.utils().modify((iCount * zTriple.Item3 * cityYieldValue(zTriple.Item2, pCity)), pCity.calculateTotalYieldModifier(zTriple.Item2)) * AI_YIELD_TURNS / Constants.YIELDS_MULTIPLIER;
                        }
                    }
                }
                return iValue;
            }



            //old version
            protected override long calculateImprovementDependentValueForTile(Tile pTile, City pCity, ImprovementType eImprovement)
            {
                long iImprovementValue = Math.Max(0, improvementBonusValue(eImprovement, pCity, pTile));

                InfoImprovement improvement = infos.improvement(eImprovement);
                long iUrbanValue = Math.Max(0, getUrbanValue(pTile, improvement.mbUrban, pTile.isImprovementBorderSpread(eImprovement), false, pCity));
                if (improvement.mbUrban && !improvement.mbRequiresUrban)
                {
                    iUrbanValue *= 2;
                }
                iImprovementValue += iUrbanValue;

                if (improvement.meReligionSpread != ReligionType.NONE && !game.isReligionFounded(improvement.meReligionSpread))
                {
                    iImprovementValue += Math.Max(0, religionValue(improvement.meReligionSpread, false, pCity, true, true, false));
                }

                MohawkAssert.Assert(iImprovementValue >= 0, "negative improvement value");
                return iImprovementValue;
            }

            //lines 11727-11898
            protected override long calculateSpecialistValue(SpecialistType eSpecialist, City pCity, Tile pTile, ImprovementType eImprovement)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.calculateSpecialistValue");

                if (player == null)
                {
                    return 0;
                }

                bool bRemove = pTile.getSpecialist() == eSpecialist;
                SpecialistClassType eSpecialistClass = infos.specialist(eSpecialist).meClass;

                long iValue = 0;

/*####### Better Old World AI - Base DLL #######
  ### AI: less value for specialist    START ###
  ##############################################*/
                //moved to the start so that extra city effects from improvement can be part of the equation
                using (var effectCityCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
                using (var effectCityExtraCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
                using (var effectCityImprovementOnlyExtraCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
                using (var effectCityExtraYieldsScoped = CollectionCache.GetListScoped<int>())
                {
                    Dictionary<EffectCityType, int> dEffectCityCounts = effectCityCountsScoped.Value;
                    Dictionary<EffectCityType, int> dEffectCityExtraCounts = effectCityExtraCountsScoped.Value;
                    Dictionary<EffectCityType, int> dEffectCityImprovementOnlyExtraCounts = effectCityImprovementOnlyExtraCountsScoped.Value;
                    List<int> extraYields = effectCityExtraYieldsScoped.Value;
                    for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                    {
                        extraYields.Add(0);
                    }

                    pCity.getEffectCityCountsForGovernor(pCity.governor(), dEffectCityCounts);

                    //extra city effect counts from improvement
                    if (pTile.getImprovement() != eImprovement)
                    {
                        addImprovementCityEffectCounts(eImprovement, pTile, dEffectCityExtraCounts);
                        if (pTile.getImprovement() != ImprovementType.NONE && pCity == pTile.cityTerritory())
                        {
                            addImprovementCityEffectCounts(pTile.getImprovement(), pTile, dEffectCityExtraCounts, bRemove: true);
                        }
                    }

                    foreach (KeyValuePair<EffectCityType, int> p in dEffectCityExtraCounts)
                    {
                        dEffectCityCounts[p.Key] = dEffectCityCounts.GetOrDefault(p.Key, 0) + p.Value;
                        dEffectCityImprovementOnlyExtraCounts[p.Key] = dEffectCityCounts.GetOrDefault(p.Key, 0) + p.Value;
                    }

                    //assumption: city effects of a single type of specialist don't interact with each other, so addSpecialistCityEffectCounts can wait
                    {
                        EffectCityType eEffectCity = infos.specialist(eSpecialist).meEffectCity;
                        if (eEffectCity != EffectCityType.NONE)
                        {
                            iValue += effectCityValue(eEffectCity, pCity, bRemove);
                            cityEffectExtraYieldFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts, extraYields);
                            iValue += cityEffectExtraUnlockValueFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts);
                        }
                    }

                    {
                        EffectCityType eEffectCity = infos.specialist(eSpecialist).meEffectCityExtra;
                        if (eEffectCity != EffectCityType.NONE)
                        {
                            iValue += effectCityValue(eEffectCity, pCity, bRemove);
                            cityEffectExtraYieldFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts, extraYields);
                            iValue += cityEffectExtraUnlockValueFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts);
                        }
                    }

                    if (eSpecialistClass != SpecialistClassType.NONE)
                    {
                        EffectCityType eEffectCity = infos.specialistClass(eSpecialistClass).meEffectCity;
                        if (eEffectCity != EffectCityType.NONE)
                        {
                            iValue += effectCityValue(eEffectCity, pCity, bRemove);
                            cityEffectExtraYieldFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts, extraYields);
                            iValue += cityEffectExtraUnlockValueFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts);
                        }
                    }


                    if (pTile != null)
                    {
                        if (pTile.hasResource())
                        {
                            EffectCityType eResourceEffectCity = infos.specialistClass(eSpecialistClass).maeResourceCityEffect[pTile.getResource()];

                            if (eResourceEffectCity != EffectCityType.NONE)
                            {
                                iValue += effectCityValue(eResourceEffectCity, pCity, bRemove);
                                cityEffectExtraYieldFromExtraCityEffects(eResourceEffectCity, pCity, dEffectCityExtraCounts, extraYields);
                                iValue += cityEffectExtraUnlockValueFromExtraCityEffects(eResourceEffectCity, pCity, dEffectCityExtraCounts);
                            }
                        }

                        if (!bRemove)
                        {
                            addSpecialistCityEffectCounts(eSpecialist, pTile, dEffectCityExtraCounts, bRemove: false);
                            addSpecialistCityEffectCounts(eSpecialist, pTile, dEffectCityCounts, bRemove: false);
                            //not adding to dEffectCityImprovementOnlyExtraCounts
                        }
                        else
                        {
                            //implied: (pTile.getImprovement() == eImprovement), so dEffectCityExtraCounts should be empty
                            addSpecialistCityEffectCounts(eSpecialist, pTile, dEffectCityExtraCounts, bRemove: true);
                        }

                        ////moved to the start
                        //using (var effectCityCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
                        //{
                        //    Dictionary<EffectCityType, int> dEffectCityCounts = effectCityCountsScoped.Value;
                        //    //if (pCity != null)
                        //    {
                        //        pCity.getEffectCityCountsForGovernor(pCity.governor(), dEffectCityCounts);
                        //    }

                        for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                        {
                            //int iOutput = (pTile.yieldOutput(pTile.getImprovement(), eSpecialist, eLoopYield, null, false) - pTile.yieldOutput(pTile.getImprovement(), SpecialistType.NONE, eLoopYield, null, false));
                            //int iOutput = (pTile.yieldOutput(eImprovement, eSpecialist, eLoopYield, null, false) - pTile.yieldOutput(eImprovement, SpecialistType.NONE, eLoopYield, null, false));
                            int iOutput = ((BetterAITile)pTile).yieldOutputForGovernor(eImprovement, eSpecialist, eLoopYield, pCity, bCityEffects: false, bBaseOnly: false, bCost: true, pCity.governor(), bTheology: true, newImprovements: null, !bRemove ? dEffectCityExtraCounts : null);
                            iOutput -= ((BetterAITile)pTile).yieldOutputForGovernor(eImprovement, SpecialistType.NONE, eLoopYield, pCity, bCityEffects: false, bBaseOnly: false, bCost: true, pCity.governor(), bTheology: true, newImprovements: null, bRemove ? dEffectCityExtraCounts : dEffectCityImprovementOnlyExtraCounts); //output without the specialist and without the city effect from that specialist

                            int iExtraModifier = 0;
                            foreach (KeyValuePair<EffectCityType, int> p in dEffectCityExtraCounts)
                            {
                                iExtraModifier += infos.effectCity(p.Key).maiYieldModifier[eLoopYield] * p.Value;
                            }

                            foreach (KeyValuePair<EffectCityType, int> p in dEffectCityCounts)
                            {
                                EffectCityType eLoopEffectCity = p.Key;
                                int iEffectOutput = infos.effectCity(eLoopEffectCity).maiYieldRateSpecialist[eLoopYield];

                                if (infos.specialistClass(infos.specialist(eSpecialist).meClass).mbUrban)
                                {
                                    iEffectOutput += infos.effectCity(eLoopEffectCity).maiYieldRateSpecialistUrban[eLoopYield];
                                }

                                if (iEffectOutput != 0)
                                {
                                    //iOutput += infos.utils().modify(iEffectOutput, pCity.calculateTotalYieldModifier(eLoopYield));
                                    iOutput += iEffectOutput * p.Value;
                                }

                            }

                            if (iOutput != 0)
                            {
                                iOutput = infos.utils().modify(iOutput, pCity.calculateTotalYieldModifier(eLoopYield) + iExtraModifier);
/*####### Better Old World AI - Base DLL #######
  ### AI: less value for specialist      END ###
  ##############################################*/
                                iValue += cityYieldValue(eLoopYield, pCity) * iOutput * AI_YIELD_TURNS / Constants.YIELDS_MULTIPLIER;
                            }

                        }
                        //}
                    }

                    if (infos.specialist(eSpecialist).miOpinionReligion != 0)
                    {
                        if (eImprovement != ImprovementType.NONE)
                        {
                            if (infos.improvement(eImprovement).meReligionPrereq != ReligionType.NONE)
                            {
                                iValue += getReligionOpinionValue(infos.improvement(eImprovement).meReligionPrereq, infos.specialist(eSpecialist).miOpinionReligion, AI_YIELD_TURNS);
                            }
                            else if (infos.improvement(eImprovement).meReligionSpread != ReligionType.NONE)
                            {
                                iValue += getReligionOpinionValue(game.getImprovementReligionSpread(eImprovement), infos.specialist(eSpecialist).miOpinionReligion, AI_YIELD_TURNS);
                            }
                        }
                    }

                    if (!bRemove && player.getWorldReligionCount() == 0)
                    {
                        int iPlayerSpecialists = player.countSpecialists(eSpecialist);
                        int iNumPlayerSpecialistClass = eSpecialistClass != SpecialistClassType.NONE ? player.countSpecialistClasses(eSpecialistClass) : 0;
                        for (ReligionType eLoopReligion = 0; eLoopReligion < infos.religionsNum(); ++eLoopReligion)
                        {
                            if (!game.isReligionFounded(eLoopReligion))
                            {
                                bool bSpecialistNeeded = false;
                                if (infos.religion(eLoopReligion).maiRequiresSpecialist[eSpecialist] > iPlayerSpecialists)
                                {
                                    bSpecialistNeeded = true;
                                }

                                if (eSpecialistClass != SpecialistClassType.NONE)
                                {
                                    if (infos.religion(eLoopReligion).maiRequiresSpecialistClass[eSpecialistClass] > iNumPlayerSpecialistClass)
                                    {
                                        bSpecialistNeeded = true;
                                    }
                                }
                                if (bSpecialistNeeded)
                                {
                                    iValue += adjustForInflation(AI_SPECIALIST_FOUND_RELIGION_VALUE);
                                    break;
                                }
                            }
                        }
                    }

                    if (pTile != null && !bRemove)
                    {
                        if (pTile.hasSpecialist())
                        {
                            if (pTile.getSpecialist() != eSpecialist)
                            {
                                iValue -= specialistValue(pTile.getSpecialist(), pCity, pTile, pTile.getImprovement(), false, false, false);
                            }
                        }
                        else
                        {
                            iValue -= effectCityValue(infos.Globals.CITIZEN_EFFECTCITY, pCity, true);
                            cityEffectExtraYieldFromExtraCityEffects(infos.Globals.CITIZEN_EFFECTCITY, pCity, dEffectCityExtraCounts, extraYields);
                            iValue += cityEffectExtraUnlockValueFromExtraCityEffects(infos.Globals.CITIZEN_EFFECTCITY, pCity, dEffectCityExtraCounts);
                        }
                    }

                    foreach (GoalData pGoalData in ((BetterAIPlayer)player).getGoalDataList())
                    {
                        if (!(pGoalData.mbFinished))
                        {
                            if ((infos.goal(pGoalData.meType).miSpecialists > 0) ||
                                (infos.goal(pGoalData.meType).maiSpecialistCount[eSpecialist] > 0) ||
                                ((infos.goal(pGoalData.meType).maiCitySpecialistCount[eSpecialist] > 0) && (pGoalData.miCityID == ((pCity != null) ? pCity.getID() : -1))))
                            {
                                iValue += bonusValue(infos.Globals.FINISHED_AMBITION_BONUS);
                            }
                        }
                    }

                    if (!bRemove)
                    {
                        GoalData pGoalData = getActiveStatGoal(infos.Globals.SPECIALIST_PRODUCED_STAT);
                        if (pGoalData != null)
                        {
                            iValue += bonusValue(infos.Globals.FINISHED_AMBITION_BONUS);
                        }
                    }
                }

                return Math.Max(1, iValue);
            }


/*####### Better Old World AI - Base DLL #######
  ### AI: less value for specialist    START ###
  ##############################################*/
            public override long specialistValue(SpecialistType eSpecialist, City pCity, Tile pTile, ImprovementType eImprovement, bool bIncludeCost, bool bIncludeUnlock, bool bIncludeBorderExpansion)
            {
                return base.specialistValue(eSpecialist, pCity, pTile, eImprovement, bIncludeCost, bIncludeUnlock: false, bIncludeBorderExpansion: bIncludeBorderExpansion);  //make sure bIncludeUnlock is always false, because unlock value is part of buildvalue
            }

            public override long getSpecialistBuildValue(SpecialistType eSpecialist, Tile pTile)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.getSpecialistBuildValue");

                //City pCity = pTile.cityTerritory();
                //return getBuildValue(specialistValue(eSpecialist, pCity, pTile, bIncludeCost: true, bIncludeUnlock: false), pCity, infos.Globals.CIVICS_YIELD, player.getSpecialistBuildCost(eSpecialist, pCity, pTile.getImprovement()), AI_MIN_SPECIALIST_BUILD_TURNS, AI_HALF_VALUE_SPECIALIST_BUILD_TURNS);
                return getSpecialistBuildValue(eSpecialist, pTile, pTile.cityTerritory(), Math.Max(1, pTile.cityTerritory().calculateModifiedYield(infos.Globals.CIVICS_YIELD)), player.getSpecialistBuildCost(eSpecialist, pTile.cityTerritory(), pTile.getImprovement()), pTile.getImprovement(), bIncludeUnlock: true);
            }

            public virtual long getSpecialistBuildValue(SpecialistType eSpecialist, Tile pTile, City pCity, int iYieldRate, int iCost, ImprovementType eImprovement, bool bIncludeUnlock = false)
            {
                long iValue;
                if (pCity == null)
                {
                    pCity = pTile.cityTerritory();
                }
                iValue = getBuildValue(specialistValue(eSpecialist, pCity, pTile, eImprovement, bIncludeCost: true, bIncludeUnlock: false, bIncludeBorderExpansion: true), iYieldRate, iCost, AI_MIN_SPECIALIST_BUILD_TURNS, AI_HALF_VALUE_SPECIALIST_BUILD_TURNS);

                if (bIncludeUnlock)
                {
                    long iBestUnlockValue = 0;
                    SpecialistType eBestUnlockSpecialist = SpecialistType.NONE;
                    for (SpecialistType eLoopSpecialist = 0; eLoopSpecialist < infos.specialistsNum(); ++eLoopSpecialist)
                    {
                        if (infos.specialist(eLoopSpecialist).meSpecialistPrereq == eSpecialist)
                        {
                            //iBestUnlock = Math.Max(iBestUnlock, specialistValue(eLoopSpecialist, pCity, pTile, bIncludeCost: true, true));

                            long iLoopSpecialistValue = getSpecialistBuildValue(eLoopSpecialist, pTile, pCity, iYieldRate, iCost, eImprovement, bIncludeUnlock);
                            if (iLoopSpecialistValue > iBestUnlockValue)
                            {
                                iBestUnlockValue = iLoopSpecialistValue;
                                eBestUnlockSpecialist = eLoopSpecialist;
                            }
                        }
                    }
                    if (iBestUnlockValue > iValue)
                    {
                        //iValue = ( iValue + iBestUnlock) / 2;
                        iValue += ((iBestUnlockValue - iValue) * player.getSpecialistBuildCost(eSpecialist, pCity, eImprovement)) / (3 * player.getSpecialistBuildCost(eBestUnlockSpecialist, pCity, eImprovement)); //same specialist production cost means 1/3 of the unlock value gets added. The more expensive the unlocked specialist is, the less value gets added.
                    }
                }

                return iValue;
            }
/*####### Better Old World AI - Base DLL #######
  ### AI: less value for specialist      END ###
  ##############################################*/


            public override bool doDecision(DecisionData pDecision, bool bTransitionFromHuman)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.doDecision");

                //MohawkAssert.Assert(player != null, "Tribe event decisions not supported");
                if (pDecision.Type == DecisionType.UPGRADE_CHARACTER)
                {

                    if (player == null)
                    {
                        return false;
                    }

                    if (!player.isDecisionValid(pDecision))
                    {
                        //player.logDecisionInvalid(pDecision);
                        player.removeDecision(pDecision);
                        return false;
                    }

                    TextVariable.PoolReset(); // lots of text generated with each decision

                    int iChoice = 0;
                    int iData = -1;

                    UpgradeCharacterDecision pUpgradeDecision = (UpgradeCharacterDecision)pDecision;
                    Character pCharacter = game.character(pUpgradeDecision.getCharacterID());

                    long iBestValue = -1;
                    iChoice = -1;
                    RatingType eBestRating = RatingType.NONE;
                    TraitType eBestTrait = TraitType.NONE;

                    int iNumRatings = pUpgradeDecision.getNumUpgrades();
                    for (int iLoopOption = 0; iLoopOption < iNumRatings; iLoopOption++)
                    {
                        long iOptionValue = 0;
                        RatingType eRating = pUpgradeDecision.getRating(iLoopOption);
                        if (eRating != RatingType.NONE)
                        {
                            iOptionValue += ratingChangeValue(eRating, 1, pCharacter);
                        }
                        TraitType eTrait = pUpgradeDecision.getTrait(iLoopOption);
                        if (eTrait != TraitType.NONE)
                        {
                            iOptionValue += traitValueCurrentRole(eTrait, pCharacter, bRemove: false);

/*####### Better Old World AI - Base DLL #######
  ### Don't ignore ratings from traits START ###
  ##############################################*/
                            using (var ratingsScoped = CollectionCache.GetDictionaryScoped<RatingType, int>())
                            {
                                Dictionary<RatingType, int> dRatingsChanged = ratingsScoped.Value;
                                for (RatingType eLoopRating = 0; eLoopRating < infos.ratingsNum(); eLoopRating++)
                                {
                                    dRatingsChanged[eLoopRating] = infos.trait(eTrait).maiRating[eLoopRating] + infos.trait(eTrait).maiPermanentRating[eLoopRating];
                                }

                                foreach (TraitType eOtherTrait in infos.trait(eTrait).maeTraitReplaces)
                                {
                                    if (pCharacter.isTrait(eOtherTrait))
                                    {
                                        iOptionValue -= traitValueCurrentRole(eOtherTrait, pCharacter, bRemove: true);

                                        for (RatingType eLoopRating = 0; eLoopRating < infos.ratingsNum(); eLoopRating++)
                                        {
                                            dRatingsChanged[eLoopRating] -= (infos.trait(eOtherTrait).maiRating[eLoopRating] + infos.trait(eOtherTrait).maiPermanentRating[eLoopRating]);
                                        }
                                    }
                                }

                                for (RatingType eLoopRating = 0; eLoopRating < infos.ratingsNum(); eLoopRating++)
                                {
                                    iOptionValue += ratingChangeValue(eLoopRating, dRatingsChanged[eLoopRating], pCharacter);
                                }
                            }
/*####### Better Old World AI - Base DLL #######
  ### Don't ignore ratings from traits   END ###
  ##############################################*/
                        }

                        if (iOptionValue > iBestValue)
                        {
                            eBestRating = eRating;
                            eBestTrait = eTrait;
                            iBestValue = iOptionValue;
                            iChoice = iLoopOption;
                        }
                    }
                    if (iChoice == -1)
                    {
                        iChoice = game.randomNext(iNumRatings);
                    }
                    if (eBestTrait != TraitType.NONE)
                    {
                        player.pushDebugLogData(new GameLogData(pCharacter.getFullName(player) + " becomes " + game.textManager().TEXT(infos.trait(eBestTrait).meName), GameLogType.AI_DEBUG, "", "", "", "", game.getTurn(), game.getTeamTurn()));
                    }
                    if (eBestRating != RatingType.NONE)
                    {
                        player.pushDebugLogData(new GameLogData(pCharacter.getFullName(player) + " gains " + HelpText.TEXT(infos.rating(eBestRating).mName), GameLogType.AI_DEBUG, "", "", "", "", game.getTurn(), game.getTeamTurn()));
                    }


                    player.makeDecision(pDecision, iChoice, iData);

                    return true;
                }
                else return base.doDecision(pDecision, bTransitionFromHuman);

            }


/*####### Better Old World AI - Base DLL #######
  ### Better TurnsLeftEstimate         START ###
  ##############################################*/
            public virtual int getCouncilTurnsLeftEstimate(Character pCharacter, CouncilType eCouncil)
            {
                return (getCouncilTurnsLeftEstimateX10(pCharacter, eCouncil) + 5) / 10;
            }
            public virtual int getCouncilTurnsLeftEstimateX10(Character pCharacter, CouncilType eCouncil)
            {
                return getTurnsLeftEstimateX10(pCharacter, bGeneral: false, bJob: true);
            }

            public virtual int getJobTurnsLeftEstimate(Character pCharacter, JobType eJob)
            {
                return (getJobTurnsLeftEstimateX10(pCharacter, eJob) + 5) / 10;
            }

            public virtual int getJobTurnsLeftEstimateX10(Character pCharacter, JobType eJob)
            {
                return getTurnsLeftEstimateX10(pCharacter, bGeneral: (eJob == infos.Globals.GENERAL_JOB), bJob: true);
            }

            public virtual int getTraitTurnsLeftEstimate(Character pCharacter, TraitType eTrait)
            {
                return (getTraitTurnsLeftEstimateX10(pCharacter, eTrait) + 5) / 10;
            }

            public virtual int getTraitTurnsLeftEstimateX10(Character pCharacter, TraitType eTrait)
            {
                BetterAIInfoTrait pInfoTrait = (BetterAIInfoTrait)(infos.trait(eTrait));

                if (pInfoTrait.miRemoveTurns != 0)
                {
                    int iRemoveTurns = pInfoTrait.miRemoveTurns;
                    if (pCharacter.isTrait(eTrait))
                    {
                        iRemoveTurns -= pCharacter.getTraitTurnLength(eTrait);
                    }
                    return 10 * iRemoveTurns;
                }
                else if (pInfoTrait.maiRemoveTurnsFromTraitProbsX10 != 0)
                {
                    return pInfoTrait.maiRemoveTurnsFromTraitProbsX10;
                }
                else
                {
                    return getTurnsLeftEstimateX10(pCharacter, bGeneral: false, bJob: false);
                }

            }

            protected virtual void cacheCharacterTurnsRemaining()
            {
                using (new UnityProfileScope("PlayerAI.cacheCharacterTurnsRemaining"))
                {
                    Action<int> loopCharacterMaxAgeDelegate = new Action<int>(iCharacterID =>
                    {
                        cacheCharacterTurnsRemaining(iCharacterID);
                    });


                    //((BetterAIGame)game)
                    using (new GameCoreObjectTracker(null))
                    {
                        if (Multithreaded)
                        {
                            System.Threading.Tasks.Parallel.ForEach(((BetterAIPlayer)player).getCharacters(), game.ParallelOptions, loopCharacterMaxAgeDelegate);

                            for (PlayerType eLoopOtherPlayer = 0; eLoopOtherPlayer < game.getNumPlayers(); ++eLoopOtherPlayer)
                            {
                                if (eLoopOtherPlayer != getPlayer())
                                {
                                    BetterAIPlayer pLoopOtherPlayer = ((BetterAIPlayer)(game.player(eLoopOtherPlayer)));

                                    if (pLoopOtherPlayer.isAlive() && game.isTeamContact(player.getTeam(), pLoopOtherPlayer.getTeam()))
                                    {
                                        //game.team pPlayer.getTeam();
                                        System.Threading.Tasks.Parallel.ForEach(pLoopOtherPlayer.getCharacters(), game.ParallelOptions, loopCharacterMaxAgeDelegate);
                                    }
                                }
                            }
                        }
                        else
                        {
                            foreach (int iCharacterID in ((BetterAIPlayer)player).getCharacters())
                            {
                                loopCharacterMaxAgeDelegate(iCharacterID);
                            }

                            for (PlayerType eLoopOtherPlayer = 0; eLoopOtherPlayer < game.getNumPlayers(); ++eLoopOtherPlayer)
                            {
                                if (eLoopOtherPlayer != getPlayer())
                                {
                                    BetterAIPlayer pLoopOtherPlayer = ((BetterAIPlayer)(game.player(eLoopOtherPlayer)));

                                    if (pLoopOtherPlayer.isAlive() && game.isTeamContact(player.getTeam(), pLoopOtherPlayer.getTeam()))
                                    {
                                        foreach (int iCharacterID in pLoopOtherPlayer.getCharacters())
                                        {
                                            loopCharacterMaxAgeDelegate(iCharacterID);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            protected virtual void cacheCharacterTurnsRemaining(int iCharacterID)
            {
                Character pCharacter = game.character(iCharacterID);
                {
                    if (pCharacter != null && pCharacter.isAlive() && !pCharacter.isTemporary() && !pCharacter.hasTraitDoomed())
                    {
                        BAI_mpAICache.setCharacterGeneralTurnsLeftX10(iCharacterID, calculateTurnsLeftEstimateX10(pCharacter: pCharacter, bGeneral: true));
                        BAI_mpAICache.setCharacterAnyJobTurnsLeftX10(iCharacterID, calculateTurnsLeftEstimateX10(pCharacter: pCharacter, bGeneral: false, bJob: true));
                        BAI_mpAICache.setCharacterLifeTurnsLeftX10(iCharacterID, calculateTurnsLeftEstimateX10(pCharacter: pCharacter, bGeneral: false, bJob: false));
                    }
                    else
                    {
                        BAI_mpAICache.setCharacterGeneralTurnsLeftX10(iCharacterID, 0);
                        BAI_mpAICache.setCharacterAnyJobTurnsLeftX10(iCharacterID, 0);
                        BAI_mpAICache.setCharacterLifeTurnsLeftX10(iCharacterID, 0);
                    }
                }
            }


            public virtual int getExpectedMaxAgeHealthyX10(Character pCharacter, bool bGeneral)
            {
                BetterAIInfoMortality pInfoMortality = (BetterAIInfoMortality)infos.mortality(game.getMortality());

                if (bGeneral)
                {
                    return pInfoMortality.maiMaxAgeGeneralX10[pCharacter.getAge()];
                }
                else
                {
                    return pInfoMortality.maiMaxAgeX10[pCharacter.getAge()];
                }
            }

            public virtual int getPerMilleChanceForDecadePropTrait(BetterAICharacter pCharacter, int iExtraAge = 0)
            {
                int iChance = 10000;
                for (TraitType eLoopTrait = 0; eLoopTrait < infos.traitsNum(); eLoopTrait++)
                {
                    BetterAIInfoTrait pLoopInfoTrait = (BetterAIInfoTrait)(infos.trait(eLoopTrait));

                    if (!pCharacter.isTrait(eLoopTrait) && pLoopInfoTrait.maiDecadeProb.Count > 0 && pCharacter.getTraitValid(eLoopTrait))
                    {
                        iChance -= iChance * pLoopInfoTrait.maiDecadeProb[Math.Min((pCharacter.getAge() + iExtraAge) / 10, pLoopInfoTrait.maiDecadeProb.Count - 1)];
                    }
                }

                return (10000 - iChance) / 10;
            }

            public virtual void modifySecondaryTraitChanceForPoolSize(TraitType eSecondardDieTrait, TraitType eOriginTrait, ref int iSecondaryTraitAddChance, ref Dictionary<(TraitType, TraitType), int> dTraitProbs)
            {
                int iValue = 0;
                int iGranularity = 1000; //iSecondaryTraitAddChance is multiplied with 1000 already
                using (var traitProbTraitsScoped = CollectionCache.GetListScoped<(TraitType, TraitType)>())
                using (var traitPoolsizeProbsScoped = CollectionCache.GetDictionaryScoped<int, int>())
                {
                    List<(TraitType, TraitType)> otherTraitPairs = traitProbTraitsScoped.Value;
                    Dictionary<int, int> dPoolsizeProbs = traitPoolsizeProbsScoped.Value;
                    foreach ((TraitType, TraitType) eLoopTraitPair in dTraitProbs.Keys)
                    {
                        if (eLoopTraitPair.Item1 != eOriginTrait || eLoopTraitPair.Item2 != eSecondardDieTrait)
                        {
                            otherTraitPairs.Add(eLoopTraitPair);
                        }
                    }

                    getTraitPoolsizeChances(iIndex: 0, iPoolsize: 0, iChances: iGranularity, ref otherTraitPairs, ref dTraitProbs, ref dPoolsizeProbs);
                    foreach (int iLoopPoolsize in dPoolsizeProbs.Keys)
                    {
                        iValue += iSecondaryTraitAddChance * dPoolsizeProbs[iLoopPoolsize] / (1 + iLoopPoolsize);
                    }

                    iSecondaryTraitAddChance = (iValue + (iGranularity/2)) / iGranularity;
                    return;
                }
            }

            public virtual void getTraitPoolsizeChances(int iIndex, int iPoolsize, int iChances, ref List<(TraitType, TraitType)> otherTraitPairs, ref Dictionary<(TraitType, TraitType), int> dTraitProbs, ref Dictionary<int, int> dPoolsizeProbs)
            {
                (TraitType, TraitType) eCurrentTrait = otherTraitPairs[iIndex];
                if (iIndex <= otherTraitPairs.Count - 2)
                {
                    getTraitPoolsizeChances(iIndex + 1, iPoolsize + 0, iChances * (100 - dTraitProbs[eCurrentTrait]), ref otherTraitPairs, ref dTraitProbs, ref dPoolsizeProbs);
                    getTraitPoolsizeChances(iIndex + 1, iPoolsize + 1, iChances * dTraitProbs[eCurrentTrait], ref otherTraitPairs, ref dTraitProbs, ref dPoolsizeProbs);
                }
                else // last item: iIndex == otherTraits.Count - 1
                {
                    if (dPoolsizeProbs.ContainsKey(iPoolsize))
                    {
                        dPoolsizeProbs[iPoolsize] += iChances * (100 - dTraitProbs[eCurrentTrait]);
                    }
                    else
                    {
                        dPoolsizeProbs.Add(iPoolsize, iChances * (100 - dTraitProbs[eCurrentTrait]));
                    }

                    if (dPoolsizeProbs.ContainsKey(iPoolsize + 1))
                    {
                        dPoolsizeProbs[iPoolsize + 1] += iChances * dTraitProbs[eCurrentTrait];
                    }
                    else
                    {
                        dPoolsizeProbs.Add(iPoolsize + 1, iChances * dTraitProbs[eCurrentTrait]);
                    }
                }
            }


            public virtual void getTurnsWeightProductFromDeathTraitX10(TraitType eDieTrait, Character pCharacter, 
                ref int iValue, ref int iWeight, ref int iRemainingWeight, ref Dictionary<(TraitType, TraitType), int> dTraitProbs, int iEvalStartTurn = 1, int iEvalEndTurn = 10, bool bJob = true)
            {
                BetterAIInfoTrait pDieInfoTrait = (BetterAIInfoTrait)(infos.trait(eDieTrait));
                int iDieProb = infos.Helpers.getTraitDieProb(eDieTrait, game);
                if (bJob)
                {
                    foreach (TraitType eLoopTrait in pDieInfoTrait.maeNoJobTraitProbs)
                    {
                        //NoJob is the same as death
                        iDieProb += pDieInfoTrait.maiTraitProb[eLoopTrait];
                    }
                }

                int iRemoveTurns = pDieInfoTrait.miRemoveTurns;
                int iRemoveProb = pDieInfoTrait.miRemoveProb;
                int iNewValue = 0;

                int iYearDivisor = game.getYearDivisions();
                int iTurnYearOffset = (game.getTurn() - pCharacter.getID()) % iYearDivisor;

                //this is moved to parent method
                //using (var traitProbsScoped = CollectionCache.GetDictionaryScoped<(TraitType, TraitType), int>())
                {
                    //Dictionary<(TraitType, TraitType), int> dTraitProbs = traitProbsScoped.Value;
                    //foreach (TraitType eLoopTrait in pCharacter.getTraits())
                    //{
                    //    BetterAIInfoTrait pLoopInfoTrait = (BetterAIInfoTrait)(infos.trait(eLoopTrait));
                    //    foreach(TraitType eOtherTrait in pLoopInfoTrait.maeAllTraitProbs)
                    //    {
                    //        if (dTraitProbs.ContainsKey((eLoopTrait, eOtherTrait)))
                    //        {
                    //            dTraitProbs[(eLoopTrait, eOtherTrait)] += pLoopInfoTrait.maiTraitProb[eOtherTrait]; //not perfect but w/e
                    //        }
                    //        else
                    //        {
                    //            dTraitProbs.Add((eLoopTrait, eOtherTrait), pLoopInfoTrait.maiTraitProb[eOtherTrait]);
                    //        }
                    //    }
                    //}


                    for (int i = iEvalStartTurn; i < iEvalEndTurn; i++)
                    {
                        if ((i + iTurnYearOffset) % iYearDivisor == 0) //these things only happen yearly, for other turn scales this skips many turns
                        {

                            int iYear = game.turnsToYears(i + iTurnYearOffset) / iYearDivisor;

                            //first: dying: doTurnYear -> checkDeathTrait
                            int iDieProbWeight = iDieProb * iWeight;
                            iNewValue += iDieProbWeight * i;
                            //iWeight *= (100 - iDieProb);
                            //iWeight /= 100;
                            iWeight -= iDieProbWeight / 100;

                            //second: adding traits: doTurnYear -> checkLifeEvent -> doAddTrait
                            //this does now account for other present traits with their own aiTraitProps that could be chosen instead, thus reducing the chances of a death trait to be added
                            int iDecadePropTraitGetsAddedChanceX1000 = getPerMilleChanceForDecadePropTrait((BetterAICharacter)pCharacter, iYear);
                            foreach (TraitType eSecondardDieTrait in pDieInfoTrait.maeDieTraitProbs)
                            {
                                BetterAIInfoTrait pSecondaryDieInfoTrait = (BetterAIInfoTrait)(infos.trait(eSecondardDieTrait));
                                int iSecondaryTraitAddChance = 1000 * pDieInfoTrait.maiTraitProb[eSecondardDieTrait]; //* 1000 #1
                                if (dTraitProbs.Count > 1)
                                {
                                    modifySecondaryTraitChanceForPoolSize(eSecondardDieTrait, eDieTrait,ref iSecondaryTraitAddChance, ref dTraitProbs);

                                }
                                iSecondaryTraitAddChance *= (1000 - iDecadePropTraitGetsAddedChanceX1000); //*1000 #2
                                iSecondaryTraitAddChance += 500;  // rounding
                                iSecondaryTraitAddChance /= 1000; // /1000 #1

                                int iSecondaryWeight = ((iWeight * iSecondaryTraitAddChance) + 500) / 1000; // rounding, /1000 #2
                                iSecondaryWeight = (iSecondaryWeight + 50) / 100;
                                int iOriginalSecondaryWeight = iSecondaryWeight;
                                int iSecondaryRemainingWeight = 0;
                                int iSecondaryValue = 0;
                                if (!pDieInfoTrait.mbNoRemoveOnTraitProb)
                                {
                                    foreach (TraitType eOtherTrait in pDieInfoTrait.maeAllTraitProbs)
                                    {
                                        //first remove them
                                        dTraitProbs.Remove((eDieTrait, eOtherTrait));
                                    }
                                }

                                foreach (TraitType eOtherTrait in pSecondaryDieInfoTrait.maeAllTraitProbs)
                                {
                                    //add the new ones
                                    dTraitProbs.Add((eSecondardDieTrait, eOtherTrait), pSecondaryDieInfoTrait.maiTraitProb[eOtherTrait]);
                                }

                                getTurnsWeightProductFromDeathTraitX10(eSecondardDieTrait, pCharacter, ref iSecondaryValue, ref iSecondaryWeight, ref iSecondaryRemainingWeight, ref dTraitProbs,
                                    iEvalStartTurn: iEvalStartTurn + i, iEvalEndTurn: iEvalEndTurn + (i/2), bJob: bJob); //start at current turn, but extend range a bit too, to properly grasp TRAIT_SICKLY effect

                                {
                                    int iWeightDiff = iOriginalSecondaryWeight - iSecondaryWeight;
                                    int iWeightDie = iWeightDiff - iSecondaryRemainingWeight;
                                    int iAverageTurns = 0;
                                    if (iWeightDiff != 0)
                                    {
                                        iAverageTurns = iSecondaryValue / iWeightDiff;
                                    }
                                    
                                    Debug.Log($"Turn {i}, {pSecondaryDieInfoTrait.mzType} from {pDieInfoTrait.mzType}: Total secondary weight {iOriginalSecondaryWeight}, weight used {iWeightDiff}, average turns to die {iAverageTurns}(/10) at weight {iWeightDie}");
                                }

                                foreach (TraitType eOtherTrait in pSecondaryDieInfoTrait.maeAllTraitProbs)
                                {
                                    //and remove the new ones again
                                    dTraitProbs.Remove((eSecondardDieTrait, eOtherTrait));
                                }

                                iNewValue += 10 * iSecondaryValue; //because iSecondaryValue got divided by 10 at the end of the method, and iNewValue is not yet divided

                                //adding severely ill removes ill. This removal on adding always happens unless mbNoRemoveOnTraitProb. Character.doAddTrait
                                //if (pSecondaryDieInfoTrait.maeTraitReplaces.Contains(eDieTrait))

                                if (!pDieInfoTrait.mbNoRemoveOnTraitProb)
                                {
                                    foreach (TraitType eOtherTrait in pDieInfoTrait.maeAllTraitProbs)
                                    {
                                        //then add them again
                                        dTraitProbs.Add((eDieTrait, eOtherTrait), pDieInfoTrait.maiTraitProb[eOtherTrait]);
                                    }

                                    //chance to die:             ((100 * (iOriginalSecondaryWeight-iSecondaryWeight-iSecondaryRemainingWeight) / iOriginalSecondaryWeight)
                                    //chance to be cured:        ((100 * iSecondaryRemainingWeight) / iOriginalSecondaryWeight)
                                    //chance undecided:          ((100 * iSecondaryWeight) / iOriginalSecondaryWeight)
                                    //chance to be cured or die: ((100 * (iOriginalSecondaryWeight-iSecondaryWeight) / iOriginalSecondaryWeight)

                                    //add weight from cure
                                    iRemainingWeight += 100 * iSecondaryRemainingWeight; //iSecondaryRemainingWeight is already divided by 100 !

                                    //remove weight from  cure or dying
                                    iWeight -= (iOriginalSecondaryWeight - iSecondaryWeight);
                                }
                                else
                                {
                                    //iWeight *= (100 - ((100 * (iOriginalSecondaryWeight - iSecondaryWeight - iSecondaryRemainingWeight) / iOriginalSecondaryWeight)));
                                    //iWeight /= 100;

                                    //remove weight from dying: like above but also remove cure chance, because Character still has original trait
                                    iWeight -= (iOriginalSecondaryWeight - iSecondaryWeight - iSecondaryRemainingWeight);
                                }
                            }

                            //third: removing traits: doTurnYear -> checkLifeEvent -> doRemoveTrait
                            if (iRemoveProb > 0)
                            {
                                //remove from removeprob can only happen if no trait from traitprob was added so chances need to be modified
                                //10000 is arbitrary
                                const int iGranularity = 10000;
                                int iModifier = (iGranularity * (1000 - iDecadePropTraitGetsAddedChanceX1000)) / 1000;
                                if (dTraitProbs.Count > 0)
                                {
                                    foreach ((TraitType, TraitType) eLoopTraitPair in dTraitProbs.Keys)
                                    {
                                        iModifier *= (100 - dTraitProbs[eLoopTraitPair]);
                                        iModifier /= 100;
                                    }
                                }
                                int iTempRemoveProbX10 = (((iRemoveProb * 10 * iModifier)) + (iGranularity/2)) / iGranularity;
                                int iRemoveProbWeight = ((iWeight * iTempRemoveProbX10) + 5) / 10;
                                iRemainingWeight += iRemoveProbWeight;
                                //iWeight *= (1000 - iTempRemoveProbX10);
                                //iWeight /= 1000;
                                iWeight -= iRemoveProbWeight / 100;
                            }
                        }

                        //fourth: removing traits with miRemoveTurns in doTurn. this is not affected by anything that comes before it, it even executes every turn for all turn scales
                        if (iRemoveTurns > 0 && pCharacter.getTraitTurnLength(eDieTrait) + i > iRemoveTurns)
                        {
                            iRemainingWeight += iWeight * 100;
                            iWeight = 0;
                            break;
                        }

                        if (iWeight < 20) break;
                    }
                }

                iRemainingWeight /= 100;
                iNewValue /= 10;

                iValue += iNewValue;

                return;
            }

            protected override int getTurnsLeftEstimate(Character pCharacter, bool bGeneral)
            {
                return (getTurnsLeftEstimateX10(pCharacter, bGeneral, bJob: true) + 5) / 10;
            }
            protected virtual int getTurnsLeftEstimate(Character pCharacter, bool bGeneral, bool bJob = true)
            {
                return (getTurnsLeftEstimateX10(pCharacter, bGeneral, bJob: bJob) + 5) / 10;
            }


            public virtual int getTurnsLeftEstimateX10(Character pCharacter, bool bGeneral, bool bJob = true)
            {
                int iValue;
                //if (pCharacter.getPlayer() == player.getPlayer())
                {
                    if (bGeneral)
                    {
                        if (BAI_mpAICache.getCharacterGeneralTurnsLeftX10(pCharacter.getID(), out iValue))
                        {
                            return iValue;
                        }
                    }
                    else
                    {
                        if (bJob)
                        {
                            if (BAI_mpAICache.getCharacterAnyJobTurnsLeftX10(pCharacter.getID(), out iValue))
                            {
                                return iValue;
                            }
                        }
                        else
                        {
                            if (BAI_mpAICache.getCharacterLifeTurnsLeftX10(pCharacter.getID(), out iValue))
                            {
                                return iValue;
                            }
                        }
                    }
                }

                {
                    //log as error with char id

                    iValue = calculateTurnsLeftEstimateX10(pCharacter, bGeneral, bJob: bJob);
                    if (bGeneral)
                    {
                        BAI_mpAICache.setCharacterGeneralTurnsLeftX10(pCharacter.getID(), iValue);
                    }
                    else
                    {
                        if (bJob)
                        {
                            BAI_mpAICache.setCharacterAnyJobTurnsLeftX10(pCharacter.getID(), iValue);
                        }
                        else
                        {
                            BAI_mpAICache.setCharacterLifeTurnsLeftX10(pCharacter.getID(), iValue);
                        }
                    }

                    return iValue;
                }

            }

            //lines 11453-11465
            public virtual int calculateTurnsLeftEstimateX10(Character pCharacter, bool bGeneral, bool bJob = true)
            {
                if (pCharacter == null || pCharacter.hasTraitDoomed())
                {
                    return 0;
                }
                int iSafeTurns = (pCharacter.getSafeTurn() - game.getTurn());
                if (pCharacter.isLeader())
                {
                    iSafeTurns = Math.Max(iSafeTurns, infos.mortality(game.getMortality()).miInitLeaderSafeTurns - game.getTurn());
                    if (player.getSuccessionCount() == 0)
                    {
                        iSafeTurns = Math.Max(iSafeTurns, game.yearsToTurns(infos.Globals.LEADER_NO_HEIR_SAFE_MIN_AGE - pCharacter.getAge()));
                    }
                }
                else if (pCharacter.isHeir() && player.getSuccessionCount() == 1)
                {
                    iSafeTurns = Math.Max(iSafeTurns, game.yearsToTurns(infos.Globals.SOLE_HEIR_SAFE_MIN_AGE - pCharacter.getAge()));
                }
                iSafeTurns = Math.Max(iSafeTurns, 0);

                int iMaxAgeX10 = infos.Globals.GENERAL_RETIRE_AGE;
                //if (pCharacter.getAge() + game.turnsToYears(iSafeTurns) + 1 >= iMaxAgeX10)
                //{
                //    return game.yearsToTurns(iMaxAgeX10 - pCharacter.getAge());
                //}

                //better calculation of extected remaining life (job) time
                //either death, or NO_JOB trait like Incubated
                //aiMortalityDieProb, iDieProb, iDieModifier
                //job: bNoJob, bNoCouncil, bNoGeneral, bNoGovernor, bNoReligion (removes clergy status, which removes agent ability is not enabled in another way), enablers: bGeneralPrereq, bGeneralAll, bGovernorPrereq, bGovernorAll, bAgentPrereq, ReligionAgent
                //maiDecadeProb, aiTraitProb (bNoRemoveOnTraitProb), aeTraitInvalid, iRemoveTurns, iRemoveProb, aeTraitReplaces (bRemoveLeader)

                //just focusing on traits that can appear randomly and cause death/nojob
                //leader is protected from some: Incubated, check via (infos().Helpers.isInvalidLeaderTrait(eLoopTrait))
                //TRAIT_INCUBATED, TRAIT_ILL, TRAIT_SEVERELY_ILL, TRAIT_WOUNDED, TRAIT_SEVERELY_WOUNDED, TRAIT_POISONED, TRAIT_DROUGHT_STRICKEN, TRAIT_AMBUSHED_BY_REBELS, TRAIT_INFECTED
                //TRAIT_INCUBATED: can't be gained randomly, removeTurns 3, removed by TRAIT_SEVERELY_ILL
                //TRAIT_INFECTED, TRAIT_POISONED: can't be gained randomly, removeProb 20
                //TRAIT_AMBUSHED_BY_REBELS: can't be gained randomly, removeTurns 1
                //TRAIT_DROUGHT_STRICKEN: can't be gained randomly, removeProb 50
                //TRAIT_WOUNDED: from battle, removeProb 40
                //TRAIT_SEVERELY_WOUNDED: removeProb 20, but 20 blinded (bNoJob)
                //TRAIT_ILL: removeProb 60 but DecadeProb 0012345000
                //TRAIT_SEVERELY_ILL: removedProb 40, replaces Ill, incubated, aiDecadeProb 00000 5 10 15 25 30

                const int iTotalWeight = 10000; //10000 equals 100% chance
                int iWeight = iTotalWeight;
                int iRemainingWeight = 0; //normal max age calculation will still run, but only be weighted with iRemainingWeight. Full weight only is character is healthy and has no death traits, or traits that give dead traits
                int iValue = 0; //Years * iWeight * 10
                using (var traitProbsScoped = CollectionCache.GetDictionaryScoped<(TraitType, TraitType), int>())
                {
                    Dictionary<(TraitType, TraitType), int> dTraitProbs = traitProbsScoped.Value;
                    foreach (TraitType eLoopTrait in pCharacter.getTraits())
                    {
                        BetterAIInfoTrait pLoopInfoTrait = (BetterAIInfoTrait)(infos.trait(eLoopTrait));
                        foreach (TraitType eOtherTrait in pLoopInfoTrait.maeAllTraitProbs)
                        {
                            if (dTraitProbs.ContainsKey((eLoopTrait, eOtherTrait)))
                            {
                                dTraitProbs[(eLoopTrait, eOtherTrait)] += pLoopInfoTrait.maiTraitProb[eOtherTrait]; //not perfect but w/e
                            }
                            else
                            {
                                dTraitProbs.Add((eLoopTrait, eOtherTrait), pLoopInfoTrait.maiTraitProb[eOtherTrait]);
                            }
                        }
                    }
                    foreach (TraitType eLoopTrait in pCharacter.getTraits())
                    {
                        BetterAIInfoTrait pLoopInfoTrait = ((BetterAIInfoTrait)(infos.trait(eLoopTrait)));
                        if (infos.Helpers.getTraitDieProb(eLoopTrait, game) > 0 || pLoopInfoTrait.maeDieTraitProbs.Count > 0 || (bJob && pLoopInfoTrait.maeNoJobTraitProbs.Count > 0))
                        {
                            getTurnsWeightProductFromDeathTraitX10(eLoopTrait, pCharacter, ref iValue, ref iWeight, ref iRemainingWeight, ref dTraitProbs, iEvalStartTurn: (1 + ((bJob && pLoopInfoTrait.maeNoJobTraitProbs.Count > 0) ? 0 : iSafeTurns)), iEvalEndTurn: AI_DEATHTRAIT_PROB_EVAL_DEPTH, bJob: bJob);
                        }
                    }
                }

                iRemainingWeight += iWeight;

                iMaxAgeX10 = getExpectedMaxAgeHealthyX10(pCharacter, bGeneral && !(pCharacter.isLeader() || pCharacter.isHeir())); //age * 10
                int iValueForHealthyMaxAge = iMaxAgeX10 - (10*(pCharacter.getAge() - 1)); //-1 because a character is no longer useful on their dying year
                iValueForHealthyMaxAge = game.yearsToTurns(iValueForHealthyMaxAge) * iRemainingWeight;

                if (iWeight != iTotalWeight)
                {
                    int iTraitWeight = iTotalWeight - iRemainingWeight;
                    int iMaxAgeFromTraits = iValue / iTraitWeight;
                    Debug.Log($"TraitMax {iMaxAgeFromTraits} at weight {iTraitWeight}, NormalMax {iMaxAgeX10} at weight {iRemainingWeight}");
                }

                iValue += iValueForHealthyMaxAge;
                iValue += (iTotalWeight / 2); //rounding
                iValue /= iTotalWeight;

                int iExtraTurnsFromTurnYearOffset = 10 * ((pCharacter.getID() - game.getTurn() - 1) % game.getYearDivisions());
                return game.yearsToTurns(Math.Max(0, iValue + iExtraTurnsFromTurnYearOffset));

                //this was the original.
                ////int iMaxAge = infos.Globals.GENERAL_RETIRE_AGE;
                //if (!bGeneral || pCharacter.isLeader())
                //{
                //    iMaxAge = Math.Max(iMaxAge, pCharacter.getAge() + 5);
                //}
                //return game.yearsToTurns(Math.Max(0, iMaxAge - pCharacter.getAge()));
            }

/*####### Better Old World AI - Base DLL #######
  ### Better TurnsLeftEstimate           END ###
  ##############################################*/

            public virtual long getTotalCharacterTraitsValue(Character pCharacter, bool? bLeader = null, bool? bLeaderSpouse = null, bool? bSuccessor = null, CouncilType? eCouncil = null, int? iCityGovernor = null, UnitType? eUnitGeneral = null, int? iCityAgent = null)
            {
                long iValue = 0;

                using (var traitPlayerEffectsScoped = CollectionCache.GetHashSetScoped<TraitType>())
                {
                    HashSet<TraitType> sTraitPlayerEffectIgnore = traitPlayerEffectsScoped.Value;

                    foreach (TraitType eLoopTrait in pCharacter.getTraits())
                    {
                        iValue += traitValue(eLoopTrait, pCharacter, bRemove: true, ref sTraitPlayerEffectIgnore, bLeader, bLeaderSpouse, bSuccessor, eCouncil, iCityGovernor, eUnitGeneral, iCityAgent);
                        sTraitPlayerEffectIgnore.Add(eLoopTrait);
                    }

                    //return traitValue(eTrait, pCharacter, bRemove, ref sTraitPlayerEffectIgnore, bLeader, bLeaderSpouse, bSuccessor, eCouncil, iCityGovernor, eUnitGeneral, iCityAgent: null);
                }

                return iValue;
            }

            //lines 11624-11725
            protected override long traitValue(TraitType eTrait, Character pCharacter, bool bRemove, bool? bLeader = null, bool? bLeaderSpouse = null, bool? bSuccessor = null, CouncilType? eCouncil = null, int? iCityGovernor = null, UnitType? eUnitGeneral = null)
            {
                using (var traitPlayerEffectsScoped = CollectionCache.GetHashSetScoped<TraitType>())
                {
                    HashSet<TraitType> sTraitPlayerEffectIgnore = traitPlayerEffectsScoped.Value;
                    return traitValue(eTrait, pCharacter, bRemove, ref sTraitPlayerEffectIgnore, bLeader, bLeaderSpouse, bSuccessor, eCouncil, iCityGovernor, eUnitGeneral, iCityAgent: null);
                }
            }

            protected virtual long traitValue(TraitType eTrait, Character pCharacter, bool bRemove, ref HashSet<TraitType> sTraitPlayerEffectIgnore, bool? bLeader = null, bool? bLeaderSpouse = null, bool? bSuccessor = null, CouncilType? eCouncil = null, int? iCityGovernor = null, UnitType? eUnitGeneral = null, int? iCityAgent = null)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.traitValue");

                BetterAIInfoTrait pInfoTrait = (BetterAIInfoTrait)infos.trait(eTrait);


/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
                int iTraitTurnsLeftX10 = Math.Min(AI_YIELD_TURNS * 10, getTraitTurnsLeftEstimateX10(pCharacter, eTrait));
                //int iTurnsLeft = ((int)iTurnsLeftX10 + 5) / 10;
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/

                long iValue = 0;

                iValue += getCharacterXPValue(pCharacter, pInfoTrait.miXPTurn);

                bool bTestLeader = pCharacter.isLeader();
                if (bLeader.HasValue)
                {
                    bTestLeader = bLeader.Value;
                }
                bool bTestLeaderSpouse = pCharacter.isLeaderSpouse();  //not in use
                if (bLeaderSpouse.HasValue)
                {
                    bTestLeaderSpouse = bLeaderSpouse.Value;
                }
                bool bTestSuccessor = pCharacter.isSuccessor();
                if (bSuccessor.HasValue)
                {
                    bTestSuccessor = bSuccessor.Value;
                }

                if (bTestLeader || bTestSuccessor)
                {
                    EffectPlayerType eEffectPlayer = pInfoTrait.meLeaderEffectPlayer;

                    if (eEffectPlayer != EffectPlayerType.NONE)
                    {
                        //long iLeaderValue = effectPlayerValue(eEffectPlayer, player.getStateReligion(), bRemove);
                        long iLeaderValue = effectPlayerValue(eEffectPlayer, player.getStateReligion(), bRemove: bRemove, 
                            iMaxTurnsRemainingX10: iTraitTurnsLeftX10, bSkipTurnsRemaining: false, bIncludeDependentEffects: true);

                        if (!bTestLeader)
                        {

/*####### Better Old World AI - Base DLL #######
  ### Better TurnsLeftEstimate         START ###
  ##############################################*/
                            if (player.leader() != null || player.leader() != pCharacter)
                            {
                                int iLeaderTurnsLeftX10 = getTurnsLeftEstimateX10(player.leader(), bGeneral: false, bJob: false);
                                if (iTraitTurnsLeftX10 > iLeaderTurnsLeftX10)
                                {
                                    iLeaderValue *= (iTraitTurnsLeftX10 - iLeaderTurnsLeftX10);
                                    iLeaderValue /= iTraitTurnsLeftX10;
                                }
                                else
                                {
                                    iLeaderValue = 0;
                                }
                            }
/*####### Better Old World AI - Base DLL #######
  ### Better TurnsLeftEstimate           END ###
  ##############################################*/
                            else
                            {
                                iLeaderValue /= 2;
                            }
                        }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
                        //iLeaderValue *= Math.Min(iTurnsLeftX10, getTurnsLeftEstimateX10(pCharacter, bGeneral: false, bJob: false));
                        //iLeaderValue /= 10; //because X10 above
                        iLeaderValue *= AI_YIELD_TURNS; //already scaled for remaining turns
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/
                        iValue += iLeaderValue;
                    }
                }

                CouncilType eTestCouncil = pCharacter.getCouncil();
                if (eCouncil.HasValue)
                {
                    eTestCouncil = eCouncil.Value;
                }

                //I don't understand the changes from v1.0.84365, so I'm not updating the code.

                int iTestGovernorCityID = pCharacter.getCityGovernorID();
                if (iCityGovernor.HasValue)
                {
                    iTestGovernorCityID = iCityGovernor.Value;
                }

                UnitType eTestUnit = pCharacter.unitGeneral()?.getType() ?? UnitType.NONE;
                if (eUnitGeneral.HasValue)
                {
                    eTestUnit = eUnitGeneral.Value;
                }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ### Better TurnsLeftEstimate         START ###
  ##############################################*/
                int iTestAgentCityID = pCharacter.getCityAgentID();
                if (iCityAgent.HasValue)
                {
                    iCityAgent = iCityGovernor.Value;
                }


                if (eTestCouncil != CouncilType.NONE)
                {
                    EffectPlayerType eEffectPlayer = infos.trait(eTrait).maeCouncilEffectPlayer[eTestCouncil];

                    if (eEffectPlayer != EffectPlayerType.NONE)
                    {

                        //long iCouncilValue = effectPlayerValue(eEffectPlayer, player.getStateReligion(), bRemove);
                        long iCouncilValue = effectPlayerValue(eEffectPlayer, player.getStateReligion(), bRemove: bRemove, 
                            iMaxTurnsRemainingX10: Math.Min(getCouncilTurnsLeftEstimateX10(pCharacter, eTestCouncil), iTraitTurnsLeftX10), bSkipTurnsRemaining: false, bIncludeDependentEffects: true);
                        // iCouncilValue *= Math.Min(iTurnsLeftX10, getCouncilTurnsLeftEstimateX10(pCharacter, eTestCouncil));
                        // iCouncilValue /= 10;  //because X10
                        iCouncilValue *= AI_YIELD_TURNS; //already scaled
                        iValue += iCouncilValue;
                    }
                }

                if (iTestGovernorCityID != -1)
                {
                    EffectCityType eEffectCity = pInfoTrait.meGovernorEffectCity;

                    if (eEffectCity != EffectCityType.NONE)
                    {

                        long iGovernorEffectCityValue = effectCityValue(eEffectCity, game.city(iTestGovernorCityID), bRemove);
                        iGovernorEffectCityValue *= Math.Min(iTraitTurnsLeftX10, getJobTurnsLeftEstimateX10(pCharacter, infos.Globals.GOVERNOR_JOB));
                        iGovernorEffectCityValue /= 10;  //because X10
                        iValue += iGovernorEffectCityValue;
                    }

                    if (pInfoTrait.maeJobEffectPlayer[infos.Globals.GOVERNOR_JOB] != EffectPlayerType.NONE)
                    {
                        //iGovernorValue += effectPlayerValue(pInfoTrait.maeJobEffectPlayer[infos.Globals.GOVERNOR_JOB], player.getStateReligion(), bRemove);
                        long iGovernorEffectPlayerValue = effectPlayerValue(pInfoTrait.maeJobEffectPlayer[infos.Globals.GOVERNOR_JOB], player.getStateReligion(), bRemove: bRemove, 
                            iMaxTurnsRemainingX10: Math.Min(getJobTurnsLeftEstimateX10(pCharacter, infos.Globals.GOVERNOR_JOB), iTraitTurnsLeftX10), bSkipTurnsRemaining: false, bIncludeDependentEffects: true);
                        iGovernorEffectPlayerValue *= AI_YIELD_TURNS; //already scaled
                        iValue += iGovernorEffectPlayerValue;
                    }

                    //iGovernorValue *= Math.Min(iTraitTurnsLeftX10, getJobTurnsLeftEstimateX10(pCharacter, infos.Globals.GOVERNOR_JOB));
                    //iGovernorValue /= 10;  //because X10
                    //iValue += iGovernorValue;
                }

                if (iTestAgentCityID != -1)
                {
                    if (pInfoTrait.maeJobEffectPlayer[infos.Globals.AGENT_JOB] != EffectPlayerType.NONE)
                    {
                        //long iAgentValue = effectPlayerValue(pInfoTrait.maeJobEffectPlayer[infos.Globals.AGENT_JOB], player.getStateReligion(), bRemove);
                        long iAgentValue = effectPlayerValue(pInfoTrait.maeJobEffectPlayer[infos.Globals.AGENT_JOB], player.getStateReligion(), bRemove: bRemove, 
                            iMaxTurnsRemainingX10: Math.Min(getJobTurnsLeftEstimateX10(pCharacter, infos.Globals.AGENT_JOB), iTraitTurnsLeftX10), bSkipTurnsRemaining: false, bIncludeDependentEffects: true);
                        //iAgentValue *= Math.Min(iTraitTurnsLeftX10, getJobTurnsLeftEstimateX10(pCharacter, infos.Globals.AGENT_JOB));
                        iAgentValue *= AI_YIELD_TURNS; //already scaled
                        //iAgentValue /= 10;  //because X10
                        iValue += iAgentValue;
                    }
                }

                //iValue *= getTurnsLeftEstimate(pCharacter, false);
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ### Better TurnsLeftEstimate           END ###
  ##############################################*/


                if (eTestUnit != UnitType.NONE)
                {
                    {
                        long iGeneralValue = 0;
                        {
                            EffectUnitType eEffectUnit = pInfoTrait.meGeneralEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE && game.isEffectUnitValid(eTestUnit, eEffectUnit))
                            {
                                iGeneralValue += effectUnitValue(eEffectUnit, eTestUnit);
                            }
                        }

                        if (bTestLeader || bTestSuccessor)
                        {
                            EffectUnitType eEffectUnit = pInfoTrait.meLeaderEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE && game.isEffectUnitValid(eTestUnit, eEffectUnit))
                            {
                                iGeneralValue += effectUnitValue(eEffectUnit, eTestUnit);
                            }
                        }
                        //iValue += iGeneralValue * getTurnsLeftEstimate(pCharacter, true);
                        iGeneralValue *= Math.Min(iTraitTurnsLeftX10, getJobTurnsLeftEstimateX10(pCharacter, infos.Globals.GENERAL_JOB));
                        iGeneralValue /= 10;  //because X10
                        iValue += iGeneralValue;
                    }

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ### Better TurnsLeftEstimate         START ###
  ##############################################*/

                    {
                        //long iGeneralEffectPlayerValue = 0;
                        if (pInfoTrait.maeJobEffectPlayer[infos.Globals.GENERAL_JOB] != EffectPlayerType.NONE)
                        {
                            //  iGeneralEffectPlayerValue += effectPlayerValue(pInfoTrait.maeJobEffectPlayer[infos.Globals.GENERAL_JOB], player.getStateReligion(), bRemove);
                            long iGeneralEffectPlayerValue = effectPlayerValue(pInfoTrait.maeJobEffectPlayer[infos.Globals.GENERAL_JOB], player.getStateReligion(), bRemove: bRemove, 
                                iMaxTurnsRemainingX10: Math.Min(getJobTurnsLeftEstimateX10(pCharacter, infos.Globals.GENERAL_JOB), iTraitTurnsLeftX10), bSkipTurnsRemaining: false, bIncludeDependentEffects: true);
                            iGeneralEffectPlayerValue *= AI_YIELD_TURNS; //already scaled
                            iValue += iGeneralEffectPlayerValue;
                        }
                    }


                }

                if (pInfoTrait.bAnyTraitEffectPlayer)
                {
                    if (pCharacter.isTrait(eTrait) && !bRemove)
                    {
                        UnityEngine.Debug.Log("[traitValue] traits already on a character can only be removed.");
                    }
                    else if (!pCharacter.isTrait(eTrait) && bRemove)
                    {
                        UnityEngine.Debug.Log("[traitValue] traits not already on a character can only be added.");
                    }

                    if (!(sTraitPlayerEffectIgnore.Contains(eTrait)))
                    {
                        foreach (TraitType eLoopTrait in pCharacter.getTraits())
                        {
                            if (sTraitPlayerEffectIgnore.Contains(eLoopTrait))
                            {
                                continue;
                            }

                            EffectPlayerType eLoopEffectPlayer = pInfoTrait.maeTraitEffectPlayer[eLoopTrait];
                            if (eLoopEffectPlayer == EffectPlayerType.NONE)
                            {
                                eLoopEffectPlayer = ((BetterAIInfoTrait)(infos.trait(eLoopTrait))).maeTraitEffectPlayer[eTrait];
                            }

                            if (eLoopEffectPlayer != EffectPlayerType.NONE)
                            {
                                //long iJobliketraitValue = effectPlayerValue(eLoopEffectPlayer, player.getStateReligion(), bRemove);
                                long iJobliketraitValue = effectPlayerValue(eLoopEffectPlayer, player.getStateReligion(), bRemove: bRemove, 
                                    iMaxTurnsRemainingX10: Math.Min(getTraitTurnsLeftEstimateX10(pCharacter, eLoopTrait), iTraitTurnsLeftX10), bSkipTurnsRemaining: false, bIncludeDependentEffects: true);
                                iJobliketraitValue *= AI_YIELD_TURNS; //already scaled
                                //iJobliketraitValue *= Math.Min(iTraitTurnsLeftX10, getTraitTurnsLeftEstimateX10(pCharacter, eLoopTrait));
                                //iJobliketraitValue /= 10; //because X10
                                iValue += iJobliketraitValue;
                            }
                        }
                    }

                }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ### Better TurnsLeftEstimate           END ###
  ##############################################*/

                iValue /= AI_YIELD_TURNS;

                return infos.utils().modify(iValue, pCharacter.isHeir() ? AI_HEIR_TUTOR_MODIFIER : 0, true);
            }

            //lines 12298-12966
            protected override long effectPlayerValue(EffectPlayerType eEffectPlayer, ReligionType eStateReligion, bool bRemove)
            {
                return effectPlayerValue(eEffectPlayer, eStateReligion, bRemove: bRemove, AI_YIELD_TURNS, bSkipTurnsRemaining: !(game.isCharacters()), bIncludeDependentEffects: true);
            }

            public virtual int getEffectPlayerTurnsRemainingX10(EffectPlayerType eEffectPlayer, int iMaxTurnsX10)
            {
                BetterAIInfoEffectPlayer pEffectPlayer = (BetterAIInfoEffectPlayer)infos.effectPlayer(eEffectPlayer);
                int iRemainingEffectTurnsX10 = iMaxTurnsX10;

                bool bCheckAllCharacters = false;
                bool bSourceFound = false;
                using (var effectPlayerTurnsLeftListScoped = CollectionCache.GetListScoped<int>())
                {
                    List<int> CharactersTurnsLeftEstimateX10 = effectPlayerTurnsLeftListScoped.Value;
                    if (pEffectPlayer.meSourceTrait != TraitType.NONE && (player.leader()?.isTrait(pEffectPlayer.meSourceTrait) ?? false))
                    {
                        CharactersTurnsLeftEstimateX10.Add(Math.Min(getTraitTurnsLeftEstimateX10(player.leader(), pEffectPlayer.meSourceTrait), iMaxTurnsX10));
                        bSourceFound = true;
                    }
                    else
                    {
                        foreach (JobType eLoopJob in pEffectPlayer.mseSourceTraitJobs)
                        {
                            bSourceFound = true;
                            BetterAIInfoJob pLoopInfoJob = (BetterAIInfoJob)(infos.job(eLoopJob));

                            //TraitType eLoopTrait = pLoopInfoJob.mleEffectPlayerTraits[eEffectPlayer][0];
                            if (pLoopInfoJob.meCouncil != CouncilType.NONE)
                            {
                                BetterAICharacter pCouncilCharacter = (BetterAICharacter)player.councilCharacter(infos.job(eLoopJob).meCouncil);
                                if (pCouncilCharacter != null)
                                {
                                    TraitType eBestTrait = TraitType.NONE;
                                    int iBestTraitTurns = 0;
                                    foreach (TraitType eLoopTrait in pLoopInfoJob.mdlEffectPlayerTraits[eEffectPlayer])
                                    {
                                        if (pCouncilCharacter.isTrait(eLoopTrait))
                                        {
                                            int iTraitTurns = getTraitTurnsLeftEstimateX10(pCouncilCharacter, eLoopTrait);
                                            if (iTraitTurns > iBestTraitTurns)
                                            {
                                                iBestTraitTurns = iTraitTurns;
                                                eBestTrait = eLoopTrait;
                                            }
                                        }
                                    }
                                    iBestTraitTurns = Math.Min(iBestTraitTurns, iMaxTurnsX10);
                                    if (eBestTrait != TraitType.NONE)
                                    {
                                        CharactersTurnsLeftEstimateX10.Add(Math.Min(getJobTurnsLeftEstimateX10(pCouncilCharacter, eLoopJob), iBestTraitTurns));
                                    }
                                }
                            }
                            else if (eLoopJob == infos.Globals.GENERAL_JOB || eLoopJob == infos.Globals.GOVERNOR_JOB || eLoopJob == infos.Globals.AGENT_JOB)
                            {
                                bCheckAllCharacters = true;
                            }
                        }
                        if (!bCheckAllCharacters && pEffectPlayer.mseSourceTraitTraits.Count > 0)
                        {
                            bSourceFound = true;
                            bCheckAllCharacters = true;
                        }

                        if (bCheckAllCharacters)
                        {
                            using (var charListScoped = CollectionCache.GetListScoped<int>())
                            {
                                player.getActiveCharacters(charListScoped.Value);

                                foreach (int iLoopCharacter in charListScoped.Value)
                                {
                                    Character pLoopCharacter = game.character(iLoopCharacter);
                                    int iBestCharacterTurns = 0;

                                    if (pEffectPlayer.mseSourceTraitJobs.Contains(pLoopCharacter.getJob()))
                                    {
                                        BetterAIInfoJob pLoopCharacterInfoJob = (BetterAIInfoJob)(infos.job(pLoopCharacter.getJob()));
                                        TraitType eBestTrait = TraitType.NONE;
                                        int iBestTraitTurns = 0;
                                        foreach (TraitType eLoopTrait in pLoopCharacterInfoJob.mdlEffectPlayerTraits[eEffectPlayer])
                                        {
                                            if (pLoopCharacter.isTrait(eLoopTrait))
                                            {
                                                int iTraitTurns = getTraitTurnsLeftEstimateX10(pLoopCharacter, eLoopTrait);
                                                if (iTraitTurns > iBestTraitTurns)
                                                {
                                                    iBestTraitTurns = iTraitTurns;
                                                    eBestTrait = eLoopTrait;
                                                }
                                            }
                                        }
                                        if (eBestTrait != TraitType.NONE)
                                        {
                                            iBestCharacterTurns = Math.Min(getJobTurnsLeftEstimateX10(pLoopCharacter, pLoopCharacter.getJob()), iBestTraitTurns);
                                            //CharactersTurnsLeftEstimate.Add(Math.Min(getJobTurnsLeftEstimate(pLoopCharacter, pLoopCharacter.getJob()), iBestTraitTurns));
                                        }
                                    }

                                    foreach (TraitType eLoopCharacterTrait in pLoopCharacter.getTraits())
                                    {
                                        if (pEffectPlayer.mseSourceTraitTraits.Contains(eLoopCharacterTrait))
                                        {
                                            BetterAIInfoTrait pSourceInfoTrait = (BetterAIInfoTrait)(infos.trait(eLoopCharacterTrait));

                                            TraitType eBestTrait = TraitType.NONE;
                                            int iBestTraitTurns = 0;
                                            foreach (TraitType eLoopAdditionalTrait in pSourceInfoTrait.mleEffectPlayerTraits[eEffectPlayer])
                                            {
                                                if (pLoopCharacter.isTrait(eLoopAdditionalTrait))
                                                {
                                                    int iTraitTurns = getTraitTurnsLeftEstimateX10(pLoopCharacter, eLoopAdditionalTrait);
                                                    if (iTraitTurns > iBestTraitTurns)
                                                    {
                                                        iBestTraitTurns = iTraitTurns;
                                                        eBestTrait = eLoopAdditionalTrait;
                                                    }
                                                }
                                            }

                                            if (eBestTrait != TraitType.NONE)
                                            {
                                                iBestCharacterTurns = Math.Max(iBestCharacterTurns,
                                                    Math.Min(getTraitTurnsLeftEstimateX10(pLoopCharacter, eLoopCharacterTrait), iBestTraitTurns));
                                            }
                                        }
                                    }
                                    iBestCharacterTurns = Math.Min(iBestCharacterTurns, iMaxTurnsX10);
                                    CharactersTurnsLeftEstimateX10.Add(iBestCharacterTurns);
                                }
                            }
                        }
                    }

                    if (!bSourceFound) //only check EffectPlayer sources if no characters are found
                    {
                        foreach (EffectPlayerType eLoopEffectPlayer in pEffectPlayer.mseGetsUnlockedByEffectPlayers)
                        {
                            CharactersTurnsLeftEstimateX10.Add(getEffectPlayerTurnsRemainingX10(eLoopEffectPlayer, iMaxTurnsX10));
                        }
                        foreach ((EffectPlayerType, EffectPlayerType) eLoopEffectPlayerPair in pEffectPlayer.maeeSourceEffectPlayers)
                        {
                            CharactersTurnsLeftEstimateX10.Add(Math.Min(getEffectPlayerTurnsRemainingX10(eLoopEffectPlayerPair.Item1, iMaxTurnsX10), getEffectPlayerTurnsRemainingX10(eLoopEffectPlayerPair.Item2, iMaxTurnsX10)));
                        }
                    }
                    

                    if (CharactersTurnsLeftEstimateX10.Count > 0)
                    {
                        iRemainingEffectTurnsX10 = infos.utils().range((CharactersTurnsLeftEstimateX10.Sum()) / (CharactersTurnsLeftEstimateX10.Count), 0, AI_YIELD_TURNS);
                    }

                    return iRemainingEffectTurnsX10;
                }
            }

            //protection levels
            public virtual int getAI_YIELD_TURNS_X10()
            {
                return AI_YIELD_TURNS * 10;
            }

            protected virtual long effectPlayerValue(EffectPlayerType eEffectPlayer, ReligionType eStateReligion, bool bRemove, int iMaxTurnsRemainingX10, bool bSkipTurnsRemaining = true, bool bIncludeDependentEffects = true)
            {
                if (!bIncludeDependentEffects) return base.effectPlayerValue(eEffectPlayer, eStateReligion, bRemove);

                long iValue = 0;
                using (var effectPlayerIngoreListScoped = CollectionCache.GetListScoped<EffectPlayerType>())
                using (var effectPlayerCountChangeDictionaryScoped = CollectionCache.GetDictionaryScoped<EffectPlayerType, int>())
                using (var effectPlayerTurnsRemainingDictionaryScoped = CollectionCache.GetDictionaryScoped<EffectPlayerType, int>())
                {
                    List<EffectPlayerType> aeEffectPlayerIgnore = effectPlayerIngoreListScoped.Value;
                    Dictionary<EffectPlayerType, int> effectPlayerCountChange = effectPlayerCountChangeDictionaryScoped.Value;
                    Dictionary<EffectPlayerType, int> effectPlayerTurnsRemaining = effectPlayerTurnsRemainingDictionaryScoped.Value;

                    //ToDo: maybe cache effectPlayer values
                    ((BetterAIPlayer)player).getDependentEffectPlayerCountChangesWithTurnsRemaining(eEffectPlayer, iChange: 1, iMaxTurnsRemainingX10, ref aeEffectPlayerIgnore, ref effectPlayerCountChange, ref effectPlayerTurnsRemaining, bSkipTurnsRemaining: (!game.isCharacters()));

                    long iTurnsRemainingX10 = iMaxTurnsRemainingX10;
                    if (effectPlayerTurnsRemaining.ContainsKey(eEffectPlayer))
                    {
                        iTurnsRemainingX10 = effectPlayerTurnsRemaining[eEffectPlayer];
                    }

                    foreach (KeyValuePair<EffectPlayerType, int> eLoopEffectPlayerChangeCount in effectPlayerCountChange)
                    {
                        long iSubValue = base.effectPlayerValue(eLoopEffectPlayerChangeCount.Key, eStateReligion, bRemove) * eLoopEffectPlayerChangeCount.Value;
                        if (iSubValue != 0 && effectPlayerTurnsRemaining.ContainsKey(eLoopEffectPlayerChangeCount.Key) && iTurnsRemainingX10 > effectPlayerTurnsRemaining[eLoopEffectPlayerChangeCount.Key])
                        {
                            iSubValue *= effectPlayerTurnsRemaining[eLoopEffectPlayerChangeCount.Key];
                        }
                        else
                        {
                            iSubValue *= iTurnsRemainingX10;
                        }

                        iSubValue /= (AI_YIELD_TURNS * 10); //if remaing turns == AI_YIELD_TURNS, value is unchanged. If less, then it's scaled down.

                        iValue += iSubValue;
                    }
                }
                return iValue;
            }




            //lines 13344-15251
            protected override long bonusValue(BonusType eBonus, ref BonusParameters zParameters)
            {
                if (player == null)
                {
                    return 0;
                }
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers  START ###
  ##############################################*/
                //reducing Courtier values if they can't work by 2/3
                long iPlayerValue = 0;
                foreach ((CourtierType Key, GenderType Value) pair in infos.bonus(eBonus).maeAddCourtier)
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

                foreach ((CourtierType Key, GenderType Value) pair in infos.bonus(eBonus).maeAddCourtierOther)
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
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers    END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Oracle Bonus Evaluation          START ###
  ##############################################*/
                if (infos.bonus(eBonus).mbHolyCityAgents)
                {
                    iPlayerValue -= adjustForInflation(AI_HOLY_CITY_AGENT_VALUE); //subtract all of it, then add the modified number

                    //slight cheat because the player doesn't know when Pagan Religions get founded, but this should at least
                    // make Egypt pick another Wonder on Turn 1. Hopefully.
                    int iPossibleWorldReligions = 0;
                    int iWorldReligionsWithHolyCity = 0;
                    int iPossiblePaganReligions = 0;
                    int iPaganReligionsWithHolyCity = 0;
                    int iHolyCities = 0;
                    int iPossibleReligions = 0;

                    HashSet<int> nationSet = new HashSet<int>();

                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < game.getNumPlayers(); ++eLoopPlayer)
                    {
                        if (game.player(eLoopPlayer).isAlive() && game.player(eLoopPlayer).hasNation())
                        {
                            nationSet.Add((int)(game.player(eLoopPlayer).getNation()));
                        }
                    }

                    for (ReligionType eLoopReligion = 0; eLoopReligion < infos.religionsNum(); eLoopReligion++)
                    {
                        if (game.hasReligionHolyCity(eLoopReligion))
                        {
                            iHolyCities++;
                            iPossibleReligions++;
                            if (infos.religion(eLoopReligion).mePaganNation == NationType.NONE)
                            {
                                iWorldReligionsWithHolyCity++;
                                iPossibleWorldReligions++;
                            }
                            else
                            {
                                iPaganReligionsWithHolyCity++;
                                iPossiblePaganReligions++;
                            }
                        }
                        else if (!(game.isReligionFounded(eLoopReligion)))
                        {
                            if (infos.religion(eLoopReligion).mePaganNation == NationType.NONE)
                            {
                                iPossibleReligions++;
                                iPossibleWorldReligions++;
                            }
                            else if (nationSet.Contains((int)(infos.religion(eLoopReligion).mePaganNation)))
                            {
                                iPossibleReligions++;
                                iPossiblePaganReligions++;
                            }
                        }

                    }

                    if ((iPossibleReligions) != 0)
                    {
                        //World Religions contribute from 0 to 0.25 * AI_HOLY_CITY_AGENT_VALUE
                        //Pagan Religions contribute from -0.5 * AI_HOLY_CITY_AGENT_VALUE to +0.75 * AI_HOLY_CITY_AGENT_VALUE
                        //Total value therefore ranges from -0.5 * AI_HOLY_CITY_AGENT_VALUE when no Religion is founded, to AI_HOLY_CITY_AGENT_VALUE when they all have Holy Cities
                        iPlayerValue += (adjustForInflation(AI_HOLY_CITY_AGENT_VALUE) * (5 * iPaganReligionsWithHolyCity - 2 * iPossiblePaganReligions)) / (4 * iPossiblePaganReligions);
                        iPlayerValue += (adjustForInflation(AI_HOLY_CITY_AGENT_VALUE) * iWorldReligionsWithHolyCity) / (4 * iPossibleWorldReligions);
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### Oracle Bonus Evaluation            END ###
  ##############################################*/

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

            //lines 14763-14815
            protected override int getNeedSettlers(City pCity)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.getNeedSettlers");

/*####### Better Old World AI - Base DLL #######
  ### Segmented Territory Settler number START##
  ##############################################*/
                if (player == null) return 0;

                if (pCity == null)
                {
                    List<City> SeparatedCities = new List<City>();

                    bool bSeparated;
                    foreach (int iLoopCity in getCities())
                    {
                        City pLoopCity = game.city(iLoopCity);
                        if (pLoopCity != null)
                        {
                            bSeparated = true;
                            foreach (City pSeparateCity in SeparatedCities)
                            {
                                if (isTileReachable(pSeparateCity.tile(), pLoopCity.tile()))
                                {
                                    bSeparated = false;
                                    break;
                                }
                            }

                            if (bSeparated)
                            {
                                SeparatedCities.Add(pLoopCity);
                            }
                        }

                    }

                    int iValue = 0;
                    foreach (City pSeparateCity in SeparatedCities)
                    {
                        iValue += base.getNeedSettlers(pSeparateCity);
                    }
                    return iValue;
                }
/*####### Better Old World AI - Base DLL #######
  ### Segmented Territory Settler number  END ##
  ##############################################*/

                return base.getNeedSettlers(pCity);
            }

            //lines 12794-13239
            protected override long calculateEffectCityValue(EffectCityType eEffectCity, City pCity, bool bRemove)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.calculateEffectCityValue");

                if (player == null)
                {
                    return 0;
                }

                if (!player.canEverHaveEffectCity(eEffectCity))
                {
                    return 0;
                }

                int iExtraCount = bRemove ? -1 : 1;

                long calculateLuxuryValue(ResourceType eResource)
                {
                    long iValue = 0;

                    iValue += effectCityValue(infos.Globals.LUXURY_EFFECTCITY, pCity, bRemove);

                    if (pCity.hasFamily())
                    {
                        EffectCityType eLuxuryEffectCity = pCity.familyClass().maeLuxuryEffectCity[eResource];

                        if (eLuxuryEffectCity != EffectCityType.NONE)
                        {
                            iValue += effectCityValue(eLuxuryEffectCity, pCity, bRemove);
                        }
                    }

                    foreach (GoalData pGoalData in ((BetterAIPlayer)player).getGoalDataList())
                    {
                        if (!(pGoalData.mbFinished))
                        {
                            if (infos.goal(pGoalData.meType).miLuxuries > 0)
                            {
                                iValue += bonusValue(infos.Globals.FINISHED_AMBITION_BONUS);
                            }
                        }
                    }

                    return iValue;
                }

                BetterAIInfoEffectCity pInfoEffectCity = (BetterAIInfoEffectCity)infos.effectCity(eEffectCity);

                long iValue = 0;

                {
                    long iDefenseValue = 0;
                    if (pInfoEffectCity.miCityHP != 0)
                    {
                        iDefenseValue += pInfoEffectCity.miCityHP * AI_CITY_HP_VALUE;
                    }

/*####### Better Old World AI - Base DLL #######
  ### Less value than unit             START ###
  ##############################################*/
                    //iDefenseValue += (effectCity.miStrengthModifier * AI_UNIT_STRENGTH_MODIFIER_VALUE);
                    //iDefenseValue += (effectCity.miStrengthModifier * AI_TYPICAL_UNIT_STRENGTH_VALUE * AI_UNIT_STRENGTH_VALUE / 2);
                    iDefenseValue += (pInfoEffectCity.miStrengthModifier * AI_TYPICAL_UNIT_STRENGTH_VALUE * AI_UNIT_STRENGTH_VALUE / 10);
/*####### Better Old World AI - Base DLL #######
  ### Less value than unit               EMD ###
  ##############################################*/

                    if (isBorderCity(pCity))
                    {
                        iDefenseValue *= 5;
                    }
                    //else if (getCurrentMilitaryUnitNumber() >= getTargetMilitaryUnitNumber())
                    //{
                    //    iDefenseValue *= 3;
                    //}

                    iValue += iDefenseValue;
                }

                int iUnitsBuiltEstimate = pCity.calculateModifiedYield(infos.Globals.TRAINING_YIELD) * AI_CITY_UNITS_BUILT_PER_TRAINING / Constants.YIELDS_MULTIPLIER;

                iValue += (pInfoEffectCity.miUnitXP * AI_UNIT_XP_VALUE * iUnitsBuiltEstimate);
                iValue += (pInfoEffectCity.miUnitLevel * AI_UNIT_LEVEL_VALUE * iUnitsBuiltEstimate);
                iValue += (pInfoEffectCity.miUnitHeal * AI_UNIT_HEAL_VALUE * iUnitsBuiltEstimate);
                iValue += (pInfoEffectCity.miRangeChange * AI_UNIT_RANGE_VALUE);
                iValue += (pInfoEffectCity.miRandomPromotions * AI_UNIT_RANDOM_PROMOTION_VALUE * iUnitsBuiltEstimate);

                iValue += (pInfoEffectCity.miHurryDiscontentModifier * AI_CITY_HURRY_DISCONTENT_VALUE);
                if (pCity.hasLastPlayer())
                {
                    iValue += (pCity.lastPlayer().getEffectCityRebelProb(eEffectCity) * AI_CITY_REBEL_VALUE);
                }
                iValue += (pInfoEffectCity.miRegrowthModifier * AI_CITY_REGROWTH_VALUE);
                iValue += (pInfoEffectCity.miTradeValueModifier * AI_CITY_TRADE_VALUE);
                iValue -= (pInfoEffectCity.miImprovementCostModifier * AI_CITY_IMPROVEMENT_COST_VALUE);
                iValue -= (pInfoEffectCity.miAdjacentClassCostModifier * AI_CITY_ADJACENT_CLASS_COST_VALUE);
                iValue -= (pInfoEffectCity.miSpecialistCostModifier * AI_CITY_SPECIALIST_COST_VALUE);
                iValue -= (pInfoEffectCity.miSpecialistRuralTrainTimeModifier * AI_CITY_SPECIALIST_TRAIN_VALUE / 2);
                iValue -= (pInfoEffectCity.miSpecialistUrbanCostModifier * AI_CITY_SPECIALIST_COST_VALUE / 2);
                iValue -= (pInfoEffectCity.miSpecialistUrbanTrainTimeModifier * AI_CITY_SPECIALIST_TRAIN_VALUE / 2);
                iValue -= (pInfoEffectCity.miProjectCostModifier * AI_CITY_PROJECT_COST_VALUE);
                iValue -= (pInfoEffectCity.miBuildTurnChange * AI_WORKER_BUILD_VALUE);
                iValue -= (pInfoEffectCity.miUrbanBuildTurnChange * AI_WORKER_BUILD_VALUE / 2);

                if (pInfoEffectCity.mbNoBuildUnits)
                {
                    if (pCity.isNoBuildUnits() != pCity.isNoBuildUnits(iExtraCount))
                    {
                        iValue += AI_CITY_NO_UNIT_VALUE; // XXX
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### Better Hurry Unlock Evaluation   START ###
  ##############################################*/
                //if (effectCity.mbNoHurry)
                //{
                //    if (pCity.isNoHurry() != pCity.isNoHurry(iExtraCount))
                //    {
                //        iValue -= AI_CITY_HURRY_VALUE; // XXX
                //    }
                //}

                //if (effectCity.mbHurryCivics)
                //{
                //    if (pCity.isHurryCivics() != pCity.isHurryCivics(iExtraCount))
                //    {
                //        iValue += AI_CITY_HURRY_VALUE / 2; // XXX
                //    }
                //}

                //if (effectCity.mbHurryTraining)
                //{
                //    if (pCity.isHurryTraining() != pCity.isHurryTraining(iExtraCount))
                //    {
                //        iValue += AI_CITY_HURRY_VALUE / 2; // XXX
                //    }
                //}

                //if (effectCity.mbHurryMoney)
                //{
                //    if (pCity.isHurryMoney() != pCity.isHurryMoney(iExtraCount))
                //    {
                //        iValue += AI_CITY_HURRY_VALUE / 2; // XXX
                //    }
                //}

                //if (effectCity.mbHurryPopulation)
                //{
                //    if (pCity.isHurryPopulation() != pCity.isHurryPopulation(iExtraCount))
                //    {
                //        iValue += AI_CITY_HURRY_VALUE / 2; // XXX
                //    }
                //}

                //if (effectCity.mbHurryOrders)
                //{
                //    if (pCity.isHurryOrders() != pCity.isHurryOrders(iExtraCount))
                //    {
                //        iValue += AI_CITY_HURRY_VALUE / 2; // XXX
                //    }
                //}
/*####### Better Old World AI - Base DLL #######
  ### Better Hurry Unlock Evaluation     END ###
  ##############################################*/



                if (pInfoEffectCity.mbAutoBuild)
                {
                    if (pCity.isAutoBuild() != pCity.isAutoBuild(iExtraCount))
                    {
                        iValue += AI_CITY_AUTOBUILD_VALUE; // XXX
                    }
                }
                if (pInfoEffectCity.mbEnablesGovernor && game.isCharacters())
                {
                    if (((BetterAICity)pCity).isEnablesGovernor() != ((BetterAICity)pCity).isEnablesGovernor(iExtraCount))
                    {
                        iValue += AI_CITY_GOVERNOR_VALUE;
                    }
                }

                if (pInfoEffectCity.mbNoReligionSpread)
                {
                    if (pCity.isNoRandomReligionSpreadUnlock() != pCity.isNoRandomReligionSpreadUnlock(iExtraCount))
                    {
                        iValue += AI_RELIGION_VALUE; // XXX
                    }
                }


                if (pInfoEffectCity.meSpecialistNoPrereq != EffectCityType.NONE)
                {
                    if (pCity.isSpecialistNoPrereq(pInfoEffectCity.meSpecialistNoPrereq) != pCity.isSpecialistNoPrereq(pInfoEffectCity.meSpecialistNoPrereq, iExtraCount))
                    {
                        iValue += AI_CITY_SPECIALIST_UPGRADE_VALUE * pCity.getPopulation();
                    }
                }

                foreach (UnitType eUnit in pInfoEffectCity.maeBuildAnyReligionUnit)
                {
                    if (pCity.isBuildAnyReligionUnitUnlock(eUnit) != pCity.isBuildAnyReligionUnitUnlock(eUnit, iExtraCount))
                    {
                        iValue += AI_RELIGION_VALUE; // XXX
                    }
                }

                foreach (ImprovementClassType eImprovementClass in pInfoEffectCity.maeVoidTechPrereqImprovementClass)
                {
                    TechType ePrereq = infos.improvementClass(eImprovementClass).meTechPrereq;
                    if (ePrereq != TechType.NONE && !player.isTechAcquired(ePrereq))
                    {
                        if (pCity.isVoidTechPrereqUnlock(eImprovementClass) != pCity.isVoidTechPrereqUnlock(eImprovementClass, iExtraCount))
                        {
                            iValue += yieldValue(infos.Globals.SCIENCE_YIELD) * infos.tech(ePrereq).miCost;
                        }
                    }
                }


                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos.unitTraitsNum(); eLoopUnitTrait++)
                {
                    iValue += (pInfoEffectCity.maiUnitTraitXP[eLoopUnitTrait] * AI_UNIT_XP_VALUE / AI_UNIT_TRAITS);
                    iValue += (pInfoEffectCity.maiUnitTraitLevel[eLoopUnitTrait] * AI_UNIT_LEVEL_VALUE / AI_UNIT_TRAITS);
                    iValue += -(pInfoEffectCity.maiUnitTraitCostModifier[eLoopUnitTrait] * AI_UNIT_COST_VALUE / AI_UNIT_TRAITS);
                    iValue += -(pInfoEffectCity.maiUnitTraitTrainModifier[eLoopUnitTrait] * AI_TYPICAL_UNIT_STRENGTH_VALUE * AI_UNIT_COST_VALUE / AI_UNIT_TRAITS);
                    if (player.hasStateReligion())
                    {
                        iValue += -(pInfoEffectCity.maiStateReligionUnitTraitTrainModifier[eLoopUnitTrait] * AI_TYPICAL_UNIT_STRENGTH_VALUE * AI_UNIT_COST_VALUE / AI_UNIT_TRAITS / 2);
                    }
                }

                foreach (YieldType eLoopYield in infos.effectCity(eEffectCity).maeBuyTile)
                {
                    if (pCity.isBuyTileUnlock(eLoopYield) != pCity.isBuyTileUnlock(eLoopYield, iExtraCount))
                    {
                        iValue += 40 * AI_TILE_VALUE; // XXX
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### Better Hurry Unlock Evaluation   START ###
  ##############################################*/

                //foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryPopulation)
                //{
                //    if (!pCity.isHurryPopulation() && (pCity.isHurryPopulation(eLoopBuild) != pCity.isHurryPopulation(eLoopBuild, iExtraCount)))
                //    {
                //        iHurryValue += AI_CITY_HURRY_VALUE / 2;
                //    }
                //}

                //foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryCivics)
                //{
                //    if (!pCity.isHurryCivics() && (pCity.isHurryCivics(eLoopBuild) != pCity.isHurryCivics(eLoopBuild, iExtraCount)))
                //    {
                //        iHurryValue += AI_CITY_HURRY_VALUE / 2;
                //    }
                //}

                //foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryTraining)
                //{
                //    if (!pCity.isHurryTraining() && (pCity.isHurryTraining(eLoopBuild) != pCity.isHurryTraining(eLoopBuild, iExtraCount)))
                //    {
                //        iHurryValue += AI_CITY_HURRY_VALUE / 2;
                //    }
                //}

                //foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryMoney)
                //{
                //    if (!pCity.isHurryMoney() && (pCity.isHurryMoney(eLoopBuild) != pCity.isHurryMoney(eLoopBuild, iExtraCount)))
                //    {
                //        iHurryValue += AI_CITY_HURRY_VALUE / 2;
                //    }
                //}

                //foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryOrders)
                //{
                //    if (!pCity.isHurryOrders() && (pCity.isHurryOrders(eLoopBuild) != pCity.isHurryOrders(eLoopBuild, iExtraCount)))
                //    {
                //        iHurryValue += AI_CITY_HURRY_VALUE / 2;
                //    }
                //}
                //iValue += iHurryValue / (int)infos.buildsNum();

                int iTotalHurryValue = 0;

                if (pInfoEffectCity.mbNoHurry && pCity.isNoHurry() != pCity.isNoHurry(iExtraCount))
                {
                    iValue -= AI_CITY_HURRY_VALUE; // XXX
                }
                else
                {
                    if (pInfoEffectCity.mbHurryCivics)
                    {
                        if (pCity.isHurryCivics() != pCity.isHurryCivics(iExtraCount))
                        {
                            iTotalHurryValue += AI_CITY_HURRY_VALUE / 4; // XXX

                            int iHurryValue = 0;
                            for (BuildType eLoopBuild = 0; eLoopBuild < infos.buildsNum(); eLoopBuild++)
                            {
                                if (pCity.isHurryCivics(eLoopBuild))
                                {
                                    iHurryValue -= AI_CITY_HURRY_VALUE / 4;
                                }
                            }
                            iTotalHurryValue -= iHurryValue / (int)infos.buildsNum();
                        }
                    }
                    else if (!pCity.isHurryCivics())
                    {
                        int iHurryValue = 0;
                        foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryCivics)
                        {
                            if (!pCity.isHurryCivics() && (pCity.isHurryCivics(eLoopBuild) != pCity.isHurryCivics(eLoopBuild, iExtraCount)))
                            {
                                iHurryValue += AI_CITY_HURRY_VALUE / 4;
                            }
                        }
                        iTotalHurryValue += iHurryValue / (int)infos.buildsNum();
                    }

                    if (pInfoEffectCity.mbHurryTraining)
                    {
                        if (pCity.isHurryTraining() != pCity.isHurryTraining(iExtraCount))
                        {
                            iTotalHurryValue += AI_CITY_HURRY_VALUE / 3; // XXX

                            int iHurryValue = 0;
                            for (BuildType eLoopBuild = 0; eLoopBuild < infos.buildsNum(); eLoopBuild++)
                            {
                                if (pCity.isHurryTraining(eLoopBuild))
                                {
                                    iHurryValue -= AI_CITY_HURRY_VALUE / 3;
                                }
                            }
                            iTotalHurryValue -= iHurryValue / (int)infos.buildsNum();
                        }
                    }
                    else if (!pCity.isHurryTraining())
                    {
                        int iHurryValue = 0;
                        foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryCivics)
                        {
                            if (!pCity.isHurryTraining() && (pCity.isHurryTraining(eLoopBuild) != pCity.isHurryTraining(eLoopBuild, iExtraCount)))
                            {
                                iHurryValue += AI_CITY_HURRY_VALUE / 3;
                            }
                        }
                        iTotalHurryValue += iHurryValue / (int)infos.buildsNum();
                    }

                    if (pInfoEffectCity.mbHurryMoney)
                    {
                        if (pCity.isHurryMoney() != pCity.isHurryMoney(iExtraCount))
                        {
                            iTotalHurryValue += AI_CITY_HURRY_VALUE / 2; // XXX

                            int iHurryValue = 0;
                            for (BuildType eLoopBuild = 0; eLoopBuild < infos.buildsNum(); eLoopBuild++)
                            {
                                if (pCity.isHurryMoney(eLoopBuild))
                                {
                                    iHurryValue -= AI_CITY_HURRY_VALUE / 2;
                                }
                            }
                            iTotalHurryValue -= iHurryValue / (int)infos.buildsNum();
                        }
                    }
                    else if (!pCity.isHurryMoney())
                    {
                        int iHurryValue = 0;
                        foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryCivics)
                        {
                            if (!pCity.isHurryMoney() && (pCity.isHurryMoney(eLoopBuild) != pCity.isHurryMoney(eLoopBuild, iExtraCount)))
                            {
                                iHurryValue += AI_CITY_HURRY_VALUE / 2;
                            }
                        }
                        iTotalHurryValue += iHurryValue / (int)infos.buildsNum();
                    }

                    if (pInfoEffectCity.mbHurryPopulation)
                    {
                        if (pCity.isHurryPopulation() != pCity.isHurryPopulation(iExtraCount))
                        {
                            iTotalHurryValue += AI_CITY_HURRY_VALUE / 2; // XXX

                            int iHurryValue = 0;
                            for (BuildType eLoopBuild = 0; eLoopBuild < infos.buildsNum(); eLoopBuild++)
                            {
                                if (pCity.isHurryPopulation(eLoopBuild))
                                {
                                    iHurryValue -= AI_CITY_HURRY_VALUE / 2;
                                }
                            }
                            iTotalHurryValue -= iHurryValue / (int)infos.buildsNum();
                        }
                    }
                    else if (!pCity.isHurryPopulation())
                    {
                        int iHurryValue = 0;
                        foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryCivics)
                        {
                            if (!pCity.isHurryPopulation() && (pCity.isHurryPopulation(eLoopBuild) != pCity.isHurryPopulation(eLoopBuild, iExtraCount)))
                            {
                                iHurryValue += AI_CITY_HURRY_VALUE / 2;
                            }
                        }
                        iTotalHurryValue += iHurryValue / (int)infos.buildsNum();
                    }

                    if (pInfoEffectCity.mbHurryOrders)
                    {
                        if (pCity.isHurryOrders() != pCity.isHurryOrders(iExtraCount))
                        {
                            iTotalHurryValue += AI_CITY_HURRY_VALUE / 4; // XXX

                            int iHurryValue = 0;
                            for (BuildType eLoopBuild = 0; eLoopBuild < infos.buildsNum(); eLoopBuild++)
                            {
                                if (pCity.isHurryOrders(eLoopBuild))
                                {
                                    iHurryValue -= AI_CITY_HURRY_VALUE / 3;
                                }
                            }
                            iTotalHurryValue -= iHurryValue / (int)infos.buildsNum();
                        }
                    }
                    else if (!pCity.isHurryOrders())
                    {
                        int iHurryValue = 0;
                        foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryCivics)
                        {
                            if (!pCity.isHurryOrders() && (pCity.isHurryOrders(eLoopBuild) != pCity.isHurryOrders(eLoopBuild, iExtraCount)))
                            {
                                iHurryValue += AI_CITY_HURRY_VALUE / 3;
                            }
                        }
                        iTotalHurryValue += iHurryValue / (int)infos.buildsNum();
                    }

                    iValue += Math.Min(iTotalHurryValue, AI_CITY_HURRY_VALUE);

                }
/*####### Better Old World AI - Base DLL #######
  ### Better Hurry Unlock Evaluation     END ###
  ##############################################*/

                iValue = adjustForInflation(iValue);

                // evertything below needs to be already adjusted for inflation

                if (pInfoEffectCity.meNewUnitReligion != ReligionType.NONE)
                {
                    long iUnitReligionValue = 0;
                    int iNumUnits = 0;
                    for (UnitType eLoopUnit = 0; eLoopUnit < infos.unitsNum(); ++eLoopUnit)
                    {
                        if (pCity.canBuildUnit(eLoopUnit, false, false, false))
                        {
                            iUnitReligionValue += getUnitReligionValue(eLoopUnit, pInfoEffectCity.meNewUnitReligion);
                            ++iNumUnits;
                        }
                    }
                    if (iNumUnits > 0)
                    {
                        iValue += iUnitReligionValue * iUnitsBuiltEstimate / iNumUnits;
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### Better Unit Unlock Evaluation    START ###
  ##############################################*/
                //foreach (EffectCityType eLoopEffectCity in effectCity.maeFreeUnitEffectCity)
                //{
                //    if (pCity.isFreeUnitEffectCityUnlock(eLoopEffectCity) != pCity.isFreeUnitEffectCityUnlock(eLoopEffectCity, iExtraCount))
                //    {
                //        iValue += effectCityValue(eLoopEffectCity, pCity, bRemove);
                //    }
                //}
/*####### Better Old World AI - Base DLL #######
  ### Better Unit Unlock Evaluation    START ###
  ##############################################*/

                if (pCity.hasFamily())
                {
                    iValue += getFamilyOpinionValue(pCity.getFamily(), pInfoEffectCity.miFamilyOpinion);
                }

                if (pInfoEffectCity.mbAlwaysConnected)
                {
                    if (pCity.isAlwaysConnectedUnlock() != pCity.isAlwaysConnectedUnlock(iExtraCount))
                    {
                        iValue += getRoadValue(pCity, false, !pCity.tile().onTradeNetworkCapital(getPlayer()), bRemove, null);
                    }
                }

                if (pInfoEffectCity.mbLuxury)
                {
                    iValue += calculateLuxuryValue(pInfoEffectCity.meSourceResource);
                }

                foreach (EffectUnitType eLoopEffectUnit in pInfoEffectCity.maeFreeEffectUnit)
                {
                    if (pCity.isFreeEffectUnit(eLoopEffectUnit) != pCity.isFreeEffectUnit(eLoopEffectUnit, iExtraCount))
                    {
                        iValue += effectUnitValue(eLoopEffectUnit, UnitType.NONE) * iUnitsBuiltEstimate / AI_TURNS_BETWEEN_KILLS;
                    }
                }

                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos.unitTraitsNum(); eLoopUnitTrait++)
                {
                    EffectUnitType eEffectUnit = pInfoEffectCity.maeTraitEffectUnit[eLoopUnitTrait];
                    if (eEffectUnit != EffectUnitType.NONE)
                    {
                        if (!pCity.isFreeEffectUnit(eEffectUnit) && (pCity.isFreeTraitEffectUnit(eLoopUnitTrait, eEffectUnit) != pCity.isFreeTraitEffectUnit(eLoopUnitTrait, eEffectUnit, iExtraCount)))
                        {
                            iValue += effectUnitValue(eEffectUnit, UnitType.NONE) * iUnitsBuiltEstimate / AI_UNIT_TRAITS / AI_TURNS_BETWEEN_KILLS;
                        }
                    }
                }

                {
                    EffectCityType eEffectCityUnlock = pInfoEffectCity.meEffectCityUnlock;

                    if (eEffectCityUnlock != EffectCityType.NONE)
                    {
                        iValue += effectCityValue(eEffectCityUnlock, pCity, bRemove);
                    }
                }


                {
                    BonusType eCultureBonus = pInfoEffectCity.meCultureBonus;

                    if (eCultureBonus != BonusType.NONE)
                    {
                        BonusParameters zParameters = new BonusParameters(null);
                        zParameters.pTargetCity = pCity;
                        iValue += bonusValue(eCultureBonus, ref zParameters) * Math.Max(1, (int)(infos.culturesNum() - infos.Helpers.getCultureNumber(pCity.getCulture())));
                    }
                }

                foreach (ResourceType eLuxuryResource in pInfoEffectCity.maeLuxuryResources)
                {
                    iValue += calculateLuxuryValue(eLuxuryResource);
                }

                iValue += getLegitimacyValue(pInfoEffectCity.miLegitimacy, bIncludeYields: false);

                for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); eLoopYield++)
                {
                    int iExtraBaseYield = pCity.getEffectCityYieldRate(eEffectCity, eLoopYield, pCity.governor());

                    foreach (int iTileID in pCity.getTerritoryTiles())
                    {
                        Tile pLoopTile = game.tile(iTileID);
                        if (pLoopTile.hasImprovement())
                        {
                            iExtraBaseYield += getEffectCityTileYieldRate(eEffectCity, game.tile(iTileID), pLoopTile.getImprovement(), pLoopTile.getSpecialist(), eLoopYield, pCity);
                        }
                    }
                    foreach (EffectCityType eLoopEffectCity in pCity.getActiveEffectCity())
                    {
                        int iEffectCityYield = infos.effectCity(eLoopEffectCity).maaiEffectCityYieldRate[eEffectCity, eLoopYield];
                        if (iEffectCityYield != 0)
                        {
                            iExtraBaseYield += iEffectCityYield * pCity.getEffectCityCount(eLoopEffectCity);
                        }
                    }

                    if (isBorderCity(pCity))
                    {
                        iExtraBaseYield += pInfoEffectCity.maiYieldRateDefending[eLoopYield]; // assume it's going to be defended if it's a border city
                    }

                    int iExtraModifier = pCity.getEffectCityYieldModifier(eEffectCity, eLoopYield);
                    int iModifiedExtraYield = 0;
                    if (iExtraBaseYield != 0)
                    {
                        iModifiedExtraYield += infos.utils().modify(iExtraBaseYield, pCity.calculateTotalYieldModifier(eLoopYield) + iExtraModifier);
                    }
                    if (iExtraModifier != 0)
                    {
                        iModifiedExtraYield += infos.utils().modify(cityYield(eLoopYield, pCity), iExtraModifier);
                    }
                    if (iModifiedExtraYield != 0)
                    {
                        iValue += ((iModifiedExtraYield * cityYieldValue(eLoopYield, pCity) * AI_YIELD_TURNS) / Constants.YIELDS_MULTIPLIER);
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### Better Unit Unlock Evaluation    START ###
  ##############################################*/
                using (var unitTypelistScoped = CollectionCache.GetListScoped<UnitType>())
                {
                    List<UnitType> unitsUnlocked = unitTypelistScoped.Value;

                    for (UnitType eLoopUnit = 0; eLoopUnit < infos.unitsNum(); eLoopUnit++)
                    {
                        if (!(player.canBuildUnit(eLoopUnit))) continue;  //if we can't build it, none of this matters

                        iValue -= (pInfoEffectCity.maiUnitCostModifier[eLoopUnit] * AI_UNIT_COST_VALUE / 10);
                        iValue -= (pInfoEffectCity.maiUnitTrainModifier[eLoopUnit] * AI_UNIT_COST_VALUE / 10);

                        //if (pCity.getEffectCityCount(eEffectCity) == 0)
                        //{
                        //    if (infos.unit(eLoopUnit).meEffectCityPrereq == eEffectCity)
                        //    {
                        //        if (player.canBuildUnit(eLoopUnit))
                        //        {
                        //            iValue += Math.Max(0, unitValue(eLoopUnit, pCity, true));

                        //            long iBestExisting = 0;
                        //            for (UnitType eLoopExisting = 0; eLoopExisting < infos.unitsNum(); eLoopExisting++)
                        //            {
                        //                if (infos.unit(eLoopExisting).maeUpgradeUnit.Contains(eLoopUnit))
                        //                {
                        //                    iBestExisting = Math.Max(unitValue(eLoopExisting, pCity, true), iBestExisting);
                        //                }
                        //            }
                        //            iValue -= iBestExisting;
                        //        }
                        //    }
                        //}

                        bool bFreeUnitEffectCity = false;

                        foreach (EffectCityType eLoopEffectCity in pInfoEffectCity.maeFreeUnitEffectCity)
                        {
                            if (infos.unit(eLoopUnit).meEffectCityPrereq == eLoopEffectCity)
                            {
                                bFreeUnitEffectCity = true;
                                break;
                            }
                        }

                        if (infos.unit(eLoopUnit).meEffectCityPrereq == eEffectCity || bFreeUnitEffectCity)
                        {
                            if (((BetterAICity)pCity).isUnitEffectCityUnlock(eEffectCity) != ((BetterAICity)pCity).isUnitEffectCityUnlock(eEffectCity,
                                (infos.unit(eLoopUnit).meEffectCityPrereq == eEffectCity ? iExtraCount : 0), (bFreeUnitEffectCity ? iExtraCount : 0)))
                            {
                                unitsUnlocked.Add(eLoopUnit);
                            }
                        }
                    }

                    unitsUnlocked.Sort((x, y) => CompareUnitValue(x, y, pCity));

                    //foreach (UnitType eLoopUnlockedUnit in unitsUnlocked)
                    for (int i = 0; i < unitsUnlocked.Count; i++)
                    {
                        BetterAIInfoUnit pLoopUnlockedUnit = (BetterAIInfoUnit)infos.unit(unitsUnlocked[i]);

                        bool bBetterUpgrade = false;
                        //foreach (UnitType eLoopPossibleUpgradeUnit in unitsUnlocked)
                        for (int j = i + 1; j < unitsUnlocked.Count; j++)
                        {
                            if (pLoopUnlockedUnit.mseUpgradeUnitAccumulated.Contains(unitsUnlocked[j])
                                //&& unitValue(eLoopPossibleUpgradeUnit, pCity, true) > unitValue(eLoopUnlockedUnit, pCity, true)  //sidegrade possibilities: count each unlock separately. Skip value adding only if the other upgrade unit is actually better
                                )
                            {
                                bBetterUpgrade = true;
                                break;
                            }
                        }

                        if (!bBetterUpgrade)
                        {
                            //This part has no relevance in base game:
                            // units with EffectCityPrereq can't be upgraded from any unit without the same EffectCityPrereq
                            long iBestExisting = 0;
                            for (UnitType eLoopExisting = 0; eLoopExisting < infos.unitsNum(); eLoopExisting++)
                            {
                                if (((BetterAIInfoUnit)infos.unit(eLoopExisting)).mseUpgradeUnitAccumulated.Contains(unitsUnlocked[i])
                                    && infos.unit(eLoopExisting).meEffectCityPrereq != pLoopUnlockedUnit.meEffectCityPrereq  //these are already checked
                                    && player.canBuildUnit(unitsUnlocked[i]))  //no use looking at units we can't build
                                {
                                    iBestExisting = Math.Max(unitValue(eLoopExisting, pCity, -1, true), iBestExisting);
                                }
                            }
                            
                            iValue += infos.utils().modify(Math.Max(0, unitValue(unitsUnlocked[i], pCity, -1, true) - iBestExisting), cityYieldSpecializationModifier(pCity, pLoopUnlockedUnit.meProductionType)); ;
                        }
                    }

                }
/*####### Better Old World AI - Base DLL #######
  ### Better Unit Unlock Evaluation      END ###
  ##############################################*/

                foreach (GoalData pGoalData in ((BetterAIPlayer)player).getGoalDataList())
                {
                    if (!(pGoalData.mbFinished))
                    {
                        if ((infos.goal(pGoalData.meType).maiEffectCityCount[eEffectCity] > 0) ||
                            ((infos.goal(pGoalData.meType).maiEffectCityCount[eEffectCity] > 0) && (pGoalData.miCityID == ((pCity != null) ? pCity.getID() : -1))))
                        {
                            iValue += bonusValue(infos.Globals.FINISHED_AMBITION_BONUS);
                        }
                    }
                }

                if (pCity.hasPlayer() && ((pCity.getEffectCityCount(eEffectCity) > 0) != (pCity.getEffectCityCount(eEffectCity) + iExtraCount > 0)))
                {
                    for (ProjectType eLoopProject = 0; eLoopProject < infos.projectsNum(); ++eLoopProject)
                    {
                        InfoProject project = infos.project(eLoopProject);
                        if (project.meEffectCityPrereq == eEffectCity)
                        {
                            if (!project.mbHidden)
                            {
                                if (project.meTechPrereq != TechType.NONE && player.isTechAcquired(project.meTechPrereq))
                                {
                                    
                                    if (pCity.canBuildProject(eLoopProject, true, false, false))
                                    {
                                        iValue += projectValue(eLoopProject, pCity, false, false, true, true, false);
                                    }
                                }
                            }
                        }
                    }
                }

                for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos.improvementClassesNum(); eLoopImprovementClass++)
                {
                    if (pInfoEffectCity.maiImprovementClassUpgradeTurnChange[eLoopImprovementClass] != 0)
                    {
                        // XXX
                    }
                }

                for (ReligionType eLoopReligion = 0; eLoopReligion < infos.religionsNum(); eLoopReligion++)
                {
                    int iSubValue = pInfoEffectCity.maiReligionOpinion[eLoopReligion];
                    if (iSubValue != 0)
                    {
                        iValue += getReligionOpinionValue(eLoopReligion, iSubValue, AI_YIELD_TURNS);
                    }
                }

                for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos.improvementClassesNum(); eLoopImprovementClass++)
                {
                    if (pInfoEffectCity.mabNoImprovementClassMax[(int)eLoopImprovementClass])
                    {
                        // XXX
                    }
                }

                for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos.terrainsNum(); ++eLoopTerrain)
                {
                    if (pInfoEffectCity.mabUrbanTerrainValid[(int)eLoopTerrain])
                    {
                        iValue += adjustForInflation(pCity.getTerritoryTileCount(eLoopTerrain) * AI_BUILD_URBAN_VALUE);
                    }
                }

                foreach (InfoImprovement improvement in infos.improvements())
                {
                    if (improvement.meEffectCityPrereq == eEffectCity || improvement.maeEffectCityAnyPrereq.Contains(eEffectCity))
                    {
                        iValue += adjustForInflation(AI_CITY_IMPROVEMENT_VALUE);
                    }
                }

                return iValue;
            }

            public override long upgradeValue(UnitType eUnit, Unit pUnit)
            {
                long iNewValue = getUnitValue(eUnit);

                int iModifier = pUnit.getLevelPromotion() * 100 / infos.Globals.MAX_LEVELS;

                if (player.isUnitObsolete(pUnit.getType()))
                {
                    iModifier += 50;
                }

                // upgrade at a higher priority, if we're less likely to build new units
                if (isLatestMilitaryLandUnit(eUnit) && getTargetMilitaryUnitNumber() > 0)
                {
                    iModifier += Math.Min(50, 50 * getCurrentMilitaryUnitNumber() / getTargetMilitaryUnitNumber());
                }

/*####### Better Old World AI - Base DLL #######
  ### Water unit upgrade value: Area   START ###
  ##############################################*/
                //else if (isWarship(eUnit) && getWaterUnitTargetNumber(-1) > 0)
                //{
                //    if (getWaterUnitCurrentNumber(-1) >= getWaterUnitTargetNumber(-1))
                //    {
                //        iModifier += Math.Min(50, 50 * getWaterUnitCurrentNumber(-1) / getWaterUnitTargetNumber(-1));
                //    }
                //}
                else if (isWarship(eUnit))
                {
                    int iArea = pUnit.tile()?.getArea() ?? -1;
                    if (iArea != -1 && getWaterUnitTargetNumber(iArea) > 0)
                    {
                        if (getWaterUnitCurrentNumber(iArea) >= getWaterUnitTargetNumber(iArea))
                        {
                            iModifier += Math.Min(50, 50 * getWaterUnitCurrentNumber(iArea) / getWaterUnitTargetNumber(-1));
                        }
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### Water unit upgrade value: Area     END ###
  ##############################################*/

                return Math.Max(0, infos.utils().modify(iNewValue, iModifier) - getUnitValue(pUnit.getType()) - getUnitCostValue(eUnit, null));
            }


            public virtual int CompareUnitValue(UnitType x, UnitType y, City pCity)
            {
                return (int)(unitValue(x, pCity, -1, true) - unitValue(y, pCity, -1, true));
            }

            //lines 15046-15076
            protected override long getFamilyOpinionValue(FamilyType eFamily, int iOpinionChange, int iTurns, bool bNewFamily = false)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.getFamilyOpinionValue");

                if (player == null)
                {
                    return 0;
                }
                if (iOpinionChange == 0 || iTurns == 0)
                {
                    return 0;
                }
                if (!bNewFamily && !player.isFamilyStarted(eFamily))
                {
                    return 0;
                }

                long iValue = iOpinionChange * AI_FAMILY_OPINION_VALUE;
                if (AI_FAMILY_OPINION_VALUE_PER != 0)
                {
                    iValue += iOpinionChange * AI_FAMILY_OPINION_VALUE_PER * player.countFamilyCities(eFamily);
                }
                if (iTurns != AI_YIELD_TURNS)
                {
                    iValue *= iTurns;
                    iValue /= 2 * AI_YIELD_TURNS;  //because temporary opinion boosts decay over time
                }

                int iOpinionRate = player.getFamilyOpinionRate(eFamily) + (iOpinionChange < 0 ? iOpinionChange : 0);

                //highest level
                int iHighestThreshold = 0;
                for (OpinionFamilyType eLoopOpinion = infos.opinionFamiliesNum() - 2; eLoopOpinion >= 0; eLoopOpinion--)
                {
                    if (infos.opinionFamily(eLoopOpinion).miThreshold > 0)
                    {
                        iHighestThreshold = infos.opinionFamily(eLoopOpinion).miThreshold;
                        break;
                    }
                }
                if (!bNewFamily)
                {
                    if (iOpinionRate > iHighestThreshold + 140) //when opinion is +340 or above, there is no value to increasing it any further
                    {
                        return 0;
                    }
                    else if (iOpinionRate > iHighestThreshold + 41) //gradual reducion of value between +240 and +340, full value for below +240
                    {
                        iValue = infos.utils().modify(iValue, iHighestThreshold + 41 - iOpinionRate);
                    }
                    else if (iOpinionRate < 0) //gradual increase in value for opinion lower than 0, because rebels are bad
                    {
                        iValue = infos.utils().modify(iValue, -iOpinionRate);
                    }
                }

                return adjustForInflation(iValue);
            }

            //lines 16955-17023
            public override long improvementValueTile(ImprovementType eImprovement, Tile pTile, City pCity, bool bIncludeCost, bool bSubtractCurrent, bool bModified)
            {
                //using (new UnityProfileScope("PlayerAI.improvementValueTile"))
                {
                    if (player == null)
                    {
                        return -1;
                    }

                    City pImprovementCity = pCity ?? pTile.cityTerritory();
                    if (pImprovementCity == null)
                    {
                        if (getDistanceFromNationBorder(pTile) == 1)
                        {
                            for (DirectionType eDir = 0; eDir < DirectionType.NUM_TYPES && pImprovementCity == null; ++eDir)
                            {
                                Tile pAdjacent = pTile.tileAdjacent(eDir);
                                if (pAdjacent != null && pAdjacent.getOwner() == getPlayer())
                                {
                                    pImprovementCity = pAdjacent.cityTerritory(); // just take the first city, if adjacent to more than one
                                }
                            }
                        }
                    }
                    if (pImprovementCity == null)
                    {
                        pImprovementCity = player.findClosestCity(pTile);
                    }
                    if (pImprovementCity == null)
                    {
                        MohawkAssert.Assert(false, "Improvement without cities");
                        return -1;
                    }

                    long iValue;
                    bool bFound = bModified ? mpAICache.getImprovementModifiedValue(pTile.getID(), pImprovementCity.getID(), eImprovement, out iValue) : mpAICache.getImprovementBaseValue(pTile.getID(), pImprovementCity.getID(), eImprovement, out iValue);
                    if (!bFound)
                    {
                        MohawkAssert.Assert(AreCacheWarningsMuted, "AI improvement value not cached:" + infos.improvement(eImprovement).mzType + " Tile:" + pTile.getID());

                        iValue = calculateImprovementValueForTile(pTile, pImprovementCity, eImprovement);
                        if (iValue >= 0)
                        {
                            iValue += calculateImprovementDependentValueForTile(pTile, pImprovementCity, eImprovement);

                            mpAICache.setImprovementBaseValue(pTile.getID(), pImprovementCity.getID(), eImprovement, iValue);

                            if (bModified && iValue > 0)
                            {
                                modifyImprovementValue(eImprovement, pTile, pImprovementCity, ref iValue);
                                mpAICache.setImprovementModifiedValue(pTile.getID(), pImprovementCity.getID(), eImprovement, iValue);
                            }
                        }
                    }

                    if (iValue < 0)
                    {
                        return iValue;
                    }


                    if (infos.improvement(eImprovement).meUpgradeImprovement != ImprovementType.NONE)
                    {
                        CultureType eCulturePrereq = infos.improvement(infos.improvement(eImprovement).meUpgradeImprovement).meCulturePrereq;
                        if (eCulturePrereq == CultureType.NONE || !infos.Helpers.isCultureHigher(eCulturePrereq, pImprovementCity.getCulture()))
                        {
                            //iValue += improvementValue(infos.improvement(eImprovement).meUpgradeImprovement);
                            //just adding would result in double value
                            iValue = Math.Max(iValue, improvementValue(infos.improvement(eImprovement).meUpgradeImprovement));
                        }
                    }

                    if (bIncludeCost)
                    {
                        for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                        {
                            iValue -= infos.Helpers.getBuildCost(eImprovement, eLoopYield, pTile) * yieldValue(eLoopYield);
                        }

/*####### Better Old World AI - Base DLL #######
  ### Max vegetation remove value 0    START ###
  ##############################################*/
                        {
                            long iSubValue = 0;
                            long iLimit = 0;
                            if (infos.improvement(eImprovement).mbNoVegetation && pTile.hasVegetation())
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                                {
                                    int iRemoveYieldAmount = player.getYieldRemove(pTile, eLoopYield, true, pCity);
                                    iSubValue += iRemoveYieldAmount * yieldValue(eLoopYield);

                                    int iLostYieldAmount = getYieldLostFromClear(pTile, pCity, eLoopYield, pTile.getVegetation());
                                    if (pTile.vegetation().meVegetationRemove != VegetationType.NONE)
                                    {
                                        iLostYieldAmount += getYieldLostFromClear(pTile, pCity, eLoopYield, pTile.vegetation().meVegetationRemove);
                                    }
                                    iLimit += iLostYieldAmount * yieldValue(eLoopYield);
                                }

                                iSubValue -= pTile.vegetation().miBuildCost * yieldValue(infos.Globals.ORDERS_YIELD);
                            }

                            iValue += Math.Min(iLimit, iSubValue);
                        }
/*####### Better Old World AI - Base DLL #######
  ### Max vegetation remove value 0      END ###
  ##############################################*/
                    }

                    if (pTile.hasImprovement())
                    {
                        if (pTile.getImprovement() == eImprovement)
                        {
                            iValue += adjustForInflation(AI_EXISTING_IMPROVEMENT_VALUE); // encourage repair and discourage replacing improvements
                        }
                        else if (bSubtractCurrent)
                        {
                            if (mpAICache.getImprovementBaseValue(pTile.getID(), pImprovementCity.getID(), pTile.getImprovement(), out long iExistingValue))
                            {
                                iValue -= Math.Max(0, iExistingValue);
                            }

/*####### Better Old World AI - Base DLL #######
  ### ImprovementValue                 START ###
  ##############################################*/
                            //already part of getImprovementBaseValue
                            //if (pTile.hasSpecialist() && pCity != null)
                            //{
                            //    iValue -= Math.Max(0, specialistValue(pTile.getSpecialist(), pImprovementCity, pTile, pTile.getImprovement(), false, true, false));
                            //}
/*####### Better Old World AI - Base DLL #######
  ### ImprovementValue                   END ###
  ##############################################*/
                        }
                    }

                    return iValue;
                }
            }



            //copy-paste START
            //lines 17008-17183
            // Chance that we will declare war on them
            //I should do Peace and Truce too
            public override int getWarOfferPercent(PlayerType eOtherPlayer, bool bDeclare = true, bool bPreparedOnly = false, bool bCurrentPlayer = true)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.getWarOfferPercent");

                if (player == null)
                {
                    return 0;
                }

                BetterAIPlayer pOtherPlayer = (BetterAIPlayer)game.player(eOtherPlayer);
                PlayerType eThisPlayer = getPlayer();

                if (!(player.canDeclareWar(pOtherPlayer)))
                {
                    return 0;
                }

                if (pOtherPlayer.isGivingTributeToPlayer(eThisPlayer, AI_TRIBUTE_NO_WAR_TURNS))
                {
                    return 0;
                }

                for (DiplomacyType eDiplomacy = 0; eDiplomacy < infos.diplomaciesNum(); ++eDiplomacy)
                {
                    if (infos.diplomacy(eDiplomacy).mbHostile)
                    {
                        MemoryType eMemory = infos.diplomacy(eDiplomacy).mePlayerMemory;
                        if (eMemory != MemoryType.NONE)
                        {
                            if (player.countPlayerMemories(eMemory, PlayerType.NONE) > 0)
                            {
                                return 0;
                            }
                        }
                    }
                }

                ProximityType eProximity = pOtherPlayer.calculateProximityPlayer(eThisPlayer);
                PowerType eStrength = pOtherPlayer.calculateWarStrengthOf(eThisPlayer);

                bool bPlayToWin = game.isPlayToWinVs(eOtherPlayer);

                if (!bPlayToWin)
                {
                    if (game.isPlayToWinAny())
                    {
                        return 0;
                    }

                    if (bCurrentPlayer)
                    {
                        if (!isPlayerCityReachable(eOtherPlayer))
                        {
                            return 0;
                        }
                    }

                    if (bDeclare)
                    {
                        if (game.getTurn() < (20 + ((pOtherPlayer.isHuman()) ? game.opponentLevel().miStartWarMinTurn : 0)))
                        {
                            return 0;
                        }

                        if (!(pOtherPlayer.playerOpinionOfUs(eThisPlayer).mbDeclareWar))
                        {
                            return 0;
                        }

                        if (!(infos.proximity(eProximity).mbDeclareWar))
                        {
                            return 0;
                        }

                        if (!(infos.power(eStrength).mbDeclareWar))
                        {
                            return 0;
                        }

                        if (player.countTeamWars() > 1)
                        {
                            return 0;
                        }

                        if (bCurrentPlayer)
                        {
                            if (!areAllCitiesDefended())
                            {
                                return 0;
                            }

                            //what is this?
                            //foreach (int iLoopUnit in pOtherPlayer.getUnits())
                            //{
                            //    Unit pLoopUnit = game.unit(iLoopUnit);
                            //    if (pLoopUnit != null && pLoopUnit.isVisibleTo(player.getTeam()))
                            //    {
                            //        Tile pLoopTile = pLoopUnit.tile();
                            //        if (getTileProtection(pLoopTile) > 0 && getTileDanger(pLoopTile) > getTileProtection(pLoopTile))
                            //        {
                            //            return 0;
                            //        }
                            //    }
                            //}
                        }
                    }
                }

                if (bCurrentPlayer)
                {
                    if (getWarPreparingPlayer() != PlayerType.NONE)
                    {
                        if (getWarPreparingPlayer() != eOtherPlayer)
                        {
                            return 0;
                        }
                        if (bPreparedOnly)
                        {
                            return getWarPreparingTurns() > 0 ? 0 : 100;
                        }
                    }
                    else
                    {
                        if (bPreparedOnly && !bPlayToWin)
                        {
                            return 0;
                        }
                    }
                }

                int iPercent = pOtherPlayer.playerOpinionOfUs(eThisPlayer).miWarPercent;

/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer      START ###
  ##############################################*/
                int iMulti = 640;
                int iMultiHalf = 320;
                iPercent *= iMulti;
/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer        END ###
  ##############################################*/

                if (bPlayToWin && bCurrentPlayer)
                {
                    if (pOtherPlayer.isCloseToWinning(5))
                    {
                        iPercent = infos.utils().modify(iPercent, infos.Globals.PLAY_TO_WIN_WAR_PERCENT_MODIFIER_5);
                    }
                    else if (pOtherPlayer.isCloseToWinning(10))
                    {
                        iPercent = infos.utils().modify(iPercent, infos.Globals.PLAY_TO_WIN_WAR_PERCENT_MODIFIER_10);
                    }
                    else if (pOtherPlayer.isCloseToWinning(15))
                    {
                        iPercent = infos.utils().modify(iPercent, infos.Globals.PLAY_TO_WIN_WAR_PERCENT_MODIFIER_15);
                    }
                    else if (pOtherPlayer.isCloseToWinning(20))
                    {
                        iPercent = infos.utils().modify(iPercent, infos.Globals.PLAY_TO_WIN_WAR_PERCENT_MODIFIER_20);
                    }
                    else
                    {
                        iPercent = infos.utils().modify(iPercent, infos.Globals.PLAY_TO_WIN_WAR_PERCENT_MODIFIER_OTHER);
                    }
                }

                iPercent = infos.utils().modify(iPercent, infos.proximity(eProximity).miWarModifier);
                iPercent = infos.utils().modify(iPercent, infos.power(eStrength).miWarModifier);

                iPercent = infos.utils().modify(iPercent, game.teamDiplomacy(eThisPlayer, eOtherPlayer).miWarModifier);

                {
                    Character pLeader = player.leader();

                    if (pLeader != null)
                    {
                        foreach (TraitType eLoopTrait in pLeader.getTraits())
                        {
                            iPercent = infos.utils().modify(iPercent, infos.trait(eLoopTrait).miWarModifier);
                        }
                    }
                }

                if (pOtherPlayer.isHuman())
                {
                    iPercent = infos.utils().modify(iPercent, game.opponentLevel().miWarModifier);

                    if (game.getTurn() > 20)
                    {
                        int iScore = game.getTeamWarScoreTotalAll(pOtherPlayer.getTeam());

                        if (iScore < 10)
                        {
                            iPercent *= 5;
                            iPercent /= 4;
                        }
                        else if (iScore > 100)
                        {
                            if (!bPlayToWin)
                            {
                                iPercent *= 4;
                                iPercent /= 5;
                            }
                        }
                    }
                }

                iPercent *= (5 + pOtherPlayer.countTeamWars());
                iPercent /= (5 + 0);

                iPercent /= (player.countTeamWars() + (getWarPreparingPlayer() != PlayerType.NONE ? 2 : 1));

                if (game.hasTeamAlliance(pOtherPlayer.getTeam()))
                {
                    iPercent *= 4;
                    iPercent /= 5;
                }

                if (!bPlayToWin)
                {
                    if (bDeclare)
                    {
                        iPercent *= 2;
                        iPercent /= 3;
                    }

                    for (TeamType eLoopTeam = 0; eLoopTeam < game.getNumTeams(); eLoopTeam++)
                    {
                        if ((eLoopTeam != Team) && (eLoopTeam != pOtherPlayer.getTeam()))
                        {
                            if (game.isTeamAlive(eLoopTeam))
                            {
                                if (game.teamDiplomacy(eLoopTeam, Team).mbHostile && game.teamDiplomacy(eLoopTeam, pOtherPlayer.getTeam()).mbHostile)
                                {
                                    iPercent /= 2;
                                }
                            }
                        }
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer      START ###
  ##############################################*/
                iPercent = (iPercent + iMultiHalf) / iMulti; //rounding
/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer        END ###
  ##############################################*/

                if (bCurrentPlayer && isWarPreparing(pOtherPlayer.getPlayer()) && getWarPreparingTurns() <= 0)
                {
                    iPercent *= 2;
                }

                return infos.utils().range(iPercent, 0, 100);
            }
            //copy-paste END


            public override int getWarOfferPercent(TribeType eTribe)
            {
                //using var profileScope = new UnityProfileScope("PlayerAI.getWarOfferPercent");

                if (player == null)
                {
                    return 0;
                }

                PlayerType eThisPlayer = getPlayer();
                Tribe pTribe = game.tribe(eTribe);

                if (!(player.canDeclareWarTribe(eTribe)))
                {
                    return 0;
                }

                for (DiplomacyType eDiplomacy = 0; eDiplomacy < infos.diplomaciesNum(); ++eDiplomacy)
                {
                    if (infos.diplomacy(eDiplomacy).mbHostile)
                    {
                        MemoryType eMemory = infos.diplomacy(eDiplomacy).meTribeMemory;
                        if (eMemory != MemoryType.NONE)
                        {
                            if (player.countTribeMemories(eMemory) > 0)
                            {
                                return 0;
                            }
                        }
                    }
                }

                if (pTribe.hasPlayerAlly())
                {
                    return getWarOfferPercent(pTribe.getPlayerAlly());
                }

                //seriously what is this? Getting scared a bit too easily
                //foreach (int iLoopUnit in pTribe.getUnits())
                //{
                //    Unit pLoopUnit = game.unit(iLoopUnit);
                //    if (pLoopUnit != null && isUnitVisible(pLoopUnit))
                //    {
                //        Tile pLoopTile = pLoopUnit.tile();
                //        if (getTileProtection(pLoopTile) > 0 && getTileDanger(pLoopTile) > getTileProtection(pLoopTile))
                //        {
                //            return 0;
                //        }
                //    }
                //}

                int iPercent = player.tribeOpinion(eTribe).miWarPercent;

/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer      START ###
  ##############################################*/
                int iMulti = 640;
                int iMultiHalf = 320;
                iPercent *= iMulti;
/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer        END ###
  ##############################################*/

                iPercent = infos.utils().modify(iPercent, game.tribeDiplomacy(eTribe, eThisPlayer).miWarModifier);

                {
                    Character pLeader = player.leader();

                    if (pLeader != null)
                    {
                        foreach (TraitType eLoopTrait in pLeader.getTraits())
                        {
                            iPercent = infos.utils().modify(iPercent, infos.trait(eLoopTrait).miWarModifier);
                        }
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer      START ###
  ##############################################*/
                iPercent = (iPercent + iMultiHalf) / iMulti; //rounding
/*####### Better Old World AI - Base DLL #######
  ### more precision for WarOffer        END ###
  ##############################################*/

                return infos.utils().range(iPercent, 0, 100);
            }

        }
    }

}
