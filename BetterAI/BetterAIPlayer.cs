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
  ### Limit Settler Numbers again      START ###
  ##############################################*/
        //lines 16082-16085
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
            using (new UnityProfileScope("Player.countFoundUnitsAndBuilds"))
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
        //canBuildUnit: lines 16983-17022
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

        //lines 17120-17138
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
                BetterAIInfoImprovement eInfoImprovement = (BetterAIInfoImprovement)infos().improvement(eImprovement);
                if (eInfoImprovement.isAnySecondaryPrereq())
                {
                    TechType eTechSecondaryPrereq = eInfoImprovement.meSecondaryUnlockTechPrereq;
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
                if (eInfoImprovement.isAnyTertiaryPrereq())
                {
                    TechType eTechTertiaryPrereq = eInfoImprovement.meTertiaryUnlockTechPrereq;
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
                    FamilyClassType eTertiaryUnlockFamilyClassPrereq = eInfoImprovement.meTertiaryUnlockFamilyClassPrereq;
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

        //lines 17139-17203
        public override bool canStartImprovement(ImprovementType eImprovement, Tile pTile, bool bTestTech = true, bool bForceImprovement = false)
        {
            if (base.canStartImprovement(eImprovement, pTile, bTestTech, bForceImprovement))
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ##############################################*/
            {
                if (bTestTech && pTile != null && !bForceImprovement)
                {
                    if (base.isImprovementUnlocked(eImprovement))
                    {
                        return true;
                    }
                    else
                    {
                        //when testing tech on a specific tile and unlock is not primary, check city for unlock conditions
                        //check secondary unlock on pTile
                        BetterAICity pTerritoryCity = (BetterAICity)pTile.cityTerritory();
                        if (pTerritoryCity != null)
                        {
                            //does city fulfill secondary prereqs?
                            //tertiary?
                            bool bPrimaryUnlock = true;
                            if (!(pTerritoryCity.ImprovementUnlocked(eImprovement, ref bPrimaryUnlock, true, bTestTech)))
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
        //lines 18789-18806
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

    }
}
