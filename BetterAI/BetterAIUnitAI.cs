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

            protected override bool foundCity()
            {
                //using var profileScope = new UnityProfileScope("UnitAI.foundCity");

                FamilyType eBestFamily = FamilyType.NONE;
                NationType eBestNation = NationType.NONE;

                if (unit.getNation() == NationType.NONE)
                {
                    int iBestValue = 0;

                    for (NationType eLoopNation = 0; eLoopNation < infos.nationsNum(); eLoopNation++)
                    {
                        FamilyType eLoopFamily = game.isCharacters() ? AI.getBestFoundFamily(unit.tile(), eLoopNation) : FamilyType.NONE;

                        if (ActingPlayer != null && ActingPlayer.canFoundCity(eLoopFamily, eLoopNation))
                        {
                            int iValue = game.randomNext(1000) + 1;
                            if (iValue > iBestValue)
                            {
                                eBestNation = eLoopNation;
                                eBestFamily = eLoopFamily;
                                iBestValue = iValue;
                            }
                        }
                    }

                    if (eBestNation == NationType.NONE)
                    {
                        MohawkAssert.Assert(false, "more players than unique nations");
                        return false;
                    }
                }
                else
                {
                    eBestNation = unit.getNation();
                    if (game.isCharacters())
                    {
                        eBestFamily = AI.getBestFoundFamily(unit.tile(), eBestNation);
                    }
                }

                if (unit.canFoundCity(unit.tile(), ActingPlayer, eBestFamily, eBestNation))
                {
                    using (var unitsScoped = CollectionCache.GetHashSetScoped<int>())
                    {
                        if (unit.hasPlayer())
                        {
                            for (int i = 0; i < unit.player().getNumUnits(); ++i)
                            {
                                Unit pUnit = unit.player().unitAt(i);
                                if (pUnit != null)
                                {
                                    unitsScoped.Value.Add(pUnit.getID());
                                }
                            }
                        }
                        City pNewCity = unit.foundCity(eBestFamily, eBestNation, ActingPlayer);
                        AI.updateCityDistances();
                        AI.cacheCityYieldValues(pNewCity);
/*####### Better Old World AI - Base DLL #######
  ### AI: cache improvement values     START ###
  ##############################################*/
                        ((BetterAIPlayer.BetterAIPlayerAI)AI).cacheCityImprovementValues(pNewCity);
/*####### Better Old World AI - Base DLL #######
  ### AI: cache improvement values       END ###
  ##############################################*/
                        AI.cacheCityBestImprovements(pNewCity);
                        AI.cacheTechValues();
                        AI.assignBestGovernor(pNewCity);
                        AI.doCityTurn(pNewCity);
                        AI.checkNewCitySites(true);
                        if (unit.hasPlayer())
                        {
                            for (int i = 0; i < unit.player().getNumUnits(); ++i)
                            {
                                Unit pNewUnit = unit.player().unitAt(i);
                                if (pNewUnit != null && !unitsScoped.Value.Contains(pNewUnit.getID()))
                                {
                                    AI.addTileProtection(pNewUnit.getID());
                                }
                            }
                        }
                    }

                    return true;
                }

                return false;
            }


            //re-enabling pillaging on water by tribal land units if delay turns are set
            //lines 1019-1064
            public override bool shouldTribePillage(Tile pTile)
            {
                //using var profileScope = new UnityProfileScope("UnitAI.shouldPillage");

                if (!pTile.canUnitOccupy(unit, TeamType.NONE, bTestTheirUnits: false, bTestOurUnits: false, bFinalMoveTile: true, bBumped: false))
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
