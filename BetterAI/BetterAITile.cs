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

namespace BetterAI
{
    public class BetterAITile : Tile
    {
/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
        public virtual CityBiomeType getCityBiome()
        {
            BetterAICity pCityTerritory = (BetterAICity)cityTerritory();

            if (pCityTerritory != null)
            {
                return pCityTerritory.getCityBiome();
            }
            else return CityBiomeType.NONE;
        }

        //lines 3077-3121
        public override void setTerrain(TerrainType eNewValue)
        {
            TerrainType eOldTerrain = getTerrain();
            base.setTerrain(eNewValue);
            if (eOldTerrain != eNewValue)
            {
                BetterAICity pCityTerritory = (BetterAICity)cityTerritory();
                pCityTerritory?.setTerrainChanged();
            }
        }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/


        //canHaveImprovement: lines 4805-5098
        public virtual bool canCityTileHaveImprovement(ImprovementType eImprovement, TeamType eTeamTerritory = TeamType.NONE, bool bTestTerritory = true, bool bTestEnabled = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bUpgradeImprovement = false, bool bForceImprovement = false)
        {
            BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            City pCityTerritory = cityTerritory();


            if (!isImprovementValid(eImprovement, pCityTerritory, bTestEnabled))
            {
                return false;
            }

            if (!bForceImprovement)
            {
                ImprovementType eImprovementPrereq = pImprovementInfo.meImprovementPrereq;

                if (eImprovementPrereq != ImprovementType.NONE)
                {
                    //if (pCityTerritory == null)
                    //{
                    //    return false;
                    //}

                    int iCount = pCityTerritory.getFinishedImprovementCount(eImprovementPrereq);

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
            return true;
        }

        //canHaveImprovement: lines 4805-5098
        public virtual bool canGeneralTileHaveImprovement(ImprovementType eImprovement, TeamType eTeamTerritory = TeamType.NONE, bool bTestTerritory = true, bool bTestEnabled = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bUpgradeImprovement = false, bool bForceImprovement = false)
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
            if (getCityTerritory() == -1 && !isImprovementValid(eImprovement, null, bTestEnabled))
            {
                return false;
            }

            BetterAIInfoImprovement pImprovementInfo = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            if (isUrban() && !(pImprovementInfo.mbUrban))
            {
                return false;
            }

            if (bTestTerritory)
            {
                if (infos().improvement(eImprovement).mbTerritoryOnly)
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

            if (hasImprovementFinished())
            {
                if (improvement().mbPermanent)
                {
                    return false;
                }
            }

            if (bTestAdjacent)
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

                ImprovementClassType eImprovementClass = pImprovementInfo.meClass;
                if (bTestAdjacent)
                {
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
                            if (!adjacentToCityImprovementFinished(eAdjacentImprovementPrereq))
                            {
                                return false;
                            }
                        }
                    }

                    if (!bUpgradeImprovement)
                    {
                        ImprovementClassType eAdjacentImprovementClassPrereq = pImprovementInfo.meAdjacentImprovementClassPrereq;

                        if (eAdjacentImprovementClassPrereq != ImprovementClassType.NONE)
                        {
                            if (!adjacentToCityImprovementClassFinished(eAdjacentImprovementClassPrereq))
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
            return true;
        }

        //lines 4805-5098
        public override bool canHaveImprovement(ImprovementType eImprovement, TeamType eTeamTerritory = TeamType.NONE, bool bTestTerritory = true, bool bTestEnabled = true, bool bTestAdjacent = true, bool bTestReligion = true, bool bUpgradeImprovement = false, bool bForceImprovement = false)
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
            BetterAICity pCityTerritory = (BetterAICity)cityTerritory();
            if (pCityTerritory != null)
            {
                bool bPrimaryUnlock = true;
                if (!(pCityTerritory.canCityHaveImprovement(eImprovement, ref bPrimaryUnlock, eTeamTerritory, bTestTerritory, bTestEnabled, bTestReligion, bUpgradeImprovement, bForceImprovement)))
                {
                    return false;
                }

                if (!(canCityTileHaveImprovement(eImprovement, eTeamTerritory, bTestTerritory, bTestEnabled, bTestAdjacent, bTestReligion, bUpgradeImprovement, bForceImprovement)))
                {
                    return false;
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/

            return true;
        }

/*####### Better Old World AI - Base DLL #######
  ### AI: Improvement Value            START ###
  ##############################################*/
        //lines 12161-12258 (Test v1.0.70827) but with added dEffectCityExtraCounts
        public virtual int yieldOutputForGovernor(ImprovementType eImprovement, SpecialistType eSpecialist, YieldType eYield, City pCity, bool bCityEffects, bool bBaseOnly, Character pGovernor, Dictionary<EffectCityType, int> dEffectCityExtraCounts)
        {
            //using var profileScope = new UnityProfileScope("Game.tileYieldOutput");

            //for the next update: 
            //if (dEffectCityExtraCounts == null || dEffectCityExtraCounts.Count = 0)
            //{
            //    return base.yieldOutputForGovernor(eImprovement, eSpecialist, eYield, pCity, bCityEffects, bBaseOnly, pGovernor);
            //}

            int iOutput = yieldBaseForGovernor(eImprovement, eYield, pCity, pGovernor, dEffectCityExtraCounts);

            if (eImprovement != ImprovementType.NONE)
            {
                if (iOutput != 0)
                {
                    if (!bBaseOnly)
                    {
                        iOutput = infos().utils().modify(iOutput, yieldModifierNoSpecialist(eImprovement, eYield, pCity, pGovernor, dEffectCityExtraCounts));
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
                    iOutput += infos().improvement(eImprovement).maiYieldConsumption[eYield];
                }
            }

            if (bCityEffects)
            {
                iOutput += yieldOutputCityEffects(eImprovement, eSpecialist, eYield, pGovernor, bBaseOnly, pCity);
            }

            if (game().isOccurrenceActive(ePlayer: getOwner()))
            {
                for (int i = 0; i < game().getNumOccurrences(); ++i)
                {
                    OccurrenceData pLoopData = game().getOccurrenceDataAt(i);
                    if (pLoopData.isActive(game()) && pLoopData.isValidForPlayer(getOwner()))
                    {
                        if (infos().occurrence(pLoopData.meType).mbNoYieldsOnCoast)
                        {
                            if (isSaltCoastLand() || isSaltCoastWater())
                            {
                                return 0;
                            }
                        }
                        if (infos().occurrence(pLoopData.meType).mbNoYieldsOnFlatRiver)
                        {
                            if (isFreshWaterAccess() && isFlat())
                            {
                                return 0;
                            }
                        }
                        if (infos().occurrence(pLoopData.meType).miBaseYieldCoastModifier != 0)
                        {
                            if (isSaltCoastLand() || isSaltCoastWater())
                            {
                                return infos().utils().modify(iOutput, infos().occurrence(pLoopData.meType).miBaseYieldCoastModifier);
                            }
                        }
                        if (infos().occurrence(pLoopData.meType).miBaseYieldRiverModifier != 0)
                        {
                            if (isRiver())
                            {
                                return infos().utils().modify(iOutput, infos().occurrence(pLoopData.meType).miBaseYieldRiverModifier);
                            }
                        }
                        if (infos().occurrence(pLoopData.meType).miTileBaseYieldModifier != 0)
                        {
                            if (pLoopData.isAffectedTile(getID()))
                            {
                                return infos().utils().modify(iOutput, infos().occurrence(pLoopData.meType).miTileBaseYieldModifier);
                            }
                        }
                        if (infos().occurrence(pLoopData.meType).miTileBaseYieldModifierAdjacent != 0)
                        {
                            for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; ++eDirection)
                            {
                                Tile pAdjacent = tileAdjacent(eDirection, true);
                                if (pAdjacent != null)
                                {
                                    if (pLoopData.isAffectedTile(pAdjacent.getID()))
                                    {
                                        return infos().utils().modify(iOutput, infos().occurrence(pLoopData.meType).miTileBaseYieldModifierAdjacent);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return iOutput;
        }

        //lines 12366-12479 (Test v1.0.70827)
        public virtual int yieldBaseForGovernor(ImprovementType eImprovement, YieldType eYield, City pCity, Character pGovernor, Dictionary<EffectCityType, int> dEffectCityExtraCounts)
        {
            //using var profileScope = new UnityProfileScope("Game.tileYieldBaseOutputNoSpecialist");

            //for the next update: 
            //if (dEffectCityExtraCounts == null || dEffectCityExtraCounts.Count = 0)
            //{
            //    return base.yieldBaseForGovernor(eImprovement, eYield, pCity, pGovernor);
            //}

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

            ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;

            BetterAICity pCityTerritory = (BetterAICity)pCity ?? (BetterAICity)cityTerritory();

            int iOutput = 0;

            infos().Helpers.yieldOutputImprovement(eImprovement, eYield, eResource, game(), ref iOutput);

            iOutput += infos().improvement(eImprovement).maaiTerrainYieldOutput[getTerrain(), eYield];

            {
                int iValue = infos().improvement(eImprovement).maiAdjacentWonderYieldOutput[eYield];
                if (iValue != 0)
                {
                    iOutput += (iValue * countTeamAdjacentWonders());
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
                if (pAdjacent != null)
                {
                    ImprovementClassType eAdjacentImprovementClass = pAdjacent.getImprovementClassFinished();
                    if (eAdjacentImprovementClass != ImprovementClassType.NONE)
                    {
                        iOutput += infos().improvement(eImprovement).maaiAdjacentImprovementClassYield[eAdjacentImprovementClass, eYield];
                    }
                }
            }

            if (pCityTerritory != null)
            {
                iOutput += pCityTerritory.getEffectCityImprovementYieldForGovernor(eImprovement, eYield, pGovernor, dEffectCityExtraCounts);
                iOutput += pCityTerritory.getEffectCityTerrainYieldForGovernor(getTerrain(), eYield, pGovernor, dEffectCityExtraCounts);
            }

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

                if (pCityTerritory != null)
                {
                    iOutput += pCityTerritory.getEffectCityImprovementClassYieldForGovernor(eImprovementClass, eYield, pGovernor, dEffectCityExtraCounts);
                }
            }

            return iOutput;
        }

        //lines 12481-12559 (Test v1.0.70827)
        public virtual int yieldModifierNoSpecialist(ImprovementType eImprovement, YieldType eYield, City pCity, Character pGovernor, Dictionary<EffectCityType, int> dEffectCityExtraCounts)
        {
            //using var profileScope = new UnityProfileScope("Game.tileYieldModifierNoSpecialist");

            //for the next update: 
            //if (dEffectCityExtraCounts == null || dEffectCityExtraCounts.Count = 0)
            //{
            //    return base.yieldBaseForGovernor(eImprovement, eYield, pCity, pGovernor);
            //}

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

            ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;
            if (eImprovementClass != ImprovementClassType.NONE)
            {
                if (pCityTerritory != null)
                {
                    iModifier += pCityTerritory.getImprovementClassModifierForGovernor(eImprovementClass, pGovernor, dEffectCityExtraCounts);
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
                        ImprovementType eAdjacentImprovement = pAdjacentTile.getImprovementFinished();

                        if (eAdjacentImprovement != ImprovementType.NONE)
                        {
                            ImprovementClassType eAdjacentImprovementClass = infos().improvement(eAdjacentImprovement).meClass;

                            iModifier += infos().improvement(eAdjacentImprovement).maiAdjacentImprovementModifier[eImprovement];
                            if ((eAdjacentImprovementClass != ImprovementClassType.NONE) && (eImprovementClass != ImprovementClassType.NONE))
                            {
                                iModifier += infos().improvement(eAdjacentImprovement).maiAdjacentImprovementClassModifier[eImprovementClass];
                                iModifier += infos().improvementClass(eAdjacentImprovementClass).maiAdjacentImprovementClassModifier[eImprovementClass];
                            }
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
