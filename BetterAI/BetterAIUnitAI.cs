using System;
using System.Xml;
using System.Collections.Generic;
using Mohawk.SystemCore;
using UnityEngine;
using TenCrowns.GameCore;

namespace BetterAI
{
    public partial class BetterAIUnit : Unit
    {
        public class BetterAIUnitAI : UnitAI
        {
            static public int LAST_STAND_EXTRA_HP = 3;


            //re-enabling pillaging on water by tribal land units if delay turns are set
            //lines 1019-1064
            public override bool shouldTribePillage(Tile pTile)
            {
                //using var profileScope = new UnityProfileScope("UnitAI.shouldPillage");

                if (!pTile.canUnitOccupy(unit, TeamType.NONE, bTestTheirUnits: false, bTestOurUnits: false, bFinalMoveTile: true, bBump: false))
                {
                    return false;
                }

                if (!unit.canPillage(pTile, ActingPlayer))
                {
                    return false;
                }

                City pCityTerritory = pTile.revealedCityTerritory(getTeam());

                if (pCityTerritory != null)
                {
                    if (!game.isHostileUnitCity(unit, pCityTerritory))
                    {
                        return false;
                    }
                }

                TribeType eTribe = unit.getTribe();

                if (eTribe == TribeType.NONE)
                {
                    return false;
                }

                if (pCityTerritory != null)
                {
                    if (game.hasTribeAlly(eTribe) && (game.getTribeAllyTeam(eTribe) == pCityTerritory.getCaptureTeam()))
                    {
                        return false;
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### No Raider Ships                  START ###
  ##############################################*/
                //if (pTile.isWater() && !unit.info().mbWater)
                if (pTile.isWater() && !unit.info().mbWater && (((BetterAIInfoGlobals)(infos.Globals)).BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS) == 0)
                {
                    return false;
                }
/*####### Better Old World AI - Base DLL #######
  ### No Raider Ships                  START ###
  ##############################################*/

                return true;
            }

            //lines 1828-2063
            //there is a lot more to be done here
            public override int attackValue(Tile pFromTile, Tile pTargetTile, bool bCheckOtherUnits, int iExtraModifier, out bool bCivilian, out int iPushTileID, out bool bStun, out int iSelfDamage)
            {
                int iValue = base.attackValue(pFromTile, pTargetTile, bCheckOtherUnits, iExtraModifier, out bCivilian, out iPushTileID, out bStun, out iSelfDamage);

                Unit pTargetUnit = pTargetTile.defendingUnit();
                if ((unit.hasPlayer()) && !(unit.canDamageCity(pTargetTile)) && (pTargetUnit != null) && (unit.canDamageUnit(pTargetUnit)))
                {
                    if (unit.player().isUnitObsolete(unit.getType()))
                    {
                        iValue *= 2; // do use it. Don't just decide to go upgrade instead
                    }
                }
                return iValue;
            }

        }
    }
}
