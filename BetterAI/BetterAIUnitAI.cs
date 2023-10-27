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
                using var profileScope = new UnityProfileScope("UnitAI.shouldPillage");

                if (!pTile.canUnitOccupy(unit, TeamType.NONE, false, false, true))
                {
                    return false;
                }

                if (!unit.canPillage(pTile, actingPlayer))
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

                if (pTile.isWater() && !unit.info().mbWater && (((BetterAIInfoGlobals)(infos.Globals)).BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS) == 0)
                {
                    return false;
                }

                return true;
            }

            //lines 1460-1507
            public override bool doTribeCaptureSite(PathFinder pPathfinder, int iMaxSteps)
            {
                using var profileScope = new UnityProfileScope("UnitAI.doTribeCaptureSite");

/*####### Better Old World AI - Base DLL #######
  ### Attack cities and defended sites START ###
  ##############################################*/
                //bool shouldCaptureTile(Tile pTile)
                //{
                //    if (pTile.isHostileUnit(unit))
                //    {
                //        return true;
                //    }
                //    TribeType eTribe = pTile.getTribeSettlementOrRuins();
                //    if (eTribe != TribeType.NONE)
                //    {
                //        if (game.isHostileUnit(TeamType.NONE, eTribe, unit))
                //        {
                //            return true;
                //        }
                //    }
                //    return false;
                //}

                bool shouldCaptureTile(Tile pTile)
                {
                    if (pTile.isSettlement())
                    {
                        if (pTile.isHostileUnit(unit))
                        {
                            return true;
                        }
                    }

                    {
                        TribeType eTribe = pTile.getTribeSettlementOrRuins();
                        if (eTribe != TribeType.NONE)
                        {
                            if (pTile.defendingUnit() == null || game.isHostileUnitUnit(pTile.defendingUnit(), unit))
                            {
                                return true;
                            }
                        }
                        //not sure if we need to have tribes opportunistically grab (non-start, non-reserved) active sites without cities or ruins
                        else if ((pTile.getCitySite() == CitySiteType.ACTIVE))
                        {
                            if (pTile.defendingUnit() == null)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }
/*####### Better Old World AI - Base DLL #######
  ### Attack cities and defended sites   END ###
  ##############################################*/

                int iRange = iMaxSteps * unit.movement();
                using (var targetTileListScoped = CollectionCache.GetListScoped<int>())
                {
                    unit.tile().getTilesInRange(iRange, targetTileListScoped.Value);

                    foreach (int iTileID in targetTileListScoped.Value)
                    {
                        Tile pTile = game.tile(iTileID);

                        if (shouldCaptureTile(pTile))
                        {
                            if (canMoveTo(pPathfinder, pTile, iMaxSteps))
                            {
                                setMoveTile(pTile.getID());
                                Target = pTile.getID();
                                Role = RoleType.MOVE_TO_ATTACK;

                                if (tribeMoveToTarget(pPathfinder))
                                {
                                    return true;
                                }
                            }
                        }
                    }
                }
                return false;
            }

            //lines 6099-6326
            public override int movePriorityCompare(Unit pOther)
            {
                if (Role == RoleType.KILL_TARGET && Role == pOther.AI.Role)
                {
                    if (Target == pOther.AI.Target)
                    {

                        Tile pTargetTile = getTargetTile();
                        if (pTargetTile != null)
                        {
/*####### Better Old World AI - Base DLL #######
  ### AI fix: Enlister attacks last    START ###
  ##############################################*/
                            //// push last
                            //bool bPush = unit.hasPush(pTargetTile);
                            //if (bPush != pOther.hasPush(pTargetTile))
                            //{
                            //    return bPush ? 1 : -1;
                            //}

                            // try to enlist by attacking last
                            int iCaptureChance = ((BetterAIUnit)unit).getEnlistOnKillChance(pTargetTile);
                            int iOtherCaptureChance = ((BetterAIUnit)pOther).getEnlistOnKillChance(pTargetTile);
/*####### Better Old World AI - Base DLL #######
  ### AI fix: Enlister attacks last      END ###
  ##############################################*/
                            if (iCaptureChance != iOtherCaptureChance)
                            {

                                //check for things that have higher priority in movePriorityCompare
                                {
                                    bool bInTheWay = isInTheWay();
                                    if (bInTheWay != pOther.AI.isInTheWay())
                                    {
                                        return bInTheWay ? -1 : 1;
                                    }
                                }

                                if ((SubRole == SubRoleType.URGENT) != (pOther.AI.SubRole == SubRoleType.URGENT))
                                {
                                    return SubRole == SubRoleType.URGENT ? -1 : 1;
                                }

                                //if (checkRole(pOther, RoleType.SHOCK_TARGET))
                                //{
                                //    return Role == RoleType.SHOCK_TARGET ? -1 : 1;
                                //}

                                //if (checkRole(pOther, RoleType.KILL_TARGET))
                                //{
                                //    return Role == RoleType.KILL_TARGET ? -1 : 1;
                                //}

                                //if (Role == pOther.AI.Role)
                                {
                                    if (checkSubRole(pOther, SubRoleType.ADVANCE))
                                    {
                                        return SubRole == SubRoleType.ADVANCE ? 1 : -1;
                                    }
                                    if (checkSubRole(pOther, SubRoleType.SUPPORT))
                                    {
                                        return SubRole == SubRoleType.SUPPORT ? -1 : 1;
                                    }
                                    if (checkSubRole(pOther, SubRoleType.NEEDS_SUPPORT))
                                    {
                                        return SubRole == SubRoleType.NEEDS_SUPPORT ? 1 : -1;
                                    }
                                    if (SubRole == SubRoleType.NEEDS_SUPPORT && pOther.AI.SubRole == SubRoleType.NEEDS_SUPPORT)
                                    {
                                        bool bInPlace = (MoveTile == unit.getTileID());
                                        if (bInPlace != (pOther.AI.MoveTile == pOther.getTileID()))
                                        {
                                            return bInPlace ? 1 : -1;
                                        }
                                    }
                                }
                                //end of check for higher priority comparisons

                                return iCaptureChance - iOtherCaptureChance;
                            }
                        }
                    }
                }

                return base.movePriorityCompare(pOther);
            }


        }
    }

}
