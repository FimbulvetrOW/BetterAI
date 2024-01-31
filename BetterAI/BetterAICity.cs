﻿using System;
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
using System.Xml;

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
                for (CityBiomeType iBiome = 0; iBiome < ((BetterAIInfos)infos()).cityBiomesNum(); iBiome++)
                {
                    iaBiomeScore[(int)iBiome] += eLoopTileInfoTerrain.maiBiomePoints[iBiome];
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

/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                START ###
  ##############################################*/
        public virtual bool isDiscontent()
        {
            if (((BetterAIInfoGlobals)infos().Globals).BAI_DISCONTENT_LEVEL_ZERO == 2)
            {
                return (getHappinessLevel() < 0);
            }
            else
            {
                return (getHappinessLevel() <= 0);
            }
        }
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                  END ###
  ##############################################*/

        //lines 1056-1418
        //copy-paste START
        public override void writeGameXML(XmlWriter pWriter)
        {
            pWriter.WriteStartElement("City");
            pWriter.WriteAttributeString("ID", getID().ToStringCached());
            pWriter.WriteAttributeString("TileID", getTileID().ToStringCached());
            pWriter.WriteAttributeString("Player", getPlayerInt().ToStringCached());
            pWriter.WriteAttributeString("Family", ((hasFamily()) ? family().mzType : Infos.zTYPE_NONE));
            pWriter.WriteAttributeString("Founded", getFoundedTurn().ToStringCached());

            if (getNameType() != CityNameType.NONE)
            {
                pWriter.WriteElementString("NameType", infos().cityName(getNameType()).mzType);
            }
            if (!string.IsNullOrEmpty(getCustomName()))
            {
                pWriter.WriteElementString("Name", getCustomName());
            }

            if (hasGovernor())
            {
                pWriter.WriteElementString("GovernorID", getGovernorID().ToStringCached());
            }
            if (getGovernorTurn() != -1)
            {
                pWriter.WriteElementString("GovernorTurn", getGovernorTurn().ToStringCached());
            }
            if (getGiftedTurn() != -1)
            {
                pWriter.WriteElementString("GiftedTurn", getGiftedTurn().ToStringCached());
            }
            if (getRaidedTurn() != -1)
            {
                pWriter.WriteElementString("RaidedTurn", getRaidedTurn().ToStringCached());
            }
            if (getCitizens() > 0)
            {
                pWriter.WriteElementString("Citizens", getCitizens().ToStringCached());
            }
            if (getCitizensQueue() > 0)
            {
                pWriter.WriteElementString("CitizensQueue", getCitizensQueue().ToStringCached());
            }
            if (getGrowthCount() > 0)
            {
                pWriter.WriteElementString("GrowthCount", getGrowthCount().ToStringCached());
            }
            if (getDamage() > 0)
            {
                pWriter.WriteElementString("Damage", getDamage().ToStringCached());
            }
            if (getHurryCivicsCount() > 0)
            {
                pWriter.WriteElementString("HurryCivicsCount", getHurryCivicsCount().ToStringCached());
            }
            if (getHurryTrainingCount() > 0)
            {
                pWriter.WriteElementString("HurryTrainingCount", getHurryTrainingCount().ToStringCached());
            }
            if (getHurryMoneyCount() > 0)
            {
                pWriter.WriteElementString("HurryMoneyCount", getHurryMoneyCount().ToStringCached());
            }
            if (getHurryPopulationCount() > 0)
            {
                pWriter.WriteElementString("HurryPopulationCount", getHurryPopulationCount().ToStringCached());
            }
            if (getHurryOrdersCount() > 0)
            {
                pWriter.WriteElementString("HurryOrdersCount", getHurryOrdersCount().ToStringCached());
            }
            if (getBuyTileCount() > 0)
            {
                pWriter.WriteElementString("BuyTileCount", getBuyTileCount().ToStringCached());
            }
            if (getSpecialistProducedCount() > 0)
            {
                pWriter.WriteElementString("SpecialistProducedCount", getSpecialistProducedCount().ToStringCached());
            }
            if (getCaptureTurns() > 0)
            {
                pWriter.WriteElementString("CaptureTurns", getCaptureTurns().ToStringCached());
            }
            if (getAssimilateTurns() > 0)
            {
                pWriter.WriteElementString("AssimilateTurns", getAssimilateTurns().ToStringCached());
            }

            if (isCapturedCapital())
            {
                pWriter.WriteElementString("CapturedCapital", "");
            }
            if (isCapital())
            {
                pWriter.WriteElementString("Capital", "");
            }
            pWriter.WriteElementString("FirstPlayer", ((int)(getFirstPlayer())).ToStringCached());
            pWriter.WriteElementString("LastPlayer", ((int)(getLastPlayer())).ToStringCached());
            if (hasCapturePlayer())
            {
                pWriter.WriteElementString("CapturePlayer", ((int)(getCapturePlayer())).ToStringCached());
            }
            if (isTribe())
            {
                pWriter.WriteElementString("Tribe", infos().tribe(getTribe()).mzType);
            }

            {
                pWriter.WriteStartElement("YieldProgress");

                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                {
                    int iValue = getYieldProgress(eLoopYield);
                    if (iValue != 0)
                    {
                        pWriter.WriteElementString(infos().yield(eLoopYield).mzType, iValue.ToStringCached());
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("YieldOverflow");

                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                {
                    int iValue = getYieldOverflow(eLoopYield);
                    if (iValue != 0)
                    {
                        pWriter.WriteElementString(infos().yield(eLoopYield).mzType, iValue.ToStringCached());
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("UnitProductionCounts");

                for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                {
                    int iValue = getUnitProductionCount(eLoopUnit);
                    if (iValue > 0)
                    {
                        pWriter.WriteElementString(infos().unit(eLoopUnit).mzType, iValue.ToStringCached());
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("ProjectCount");

                for (ProjectType eLoopProject = 0; eLoopProject < infos().projectsNum(); eLoopProject++)
                {
                    int iValue = getProjectCount(eLoopProject);
                    if (iValue > 0)
                    {
                        pWriter.WriteElementString(infos().project(eLoopProject).mzType, iValue.ToStringCached());
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("LuxuryTurn");

                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                {
                    if (isLuxury(eLoopResource))
                    {
                        pWriter.WriteElementString(infos().resource(eLoopResource).mzType, getLuxuryTurn(eLoopResource).ToStringCached());
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("AgentTurn");

                for (PlayerType eLoopPlayer = 0; eLoopPlayer < game().getNumPlayers(); eLoopPlayer++)
                {
                    if (isAgentPlayer(eLoopPlayer))
                    {
                        pWriter.WriteElementString("P" + Constants.TYPE_SPLIT_CHAR + ((int)eLoopPlayer).ToStringCached(), getAgentTurn(eLoopPlayer).ToStringCached());
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("AgentCharacterID");

                for (PlayerType eLoopPlayer = 0; eLoopPlayer < game().getNumPlayers(); eLoopPlayer++)
                {
                    if (hasAgentCharacter(eLoopPlayer))
                    {
                        pWriter.WriteElementString("P" + Constants.TYPE_SPLIT_CHAR + ((int)eLoopPlayer).ToStringCached(), getAgentCharacterID(eLoopPlayer).ToStringCached());
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("AgentTileID");

                for (PlayerType eLoopPlayer = 0; eLoopPlayer < game().getNumPlayers(); eLoopPlayer++)
                {
                    if (hasAgentTile(eLoopPlayer))
                    {
                        pWriter.WriteElementString("P" + Constants.TYPE_SPLIT_CHAR + ((int)eLoopPlayer).ToStringCached(), getAgentTileID(eLoopPlayer).ToStringCached());
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("TeamCultureStep");

                for (TeamType eLoopTeam = 0; eLoopTeam < game().getNumTeams(); eLoopTeam++)
                {
                    int iValue = getTeamCultureStep(eLoopTeam);
                    if (iValue != 0)
                    {
                        pWriter.WriteElementString("T" + Constants.TYPE_SPLIT_CHAR + ((int)eLoopTeam).ToStringCached(), iValue.ToStringCached());
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("TeamHappinessLevel");

                for (TeamType eLoopTeam = 0; eLoopTeam < game().getNumTeams(); eLoopTeam++)
                {
                    int iValue = getTeamHappinessLevel(eLoopTeam);
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                START ###
  ##############################################*/
                    //Alex, please fix this in base game
                    if (iValue != -1)
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                  END ###
  ##############################################*/
                    {
                        pWriter.WriteElementString("T" + Constants.TYPE_SPLIT_CHAR + ((int)eLoopTeam).ToStringCached(), iValue.ToStringCached());
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("YieldLevel");

                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                {
                    int iValue = getYieldLevel(eLoopYield);
                    if (iValue > 0)
                    {
                        pWriter.WriteElementString(infos().yield(eLoopYield).mzType, iValue.ToStringCached());
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("Religion");

                for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); eLoopReligion++)
                {
                    if (isReligion(eLoopReligion))
                    {
                        pWriter.WriteElementString(infos().religion(eLoopReligion).mzType, "");
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("PlayerFamily");

                for (PlayerType eLoopPlayer = 0; eLoopPlayer < game().getNumPlayers(); eLoopPlayer++)
                {
                    FamilyType ePlayerFamily = getPlayerFamily(eLoopPlayer);
                    if (ePlayerFamily != FamilyType.NONE)
                    {
                        pWriter.WriteElementString("P" + Constants.TYPE_SPLIT_CHAR + ((int)eLoopPlayer).ToStringCached(), infos().family(ePlayerFamily).mzType);
                    }
                }

                pWriter.WriteEndElement();
            }

            {
                pWriter.WriteStartElement("TeamCulture");

                for (TeamType eLoopTeam = 0; eLoopTeam < game().getNumTeams(); eLoopTeam++)
                {
                    CultureType eTeamCulture = getTeamCulture(eLoopTeam);
                    if (eTeamCulture != CultureType.NONE)
                    {
                        pWriter.WriteElementString("T" + Constants.TYPE_SPLIT_CHAR + ((int)eLoopTeam).ToStringCached(), infos().culture(eTeamCulture).mzType);
                    }
                }

                pWriter.WriteEndElement();
            }

            if (getEventStoryTurns().Count > 0)
            {
                pWriter.WriteStartElement("EventStoryTurn");

                foreach (KeyValuePair<EventStoryType, int> p in getEventStoryTurns())
                {
                    pWriter.WriteElementString(infos().eventStory(p.Key).mzType, p.Value.ToStringCached());
                }

                pWriter.WriteEndElement();
            }

            if (hasBuild())
            {
                pWriter.WriteStartElement("BuildQueue");

                foreach (CityQueueData pLoopBuild in getBuildQueue())
                {
                    pLoopBuild.writeXML(pWriter, infos());
                }

                pWriter.WriteEndElement();
            }

            if (hasCompletedBuild())
            {
                pWriter.WriteStartElement("CompletedBuild");

                CityQueueData pBuild = getCompletedBuild();
                pBuild.writeXML(pWriter, infos());

                pWriter.WriteEndElement();
            }

            pWriter.WriteEndElement();
        }
        //copy-paste END




        //lines 4272-4282
        public override int getYieldTurnsLeft(YieldType eYield)
        {

            if ((eYield == infos().Globals.HAPPINESS_YIELD) && isDiscontent())

            {
                return infos().Helpers.turnsLeft(getYieldThresholdWhole(eYield), getYieldProgress(eYield), -(calculateCurrentYield(eYield, bTestBuild: true, bAccountForCurrentBuild: true)));
            }
            else
            {
                return infos().Helpers.turnsLeft(getYieldThresholdWhole(eYield), getYieldProgress(eYield), calculateCurrentYield(eYield, bTestBuild: true, bAccountForCurrentBuild: true));
            }
        }

        //lines 4291-4395
        protected override void setYieldProgress(YieldType eIndex, int iNewValue)
        {
            if (eIndex == infos().Globals.HAPPINESS_YIELD)
            {

                if (infos().yield(eIndex).mbFloor)
                {
                    iNewValue = Math.Max(0, iNewValue);
                }

                if (getYieldProgress(eIndex) != iNewValue)
                {
                    MohawkAssert.IsFalse(infos().yield(eIndex).mbGlobal);
                    MohawkAssert.IsTrue(infos().yield(eIndex).meSubtractFromYield == YieldType.NONE);

                    loadYieldProgress(eIndex, iNewValue);

                    int iThreshold = getYieldThreshold(eIndex);

                    if (getYieldProgress(eIndex) >= iThreshold)
                    {

                        if (!(isDiscontent()))
                        {
                            changeHappinessLevel(1);

                            player().pushLogData(() => TextManager.TEXT("TEXT_GAME_CITY_HAPPINESS_CHANGE_LOG_DATA", HelpText.buildSignedTextVariable(1), HelpText.buildCityLinkVariable(this, player())), GameLogType.CITY_EVENT, getTileID());
                        }
                        else
                        {
                            changeHappinessLevel(-1);

                            player().pushLogData(() => TextManager.TEXT("TEXT_GAME_CITY_DISCONTENT_CHANGE_LOG_DATA", HelpText.buildSignedTextVariable(1), HelpText.buildCityLinkVariable(this, player())), GameLogType.CITY_WARNING, getTileID());
                        }

                        setYieldProgress(eIndex, (getYieldProgress(eIndex) - iThreshold));
                    }
                    else if (getYieldProgress(eIndex) < 0)
                    {
                        bool bFlipped = false;


                        if (!(isDiscontent()))
                        {
                            changeHappinessLevel(-1);

                            player().pushLogData(() => TextManager.TEXT("TEXT_GAME_CITY_HAPPINESS_CHANGE_LOG_DATA", HelpText.buildSignedTextVariable(-1), HelpText.buildCityLinkVariable(this, player())), GameLogType.CITY_WARNING, getTileID());


                            if (isDiscontent())

                            {
                                bFlipped = true;
                            }
                        }
                        else
                        {
                            changeHappinessLevel(1);

                            player().pushLogData(() => TextManager.TEXT("TEXT_GAME_CITY_DISCONTENT_CHANGE_LOG_DATA", HelpText.buildSignedTextVariable(-1), HelpText.buildCityLinkVariable(this, player())), GameLogType.CITY_EVENT, getTileID());


                            if (!(isDiscontent()))

                            {
                                bFlipped = true;
                            }
                        }

                        if (bFlipped)
                        {
                            setYieldProgress(eIndex, -(getYieldProgress(eIndex)));
                        }
                        else
                        {
                            setYieldProgress(eIndex, (getYieldThreshold(eIndex) + getYieldProgress(eIndex)));
                        }
                    }
                }
            }
            else
            {
                base.setYieldProgress(eIndex, iNewValue);
            }
        }

        //lines 4400-4422
        public override void changeYieldProgress(YieldType eIndex, int iChange)
        {
            if (iChange != 0)
            {
                YieldType eYield = eIndex;

                if (infos().yield(eIndex).meSubtractFromYield != YieldType.NONE)
                {
                    eYield = infos().yield(eIndex).meSubtractFromYield;
                    iChange *= -1;
                }

                if (eYield == infos().Globals.HAPPINESS_YIELD)
                {


                    if (isDiscontent())
                    {
                        iChange *= -1;
                    }
                }

                setYieldProgress(eYield, getYieldProgress(eYield) + iChange);
            }
        }

        //lines 5505-5530
        public override void setHappinessLevel(int iNewValue)
        {
            if (!hasPlayer())
            {
                return;
            }

            bool bWasDiscontent = isDiscontent();

            int iOldValue = getTeamHappinessLevel(getTeam());
            if (iOldValue != iNewValue)
            {
                loadTeamHappinessLevel(getTeam(), iNewValue);
                updateFamilyOpinion();

                //if (Math.Sign(iOldValue) != Math.Sign(iNewValue))
                if (bWasDiscontent ^ isDiscontent()) //XOR
                {
                    //if (getYieldProgress(infos().Globals.HAPPINESS_YIELD) < 0)
                    //{
                    //    setYieldProgress(infos().Globals.HAPPINESS_YIELD, -(getYieldProgress(infos().Globals.HAPPINESS_YIELD)));
                    //}
                    //else
                    if (getYieldProgress(infos().Globals.HAPPINESS_YIELD) >= 0) //then it must come from a bonus, so one whole threshold should be added
                    {
                        setYieldProgress(infos().Globals.HAPPINESS_YIELD, Math.Max(0, (getYieldThreshold(infos().Globals.HAPPINESS_YIELD) - getYieldProgress(infos().Globals.HAPPINESS_YIELD))));
                    }
                }
            }
        }


        //lines 5531-5561
        public override void changeHappinessLevel(int iChange)
        {
            if (((BetterAIInfoGlobals)infos().Globals).BAI_DISCONTENT_LEVEL_ZERO > 0)
            {
                if (iChange > 0)
                {
                    for (int iI = 0; iI < iChange; iI++)
                    {

                    //if (getHappinessLevel() == -1)
                    //{
                    //    setHappinessLevel(1);
                    //}
                    //else

                        {
                            setHappinessLevel(getHappinessLevel() + 1);
                        }
                    }
                }
                else if (iChange < 0)
                {
                    for (int iI = 0; iI > iChange; iI--)
                    {

                    //if (getHappinessLevel() == 1)
                    //{
                    //    setHappinessLevel(-1);
                    //}
                    //else

                        {
                            setHappinessLevel(getHappinessLevel() - 1);
                        }
                    }
                }
            }
            else
            {
                base.changeHappinessLevel(iChange);
            }
        }
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                  END ###
  ##############################################*/


/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again      START ###
  ##############################################*/
        //copy & paste START
        //lines 6613-6628
        protected override bool verifyBuildUnit(CityQueueData pBuild, bool bHurry = false)
        {
            UnitType eUnit = (UnitType)(pBuild.miType);

            if (pBuild.miProgress > 0)
            {
                return true;
            }

/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again      START ###
  ##############################################*/
            if (!bHurry && !canContinueBuildUnitCurrent(eUnit)) //ignore number limits
/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again        END ###
  ##############################################*/
            {
                return false;
            }

            return true;
        }

        //canBuildUnitCurrent: lines 9929-9306
        public virtual bool canContinueBuildUnitCurrent(UnitType eUnit, bool bTestEnabled = true)
        {
            Player pPlayer = ((hasPlayer()) ? player() : lastPlayer());

/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again      START ###
  ##############################################*/
            if (!(((BetterAIPlayer)pPlayer).canContinueBuildUnit(eUnit))) //ignore number limits
/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again        END ###
  ##############################################*/
            {
                return false;
            }

            {
                EffectCityType eEffectCityPrereq = infos().unit(eUnit).meEffectCityPrereq;

                if (eEffectCityPrereq != EffectCityType.NONE)
                {
                    if (!isFreeUnitEffectCityUnlock(eEffectCityPrereq))
                    {
                        if (getEffectCityCount(eEffectCityPrereq) == 0)
                        {
                            return false;
                        }
                    }
                }
            }

            {
                CultureType eCultureObsolete = infos().unit(eUnit).meCultureObsolete;
                ImprovementType eImprovementObsolete = infos().unit(eUnit).meImprovementObsolete;

                if ((eCultureObsolete != CultureType.NONE) && (eImprovementObsolete != ImprovementType.NONE))
                {
                    if ((getCulture() >= eCultureObsolete) && (getFinishedImprovementCount(eImprovementObsolete) > 0))
                    {
                        return false;
                    }
                }
                else if (eCultureObsolete != CultureType.NONE)
                {
                    if (getCulture() >= eCultureObsolete)
                    {
                        return false;
                    }
                }
                else if (eImprovementObsolete != ImprovementType.NONE)
                {
                    if (getFinishedImprovementCount(eImprovementObsolete) > 0)
                    {
                        return false;
                    }
                }
            }

            {
                ReligionType eRequiresReligion = infos().unit(eUnit).meRequiresReligion;

                if (eRequiresReligion != ReligionType.NONE)
                {
                    if (!isReligionHolyCity(eRequiresReligion) && (pPlayer.getStateReligion() != eRequiresReligion) && !(pPlayer.isBuildAllReligionsUnlock()))
                    {
                        return false;
                    }
                }
            }

            if (bTestEnabled)
            {
                ImprovementType eImprovementPrereq = infos().unit(eUnit).meImprovementPrereq;

                if (eImprovementPrereq != ImprovementType.NONE)
                {
                    if (getFinishedImprovementCount(eImprovementPrereq) == 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        //base game code paste END


        //Hurry changes START

        //lines 6385-6427
        protected override CityProductionYield getNetCityProductionYieldHelper(YieldType eYield)
        {
            using var profileScoped = new UnityProfileScope("City.getNetCityProductionYield");

            CityProductionYield zYield = new CityProductionYield();

            zYield.iRate = calculateCurrentYield(eYield);

            CityQueueData pCurrentBuild = getCurrentBuild();
            if (isYieldBuildCurrent(eYield) && getBuildThreshold(pCurrentBuild) > 0)
            {
                int iTotalRate = zYield.iRate + getYieldOverflow(eYield);
                int iMissingYieldCost = getBuildThreshold(pCurrentBuild) - pCurrentBuild.miProgress;
                int iExtraYield = iTotalRate - iMissingYieldCost;

//slightly restructured
                if (iExtraYield >= 0)
                {
                    if (pCurrentBuild.mbHurried)
                    {
                        zYield.iProduction = iMissingYieldCost;

/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry               START ###
  ##############################################*/
                        if (((BetterAIInfoGlobals)infos().Globals).BAI_HURRY_COST_REDUCED < 3)
                        {
                            zYield.iStockpile = iExtraYield;
                        }
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry                 END ###
  ##############################################*/

                    }
                    else
                    {
                        //int iOverflow = Math.Max(0, iExtraYield); //iExtraYield >= 0
                        zYield.iProductionOverflow = Math.Min(iExtraYield, zYield.iRate);
                        zYield.iStockpileOverflow = iExtraYield - zYield.iProductionOverflow;
                        zYield.iStockpile = zYield.iStockpileOverflow;
                        zYield.iProduction = iTotalRate - zYield.iStockpileOverflow;
                    }
                }
                else
                {
                    zYield.iProduction = iTotalRate;
                }
            }
            else
            {
                zYield.iStockpile = zYield.iRate;
                zYield.iProductionOverflow = getYieldOverflow(eYield);
            }

            return zYield;
        }

        //overriding doPlayerTurn is no longer needed


/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry               START ###
  ##############################################*/

//OK this part causes AI turn hangs
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

        //lines 6356-6365
        public override bool canCancelBuildQueue(CityQueueData pQueueData, int iOldIndex)
        {
            if (base.canCancelBuildQueue(pQueueData, iOldIndex))
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry               START ###
  ##############################################*/
            {
                if (iOldIndex == 0)
                {
                    if (pQueueData.mbHurried && (pQueueData.miProgress > 0) && (((BetterAIInfoGlobals)infos().Globals).BAI_HURRY_COST_REDUCED > 0))
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
        //lines 6164-6166
        public override int getBuildDiffWholePositive(CityQueueData pQueueInfo, bool bIncludeOverflow = true)
        {
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry               START ###
  ##############################################*/
            if (((BetterAIInfoGlobals)infos().Globals).BAI_HURRY_COST_REDUCED > 1)
            {
                int iDiff = getBuildThreshold(pQueueInfo) - getBuildProgress(pQueueInfo, bIncludeOverflow: true);
                if (((BetterAIInfoGlobals)infos().Globals).BAI_HURRY_COST_REDUCED >= 3) iDiff -= getBuildRate(pQueueInfo.meBuild, pQueueInfo.miType);

                return Math.Max(0, iDiff / Constants.YIELDS_MULTIPLIER);
                //return Math.Max(0, (getBuildThreshold(pQueueInfo) - (getBuildProgress(pQueueInfo) + getBuildRate(pQueueInfo.meBuild, pQueueInfo.miType))) / Constants.YIELDS_MULTIPLIER);
            }
            else
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry                 END ###
  ##############################################*/
            {
                return base.getBuildDiffWholePositive(pQueueInfo, bIncludeOverflow: bIncludeOverflow);
            }
        }

        //lines 6375-6539
        public override void moveBuildQueue(int iNewIndex, int iOldIndex)
        {
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry               START ###
  ##############################################*/
            CityQueueData pBuildNodeOld = getBuildQueueNode(iOldIndex);
            if (iNewIndex == -1 && pBuildNodeOld.mbHurried && (pBuildNodeOld.miProgress > 0) && (((BetterAIInfoGlobals)infos().Globals).BAI_HURRY_COST_REDUCED > 0))
            {
                //can't cancel
                return;
            }
/*####### Better Old World AI - Base DLL #######
  ### Altnernative Hurry                 END ###
  ##############################################*/
            else
            {
                base.moveBuildQueue(iNewIndex, iOldIndex);
            }
        }
        //Hurry changes END

/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
        //Sorry, I have no clue how the "Dirty" type stuff works
        //lines 5981-5996 (add & remove)
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

        //copy-paste START
        //lines 7447-7544
        public override bool doDistantRaid(bool bTest = false)
        {
            using var profileScope = new UnityProfileScope("City.doDistantRaid");

            if (!canGetRaided())
            {
                return false;
            }

            Tile pCityTile = tile();

            Tile pBestTile = null;
            int iBestValue = 0;

            using (var tilesScoped = CollectionCache.GetListScoped<int>())
            {
                List<int> liTiles = tilesScoped.Value;
                pCityTile.getTilesInRange(infos().Globals.MAX_CITY_RAID_DIST, liTiles);
                RandomStruct pRandom = new RandomStruct(game().getSeedForId(getID() + game().getCitySiteCount() * game().getTurn()));
                foreach (int iLoopTile in liTiles)
                {
                    Tile pLoopTile = game().tile(iLoopTile);

                    if (canRaidFrom(pLoopTile, true))
                    {
                        if (bTest)
                        {
                            return true;
                        }

                        int iValue = pRandom.Next(1000) + 1;

                        iValue += ((infos().Globals.MAX_CITY_RAID_DIST - pLoopTile.distanceTile(pCityTile)) * 250);

                        if (iValue > iBestValue)
                        {
                            pBestTile = pLoopTile;
                            iBestValue = iValue;
                        }
                    }
                }

                if (pBestTile == null)
                {
                    return false;
                }

                using (var dieMapScoped = CollectionCache.GetListScoped<(UnitType, int)>())
                {
                    List<(UnitType, int)> mapUnitDie = dieMapScoped.Value;

                    int iTotalWeight = 0;
                    for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); ++eLoopUnit)
                    {
                        if (infos().unit(eLoopUnit).mbBarbRaid)
                        {
                            if (!infos().unit(eLoopUnit).mbWater || pBestTile.isWater())
                            {
                                int iWeight = game().countUnits(x => x.getType() == eLoopUnit) + 1;
                                mapUnitDie.Add((eLoopUnit, iWeight));
                                iTotalWeight += iWeight;
                            }
                        }
                    }

/*####### Better Old World AI - Base DLL #######
  ### No Raider Ships                  START ###
  ##############################################*/
                    if (pBestTile.isWater() && (((BetterAIInfoGlobals)(infos().Globals)).BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS) == 0)
/*####### Better Old World AI - Base DLL #######
  ### No Raider Ships                    END ###
  ##############################################*/
                    {
                        for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); ++eLoopUnit)
                        {
                            if (!infos().unit(eLoopUnit).mbBarbRaid && infos().unit(eLoopUnit).mbWater && canBuildUnitPossible(eLoopUnit))
                            {
                                mapUnitDie.Add((eLoopUnit, iTotalWeight));
                            }
                        }
                    }

                    if (mapUnitDie.Count == 0)
                    {
                        return false;
                    }

                    int iCount = pRandom.Next(player().difficulty().miRaidNumCity) + infos().Globals.MIN_DISTANT_RAID_UNITS;
                    for (int iI = 0; iI < iCount; iI++)
                    {
                        UnitType eUnitType = infos().utils().randomDieMap(mapUnitDie, pRandom.NextSeed(), UnitType.NONE);
                        if (eUnitType != UnitType.NONE)
                        {
                            Unit pUnit = game().createUnitNearby(eUnitType, pBestTile, PlayerType.NONE, infos().Globals.RAIDERS_TRIBE);
                            if (pUnit != null)
                            {
                                pUnit.startRaid(getTeam());
                            }
                        }
                    }

                    setRaidedTurn(game().getTurn());

                    player().addTurnSummary(() => TextManager.TEXT("TEXT_GAME_BOUNDARY_RAID_WARNING", HelpText.buildCityLinkVariable(this, player())), TurnLogType.TRIBAL_RAID);
                }
            }

            return true;
        }
//copy-paste END


/*####### Better Old World AI - Base DLL #######
  ### self-aaiEffectCityYieldRate      START ###
  ##############################################*/
        //lines 10420-10563
        public override int getEffectCityYieldRate(EffectCityType eEffectCity, YieldType eYield, Character pGovernor, bool bComplete = false)
        {
            int iRate = base.getEffectCityYieldRate(eEffectCity, eYield, pGovernor, bComplete: bComplete);

            if (bComplete)
            {
                foreach (var p in getCurrentEffectCityCounts())
                {
                    EffectCityType eLoopEffectCity = p.Key;
                    if (eLoopEffectCity == eEffectCity) //this is counted twice
                    {
                        int iCount = p.Value;
                        {
                            iRate -= (iCount * infos().effectCity(eLoopEffectCity).maaiEffectCityYieldRate[eEffectCity, eYield]);
                        }
                    }

                }
            }

            return iRate;
        }
/*####### Better Old World AI - Base DLL #######
  ### self-aaiEffectCityYieldRate        END ###
  ##############################################*/


/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ##############################################*/
        //Player.isImprovementUnlocked: lines 17320-17338
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

        //Tile.canHaveImprovement: lines 4805-5098
        public virtual bool canCityHaveImprovement(ImprovementType eImprovement, ref bool bPrimaryUnlock, TeamType eTeamTerritory = TeamType.NONE, bool bTestTerritory = true, bool bTestEnabled = true, bool bTestReligion = true, bool bUpgradeImprovement = false, bool bForceImprovement = false)
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

                //as of v1.0.69678, Improvement religion checks are changed and mostly also run with bTestReligion = false.
                //This makes sense since bTestReligion is only false when called by PlayerAI.calculateImprovementValueForTile
                if (bTestReligion)
                {
                    if (eReligionPrereq != ReligionType.NONE)
                    {
                        if (!(isReligion(eReligionPrereq)))
                        {
                            return false;
                        }

                        if (eInfoImprovement.mbHolyCity)
                        {
                            if (!(isReligionHolyCity(eReligionPrereq)))
                            {
                                return false;
                            }
                        }
                    }
                }
                else if (eReligionPrereq != ReligionType.NONE)
                {
                    if (!hasPlayer() || player().getReligionCount(eReligionPrereq) == 0)
                    {
                        return false;
                    }

                    if (eInfoImprovement.mbHolyCity)
                    {
                        if (!(isReligionHolyCity(eReligionPrereq)))
                        {
                            return false;
                        }
                    }
                }

                if (eInfoImprovement.mbHolyCity && eReligionPrereq == ReligionType.NONE) //invalid Improvement Info
                {
                    return false;
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
                        FamilyType eFamilyPrereq = infos().improvement(eImprovement).meFamilyPrereq;

                        if (eFamilyPrereq != FamilyType.NONE && game().isCharacters())
                        {
                            if (getFamily() != eFamilyPrereq)
                            {
                                return false;
                            }
                        }
                    }

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
