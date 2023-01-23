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
    public class BetterAICity : City
    {
/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
        CityBiomeType meCityBiome = (CityBiomeType)1; //default to BIOME_TEMERATE
        protected bool mbTerritoryChanged = true;
        protected bool mbTerrainChanged = false;

        //protected bool mbProductionHurried = false;

        public virtual CityBiomeType getCityBiome()
        {
            //calculate only on demand
            if (mbTerritoryChanged || mbTerrainChanged)
            {
                calculateCityBiome();
            }
            return meCityBiome;
        }


        protected virtual void calculateCityBiome()
        {
            int[] iaBiomeScore = new int[(int)((BetterAIInfos)infos()).cityBiomesNum()];
            int iBiomeScoreTotal = 0;
            foreach (int iTileID in getTerritoryTiles())
            {
                if (game().tile(iTileID).impassable())
                {
                    continue;
                }

                BetterAIInfoTerrain eLoopTileInfoTerrain = (BetterAIInfoTerrain)game().tile(iTileID).terrain();
                for (int iBiome = 0; iBiome < (int)((BetterAIInfos)infos()).cityBiomesNum(); iBiome++)
                {
                    iaBiomeScore[iBiome] += eLoopTileInfoTerrain.maiBiomePoints[iBiome];
                    iBiomeScoreTotal += eLoopTileInfoTerrain.maiBiomePoints[iBiome];
                }
            }
            int iBestBiome = 1; //Default to Temperate
            int iBestBiomeScore = 0;
            for (int iBiome = 0; iBiome < (int)((BetterAIInfos)infos()).cityBiomesNum(); iBiome++)
            {
                if (iaBiomeScore[iBiome] > iBestBiomeScore)
                {
                    iBestBiome = iBiome;
                    iBestBiomeScore = iaBiomeScore[iBiome];
                }
            }
            mbTerritoryChanged = false;
            mbTerrainChanged = false;
            meCityBiome = (CityBiomeType)iBestBiome;

        }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/
        //Hurry changes START

        //base game code paste START
        public override void doPlayerTurn(List<int> aiPlayerYieldAmounts)
        {
            MohawkAssert.Assert(hasPlayer());

            if (isDamaged())
            {
                changeDamage(-((infos().Globals.CITY_HEAL_PERCENT_PER_TURN * getHPMax()) / 100));
            }

            if (getAssimilateTurns() > 0)
            {
                changeAssimilateTurns(culture().miAssimilationRate);
            }

            doDistantRaidTurn();

            if (!isEnablesGovernor() && hasGovernor())
            {
                clearGovernor();
            }

            if (hasPlayer() && !player().isFirstTurnProcessing())
            {
                if (!hasBuild())
                {
                    if (isAutoBuild())
                    {
                        player().AI.chooseBuild(this, BuildType.NONE, false);
                    }
                }

                if (!hasBuild())
                {
                    ProjectType eDefaultProject = culture().meDefaultProject;

                    if (eDefaultProject != ProjectType.NONE)
                    {
                        pushBuildProjectFirst(eDefaultProject);
                    }
                }

                clearCompletedBuild();

                using (var yieldListScoped = CollectionCache.GetListScoped<int>())
                {
                    List<int> aiYieldAmounts = yieldListScoped.Value;

                    // save current yields since they are affected by what is in the queue and top build may change
                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                    {
                        aiYieldAmounts.Add(calculateCurrentYield(eLoopYield));
                    }

                    if (hasBuild())
                    {
                        CityQueueData pCurrentBuild = getCurrentBuild();
                        if (getBuildThreshold(pCurrentBuild) == 0)
                        {
                            finishBuild();
                        }
                        else
                        {
                            YieldType eBuildYield = getBuildYieldType(pCurrentBuild);
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry               START ###
  ##############################################*/
                            //if (((mbProductionHurried) || (getBuildThreshold(pCurrentBuild) == pCurrentBuild.miProgress)) && (((BetterAIInfoGlobals)infos().Globals).BAI_HURRY_COST_REDUCED_BY_PRODUCTION == 1))
                            if ((getBuildThreshold(pCurrentBuild) == pCurrentBuild.miProgress) && (((BetterAIInfoGlobals)infos().Globals).BAI_HURRY_COST_REDUCED_BY_PRODUCTION == 1))
                            {
                                setYieldOverflow(eBuildYield, 0);
                                aiYieldAmounts[(int)eBuildYield] = 0;
                                finishBuild();
                                //mbProductionHurried = false;
                            }
                            else
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry                 END ###
  ##############################################*/
                            {
                                int iOldOverflow = getYieldOverflow(eBuildYield);
                                changeCurrentBuildProgress(getBuildRate(pCurrentBuild.meBuild, pCurrentBuild.miType));
                                int iExtraYield = -getBuildDiff(getCurrentBuild());
                                changeCurrentBuildProgress(iOldOverflow);
                                if (iExtraYield >= 0)
                                {
                                    int iNewOverflow = iExtraYield;
                                    if (iOldOverflow > 0 && iNewOverflow > iOldOverflow)
                                    {
                                        iNewOverflow = iOldOverflow;
                                    }
                                    setYieldOverflow(eBuildYield, iNewOverflow);
                                    iExtraYield -= iNewOverflow;
                                    aiYieldAmounts[(int)eBuildYield] = iExtraYield;

                                    finishBuild();
                                }
                                else
                                {
                                    setYieldOverflow(eBuildYield, 0);
                                    aiYieldAmounts[(int)eBuildYield] = 0;
                                }
                            }

                        }
                    }

                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                    {
                        if (!infos().yield(eLoopYield).mbGlobal)
                        {
                            changeYieldProgress(eLoopYield, aiYieldAmounts[(int)eLoopYield]);
                        }
                        else
                        {
                            aiPlayerYieldAmounts[(int)eLoopYield] += aiYieldAmounts[(int)eLoopYield];
                        }
                    }
                }

                decayBuildQueue();
            }

            verifyBuildProjects();
            verifyBuildSpecialists();
            verifyBuildUnits();
        }
        //base game code paste END

/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry               START ###
  ##############################################*/

//OK this shit causes AI turn hangs
        //public override void moveBuildQueue(int iNewIndex, int iOldIndex)
        //{

        //    if ((getCurrentBuild().miProgress > 0) && (getCurrentBuild().miProgress == getBuildThreshold(getCurrentBuild())) && (((BetterAIInfoGlobals)infos().Globals).BAI_HURRY_COST_REDUCED_BY_PRODUCTION == 1)) //getBuildProgress(CityQueueData pData)
        //    {
        //        //mbProductionHurried = true;
        //        if (iOldIndex == 0) return; //can't move or cancel hurried item
        //        if (iNewIndex == 0)
        //        {
        //            iNewIndex = 1;
        //        }
        //    }

        //    base.moveBuildQueue(iNewIndex, iOldIndex);

        //}

/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry                 END ###
  ##############################################*/


        public override bool canCancelBuildQueue(CityQueueData pQueueData, int iOldIndex)
        {
            if (base.canCancelBuildQueue(pQueueData, iOldIndex))
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry               START ###
  ##############################################*/
            {
                if (iOldIndex == 0)
                {
                    if ((getBuildThreshold(pQueueData) == pQueueData.miProgress) && (pQueueData.miProgress > 0) && (((BetterAIInfoGlobals)infos().Globals).BAI_HURRY_COST_REDUCED_BY_PRODUCTION == 1))
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry               END ###
  ##############################################*/
        }


        //now the hurry costs
        //this method is only used for hurry cost, so I can just override it, so I don't have to override all the getHurry<yield> methods
        public override int getBuildDiffWholePositive(CityQueueData pQueueInfo)
        {
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry               START ###
  ##############################################*/
            if (((BetterAIInfoGlobals)infos().Globals).BAI_HURRY_COST_REDUCED_BY_PRODUCTION == 1)
            {
                return Math.Max(0, (getBuildThreshold(pQueueInfo) - (getBuildProgress(pQueueInfo) + getBuildRate(pQueueInfo.meBuild, pQueueInfo.miType))) / Constants.YIELDS_MULTIPLIER);
            }
            else
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry                 END ###
  ##############################################*/
            {
                return base.getBuildDiffWholePositive(pQueueInfo);
            }
        }
        //Hurry changes END


/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
        //Sorry, I have no clue how the "Dirty" type stuff works
        public override void addTerritoryTile(int iTileID)
        {
            int iCount = getTerritoryTiles().Count;
            base.addTerritoryTile(iTileID);
            mbTerritoryChanged = mbTerritoryChanged || (iCount != getTerritoryTiles().Count);
        }
        public override void removeTerritoryTile(int iTileID)
        {
            int iCount = getTerritoryTiles().Count;
            base.removeTerritoryTile(iTileID);
            mbTerritoryChanged = mbTerritoryChanged || (iCount != getTerritoryTiles().Count);
        }
        public virtual void setTerrainChanged()
        {
            mbTerrainChanged = true;
        }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ##############################################*/
        public virtual bool ImprovementUnlocked(ImprovementType eImprovement, ref bool bPrimaryUnlock, bool bTestEnabled = true, bool bTestTech = true)
        {
            BetterAIPlayer pOwner = (BetterAIPlayer)player();
            BetterAIInfoImprovement eInfoImprovement = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            if (pOwner == null || eInfoImprovement == null) return false;
            ImprovementClassType eImprovementClass = eInfoImprovement.meClass;
            bPrimaryUnlock = true;

            {
                //primary unlock: class tech + culture
                if (bTestTech)
                {
                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        TechType eTechPrereq = infos().improvementClass(eImprovementClass).meTechPrereq;

                        if (eTechPrereq != TechType.NONE)
                        {
                            if (!pOwner.isTechAcquired(eTechPrereq))
                            {
                                bPrimaryUnlock = false;
                            }
                        }
                    }
                }

                CultureType eCulturePrereq = eInfoImprovement.meCulturePrereq;
                if (eCulturePrereq != CultureType.NONE)
                {
                    if (( (bTestEnabled) ? (getCulture() < eCulturePrereq) : ((getCulture() + 1) < eCulturePrereq)))
                    {
                        bPrimaryUnlock = false;
                    }
                }
            }

            if (bPrimaryUnlock)
            {
                return true;
            }
            else
            {
                bool bAnySecondaryPrereqs = false;
                bool bSecondaryUnlock = true;
                //secondary unlock: only if at least 1 item not empty
                // tech + culture + pop + effectCity
                TechType eSecondaryUnlockTechPrereq = eInfoImprovement.meSecondaryUnlockTechPrereq;
                if (eSecondaryUnlockTechPrereq != TechType.NONE)
                {
                    bAnySecondaryPrereqs = true;
                    if (bTestTech && !pOwner.isTechAcquired(eSecondaryUnlockTechPrereq))
                    {
                        bSecondaryUnlock = false;
                    }
                }
                CultureType eSecondaryUnlockCulturePrereq = eInfoImprovement.meSecondaryUnlockCulturePrereq;
                if (eSecondaryUnlockCulturePrereq != CultureType.NONE)
                {
                    bAnySecondaryPrereqs = true;
                    if (((bTestEnabled) ? (getCulture() < eSecondaryUnlockCulturePrereq) : ((getCulture() + 1) < eSecondaryUnlockCulturePrereq)))
                    {
                        bSecondaryUnlock = false;
                    }
                }
                int iSecondaryUnlockPopulationPrereq = eInfoImprovement.miSecondaryUnlockPopulationPrereq;
                if (iSecondaryUnlockPopulationPrereq > 0)
                {
                    bAnySecondaryPrereqs = true;
                    if (getPopulation() < iSecondaryUnlockPopulationPrereq)
                    {
                        bSecondaryUnlock = false;
                    }

                }
                EffectCityType eSecondaryUnlockEffectCityPrereq = eInfoImprovement.meSecondaryUnlockEffectCityPrereq;
                if (eSecondaryUnlockEffectCityPrereq != EffectCityType.NONE)
                {
                    bAnySecondaryPrereqs = true;
                    if (getEffectCityCount(eSecondaryUnlockEffectCityPrereq) == 0)
                    {
                        bSecondaryUnlock = false;
                    }
                }
                if (bAnySecondaryPrereqs && bSecondaryUnlock)
                {
                    return true;
                }
                else
                {
                    bool bAnyTertiaryPrereqs = false;
                    bool bTertiaryUnlock = true;
                    //tertiary unlock: only if at least 1 item not empty
                    // family (+ seatOnly) + tech + culture + effectCity
                    FamilyClassType eTertiaryUnlockFamilyClassPrereq = eInfoImprovement.meTertiaryUnlockFamilyClassPrereq;
                    bool bTertiaryUnlockSeatOnly = eInfoImprovement.mbTertiaryUnlockSeatOnly;
                    if (eTertiaryUnlockFamilyClassPrereq != FamilyClassType.NONE)
                    {
                        bAnyTertiaryPrereqs = true;
                        FamilyClassType eCityFamilyClass = getFamilyClass(); //familyClass()?.meType ?? FamilyClassType.NONE;
                        if (eCityFamilyClass != FamilyClassType.NONE)
                        {
                            if (eCityFamilyClass != eTertiaryUnlockFamilyClassPrereq)
                            {
                                bTertiaryUnlock = false;
                            }
                            else
                            {
                                if (bTertiaryUnlockSeatOnly)
                                {
                                    if (!(isFamilySeat()))
                                    {
                                        bTertiaryUnlock = false;
                                    }
                                }
                            }
                        }
                        else
                        {
                            bTertiaryUnlock = false;
                        }
                    }
                    TechType eTertiaryUnlockTechPrereq = eInfoImprovement.meTertiaryUnlockTechPrereq;
                    if (eTertiaryUnlockTechPrereq != TechType.NONE)
                    {
                        bAnyTertiaryPrereqs = true;
                        if (bTestTech && !pOwner.isTechAcquired(eTertiaryUnlockTechPrereq))
                        {
                            bTertiaryUnlock = false;
                        }
                    }
                    CultureType eTertiaryUnlockCulturePrereq = eInfoImprovement.meTertiaryUnlockCulturePrereq;
                    if (eTertiaryUnlockCulturePrereq != CultureType.NONE)
                    {
                        bAnyTertiaryPrereqs = true;
                        if (((bTestEnabled) ? (getCulture() < eTertiaryUnlockCulturePrereq) : ((getCulture() + 1) < eTertiaryUnlockCulturePrereq)))
                        {
                            bTertiaryUnlock = false;
                        }
                    }
                    EffectCityType eTertiaryUnlockEffectCityPrereq = eInfoImprovement.meTertiaryUnlockEffectCityPrereq;
                    if (eTertiaryUnlockEffectCityPrereq != EffectCityType.NONE)
                    {
                        bAnyTertiaryPrereqs = true;
                        if (getEffectCityCount(eTertiaryUnlockEffectCityPrereq) == 0)
                        {
                            bTertiaryUnlock = false;
                        }
                    }
                    if (bAnyTertiaryPrereqs && bTertiaryUnlock)
                    {
                        return true;
                    }

                }
            }

            return false;
        }
        public virtual bool canCityHaveImprovement(ImprovementType eImprovement, ref bool bPrimaryUnlock, TeamType eTeamTerritory = TeamType.NONE, bool bTestEnabled = true, bool bTestReligion = true, bool bUpgradeImprovement = false, bool bForceImprovement = false)
        {
            if (!bForceImprovement && !ImprovementUnlocked(eImprovement, ref bPrimaryUnlock, bTestEnabled, false)) //testing without tech
            {
                return false;
            }
            else
            {
                BetterAIInfoImprovement eInfoImprovement = (BetterAIInfoImprovement)infos().improvement(eImprovement);

                //check new Biome prereq
                CityBiomeType eCityBiomePrereq = eInfoImprovement.meCityBiomePrereq;
                if (eCityBiomePrereq != CityBiomeType.NONE)
                {
                    if (eCityBiomePrereq != getCityBiome())
                    {
                        return false;
                    }
                }
                //check new class maxcity
                ImprovementClassType eImprovementClass = eInfoImprovement.meClass;
                if (eImprovementClass != ImprovementClassType.NONE)
                {
                    int iMaxCityCount = ((BetterAIInfoImprovementClass)infos().improvementClass(eImprovementClass)).miMaxCityCount;
                    if (iMaxCityCount > 0)
                    {
                        if (getImprovementClassCount(eImprovementClass) >= iMaxCityCount)
                        {
                            return false;
                        }
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/

                //now, a lot of base game code (from Tile > canHaveImprovement)
                //basically canHaveImprovement without the culture check. Tech is checked elsewhere
                //city-specific only

                {
                    ReligionType eReligionSpread = eInfoImprovement.meReligionSpread;

                    if (eReligionSpread != ReligionType.NONE)
                    {
                        if (infos().religion(eReligionSpread).mbDisabled)
                        {
                            return false;
                        }
                    }
                }

                ReligionType eReligionPrereq = eInfoImprovement.meReligionPrereq;

                if (bTestReligion)
                {
                    if (eReligionPrereq != ReligionType.NONE)
                    {
                        if (!(isReligion(eReligionPrereq)))
                        {
                            return false;
                        }

                        if (eInfoImprovement.mbHolyCity) //without eReligionPrereq, mbHolyCity is ignored. must use mbHolyCityValid
                        {
                            if (!(isReligionHolyCity(eReligionPrereq)))
                            {
                                return false;
                            }
                        }
                    }

                    if (eInfoImprovement.mbHolyCityValid)
                    {
                        if (isReligionHolyCityAny())
                        {
                            return true;
                        }
                    }
                }

                {
                    int iMaxCount = eInfoImprovement.miMaxCityCount;
                    if (iMaxCount > 0)
                    {
                        if ((getImprovementCount(eImprovement) >= iMaxCount))
                        {
                            return false;
                        }
                    }
                }

                //tile-specific
                //if (!isImprovementValid(eImprovement))
                //{
                //    return false;
                //}

                if (!bForceImprovement)
                {

                    //instad of culture, check unlock
                    //CultureType eCulturePrereq = infos().improvement(eImprovement).meCulturePrereq;
                    //if (eCulturePrereq != CultureType.NONE)
                    //{
                    //    if (((bTestEnabled) ? (getCulture() < eCulturePrereq) : ((getCulture() + 1) < eCulturePrereq)))
                    //    {
                    //        return false;
                    //    }
                    //}

                    {
                        ImprovementType eImprovementPrereq = eInfoImprovement.meImprovementPrereq;

                        if (eImprovementPrereq != ImprovementType.NONE)
                        {
                            //if (pCityTerritory == null)
                            //{
                            //    return false;
                            //}

                            int iCount = getFinishedImprovementCount(eImprovementPrereq);

                            if (iCount == 0)
                            {
                                return false;
                            }

                            //tile-specific
                            //if ((iCount == 1) && !bUpgradeImprovement)
                            //{
                            //    if (getImprovement() == eImprovementPrereq)
                            //    {
                            //        return false;
                            //    }
                            //}
                        }
                    }

                    {
                        EffectCityType eEffectCityPrereq = eInfoImprovement.meEffectCityPrereq;

                        if (eEffectCityPrereq != EffectCityType.NONE)
                        {
                            //if (pCityTerritory == null)
                            //{
                            //    return false;
                            //}

                            if (getEffectCityCount(eEffectCityPrereq) == 0)
                            {
                                return false;
                            }
                        }
                    }
                }

                //tile-specific
                //if (isUrban() && !(infos().improvement(eImprovement).mbUrban))
                //{
                //    return false;
                //}

                if (eTeamTerritory != TeamType.NONE)
                {
                    //partially tile-specificif ((getTeam() != eTeamTerritory) && !(getTeam() == TeamType.NONE && getOwnerTribe() == TribeType.NONE && !infos().improvement(eImprovement).mbTerritoryOnly))
                    if ((getTeam() != eTeamTerritory))
                    {
                        return false;
                    }
                }

                //tile-specific
                //else if (infos().improvement(eImprovement).mbTerritoryOnly)
                //{
                //    if (!hasOwner())
                //    {
                //        return false;
                //    }
                //}

                //tile-specific
                //if (hasImprovementFinished())
                //{
                //    if (improvement().mbPermanent)
                //    {
                //        return false;
                //    }
                //}

                if (eImprovementClass != ImprovementClassType.NONE)
                {
                    //if (pCityTerritory != null)
                    //{
                        foreach (EffectCityType eLoopEffectCity in infos().improvementClass(eImprovementClass).maeEffectCityDisabled)
                        {
                    //      if (pCityTerritory.getEffectCityCount(eLoopEffectCity) > 0)
                            if (getEffectCityCount(eLoopEffectCity) > 0)
                            {
                                return false;
                            }
                        }
                    //}
                }

                //tile-specific
                //if (bTestAdjacent)
                //{
                //    if (infos().improvement(eImprovement).mbRequiresUrban)
                //    {
                //        if (!urbanEligible())
                //        {
                //            return false;
                //        }
                //    }
                //}

                if (bTestEnabled)
                {
                    //ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;

                    //tile-specific. returns true for city center
                    //if (hasCity())
                    //{
                    //    return false;
                    //}

                    //if (bTestAdjacent)
                    //{
                    //    if (bTestReligion && eReligionPrereq != ReligionType.NONE)
                    //    {
                    //        if (infos().improvement(eImprovement).mbNoAdjacentReligion)
                    //        {
                    //            if (adjacentToOtherImprovementReligion(eReligionPrereq))
                    //            {
                    //                return false;
                    //            }
                    //        }
                    //    }

                    //    if (!bUpgradeImprovement)
                    //    {
                    //        ImprovementType eAdjacentImprovementPrereq = infos().improvement(eImprovement).meAdjacentImprovementPrereq;

                    //        if (eAdjacentImprovementPrereq != ImprovementType.NONE)
                    //        {
                    //            if (!adjacentToCityImprovementFinished(eAdjacentImprovementPrereq))
                    //            {
                    //                return false;
                    //            }
                    //        }
                    //    }

                    //    if (!bUpgradeImprovement)
                    //    {
                    //        ImprovementClassType eAdjacentImprovementClassPrereq = infos().improvement(eImprovement).meAdjacentImprovementClassPrereq;

                    //        if (eAdjacentImprovementClassPrereq != ImprovementClassType.NONE)
                    //        {
                    //            if (!adjacentToCityImprovementClassFinished(eAdjacentImprovementClassPrereq))
                    //            {
                    //                return false;
                    //            }
                    //        }
                    //    }

                    //    if (eImprovementClass != ImprovementClassType.NONE)
                    //    {
                    //        if (infos().improvementClass(eImprovementClass).mbNoAdjacent && !notAdjacentToImprovementClass(eImprovementClass))
                    //        {
                    //            return false;
                    //        }
                    //    }
                    //}

                    //if (pCityTerritory != null)
                    {
                        if (!bForceImprovement)
                        {
                            int iRequiresLaws = eInfoImprovement.miPrereqLaws;
                            if (iRequiresLaws > 0)
                            {
                                if (!(hasPlayer()))
                                {
                                    return false;
                                }

                                if (player().countActiveLaws() < iRequiresLaws)
                                {
                                    return false;
                                }
                            }
                        }

                        if (hasFamily())
                        {
                            int iMaxFamilyCount = eInfoImprovement.miMaxFamilyCount;
                            if (iMaxFamilyCount > 0)
                            {
                                if (game().countFamilyImprovements(getFamily(), eImprovement) >= iMaxFamilyCount)
                                {
                                    return false;
                                }
                            }
                        }

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            int iMaxPerCulture = infos().improvementClass(eImprovementClass).miMaxCultureCount;
                            if (iMaxPerCulture > 0)
                            {
                                if (getImprovementClassCount(eImprovementClass) >= (iMaxPerCulture * (int)(getCulture() + getCultureStep() + 1)))
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                return true;

                //end base game code
            }
        }
    }
}
