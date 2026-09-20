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

            public virtual bool isClosestCity(City pCity)
            {
                return pCity == ClosestCity;
            }



            //re-enabling pillaging on water by tribal land units if delay turns are set
            //lines 1019-1064
            public override bool shouldTribePillage(Tile pTile)
            {
                //UnityEngine.Debug.Log("UnitAI.shouldTribePillage");

                //using var profileScope = new UnityProfileScope("UnitAI.shouldTribePillage");

                if (!pTile.canUnitOccupy(unit, TeamType.NONE, bTestTheirUnits: false, bTestOurUnits: false, bFinalMoveTile: true, bBump: false))
                {
                    return false;
                }

                if (!canPillage(pTile))
                {
                    return false;
                }

                City pCityTerritory = pTile.revealedCityTerritory(ActingTeam);

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
  ### No Raider Ships                    END ###
  ##############################################*/

                return true;
            }

            //lines 1828-2063
            //there is a lot more to be done here
            public override int attackValue(Tile pFromTile, Tile pTargetTile, bool bCheckOtherUnits, int iExtraModifier, out bool bCivilian, out int iPushTileID, out bool bStun, out int iSelfDamage, out bool bPriorityUnit)
            {
                int iValue = base.attackValue(pFromTile, pTargetTile, bCheckOtherUnits, iExtraModifier, out bCivilian, out iPushTileID, out bStun, out iSelfDamage, out bPriorityUnit);

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


            public override Character getBestGeneral()
            {
                //UnityEngine.Debug.Log("UnitAI.getBestGeneral");

                //using var profileScope = new UnityProfileScope("UnitAI.getBestGeneral");

                long iBestValue = 0;
                Character pBestCharacter = null;
                using (var characterListScoped = CollectionCache.GetListScoped<int>())
                {
                    unit.buildGeneralList(characterListScoped.Value);

                    foreach (int iCharacter in characterListScoped.Value)
                    {
                        Character pCharacter = game.character(iCharacter);

/*####### Better Old World AI - Base DLL #######
  ### No Governor Courtiers as Generals START###
  ##############################################*/
                        InfoTrait pInfoArcheType = infos.trait(pCharacter.getArchetype());

                        if ((pInfoArcheType.mbGovernorPrereq || pInfoArcheType.mbGovernorAll) && !(pInfoArcheType.mbGeneralPrereq || pInfoArcheType.mbGeneralAll))
                        {
                            continue;
                        }
/*####### Better Old World AI - Base DLL #######
  ### No Governor Courtiers as Generals  END ###
  ##############################################*/
                        
                        long iValue = getGeneralValue(pCharacter);
                        if (iValue > iBestValue)
                        {
                            iBestValue = iValue;
                            pBestCharacter = pCharacter;
                        }
                    }
                }
                return pBestCharacter;
            }

            //lines 5669-5742
            //I no longer remember why I thought I needed to modify setRaidCity. The original should do fine.


            protected override Tile getBestBuyTile(City pCity, YieldType eYield)
            {
                //using var profileScope = new UnityProfileScope("UnitAI.getBestBuyTile");

                //(Tile pBestTile, long iBestValue) = AI.getBestBuyTile(pCity, eYield);
                (Tile pBuyTile, long iBuyValue) = ((BetterAIPlayer.BetterAIPlayerAI)AI).getBestUnitBuyTileInCity(unit, eYield, pCity, bSkipIfUnlockedInCity: false, bUnitInCity: true);

                if (pBuyTile != null)
                {
                    if (unit.canBuyTile(pBuyTile, pCity, eYield, ActingPlayer))
                    {
                        return pBuyTile;
                    }
                }
                return null;
            }

        }
    }
}
