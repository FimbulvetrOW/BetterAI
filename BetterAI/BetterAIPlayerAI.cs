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
using System.IO.Ports;

namespace BetterAI
{
    public partial class BetterAIPlayer : Player
    {
        public partial class BetterAIPlayerAI : BetterAIPlayer.PlayerAI
        {
            protected int AI_GROWTH_CITY_SPECIALIZATION_MODIFIER = 0;
            protected int AI_CIVICS_CITY_SPECIALIZATION_MODIFIER = 100;
            protected int AI_TRAINING_CITY_SPECIALIZATION_MODIFIER = 100;
            protected int AI_FAMILY_OPINION_VALUE_PER = 0;


            //[SkipCheckSaveConsistency] protected BetterAIPlayerCache BAI_mpAICache = new BetterAIPlayerCache();

            public override void init(Game pGame, Player pPlayer, Tribe pTribe)
            {
                if (pGame != null)
                {
                    if (pGame.infos() != null)
                    {
                        //Debug.Log("PlayerAI init");
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
                        //Debug.Log("PlayerAI initClient");
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
                AI_FAMILY_OPINION_VALUE_PER = pInfos.getGlobalInt("AI_FAMILY_OPINION_VALUE_PER");
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
                AI_WATER_UNIT_EXTRA_VALUE = pInfos.getGlobalInt("AI_WATER_UNIT_EXTRA_VALUE");
                AI_INVISIBLE_UNIT_EXTRA_VALUE = pInfos.getGlobalInt("AI_INVISIBLE_UNIT_EXTRA_VALUE");
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
                AI_TRASHED_TECH_VALUE_MODIFIER = pInfos.getGlobalInt("AI_TRASHED_TECH_VALUE_MODIFIER");
                AI_HAPPINESS_LEVEL_HAPPINESS_MODIFIER = pInfos.getGlobalInt("AI_HAPPINESS_LEVEL_HAPPINESS_MODIFIER");
                AI_WORKER_ORDER_SURPLUS_MODIFIER = pInfos.getGlobalInt("AI_WORKER_ORDER_SURPLUS_MODIFIER");
                AI_TECH_VALUE_TURN_MODIFIER = pInfos.getGlobalInt("AI_TECH_VALUE_TURN_MODIFIER");
                AI_BORDER_EXPANSION_VALUE_MODIFIER = pInfos.getGlobalInt("AI_BORDER_EXPANSION_VALUE_MODIFIER");

                AI_GROWTH_CITY_SPECIALIZATION_MODIFIER = pInfos.getGlobalInt("AI_GROWTH_CITY_SPECIALIZATION_MODIFIER");
                AI_CIVICS_CITY_SPECIALIZATION_MODIFIER = pInfos.getGlobalInt("AI_CIVICS_CITY_SPECIALIZATION_MODIFIER");
                AI_TRAINING_CITY_SPECIALIZATION_MODIFIER = pInfos.getGlobalInt("AI_TRAINING_CITY_SPECIALIZATION_MODIFIER");
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

            //public override void refreshCachedValues()
            //{
            //    base.refreshCachedValues();
            //    BAI_mpAICache.clear();
            //}

            //lines 1362-1365
            //protected override void cacheCityYieldValue(YieldType eYield, City pCity)
            //{
            //    mpAICache.setCityYieldValue(eYield, pCity.getID(), calculateCityYieldValue(eYield, pCity, out long iValueFlat));
            //    BAI_mpAICache.setCityYieldValueFlat(eYield, pCity.getID(), iValueFlat);
            //}


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

            public virtual int cityYieldSpecializationModifier(City pCity, YieldType eYield)
            {
                int iYieldRate = pCity.calculateModifiedYield(eYield);
                int iYieldRatePlayer = 0;
                int iNumPlayerCities = 0;
                int iYieldRateAverage;
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

                if (iNumPlayerCities > 1 && iYieldRate > iYieldRateAverage && iYieldRateAverage > 0)
                {
                    if (eYield == infos.Globals.TRAINING_YIELD)
                    {
                        iSpecializationModifier = AI_TRAINING_CITY_SPECIALIZATION_MODIFIER;
                    }
                    else if (eYield == infos.Globals.CIVICS_YIELD)
                    {
                        iSpecializationModifier = AI_CIVICS_CITY_SPECIALIZATION_MODIFIER;
                    }
                    else if (eYield == infos.Globals.GROWTH_YIELD)
                    {
                        iSpecializationModifier = AI_GROWTH_CITY_SPECIALIZATION_MODIFIER;
                    }

                    if (iSpecializationModifier > 0)
                    {
                        iSpecializationModifier *= iYieldRate;
                        iSpecializationModifier /= iYieldRateAverage;
                        infos.utils().modify(iSpecializationModifier, -(100 / iNumPlayerCities));
                    }
                }

                return iSpecializationModifier;

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
                                iValue -= ((pCity.getCitizens() - 2) * iValue) / pCity.getCitizens();  //overpopulation
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
                int iUnits = Math.Min(3 * getCities().Count, getNetOrdersAfterUnitsWhole() / 2);
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't go crazy with mil units  END ###
  ##############################################*/

                for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos.improvementsNum(); ++eLoopImprovement)
                {
                    if (isFort(eLoopImprovement))
                    {
                        iUnits += player.getFinishedImprovementCount(eLoopImprovement);
                    }
                }
                if (player != null)
                {
                    for (int iTileID = 0; iTileID < game.getNumTiles(); ++iTileID)
                    {
                        Tile pLoopTile = game.tile(iTileID);
                        if (pLoopTile != null)
                        {
                            //if (!pLoopTile.hasCityTerritory() && pLoopTile.hasRevealedImprovement(player.getTeam()) && pLoopTile.hasImprovementFinished())
                            if (!pLoopTile.hasCityTerritory() && pLoopTile.isVisible(player.getTeam()) && pLoopTile.hasImprovementFinished())
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

                int iModifier = 0;
                if (!isPeaceful() || game.isCompetitiveGameMode())
                {
                    iModifier += 50;
                }
                else
                {
                    for (PlayerType eLoopPlayer = 0; eLoopPlayer < game.getNumPlayers(); ++eLoopPlayer)
                    {
                        Player pLoopPlayer = game.player(eLoopPlayer);
                        if (pLoopPlayer.isAlive() && !game.areTeamsAllied(pLoopPlayer.getTeam(), Team) && game.isTeamContact(Team, pLoopPlayer.getTeam()))
                        {
                            PowerType eStrength = player.calculatePowerOf(eLoopPlayer);
                            if (eStrength != PowerType.NONE)
                            {
                                iModifier = Math.Max(iModifier, infos.power(eStrength).miWarModifier);
                            }
                        }
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### AI: Don't go crazy with mil units START ##
  ##############################################*/
                //return infos.utils().modify(iUnits, iModifier);
                return Math.Max(1 + (2 * getCities().Count), infos.utils().modify(iUnits, iModifier));
/*####### Better Old World AI - Base DLL #######
  ### AI: Don't go crazy with mil units  END ###
  ##############################################*/
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

                if (pTile.getImprovement() != eImprovement && !pTile.canHaveImprovement(eImprovement, pCity, bTestTerritory: false, bTestEnabled: false, bTestAdjacent: false, bTestReligion: false))
                {
                    return -1;
                }

                InfoImprovement pImprovementInfo = infos.improvement(eImprovement);

                if (pImprovementInfo.mbUrban && !pTile.isUrban() && pTile.hasResource())
                {
                    return -1;
                }

                // Don't even evaluate the Jerwan in a bad location
                if (pImprovementInfo.meAdjacentImprovementSpecialist != ImprovementType.NONE)
                {
                    int iNumEligible = 0;
                    for (DirectionType eDir = 0; eDir < DirectionType.NUM_TYPES; ++eDir)
                    {
                        Tile pAdjacent = pTile.tileAdjacent(eDir);
                        if (pAdjacent != null)
                        {
                            if (pAdjacent.getImprovement() == pImprovementInfo.meAdjacentImprovementSpecialist || pAdjacent.canHaveImprovement(pImprovementInfo.meAdjacentImprovementSpecialist, pCity, bTestTerritory: false, bTestEnabled: false, bTestAdjacent: false, bTestReligion: false))
                            {
                                ++iNumEligible;
                            }
                        }
                    }
                    if (iNumEligible < 4)
                    {
                        return -1;
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

                bool bRemove = pTile.getImprovement() == eImprovement;

                ImprovementClassType eImprovementClass = pImprovementInfo.meClass;

                long iValue = 0;

                {
                    EffectCityType eEffectCity = pImprovementInfo.meEffectCity;

                    if (eEffectCity != EffectCityType.NONE)
                    {
                        iValue += effectCityValue(eEffectCity, pCity, bRemove);
                    }
                }

                {
                    long iBestUnitValue = 0;
                    UnitType eBestUnit = UnitType.NONE;
                    for (UnitType eLoopUnit = 0; eLoopUnit < infos.unitsNum(); ++eLoopUnit)
                    {
                        if (player.canEverBuildUnit(eLoopUnit))
                        {
                            if (infos.unit(eLoopUnit).meImprovementPrereq == eImprovement)
                            {
                                long iUnitValue = getUnitBuildValue(eLoopUnit, pCity);
                                if (iUnitValue > iBestUnitValue)
                                {
                                    iBestUnitValue = iUnitValue;
                                    eBestUnit = eLoopUnit;
                                }
                            }
                        }
                    }
                    if (iBestUnitValue > 0)
                    {
                        iValue += pCity.calculateModifiedYield(infos.unit(eBestUnit).meProductionType) * AI_CITY_UNITS_BUILT_PER_TRAINING / Constants.YIELDS_MULTIPLIER;
                    }
                }

                if (pImprovementInfo.miUnitTurns > 0)
                {
                    long iUnitValue = 0;
                    int iDiceTotal = 0;
                    for (UnitType eFreeUnit = 0; eFreeUnit < infos.unitsNum(); ++eFreeUnit)
                    {
                        int iDie = pImprovementInfo.maiUnitDie[eFreeUnit];
                        if (iDie > 0)
                        {
                            iDiceTotal += iDie;
                            iUnitValue += unitValue(eFreeUnit, pCity, false) * iDie;
                        }
                    }
                    if (iDiceTotal > 0)
                    {
                        iValue = iUnitValue * AI_YIELD_TURNS / iDiceTotal / pImprovementInfo.miUnitTurns;
                    }
                }

                if (eImprovementClass != ImprovementClassType.NONE)
                {
                    {
                        EffectCityType eEffectCity = infos.improvementClass(eImprovementClass).meEffectCity;

                        if (eEffectCity != EffectCityType.NONE)
                        {
                            iValue += effectCityValue(eEffectCity, pCity, bRemove);
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
                    int iPlayerImprovements = player.getFinishedImprovementCount(eImprovement);
                    int iPlayerImprovementClasses = eImprovementClass != ImprovementClassType.NONE ? player.getFinishedImprovementClassCount(eImprovementClass) : 0;
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
                            if (bImprovementNeeded)
                            {
                                iValue += stateReligionValue(eLoopReligion, false, false);
                            }
                        }
                    }
                }

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

                                if (pImprovementInfo.meAdjacentImprovementSpecialist != ImprovementType.NONE)
                                {
                                    aeImprovements.Add(pImprovementInfo.meAdjacentImprovementSpecialist);
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
                                            int iModifier = pCity.calculateTotalYieldModifier(eLoopYield);
                                            int iAdjacentModifier = (pAdjacentCity == pCity) ? iModifier : pAdjacentCity.calculateTotalYieldModifier(eLoopYield);

                                            if (pAdjacentCity.getPlayer() == getPlayer())
                                            {
                                                iAdjacentValue += infos.utils().modify(getYieldToAdjacent(eLoopYield, eImprovement, eLoopImprovement, pAdjacent.getResource()), iAdjacentModifier) * cityYieldValue(eLoopYield, pAdjacentCity);
                                            }
                                            iAdjacentValue += infos.utils().modify(getYieldToAdjacent(eLoopYield, eLoopImprovement, eImprovement, pTile.getResource()), iModifier) * cityYieldValue(eLoopYield, pCity);
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

                int iExtraCivicsProduction = 0;

                using (var effectCityExtraCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
                {
                    Dictionary<EffectCityType, int> dEffectCityExtraCounts = effectCityExtraCountsScoped.Value;

                    //SpecialistType eImprovementSpecialist = ((bRemove || pTile.isSpecialistValid(pTile.getSpecialist(), eImprovement)) ? pTile.getSpecialist() : SpecialistType.NONE);
                    SpecialistType eImprovementSpecialist;
                    if (bRemove || pTile.isSpecialistValid(pTile.getSpecialist(), eImprovement))
                    {
                        eImprovementSpecialist = pTile.getSpecialist();
                    }
                    else
                    {
                        eImprovementSpecialist = pTile.getFreeSpecialist(eImprovement);

                        if (pTile.getSpecialist() != SpecialistType.NONE && pCity == pTile.cityTerritory())
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
                        long iTileOutputValue = ((BetterAITile)pTile).yieldOutputForGovernor(eImprovement, SpecialistType.NONE, eLoopYield, pCity, bCityEffects: false, bBaseOnly: false, pCity.governor(), dEffectCityExtraCounts);

                        if (eLoopYield == infos.Globals.CIVICS_YIELD)
                        {
                            if (eImprovementSpecialist != SpecialistType.NONE)
                            {
                                iExtraCivicsProduction = ((BetterAITile)pTile).yieldOutputForGovernor(eImprovement, eImprovementSpecialist, eLoopYield, pCity, bCityEffects: false, bBaseOnly: false, pCity.governor(), dEffectCityExtraCounts);
                            }
                            else
                            {
                                iExtraCivicsProduction = (int)iTileOutputValue;
                            }
                        }

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
                            iTileOutputValue = infos.utils().modify((iTileOutputValue * cityYieldValue(eLoopYield, pCity)), pCity.calculateTotalYieldModifier(eLoopYield));
                            iValue += iTileOutputValue * AI_YIELD_TURNS / Constants.YIELDS_MULTIPLIER;
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

                // encourage new improvements that allow good specialists
/*####### Better Old World AI - Base DLL #######
  ### AI: less value for specialist    START ###
  ##############################################*/
                if (infos.improvement(eImprovement).meSpecialist != SpecialistType.NONE && (pTile.getSpecialist() == SpecialistType.NONE || !pTile.isSpecialistValid(pTile.getSpecialist(), eImprovement)))
                {
                    //There is value in having the option to build another specialist. 
                    //This value is reduced if there are already other specialist build options present, or if citizens can also be used to rush productions.

                    long iSpecialistBuildValue = getSpecialistBuildValue(infos.improvement(eImprovement).meSpecialist, pTile, pCity, 
                        Math.Max(1, pCity.calculateModifiedYield(infos.Globals.CIVICS_YIELD) + iExtraCivicsProduction), 
                        player.getSpecialistBuildCost(infos.improvement(eImprovement).meSpecialist, pCity, eImprovement), eImprovement, bIncludeUnlock: true);

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


                if (pTile.getImprovement() == eImprovement && pTile.isPillaged())
                {
                    for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                    {
                        iValue += infos.Helpers.getBuildCost(eImprovement, eLoopYield, pTile) * yieldValue(eLoopYield);
                    }
                }

                if (pImprovementInfo.mbNoVegetation && pTile.hasVegetation())
                {
                    for (YieldType eLoopYield = 0; eLoopYield < infos.yieldsNum(); ++eLoopYield)
                    {
                        int iYieldAmount = player.getYieldRemove(pTile, eLoopYield, true, pCity);

                        iYieldAmount -= getYieldLostFromClear(pTile, pCity, eLoopYield, pTile.getVegetation());
                        if (pTile.vegetation().meVegetationRemove != VegetationType.NONE)
                        {
                            iYieldAmount -= getYieldLostFromClear(pTile, pCity, eLoopYield, pTile.vegetation().meVegetationRemove);
                        }
                        iValue += iYieldAmount * yieldValue(eLoopYield);
                    }
                }

                iValue += getImprovementFamilyOpinionValue(pCity, eImprovement);

                if (pImprovementInfo.mbUrban && !bRemove)
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

            public virtual long cityEffectExtraValueFromExtraCityEffects(EffectCityType eEffectCity, City pCity, Dictionary<EffectCityType, int> dEffectCityExtraCounts)
            {
                long iValue = 0;
                InfoEffectCity infoEffectCity = infos.effectCity(eEffectCity);

                foreach (var zTriple in infoEffectCity.maaiEffectCityYieldRate)
                {
                    int iCount = dEffectCityExtraCounts.GetOrDefault(zTriple.Item1, 0);
                    if (iCount > 0)
                    {
                        iValue += infos.utils().modify((iCount * zTriple.Item3 * cityYieldValue(zTriple.Item2, pCity)), pCity.calculateTotalYieldModifier(zTriple.Item2)) * AI_YIELD_TURNS / Constants.YIELDS_MULTIPLIER;
                    }
                }

                foreach (var p in dEffectCityExtraCounts)
                {
                    foreach (var zTriple in infos.effectCity(p.Key).maaiEffectCityYieldRate)
                    {
                        if (zTriple.Item1 == eEffectCity)
                        {
                            iValue += infos.utils().modify((zTriple.Item3 * cityYieldValue(zTriple.Item2, pCity)), pCity.calculateTotalYieldModifier(zTriple.Item2)) * AI_YIELD_TURNS / Constants.YIELDS_MULTIPLIER;
                        }
                    }
                }
                return iValue;
            }

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
                {
                    Dictionary<EffectCityType, int> dEffectCityCounts = effectCityCountsScoped.Value;
                    Dictionary<EffectCityType, int> dEffectCityExtraCounts = effectCityExtraCountsScoped.Value;
                    Dictionary<EffectCityType, int> dEffectCityImprovementOnlyExtraCounts = effectCityImprovementOnlyExtraCountsScoped.Value;

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


                    foreach (var p in dEffectCityExtraCounts)
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
                            iValue += cityEffectExtraValueFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts); //extra value from maaiEffectCityYieldRate
                        }
                    }

                    {
                        EffectCityType eEffectCity = infos.specialist(eSpecialist).meEffectCityExtra;
                        if (eEffectCity != EffectCityType.NONE)
                        {
                            iValue += effectCityValue(eEffectCity, pCity, bRemove);
                            iValue += cityEffectExtraValueFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts); //extra value from maaiEffectCityYieldRate
                        }
                    }

                    if (eSpecialistClass != SpecialistClassType.NONE)
                    {
                        EffectCityType eEffectCity = infos.specialistClass(eSpecialistClass).meEffectCity;
                        if (eEffectCity != EffectCityType.NONE)
                        {
                            iValue += effectCityValue(eEffectCity, pCity, bRemove);
                            iValue += cityEffectExtraValueFromExtraCityEffects(eEffectCity, pCity, dEffectCityExtraCounts); //extra value from maaiEffectCityYieldRate
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
                                iValue += cityEffectExtraValueFromExtraCityEffects(eResourceEffectCity, pCity, dEffectCityExtraCounts); //extra value from maaiEffectCityYieldRate
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
                            int iOutput = ((BetterAITile)pTile).yieldOutputForGovernor(eImprovement, eSpecialist, eLoopYield, pCity, bCityEffects: false, bBaseOnly: false, pCity.governor(), !bRemove ? dEffectCityExtraCounts : null);
                            iOutput -= ((BetterAITile)pTile).yieldOutputForGovernor(eImprovement, SpecialistType.NONE, eLoopYield, pCity, bCityEffects: false, bBaseOnly: false, pCity.governor(), bRemove ? dEffectCityExtraCounts : dEffectCityImprovementOnlyExtraCounts); //output without the specialist and without the city effect from that specialist

                            int iExtraModifier = 0;
                            foreach (var p in dEffectCityCounts)
                            {
                                iExtraModifier += infos.effectCity(p.Key).maiYieldModifier[eLoopYield] * p.Value;
                            }

                            foreach (var p in dEffectCityCounts)
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

            //lines 15046-15076
            protected override long getFamilyOpinionValue(FamilyType eFamily, int iOpinionChange, int iTurns)
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
                if (!player.isFamilyStarted(eFamily))
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
                        MohawkAssert.Assert(AreCacheWarningsMuted, "Unexpected improvement value calculation");
                        if (bModified)
                        {
                            iValue = improvementValueTile(eImprovement, pTile, pCity, bIncludeCost: false, bSubtractCurrent: false, bModified: false);
                            if (iValue > 0)
                            {
                                modifyImprovementValue(eImprovement, pTile, pCity, ref iValue);
                            }

                            lock (gameCacheLock)
                            {
                                mpAICache.setImprovementModifiedValue(pTile.getID(), pCity.getID(), eImprovement, iValue);
                            }

                        }
                        else
                        {
                            iValue = calculateImprovementValueForTile(pTile, pCity, eImprovement);
                            if (iValue > 0)
                            {
                                iValue += calculateImprovementDependentValueForTile(pTile, pCity, eImprovement);
                            }

                            lock (gameCacheLock)
                            {
                                mpAICache.setImprovementBaseValue(pTile.getID(), pCity.getID(), eImprovement, iValue);
                            }
                        }
                    }

                    if (iValue < 0)
                    {
                        return iValue;
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
