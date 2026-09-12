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
using BetterAI;
using Random = Mohawk.SystemCore.Random;
using System.Diagnostics;

namespace BetterAI
{
    public class BetterAITile : Tile
    {

/*####### Better Old World AI - Base DLL #######
  ### Better bounce tile search        START ###
  ##############################################*/
        public virtual bool isDistanceCloserToOtherTiles(Tile pStartTile, HashSet<Tile> otherTiles)
        {
            if (otherTiles == null || otherTiles.Count == 0)
            {
                return true;
            }

            int iMinStartTileDistance = int.MaxValue;
            int iMinThisTileDistance = int.MaxValue;
            int iDistance = distanceTile(pStartTile);

            foreach (Tile pOtherTile in otherTiles)
            {
                iMinStartTileDistance = Math.Min(iMinStartTileDistance, pStartTile.distanceTile(pOtherTile));
                iMinThisTileDistance = Math.Min(iMinThisTileDistance, this.distanceTile(pOtherTile));
            }

            //original, to be restored when bounce on founding and city owner change is fixed:
            //bool bCloser = iMinThisTileDistance <= iMinStartTileDistance - (iDistance / 2);
            bool bCloser = iMinThisTileDistance <= iMinStartTileDistance - ((iDistance - 2) / 2) - 1;

            return bCloser;
        }

        public virtual bool isPathShorterToOtherTiles(Tile pStartTile, HashSet<Tile> otherTiles, TeamType eWaterControlTeam, bool bWaterUnit)
        {
            if (otherTiles == null || otherTiles.Count == 0)
            {
                return true;
            }

            int iMinStartTileDistance = int.MaxValue;
            Tile pNearestCityTile = null;

            using (var pathfinderScoped = game().GetAreaPathFinderScoped())
            {
                foreach (Tile pOtherTile in otherTiles)
                {
                    if (game().findPathDistance(pathfinderScoped.Value, pStartTile, pOtherTile, bLandRoute: !bWaterUnit, bWaterRoute: bWaterUnit, eWaterControlTeam: eWaterControlTeam, out int iValue))
                    {
                        if (iValue <= iMinStartTileDistance ||
                            (iValue == iMinStartTileDistance && pStartTile.distanceTile(pOtherTile) < pStartTile.distanceTile(pNearestCityTile)))
                        {
                            pNearestCityTile = pOtherTile;
                            iMinStartTileDistance = iValue;
                        }
                    }
                }

                if (pNearestCityTile != null)
                {
                    game().findPathDistance(pathfinderScoped.Value, this, pNearestCityTile, bLandRoute: !bWaterUnit, bWaterRoute: bWaterUnit, eWaterControlTeam: eWaterControlTeam, out int iDistance);
                    //original, to be restored when bounce on founding and city owner change is fixed:
                    //if (game().findPathDistance(pathfinderScoped.Value, this, pNearestCityTile, true, out int iValue) && iValue - (iDistance / 2) <= iMinStartTileDistance)
                    if (game().findPathDistance(pathfinderScoped.Value, this, pNearestCityTile, bLandRoute: !bWaterUnit, bWaterRoute: bWaterUnit, eWaterControlTeam: eWaterControlTeam, out int iValue) && iValue - ((iDistance - 2) / 2) - 1 <= iMinStartTileDistance)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
/*####### Better Old World AI - Base DLL #######
  ### Better bounce tile search          END ###
  ##############################################*/


/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
        public virtual CityBiomeType getCityBiome(BetterAICity pCity = null)
        {
            if (pCity != null)
            {
                return pCity.getCityBiome();
            }

            BetterAICity pCityTerritory = (BetterAICity)cityTerritory();

            if (pCityTerritory != null)
            {
                return pCityTerritory.getCityBiome();
            }
            else return CityBiomeType.NONE;
        }

        //lines 3077-3121
        public override void setTerrain(TerrainType eNewValue, bool bValidateVegetation = false, bool bBuildAreas = true)
        {
            TerrainType eOldTerrain = getTerrain();
            base.setTerrain(eNewValue, bValidateVegetation: bValidateVegetation, bBuildAreas: bBuildAreas);
            if (eOldTerrain != eNewValue)
            {
                BetterAICity pCityTerritory = (BetterAICity)cityTerritory();
                pCityTerritory?.setTerrainChanged();
            }
        }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement       START ###
  ##############################################*/
        public virtual bool canCityTileAddImprovementAdjacent(BetterAICity pCityTerritory, ImprovementType eImprovement, bool bSourceSpreadsBorders)
        {
            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                Tile pAdjacentTile = tileAdjacent(eLoopDirection);

                if ((pAdjacentTile.cityTerritory() == pCityTerritory || (bSourceSpreadsBorders && pAdjacentTile.cityTerritory() == null)) && pCityTerritory.canAddImprovementTileNoTerritoryCheck(eImprovement, pAdjacentTile))
                {
                    return true;
                }

            }

            return false;
        }

        public virtual bool canCityTileAddImprovementClassAdjacent(BetterAICity pCityTerritory, ImprovementClassType eImprovementClass, bool bSourceSpreadsBorders, out ImprovementType eAddImprovement)
        {
            for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
            {
                if (infos().improvement(eLoopImprovement).meClass == eImprovementClass)
                {
                    if (canCityTileAddImprovementAdjacent(pCityTerritory, eLoopImprovement, bSourceSpreadsBorders))
                    {
                        eAddImprovement = eLoopImprovement;
                        return true;
                    }
                }
            }
            eAddImprovement = ImprovementType.NONE;
            return false;
        }

        //adjacentToPassableLand: lines 3028-3044
        public virtual bool adjacentToPassableLandSameCityWithoutImprovement(bool bSpreadsBorders, ImprovementType eTestImprovement)
        {
            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                Tile pAdjacentTile = tileAdjacent(eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.isLand() && !(pAdjacentTile.impassable())
                        && pAdjacentTile.getImprovement() == ImprovementType.NONE
                        && (!pAdjacentTile.hasResource() || infos().Helpers.isImprovementResourceValid(eTestImprovement, pAdjacentTile.getResource()))
                        && pAdjacentTile.cityTerritory() == cityTerritory() || (bSpreadsBorders && pAdjacentTile.cityTerritory() == null))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement         END ###
  ##############################################*/

        public virtual bool isBorder(TeamType eTeam)
        {
            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                Tile pAdjacentTile = tileAdjacent(eLoopDirection, true);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.getTeam() != eTeam)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public virtual bool adjacentToCityImprovementClassFinished(City pCity, ImprovementClassType eImprovementClass)
        {
            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                Tile pAdjacentTile = tileAdjacent(eLoopDirection);

                if (pAdjacentTile != null)
                {
                    if (pAdjacentTile.getTeam() == pCity.getTeam())
                    {
                        if (pAdjacentTile.getActiveImprovementClass() == eImprovementClass)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public virtual bool canUnownedTileHaveImprovement(ImprovementType eImprovement, TeamType eTeamTerritory = TeamType.NONE, bool bTestTerritory = true, bool bTestEnabled = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bUpgradeImprovement = false, bool bForceImprovement = false)
        {
            BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);

            ReligionType eReligionPrereq = pImprovementInfo.meReligionPrereq;
            if (eReligionPrereq != ReligionType.NONE)
            {
                return false;
            }

            if (pImprovementInfo.mbHolyCity)
            {
                return false;
            }

            if (pImprovementInfo.miMaxCityCount > 0)
            {
                return false;
            }

            if (!bForceImprovement)
            {
                if (pImprovementInfo.meCulturePrereq != CultureType.NONE)
                {
                    return false;
                }

                if (pImprovementInfo.meImprovementPrereq != ImprovementType.NONE)
                {
                    return false;
                }

                if (pImprovementInfo.meFamilyPrereq != FamilyType.NONE && game().isCharacters())
                {
                    return false;
                }

                if (pImprovementInfo.meEffectCityPrereq != EffectCityType.NONE)
                {
                    return false;
                }

                if (pImprovementInfo.maeEffectCityAnyPrereq.Count > 0)
                {
                    return false;
                }

                if (pImprovementInfo.mbNoVegetation && hasVegetation() && vegetation().mbRequiresUnlock)
                {
                    return false;
                }


                if (!bUpgradeImprovement)
                {
                    ImprovementType eAdjacentImprovementPrereq = pImprovementInfo.meAdjacentImprovementPrereq;

                    if (eAdjacentImprovementPrereq != ImprovementType.NONE)
                    {
                        if (!adjacentToImprovementFinished(eAdjacentImprovementPrereq))
                        {
                            return false;
                        }
                    }

                    SpecialistType eAdjacentSpecialistPrereq = pImprovementInfo.meAdjacentSpecialistPrereq;

                    if (eAdjacentSpecialistPrereq != SpecialistType.NONE)
                    {
                        if (!adjacentToSpecialist(eAdjacentSpecialistPrereq))
                        {
                            return false;
                        }
                    }

                    foreach (ImprovementClassType eClassPrereq in pImprovementInfo.maeAdjacentImprovementClassAllPrereq)
                    {
                        if (!adjacentToImprovementClassFinished(eClassPrereq))
                        {
                            return false;
                        }
                    }

                }

                ImprovementClassType eImprovementClass = pImprovementInfo.meClass;
                if (eImprovementClass != ImprovementClassType.NONE)
                {
                    if (infos().improvementClass(eImprovementClass).mbNoAdjacent && !notAdjacentToImprovementClass(eImprovementClass))
                    {
                        return false;
                    }
                }

            }

            if (bTestTerritory)
            {
                if (pImprovementInfo.mbTerritoryOnly)
                {
                    return false;
                }
            }

            return true;
        }

        public virtual bool canCityTileHaveImprovement(BetterAICity pCity, ImprovementType eImprovement, TeamType eTeamTerritory = TeamType.NONE, bool bTestEnabled = true, bool bTestTerritory = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bTestResource = true, bool bUpgradeImprovement = false, bool bForceImprovement = false, bool bTestCulture = true, bool bTestImprovement = true, bool bTestTerrain = true)
        {
            if (!isImprovementValid(eImprovement, pCity, bTestEnabled))
            {
                return false;
            }
            BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            ImprovementClassType eImprovementClass = pImprovementInfo.meClass;

            if (!bForceImprovement)
            {
                ImprovementType eImprovementPrereq = pImprovementInfo.meImprovementPrereq;

                if (eImprovementPrereq != ImprovementType.NONE)
                {
                    //if (pCityTerritory == null)
                    //{
                    //    return false;
                    //}

                    int iCount = pCity.getActiveImprovementCount(eImprovementPrereq);

                    //if (iCount == 0)
                    //{
                    //    return false;
                    //}

                    if ((iCount == 1) && !bUpgradeImprovement)
                    {
                        if (getImprovement() == eImprovementPrereq)
                        {
                            return false;
                        }
                    }
                }
            }

            if (bTestTerrain)
            {

                if (infos().improvement(eImprovement).mbNoVegetation && hasVegetation() && vegetation().mbRequiresUnlock)
                {
                    //if (pCityTerritory == null)
                    //{
                    //    return false;
                    //}
                    if (pCity.hasPlayer() && !pCity.player().isRemoveAllVegetationUnlock())
                    {
                        return false;
                    }
                }
            }

            bool bSkipAdjacentCheck = false;
            bool bDoAdjacentCheck = false;

            if (eImprovementClass != ImprovementClassType.NONE)
            {

                if (infos().improvementClass(eImprovementClass).mbAdjacentValid)
                {
                    if (adjacentToCityImprovementClassFinished(pCity, eImprovementClass))
                    {
                        bSkipAdjacentCheck = true;
                    }
                    else
                    {
                        bDoAdjacentCheck = true;
                    }
                }

                if (infos().improvementClass(eImprovementClass).mbContiguous)
                {
                    if (!adjacentToCityImprovementClassFinished(pCity, eImprovementClass))
                    {
                        for (int iI = 0; iI < game().getNumTiles(); iI++)
                        {
                            Tile pLoopTile = game().tile(iI);

                            if (pLoopTile != null && pLoopTile.getID() != getID() && pLoopTile.hasImprovement())
                            {
                                if (pLoopTile.improvement().meClass == eImprovementClass)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }

            if (bTestAdjacent && !bSkipAdjacentCheck)
            {
                //this has to be in the CityTile part too now because of the bSkipAdjacentCheck part
                if (bDoAdjacentCheck && pImprovementInfo.mbRequiresUrban)
                {
                    if (!urbanEligible())
                    {
                        return false;
                    }
                }

                if (pImprovementInfo.mbRequiresBorder)
                {
                    if (!isBorder(pCity.getTeam()))
                    {
                        return false;
                    }
                }
            }

            if (bTestEnabled)
            {
                //for improvements with mbAdjacentValid, these tests were skipped in General Tile, so they need to be done now
                if (bTestAdjacent && bDoAdjacentCheck)
                {
                    ReligionType eReligionPrereq = pImprovementInfo.meReligionPrereq;

                    if (bTestReligion && eReligionPrereq != ReligionType.NONE)
                    {
                        if (pImprovementInfo.mbNoAdjacentReligion)
                        {
                            if (adjacentToOtherImprovementReligion(eReligionPrereq))
                            {
                                return false;
                            }
                        }
                    }

                    if (!bUpgradeImprovement)
                    {
                        ImprovementType eAdjacentImprovementPrereq = pImprovementInfo.meAdjacentImprovementPrereq;

                        if (eAdjacentImprovementPrereq != ImprovementType.NONE)
                        {
                            if (!adjacentToImprovementFinished(eAdjacentImprovementPrereq))
                            {
                                return false;
                            }
                        }
                    }

                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        if (infos().improvementClass(eImprovementClass).mbNoAdjacent && !notAdjacentToImprovementClass(eImprovementClass))
                        {
                            return false;
                        }
                    }
                }
            }

/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement       START ###
  ##############################################*/

            //if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE || pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
            //{
            //    UnityEngine.Debug.Log("BonusAdjacentImprovement: canCityTileHaveImprovement start");
            //}
            bool bImprovementSpreadsBorders = improvementSpreadsBorders(eImprovement, (pCity != null) ? pCity.getPlayer() : PlayerType.NONE);

            if (!(pImprovementInfo.mbMakesAdjacentPassableLandTileValidForBonusImprovement))
            {
                if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE)
                {
                    if (!canCityTileAddImprovementAdjacent(pCity, pImprovementInfo.meBonusAdjacentImprovement, bImprovementSpreadsBorders))
                    {
                        //UnityEngine.Debug.Log("BonusAdjacentImprovement: canCityTileHaveImprovement end 1");
                        return false;
                    }
                }
                else if (pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                {
                    if (!canCityTileAddImprovementClassAdjacent(pCity, pImprovementInfo.meBonusAdjacentImprovementClass, bImprovementSpreadsBorders, out _))
                    {
                        //UnityEngine.Debug.Log("BonusAdjacentImprovement: canCityTileHaveImprovement end 2");
                        return false;
                    }
                }
            }
            else
            {
                ImprovementType eTestImprovement = ImprovementType.NONE;
                if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE)
                {
                    if (pCity.player() == null || pCity.player().canStartImprovement(pImprovementInfo.meBonusAdjacentImprovement, pCity, bTestTech: false, bForceImprovement: true) || !pCity.canCityHaveImprovement(pImprovementInfo.meBonusAdjacentImprovement, eTeamTerritory, bTestTerritory: bTestTerritory, bTestReligion: bTestReligion, bForceImprovement: true))
                    {
                        //UnityEngine.Debug.Log("BonusAdjacentImprovement: canCityTileHaveImprovement end 3");
                        return false;
                    }
                    eTestImprovement = pImprovementInfo.meBonusAdjacentImprovement;
                }
                else if (pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                {
                    bool bFound = false;

                    for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                    {
                        if (infos().improvement(eLoopImprovement).meClass == pImprovementInfo.meBonusAdjacentImprovementClass)
                        {
                            if (pCity.player() != null && pCity.player().canStartImprovement(eLoopImprovement, pCity, bTestTech: false, bForceImprovement: true) && pCity.canCityHaveImprovement(eImprovement, eTeamTerritory, bTestTerritory: bTestTerritory, bTestReligion: bTestReligion, bForceImprovement: true))
                            {
                                bFound = true;
                                eTestImprovement = eLoopImprovement;
                                break;
                            }
                        }
                    }

                    if (!bFound)
                    {
                        //UnityEngine.Debug.Log("BonusAdjacentImprovement: canCityTileHaveImprovement end 4");
                        return false;
                    }
                }

                if (!adjacentToPassableLandSameCityWithoutImprovement(bImprovementSpreadsBorders, eTestImprovement))
                {
                    //UnityEngine.Debug.Log("BonusAdjacentImprovement: canCityTileHaveImprovement end 5");
                    return false;
                }
            }

            //if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE || pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
            //{
            //    UnityEngine.Debug.Log("BonusAdjacentImprovement: canCityTileHaveImprovement end 6");
            //}
/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement         END ###
  ##############################################*/

            return true;
        }

        //canHaveImprovement: lines 4805-5098
        public virtual bool canGeneralTileHaveImprovement(ImprovementType eImprovement, TeamType eTeamTerritory = TeamType.NONE, bool bTestEnabled = true, bool bTestTerritory = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bTestResource = true, bool bUpgradeImprovement = false, bool bForceImprovement = false, bool bTestCulture = true, bool bTestImprovement = true, bool bTestTerrain = true)
        {
            if (getTerrain() == TerrainType.NONE)
            {
                return false;
            }

            if (impassable())
            {
                return false;
            }

            //we need this part after all
            if (getCityTerritory() == -1 && !isImprovementValid(eImprovement, null, bTestEnabled: bTestEnabled, bTestResource: bTestResource, bTestTerrain: bTestTerrain))
            {
                return false;
            }

            BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);

            if (!bForceImprovement && getCityTerritory() == -1)

            if (isUrban() && !(pImprovementInfo.mbUrban))
            {
                return false;
            }

            if (bTestTerritory)
            {
                if (pImprovementInfo.mbTerritoryOnly)
                {
                    if (!hasCityTerritory())
                    {
                        return false;
                    }

                    if (eTeamTerritory != TeamType.NONE)
                    {
                        if (getTeam() != eTeamTerritory)
                        {
                            return false;
                        }
                    }
                }
            }

            ReligionType eReligionPrereq = pImprovementInfo.meReligionPrereq;
            if (!bTestReligion && eReligionPrereq != ReligionType.NONE)
            {
                if (!hasCityTerritory())
                {
                    return false;
                }
            }

            if (hasActiveImprovement())
            {
                if (improvement().mbPermanent)
                {
                    return false;
                }
            }

            ImprovementClassType eImprovementClass = pImprovementInfo.meClass;
            bool bSkipAdjacentCheck = false;


            if (eImprovementClass != ImprovementClassType.NONE)
            {
                if (infos().improvementClass(eImprovementClass).mbAdjacentValid)
                {
                    //mbAdjacentValid: this needs to be checked in CityTile part
                    //if (adjacentToCityImprovementClassFinished(eImprovementClass))
                    {
                        bSkipAdjacentCheck = true;
                    }
                }
            }

            if (bTestAdjacent && !bSkipAdjacentCheck)
            {
                if (pImprovementInfo.mbRequiresUrban)
                {
                    if (!urbanEligible())
                    {
                        return false;
                    }
                }
            }

            if (bTestEnabled)
            {
                if (hasCity())
                {
                    return false;
                }

                if (bTestAdjacent && !bSkipAdjacentCheck)
                {

                    if (!canHaveImprovementAdjacentTest(eImprovement, bTestReligion, bUpgradeImprovement))
                    {
                        return false;
                    }

                    //if (bTestReligion && eReligionPrereq != ReligionType.NONE)
                    //{
                    //    if (pImprovementInfo.mbNoAdjacentReligion)
                    //    {
                    //        if (adjacentToOtherImprovementReligion(eReligionPrereq))
                    //        {
                    //            return false;
                    //        }
                    //    }
                    //}

                    //if (!bUpgradeImprovement)
                    //{
                    //    ImprovementType eAdjacentImprovementPrereq = pImprovementInfo.meAdjacentImprovementPrereq;

                    //    if (eAdjacentImprovementPrereq != ImprovementType.NONE)
                    //    {
                    //        if (!adjacentToImprovementFinished(eAdjacentImprovementPrereq))
                    //        {
                    //            return false;
                    //        }
                    //    }
                    //}

                    //if (eImprovementClass != ImprovementClassType.NONE)
                    //{
                    //    if (infos().improvementClass(eImprovementClass).mbNoAdjacent && !notAdjacentToImprovementClass(eImprovementClass))
                    //    {
                    //        return false;
                    //    }
                    //}
                }
            }
            return true;
        }

        //lines 4805-5098
        //old vs new: + bTestResource + TestCulture + bTestImprovement + bTestTerrain
        //public override bool canHaveImprovement(ImprovementType eImprovement, City pCity = null, TeamType eTeamTerritory = TeamType.NONE, bool bTestTerritory = true, bool bTestEnabled = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bUpgradeImprovement = false, bool bForceImprovement = false)

        public override bool canHaveImprovement(ImprovementType eImprovement, City pCity = null, TeamType eTeamTerritory = TeamType.NONE, bool bTestEnabled = true, bool bTestTerritory = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bTestResource = true, bool bUpgradeImprovement = false, bool bForceImprovement = false, bool bTestCulture = true, bool bTestImprovement = true, bool bTestTerrain = true)
        {
            //split into 3:
            //tile.canGeneralTileHaveImprovement (not tied to city)
            //city.canCityHaveImprovement (tied only to city)
            //tile.canCityTileHaveImprovement (tied to city and tile)

/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ##############################################*/
            if (!(canGeneralTileHaveImprovement(eImprovement, eTeamTerritory, bTestTerritory, bTestEnabled, bTestAdjacent, bTestReligion, bUpgradeImprovement, bForceImprovement)))
            {
                return false;
            }

            //BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            BetterAICity pCityTerritory = (BetterAICity)pCity ?? (BetterAICity)cityTerritory();
            if (pCityTerritory != null)
            {
                if (!(pCityTerritory.canCityHaveImprovement(eImprovement, eTeamTerritory, bTestEnabled: bTestEnabled, bTestTerritory: bTestTerritory, bTestReligion: bTestReligion, 
                    bForceImprovement: bForceImprovement, bTestCulture: bTestCulture, bTestImprovement: bTestImprovement)))
                {
                    return false;
                }

                if (!(canCityTileHaveImprovement(pCityTerritory, eImprovement, eTeamTerritory, bTestEnabled: bTestEnabled, bTestTerritory: bTestTerritory, 
                    bTestAdjacent: bTestAdjacent, bTestReligion: bTestReligion, bTestResource: bTestResource, bUpgradeImprovement: bUpgradeImprovement, 
                    bForceImprovement: bForceImprovement, bTestCulture: bTestCulture, bTestImprovement: bTestImprovement, bTestTerrain: bTestTerrain)))
                {
                    return false;
                }
            }
            else
            {
                if (!canUnownedTileHaveImprovement(eImprovement, eTeamTerritory, bTestTerritory, bTestEnabled, bTestAdjacent, bTestReligion, bUpgradeImprovement, bForceImprovement))
                {
                    return false;
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/

            return true;
        }

        //lines 4991-
        public override bool isImprovementValid(ImprovementType eImprovement, City pCityTerritory, bool bTestEnabled = true, bool bTestVegetationGrow = false, bool bTestResource = true, bool bFutureCity = false, bool bTestTerrain = true)
        {
            BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);

            if (!(base.isImprovementValid(eImprovement, pCityTerritory, bTestEnabled: bTestEnabled, bTestVegetationGrow: bTestVegetationGrow, bTestResource: bTestResource, bFutureCity: bFutureCity, bTestTerrain: bTestTerrain)))
            {
                return false;
            }

/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement       START ###
  ##############################################*/

            {
                //if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE || pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                //{
                //    UnityEngine.Debug.Log("BonusAdjacentImprovement: isImprovementValid start");
                //}

                //check if the BonusAdjacentImprovements are also valid
                if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE)
                {
                    bool bFound = false;

                    for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                    {
                        Tile pAdjacentTile = tileAdjacent(eLoopDirection);
                        if (pAdjacentTile != null)
                        {
                            if (pAdjacentTile.cityTerritory() == pCityTerritory || pAdjacentTile.cityTerritory() == null)
                            {
                                if (pAdjacentTile.isImprovementValid(pImprovementInfo.meBonusAdjacentImprovement, pCityTerritory, bTestEnabled: bTestEnabled, bTestVegetationGrow: bTestVegetationGrow, bTestResource: bTestResource, bFutureCity: bFutureCity, bTestTerrain: bTestTerrain))
                                {
                                    bFound = true;
                                    break;
                                }
                            }
                        }

                    }

                    if (!bFound)
                    {
                        return false;
                    }
                }

                if (pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                {
                    bool bFound = false;
                    for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                    {
                        if (infos().improvement(eLoopImprovement).meClass == pImprovementInfo.meBonusAdjacentImprovementClass)
                        {
                            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                            {
                                Tile pAdjacentTile = tileAdjacent(eLoopDirection);
                                if (pAdjacentTile != null)
                                {
                                    if (pAdjacentTile.cityTerritory() == pCityTerritory || pAdjacentTile.cityTerritory() == null)
                                    {
                                        if (pAdjacentTile.isImprovementValid(eLoopImprovement, pCityTerritory, bTestEnabled: bTestEnabled, bTestVegetationGrow: bTestVegetationGrow, bTestResource: bTestResource, bFutureCity: bFutureCity, bTestTerrain: bTestTerrain))
                                        {
                                            bFound = true;
                                            break;
                                        }
                                    }
                                }
                            }
                            if (bFound) break;
                        }
                    }

                    if (!bFound)
                    {
                        return false;
                    }
                }

                //if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE || pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                //{
                //    UnityEngine.Debug.Log("BonusAdjacentImprovement: isImprovementValid end");
                //}
            }
/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement         END ###
  ##############################################*/

            return true;
        }

/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement       START ###
  ##############################################*/
        public virtual bool placeUnownedAdjacentImprovement(ImprovementType eImprovement)
        {
            City pCityTerritory = cityTerritory();
            int iOffset = game().randomNext((int)DirectionType.NUM_TYPES); //0-5
            int iStep = iOffset / ((int)DirectionType.NUM_TYPES / 2);  //0 or 1
            iStep = (2 * iStep) - 1;  //1 or -1

            for (int eLoopDirection = 0; eLoopDirection < (int)DirectionType.NUM_TYPES / 2; eLoopDirection += iStep)
            {
                for (int eFlipDirection = 0; eFlipDirection <= (int)DirectionType.NUM_TYPES / 2; eFlipDirection += (int)DirectionType.NUM_TYPES / 2)
                {
                    Tile pAdjacentTile = tileAdjacent((DirectionType)((eLoopDirection + (iOffset / 2) + eFlipDirection) % (int)DirectionType.NUM_TYPES));

                    if (pAdjacentTile.cityTerritory() == pCityTerritory && pAdjacentTile.canHaveImprovement(eImprovement))
                    {
                        pAdjacentTile.setImprovementFinished(eImprovement);
                        return true;
                    }
                }
            }

            return false;
        }


        public override bool isImprovementBorderSpread(ImprovementType eImprovement)
        {
            return ((BetterAIInfoHelpers)(infos().Helpers)).improvementSpreadsBorders(eImprovement, game(), owner(), this);
        }
        public override bool hasImprovementWithBorderSpread()
        {
            return ((BetterAIInfoHelpers)(infos().Helpers)).improvementSpreadsBorders(getImprovement(), game(), owner(), this);
        }
        public virtual bool improvementSpreadsBorders(ImprovementType eImprovement, PlayerType ePlayer)
        {
            return ((BetterAIInfoHelpers)(infos().Helpers)).improvementSpreadsBorders(eImprovement, game(), game().player(ePlayer), this);
        }
/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement         END ###
  ##############################################*/

        //lines 5588-5784
        public override void doImprovementFinished()
        {
            //START base.doImprovementFinished()
            ImprovementClassType eImprovementClass = getImprovementClass();
            City pCityTerritory = cityTerritory();

            if (hasImprovementWithBorderSpread())
            {
                spreadBorders();
            }

            resetImprovementUnitTurns();

            if (improvement().mbUrban)
            {
                using var tileTextScoped = CollectionCache.GetListScoped<TileText>();
                List<TileText> azTileTexts = tileTextScoped.Value;
                makeUrban(getOwner(), azTileTexts);
                foreach (TileText tileText in azTileTexts)
                {
                    game().sendTileText(tileText);
                }
            }

            if (improvement().mbRoadFree)
            {
                setRoad(true);
            }

            if (improvement().mbWonder)
            {
                for (TeamType eLoopTeam = 0; eLoopTeam < game().getNumTeams(); eLoopTeam++)
                {
                    doTileReveal(eLoopTeam, PlayerType.NONE);

                    if (hasCityTerritory())
                    {
                        cityTerritory().tile().doTileReveal(eLoopTeam, PlayerType.NONE);
                    }
                }
            }

            {
                ImprovementType eDevelopImprovement = game().getDevelopImprovement(getImprovement());

                if (eDevelopImprovement != ImprovementType.NONE)
                {
                    if ((pCityTerritory != null) && (eImprovementClass != ImprovementClassType.NONE))
                    {
                        changeImprovementDevelopTurns(pCityTerritory.getImprovementClassDevelopChange(eImprovementClass));
                        updateDevelopImprovement();
                    }

                    {
                        int iRand = improvement().miDevelopRand;

                        if (improvement().mbTribe)
                        {
                            iRand = infos().utils().modify(iRand, game().tribeLevel().miImprovementDevelopModifier);
                        }

                        if (iRand > 0)
                        {
                            changeImprovementDevelopTurns(game().randomNext(iRand));
                            updateDevelopImprovement();
                        }
                    }
                }
            }

            if (improvement().mbTribe)
            {
                TribeType eTribe = getTribeSite();

                if (eTribe == TribeType.NONE)
                {
                    eTribe = game().findNearestSettlementTribe(this, 12);
                }

                if (eTribe == TribeType.NONE)
                {
                    eTribe = infos().Globals.BARBARIANS_TRIBE;
                }

                if (eTribe != TribeType.NONE)
                {
                    UnitType eUnitDefendNew = getImprovementUnitDefend(getImprovement());

                    if (eUnitDefendNew != UnitType.NONE)
                    {
                        bounceUnits();

                        addImprovementUnit(eUnitDefendNew);
                    }
                }
            }

            if (hasOwner())
            {
                {
                    BonusType eBonus = improvement().meBonus;

                    if (eBonus != BonusType.NONE)
                    {
                        owner().doBonus(eBonus, pTile: this, logPrefix: () => HelpText.buildImprovementLinkVariable(getImprovement(), game(), this));
                    }
                }

                {
                    BonusType eBonusCities = improvement().meBonusCities;

                    if (eBonusCities != BonusType.NONE)
                    {
                        foreach (int iLoopCity in owner().getCities())
                        {
/*####### Better Old World AI - Base DLL #######
  ### Do Bonus only in valid cities    START ###
  ##############################################*/
                            //do only valid bonuses
                            City pLoopCity = game().city(iLoopCity);
                            if (pLoopCity != null)
                            {
                                if (owner().canDoBonus(eBonusCities, pCity: pLoopCity))
/*####### Better Old World AI - Base DLL #######
  ### Do Bonus only in valid cities      END ###
  ##############################################*/
                                {
                                    owner().doBonus(eBonusCities, pCity: pLoopCity, logPrefix: () => HelpText.buildImprovementLinkVariable(getImprovement(), game(), this));
                                }
                            }

                        }
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### Extra Bonus for all cities       START ###
  ##############################################*/
                //{
                //    BonusType eBonusCities = ((BetterAIInfoImprovement)improvement()).meBonusCitiesExtra;

                //    if (eBonusCities != BonusType.NONE)
                //    {
                //        foreach (int iLoopCity in owner().getCities())
                //        {

                //            //do only valid bonuses
                //            if (owner().canDoBonus(eBonusCities, pCity: game().city(iLoopCity)))
                //            {
                //                owner().doBonus(eBonusCities, pCity: game().city(iLoopCity), logPrefix: () => HelpText.buildImprovementLinkVariable(getImprovement(), game(), this));
                //            }
                //        }
                //    }
                //}
/*####### Better Old World AI - Base DLL #######
  ### Extra Bonus for all cities         END ###
  ##############################################*/

                {
                    ReligionType eReligionSpread = game().getImprovementReligionSpread(getImprovement());

                    if (eReligionSpread != ReligionType.NONE)
                    {
                        if (pCityTerritory != null)
                        {
                            pCityTerritory.spreadReligion(eReligionSpread, pSpreadPlayer: owner());
                        }
                    }
                }

                if (improvement().mbHolyCity)
                {
                    if (!(owner().isHuman()))
                    {
                        owner().doBonus(infos().Globals.FINISHED_HOLY_SITE_BONUS);
                    }
                }

                if (improvement().mbWonder)
                {
                    if (!owner().canDoEvents() || game().isNoEvents())
                    {
                        BonusType eBonus = infos().Helpers.getWonderCompletionBonus(getImprovement());

                        if (eBonus != BonusType.NONE)
                        {
                            owner().doBonus(eBonus);
                        }
                    }

                    {
                        Func<string> textFunc = () => TextManager.TEXT("TEXT_GAME_IMPROVEMENT_COMPLETED", HelpText.buildImprovementLinkVariable(getImprovement(), game(), this), HelpText.buildPlayerLinkVariable(owner()));

                        game().addTurnSummary(textFunc, TurnLogType.WONDER, bGameLog: false);

                        game().pushLogData(textFunc, GameLogType.WONDER_ACTIVITY, TeamType.NONE, getID(), improvement());

                        for (TeamType eLoopTeam = 0; eLoopTeam < game().getNumTeams(); eLoopTeam++)
                        {
                            doTileReveal(eLoopTeam, PlayerType.NONE);
                        }
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### Units from improvement completion START ##
  ##############################################*/
                //BAI_NUM_IMPROVEMENT_FINISHED_UNITS
                //addImprovementUnit();
                if (((BetterAIInfoGlobals)infos().Globals).BAI_NUM_IMPROVEMENT_FINISHED_UNITS > 0)
                {
                    for (int i = 0; i < ((BetterAIInfoGlobals)infos().Globals).BAI_NUM_IMPROVEMENT_FINISHED_UNITS; i++)
                    {
                        addImprovementUnit();
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### Units from improvement completion  END ###
  ##############################################*/

                owner().doEventTrigger(infos().Globals.IMPROVEMENT_FINISHED_EVENTTRIGGER, (int)getImprovement(), lTriggerSubjects: new() { cityTerritory(), this });

                owner().incrementLeaderStat(infos().Globals.IMPROVEMENT_FINISHED_STAT);

                if (improvement().mbWonder)
                {
                    owner().incrementLeaderStat(infos().Globals.WONDER_FINISHED_STAT);
                }

                {
                    AchievementType eAchievement = improvement().meAchievement;

                    if (eAchievement != AchievementType.NONE)
                    {
                        game().doAchievement(eAchievement, owner());
                    }
                }

                if (!owner().isAIAutoPlay())
                {
                    if (improvement().mbWonder)
                    {
                        game().sendAnalytics(AnalyticsEventType.WONDER_BUILT, "wonder", improvement().mzType);
                    }
                    else
                    {
                        game().sendAnalytics(AnalyticsEventType.IMPROVEMENT_BUILT, "improvement", improvement().mzType);
                    }
                }


                game().markDirtyGoalsAll(); // mark all dirty because of Wonder goals

            }

            //END base.doImprovementFinished();


/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement       START ###
  ##############################################*/
            if (pCityTerritory == null || pCityTerritory.player() == null)
            {
                //just choose randomly
                if (((BetterAIInfoImprovement)improvement()).meBonusAdjacentImprovement != ImprovementType.NONE)
                {
                    if (placeUnownedAdjacentImprovement(((BetterAIInfoImprovement)improvement()).meBonusAdjacentImprovement))
                    {
                        return;
                    }
                }

                if (((BetterAIInfoImprovement)improvement()).meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                {
                    for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                    {
                        if (infos().improvement(eLoopImprovement).meClass == ((BetterAIInfoImprovement)improvement()).meBonusAdjacentImprovementClass)
                        {
                            if (placeUnownedAdjacentImprovement(eLoopImprovement))
                            {
                                return;
                            }
                        }
                    }
                }
            }
            else
            {
                if (((BetterAIInfoImprovement)improvement()).meBonusAdjacentImprovement != ImprovementType.NONE)
                {
                    if (((BetterAICity)pCityTerritory).canAddImprovementTileAdjacent(this, ((BetterAIInfoImprovement)improvement()).meBonusAdjacentImprovement))
                    {
                        ((BetterAICity)pCityTerritory).addImprovementTileAdjacent(this, ((BetterAIInfoImprovement)improvement()).meBonusAdjacentImprovement);
                        return;
                    }
                }

                if (((BetterAIInfoImprovement)improvement()).meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                {
                    for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                    {
                        if (infos().improvement(eLoopImprovement).meClass == ((BetterAIInfoImprovement)improvement()).meBonusAdjacentImprovementClass)
                        {
                            if (((BetterAICity)pCityTerritory).canAddImprovementTileAdjacent(this, ((BetterAIInfoImprovement)improvement()).meBonusAdjacentImprovement))
                            {
                                ((BetterAICity)pCityTerritory).addImprovementTileAdjacent(this, ((BetterAIInfoImprovement)improvement()).meBonusAdjacentImprovement);
                                return;
                            }
                        }
                    }
                }

            }
            return;
        }
/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement         END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### No immediate unit from improvement START #
  ##############################################*/
        //solved by overriding doImprovementFinished directly
        //public override Unit addImprovementUnit(UnitType eUnit = UnitType.NONE)
        //{
        //    MethodBase MB_doImprovementFinished = typeof(BetterAITile).GetMethod("doImprovementFinished", BindingFlags.Instance | BindingFlags.Public);
        //    MethodBase caller = (new StackFrame(skipFrames: 1, fNeedFileInfo: false).GetMethod());
            
        //    if (caller.GetHashCode() != MB_doImprovementFinished.GetHashCode())
        //    {
        //        return base.addImprovementUnit(eUnit);
        //    }
        //    else return null;
        //}
/*####### Better Old World AI - Base DLL #######
  ### No immediate unit from improvement END ###
  ##############################################*/

        //lines 9991-10060
        public virtual bool isDirectionHostileZOC(DirectionType eDirection, Unit pUnit, TeamType eTeamVisibility, bool bIgnoreRiver = false)
        {
            Tile pAdjacentTile = tileAdjacent(eDirection);
            if (pAdjacentTile == null)
            {
                return false;
            }
            if (isWater() != pAdjacentTile.isWater())
            {
                return false;
            }

            if (pAdjacentTile.isRevealedCity(eTeamVisibility) && isLand() && !pAdjacentTile.isNeutralOrFriendly(pUnit?.getTeam() ?? eTeamVisibility, pUnit?.getTribe() ?? TribeType.NONE, eTeamVisibility))
            {
                if ((pUnit == null) || pUnit.info().mbWater != isWater() || !(pUnit.hasIgnoreZOC()))
                {
                    return true;
                }
            }

            if (pAdjacentTile.hasUnit() && pAdjacentTile.isVisible(eTeamVisibility))
            {
                using (var unitListScoped = CollectionCache.GetListScoped<int>())
                {
                    pAdjacentTile.getAliveUnits(unitListScoped.Value);

                    foreach (int iUnitID in unitListScoped.Value)
                    {

/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
                        //Unit pAdjacentUnit = game().unit(iUnitID);
                        BetterAIUnit pAdjacentUnit = (BetterAIUnit)game().unit(iUnitID);
                        if (bIgnoreRiver || !isRiver(eDirection) || (pAdjacentUnit.mbAmphibiousEmbark && ((BetterAIInfoGlobals)infos().Globals).BAI_AMPHIBIOUS_ZOC_CROSSES_RIVER == 1)) //BAI: without river or with mbAmphibiousEmbark
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
                        {
                            if (pAdjacentUnit.hasZOC(pUnit?.getType() ?? UnitType.NONE) && pAdjacentUnit.info().mbWater == isWater() && !(pAdjacentUnit.isHiddenFrom(eTeamVisibility)))
                            {
                                if (pUnit == null || !(pUnit.isHiddenTileFrom(eTeamVisibility, this)))
                                {
                                    if (game().isHostileUnit(pUnit?.getTeam() ?? eTeamVisibility, pUnit?.getTribe() ?? TribeType.NONE, pAdjacentUnit))
                                    {
                                        if (pUnit == null)
                                        {
                                            return true;
                                        }
                                        if (isWater())
                                        {
                                            if (!game().isWaterUnit(pUnit.getType(), pUnit.getPlayer(), TribeType.NONE)) // land tribe units can't ignore water ZOC, even raiders
                                            {
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            if (pUnit.info().mbWater)
                                            {
                                                return true;
                                            }
                                        }
                                        if (!(pUnit.hasIgnoreZOC()))
                                        {
                                            return true;
                                        }
                                        if (pAdjacentUnit.isUnitZoc(pUnit.getType()))
                                        {
                                            return true;
                                        }
                                    }
                                }
                            }
                        }



                    }
                }
            }
            return false;
        }
        
        //lines 10061-10097
        public override bool isHostileZOC(Unit pUnit, TeamType eTeamVisibility, bool bIgnoreRiver = false, Dictionary<(int, DirectionType), bool> zocMap = null)
        {
            using var profileScope = new UnityProfileScope("Tile.isHostileZOC");

            if (impassable())
            {
                return false;
            }

            if (hasCity())
            {
                return false;
            }

            if (hasActiveImprovement() && improvement().mbIgnoreZOC)
            {
                return false;
            }

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
                //if (bIgnoreRiver || !isRiver(eLoopDirection))
                {
                    if (zocMap == null || !zocMap.TryGetValue((getID(), eLoopDirection), out bool bZoc))
                    {
                        bZoc = isDirectionHostileZOC(eLoopDirection, pUnit, eTeamVisibility, bIgnoreRiver);
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement           END ###
  ##############################################*/
                        zocMap?.Add((getID(), eLoopDirection), bZoc);
                    }
                    if (bZoc)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

/*####### Better Old World AI - Base DLL #######
  ### AI: Improvement Value            START ###
  ##############################################*/
        //lines 12161-12258 (Test v1.0.70827) but with added dEffectCityExtraCounts
        public virtual int yieldOutputForGovernor(ImprovementType eImprovement, SpecialistType eSpecialist, YieldType eYield, City pCity, bool bCityEffects, bool bBaseOnly, bool bCost, Character pGovernor, bool bTheology = true, Dictionary<int, ImprovementType> newImprovements = null, Dictionary<EffectCityType, int> dEffectCityExtraCounts = null)
        {
            //using var profileScope = new UnityProfileScope("Game.tileYieldOutput");

            if (dEffectCityExtraCounts == null || dEffectCityExtraCounts.Count == 0)
            {
                return base.yieldOutputForGovernor(eImprovement, eSpecialist, eYield, pCity, bCityEffects, bBaseOnly, bCost, pGovernor, bTheology, newImprovements);

            }

            int iOutput = yieldBaseForGovernor(eImprovement, eSpecialist, bTheology, eYield, pCity, pGovernor, newImprovements, dEffectCityExtraCounts);
            //int iOutput = yieldBaseForGovernor(eImprovement, eSpecialist, bTheology, eYield, pCity, pGovernor, newImprovements);

            if (eImprovement != ImprovementType.NONE)
            {
                if (iOutput != 0)
                {
                    if (!bBaseOnly)
                    {
                        iOutput = infos().utils().modify(iOutput, yieldModifierNoSpecialist(eImprovement, eYield, pCity, pGovernor, newImprovements, dEffectCityExtraCounts));
                    }

                    if (eSpecialist != SpecialistType.NONE)
                    {
                        ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;
                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            iOutput = infos().utils().modify(iOutput, infos().specialist(eSpecialist).maiImprovementClassModifier[eImprovementClass]);
                        }
                    }
                }

                if (!bBaseOnly)
                {
                    iOutput = infos().utils().modify(iOutput, getBaseYieldModifier());
                }

                if (!bBaseOnly && bCost)
                {
                    iOutput += infos().improvement(eImprovement).maiYieldConsumption[eYield];
                }
            }

            if (bCityEffects)
            {
                iOutput += yieldOutputCityEffects(eImprovement, eSpecialist, eYield, pGovernor, bBaseOnly, pCity);
            }

            return iOutput;
        }

        //lines 12366-12479 (Test v1.0.70827)
        public virtual int yieldBaseForGovernor(ImprovementType eImprovement, SpecialistType eSpecialist, bool bTheology, YieldType eYield, City pCity, Character pGovernor, Dictionary<int, ImprovementType> newImprovements = null, Dictionary<EffectCityType, int> dEffectCityExtraCounts = null)
        {
            //using var profileScope = new UnityProfileScope("Game.tileYieldBaseOutputNoSpecialist");

            if (dEffectCityExtraCounts == null || dEffectCityExtraCounts.Count == 0)
            {
                return base.yieldBaseForGovernor(eImprovement, eSpecialist, bTheology, eYield, pCity, pGovernor, newImprovements);
            }

            ResourceType eResource = getResource();

            if (eImprovement == ImprovementType.NONE)
            {
                if (eResource != ResourceType.NONE)
                {
                    return infos().resource(eResource).maiYieldNoImprovement[eYield];
                }
                else
                {
                    return 0;
                }
            }

            BetterAICity pCityTerritory = (BetterAICity)pCity ?? (BetterAICity)cityTerritory();

            int iOutput = 0;

            infos().Helpers.yieldOutputImprovement(eImprovement, eYield, eResource, ref iOutput);

            iOutput += infos().improvement(eImprovement).maaiTerrainYieldOutput[getTerrain(), eYield];

            if (bTheology)
            {
                ReligionType eReligionPrereq = infos().improvement(eImprovement).meReligionPrereq;
                if (eReligionPrereq != ReligionType.NONE)
                {
                    for (TheologyType eLoopTheology = 0; eLoopTheology < infos().theologiesNum(); eLoopTheology++)
                    {
                        if (game().isReligionTheology(eReligionPrereq, eLoopTheology))
                        {
                            infos().Helpers.yieldOutputTheologySpecialist(eSpecialist, eYield, eLoopTheology, ref iOutput);
                        }
                    }
                }
            }

            {
                int iValue = infos().improvement(eImprovement).maiAdjacentWonderYieldOutput[eYield];
                if (iValue != 0)
                {
                    iOutput += (iValue * countTeamAdjacentWonders(newImprovements));
                }
            }

            {
                int iValue = infos().improvement(eImprovement).maiAdjacentResourceYieldOutput[eYield];
                if (iValue != 0)
                {
                    iOutput += (iValue * countTeamAdjacentResources());
                }
            }

            {
                int iValue = infos().improvement(eImprovement).maiTradeNetworkYieldOutput[eYield];
                if (iValue != 0)
                {
                    if (pCityTerritory != null)
                    {
                        if (tradeNetworkTestImprovement(pCityTerritory.getPlayer(), eImprovement))
                        {
                            iOutput += iValue;
                        }
                    }
                }
            }

            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; ++eDirection)
            {
                Tile pAdjacent = tileAdjacent(eDirection, true);
                if (pAdjacent != null && pAdjacent.getTeam() == getTeam())
                {
                    ImprovementType eAdjacentImprovement = pAdjacent.getActiveImprovement();
                    if (newImprovements != null && newImprovements.TryGetValue(pAdjacent.getID(), out ImprovementType eNewImprovement))
                    {
                        eAdjacentImprovement = eNewImprovement;
                    }
                    if (eAdjacentImprovement != ImprovementType.NONE)
                    {
                        iOutput += infos().improvement(eImprovement).maaiAdjacentImprovementYield[eAdjacentImprovement, eYield];

                        if (infos().improvement(eAdjacentImprovement).meClass != ImprovementClassType.NONE)
                        {
                            iOutput += infos().improvement(eImprovement).maaiAdjacentImprovementClassYield[infos().improvement(eAdjacentImprovement).meClass, eYield];
                        }
                    }
                }
            }

            if (pCityTerritory != null)
            {
                iOutput += pCityTerritory.getEffectCityImprovementYieldForGovernor(eImprovement, eYield, pGovernor, dEffectCityExtraCounts);
            }

            ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;

            if (eImprovementClass != ImprovementClassType.NONE)
            {
                {
                    int iValue = infos().improvementClass(eImprovementClass).maiAdjacentResourceYieldOutput[eYield];
                    if (iValue != 0)
                    {
                        iOutput += (iValue * countTeamAdjacentResources());
                    }
                }

                {
                    ReligionType eReligionPrereq = infos().improvement(eImprovement).meReligionPrereq;

                    if (eReligionPrereq != ReligionType.NONE)
                    {
                        for (TheologyType eLoopTheology = 0; eLoopTheology < infos().theologiesNum(); eLoopTheology++)
                        {
                            if (game().isReligionTheology(eReligionPrereq, eLoopTheology))
                            {
                                int iValue = infos().improvementClass(eImprovementClass).maaiTheologyYieldOutput[eLoopTheology, eYield];
                                if (iValue != 0)
                                {
                                    iOutput += iValue;
                                }
                            }
                        }
                    }
                }
            }

            return iOutput;
        }

        //lines 12481-12559 (Test v1.0.70827)
        public virtual int yieldModifierNoSpecialist(ImprovementType eImprovement, YieldType eYield, City pCity, Character pGovernor, Dictionary<int, ImprovementType> newImprovements = null, Dictionary<EffectCityType, int> dEffectCityExtraCounts = null)
        {
            //using var profileScope = new UnityProfileScope("Game.tileYieldModifierNoSpecialist");
            if (eImprovement == ImprovementType.NONE)
            {
                return 0;
            }

            if (dEffectCityExtraCounts == null || dEffectCityExtraCounts.Count == 0)
            {
                return base.yieldModifierNoSpecialist(eImprovement, eYield, pCity, pGovernor, newImprovements);
            }

            int iModifier = 0;

            iModifier += infos().improvement(eImprovement).maaiTerrainYieldModifier[getTerrain(), eYield];
            iModifier += infos().improvement(eImprovement).maaiHeightYieldModifier[getHeight(), eYield];

            BetterAICity pCityTerritory = (BetterAICity)pCity ?? (BetterAICity)cityTerritory();

            if (pCityTerritory != null)
            {
                iModifier += pCityTerritory.getImprovementModifierForGovernor(eImprovement, pGovernor, dEffectCityExtraCounts);
            }

            {
                int iSubValue = infos().improvement(eImprovement).miFreshWaterModifier + infos().improvement(eImprovement).maiYieldFreshWaterModifier[eYield];
                if (iSubValue != 0)
                {
                    if (isFreshWaterAccess())
                    {
                        iModifier += iSubValue;
                    }
                }
            }

            {
                int iSubValue = infos().improvement(eImprovement).miRiverModifier + infos().improvement(eImprovement).maiYieldRiverModifier[eYield];
                if (pCityTerritory != null)
                {
                    iSubValue += pCityTerritory.getImprovementRiverModifierForGovernor(eImprovement, pGovernor, dEffectCityExtraCounts);
                }
                if (iSubValue != 0)
                {
                    if (isRiver())
                    {
                        iModifier += iSubValue;
                    }
                }
            }

            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
            {
                Tile pAdjacentTile = tileAdjacent(eLoopDirection, true);

                if (pAdjacentTile != null)
                {
                    iModifier += infos().improvement(eImprovement).maaiAdjacentHeightYieldModifier[pAdjacentTile.getHeight(), eYield];

                    if (pAdjacentTile.getTeam() == getTeam())
                    {
                        ImprovementType eAdjacentImprovement = pAdjacentTile.getActiveImprovement();
                        if (newImprovements != null)
                        {
                            eAdjacentImprovement = newImprovements.GetOrDefault(pAdjacentTile.getID(), eAdjacentImprovement);
                        }
                        if (eAdjacentImprovement != ImprovementType.NONE)
                        {
                            iModifier += infos().Helpers.adjacentYieldOutputImprovementModifier(eImprovement, eAdjacentImprovement);
                        }
                    }
                }
            }
            return iModifier;
        }
/*####### Better Old World AI - Base DLL #######
  ### AI: Improvement Value              END ###
  ##############################################*/

    }
}
