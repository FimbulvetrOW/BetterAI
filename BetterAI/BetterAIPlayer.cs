using System;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Concurrent;
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

namespace BetterAI
{
    public partial class BetterAIPlayer : Player
    {

/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites     START ###
  ##############################################*/
        protected virtual UnitType getCurrentFoundUnitType()
        {
            //UnityEngine.Debug.Log("Player.getCurrentFoundUnitType");

            //simplified, assuming there is only 1 founding Unittype at any given time
            //copied from countFoundUnitsAndBuilds, and adapted/rearranged

            //using (new UnityProfileScope("Player.countFoundUnitsAndBuilds"))
            {
                //int iCount = 0;

                foreach (int iUnitID in getUnits())
                {
                    Unit pUnit = game().unit(iUnitID);

                    if ((pUnit != null) && pUnit.isAlive())
                    {
                        if (pUnit.info().mbFound)
                        {
                            return pUnit.getType();
                        }
                    }
                }

                foreach (int iCityID in getCities())
                {
                    City pLoopCity = game().city(iCityID);
                    if (pLoopCity != null)
                    {
                        for (int iIndex = 0; iIndex < pLoopCity.getBuildCount(); ++iIndex)
                        {
                            CityQueueData pLoopBuild = pLoopCity.getBuildQueueNode(iIndex);

                            if ((pLoopBuild.meBuild == infos().Globals.UNIT_BUILD) && infos().unit((UnitType)(pLoopBuild.miType)).mbFound)
                            {
                                return (UnitType)(pLoopBuild.miType);
                            }
                        }
                    }

                }

                //return iCount;
            }
            return UnitType.NONE;
        }
/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites       END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Does improvement spread borders  START ###
  ##############################################*/
        public virtual bool improvementSpreadsBorders(ImprovementType eImprovement, int iTileID)
        {
            return ((BetterAIInfoHelpers)(infos().Helpers)).improvementSpreadsBorders(eImprovement, game(), this, game()?.tile(iTileID));
        }

/*####### Better Old World AI - Base DLL #######
  ### Does improvement spread borders    END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Law: remaining Techs to research START ###
  ##############################################*/
        public virtual void getUnlockTechCount(TechType eTech, ref int iCount, ref bool bInvalid)
        {
            //UnityEngine.Debug.Log("Player.getUnlockTechCount");

            if (!isTechValid(eTech, true) || isTechTrashed(eTech) || isTechLocked(eTech))
            {
                bInvalid = true;
                return;
            }
            if (isTechAcquired(eTech))
            {
                return;
            }

            iCount++;
            //in-hand || discard pile || draw pile
            if (isTechAvailable(eTech) || isTechPassed(eTech) || canMakeTechAvailable(eTech))
            {
                return;
            }
            //else

            for (TechType eLoopTech = 0; eLoopTech < infos().techsNum(); eLoopTech++)
            {
                if (game().isTechPrereq(eTech, eLoopTech))
                {
                    getUnlockTechCount(eTech, ref iCount, ref bInvalid);
                }
            }

            return;
        }
/*####### Better Old World AI - Base DLL #######
  ### Law: remaining Techs to research   END ###
  ##############################################*/


        //lines 8855-9323
        public override void changeEffectPlayerCount(EffectPlayerType eIndex, int iChange)
        {
            //UnityEngine.Debug.Log("Player.changeEffectPlayerCount");

            if (iChange != 0 && eIndex != EffectPlayerType.NONE)
            {
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ###  EffectPlayer combinations             ###
  ##############################################*/
                using (var effectPlayerIngoreListScoped = CollectionCache.GetListScoped<EffectPlayerType>())
                using (var effectPlayerCountChangeDictionaryScoped = CollectionCache.GetDictionaryScoped<EffectPlayerType, int>())
                using (var effectPlayerCountChangeExtraDictionaryScoped = CollectionCache.GetDictionaryScoped<EffectPlayerType, int>())
                {
                    List<EffectPlayerType> aeEffectPlayerIgnore = effectPlayerIngoreListScoped.Value;
                    Dictionary<EffectPlayerType, int> effectPlayerCountChange = effectPlayerCountChangeDictionaryScoped.Value;
                    Dictionary<EffectPlayerType, int> effectPlayerCountChangeExtra = effectPlayerCountChangeExtraDictionaryScoped.Value;

                    getDependentEffectPlayerCountChanges(eIndex, iChange, ref aeEffectPlayerIgnore, ref effectPlayerCountChange);

                    //add extras from EffectPlayer.meEffectPlayer
                    foreach (KeyValuePair<EffectPlayerType, int> eLoopEffectPlayerChangeCount in effectPlayerCountChange)
                    {
                        EffectPlayerType eLoopLoopEffectPlayer = infos().effectPlayer(eLoopEffectPlayerChangeCount.Key).meEffectPlayer;
                        int iIterationsRemaining = 100;
                        while (eLoopLoopEffectPlayer != EffectPlayerType.NONE && iIterationsRemaining > 0)
                        {
                            effectPlayerCountChangeExtra.Add(eLoopLoopEffectPlayer, eLoopEffectPlayerChangeCount.Value);
                            eLoopLoopEffectPlayer = infos().effectPlayer(eLoopLoopEffectPlayer).meEffectPlayer;
                            iIterationsRemaining--;
                        }
                    }

                    //joining the dictionaries
                    foreach (KeyValuePair<EffectPlayerType, int> eLoopEffectPlayerChangeCountExtra in effectPlayerCountChangeExtra)
                    {
                        if (effectPlayerCountChange.ContainsKey(eLoopEffectPlayerChangeCountExtra.Key))
                        {
                            effectPlayerCountChange[eLoopEffectPlayerChangeCountExtra.Key] += eLoopEffectPlayerChangeCountExtra.Value;
                        }
                        else
                        {
                            effectPlayerCountChange.Add(eLoopEffectPlayerChangeCountExtra.Key, eLoopEffectPlayerChangeCountExtra.Value);
                        }
                    }

                    updateLastData(DirtyType.maiEffectPlayerCount, mpCurrentData.maiEffectPlayerCount, ref mpLastUpdateData.maiEffectPlayerCount);
                    //mpCurrentData.maiEffectPlayerCount[(int)eIndex] += iChange;

                    //updating mpCurrentData for all EffectPlayer counts
                    foreach (KeyValuePair<EffectPlayerType, int> eLoopEffectPlayerChangeCount in effectPlayerCountChange)
                    {
                        mpCurrentData.maiEffectPlayerCount[(int)eLoopEffectPlayerChangeCount.Key] += eLoopEffectPlayerChangeCount.Value;
                    }

                    //apply all player effects
                    foreach (KeyValuePair<EffectPlayerType, int> eLoopEffectPlayerChangeCount in effectPlayerCountChange)
                    {
                        eIndex = eLoopEffectPlayerChangeCount.Key;
                        iChange = eLoopEffectPlayerChangeCount.Value;
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ###  EffectPlayer combinations             ###
  ##############################################*/

                        {
                            int iValue = infos().effectPlayer(eIndex).miLegitimacy;
                            if (iValue != 0 && game().isCharacters())
                            {
                                changeLegitimacyBase(iValue * iChange);
                            }
                        }
                        {
                            int iValue = infos().effectPlayer(eIndex).miMaxOrders;
                            if (iValue != 0)
                            {
                                changeMaxOrders(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miHarvestModifier;
                            if (iValue != 0)
                            {
                                changeHarvestModifier(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miSellPenaltyModifier;
                            if (iValue != 0)
                            {
                                changeSellPenaltyModifier(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miConsumptionModifier;
                            if (iValue != 0)
                            {
                                changeConsumptionModifier(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miTechCostModifier;
                            if (iValue != 0)
                            {
                                changeTechCostModifier(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miWonderModifier;
                            if (iValue != 0)
                            {
                                changeWonderModifier(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miMissionModifier;
                            if (iValue != 0)
                            {
                                changeMissionModifier(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miStartLawModifier;
                            if (iValue != 0)
                            {
                                changeStartLawModifier(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miSwitchLawModifier;
                            if (iValue != 0)
                            {
                                changeSwitchLawModifier(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miYieldUpkeepModifier;
                            if (iValue != 0)
                            {
                                changeYieldUpkeepModifier(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miTrainingOrderModifier;
                            if (iValue != 0)
                            {
                                changeTrainingOrderModifier(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miStateReligionSpread;
                            if (iValue != 0)
                            {
                                changeStateReligionSpread(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miWorldReligionSpread;
                            if (iValue != 0)
                            {
                                changeWorldReligionSpread(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miVisionChange;
                            if (iValue != 0)
                            {
                                changeVisionChange(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miTribeFatigueChange;
                            if (iValue != 0)
                            {
                                changeTribeFatigueLimitChange(iValue * iChange);
                            }
                        }

                        {
                            int iValue = infos().effectPlayer(eIndex).miTechsAvailableChange;
                            if (iValue != 0)
                            {
                                changeTechsAvailableChange(iValue * iChange);
                            }
                        }

                        if (infos().effectPlayer(eIndex).miLeaderOpinionChange != 0 || infos().effectPlayer(eIndex).miLeaderDescendantOpinionChange != 0)
                        {
                            if (game().originalAllHumanTeams())
                            {
                                updateCharacterOpinionAll();
                            }
                            else
                            {
                                updateLeaderOpinionAll();
                            }
                        }

                        if (infos().effectPlayer(eIndex).miTribeLeaderOpinionChange != 0)
                        {
                            updateTribeLeaderOpinionAll();
                        }

                        if (infos().effectPlayer(eIndex).miReligionOpinionChange != 0)
                        {
                            updateReligionOpinionAll();
                        }

                        if (infos().effectPlayer(eIndex).miStateReligionOpinionChange != 0)
                        {
                            if (hasStateReligion())
                            {
                                updateReligionOpinion(getStateReligion());
                            }
                        }

                        if (infos().effectPlayer(eIndex).miLeaderReligionOpinionChange != 0)
                        {
                            Character pLeader = leader();

                            if (pLeader != null)
                            {
                                if (pLeader.hasReligion())
                                {
                                    updateReligionOpinion(pLeader.getReligion());
                                }
                            }
                        }

                        if (infos().effectPlayer(eIndex).miFamilyOpinionChange != 0)
                        {
                            updateFamilyOpinionAll();
                        }

                        if (infos().effectPlayer(eIndex).mbStartMusic)
                        {
                            changeStartMusicUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbRedrawTechs)
                        {
                            changeRedrawTechsUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbAddRoad)
                        {
                            changeAddRoadUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbAddUrban)
                        {
                            changeAddUrbanUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbRemoveAllVegetation)
                        {
                            changeRemoveAllVegetationUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbUpgradeImprovement)
                        {
                            changeUpgradeImprovementUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbMultipleWorkers)
                        {
                            changeMultipleWorkersUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbMoveAlliedUnits)
                        {
                            changeMoveAlliedUnitsUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbAgent)
                        {
                            changeAgentUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbRiverMovement)
                        {
                            changeRiverMovementUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbRiverBridging)
                        {
                            changeRiverBridgingUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbNoSellPenalty)
                        {
                            changeNoSellPenaltyUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbNoOutsideConsumption)
                        {
                            changeNoOutsideConsumptionUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbPurgeReligions)
                        {
                            changePurgeReligionsUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbBuildAllReligions)
                        {
                            changeBuildAllReligionsUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbPaganStateReligion)
                        {
                            changePaganStateReligionUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbLegitimacyOrders)
                        {
                            changeLegitimacyOrdersUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbOrdersScience)
                        {
                            changeOrdersToScienceUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbRecruitMercenaries)
                        {
                            changeRecruitMercenariesUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbHireMercenaries)
                        {
                            changeHireMercenariesUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbAutomateWorkers)
                        {
                            changeAutomateWorkersUnlock(iChange);
                        }

                        if (infos().effectPlayer(eIndex).mbAutomateScouts)
                        {
                            changeAutomateScoutsUnlock(iChange);
                        }

                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            changeYieldEffectPlayer(eLoopYield, iChange * infos().effectPlayer(eIndex).maiYieldRate[eLoopYield]);
                        }

                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            changeYieldLawsEffectPlayer(eLoopYield, iChange * infos().effectPlayer(eIndex).maiYieldRateLaws[eLoopYield]);
                        }

                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            changeYieldGeneralsEffectPlayer(eLoopYield, iChange * infos().effectPlayer(eIndex).maiYieldRateGenerals[eLoopYield]);
                        }

                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            changeYieldUpkeep(eLoopYield, iChange * infos().effectPlayer(eIndex).maiYieldUpkeep[eLoopYield]);
                        }

                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            changeMissionYieldModifier(eLoopYield, iChange * infos().effectPlayer(eIndex).maiMissionYieldCostModifier[eLoopYield]);
                        }

                        for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                        {
                            changeUnitTraitConsumptionModifier(eLoopUnitTrait, iChange * infos().effectPlayer(eIndex).maiUnitTraitConsumptionModifier[eLoopUnitTrait]);
                        }

                        for (JobType eLoopJob = 0; eLoopJob < infos().jobsNum(); eLoopJob++)
                        {
                            changeJobOpinionRate(eLoopJob, iChange * infos().effectPlayer(eIndex).maiJobOpinionRate[eLoopJob]);
                        }

                        foreach (YieldType eLoopYield in infos().effectPlayer(eIndex).maeTradeYield)
                        {
                            changeYieldTradeUnlock(eLoopYield, iChange);
                        }

                        foreach (YieldType eLoopYield in infos().effectPlayer(eIndex).maeNoBuyYield)
                        {
                            changeYieldBuyLock(eLoopYield, iChange);
                        }

                        foreach (YieldType eLoopYield in infos().effectPlayer(eIndex).maeNoSellPenaltyYield)
                        {
                            changeNoSellPenaltyYieldUnlock(eLoopYield, iChange);
                        }

                        foreach (YieldType eLoopYield in infos().effectPlayer(eIndex).maeConnectedForeign)
                        {
                            changeConnectedForeignUnlock(eLoopYield, iChange);
                        }

                        foreach (YieldType eLoopYield in infos().effectPlayer(eIndex).maeBuyTile)
                        {
                            changeBuyTileUnlock(eLoopYield, iChange);
                        }

                        foreach (UnitType eLoopUnit in infos().effectPlayer(eIndex).maeWaterUnit)
                        {
                            changeWaterUnitUnlock(eLoopUnit, iChange);
                        }

                        foreach (UnitType eLoopUnit in infos().effectPlayer(eIndex).maeInvisibleUnit)
                        {
                            changeHideUnitUnlock(eLoopUnit, iChange);
                        }

                        foreach (ImprovementType eLoopImprovement in infos().effectPlayer(eIndex).maeImprovementSpreadBorders)
                        {
                            changeSpreadBordersUnlock(eLoopImprovement, iChange);
                        }

                        foreach (JobType eLoopJob in infos().effectPlayer(eIndex).maeNoFamilyRestrictionJob)
                        {
                            changeNoFamilyRestrictionJobUnlock(eLoopJob, iChange);
                        }

/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ###  EffectPlayer combinations             ###
  ##############################################*/
                        // meEffectPlayer is alraedy handled at the start
                        //if (infos().effectPlayer(eIndex).meEffectPlayer != EffectPlayerType.NONE)
                        //{
                        //    changeEffectPlayerCount(infos().effectPlayer(eIndex).meEffectPlayer, iChange);
                        //}
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ###  EffectPlayer combinations             ###
  ##############################################*/

                        if (infos().effectPlayer(eIndex).meEffectCity != EffectCityType.NONE)
                        {
                            foreach (int iCityID in getCities())
                            {
                                game().city(iCityID)?.changeEffectCityCount(infos().effectPlayer(eIndex).meEffectCity, iChange);
                            }
                        }

                        if (infos().effectPlayer(eIndex).meEffectCityExtra != EffectCityType.NONE)
                        {
                            foreach (int iCityID in getCities())
                            {
                                game().city(iCityID)?.changeEffectCityCount(infos().effectPlayer(eIndex).meEffectCityExtra, iChange);
                            }
                        }

                        if (infos().effectPlayer(eIndex).meCapitalEffectCity != EffectCityType.NONE)
                        {
                            City pCapitalCity = capitalCity();

                            if (pCapitalCity != null)
                            {
                                pCapitalCity.changeEffectCityCount(infos().effectPlayer(eIndex).meCapitalEffectCity, iChange);
                            }
                        }

                        if (infos().effectPlayer(eIndex).meConnectedEffectCity != EffectCityType.NONE)
                        {
                            foreach (int iCityID in getCities())
                            {
                                City pLoopCity = game().city(iCityID);

                                if (pLoopCity?.isConnected() ?? false)
                                {
                                    pLoopCity.changeEffectCityCount(infos().effectPlayer(eIndex).meConnectedEffectCity, iChange);
                                }
                            }
                        }

                        if (infos().effectPlayer(eIndex).meNoGovernorEffectCity != EffectCityType.NONE)
                        {
                            foreach (int iCityID in getCities())
                            {
                                City pLoopCity = game().city(iCityID);

                                if (!(pLoopCity?.hasGovernor() ?? true))
                                {
                                    pLoopCity.changeEffectCityCount(infos().effectPlayer(eIndex).meNoGovernorEffectCity, iChange);
                                }
                            }
                        }

                        if (infos().effectPlayer(eIndex).meStateReligionEffectCity != EffectCityType.NONE)
                        {
                            if (hasStateReligion())
                            {
                                foreach (int iCityID in getCities())
                                {
                                    City pLoopCity = game().city(iCityID);

                                    if (pLoopCity?.isReligion(getStateReligion()) ?? false)
                                    {
                                        pLoopCity.changeEffectCityCount(infos().effectPlayer(eIndex).meStateReligionEffectCity, iChange);
                                    }
                                }
                            }
                        }

                        {
                            foreach (int iCityID in getCities())
                            {
                                City pLoopCity = game().city(iCityID);

                                if (pLoopCity?.hasFamily() ?? false)
                                {
                                    if (infos().effectPlayer(eIndex).maeFamilyClassEffectCity[pLoopCity.getFamilyClass()] != EffectCityType.NONE)
                                    {
                                        pLoopCity.changeEffectCityCount(infos().effectPlayer(eIndex).maeFamilyClassEffectCity[pLoopCity.getFamilyClass()], iChange);
                                    }
                                }
                            }
                        }

                        {
                            EffectUnitType eEffectUnit = infos().effectPlayer(eIndex).meEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                foreach (int iUnitID in getUnits())
                                {
                                    game().unit(iUnitID).changeEffectUnit(eEffectUnit, SourceEffectUnitType.EFFECT_PLAYER, iChange);
                                }
                            }
                        }

                        for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                        {
                            EffectUnitType eEffectUnit = infos().effectPlayer(eIndex).maeEffectUnitTrait[eLoopUnitTrait];

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                foreach (int iUnitID in getUnits())
                                {
                                    Unit pLoopUnit = game().unit(iUnitID);

                                    if (pLoopUnit.info().maeUnitTrait.Contains(eLoopUnitTrait))
                                    {
                                        game().unit(iUnitID).changeEffectUnit(eEffectUnit, SourceEffectUnitType.EFFECT_PLAYER, iChange);
                                    }
                                }
                            }
                        }

                        for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                        {
                            EffectUnitType eEffectUnit = infos().effectPlayer(eIndex).maeEffectUnit[eLoopUnit];

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                foreach (int iUnitID in getUnits())
                                {
                                    Unit pLoopUnit = game().unit(iUnitID);

                                    if (pLoopUnit.getType() == eLoopUnit)
                                    {
                                        game().unit(iUnitID).changeEffectUnit(eEffectUnit, SourceEffectUnitType.EFFECT_PLAYER, iChange);
                                    }
                                }
                            }
                        }

                        if (infos().effectPlayer(eIndex).miVP > 0)
                        {
                            game().doVictory();
                        }
                    }

                }
            }
        }

        public virtual void getDependentEffectPlayerCountChanges(EffectPlayerType eIndex, int iChange, ref List<EffectPlayerType> aeEffectPlayerIgnore, ref Dictionary<EffectPlayerType, int> effectPlayerCountChange, int iMaxDepth = 100)
        {
            //UnityEngine.Debug.Log("Player.getDependentEffectPlayerCountChanges");

            using (var effectPlayerTurnsRemainingDictionaryScoped = CollectionCache.GetDictionaryScoped<EffectPlayerType, int>())
            {
                Dictionary<EffectPlayerType, int> effectPlayerTurnsRemaining = effectPlayerTurnsRemainingDictionaryScoped.Value;
                BetterAIPlayerAI pPlayerAI = (BetterAIPlayerAI)AI;
                int iMaxTurnsRemainingX10 = 1000;
                if (pPlayerAI != null)
                {
                    iMaxTurnsRemainingX10 = pPlayerAI.getAI_YIELD_TURNS_X10();
                }

                getDependentEffectPlayerCountChangesWithTurnsRemaining(eIndex, iChange, iMaxTurnsRemainingX10: iMaxTurnsRemainingX10, ref aeEffectPlayerIgnore, ref effectPlayerCountChange, ref effectPlayerTurnsRemaining, bSkipTurnsRemaining: true, iMaxDepth);
            }

            return;

            //if (iChange != 0)
            //{
            //    //base.changeEffectPlayerCount(eIndex, iChange);
            //    if (effectPlayerCountChange.ContainsKey(eIndex))
            //    {
            //        effectPlayerCountChange[eIndex] += iChange;
            //    }
            //    else
            //    {
            //        effectPlayerCountChange.Add(eIndex, iChange);
            //    }

            //    if (((BetterAIInfoEffectPlayer)infos().effectPlayer(eIndex)).bAnyEffectPlayerEffectPlayer && iMaxDepth > 0)
            //    {
            //        aeEffectPlayerIgnore.Add(eIndex);
            //        for (EffectPlayerType eLoopEffectPlayer = 0; eLoopEffectPlayer < infos().effectPlayersNum(); eLoopEffectPlayer++)
            //        {
            //            if (aeEffectPlayerIgnore.Contains(eLoopEffectPlayer) || !(((BetterAIInfoEffectPlayer)infos().effectPlayer(eLoopEffectPlayer)).bAnyEffectPlayerEffectPlayer))
            //            {
            //                continue;
            //            }

            //            int iCount = getEffectPlayerCount(eLoopEffectPlayer);
            //            if (iCount > 0)
            //            {
            //                EffectPlayerType eEffectPlayerEffectPlayer = ((BetterAIInfoEffectPlayer)infos().effectPlayer(eIndex)).maeEffectPlayerEffectPlayer[eLoopEffectPlayer];
            //                if (eEffectPlayerEffectPlayer == EffectPlayerType.NONE)
            //                {
            //                    eEffectPlayerEffectPlayer = ((BetterAIInfoEffectPlayer)infos().effectPlayer(eLoopEffectPlayer)).maeEffectPlayerEffectPlayer[eIndex];
            //                }

            //                if (eEffectPlayerEffectPlayer != EffectPlayerType.NONE)
            //                {
            //                    getDependentEffectPlayerCountChanges(eEffectPlayerEffectPlayer, iChange * iCount, ref aeEffectPlayerIgnore, ref effectPlayerCountChange, (iMaxDepth - 1));
            //                }

            //            }
            //        }
            //        aeEffectPlayerIgnore.Remove(eIndex);
            //    }

            //}
        }



        public virtual void getDependentEffectPlayerCountChangesWithTurnsRemaining(EffectPlayerType eIndex, int iChange, int iMaxTurnsRemainingX10, ref List<EffectPlayerType> aeEffectPlayerIgnore, ref Dictionary<EffectPlayerType, int> effectPlayerCountChange, ref Dictionary<EffectPlayerType, int> effectPlayerTurnsRemainingX10, bool bSkipTurnsRemaining = true, int iMaxDepth = 100)
        {
            //UnityEngine.Debug.Log("Player.getDependentEffectPlayerCountChangesWithTurnsRemaining");

            if (iChange != 0)
            {
                BetterAIInfoEffectPlayer pEffectPlayer = (BetterAIInfoEffectPlayer)infos().effectPlayer(eIndex);

                if (effectPlayerCountChange.ContainsKey(eIndex))
                {
                    effectPlayerCountChange[eIndex] += iChange;
                }
                else
                {
                    effectPlayerCountChange.Add(eIndex, iChange);

                    if (!bSkipTurnsRemaining)
                    {
                        if (!pEffectPlayer.mbPermanent)
                        {
                            if (!effectPlayerTurnsRemainingX10.ContainsKey(eIndex))
                            {
                                iMaxTurnsRemainingX10 = ((BetterAIPlayerAI)AI).getEffectPlayerTurnsRemainingX10(eIndex, iMaxTurnsRemainingX10); //iMaxTurnsRemainingX10 reduced only for further down the ladder, since iMaxTurnsRemainingX10 not passed by ref
                                effectPlayerTurnsRemainingX10.Add(eIndex, iMaxTurnsRemainingX10);
                            }
                        }
                    }

                }

                if (pEffectPlayer.bAnyEffectPlayerEffectPlayer && iMaxDepth > 0)
                {
                    aeEffectPlayerIgnore.Add(eIndex);
                    for (EffectPlayerType eLoopEffectPlayer = 0; eLoopEffectPlayer < infos().effectPlayersNum(); eLoopEffectPlayer++)
                    {
                        if (aeEffectPlayerIgnore.Contains(eLoopEffectPlayer) || !(((BetterAIInfoEffectPlayer)infos().effectPlayer(eLoopEffectPlayer)).bAnyEffectPlayerEffectPlayer))
                        {
                            continue;
                        }

                        int iCount = getEffectPlayerCount(eLoopEffectPlayer);
                        if (iCount > 0)
                        {
                            EffectPlayerType eEffectPlayerEffectPlayer = pEffectPlayer.maeEffectPlayerEffectPlayer[eLoopEffectPlayer];
                            if (eEffectPlayerEffectPlayer == EffectPlayerType.NONE)
                            {
                                eEffectPlayerEffectPlayer = ((BetterAIInfoEffectPlayer)infos().effectPlayer(eLoopEffectPlayer)).maeEffectPlayerEffectPlayer[eIndex];
                            }

                            if (eEffectPlayerEffectPlayer != EffectPlayerType.NONE)
                            {
                                aeEffectPlayerIgnore.Add(eLoopEffectPlayer);
                                getDependentEffectPlayerCountChangesWithTurnsRemaining(eEffectPlayerEffectPlayer, iChange * iCount, iMaxTurnsRemainingX10, ref aeEffectPlayerIgnore, ref effectPlayerCountChange, ref effectPlayerTurnsRemainingX10, bSkipTurnsRemaining, (iMaxDepth - 1));
                                aeEffectPlayerIgnore.Remove(eLoopEffectPlayer);
                            }

                        }
                    }
                    aeEffectPlayerIgnore.Remove(eIndex);
                }

            }
        }

        //lines 9720-9755
        //setCouncilCharacter: no longer needed, council job trait effects are handled in base game via trait.maeCouncilEffectPlayer


/*####### Better Old World AI - Base DLL #######
  ### Fix: Handling of Regents for           ###
  ### legitimacy from former leaders   START ###
  ##############################################*/
        //lines 10388-10391
        public override int findLeaderIndex(Character pCharacter, bool bIncludeRegents)
        {
            ////UnityEngine.Debug.Log("Player.findLeaderIndex");

            int iLeaderIndex = getLeaders().IndexOf(pCharacter.getID());
            if (!bIncludeRegents || iLeaderIndex == -1)
            {
                return iLeaderIndex;
            }

            int iNumPriorRegents = 0;
            //if (!bIncludeRegents)
            {
                for (int i = 0; i < iLeaderIndex - 1; i++) //stop before iLeaderIndex because game().character(getLeaders()[iLeaderIndex] == pCharacter
                                                        //and pCharacter is handled separately below
                {
                    if (game().character(getLeaders()[i]).isOrWasRegent())
                    {
                        ++iNumPriorRegents;
                    }
                }
                if ((!pCharacter.isLeader() && pCharacter.isOrWasRegent()))
                {
                    iNumPriorRegents += ((BetterAIInfoGlobals)infos().Globals).BAI_PROPER_REGENT_LEGITIMACY_DECAY;
                    // BAI_PROPER_REGENT_LEGITIMACY_DECAY == 1: when a Chosen Heir (or anyone else) takes over after a Regent,
                    // legitimacy from the now-former Regent is 1/2, like the legitimacy from the leader before the Regent,
                    // and will continue to have the same decay factor as that leader from before.
                    // BAI_PROPER_REGENT_LEGITIMACY_DECAY == 0 (unmodded): when a Chosen Heir (or anyone else) takes over after a Regent,
                    // legitimacy from the now-former Regent is full 1/1, like the legitimacy from the current leader,
                    // and will continue to have the same decay factor as that current leader.
                }
            }

            return iLeaderIndex - iNumPriorRegents;
        }

        public override int getNumLeaders(bool bIncludeRegents = true)
        {
            ////UnityEngine.Debug.Log("Player.getNumLeaders");

            if (bIncludeRegents)
            {
                return mpCurrentData.mliLeaders.Count;
            }

            int iNumLeaders = 1; //always count current leader
            for (int i = 0; i < getLeaders().Count - 1 ; i++)  //don't check current leader
            {
                if (!game().character(getLeaders()[i]).isOrWasRegent())
                {
                    ++iNumLeaders;
                }
            }

            return iNumLeaders;
        }
/*####### Better Old World AI - Base DLL #######
  ### Fix: Handling of Regents for           ###
  ### legitimacy from former leaders   START ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Less development cities variation START ##
  ##############################################*/
        //lines 15239-15242
        public override bool isStartCityNumberFlexible()
        {
            //UnityEngine.Debug.Log("Player.isStartCityNumberFlexible");

            return base.isStartCityNumberFlexible() && (((BetterAIInfoGlobals)(infos().Globals)).BAI_PLAYER_MAX_EXTRA_DEVELOPMENT_CITIES_PERCENT > 0);
        }

/*####### Better Old World AI - Base DLL #######
  ### Less development cities variation  END ###
  ##############################################*/


        public virtual int countMinCultureCities(CultureType eCulture, bool bHolyCitiesOnly = false)
        {
            //UnityEngine.Debug.Log("Player.countMinCultureCities");

            //using var profileGame = new UnityProfileScope("Player.countMinCultureCities");

            int iCount = 0;
            foreach (int iCityID in getCities())
            {
                City pLoopCity = game().city(iCityID);
                if (pLoopCity != null && (!bHolyCitiesOnly || pLoopCity.isReligionHolyCityAny()) && pLoopCity.getCulture() >= eCulture)
                {
                    ++iCount;
                }
            }
            return iCount;
        }


/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again      START ###
  ##############################################*/
        //lines 16282-16285
        public override bool isCityMaxReached()
        {
            //UnityEngine.Debug.Log("Player.isCityMaxReached");

            return ((getNumCities() + countFoundUnitsAndBuilds()) >= getCityMax());
        }
/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again        END ###
  ##############################################*/

        //removed in 1.0.66355
        protected virtual int countFoundUnitsAndBuilds()
        {
            ////UnityEngine.Debug.Log("Player.countFoundUnitsAndBuilds");

            //using (new UnityProfileScope("Player.countFoundUnitsAndBuilds"))
            {
                int iCount = 0;

                foreach (int iCityID in getCities())
                {
                    City pLoopCity = game().city(iCityID);
                    if (pLoopCity != null)
                    {
                        for (int iIndex = 0; iIndex < pLoopCity.getBuildCount(); ++iIndex)
                        {
                            CityQueueData pLoopBuild = pLoopCity.getBuildQueueNode(iIndex);

                            if ((pLoopBuild.meBuild == infos().Globals.UNIT_BUILD) && infos().unit((UnitType)(pLoopBuild.miType)).mbFound)
                            {
                                iCount++;
                            }
                        }
                    }

                }

                foreach (int iUnitID in getUnits())
                {
                    Unit pUnit = game().unit(iUnitID);

                    if ((pUnit != null) && pUnit.isAlive())
                    {
                        if (pUnit.info().mbFound)
                        {
                            iCount++;
                        }
                    }
                }

                return iCount;
            }
        }


        //copy-paste START
        //canBuildUnit: lines 17183-17222
        public virtual bool canContinueBuildUnit(UnitType eUnit)
        {
            //UnityEngine.Debug.Log("Player.canContinueBuildUnit");

            if (!canEverBuildUnit(eUnit))
            {
                return false;
            }

            if (!isUnitUnlocked(eUnit))
            {
                return false;
            }

            if (isUnitObsolete(eUnit))
            {
                return false;
            }

/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again      START ###
  ##############################################*/
            //not relevant for continuing
            /*
            if (infos().unit(eUnit).mbFound)
            {
                if (game().getCitySiteLeftCount() == 0)
                {
                    return false;
                }

                if (isCityMaxReached())
                {
                    return false;
                }
            }
            */
/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again        END ###
  ##############################################*/

            if (infos().unit(eUnit).mbCaravan)
            {
                if (game().getNumPlayersInt() == 1)
                {
                    return false;
                }
            }

            return true;
        }
        //copy-paste END

        //lines 17320-17338
        public override bool isImprovementUnlocked(ImprovementType eImprovement, City pCity)
        {
            ////UnityEngine.Debug.Log("Player.isImprovementUnlocked");

            if (pCity == null)
            {
                return base.isImprovementUnlocked(eImprovement, pCity);
            }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ##############################################*/
            else
            {
                return ((BetterAICity)pCity).isImprovementUnlockedInCity(eImprovement);
            }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/
        }


        //canStartImprovement now calls the new isImprovementUnlocked with pCity as argument, so no more override necessary

/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers  START ###
  ##############################################*/
        //lines 18983-19002
        protected override Character addCourtier(CourtierType eType, GenderType eGender, FamilyType eFamily, Player pFromPlayer = null)
        {
            //UnityEngine.Debug.Log("Player.addCourtier");

            RandomStruct pRandom = new RandomStruct(game().getNextCharacterID());

            if (eType == CourtierType.NONE)
            {
                List<CourtierType> randomValidCourtiers = new List<CourtierType>();
                for (CourtierType eLoopCourtier = (CourtierType)0; eLoopCourtier < infos().courtiersNum(); eLoopCourtier++)
                {
                    if (!(((BetterAIInfoCourtier)infos().courtier(eLoopCourtier)).mbNotRandomCourtier))
                    {
                        randomValidCourtiers.Add(eLoopCourtier);
                    }
                }

                //eType = (CourtierType)(game().randomNext((int)infos().courtiersNum()));
                eType = (CourtierType)(pRandom.Next(randomValidCourtiers.Count));
            }

            Character pCharacter = game().createNewCharacter(infos().Globals.COURTIER_AGE, getPlayer(), eGender, NameType.NONE, eFamily, TribeType.NONE, ((pFromPlayer != null) ? pFromPlayer.getNation() : NationType.NONE), null, pRandom.NextSeed());
            pCharacter.setCourtier(eType);
            pCharacter.generateRatingsCourtier(eType);
            pCharacter.assignNickname();

            doEventTrigger(infos().Globals.COURTIER_EVENTTRIGGER, pTriggerSubject: pCharacter);

            pushLogData(() => TextManager.TEXT("TEXT_GAME_COURTIER_JOINED_POPUP", HelpText.buildCharacterLinkVariable(pCharacter, this)), GameLogType.COURTIER, infos().courtier(eType), pCharacter.getID());

            return pCharacter;
        }
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers    END ###
  ##############################################*/

        public override bool addPing(PingData sPing)
        {
            //UnityEngine.Debug.Log("Player.addPing");

            removePing(sPing.miTileID);

            if (sPing.meType == PingType.NONE)
            {
                return false;
            }

            if (game().isMultiplayer() && (infos().ping(sPing.meType).miMaxNumber != -1) && (getPingCount(sPing.meType) >= infos().ping(sPing.meType).miMaxNumber))
            {
                removePing(sPing.meType);
            }

            loadPing(sPing);
            //AI.clearImprovementTotalValues();

            return true;
        }
        protected override bool removePing(PingType ePing)
        {
            //UnityEngine.Debug.Log("Player.removePing");

            int iBestPingTurn = -1;
            int iBestPingTile = -1;

            foreach (PingData zPing in getPings().Values)
            {
                if (zPing.meType == ePing)
                {
                    if (iBestPingTile == -1 || zPing.miTurn < iBestPingTurn)
                    {
                        iBestPingTurn = zPing.miTurn;
                        iBestPingTile = zPing.miTileID;
                    }
                }
            }

            if (iBestPingTile != -1)
            {
                if (removePing(iBestPingTile))
                {
                    //AI.clearImprovementTotalValues();
                    return true;
                }
            }
            return false;
        }

    }
}
