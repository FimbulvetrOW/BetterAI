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
using System.Xml;

namespace BetterAI
{
    public class BetterAICity : City
    {

/*####### Better Old World AI - Base DLL #######
  ### bEnablesGovernor (EffectCity)    START ###
  ##############################################*/
        protected enum BetterAIDirtyType
        {
            FIRST,
            miEnablesGovernorUnlock,
            NUM_TYPES
        };
        [SkipCheckSaveConsistency] protected BitMaskMulti mBetterAIDirtyBits = new BitMaskMulti((int)BetterAIDirtyType.NUM_TYPES);


        protected class BetterAINetworkData : NetworkData
        {
            public int miEnablesGovernorUnlock;

            public BetterAINetworkData(Infos pInfos, Game pGame)
                : base(pInfos, pGame)
            {
                miEnablesGovernorUnlock = 0;
            }
        }
        protected override NetworkData createNetworkData()
        {
            //UnityEngine.Debug.Log("City.createNetworkData");
            return new BetterAINetworkData(infos(), game());
        }


        protected override bool isDirty(Enum eType)
        {
            //UnityEngine.Debug.Log("City.isDirty");
            if (!(eType is BetterAIDirtyType))
            {
                return base.isDirty(eType);
            }
            if (game().IsDirtyOverride)
            {
                return true;
            }
            return mBetterAIDirtyBits.GetBit((int)(BetterAIDirtyType)eType);
        }
        public override bool isAnyDirty()
        {
            //UnityEngine.Debug.Log("City.isAnyDirty");
            if (base.isAnyDirty())
            {
                return true;
            }
            return !mBetterAIDirtyBits.IsEmpty();
        }
        protected override void makeDirty(Enum eType)
        {
            //UnityEngine.Debug.Log("City.makeDirty");
            if (eType is BetterAIDirtyType)
            {
                mBetterAIDirtyBits.SetBit((int)(DirtyType)eType, true);
            }
            else
            {
                base.makeDirty(eType);
            }
        }
        public override void clearDirty()
        {
            //UnityEngine.Debug.Log("City.clearDirty");
            base.clearDirty();
            mBetterAIDirtyBits.Clear();
        }

        public override void dirtyValuesIO(object pStream, bool bCurrent)
        {
            //UnityEngine.Debug.Log("City.dirtyValuesIO");
            base.dirtyValuesIO(pStream, bCurrent);

            BetterAINetworkData data = bCurrent ? (BetterAINetworkData)mpCurrentData : (BetterAINetworkData)mpLastUpdateData;

            if (!game().IsDirtyOverride)
            {
                SimplifyIO.Data(pStream, ref mBetterAIDirtyBits);
            }

            if (isDirty(BetterAIDirtyType.miEnablesGovernorUnlock))
            {
                SimplifyIO.Data(pStream, ref data.miEnablesGovernorUnlock);
            }
        }

        protected virtual int getEnablesGovernorUnlock()
        {
            //UnityEngine.Debug.Log("City.getEnablesGovernorUnlock");
            return ((BetterAINetworkData)mpCurrentData).miEnablesGovernorUnlock;
        }
        public virtual bool isEnablesGovernor(int iExtraUnlock = 0)
        {
            //UnityEngine.Debug.Log("City.isEnablesGovernor");
            return (getEnablesGovernorUnlock() + iExtraUnlock > 0);
        }
        public virtual void changeEnablesGovernorUnlock(int iChange)
        {
            //UnityEngine.Debug.Log("City.changeEnablesGovernorUnlock");
            if (iChange != 0)
            {
                if (mpCurrentData != null)
                {
                    if (mpLastUpdateData != null)
                    {
                        updateLastData(BetterAIDirtyType.miEnablesGovernorUnlock, ((BetterAINetworkData)mpCurrentData).miEnablesGovernorUnlock, ref ((BetterAINetworkData)mpLastUpdateData).miEnablesGovernorUnlock);
                    }
                    ((BetterAINetworkData)mpCurrentData).miEnablesGovernorUnlock += iChange;
                }
                else Debug.Log("mpCurrentData null");
            }
        }
        public override void changeEffectCityCount(EffectCityType eIndex, int iChange)
        {
            //UnityEngine.Debug.Log("City.changeEffectCityCount");
            if (iChange == 0 || eIndex == EffectCityType.NONE)
            {
                return;
            }

            base.changeEffectCityCount(eIndex, iChange);

            if (((BetterAIInfoEffectCity)infos().effectCity(eIndex)).mbEnablesGovernor)
            {
                changeEnablesGovernorUnlock(iChange);
            }
        }

        public override bool canHaveGovenor()
        {
            if (!(game().isCharacters()))
            {
                return false;
            }

            if (!hasPlayer())
            {
                return false;
            }

            if ((player().getNumFamilies() > 0) && !hasFamily())
            {
                return false;
            }

            //return true;
            return isEnablesGovernor();
        }
/*####### Better Old World AI - Base DLL #######
  ### bEnablesGovernor (EffectCity)      END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
        CityBiomeType meCityBiome = (CityBiomeType)1; //default to BIOME_TEMERATE
        protected bool mbTerritoryChanged = true;
        protected bool mbTerrainChanged = false;

        public virtual void updateCityBiome()
        {
            //UnityEngine.Debug.Log("City.updateCityBiome");
            //calculate only on demand
            if (mbTerritoryChanged || mbTerrainChanged)
            {
                calculateCityBiome();
            }
        }

        public virtual CityBiomeType getCityBiome()
        {
            //UnityEngine.Debug.Log("City.getCityBiome");
            updateCityBiome();
            return meCityBiome;
        }

        public virtual void calculateCityBiome()
        {
            //UnityEngine.Debug.Log("City.calculateCityBiome");
            if ((int)((BetterAIInfos)infos()).cityBiomesNum() <= 1)
            {
                meCityBiome = CityBiomeType.NONE;
                return;
            }

            using (var biomeScoreScoped = CollectionCache.GetDictionaryScoped<CityBiomeType, int>())
            {
                Dictionary<CityBiomeType, int> mapBiomeScores = biomeScoreScoped.Value;
                int iBiomeScoreTotal = 0;
                foreach (int iTileID in getTerritoryTiles())
                {
                    if (game().tile(iTileID).impassable())
                    {
                        continue;
                    }

                    BetterAIInfoTerrain pLoopTileTerrainInfo = (BetterAIInfoTerrain)game().tile(iTileID).terrain();
                    for (CityBiomeType eBiome = 0; eBiome < ((BetterAIInfos)infos()).cityBiomesNum(); eBiome++)
                    {
                        int iCount = pLoopTileTerrainInfo.maiBiomePoints[eBiome];
                        mapBiomeScores[eBiome] = mapBiomeScores.GetOrDefault(eBiome, 0) + iCount;
                        iBiomeScoreTotal += iCount;
                    }
                }
                CityBiomeType iBestBiome = ((int)((BetterAIInfos)infos()).cityBiomesNum() >= 2) ? (CityBiomeType)1 : CityBiomeType.NONE; //Default to Temperate
                int iBestBiomeScore = 0;
                for (CityBiomeType eBiome = 0; eBiome < ((BetterAIInfos)infos()).cityBiomesNum(); eBiome++)
                {
                    if (mapBiomeScores[eBiome] > iBestBiomeScore)
                    {
                        iBestBiome = eBiome;
                        iBestBiomeScore = mapBiomeScores[eBiome];
                    }
                }
                meCityBiome = (CityBiomeType)iBestBiome;

                mbTerritoryChanged = false;
                mbTerrainChanged = false;
            }

        }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                START ###
  ##############################################*/
        public virtual bool isDiscontent()
        {
            //UnityEngine.Debug.Log("City.isDiscontent");
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
            if (isAutomated())
            {
                pWriter.WriteElementString("Automated", "");
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

            if (getReligions().Count > 0)
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

            if (getBannedSpreadReligions().Count > 0)
            {
                pWriter.WriteStartElement("BannedReligion");

                foreach (ReligionType eLoopReligion in getBannedSpreadReligions())
                {
                    pWriter.WriteElementString(infos().religion(eLoopReligion).mzType, "");
                }

                pWriter.WriteEndElement();
            }

            if (getAgentPlayers().Count > 0)
            {
                pWriter.WriteStartElement("AgentPlayers");

                foreach (PlayerType eLoopPlayer in getAgentPlayers())
                {
                    pWriter.WriteElementString("Player", ((int)eLoopPlayer).ToStringCached());
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

        //lines 1928-1948
        protected override void setGovernorID(int iNewValue)
        {
            //UnityEngine.Debug.Log("City.setGovernorID");
            if (getGovernorID() != iNewValue)
            {
                Character pOldGovernor = governor();
                Character pNewGovernor = game().character(iNewValue);

                resetGovernorTraitEffectCity(-1);
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
                ((BetterAICharacter)pOldGovernor)?.resetJobTraitEffectPlayer(infos().Globals.GOVERNOR_JOB, -1);
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/
                resetPlayerEffectCity(-1);

                pOldGovernor?.setCityGovernorID(-1);

                updateLastData(DirtyType.miGovernorID, mpCurrentData.miGovernorID, ref mpLastUpdateData.miGovernorID);
                mpCurrentData.miGovernorID = iNewValue;

                pNewGovernor?.setCityGovernorID(getID());

                resetPlayerEffectCity(1);
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
                ((BetterAICharacter)pNewGovernor)?.resetJobTraitEffectPlayer(infos().Globals.GOVERNOR_JOB, 1);
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/
                resetGovernorTraitEffectCity(1);

                pOldGovernor?.updateOpinionPlayer();
                pOldGovernor?.updateFamilyOpinion();
                pNewGovernor?.updateOpinionPlayer();
                pNewGovernor?.updateFamilyOpinion();
                game().updateLeaderOpinionAll();
                pNewGovernor?.player()?.markDirtyGoals();
            }
        }



        public virtual int getImprovementModifierForGovernor(ImprovementType eIndex, Character pGovernor, Dictionary<EffectCityType, int> dEffectCityExtraCounts)
        {
            //UnityEngine.Debug.Log("City.getImprovementModifierForGovernor");
            if (dEffectCityExtraCounts == null || dEffectCityExtraCounts.Count == 0)
            {
                return base.getImprovementModifierForGovernor(eIndex, pGovernor);
            }

            using (var effectCityCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
            {
                int iRate = 0;
                ImprovementClassType eImprovementClass = infos().improvement(eIndex).meClass;

                if (pGovernor == governor())
                {
                    iRate += getImprovementModifier(eIndex);
                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        iRate += getImprovementClassModifier(eImprovementClass);
                    }
                }
                else
                {
                    getEffectCityCountsForGovernor(pGovernor, effectCityCountsScoped.Value);

                    foreach (KeyValuePair<EffectCityType, int> p in effectCityCountsScoped.Value)
                    {
                        iRate += infos().effectCity(p.Key).maiImprovementModifier[eIndex] * p.Value;
                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            iRate += infos().effectCity(p.Key).maiImprovementClassModifier[eImprovementClass] * p.Value;
                        }
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### AI: Improvement Value            START ###
  ##############################################*/
                foreach (KeyValuePair<EffectCityType, int> p in dEffectCityExtraCounts)
                {
                    iRate += infos().effectCity(p.Key).maiImprovementModifier[eIndex] * p.Value;
                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        iRate += infos().effectCity(p.Key).maiImprovementClassModifier[eImprovementClass] * p.Value;
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### AI: Improvement Value              END ###
  ##############################################*/

                return iRate;
            }
        }

        public virtual int getImprovementRiverModifierForGovernor(ImprovementType eIndex, Character pGovernor, Dictionary<EffectCityType, int> dEffectCityExtraCounts)
        {
            //UnityEngine.Debug.Log("City.getImprovementRiverModifierForGovernor");
            if (dEffectCityExtraCounts == null || dEffectCityExtraCounts.Count == 0)
            {
                return base.getImprovementRiverModifierForGovernor(eIndex, pGovernor);
            }

            using (var effectCityCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
            {
                int iRate = 0;
                getEffectCityCountsForGovernor(pGovernor, effectCityCountsScoped.Value);

                foreach (KeyValuePair<EffectCityType, int> p in effectCityCountsScoped.Value)
                {
                    iRate += infos().effectCity(p.Key).maiImprovementRiverModifier[eIndex] * p.Value;
                }

/*####### Better Old World AI - Base DLL #######
  ### AI: Improvement Value            START ###
  ##############################################*/
                foreach (KeyValuePair<EffectCityType, int> p in dEffectCityExtraCounts)
                {
                    iRate += infos().effectCity(p.Key).maiImprovementRiverModifier[eIndex] * p.Value;
                }
/*####### Better Old World AI - Base DLL #######
  ### AI: Improvement Value              END ###
  ##############################################*/

                return iRate;
            }
        }

        //lines 4272-4282
        public override int getYieldTurnsLeft(YieldType eYield)
        {
            //UnityEngine.Debug.Log("City.getYieldTurnsLeft");

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
            //UnityEngine.Debug.Log("City.setYieldProgress");
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
            //UnityEngine.Debug.Log("City.changeYieldProgress");
            if (iChange != 0)
            {
                player()?.changeYieldTotal(eIndex, iChange);

                YieldType eYield = eIndex;

                if (infos().yield(eIndex).meSubtractFromYield != YieldType.NONE)
                {
                    eYield = infos().yield(eIndex).meSubtractFromYield;
                    iChange *= -1;
                }

                if (eYield == infos().Globals.HAPPINESS_YIELD)
                {
                    //instead of: if (getHappinessLevel() < 0)
                    if (isDiscontent())
                    {
                        iChange *= -1;
                    }
                }

                setYieldProgress(eYield, getYieldProgress(eYield) + iChange);
            }
        }

        //lines 4885-4901
        public virtual int getEffectCityImprovementYieldForGovernor(ImprovementType eImprovement, YieldType eYield, Character pGovernor, Dictionary<EffectCityType, int> dEffectCityExtraCounts)
        {
            ////UnityEngine.Debug.Log("City.getEffectCityImprovementYieldForGovernor");
            if (dEffectCityExtraCounts == null || dEffectCityExtraCounts.Count == 0)
            {
                return base.getEffectCityImprovementYieldForGovernor(eImprovement, eYield, pGovernor);
            }

            using (var effectCityCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
            {
                int iRate = 0;
                ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;

                if (pGovernor == governor())
                {
                    iRate += getEffectCityImprovementYield(eImprovement, eYield);
                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        iRate += getEffectCityImprovementClassYield(eImprovementClass, eYield);
                    }
                }
                else
                {
                    getEffectCityCountsForGovernor(pGovernor, effectCityCountsScoped.Value);

                    foreach (KeyValuePair<EffectCityType, int> p in effectCityCountsScoped.Value)
                    {
                        iRate += infos().effectCity(p.Key).maaiImprovementYield[eImprovement, eYield] * p.Value;
                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            iRate += infos().effectCity(p.Key).maaiImprovementClassYield[eImprovementClass, eYield] * p.Value;
                        }
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### AI: Improvement Value            START ###
  ##############################################*/
                foreach (KeyValuePair<EffectCityType, int> p in dEffectCityExtraCounts)
                {
                    iRate += infos().effectCity(p.Key).maaiImprovementYield[eImprovement, eYield] * p.Value;
                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        iRate += infos().effectCity(p.Key).maaiImprovementClassYield[eImprovementClass, eYield] * p.Value;
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### AI: Improvement Value              END ###
  ##############################################*/

                return iRate;
            }
        }

        //wtf this is actually not in use?
        //lines 4959-4975
        public virtual int getEffectCityTerrainYieldForGovernor(TerrainType eTerrain, YieldType eYield, Character pGovernor, Dictionary<EffectCityType, int> dEffectCityExtraCounts)
        {
            //UnityEngine.Debug.Log("City.getEffectCityTerrainYieldForGovernor");
            if (dEffectCityExtraCounts == null || dEffectCityExtraCounts.Count == 0)
            {
                return base.getEffectCityTerrainYieldForGovernor(eTerrain, eYield, pGovernor);
            }

            using (var effectCityCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
            {
                int iRate = 0;
                getEffectCityCountsForGovernor(pGovernor, effectCityCountsScoped.Value);

                foreach (KeyValuePair<EffectCityType, int> p in effectCityCountsScoped.Value)
                {
                    iRate += infos().effectCity(p.Key).maaiTerrainYield[eTerrain, eYield] * p.Value;
                }

/*####### Better Old World AI - Base DLL #######
  ### AI: Improvement Value            START ###
  ##############################################*/
                foreach (KeyValuePair<EffectCityType, int> p in dEffectCityExtraCounts)
                {
                    iRate += infos().effectCity(p.Key).maaiTerrainYield[eTerrain, eYield] * p.Value;
                }
/*####### Better Old World AI - Base DLL #######
  ### AI: Improvement Value              END ###
  ##############################################*/

                return iRate;
            }
        }

        public virtual bool isUnitEffectCityUnlock(EffectCityType eIndex, int iEffectExtra = 0, int iFreeUnlockExtra = 0)
        {
            //UnityEngine.Debug.Log("City.isUnitEffectCityUnlock");
            if (isFreeUnitEffectCityUnlock(eIndex, iFreeUnlockExtra))
            {
                return true;
            }

            if (getEffectCityCount(eIndex) + iEffectExtra > 0)
            {
                return true;
            }

            return false;
        }

        //lines 5505-5530
        public override void setHappinessLevel(int iNewValue)
        {
            //UnityEngine.Debug.Log("City.setHappinessLevel");
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

        public override int getNewHappinessLevel(int iChange)
        {
            //UnityEngine.Debug.Log("City.getNewHappinessLevel");
            int iNewLevel = getHappinessLevel() + iChange;
            if (((BetterAIInfoGlobals)infos().Globals).BAI_DISCONTENT_LEVEL_ZERO == 0)
            {
                // no zero level
                if (getHappinessLevel() > 0 && iNewLevel <= 0)
                {
                    --iNewLevel;
                }
                else if (getHappinessLevel() < 0 && iNewLevel >= 0)
                {
                    ++iNewLevel;
                }
            }

            return iNewLevel;
        }

        //lines 5531-5561
        public override void changeHappinessLevel(int iChange)
        {
            //UnityEngine.Debug.Log("City.changeHappinessLevel");
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

                    if (iChange > 0)
                    {
                        player().incrementLeaderStat(infos().Globals.HAPPINESS_LEVEL_INCREASED_STAT);
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

        public override void loadAgentCharacterID(PlayerType eIndex, int iNewValue)
        {
            //UnityEngine.Debug.Log("City.loadAgentCharacterID");
            if (getAgentCharacterID(eIndex) != iNewValue)
            {
                resetVisibilty(eIndex, -1);

                if (hasAgentCharacter(eIndex))
                {
                    agentCharacter(eIndex).setCityAgentID(-1);
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
                    ((BetterAICharacter)agentCharacter(eIndex)).resetJobTraitEffectPlayer(infos().Globals.AGENT_JOB, -1);
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/
                }

                updateLastData(DirtyType.maiAgentCharacterID, mpCurrentData.maiAgentCharacterID, ref mpLastUpdateData.maiAgentCharacterID);
                mpCurrentData.maiAgentCharacterID[(int)eIndex] = iNewValue;

                if (hasAgentCharacter(eIndex))
                {
                    agentCharacter(eIndex).setCityAgentID(getID());
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
                    ((BetterAICharacter)agentCharacter(eIndex)).resetJobTraitEffectPlayer(infos().Globals.AGENT_JOB, 1);
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/
                }

                resetVisibilty(eIndex, 1);
            }
        }

        //Continue specialist on pillaged: functionality is in base game now.

/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again      START ###
  ##############################################*/
        //copy & paste START
        //lines 6613-6628
        protected override bool verifyBuildUnit(CityQueueData pBuild, bool bHurry = false)
        {
            //UnityEngine.Debug.Log("City.verifyBuildUnit");
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
            //UnityEngine.Debug.Log("City.canContinueBuildUnitCurrent");
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
                    if ((getCulture() >= eCultureObsolete) && (getActiveImprovementCount(eImprovementObsolete) > 0))
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
                    if (getActiveImprovementCount(eImprovementObsolete) > 0)
                    {
                        return false;
                    }
                }
            }

            {
                ReligionType eRequiresReligion = infos().unit(eUnit).meRequiresReligion;

                if (eRequiresReligion != ReligionType.NONE)
                {
                    if (bTestEnabled || !isReligion(eRequiresReligion))
                    {
                        if (!isReligionHolyCity(eRequiresReligion) && (pPlayer.getStateReligion() != eRequiresReligion) && !(pPlayer.isBuildAllReligionsUnlock()) && !(isBuildAnyReligionUnitUnlock(eUnit)))
                        {
                            return false;
                        }
                    }
                }
            }

            if (bTestEnabled)
            {
                ImprovementType eImprovementPrereq = infos().unit(eUnit).meImprovementPrereq;

                if (eImprovementPrereq != ImprovementType.NONE)
                {
                    if (getActiveImprovementCount(eImprovementPrereq) == 0)
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
            //UnityEngine.Debug.Log("City.getNetCityProductionYieldHelper");
            //using var profileScoped = new UnityProfileScope("City.getNetCityProductionYield");

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
            //UnityEngine.Debug.Log("City.canCancelBuildQueue");
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
            //UnityEngine.Debug.Log("City.getBuildDiffWholePositive");
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
            //UnityEngine.Debug.Log("City.moveBuildQueue");
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
            //UnityEngine.Debug.Log("City.addTerritoryTile");
            int iCount = getTerritoryTiles().Count;
            base.addTerritoryTile(iTileID);
            mbTerritoryChanged = mbTerritoryChanged || (iCount != getTerritoryTiles().Count);
        }
        public override void removeTerritoryTile(int iTileID)
        {
            //UnityEngine.Debug.Log("City.removeTerritoryTile");
            int iCount = getTerritoryTiles().Count;
            base.removeTerritoryTile(iTileID);
            mbTerritoryChanged = mbTerritoryChanged || (iCount != getTerritoryTiles().Count);
        }

        public virtual void setTerrainChanged()
        {
            //UnityEngine.Debug.Log("City.setTerrainChanged");
            mbTerrainChanged = true;
        }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/

        //copy-paste START
        //lines 7447-7544
        public override bool doDistantRaid(bool bTest = false)
        {
            //UnityEngine.Debug.Log("City.doDistantRaid");
            //using var profileScope = new UnityProfileScope("City.doDistantRaid");

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

                    for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); ++eLoopUnit)
                    {
                        if (infos().unit(eLoopUnit).mbBarbRaid)
                        {
/*####### Better Old World AI - Base DLL #######
  ### No Raider Ships                  START ###
  ##############################################*/
                            if (!infos().unit(eLoopUnit).mbWater || (pBestTile.isWater() && (((BetterAIInfoGlobals)(infos().Globals)).BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS) == 0))
/*####### Better Old World AI - Base DLL #######
  ### No Raider Ships                    END ###
  ##############################################*/
                            {
                                int iWeight = game().countUnits(x => x.getType() == eLoopUnit) + 1;
                                mapUnitDie.Add((eLoopUnit, iWeight));
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

                    player().addTurnSummary(() => TextManager.TEXT("TEXT_GAME_BOUNDARY_RAID_WARNING", HelpText.buildCityLinkVariable(this, player(), bSelect: false)), TurnLogType.TRIBAL_RAID);
                }
            }

            return true;
        }
        //copy-paste END

/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement       START ###
  ##############################################*/
        //lines 9277-9292, adjusted for adjacent
        public virtual bool canAddImprovementTileNoTerritoryCheck(ImprovementType eImprovement, Tile pTile)
        {
            //UnityEngine.Debug.Log("City.canAddImprovementTileNoTerritoryCheck");
            if (!hasPlayer())
            {
                return false;
            }

            ImprovementType ePing = player().getTileImprovementPing(pTile.getID(), false);
            if (ePing != eImprovement)
            {
                if (pTile.hasResource() && !infos().Helpers.isImprovementResourceValid(eImprovement, pTile.getResource()))
                {
                    return false;
                }

                if (pTile.hasImprovement())
                {
                    return false;
                }
            }

            return player().canStartImprovementOnTile(pTile, eImprovement, bTestEnabled: true, bTestTerritory: false, bTestAdjacent: true, bTestReligion: true, bForceImprovement: true);
        }

        public virtual bool canAddImprovementTileAdjacent(Tile pTile, ImprovementType eImprovement)
        {
            //UnityEngine.Debug.Log("City.canAddImprovementTileAdjacent");
            //using var profileScope = new UnityProfileScope("City.canAddImprovement");

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                Tile pAdjacentTile = pTile.tileAdjacent(eLoopDirection);

                if (pAdjacentTile.cityTerritory() == this && canAddImprovementTile(eImprovement, pAdjacentTile))
                {
                    return true;
                }
            }

            return false;
        }

        //lines 9293-9314, adjusted for adjacent
        protected virtual Tile getBestImprovementTileAdjacent(Tile pTile, ImprovementType eImprovement, Predicate<int> condition)
        {
            //UnityEngine.Debug.Log("City.getBestImprovementTileAdjacent");
            Tile pBestTile = null;
            long iBestValue = long.MinValue;

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                Tile pAdjacentTile = pTile.tileAdjacent(eLoopDirection);

                if (pAdjacentTile.cityTerritory() == this && canAddImprovementTile(eImprovement, pAdjacentTile))
                {
                    if (condition == null || condition(pAdjacentTile.getID()))
                    {
                        long iValue = 0;
                        if (hasPlayer())
                        {
                            iValue = player().AI.improvementValueTile(eImprovement, pAdjacentTile, this, false, false, true);
                        }
                        else if (isTribe())
                        {
                            iValue = game().randomNext(1000) + 1;
                        }

                        if (iValue > iBestValue)
                        {
                            pBestTile = pAdjacentTile;
                            iBestValue = iValue;
                        }
                    }
                }
            }

            return pBestTile;
        }

        //lines 9315-9346, adjusted for adjacent
        public virtual bool addImprovementTileAdjacent(Tile pTile, ImprovementType eImprovement)
        {
            //UnityEngine.Debug.Log("City.addImprovementTileAdjacent");
            Tile pBestTile = getBestImprovementTileAdjacent(pTile, eImprovement, iTileID => player().getTileImprovementPing(iTileID, false) == eImprovement);

            if (pBestTile == null)
            {
                ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;
                if (eImprovementClass != ImprovementClassType.NONE)
                {
                    pBestTile = getBestImprovementTileAdjacent(pTile, eImprovement, iTileID =>
                    {
                        ImprovementType ePingImprovement = player().getTileImprovementPing(iTileID, false);
                        return (ePingImprovement != ImprovementType.NONE && infos().improvement(ePingImprovement).meClass == eImprovementClass);
                    });
                }
            }

            if (pBestTile == null)
            {
                pBestTile = getBestImprovementTileAdjacent(pTile, eImprovement, iTileID => player().getTileImprovementPing(iTileID, true) == infos().Helpers.getGenericImprovementPing());
            }

            if (pBestTile == null)
            {
                pBestTile = getBestImprovementTileAdjacent(pTile, eImprovement, null);
            }

            if (pBestTile != null)
            {
                pBestTile.setImprovementFinished(eImprovement);
                return true;
            }

            return false;
        }

/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement         END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### self-aaiEffectCityYieldRate      START ###
  ##############################################*/
        //lines 10420-10563
        public override int getEffectCityYieldRate(EffectCityType eEffectCity, YieldType eYield, Character pGovernor, bool bComplete = false, Dictionary<EffectCityType, int> mapEffectCityChanges = null, Dictionary<SpecialistType, int> mapSpecialistChanges = null)
        {
            //UnityEngine.Debug.Log("City.getEffectCityYieldRate");
            int iRate = base.getEffectCityYieldRate(eEffectCity, eYield, pGovernor, bComplete, mapEffectCityChanges, mapSpecialistChanges);

            if (bComplete)
            {
                int iSelfRate = infos().effectCity(eEffectCity).maaiEffectCityYieldRate[eEffectCity, eYield];
                if (iSelfRate > 0)
                {
                    int iCount = getEffectCityCount(eEffectCity);
                    if (mapEffectCityChanges != null && mapEffectCityChanges.TryGetValue(eEffectCity, out int iChange))
                    {
                        iCount += iChange;
                    }

                    //this is still counted twice
                    iRate -= (iCount * iSelfRate);
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
        public virtual bool isImprovementUnlockedInCity(ImprovementType eImprovement, bool bTestEnabled = true, bool bTestTech = true, bool bTestCulture = true)
        {
            //UnityEngine.Debug.Log("City.isImprovementUnlockedInCity");

            BetterAIPlayer pOwner = (BetterAIPlayer)player();
            BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            if (pOwner == null || pImprovementInfo == null) return false;
            ImprovementClassType eImprovementClass = pImprovementInfo.meClass;
            bool bPrimaryUnlock = true;

            CultureType eCityCulture = bTestEnabled ? getCulture() : infos().Helpers.getNextCulture(getCulture());

            {
                //primary unlock: class tech + culture
                if (bTestTech)
                {
                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        TechType eTechPrereq = infos().improvementClass(eImprovementClass).meTechPrereq;

                        if (eTechPrereq != TechType.NONE)
                        {
                            if (!pOwner.isTechAcquired(eTechPrereq) && !isVoidTechPrereqUnlock(eImprovementClass))
                            {
                                bPrimaryUnlock = false;
                            }
                        }
                    }
                }

                if (bTestCulture)
                {
                    CultureType eCulturePrereq = pImprovementInfo.meCulturePrereq;
                    if (eCulturePrereq != CultureType.NONE)
                    {
                        if (eCityCulture < eCulturePrereq)
                        {
                            bPrimaryUnlock = false;
                        }
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
                TechType eSecondaryUnlockTechPrereq = pImprovementInfo.meSecondaryUnlockTechPrereq;
                if (eSecondaryUnlockTechPrereq != TechType.NONE)
                {
                    bAnySecondaryPrereqs = true;
                    if (bTestTech && !pOwner.isTechAcquired(eSecondaryUnlockTechPrereq))
                    {
                        bSecondaryUnlock = false;
                    }
                }

                if (bTestCulture)
                {
                    CultureType eSecondaryUnlockCulturePrereq = pImprovementInfo.meSecondaryUnlockCulturePrereq;
                    if (eSecondaryUnlockCulturePrereq != CultureType.NONE)
                    {
                        bAnySecondaryPrereqs = true;
                        if (eCityCulture < eSecondaryUnlockCulturePrereq)
                        {
                            bSecondaryUnlock = false;
                        }
                    }
                }

                int iSecondaryUnlockPopulationPrereq = pImprovementInfo.miSecondaryUnlockPopulationPrereq;
                if (iSecondaryUnlockPopulationPrereq > 0)
                {
                    bAnySecondaryPrereqs = true;
                    if (getPopulation() < iSecondaryUnlockPopulationPrereq)
                    {
                        bSecondaryUnlock = false;
                    }

                }
                EffectCityType eSecondaryUnlockEffectCityPrereq = pImprovementInfo.meSecondaryUnlockEffectCityPrereq;
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
                    FamilyClassType eTertiaryUnlockFamilyClassPrereq = pImprovementInfo.meTertiaryUnlockFamilyClassPrereq;
                    bool bTertiaryUnlockSeatOnly = pImprovementInfo.mbTertiaryUnlockSeatOnly;
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
                    TechType eTertiaryUnlockTechPrereq = pImprovementInfo.meTertiaryUnlockTechPrereq;
                    if (eTertiaryUnlockTechPrereq != TechType.NONE)
                    {
                        bAnyTertiaryPrereqs = true;
                        if (bTestTech && !pOwner.isTechAcquired(eTertiaryUnlockTechPrereq))
                        {
                            bTertiaryUnlock = false;
                        }
                    }

                    if (bTestCulture)
                    {
                        CultureType eTertiaryUnlockCulturePrereq = pImprovementInfo.meTertiaryUnlockCulturePrereq;
                        if (eTertiaryUnlockCulturePrereq != CultureType.NONE)
                        {
                            bAnyTertiaryPrereqs = true;
                            if (eCityCulture < eTertiaryUnlockCulturePrereq)
                            {
                                bTertiaryUnlock = false;
                            }
                        }
                    }

                    EffectCityType eTertiaryUnlockEffectCityPrereq = pImprovementInfo.meTertiaryUnlockEffectCityPrereq;
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
        //public virtual bool canHaveImprovement(ImprovementType eImprovement, City pCity = null, TeamType eTeamTerritory = TeamType.NONE, bool bTestEnabled = true, bool bTestTerritory = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bTestResource = true, bool bUpgradeImprovement = false, bool bForceImprovement = false, bool bTestCulture = true, bool bTestImprovement = true, bool bTestTerrain = true)

        //public virtual bool canCityHaveImprovement(ImprovementType eImprovement, TeamType eTeamTerritory = TeamType.NONE, bool bTestTerritory = true, bool bTestEnabled = true, bool bTestReligion = true, bool bUpgradeImprovement = false, bool bForceImprovement = false)

        public override bool canHaveImprovement(ImprovementType eImprovement)
        {
            return canCityHaveImprovement(eImprovement, eTeamTerritory: TeamType.NONE, bTestEnabled: true, bTestTerritory: true, bTestReligion: true, bForceImprovement: false, bTestCulture: true, bTestImprovement: true);
        }
        public virtual bool canCityHaveImprovement(ImprovementType eImprovement, TeamType eTeamTerritory = TeamType.NONE, bool bTestEnabled = true, bool bTestTerritory = true, bool bTestReligion = true, bool bForceImprovement = false, bool bTestCulture = true, bool bTestImprovement = true)
        {
            //UnityEngine.Debug.Log("City.canCityHaveImprovement");
            if (!bForceImprovement && !isImprovementUnlockedInCity(eImprovement, bTestEnabled, bTestTech: false, bTestCulture: bTestCulture)) //testing without tech
            {
                return false;
            }
            else
            {
                //original City.canHaveImprovement runs only with bTestEnabled and tests for: miMaxFamilyCount, miMaxCultureCount, miMaxCityCount, miMaxPlayerCount
                BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);

                //check new Biome prereq
                CityBiomeType eCityBiomePrereq = pImprovementInfo.meCityBiomePrereq;
                if (eCityBiomePrereq != CityBiomeType.NONE)
                {
                    if (eCityBiomePrereq != getCityBiome())
                    {
                        return false;
                    }
                }

                ImprovementClassType eImprovementClass = pImprovementInfo.meClass;
                //Class city max is now in base game, code moved below
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/

                //following: a lot of base game code (from Tile.canHaveImprovement)
                //basically canHaveImprovement without the culture check. Tech is checked elsewhere
                //city-specific only

                //this is also in canTileHaveImprovement
                {
                    ReligionType eReligionSpread = game().getImprovementReligionSpread(eImprovement);

                    if (eReligionSpread != ReligionType.NONE)
                    {
                        if (infos().religion(eReligionSpread).mbDisabled)
                        {
                            return false;
                        }
                    }
                }

                ReligionType eReligionPrereq = pImprovementInfo.meReligionPrereq;

                if (eReligionPrereq != ReligionType.NONE)
                {
                    if (bTestReligion)
                    {
                        if (!(isReligion(eReligionPrereq)))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!hasPlayer())
                        {
                            return false;
                        }
                        if (player().getReligionCount(eReligionPrereq) == 0 && !hasBuildAnyReligionUnitUnlock())
                        {
                            return false;
                        }
                    }
                }

                if (pImprovementInfo.mbHolyCity)
                {
                    if (eReligionPrereq != ReligionType.NONE)
                    {
                        if (!isReligionHolyCity(eReligionPrereq))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!isReligionHolyCityAny())
                        {
                            return false;
                        }
                    }
                }

                {
                    int iMaxCount = pImprovementInfo.miMaxCityCount;
                    if (iMaxCount > 0)
                    {
                        if ((getImprovementCount(eImprovement) >= iMaxCount))
                        {
                            return false;
                        }
                    }
                }

                if (!bForceImprovement)
                {

                    //instad of culture, check unlock
                    //if (bTestCulture)
                    //{
                    //    CultureType eCulturePrereq = infos().improvement(eImprovement).meCulturePrereq;
                    //
                    //    if (eCulturePrereq != CultureType.NONE)
                    //    {
                    //        if (pCityTerritory == null)
                    //        {
                    //            return false;
                    //        }
                    //
                    //        CultureType eCulture = bTestEnabled ? pCityTerritory.getCulture() : infos().Helpers.getNextCulture(pCityTerritory.getCulture());
                    //        if (eCulture != CultureType.NONE)
                    //        {
                    //            if (infos().Helpers.isCultureHigher(eCulturePrereq, eCulture))
                    //            {
                    //                return false;
                    //            }
                    //        }
                    //    }
                    //}

                    if (bTestImprovement)
                    {
                        ImprovementType eImprovementPrereq = pImprovementInfo.meImprovementPrereq;

                        if (eImprovementPrereq != ImprovementType.NONE)
                        {
                            int iCount = getActiveImprovementCount(eImprovementPrereq);

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
                        EffectCityType eEffectCityPrereq = pImprovementInfo.meEffectCityPrereq;

                        if (eEffectCityPrereq != EffectCityType.NONE)
                        {
                            if (getEffectCityCount(eEffectCityPrereq) == 0)
                            {
                                return false;
                            }
                        }
                    }

                    {
                        if (pImprovementInfo.maeEffectCityAnyPrereq.Count > 0)
                        {
                            bool bFound = false;
                            foreach (EffectCityType eLoopEffectCity in pImprovementInfo.maeEffectCityAnyPrereq)
                            {
                                //if (pCityTerritory != null && pCityTerritory.getEffectCityCount(eLoopEffectCity) > 0)
                                if (getEffectCityCount(eLoopEffectCity) > 0)
                                {
                                    bFound = true;
                                    break;
                                }
                            }

                            if (!bFound)
                            {
                                return false;
                            }
                        }
                    }

                }

                if (bTestTerritory)
                {
                    if (eTeamTerritory != TeamType.NONE)
                    {
                        //partially tile-specificif ((getTeam() != eTeamTerritory) && !(getTeam() == TeamType.NONE && getOwnerTribe() == TribeType.NONE && !infos().improvement(eImprovement).mbTerritoryOnly))
                        if ((getTeam() != eTeamTerritory))
                        {
                            return false;
                        }
                    }
                }

                if (bTestEnabled)
                {
                    //if (!bForceImprovement)
                    //{
                    //    //base game now only has this check in canStartImprovement
                    //    int iRequiresLaws = pImprovementInfo.miPrereqLaws;
                    //    if (iRequiresLaws > 0)
                    //    {
                    //        if (!(hasPlayer()))
                    //        {
                    //            return false;
                    //        }
                    //
                    //        if (player().countActiveLaws() < iRequiresLaws)
                    //        {
                    //            return false;
                    //        }
                    //    }
                    //}

                    if (game().isCharacters() && hasFamily())
                    {
                        int iMaxFamilyCount = pImprovementInfo.miMaxFamilyCount;
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
                        //in base game, NoImprovementClassMax doesn't apply to miMaxCultureCount
                        int iMaxPerCulture = infos().improvementClass(eImprovementClass).miMaxCultureCount;
                        if (iMaxPerCulture > 0)
                        {
                            if (getImprovementClassCount(eImprovementClass) >= (iMaxPerCulture * (int)(getCulture() + getCultureStep() + 1)))
                            {
                                return false;
                            }
                        }
                    }

                    if (eImprovementClass == ImprovementClassType.NONE || !isNoImprovementClassMaxUnlock(eImprovementClass))
                    {
                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            int iMaxCity = infos().improvementClass(eImprovementClass).miMaxCityCount;
                            if (iMaxCity > 0)
                            {
                                if (getImprovementClassCount(eImprovementClass) >= iMaxCity)
                                {
                                    return false;
                                }
                            }
                        }

                        if (hasPlayer())
                        {
                            //if (eImprovementClass == ImprovementClassType.NONE || !isNoImprovementClassMaxUnlock(eImprovementClass))
                            {
                                int iMaxPlayerCount = infos().improvement(eImprovement).miMaxPlayerCount;
                                if (iMaxPlayerCount > 0)
                                {
                                    if (player().getImprovementCount(eImprovement) >= iMaxPlayerCount)
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }

                if (eImprovementClass != ImprovementClassType.NONE)
                {
                    foreach (EffectCityType eLoopEffectCity in infos().improvementClass(eImprovementClass).maeEffectCityDisabled)
                    {
                        if (getEffectCityCount(eLoopEffectCity) > 0)
                        {
                            return false;
                        }
                    }
                }

                return true;
                //end base game code
            }
        }

    }
}
