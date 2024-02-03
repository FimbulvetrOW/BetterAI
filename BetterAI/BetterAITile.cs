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
            BetterAIInfoImprovement eInfoImprovement = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            City pCityTerritory = cityTerritory();


            if (!isImprovementValid(eImprovement, pCityTerritory, bTestEnabled))
            {
                return false;
            }

            if (!bForceImprovement)
            {
                ImprovementType eImprovementPrereq = eInfoImprovement.meImprovementPrereq;

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

            BetterAIInfoImprovement eInfoImprovement = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            if (isUrban() && !(eInfoImprovement.mbUrban))
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

            ReligionType eReligionPrereq = eInfoImprovement.meReligionPrereq;
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
                if (eInfoImprovement.mbRequiresUrban)
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

                ImprovementClassType eImprovementClass = eInfoImprovement.meClass;
                if (bTestAdjacent)
                {
                    if (bTestReligion && eReligionPrereq != ReligionType.NONE)
                    {
                        if (eInfoImprovement.mbNoAdjacentReligion)
                        {
                            if (adjacentToOtherImprovementReligion(eReligionPrereq))
                            {
                                return false;
                            }
                        }
                    }

                    if (!bUpgradeImprovement)
                    {
                        ImprovementType eAdjacentImprovementPrereq = eInfoImprovement.meAdjacentImprovementPrereq;

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
                        ImprovementClassType eAdjacentImprovementClassPrereq = eInfoImprovement.meAdjacentImprovementClassPrereq;

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

            //BetterAIInfoImprovement eInfoImprovement = (BetterAIInfoImprovement)infos().improvement(eImprovement);
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



    }
}
