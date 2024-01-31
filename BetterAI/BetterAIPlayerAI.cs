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
            protected int AI_GROWTH_CITY_SPECIALIZATION_MODIFIER = 0;
            protected int AI_CIVICS_CITY_SPECIALIZATION_MODIFIER = 100;
            protected int AI_TRAINING_CITY_SPECIALIZATION_MODIFIER = 100;


            [SkipCheckSaveConsistency] protected BetterAIPlayerCache BAI_mpAICache = new BetterAIPlayerCache();

            public override void init(Game pGame, Player pPlayer, Tribe pTribe)
            {
                if (pGame != null)
                {
                    if (pGame.infos() != null)
                    {
                        Debug.Log("PlayerAI init");
                        loadAIValues(pGame.infos());
                    }
                    else
                    {
                        Debug.Log("PlayerAI init (pGame.infos() == null)");
                    }
                }
                else
                {
                    Debug.Log("PlayerAI init (pGame == null)");
                }

                base.init(pGame, pPlayer, pTribe);
                //BAI_mpAICache = new BetterAIPlayerCache();
            }

            public override void initClient(Game pGame, Player pPlayer, Tribe pTribe)
            {
                if (pGame != null)
                {
                    if (pGame.infos() != null)
                    {
                        Debug.Log("PlayerAI initClient");
                        loadAIValues(pGame.infos());
                    }
                    else
                    {
                        Debug.Log("PlayerAI initClient (pGame.infos() == null)");
                    }
                }
                else
                {
                    Debug.Log("PlayerAI initClient (pGame == null)");
                }

                base.initClient(pGame, pPlayer, pTribe);
                //BAI_mpAICache = new BetterAIPlayerCache();
            }


            public virtual void loadAIValues(Infos pInfos)
            {
                AI_MAX_WATER_CONTROL_DISTANCE = pInfos.getGlobalInt("AI_MAX_WATER_CONTROL_DISTANCE");
                AI_GROWTH_VALUE = pInfos.getGlobalInt("AI_GROWTH_VALUE");
                AI_CIVICS_VALUE = pInfos.getGlobalInt("AI_CIVICS_VALUE");
                AI_TRAINING_VALUE = pInfos.getGlobalInt("AI_TRAINING_VALUE");
                AI_CULTURE_VALUE = pInfos.getGlobalInt("AI_CULTURE_VALUE");
                AI_HAPPINESS_VALUE = pInfos.getGlobalInt("AI_HAPPINESS_VALUE");
                AI_SCIENCE_VALUE = pInfos.getGlobalInt("AI_SCIENCE_VALUE");
                AI_MONEY_VALUE = pInfos.getGlobalInt("AI_MONEY_VALUE");
                AI_ORDERS_VALUE = pInfos.getGlobalInt("AI_ORDERS_VALUE");
                AI_GOODS_VALUE = pInfos.getGlobalInt("AI_GOODS_VALUE");
                AI_NUM_GOODS_TARGET = pInfos.getGlobalInt("AI_NUM_GOODS_TARGET");
                AI_TILE_VALUE = pInfos.getGlobalInt("AI_TILE_VALUE");
                AI_GROWTH_CITY_MODIFIER = pInfos.getGlobalInt("AI_GROWTH_CITY_MODIFIER");
                AI_CIVICS_CITY_MODIFIER = pInfos.getGlobalInt("AI_CIVICS_CITY_MODIFIER");
                AI_TRAINING_CITY_MODIFIER = pInfos.getGlobalInt("AI_TRAINING_CITY_MODIFIER");
                AI_CITY_UNITS_BUILT_PER_TRAINING = pInfos.getGlobalInt("AI_CITY_UNITS_BUILT_PER_TRAINING");
                AI_UNIT_TRAITS = pInfos.getGlobalInt("AI_UNIT_TRAITS");
                AI_YIELD_TURNS = pInfos.getGlobalInt("AI_YIELD_TURNS");
                AI_TURNS_BETWEEN_KILLS = pInfos.getGlobalInt("AI_TURNS_BETWEEN_KILLS");
                AI_MIN_UNIT_BUILD_TURNS = pInfos.getGlobalInt("AI_MIN_UNIT_BUILD_TURNS");
                AI_HALF_VALUE_UNIT_BUILD_TURNS = pInfos.getGlobalInt("AI_HALF_VALUE_UNIT_BUILD_TURNS");
                AI_MIN_SPECIALIST_BUILD_TURNS = pInfos.getGlobalInt("AI_MIN_SPECIALIST_BUILD_TURNS");
                AI_HALF_VALUE_SPECIALIST_BUILD_TURNS = pInfos.getGlobalInt("AI_HALF_VALUE_SPECIALIST_BUILD_TURNS");
                AI_MIN_PROJECT_BUILD_TURNS = pInfos.getGlobalInt("AI_MIN_PROJECT_BUILD_TURNS");
                AI_HALF_VALUE_PROJECT_BUILD_TURNS = pInfos.getGlobalInt("AI_HALF_VALUE_PROJECT_BUILD_TURNS");
                AI_NO_WONDER_TURNS = pInfos.getGlobalInt("AI_NO_WONDER_TURNS");
                AI_GRACE_TURNS = pInfos.getGlobalInt("AI_GRACE_TURNS");
                AI_PLAY_TO_WIN_GRACE_TURNS = pInfos.getGlobalInt("AI_PLAY_TO_WIN_GRACE_TURNS");
                AI_CHARACTER_OPINION_VALUE = pInfos.getGlobalInt("AI_CHARACTER_OPINION_VALUE");
                AI_FAMILY_OPINION_VALUE = pInfos.getGlobalInt("AI_FAMILY_OPINION_VALUE");
                AI_RELIGION_OPINION_VALUE = pInfos.getGlobalInt("AI_RELIGION_OPINION_VALUE");
                AI_PLAYER_OPINION_VALUE = pInfos.getGlobalInt("AI_PLAYER_OPINION_VALUE");
                AI_TRIBE_OPINION_VALUE = pInfos.getGlobalInt("AI_TRIBE_OPINION_VALUE");
                AI_UNIT_ABILITIES_MULTIPLIER = pInfos.getGlobalInt("AI_UNIT_ABILITIES_MULTIPLIER");
                AI_SHIP_ABILITIES_MULTIPLIER = pInfos.getGlobalInt("AI_SHIP_ABILITIES_MULTIPLIER");
                AI_BUILD_TURNS_FOR_HELP = pInfos.getGlobalInt("AI_BUILD_TURNS_FOR_HELP");
                AI_SENTRY_MAX_DISTANCE = pInfos.getGlobalInt("AI_SENTRY_MAX_DISTANCE");
                AI_MIN_YIELD_STOCKPILE = pInfos.getGlobalInt("AI_MIN_YIELD_STOCKPILE");
                AI_MONEY_STOCKPILE_TURNS = pInfos.getGlobalInt("AI_MONEY_STOCKPILE_TURNS");
                AI_CITY_MIN_DANGER = pInfos.getGlobalInt("AI_CITY_MIN_DANGER");
                AI_STOCKPILE_TURN_BUFFER = pInfos.getGlobalInt("AI_STOCKPILE_TURN_BUFFER");
                AI_YIELD_SHORTAGE_PER_TURN_MODIFIER = pInfos.getGlobalInt("AI_YIELD_SHORTAGE_PER_TURN_MODIFIER");
                AI_ORDER_SHORTAGE_PER_TURN_MODIFIER = pInfos.getGlobalInt("AI_ORDER_SHORTAGE_PER_TURN_MODIFIER");
                AI_UNIT_STRENGTH_VALUE = pInfos.getGlobalInt("AI_UNIT_STRENGTH_VALUE");
                AI_UNIT_STRENGTH_MODIFIER_VALUE = pInfos.getGlobalInt("AI_UNIT_STRENGTH_MODIFIER_VALUE");
                AI_UNIT_ATTACK_MODIFIER_VALUE = pInfos.getGlobalInt("AI_UNIT_ATTACK_MODIFIER_VALUE");
                AI_UNIT_DEFENSE_MODIFIER_VALUE = pInfos.getGlobalInt("AI_UNIT_DEFENSE_MODIFIER_VALUE");
                AI_UNIT_CRITICAL_CHANCE_VALUE = pInfos.getGlobalInt("AI_UNIT_CRITICAL_CHANCE_VALUE");
                AI_UNIT_XP_VALUE = pInfos.getGlobalInt("AI_UNIT_XP_VALUE");
                AI_UNIT_HP_VALUE = pInfos.getGlobalInt("AI_UNIT_HP_VALUE");
                AI_UNIT_LEVEL_VALUE = pInfos.getGlobalInt("AI_UNIT_LEVEL_VALUE");
                AI_UNIT_WATER_CONTROL_VALUE = pInfos.getGlobalInt("AI_UNIT_WATER_CONTROL_VALUE");
                AI_UNIT_MOVEMENT_VALUE = pInfos.getGlobalInt("AI_UNIT_MOVEMENT_VALUE");
                AI_UNIT_VISION_VALUE = pInfos.getGlobalInt("AI_UNIT_VISION_VALUE");
                AI_UNIT_REVEAL_VALUE = pInfos.getGlobalInt("AI_UNIT_REVEAL_VALUE");
                AI_UNIT_FATIGUE_VALUE = pInfos.getGlobalInt("AI_UNIT_FATIGUE_VALUE");
                AI_UNIT_RANGE_VALUE = pInfos.getGlobalInt("AI_UNIT_RANGE_VALUE");
                AI_UNIT_MELEE_VALUE = pInfos.getGlobalInt("AI_UNIT_MELEE_VALUE");
                AI_UNIT_ROAD_MOVEMENT_VALUE = pInfos.getGlobalInt("AI_UNIT_ROAD_MOVEMENT_VALUE");
                AI_UNIT_PILLAGE_YIELD_VALUE = pInfos.getGlobalInt("AI_UNIT_PILLAGE_YIELD_VALUE");
                AI_UNIT_RIVER_ATTACK_VALUE = pInfos.getGlobalInt("AI_UNIT_RIVER_ATTACK_VALUE");
                AI_UNIT_WATER_LAND_ATTACK_VALUE = pInfos.getGlobalInt("AI_UNIT_WATER_LAND_ATTACK_VALUE");
                AI_UNIT_HOME_VALUE = pInfos.getGlobalInt("AI_UNIT_HOME_VALUE");
                AI_UNIT_SETTLEMENT_ATTACK_VALUE = pInfos.getGlobalInt("AI_UNIT_SETTLEMENT_ATTACK_VALUE");
                AI_UNIT_CONNECTED_ATTACK_VALUE = pInfos.getGlobalInt("AI_UNIT_CONNECTED_ATTACK_VALUE");
                AI_UNIT_CONNECTED_DEFENSE_VALUE = pInfos.getGlobalInt("AI_UNIT_CONNECTED_DEFENSE_VALUE");
                AI_UNIT_URBAN_ATTACK_VALUE = pInfos.getGlobalInt("AI_UNIT_URBAN_ATTACK_VALUE");
                AI_UNIT_URBAN_DEFENSE_VALUE = pInfos.getGlobalInt("AI_UNIT_URBAN_DEFENSE_VALUE");
                AI_UNIT_DAMAGED_US_VALUE = pInfos.getGlobalInt("AI_UNIT_DAMAGED_US_VALUE");
                AI_UNIT_DAMAGED_THEM_VALUE = pInfos.getGlobalInt("AI_UNIT_DAMAGED_THEM_VALUE");
                AI_UNIT_GENERAL_VALUE = pInfos.getGlobalInt("AI_UNIT_GENERAL_VALUE");
                AI_UNIT_FLANKING_VALUE = pInfos.getGlobalInt("AI_UNIT_FLANKING_VALUE");
                AI_UNIT_ADJACENT_SAME_VALUE = pInfos.getGlobalInt("AI_UNIT_ADJACENT_SAME_VALUE");
                AI_UNIT_HEAL_VALUE = pInfos.getGlobalInt("AI_UNIT_HEAL_VALUE");
                AI_UNIT_ROUT_VALUE = pInfos.getGlobalInt("AI_UNIT_ROUT_VALUE");
                AI_UNIT_PUSH_VALUE = pInfos.getGlobalInt("AI_UNIT_PUSH_VALUE");
                AI_UNIT_STUN_VALUE = pInfos.getGlobalInt("AI_UNIT_STUN_VALUE");
                AI_ENLIST_ON_KILL_VALUE = pInfos.getGlobalInt("AI_ENLIST_ON_KILL_VALUE");
                AI_UNIT_LAST_STAND_VALUE = pInfos.getGlobalInt("AI_UNIT_LAST_STAND_VALUE");
                AI_UNIT_IGNORE_DISTANCE_VALUE = pInfos.getGlobalInt("AI_UNIT_IGNORE_DISTANCE_VALUE");
                AI_UNIT_MELEE_COUNTER_VALUE = pInfos.getGlobalInt("AI_UNIT_MELEE_COUNTER_VALUE");
                AI_UNIT_CRITICAL_IMMUNE_VALUE = pInfos.getGlobalInt("AI_UNIT_CRITICAL_IMMUNE_VALUE");
                AI_UNIT_HEAL_NEUTRAL_VALUE = pInfos.getGlobalInt("AI_UNIT_HEAL_NEUTRAL_VALUE");
                AI_UNIT_HEAL_PILLAGE_VALUE = pInfos.getGlobalInt("AI_UNIT_HEAL_PILLAGE_VALUE");
                AI_UNIT_LAUNCH_OFFENSIVE_VALUE = pInfos.getGlobalInt("AI_UNIT_LAUNCH_OFFENSIVE_VALUE");
                AI_UNIT_ENLIST_NEXT_VALUE = pInfos.getGlobalInt("AI_UNIT_ENLIST_NEXT_VALUE");
                AI_UNIT_NO_ROAD_COOLDOWN_VALUE = pInfos.getGlobalInt("AI_UNIT_NO_ROAD_COOLDOWN_VALUE");
                AI_UNIT_REMOVE_VEGETATION_VALUE = pInfos.getGlobalInt("AI_UNIT_REMOVE_VEGETATION_VALUE");
                AI_UNIT_STRENGTH_TILE_VALUE = pInfos.getGlobalInt("AI_UNIT_STRENGTH_TILE_VALUE");
                AI_UNIT_HIDE_VEGETATION_VALUE = pInfos.getGlobalInt("AI_UNIT_HIDE_VEGETATION_VALUE");
                AI_UNIT_IGNORES_HEIGHT_COST_VALUE = pInfos.getGlobalInt("AI_UNIT_IGNORES_HEIGHT_COST_VALUE");
                AI_UNIT_ZOC_VALUE = pInfos.getGlobalInt("AI_UNIT_ZOC_VALUE");
                AI_UNIT_FORTIFY_VALUE = pInfos.getGlobalInt("AI_UNIT_FORTIFY_VALUE");
                AI_UNIT_PILLAGE_VALUE = pInfos.getGlobalInt("AI_UNIT_PILLAGE_VALUE");
                AI_UNIT_UNLIMBER_VALUE = pInfos.getGlobalInt("AI_UNIT_UNLIMBER_VALUE");
                AI_UNIT_ANCHOR_VALUE = pInfos.getGlobalInt("AI_UNIT_ANCHOR_VALUE");
                AI_UNIT_SETTLER_VALUE = pInfos.getGlobalInt("AI_UNIT_SETTLER_VALUE");
                AI_UNIT_SCOUT_VALUE = pInfos.getGlobalInt("AI_UNIT_SCOUT_VALUE");
                AI_UNIT_WORKER_VALUE = pInfos.getGlobalInt("AI_UNIT_WORKER_VALUE");
                AI_UNIT_PROMOTE_VALUE = pInfos.getGlobalInt("AI_UNIT_PROMOTE_VALUE");
                AI_UNIT_RANDOM_PROMOTION_VALUE = pInfos.getGlobalInt("AI_UNIT_RANDOM_PROMOTION_VALUE");
                AI_UNIT_COST_VALUE = pInfos.getGlobalInt("AI_UNIT_COST_VALUE");
                AI_UNIT_TARGET_NUMBER_WEIGHT = pInfos.getGlobalInt("AI_UNIT_TARGET_NUMBER_WEIGHT");
                AI_UNIT_TRAIT_TARGET_NUMBER_WEIGHT = pInfos.getGlobalInt("AI_UNIT_TRAIT_TARGET_NUMBER_WEIGHT");
                AI_RESOURCE_EXTRA_VALUE = pInfos.getGlobalInt("AI_RESOURCE_EXTRA_VALUE");
                AI_TRADE_NETWORK_VALUE_ESTIMATE = pInfos.getGlobalInt("AI_TRADE_NETWORK_VALUE_ESTIMATE");
                AI_SPECIALIST_FOUND_RELIGION_VALUE = pInfos.getGlobalInt("AI_SPECIALIST_FOUND_RELIGION_VALUE");
                AI_VP_VALUE = pInfos.getGlobalInt("AI_VP_VALUE");
                AI_MAX_ORDER_VALUE = pInfos.getGlobalInt("AI_MAX_ORDER_VALUE");
                AI_ALL_XP_VALUE = pInfos.getGlobalInt("AI_ALL_XP_VALUE");
                AI_IDLE_XP_VALUE = pInfos.getGlobalInt("AI_IDLE_XP_VALUE");
                AI_HARVEST_VALUE = pInfos.getGlobalInt("AI_HARVEST_VALUE");
                AI_WONDER_VALUE = pInfos.getGlobalInt("AI_WONDER_VALUE");
                AI_WORKER_BUILD_VALUE = pInfos.getGlobalInt("AI_WORKER_BUILD_VALUE");
                AI_TECHS_AVAILABLE_VALUE = pInfos.getGlobalInt("AI_TECHS_AVAILABLE_VALUE");
                AI_BUILD_URBAN_VALUE = pInfos.getGlobalInt("AI_BUILD_URBAN_VALUE");
                AI_IMPROVEMENT_UPGRADE_VALUE = pInfos.getGlobalInt("AI_IMPROVEMENT_UPGRADE_VALUE");
                AI_EXISTING_IMPROVEMENT_VALUE = pInfos.getGlobalInt("AI_EXISTING_IMPROVEMENT_VALUE");
                AI_RELIGION_VALUE = pInfos.getGlobalInt("AI_RELIGION_VALUE");
                AI_RELIGION_SPREAD_VALUE = pInfos.getGlobalInt("AI_RELIGION_SPREAD_VALUE");
                AI_FOUND_RELIGION_VALUE = pInfos.getGlobalInt("AI_FOUND_RELIGION_VALUE");
                AI_MULTIPLE_WORKERS_VALUE = pInfos.getGlobalInt("AI_MULTIPLE_WORKERS_VALUE");
                AI_AGENT_VALUE = pInfos.getGlobalInt("AI_AGENT_VALUE");
                AI_MERCENARY_VALUE = pInfos.getGlobalInt("AI_MERCENARY_VALUE");
                AI_CITY_HP_VALUE = pInfos.getGlobalInt("AI_CITY_HP_VALUE");
                AI_CITY_IMPROVEMENT_COST_VALUE = pInfos.getGlobalInt("AI_CITY_IMPROVEMENT_COST_VALUE");
                AI_CITY_ADJACENT_CLASS_COST_VALUE = pInfos.getGlobalInt("AI_CITY_ADJACENT_CLASS_COST_VALUE");
                AI_CITY_SPECIALIST_COST_VALUE = pInfos.getGlobalInt("AI_CITY_SPECIALIST_COST_VALUE");
                AI_CITY_SPECIALIST_TRAIN_VALUE = pInfos.getGlobalInt("AI_CITY_SPECIALIST_TRAIN_VALUE");
                AI_CITY_PROJECT_COST_VALUE = pInfos.getGlobalInt("AI_CITY_PROJECT_COST_VALUE");
                AI_CITY_HURRY_DISCONTENT_VALUE = pInfos.getGlobalInt("AI_CITY_HURRY_DISCONTENT_VALUE");
                AI_CITY_REBEL_VALUE = pInfos.getGlobalInt("AI_CITY_REBEL_VALUE");
                AI_CITY_AUTOBUILD_VALUE = pInfos.getGlobalInt("AI_CITY_AUTOBUILD_VALUE");
                AI_CITY_HURRY_VALUE = pInfos.getGlobalInt("AI_CITY_HURRY_VALUE");
                AI_CITY_GOVERNOR_VALUE = pInfos.getGlobalInt("AI_CITY_GOVERNOR_VALUE");
                AI_CITY_IMPROVEMENT_VALUE = pInfos.getGlobalInt("AI_CITY_IMPROVEMENT_VALUE");
                AI_ALLIANCE_VALUE = pInfos.getGlobalInt("AI_ALLIANCE_VALUE");
                AI_DIPLOMACY_VALUE = pInfos.getGlobalInt("AI_DIPLOMACY_VALUE");
                AI_INVASION_VALUE = pInfos.getGlobalInt("AI_INVASION_VALUE");
                AI_COURTIER_VALUE = pInfos.getGlobalInt("AI_COURTIER_VALUE");
                AI_HOLY_CITY_AGENT_VALUE = pInfos.getGlobalInt("AI_HOLY_CITY_AGENT_VALUE");
                AI_CHARACTER_XP_VALUE = pInfos.getGlobalInt("AI_CHARACTER_XP_VALUE");
                AI_WAR_DEFEND_STRENGTH_PERCENT = pInfos.getGlobalInt("AI_WAR_DEFEND_STRENGTH_PERCENT");
                AI_SUCCESSION_CHANGE_MODIFIER = pInfos.getGlobalInt("AI_SUCCESSION_CHANGE_MODIFIER");
                AI_NO_DIPLOMACY_TURNS = pInfos.getGlobalInt("AI_NO_DIPLOMACY_TURNS");
                AI_TRIBUTE_NO_WAR_TURNS = pInfos.getGlobalInt("AI_TRIBUTE_NO_WAR_TURNS");
                AI_NO_UNIT_BUILD_PERCENT = pInfos.getGlobalInt("AI_NO_UNIT_BUILD_PERCENT");
                AI_MAX_NUM_WORKERS_PER_HUNDRED_CITIES = pInfos.getGlobalInt("AI_MAX_NUM_WORKERS_PER_HUNDRED_CITIES");
                AI_MAX_NUM_WORKERS_PER_HUNDRED_ORDERS = pInfos.getGlobalInt("AI_MAX_NUM_WORKERS_PER_HUNDRED_ORDERS");
                AI_MAX_NUM_DISCIPLES_PER_HUNDRED_CITIES = pInfos.getGlobalInt("AI_MAX_NUM_DISCIPLES_PER_HUNDRED_CITIES");
                AI_MAX_NUM_DISCIPLES_PER_HUNDRED_ORDERS = pInfos.getGlobalInt("AI_MAX_NUM_DISCIPLES_PER_HUNDRED_ORDERS");
                AI_HEIR_TUTOR_MODIFIER = pInfos.getGlobalInt("AI_HEIR_TUTOR_MODIFIER");
                AI_COURTIER_JOB_MODIFIER = pInfos.getGlobalInt("AI_COURTIER_JOB_MODIFIER");
                AI_WASTED_EFFECT_VALUE = pInfos.getGlobalInt("AI_WASTED_EFFECT_VALUE");
                AI_MAX_FORT_BORDER_DISTANCE_OUTSIDE = pInfos.getGlobalInt("AI_MAX_FORT_BORDER_DISTANCE_OUTSIDE");
                AI_MAX_FORT_BORDER_DISTANCE_INSIDE = pInfos.getGlobalInt("AI_MAX_FORT_BORDER_DISTANCE_INSIDE");
                AI_MAX_FORT_RIVAL_BORDER_DISTANCE = pInfos.getGlobalInt("AI_MAX_FORT_RIVAL_BORDER_DISTANCE");
                AI_NUM_PRIORITY_EXPANSION_TARGETS = pInfos.getGlobalInt("AI_NUM_PRIORITY_EXPANSION_TARGETS");
                AI_WAR_PREPARING_TURNS = pInfos.getGlobalInt("AI_WAR_PREPARING_TURNS");

                AI_GROWTH_CITY_SPECIALIZATION_MODIFIER = pInfos.getGlobalInt("AI_GROWTH_CITY_SPECIALIZATION_MODIFIER");
                AI_CIVICS_CITY_SPECIALIZATION_MODIFIER = pInfos.getGlobalInt("AI_CIVICS_CITY_SPECIALIZATION_MODIFIER");
                AI_TRAINING_CITY_SPECIALIZATION_MODIFIER = pInfos.getGlobalInt("AI_TRAINING_CITY_SPECIALIZATION_MODIFIER");
            }

            //lines 8034-8058
            protected override long getHurryCostValue(City pCity, CityBuildHurryType eHurry)
            {
                long iValue = 0;
                if (player == null) return iValue;

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
                        iValue = pCity.getHurryPopulationCost() * citizenValue(pCity, true);
                        break;
                }

                iValue -= cityYieldValueFlat(infos.Globals.DISCONTENT_YIELD, pCity) * pCity.getHurryDiscontent() / Constants.YIELDS_MULTIPLIER;

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

            public override void refreshCachedValues()
            {
                base.refreshCachedValues();
                BAI_mpAICache.clear();
            }

            //lines 1362-1365
            protected override void cacheCityYieldValue(YieldType eYield, City pCity)
            {
                mpAICache.setCityYieldValue(eYield, pCity.getID(), calculateCityYieldValue(eYield, pCity, out long iValueFlat));
                BAI_mpAICache.setCityYieldValueFlat(eYield, pCity.getID(), iValueFlat);
            }

            protected override void cacheTileImprovementValues(Tile pTile, City pCity, bool bIncludeBonus)
            {
/*####### Better Old World AI - Base DLL #######
  ### Debug: verbose logging           START ###
  ##############################################*/
                bool isValidPing(ImprovementType eImprovement, ImprovementType ePing)
                {
                    if (ePing == eImprovement || ePing == infos.improvementsNum())
                    {
                        return true;
                    }
                    else if (ePing == ImprovementType.NONE)
                    {
                        if (!pTile.hasResource() || infos.Helpers.isImprovementResourceValid(eImprovement, pTile.getResource()))
                        {
                            return true;
                        }
                    }
                    else if (infos.improvement(ePing).meClass != ImprovementClassType.NONE)
                    {
                        if (pTile.getImprovementClass() == infos.improvement(ePing).meClass)
                        {
                            return true;
                        }
                    }
                    return false;
                }

                bool bValid;
                for (ImprovementType eImprovement = 0; eImprovement < infos.improvementsNum(); ++eImprovement)
                {
                    bValid = false;
/*####### Better Old World AI - Base DLL #######
  ### Debug: verbose logging             END ###
  ##############################################*/
                    if (pTile.getImprovement() == eImprovement || canStartImprovement(eImprovement, pTile, bTestTech: false))
                    {
                        if (pTile.cityTerritory() == pCity || !infos.improvement(eImprovement).mbTerritoryOnly || pTile.isValidImprovementTerrain(eImprovement, true))
                        {
                            updateTileImprovementValue(pTile, pCity, eImprovement, bIncludeBonus);
                            bValid = true;
                        }
                    }

/*####### Better Old World AI - Base DLL #######
  ### Debug: verbose logging           START ###
  ##############################################*/
                    if (!bValid)
                    {
                        if (isValidPing(eImprovement, player.getTileImprovementPing(pTile.getID())))
                        {
                            bool bCanFinish = pTile.getImprovement() == eImprovement && !pTile.hasImprovementFinished() && !pTile.isPillaged() && (player.isMultipleWorkersUnlock() || pTile.countUnitsImproving() == 0);
                            bool bWonder = infos.improvement(eImprovement).mbWonder;
                            if (bCanFinish || player.canStartImprovementOnTile(pTile, eImprovement, bBuyGoods: true, bTestGoods: bWonder, bTestEnabled: true, bTestTerritory: false, bTestTech: bWonder, bTestAdjacent: true, bTestReligion: true))
                            {
                                Debug.Log(infos.improvement(eImprovement).mzType + (pCity != null ? " in " + pCity.getName() : "") + " on Tile (" + pTile.getX().ToString() + "," + pTile.getY().ToString() + ") should have been cached. Terrain: " + pTile.terrain().mzType +
                                    " / Height: " + pTile.height().mzType + (pTile.getVegetation() != VegetationType.NONE ? " / Vegetation: " + pTile.vegetation().mzType : " / no vegetation") + (pTile.getResource() != ResourceType.NONE ? " / Resource: " + pTile.resource().mzType : " / no Resource")) ;
                                updateTileImprovementValue(pTile, pCity, eImprovement, bIncludeBonus);
                            }
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### Debug: verbose logging             END ###
  ##############################################*/
                }
            }

            protected override void addTileImprovementExtraValues(Tile pTile, City pCity)
            {
/*####### Better Old World AI - Base DLL #######
  ### Debug: verbose logging           START ###
  ##############################################*/
                bool isValidPing(ImprovementType eImprovement, ImprovementType ePing)
                {
                    if (ePing == eImprovement || ePing == infos.improvementsNum())
                    {
                        return true;
                    }
                    else if (ePing == ImprovementType.NONE)
                    {
                        if (!pTile.hasResource() || infos.Helpers.isImprovementResourceValid(eImprovement, pTile.getResource()))
                        {
                            return true;
                        }
                    }
                    else if (infos.improvement(ePing).meClass != ImprovementClassType.NONE)
                    {
                        if (pTile.getImprovementClass() == infos.improvement(ePing).meClass)
                        {
                            return true;
                        }
                    }
                    return false;
                }


                bool bValid;
                for (ImprovementType eImprovement = 0; eImprovement < infos.improvementsNum(); ++eImprovement)
                {
                    bValid = false;
/*####### Better Old World AI - Base DLL #######
  ### Debug: verbose logging             END ###
  ##############################################*/
                    if (pTile.getImprovement() == eImprovement || canStartImprovement(eImprovement, pTile, bTestTech: false))
                    {
                        if (pTile.cityTerritory() == pCity || !infos.improvement(eImprovement).mbTerritoryOnly || pTile.isValidImprovementTerrain(eImprovement, true))
                        {
                            addTileImprovementExtraValue(pTile, pCity, eImprovement);
                            bValid = true;
                        }
                    }

/*####### Better Old World AI - Base DLL #######
  ### Debug: verbose logging           START ###
  ##############################################*/
                    if (!bValid)
                    {
                        if (isValidPing(eImprovement, player.getTileImprovementPing(pTile.getID())))
                        {
                            bool bCanFinish = pTile.getImprovement() == eImprovement && !pTile.hasImprovementFinished() && !pTile.isPillaged() && (player.isMultipleWorkersUnlock() || pTile.countUnitsImproving() == 0);
                            bool bWonder = infos.improvement(eImprovement).mbWonder;
                            if (bCanFinish || player.canStartImprovementOnTile(pTile, eImprovement, bBuyGoods: true, bTestGoods: bWonder, bTestEnabled: true, bTestTerritory: false, bTestTech: bWonder, bTestAdjacent: true, bTestReligion: true))
                            {
                                Debug.Log("This should have been cached (extra)");
                                addTileImprovementExtraValue(pTile, pCity, eImprovement);
                            }
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### Debug: verbose logging             END ###
  ##############################################*/
                }
            }

/*####### Better Old World AI - Base DLL #######
  ### AI: City Yield Values            START ###
  ##############################################*/
            //lines 4294-4322
            public override long cityYieldValue(YieldType eYield, City pCity)
            {
                return cityYieldValues(eYield, pCity, out _);
            }

            public virtual long cityYieldValueFlat(YieldType eYield, City pCity)
            {
                cityYieldValues(eYield, pCity, out long iValueFlat);
                return iValueFlat;
            }

            public virtual long cityYieldValues(YieldType eYield, City pCity, out long iValueFlat)
            {
                using var profileScope = new UnityProfileScope("PlayerAI.cityYieldValueBoth");

                long iValue;
                if (pCity == null)
                {
                    iValue = getBaseYieldValue(eYield);
                    iValueFlat = iValue;
                }
                else
                {
                    lock (gameCacheLock)
                    {
                        if (mpAICache.getCityYieldValue(eYield, pCity.getID(), out iValue) && BAI_mpAICache.getCityYieldValueFlat(eYield, pCity.getID(), out iValueFlat))
                        {
                            if (BAI_mpAICache.getCityYieldValueFlat(eYield, pCity.getID(), out iValueFlat))
                            {
                                return iValue;
                            }
/*####### Better Old World AI - Base DLL #######
  ### Debug: verbose logging           START ###
  ##############################################*/
                            else
                            {
                                Debug.Log("Unexpected AI city yield value (only Flat value missing) calculation [Mod]. City: " + pCity.getName() + " / Yield: " + infos.yield(eYield).mzType + Environment.StackTrace.ToString());

                            }
                        }
                        else
                        {
                            Debug.Log("Unexpected AI city yield value calculation [Mod]. City: " + pCity.getName() + " / Yield: " + infos.yield(eYield).mzType + Environment.StackTrace.ToString());
                        }
/*####### Better Old World AI - Base DLL #######
  ### Debug: verbose logging             END ###
  ##############################################*/
                    }

                    //MohawkAssert.Assert(AreCacheWarningsMuted, "Unexpected AI city yield value calculation [Mod]");
                    iValue = calculateCityYieldValue(eYield, pCity, out iValueFlat);
                    lock (gameCacheLock)
                    {
                        mpAICache.setCityYieldValue(eYield, pCity.getID(), iValue);
                        BAI_mpAICache.setCityYieldValueFlat(eYield, pCity.getID(), iValueFlat);
                    }
                }
                return iValue;
            }

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


            //lines 4141-4240
            //public override long calculateCityYieldValue(YieldType eYield, City pCity)
            public virtual long calculateCityYieldValue(YieldType eYield, City pCity, out long iValueFlat)
            {
                using var profileScope = new UnityProfileScope("PlayerAI.calculateCityYieldValue");

                long iValue = 0;

                if (infos.yield(eYield).meSubtractFromYield != YieldType.NONE)
                {
                    //return -(cityYieldValue(infos.yield(eYield).meSubtractFromYield, pCity));
                    iValue = cityYieldValues(infos.yield(eYield).meSubtractFromYield, pCity, out iValueFlat);

                    iValueFlat = -iValueFlat;
                    return infos.utils().modify(iValueFlat, pCity.calculateTotalYieldModifier(eYield));
                }

                int iModifier = 0;


                iValue = yieldValue(eYield);

                if (pCity != null && player != null)
                {
                    int iYieldRate = pCity.calculateModifiedYield(eYield);
                    int iYieldRatePlayer = 0;
                    int iNumPlayerCities = 0;
                    int iYieldRateAverage = 0;
                    int iSpecializationModifier = 0;

                    //encourage spezialization
                    if (eYield == infos.Globals.CIVICS_YIELD || eYield == infos.Globals.TRAINING_YIELD)
                    {
                        foreach (int iCityID in getCities())
                        {
                            City pLoopCity = game.city(iCityID);
                            if (pLoopCity != null)
                            {
                                iYieldRatePlayer += pLoopCity.calculateModifiedYield(eYield);
                                iNumPlayerCities++;
                            }
                        }
                    }
                    if (iNumPlayerCities == 0)
                    {
                        iNumPlayerCities = 1;
                        iYieldRateAverage = 1;
                    }
                    else
                    {
                        iYieldRateAverage = (iYieldRatePlayer + iNumPlayerCities - 1) / iNumPlayerCities;
                    }

                    if (eYield == infos.Globals.GROWTH_YIELD)
                    {

                        //if (isExpandPriority())
                        // isExpandPriority is global, we should look at how many Settlers we need right here
                        bool bNeedsGrowth = false;
                        if (getNeedSettlers(pCity) > 0)

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

                        //using modifiers here already, to then compare to citizen value fraction
                        iValue = infos.utils().modify(iValue, (iValue > 0) ? iModifier : -(iModifier));
                        iModifier = 0;
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
                                iValue -= ((pCity.getCitizens() - 2) * yieldValue(infos.Globals.GROWTH_YIELD)) / pCity.getCitizens();  //overpopulation
                                //iValue += ((pCity.getCitizens() - 2) * citizenValue(pCity, false)) / (pCity.getYieldThresholdWhole(infos.Globals.GROWTH_YIELD) * pCity.getCitizens()); //this causes unexpected city effect calculation
                            }
                        }

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
                                iModifier += 50 - iYieldRate;
                            }
                            iModifier += 25 - iYieldRate / 10;
                        }


                        foreach (GoalData pGoalData in ((BetterAIPlayer)player).getGoalDataList())
                        {
                            if (!(pGoalData.mbFinished))
                            {
                                for (CultureType eCulture = 0; eCulture < infos.culturesNum(); ++eCulture)
                                {
                                    if (infos.goal(pGoalData.meType).maiCultureCount[eCulture] > 0 && pCity.getCulture() < eCulture)
                                    {
                                        iModifier += 100;
                                    }
                                }
                            }
                        }
                    }
                    else if (eYield == infos.Globals.CIVICS_YIELD)
                    {
                        //iModifier += infos.utils().modify(AI_CIVICS_CITY_MODIFIER, pCity.calculateTotalYieldModifier(eYield));
                        iModifier += AI_CIVICS_CITY_MODIFIER;

                        //encourage specialization
                        if (iNumPlayerCities > 1 && iYieldRate > iYieldRateAverage)
                        {
                            iSpecializationModifier = (AI_CIVICS_CITY_SPECIALIZATION_MODIFIER * iYieldRate) / iYieldRateAverage;
                            infos.utils().modify(iSpecializationModifier, -(100 / iNumPlayerCities));
                            infos.utils().modify(iValue, iSpecializationModifier);
                        }
                    }
                    else if (eYield == infos.Globals.TRAINING_YIELD)
                    {
                        //int iModifierModifier = isPeaceful() ? AI_TRAINING_CITY_MODIFIER : 3 * AI_TRAINING_CITY_MODIFIER / 2;
                        //iModifier += infos.utils().modify(iModifierModifier, pCity.calculateTotalYieldModifier(eYield));
                        iModifier += isPeaceful() ? AI_TRAINING_CITY_MODIFIER : 3 * AI_TRAINING_CITY_MODIFIER / 2;

                        //encourage specialization
                        if (iNumPlayerCities > 1 && iYieldRate > iYieldRateAverage)
                        {
                            iSpecializationModifier = (AI_TRAINING_CITY_SPECIALIZATION_MODIFIER * iYieldRate) / iYieldRateAverage;

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
                            iSpecializationModifier += Math.Max(0, 25 - iRarestUnitResourcePer100CitySites);  //extra value if resource is rarer than 1 per 4 city sites
                            infos.utils().modify(iSpecializationModifier, Math.Max(0, 50 - iRarestUnitResourcePer100CitySites));

                            infos.utils().modify(iSpecializationModifier, -(100 / iNumPlayerCities));
                            infos.utils().modify(iValue, iSpecializationModifier);
                        }

                    }

                }

                //return infos.utils().modify(iValue, (iValue > 0) ? iModifier : -(iModifier));
                iValue = infos.utils().modify(iValue, (iValue > 0) ? iModifier : -(iModifier));

                iValueFlat = iValue;

                return infos.utils().modify(iValue, pCity.calculateTotalYieldModifier(eYield));
            }
            /*####### Better Old World AI - Base DLL #######
              ### AI: City Yield Values              END ###
              ##############################################*/

            //fix from Test, modified for this mod
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
                                //iLeaderValue += infos.utils().modify(iYieldAmount, pCity.calculateTotalYieldModifierForGovernor(eLoopYield, pCharacter)) * cityYieldValue(eLoopYield, pCity);
                                iLeaderValue += infos.utils().modify(iYieldAmount, pCity.calculateTotalYieldModifierForGovernor(eLoopYield, pCharacter)) * cityYieldValueFlat(eLoopYield, pCity);
                            }
                        }
                        iValue += iLeaderValue * getTurnsLeftEstimate(pCharacter, false) / Constants.YIELDS_MULTIPLIER;
                    }

                    iValue += getCharacterXPValue(pCharacter, pCity.culture().miXP * getTurnsLeftEstimate(pCharacter, false));

                    for (RatingType eRating = 0; eRating < infos.ratingsNum(); ++eRating)
                    {
                        iValue += ratingValue(eRating, pCharacter.getRating(eRating), pCharacter, false, false, false, eCouncil: CouncilType.NONE, iCityGovernor: pCity.getID(), eUnitGeneral: UnitType.NONE);
                    }

                    foreach (TraitType eLoopTrait in pCharacter.getTraits())
                    {
                        iValue += traitValue(eLoopTrait, pCharacter, true, false, false, false, eCouncil: CouncilType.NONE, iCityGovernor: pCity.getID(), eUnitGeneral: UnitType.NONE);
                    }

                    iValue = infos.utils().modify(iValue, getJobValueModifierForTutor(pCharacter), true);

                    if (pCharacter.hasFamily())
                    {
                        int iFamilyOpinion = game.familyClass(pCharacter.getFamily()).miGovernorOpinion;
                        if (pCharacter.isFamilyHead())
                        {
                            iFamilyOpinion += infos.Globals.GOVERNOR_OPINION;
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
                        if (pCity.hasGovernor())
                        {
                            iValue = characterSwapValuePerTurn(pCity.governor(), pCharacter, governorValue(pCity.governor(), pCity, false, false), iValue, false) * getOverlapTurns(pCity.governor(), pCharacter, false);
                        }
                    }


                    if (bIncludeCost)
                    {
                        iValue -= infos.Globals.GOVERNOR_CHARACTER_COST * yieldValue(infos.Globals.ORDERS_YIELD);

                        for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); eLoopYield++)
                        {
                            iValue -= infos.yield(eLoopYield).miGovernorCost * yieldValue(eLoopYield);
                        }
                    }
                }

                return iValue;
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
                    if (game.isGameOption(infos.Globals.GAMEOPTION_PLAY_TO_WIN))
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

            //lines 6975-6993
            //now obsolete, removed part was also removed in base game
            //protected override bool shouldLeaveTileForOtherPlayer(Tile pTile)

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
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much       END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much     START ###
  ##############################################*/
                    //this part can stay, to make sure we are only checking not-yet-used starting tiles
                    if (pTile.getCitySite() == CitySiteType.ACTIVE_RESERVED)
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't hold back too much       END ###
  ##############################################*/
                    {
                        // don't claim reserved sites if we already started with extra cities
                        if (!player.isHuman() && game.hasDevelopmentCities())
                        {
                            // at least for a grace period
                            if (game.getTurn() < ((game.isGameOption(infos.Globals.GAMEOPTION_PLAY_TO_WIN)) ? AI_PLAY_TO_WIN_GRACE_TURNS : AI_GRACE_TURNS))
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
                            getCityWaterUnitAreas(eUnit, game.city(iCityID), siCityAreas);
                        }
                    }
                    else if (tribe != null)
                    {
                        foreach (int iCityID in tribe.getCities())
                        {
                            getCityWaterUnitAreas(eUnit, game.city(iCityID), siCityAreas);
                        }
                    }
                }
                else
                {
                    int iBestCityWaterArea = -1;
                    foreach (int iTileId in pCity.getTerritoryTiles())
                    {
                        Tile pTile = game.tile(iTileId);
                        if (pTile.isSaltWater())
                        {
                            if (iBestCityWaterArea == -1 || game.getWaterAreaCount(pTile.getArea()) > game.getWaterAreaCount(iBestCityWaterArea))
                            {
                                iBestCityWaterArea = pTile.getArea();
                            }
                        }
                    }

                    //cities put ships only in 1 area
                    if (iBestCityWaterArea == -1) return;

                    Tile pUnitTile = game.findUnitTileNearby(eUnit, pCity.tile(), pCity.getPlayer(), pCity.getTribe(), TeamType.NONE, bTestTile: true, bSpecialTile: true, iRequiresArea: -1, pRequiresCity: pCity, null);
                    if (pUnitTile == null)
                    {
                        pUnitTile = game.findUnitTileNearby(eUnit, pCity.tile(), pCity.getPlayer(), pCity.getTribe(), TeamType.NONE, bTestTile: true, bSpecialTile: false, iRequiresArea: iBestCityWaterArea, pRequiresCity: pCity, null);
                    }
                    if (pUnitTile != null)
                    {
                        if (game.getWaterAreaCount(pUnitTile.getArea()) > 2 * infos.Globals.FRESH_WATER_THRESHOLD)
                        {
                            siCityAreas.Add(pUnitTile.getArea());
                        }
                    }
                }
            }

            //lines 8702-8799
            protected override void getWaterUnitTargetNumber(UnitType eUnit, City pCity, out int iTargetNumber, out int iCurrentNumber)
            {
                using var profileScope = new UnityProfileScope("PlayerAI.getWaterUnitTargetNumber");

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

/*####### Better Old World AI - Base DLL #######
  ### AI: Less ships                   START ###
  ##############################################*/

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
/*####### Better Old World AI - Base DLL #######
  ### AI: Less ships                     END ###
  ##############################################*/
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
/*####### Better Old World AI - Base DLL #######
  ### AI: Less ships                   START ###
  ##############################################*/
                        //iTargetWarships += siAreaCities.Count * 2;
                        iTargetWarships += (siAreaCities.Count * 2) - 1;
/*####### Better Old World AI - Base DLL #######
  ### AI: Less ships                     END ###
  ##############################################*/
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
/*####### Better Old World AI - Base DLL #######
  ### AI: Less ships                   START ###
  ##############################################*/
                        //iTargetWaterControl /= pUnitInfo.miWaterControl;
                        //iTargetWaterControl = (iTargetWaterControl + 2 * pUnitInfo.miWaterControl) / (2 * pUnitInfo.miWaterControl + 1);
/*####### Better Old World AI - Base DLL #######
  ### AI: Less ships                     END ###
  ##############################################*/
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
  ### AI: City Yield Values            START ###
  ##############################################*/
            //lines 10946-11052
            protected override long ratingValue(RatingType eRating, int iRating, Character pCharacter, bool? bLeader = null, bool? bLeaderSpouse = null, bool? bSuccessor = null, CourtierType? eCourtier = null, CouncilType? eCouncil = null, int? iCityGovernor = null, UnitType? eUnitGeneral = null)
            {
                long iValue = base.ratingValue(eRating, iRating, pCharacter, bLeader, bLeaderSpouse, bSuccessor, eCourtier, eCouncil, iCityGovernor, eUnitGeneral);

                int iTestGovernorCity = pCharacter.getCityGovernorID();
                if (iCityGovernor.HasValue)
                {
                    iTestGovernorCity = iCityGovernor.Value;
                }

                if (iTestGovernorCity != -1)
                {
                    City pCity = game.city(iTestGovernorCity);
                    if (pCity != null)
                    {
                        for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); eLoopYield++)
                        {
                            int iModifier = infos.Helpers.getRatingYieldModifierGovernor(eRating, eLoopYield, iRating, game);

                            int iBaseValue = cityYield(eLoopYield, pCity);
                            int iExtraYield = infos.Helpers.modifyYield(eLoopYield, iBaseValue, iModifier) - iBaseValue;

                            //iValue += iExtraYield * cityYieldValue(eLoopYield, pCity) / Constants.YIELDS_MULTIPLIER;
                            iValue -= iExtraYield * cityYieldValue(eLoopYield, pCity) / Constants.YIELDS_MULTIPLIER;
                            //iExtraYield is not base yield, value needs to NOT get modified by city yield rate modifiers
                            iValue += iExtraYield * cityYieldValueFlat(eLoopYield, pCity) / Constants.YIELDS_MULTIPLIER;
                        }
                    }
                }

                return iValue;
            }
/*####### Better Old World AI - Base DLL #######
  ### AI: City Yield Values              END ###
  ##############################################*/

            protected override long cityRepairValue(City pCity, int iRepairHP)
            {
                using var profileScope = new UnityProfileScope("PlayerAI.cityRepairValue");

                if (player == null)
                {
                    return 0;
                }

                long iValue = 0;
                int iChangeHP = iRepairHP;
                if (pCity != null)
                {
                    if (iChangeHP > 0)
                    {
                        iChangeHP = Math.Max(0, Math.Min(iChangeHP, pCity.getDamage()) - pCity.getPassiveHealDamage());
                    }
                    else
                    {
                        iChangeHP = Math.Max(iChangeHP, -pCity.getHP());
                    }
                }

                iValue += adjustForInflation(iChangeHP * AI_CITY_HP_VALUE);

                if (iChangeHP != 0)
                {
                    for (YieldType eYield = 0; eYield < infos.yieldsNum(); ++eYield)
                    {
                        int iBaseValue = cityYield(eYield, pCity);
                        int iExtraValue = infos.Helpers.modifyYield(eYield, iBaseValue, pCity.getDamageYieldModifier(eYield, iChangeHP)) - iBaseValue;
/*####### Better Old World AI - Base DLL #######
  ### AI: City Yield Values            START ###
  ##############################################*/
                        //iValue -= iExtraValue * cityYieldValue(eYield, pCity) * AI_YIELD_TURNS / Constants.YIELDS_MULTIPLIER;
                        iValue -= iExtraValue * cityYieldValueFlat(eYield, pCity) * AI_YIELD_TURNS / Constants.YIELDS_MULTIPLIER;
/*####### Better Old World AI - Base DLL #######
  ### AI: City Yield Values              END ###
  ##############################################*/
                    }

                    if (pCity.hasFamily())
                    {
                        if (pCity.hasFamily() && game.familyClass(pCity.getFamily()).miCityDamagedOpinion != 0)
                        {
                            iValue -= getFamilyOpinionValue(pCity.getFamily(), game.familyClass(pCity.getFamily()).miCityDamagedOpinion) * iChangeHP / pCity.getHPMax();
                        }
                    }
                }

                if (pCity != null && getCityDanger(pCity.getID()) > AI_CITY_MIN_DANGER)
                {
                    iValue *= 10;
                }

                return iValue;
            }



            //lines 11463-11855
            protected override long calculateEffectCityValue(EffectCityType eEffectCity, City pCity, bool bRemove)
            {
                using var profileScope = new UnityProfileScope("PlayerAI.calculateEffectCityValue");

                if (player == null)
                {
                    return 0;
                }

                int iExtraCount = bRemove ? -1 : 1;

                long calculateLuxuryValue(ResourceType eResource)
                {
                    long iValue = 0;

                    iValue += effectCityValue(infos.Globals.LUXURY_EFFECTCITY, pCity, bRemove, bWarningsDisabled: true);

                    if (pCity.hasFamily())
                    {
                        EffectCityType eLuxuryEffectCity = pCity.familyClass().maeLuxuryEffectCity[eResource];

                        if (eLuxuryEffectCity != EffectCityType.NONE)
                        {
                            iValue += effectCityValue(eLuxuryEffectCity, pCity, bRemove, bWarningsDisabled: true);
                        }

                        iValue += getFamilyOpinionValue(pCity.getFamily(), game.familyClass(pCity.getFamily()).miLuxuryOpinion);
                    }

                    foreach (GoalData pGoalData in ((BetterAIPlayer)player).getGoalDataList())
                    {
                        if (!(pGoalData.mbFinished))
                        {
                            if ((infos.goal(pGoalData.meType).miLuxuries > 0) ||
                                (infos.goal(pGoalData.meType).miPlayerLuxuries > 0) ||
                                (infos.goal(pGoalData.meType).miTribeLuxuries > 0) ||
                                (infos.goal(pGoalData.meType).miFamilyLuxuries > 0))
                            {
                                iValue += bonusValue(infos.Globals.FINISHED_AMBITION_BONUS);
                            }
                        }
                    }

                    return iValue;
                }

                InfoEffectCity effectCity = infos.effectCity(eEffectCity);

                long iValue = 0;

                {
                    long iDefenseValue = 0;
                    if (effectCity.miCityHP != 0)
                    {
                        iDefenseValue += effectCity.miCityHP * AI_CITY_HP_VALUE;
                    }

                    iDefenseValue += (effectCity.miStrengthModifier * AI_UNIT_STRENGTH_MODIFIER_VALUE / 10);

                    if (isBorderCity(pCity))
                    {
                        iDefenseValue *= 5;
                    }

                    iValue += iDefenseValue;
                }

                int iUnitsBuiltEstimate = pCity.calculateModifiedYield(infos.Globals.TRAINING_YIELD) * AI_CITY_UNITS_BUILT_PER_TRAINING / Constants.YIELDS_MULTIPLIER;

                iValue += (effectCity.miUnitXP * AI_UNIT_XP_VALUE * iUnitsBuiltEstimate);
                iValue += (effectCity.miUnitLevel * AI_UNIT_LEVEL_VALUE * iUnitsBuiltEstimate);
                iValue += (effectCity.miUnitHeal * AI_UNIT_HEAL_VALUE * iUnitsBuiltEstimate);
                iValue += (effectCity.miRangeChange * AI_UNIT_RANGE_VALUE);
                iValue += (effectCity.miRandomPromotions * AI_UNIT_RANDOM_PROMOTION_VALUE * iUnitsBuiltEstimate);

                iValue += (effectCity.miHurryDiscontentModifier * AI_CITY_HURRY_DISCONTENT_VALUE);
                iValue += (pCity.lastPlayer().getEffectCityRebelProb(eEffectCity) * AI_CITY_REBEL_VALUE);
                iValue -= (effectCity.miImprovementCostModifier * AI_CITY_IMPROVEMENT_COST_VALUE);
                iValue -= (effectCity.miAdjacentClassCostModifier * AI_CITY_ADJACENT_CLASS_COST_VALUE);
                iValue -= (effectCity.miSpecialistCostModifier * AI_CITY_SPECIALIST_COST_VALUE);
                iValue -= (effectCity.miSpecialistRuralTrainTimeModifier * AI_CITY_SPECIALIST_TRAIN_VALUE / 2);
                iValue -= (effectCity.miSpecialistUrbanCostModifier * AI_CITY_SPECIALIST_COST_VALUE / 2);
                iValue -= (effectCity.miSpecialistUrbanTrainTimeModifier * AI_CITY_SPECIALIST_TRAIN_VALUE / 2);
                iValue -= (effectCity.miProjectCostModifier * AI_CITY_PROJECT_COST_VALUE);
                iValue -= (effectCity.miBuildTurnChange * AI_WORKER_BUILD_VALUE);
                iValue -= (effectCity.miUrbanBuildTurnChange * AI_WORKER_BUILD_VALUE / 2);

                if (effectCity.mbAutoBuild || effectCity.mbAutoBuildUnits)
                {
                    if (pCity.isAutoBuild() != pCity.isAutoBuild(iExtraCount))
                    {
                        iValue += AI_CITY_AUTOBUILD_VALUE; // XXX
                    }
                }

                if (effectCity.mbNoHurry)
                {
                    if (pCity.isNoHurry() != pCity.isNoHurry(iExtraCount))
                    {
                        iValue -= AI_CITY_HURRY_VALUE; // XXX
                    }
                }

                if (effectCity.mbHurryCivics)
                {
                    if (pCity.isHurryCivics() != pCity.isHurryCivics(iExtraCount))
                    {
                        iValue += AI_CITY_HURRY_VALUE / 2; // XXX
                    }
                }

                if (effectCity.mbHurryTraining)
                {
                    if (pCity.isHurryTraining() != pCity.isHurryTraining(iExtraCount))
                    {
                        iValue += AI_CITY_HURRY_VALUE / 2; // XXX
                    }
                }

                if (effectCity.mbHurryMoney)
                {
                    if (pCity.isHurryMoney() != pCity.isHurryMoney(iExtraCount))
                    {
                        iValue += AI_CITY_HURRY_VALUE / 2; // XXX
                    }
                }

                if (effectCity.mbHurryPopulation)
                {
                    if (pCity.isHurryPopulation() != pCity.isHurryPopulation(iExtraCount))
                    {
                        iValue += AI_CITY_HURRY_VALUE / 2; // XXX
                    }
                }

                if (effectCity.mbHurryOrders)
                {
                    if (pCity.isHurryOrders() != pCity.isHurryOrders(iExtraCount))
                    {
                        iValue += AI_CITY_HURRY_VALUE / 2; // XXX
                    }
                }

                if (effectCity.mbEnablesGovernor && game.isCharacters())
                {
                    if (pCity.isEnablesGovernor() != pCity.isEnablesGovernor(iExtraCount))
                    {
                        iValue += AI_CITY_GOVERNOR_VALUE;
                    }
                }

                if (effectCity.mbNoReligionSpread)
                {
                    if (pCity.isNoRandomReligionSpreadUnlock() != pCity.isNoRandomReligionSpreadUnlock(iExtraCount))
                    {
                        iValue += AI_RELIGION_VALUE; // XXX
                    }
                }

                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos.unitTraitsNum(); eLoopUnitTrait++)
                {
                    iValue += (effectCity.maiUnitTraitXP[eLoopUnitTrait] * AI_UNIT_XP_VALUE / AI_UNIT_TRAITS);
                    iValue += (effectCity.maiUnitTraitLevel[eLoopUnitTrait] * AI_UNIT_LEVEL_VALUE / AI_UNIT_TRAITS);
                    iValue += -(effectCity.maiUnitTraitCostModifier[eLoopUnitTrait] * AI_UNIT_COST_VALUE / AI_UNIT_TRAITS);
                    iValue += -(effectCity.maiUnitTraitTrainModifier[eLoopUnitTrait] * AI_UNIT_STRENGTH_MODIFIER_VALUE / AI_UNIT_TRAITS);
                }

                foreach (YieldType eLoopYield in infos.effectCity(eEffectCity).maeBuyTile)
                {
                    if (pCity.isBuyTileUnlock(eLoopYield) != pCity.isBuyTileUnlock(eLoopYield, iExtraCount))
                    {
                        iValue += 40 * AI_TILE_VALUE; // XXX
                    }
                }

                int iHurryValue = 0;

                foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryPopulation)
                {
                    if (!pCity.isHurryPopulation() && (pCity.isHurryPopulation(eLoopBuild) != pCity.isHurryPopulation(eLoopBuild, iExtraCount)))
                    {
                        iHurryValue += AI_CITY_HURRY_VALUE / 2;
                    }
                }

                foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryCivics)
                {
                    if (!pCity.isHurryCivics() && (pCity.isHurryCivics(eLoopBuild) != pCity.isHurryCivics(eLoopBuild, iExtraCount)))
                    {
                        iHurryValue += AI_CITY_HURRY_VALUE / 2;
                    }
                }

                foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryTraining)
                {
                    if (!pCity.isHurryTraining() && (pCity.isHurryTraining(eLoopBuild) != pCity.isHurryTraining(eLoopBuild, iExtraCount)))
                    {
                        iHurryValue += AI_CITY_HURRY_VALUE / 2;
                    }
                }

                foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryMoney)
                {
                    if (!pCity.isHurryMoney() && (pCity.isHurryMoney(eLoopBuild) != pCity.isHurryMoney(eLoopBuild, iExtraCount)))
                    {
                        iHurryValue += AI_CITY_HURRY_VALUE / 2;
                    }
                }

                foreach (BuildType eLoopBuild in infos.effectCity(eEffectCity).maeHurryOrders)
                {
                    if (!pCity.isHurryOrders() && (pCity.isHurryOrders(eLoopBuild) != pCity.isHurryOrders(eLoopBuild, iExtraCount)))
                    {
                        iHurryValue += AI_CITY_HURRY_VALUE / 2;
                    }
                }
                iValue += iHurryValue / (int)infos.buildsNum();

                iValue = adjustForInflation(iValue);

                // evertything below needs to be already adjusted for inflation

                foreach (EffectCityType eLoopEffectCity in effectCity.maeFreeUnitEffectCity)
                {
                    if (pCity.isFreeUnitEffectCityUnlock(eLoopEffectCity) != pCity.isFreeUnitEffectCityUnlock(eLoopEffectCity, iExtraCount))
                    {
                        iValue += effectCityValue(eLoopEffectCity, pCity, bRemove);
                    }
                }

                if (pCity.hasFamily())
                {
                    iValue += getFamilyOpinionValue(pCity.getFamily(), effectCity.miFamilyOpinion);
                }

                if (effectCity.mbAlwaysConnected)
                {
                    if (pCity.isAlwaysConnectedUnlock() != pCity.isAlwaysConnectedUnlock(iExtraCount))
                    {
                        iValue += getRoadValue(pCity, false, !pCity.tile().onTradeNetworkCapital(getPlayer()), bRemove);
                    }
                }

                if (effectCity.mbLuxury)
                {
                    iValue += calculateLuxuryValue(effectCity.meSourceResource);
                }

                foreach (PromotionType eLoopPromotion in effectCity.maeFreePromotion)
                {
                    if (pCity.isFreePromotion(eLoopPromotion) != pCity.isFreePromotion(eLoopPromotion, iExtraCount))
                    {
                        iValue += promotionValue(eLoopPromotion, UnitType.NONE) * iUnitsBuiltEstimate;
                    }
                }

                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos.unitTraitsNum(); eLoopUnitTrait++)
                {
                    PromotionType ePromotion = effectCity.maeTraitPromotion[eLoopUnitTrait];
                    if (ePromotion != PromotionType.NONE)
                    {
                        if (!pCity.isFreePromotion(ePromotion) && (pCity.isFreeTraitPromotion(eLoopUnitTrait, ePromotion) != pCity.isFreeTraitPromotion(eLoopUnitTrait, ePromotion, iExtraCount)))
                        {
                            iValue += promotionValue(ePromotion, UnitType.NONE) * iUnitsBuiltEstimate / AI_UNIT_TRAITS;
                        }
                    }
                }

                {
                    EffectCityType eEffectCityUnlock = effectCity.meEffectCityUnlock;

                    if (eEffectCityUnlock != EffectCityType.NONE)
                    {
                        iValue += effectCityValue(eEffectCityUnlock, pCity, bRemove, bWarningsDisabled: true);
                    }
                }

                {
                    EffectCityType eSourceUnlock = effectCity.meSourceUnlock;

                    if (eSourceUnlock != EffectCityType.NONE)
                    {
                        iValue += effectCityValue(eSourceUnlock, pCity, bRemove, bWarningsDisabled: true);
                    }
                }

                {
                    BonusType eCultureBonus = effectCity.meCultureBonus;

                    if (eCultureBonus != BonusType.NONE)
                    {
                        BonusParameters zParameters = new BonusParameters(null);
                        zParameters.pTargetCity = pCity;
                        iValue += bonusValue(eCultureBonus, ref zParameters) * Math.Max(1, (int)infos.culturesNum() - pCity.getCultureStep());
                    }
                }

                {
                    ResourceType eLuxuryResource = effectCity.meLuxuryResource;

                    if (eLuxuryResource != ResourceType.NONE)
                    {
                        iValue += calculateLuxuryValue(eLuxuryResource);
                    }
                }

                for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); eLoopYield++)
                {
                    long iSubValue = pCity.getEffectCityYieldRate(eEffectCity, eLoopYield, pCity.governor());

                    foreach (int iTileID in pCity.getTerritoryTiles())
                    {
                        Tile pLoopTile = game.tile(iTileID);
                        if (pLoopTile.hasImprovement())
                        {
                            iSubValue += getEffectCityTileYieldRate(eEffectCity, game.tile(iTileID), pLoopTile.getImprovement(), pLoopTile.getSpecialist(), eLoopYield, pCity);
                        }
                    }
                    for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < infos.effectCitiesNum(); ++eLoopEffectCity)
                    {
                        if (eLoopEffectCity != eEffectCity)
                        {
                            int iEffectCityYield = infos.effectCity(eLoopEffectCity).maaiEffectCityYieldRate[eEffectCity, eLoopYield];
                            if (iEffectCityYield != 0)
                            {
                                int iCount = pCity.getEffectCityCount(eLoopEffectCity);
/*####### Better Old World AI - Base DLL #######
  ### self-aaiEffectCityYieldRate      START ###
  ##############################################*/
                                if (eEffectCity == eLoopEffectCity)
                                {
                                    iCount += iExtraCount;
                                }
/*####### Better Old World AI - Base DLL #######
  ### self-aaiEffectCityYieldRate        END ###
  ##############################################*/
                                if (iCount != 0)
                                {
                                    iSubValue += iEffectCityYield * iCount;
                                }
                            }
                        }
                    }

                    if (isBorderCity(pCity))
                    {
                        iSubValue += effectCity.maiYieldRateDefending[eLoopYield]; // assume it's going to be defended if it's a border city
                    }

                    int iModifier = effectCity.maiYieldModifier[eLoopYield];

/*####### Better Old World AI - Base DLL #######
  ### AI: City Yield Values            START ###
  ##############################################*/
                    long iSubValueMod = 0;
                    if (iModifier != 0)
                    {
                        //int iBaseYield = cityYield(eLoopYield, pCity);
                        int iBaseYield = cityYield(eLoopYield, pCity) + (int)iSubValue;
                        iSubValueMod += infos.Helpers.modifyYield(eLoopYield, iBaseYield, iModifier) - iBaseYield;
                    }

                    if (iSubValue != 0)
                    {
                        // culture already gets a +100% bonus in calculateCityYieldValue if city yield is 0
                        //if (eLoopYield == infos.Globals.CULTURE_YIELD)
                        //{
                        //    if (cityYield(eLoopYield, pCity) == 0)
                        //    {
                        //        iSubValue *= 2;
                        //    }
                        //}

                        iValue += ((iSubValue * cityYieldValue(eLoopYield, pCity) * AI_YIELD_TURNS) / Constants.YIELDS_MULTIPLIER);
                    }
                    if (iSubValueMod != 0)
                    {
                        iValue += ((iSubValueMod * cityYieldValueFlat(eLoopYield, pCity) * AI_YIELD_TURNS) / Constants.YIELDS_MULTIPLIER);
                    }
/*####### Better Old World AI - Base DLL #######
  ### AI: City Yield Values              END ###
  ##############################################*/
                }

                for (UnitType eLoopUnit = 0; eLoopUnit < infos.unitsNum(); eLoopUnit++)
                {
                    iValue -= (effectCity.maiUnitCostModifier[eLoopUnit] * AI_UNIT_COST_VALUE / 10);
                    iValue -= (effectCity.maiUnitTrainModifier[eLoopUnit] * AI_UNIT_COST_VALUE / 10);

                    if (pCity.getEffectCityCount(eEffectCity) == 0)
                    {
                        if (infos.unit(eLoopUnit).meEffectCityPrereq == eEffectCity)
                        {
                            if (player.canBuildUnit(eLoopUnit))
                            {
                                iValue += Math.Max(0, unitValue(eLoopUnit, pCity, true, false));

                                long iBestExisting = 0;
                                for (UnitType eLoopExisting = 0; eLoopExisting < infos.unitsNum(); eLoopExisting++)
                                {
                                    if (infos.unit(eLoopExisting).maeUpgradeUnit.Contains(eLoopUnit))
                                    {
                                        iBestExisting = Math.Max(unitValue(eLoopExisting, pCity, true, false), iBestExisting);
                                    }
                                }
                                iValue -= iBestExisting;
                            }
                        }
                    }
                }

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

                if (pCity.getEffectCityCount(eEffectCity) == (bRemove ? 1 : 0))
                {
                    for (ProjectType eLoopProject = 0; eLoopProject < infos.projectsNum(); ++eLoopProject)
                    {
                        if (!pCity.hasProject(eLoopProject))
                        {
                            InfoProject project = infos.project(eLoopProject);
                            if (project.meEffectCityPrereq == eEffectCity)
                            {
                                if (!project.mbHidden)
                                {
                                    if (project.meTechPrereq != TechType.NONE && player.isTechAcquired(project.meTechPrereq))
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
                    if (effectCity.maiImprovementClassUpgradeTurnChange[eLoopImprovementClass] != 0)
                    {
                        // XXX
                    }
                }

                for (ReligionType eLoopReligion = 0; eLoopReligion < infos.religionsNum(); eLoopReligion++)
                {
                    int iSubValue = effectCity.maiReligionOpinion[eLoopReligion];
                    if (iSubValue != 0)
                    {
                        iValue += getReligionOpinionValue(eLoopReligion, iSubValue, AI_YIELD_TURNS);
                    }
                }

                for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos.improvementClassesNum(); eLoopImprovementClass++)
                {
                    if (effectCity.mabNoImprovementClassMax[(int)eLoopImprovementClass])
                    {
                        // XXX
                    }
                }

                for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos.terrainsNum(); ++eLoopTerrain)
                {
                    if (effectCity.mabUrbanTerrainValid[(int)eLoopTerrain])
                    {
                        iValue += pCity.getTerritoryTileCount(eLoopTerrain) * AI_BUILD_URBAN_VALUE;
                    }
                }

                return iValue;
            }

            //lines 12981-13010
            protected override long getHappinessLevelValue(int iChange, City pCity)
            {
                using var profileScope = new UnityProfileScope("PlayerAI.getHappinessLevelValue");

                if (player == null)
                {
                    return 0;
                }

                int iCurrentHappiness = pCity.getHappinessLevel();
                int iNewHappiness = iCurrentHappiness + iChange;
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                START ###
  ##############################################*/
                if (((BetterAIInfoGlobals)infos.Globals).BAI_DISCONTENT_LEVEL_ZERO == 0) //no happiness level 0
                {
                    if (iCurrentHappiness < 0 != iNewHappiness < 0)
                    {
                        if (iChange > 0)
                        {
                            iNewHappiness++;
                        }
                        else
                        {
                            iNewHappiness--;
                        }
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                  END ###
  ##############################################*/

                long iValue = 0;
                for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                {
                    int iModifier = infos.Helpers.getHappinessLevelYieldModifier(iNewHappiness, eLoopYield) - infos.Helpers.getHappinessLevelYieldModifier(iCurrentHappiness, eLoopYield);
                    if (iModifier != 0)
                    {
                        int iYield = cityModifiedYield(eLoopYield, pCity, iModifier) - cityModifiedYield(eLoopYield, pCity, 0);
                        if (iYield != 0)
                        {
/*####### Better Old World AI - Base DLL #######
  ### AI fix                           START ###
  ##############################################*/
                            //iValue += iYield * cityYieldValue(eLoopYield, pCity) / Constants.YIELDS_MULTIPLIER;
                            iValue += iYield * cityYieldValueFlat(eLoopYield, pCity) * AI_YIELD_TURNS / Constants.YIELDS_MULTIPLIER;
/*####### Better Old World AI - Base DLL #######
  ### AI fix                             END ###
  ##############################################*/
                        }
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                START ###
  ##############################################*/
                //if (pCity.hasFamily())
                if (pCity.hasFamily() && (iCurrentHappiness < 0 || iNewHappiness < 0))
                {
                    //iValue -= getFamilyOpinionValue(pCity.getFamily(), infos.Globals.DISLOYALTY_OPINION * iChange, AI_YIELD_TURNS);
                    iValue -= getFamilyOpinionValue(pCity.getFamily(), infos.Globals.DISLOYALTY_OPINION * (Math.Min(0, iNewHappiness) - Math.Min(0, iCurrentHappiness)), AI_YIELD_TURNS);
                }
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                END ###
  ##############################################*/

                return iValue;
            }


            //lines 11966-13545
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

/*####### Better Old World AI - Base DLL #######
  ### AI: City Yield Values            START ###
  ##############################################*/
                long iCityValue = 0;
                if (zParameters.pTargetCity != null)
                {
                    City pCity = zParameters.pTargetCity;
                    Player pCityPlayer = pCity.player();

                    for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); eLoopYield++)
                    {
                        long iSubValue = infos.bonus(eBonus).maiCityYields[eLoopYield];
                        iSubValue += infos.bonus(eBonus).maaiCultureYield[pCity.getCulture(), eLoopYield];
                        if (iSubValue != 0)
                        {
                            //iCityValue += (iSubValue * pCityPlayer.AI.cityYieldValue(eLoopYield, pCity));
                            iCityValue -= (iSubValue * pCityPlayer.AI.cityYieldValue(eLoopYield, pCity));
                            iCityValue += (iSubValue * ((BetterAIPlayer.BetterAIPlayerAI)pCityPlayer.AI).cityYieldValueFlat(eLoopYield, pCity));
                        }
                    }
                    if (iCityValue != 0)
                    {
                        if (pCityPlayer == null || pCityPlayer.getTeam() != Team)
                        {
                            iCityValue /= 2;
                        }
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### AI: City Yield Values              END ###
  ##############################################*/

                return iPlayerValue + iCityValue + base.bonusValue(eBonus, ref zParameters);
            }


            //lines 14763-14815
            protected override int getNeedSettlers(City pCity)
            {
                using var profileScope = new UnityProfileScope("PlayerAI.getNeedSettlers");

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

            //lines 16955-17023
            //for debug only
            public override long improvementValueTile(ImprovementType eImprovement, Tile pTile, City pCity, bool bIncludeCost, bool bSubtractCurrent)
            {
                using (new UnityProfileScope("PlayerAI.improvementValueTile"))
                {
                    if (player == null)
                    {
                        return 0;
                    }

                    bool bFound = false;
                    long iValue = 0;
                    lock (gameCacheLock)
                    {
                        bFound = mpAICache.getImprovementTotalValue(pTile.getID(), pCity?.getID() ?? -1, eImprovement, out iValue);
                    }


                    if (!bFound)
                    {
                        City pImprovementCity = pCity ?? pTile.cityTerritory();
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
                        if (pImprovementCity == null)
                        {
                            pImprovementCity = player.findClosestCity(pTile);
                        }
                        if (pImprovementCity == null)
                        {
                            MohawkAssert.Assert(false, "Improvement without cities");
                            return 0;
                        }


                        iValue = calculateImprovementValueForTile(pTile, pImprovementCity, eImprovement, true);
                        lock (gameCacheLock)
                        {
                            mpAICache.setImprovementTotalValue(pTile.getID(), pImprovementCity.getID(), eImprovement, iValue);
                            Debug.Log("Unexpected improvement value calculation" + infos.improvement(eImprovement).mzType + (pCity != null ? " in " + pCity.getName() : "") + " on Tile ("+ pTile.getX().ToString() + "," + pTile.getY().ToString() + "). Terrain: " + pTile.terrain().mzType +
                                    " / Height: " + pTile.height().mzType + (pTile.getVegetation() != VegetationType.NONE ? " / Vegetation: " + pTile.vegetation().mzType : " / no vegetation") + (pTile.getResource() != ResourceType.NONE ? " / Resource: " + pTile.resource().mzType : " / no Resource" + Environment.StackTrace.ToString()));

                            //MohawkAssert.Assert(AreCacheWarningsMuted, "Unexpected improvement value calculation");
                        }

                    }

                    if (bIncludeCost)
                    {
                        for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                        {
                            iValue -= infos.Helpers.getBuildCost(eImprovement, eLoopYield, pTile) * yieldValue(eLoopYield);
                        }
                    }

                    if (pTile.hasImprovement())
                    {
                        if (pTile.getImprovement() == eImprovement)
                        {
                            iValue += adjustForInflation(AI_EXISTING_IMPROVEMENT_VALUE); // encourage repair and discourage replacing improvements
                        }
                        else if (bSubtractCurrent)
                        {
                            if (mpAICache.getImprovementTotalValue(pTile.getID(), pCity?.getID() ?? -1, pTile.getImprovement(), out long iExistingValue))
                            {
                                iValue -= Math.Max(0, iExistingValue);
                            }

                            if (pTile.hasSpecialist() && pCity != null)
                            {
                                iValue -= Math.Max(0, specialistValue(pTile.getSpecialist(), pCity, pTile, false, true));
                            }
                        }
                    }

                    return iValue;
                }
            }


            //copy-paste START
            //lines 17008-17183
            // Chance that we will declare war on them
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

                if (player.countPlayerMemories(infos.Globals.PLAYER_WAR_MEMORY) > 0)
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

                ProximityType eProximity = pOtherPlayer.calculateProximityPlayer(eThisPlayer);
                PowerType eStrength = pOtherPlayer.calculatePowerOf(eThisPlayer);

                bool bPlayToWin = game.isPlayToWinVs(eOtherPlayer);

                if (!bPlayToWin)
                {
                    if (game.isPlayToWinAny())
                    {
                        return 0;
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
                        if (bPreparedOnly)
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
                        iPercent = infos.utils().modify(iPercent, 50);
                    }
                    else if (pOtherPlayer.isCloseToWinning(10))
                    {
                        iPercent = infos.utils().modify(iPercent, 40);
                    }
                    else if (pOtherPlayer.isCloseToWinning(15))
                    {
                        iPercent = infos.utils().modify(iPercent, 30);
                    }
                    else if (pOtherPlayer.isCloseToWinning(20))
                    {
                        iPercent = infos.utils().modify(iPercent, 20);
                    }
                    else
                    {
                        iPercent = infos.utils().modify(iPercent, 10);
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

        }
    }

}
