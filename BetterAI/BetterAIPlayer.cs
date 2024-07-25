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

/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites     START ###
  ##############################################*/
        protected virtual UnitType getCurrentFoundUnitType()
        {
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

                    for (int iIndex = 0; iIndex < pLoopCity.getBuildCount(); ++iIndex)
                    {
                        CityQueueData pLoopBuild = pLoopCity.getBuildQueueNode(iIndex);

                        if ((pLoopBuild.meBuild == infos().Globals.UNIT_BUILD) && infos().unit((UnitType)(pLoopBuild.miType)).mbFound)
                        {
                            return (UnitType)(pLoopBuild.miType);
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
            return (eImprovement != ImprovementType.NONE) && (infos().improvement(eImprovement).mbUrban || infos().improvement(eImprovement).mbSpreadsBorders || isSpreadBordersUnlock(eImprovement) || ((iTileID != -1) && game().tile(iTileID).getFreeSpecialist(eImprovement) != SpecialistType.NONE));
        }

/*####### Better Old World AI - Base DLL #######
  ### Does improvement spread borders    END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Fix: Handling of Regents for           ###
  ### legitimacy from former leaders   START ###
  ##############################################*/
        //lines 10388-10391
        public override int findLeaderIndex(Character pCharacter, bool bIncludeRegents)
        {
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
            return base.isStartCityNumberFlexible() && (((BetterAIInfoGlobals)(infos().Globals)).BAI_PLAYER_MAX_EXTRA_DEVELOPMENT_CITIES_PERCENT > 0);
        }

/*####### Better Old World AI - Base DLL #######
  ### Less development cities variation  END ###
  ##############################################*/


/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again      START ###
  ##############################################*/
        //lines 16282-16285
        public override bool isCityMaxReached()
        {
            return ((getNumCities() + countFoundUnitsAndBuilds()) >= getCityMax());
        }
/*####### Better Old World AI - Base DLL #######
  ### Limit Settler Numbers again        END ###
  ##############################################*/

        //removed in 1.0.66355
        protected virtual int countFoundUnitsAndBuilds()
        {
            //using (new UnityProfileScope("Player.countFoundUnitsAndBuilds"))
            {
                int iCount = 0;

                foreach (int iCityID in getCities())
                {
                    City pLoopCity = game().city(iCityID);

                    for (int iIndex = 0; iIndex < pLoopCity.getBuildCount(); ++iIndex)
                    {
                        CityQueueData pLoopBuild = pLoopCity.getBuildQueueNode(iIndex);

                        if ((pLoopBuild.meBuild == infos().Globals.UNIT_BUILD) && infos().unit((UnitType)(pLoopBuild.miType)).mbFound)
                        {
                            iCount++;
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
        public override bool isImprovementUnlocked(ImprovementType eImprovement)
        {

            if (base.isImprovementUnlocked(eImprovement))
            {
                return true;
            }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ##############################################*/
            else
            {
                BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);
                if (pImprovementInfo.isAnySecondaryPrereq())
                {
                    TechType eTechSecondaryPrereq = pImprovementInfo.meSecondaryUnlockTechPrereq;
                    if (eTechSecondaryPrereq != TechType.NONE)
                    {
                        if (isTechAcquired(eTechSecondaryPrereq))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return true; //secondary unlock without tech
                    }
                }
                if (pImprovementInfo.isAnyTertiaryPrereq())
                {
                    TechType eTechTertiaryPrereq = pImprovementInfo.meTertiaryUnlockTechPrereq;
                    bool bHasTech = true;
                    bool bHasFamily = true;
                    if (eTechTertiaryPrereq != TechType.NONE)
                    {
                        if (!(isTechAcquired(eTechTertiaryPrereq)))
                        {
                            bHasTech = false;
                        }
                    }

                    //check if player has the family
                    FamilyClassType eTertiaryUnlockFamilyClassPrereq = pImprovementInfo.meTertiaryUnlockFamilyClassPrereq;
                    if (eTertiaryUnlockFamilyClassPrereq != FamilyClassType.NONE)
                    {
                        bHasFamily = isFamilyClassStarted(eTertiaryUnlockFamilyClassPrereq);
                    }

                    return (bHasTech && bHasFamily);

                }

                return false;
            }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/
        }

        //lines 17339-17393
        public override bool canStartImprovement(ImprovementType eImprovement, City pCity, bool bTestTech = true, bool bForceImprovement = false)
        {
            if (base.canStartImprovement(eImprovement, pCity, bTestTech, bForceImprovement))
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ##############################################*/
            {
                if (bTestTech && pCity != null && !bForceImprovement)
                {
                    if (base.isImprovementUnlocked(eImprovement))
                    {
                        return true;
                    }
                    else
                    {
                        //when testing tech on a specific tile and unlock is not primary, check city for unlock conditions
                        //check secondary unlock on pTile
                        if (pCity != null)
                        {
                            //does city fulfill secondary prereqs?
                            //tertiary?
                            if (!(((BetterAICity)pCity).isImprovementUnlockedInCity(eImprovement, true, bTestTech)))
                            {
                                return false;
                            }

                            return true;
                        }
                        else
                        {
                            //secondary and tertiary unlocks are city-related
                            return false;
                        }
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/
        }

/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers  START ###
  ##############################################*/
        //lines 18983-19002
        protected override Character addCourtier(CourtierType eType, GenderType eGender, FamilyType eFamily, Player pFromPlayer = null)
        {
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
                eType = (CourtierType)(game().randomNext(randomValidCourtiers.Count));
            }

            return base.addCourtier(eType, eGender, eFamily, pFromPlayer);
        }
/*####### Better Old World AI - Base DLL #######
  ### Additional fields for Courtiers    END ###
  ##############################################*/

        public override bool addPing(PingData sPing)
        {
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
