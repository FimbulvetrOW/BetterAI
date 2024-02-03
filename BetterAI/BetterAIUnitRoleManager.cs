using System;
using System.Collections.Generic;

using Mohawk.SystemCore;
using TenCrowns.GameCore;

namespace BetterAI
{
    public partial class BetterAIPlayer : Player
    {
        public partial class BetterAIPlayerAI : BetterAIPlayer.PlayerAI
        {
            public class BetterAIUnitRoleManager : BetterAIPlayer.BetterAIPlayerAI.UnitRoleManager
            {
                protected Dictionary<int, long> BAI_mmapStartingTileBlockableCitySiteValues = new Dictionary<int, long>();
                protected BetterAIPlayerAI BAI_AI { get; set; } = null;
                public BetterAIUnitRoleManager(Game game) : base(game)
                { }

/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites     START ###
  ##############################################*/
                protected virtual bool isAnyContactedPlayerSneaky()
                {
                    //isTeamContact(TeamType eIndex1, TeamType eIndex2)
                    for (PlayerType eLoopToPlayer = 0; eLoopToPlayer < Game.getNumPlayers(); eLoopToPlayer++)
                    {
                        if (Game.isTeamContact(BAI_AI.Team, Game.getPlayerTeam(eLoopToPlayer)) && Game.isTeamAlive(Game.getPlayerTeam(eLoopToPlayer)))
                        {
                            if (BAI_AI.isOtherPlayerSneaky(Game.player(eLoopToPlayer)))
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                }

                //lines 414-455
                public override void reset(Player player, Tribe tribe, Unit.MovePriority eMovePriority, int saveOrders, bool bReassign)
                {
                    BAI_AI = (BetterAIPlayerAI)(player?.AI ?? tribe?.AI);
                    base.reset(player, tribe, eMovePriority, saveOrders, bReassign);
                    BAI_mmapStartingTileBlockableCitySiteValues.Clear();
                }
/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites       END ###
  ##############################################*/

                //lines 886-902
                protected override void calculateBlockableCitySiteValues()
                {
                    //using var profileScope = new UnityProfileScope("UnitRoleManager.calculateBlockableCitySiteValues");

                    foreach (int iTileID in BAI_AI.getValidCitySites())
                    {
/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites     START ###
  ##############################################*/
                        //don't bother defending free city sites at the start of the game with your only military unit, but allow using Militia for that purpose
                        if (((BetterAIPlayer)BAI_AI.player).getStartingTiles().Contains(iTileID) && 
                            (Game.getTurn() < ((Game.isGameOption(Infos.Globals.GAMEOPTION_PLAY_TO_WIN)) ? BAI_AI.AI_PLAY_TO_WIN_GRACE_TURNS : BAI_AI.AI_GRACE_TURNS)) &&
                            !(isAnyContactedPlayerSneaky()))
                        {
                            BAI_mmapStartingTileBlockableCitySiteValues.Add(iTileID, BAI_AI.getTileSettleValue(Game.tile(iTileID)));
                        }
                        else
/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites       END ###
  ##############################################*/
                        {
                            mmapBlockableCitySiteValues.Add(iTileID, BAI_AI.getTileSettleValue(Game.tile(iTileID)));
                        }
                    }

                    foreach (City pLoopCity in Game.getCities())
                    {
                        if (BAI_AI.shouldOccupyCity(pLoopCity))
                        {
                            mmapBlockableCitySiteValues[pLoopCity.getTileID()] = pLoopCity.getPopulation() * AI.getTileSettleValue(pLoopCity.tile());
                        }
                    }
                }

                //lines 1016-1153
                protected override void assignAttackUnits(AttackTactics eTactics, AttackThreat eMinThreat, bool bExpansionOnly = false, bool bAllowFatigue = false, bool bPlayerOnly = false, bool bInPlaceOnly = false, int iMinPowerPercent = 0, int iMaxPowerPercent = int.MaxValue)
                {
                    //using var profileScope = new UnityProfileScope("UnitRoleManager.assignAttackUnits");

                    using var tileSetScoped = CollectionCache.GetHashSetScoped<int>();
                    HashSet<int> targetsChecked = tileSetScoped.Value;

                    for (int iTargetIndex = 0; iTargetIndex < maTargetUnitValues.Count; ++iTargetIndex)
                    {
                        TargetUnitValue val = maTargetUnitValues[iTargetIndex];
                        if (eTactics != AttackTactics.Approach && eTactics != AttackTactics.Individual)
                        {
                            if (msiAttackTargetsDone.Contains(val.miTargetID))
                            {
                                continue;
                            }
                        }
                        if (!msiAvailableUnits.Contains(val.miUnitID))
                        {
                            continue;
                        }
                        if (val.meThreat < eMinThreat)
                        {
                            continue;
                        }
                        int iTargetTile = val.miTargetID;
                        if (targetsChecked.Contains(iTargetTile))
                        {
                            continue;
                        }
                        Tile pTargetTile = Game.tile(iTargetTile);
                        if (pTargetTile == null)
                        {
                            continue;
                        }
                        if (pTargetTile.isCitySiteActive())
                        {
                            if (eTactics == AttackTactics.Kill)
                            {
                                continue; // don't clear settlements if you're not going to capture them
                            }
                        }
                        else if (pTargetTile.hasCity())
                        {
                            if (eTactics == AttackTactics.Kill)
                            {
                                continue;
                            }
                            else if (eTactics == AttackTactics.Capture)
                            {
                                if (!BAI_AI.isExpansionCityValid(pTargetTile.city()))
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (eTactics == AttackTactics.Capture)
                            {
                                continue;
                            }
                        }
                        if (val.IsFatigue && !bAllowFatigue)
                        {
                            continue;
                        }
                        if (eTactics == AttackTactics.Stun)
                        {
                            if (!val.mbStun)
                            {
                                continue;
                            }
                        }
                        if (eTactics == AttackTactics.Stun || eTactics == AttackTactics.Kill)
                        {
                            Unit pTargetUnit = pTargetTile.defendingUnit();
                            if (pTargetUnit != null && !pTargetUnit.AI.canAttackNextTurn(pTargetTile))
                            {
                                continue;
                            }
                        }

                        if (bPlayerOnly)
                        {
                            PlayerType eDefendingPlayer = pTargetTile.defendingUnit()?.getPlayer() ?? pTargetTile.getOwner();
                            if (eDefendingPlayer == PlayerType.NONE || !BAI_AI.getEnemyPlayers().Contains(eDefendingPlayer))
                            {
                                continue;
                            }
                        }

                        Tile pMoveTile = Game.tile(val.miMoveTileID);
                        Unit pUnit = Game.unit(val.miUnitID);

                        if (pUnit == null)
                        {
                            continue;
                        }

                        if (BAI_AI.mpAICache.isNoAttackTarget(val.miTargetID))
                        {
                            continue;
                        }

                        if (bInPlaceOnly)
                        {
                            if (val.miMoveTileID != pUnit.getTileID())
                            {
                                continue;
                            }
                        }

                        if (eTactics != AttackTactics.Approach)
                        {
                            if (!pUnit.AI.canTargetTileThisTurn(pTargetTile, pMoveTile))
                            {
                                continue;
                            }
                        }
                        if (bExpansionOnly)
                        {
                            if (!BAI_AI.getExpansionTargets().Contains(iTargetTile))
                            {
                                continue;
                            }
                        }

                        int iExtraDanger = 0;
                        if (eTactics == AttackTactics.Kill || eTactics == AttackTactics.Capture || eTactics == AttackTactics.Stun)
                        {
                            Unit pDefender = pTargetTile.defendingUnit();
                            if (pDefender != null)
                            {
                                iExtraDanger -= pDefender.attackUnitStrength(pDefender.tile(), pMoveTile, null, false);
                            }
                        }


                        if (iMinPowerPercent > 0)
                        {
/*####### Better Old World AI - Base DLL #######
  ### AI must not be timid when expanding START#
  ##############################################*/
                            if (bExpansionOnly && iMinPowerPercent > 100)
                            {
                                int iMin = 140;
                                Unit pDefender = pTargetTile.defendingUnit();
                                if (pDefender != null)
                                {
                                    if (!pDefender.AI.hasCentralAI()) //Tribes are easy. Go kill them.
                                    {
                                        if (pDefender.getTribe() != TribeType.NONE)
                                        {
                                            if (Game.getTribeAllyTeam(pDefender.getTribe()) != TeamType.NONE)
                                            {
                                                iMin = 125;
                                            }
                                            else
                                            {
                                                iMin = 110;
                                            }
                                        }
                                    }
                                }

                                iMinPowerPercent = Math.Min(iMin, iMinPowerPercent);
                            }
/*####### Better Old World AI - Base DLL #######
  ### AI must not be timid when expanding END###
  ##############################################*/
                            if (!pUnit.AI.isProtectedTile(pMoveTile, true, iMinPowerPercent, iExtraDanger))
                            {
                                continue;
                            }
                        }


                        if (iMaxPowerPercent < int.MaxValue)
                        {
                            if (pUnit.AI.isProtectedTile(pMoveTile, true, iMaxPowerPercent, iExtraDanger))
                            {
                                continue;
                            }
                        }
                        if (eTactics != AttackTactics.Kill && eTactics != AttackTactics.Capture && eTactics != AttackTactics.Approach)
                        {
                            if (!pUnit.AI.isProtectedTile(pMoveTile, true, iMinPowerPercent, iExtraDanger))
                            {
                                if (!bExpansionOnly && !pUnit.isHealPossibleTile(pMoveTile))
                                {
                                    continue;
                                }
                            }
                        }

                        if (assignUnitstoTarget(eTactics, pTargetTile, eMinThreat, !bAllowFatigue, iMinPowerPercent))
                        {
                            --iTargetIndex;
                        }
                        targetsChecked.Add(iTargetTile);
                    }
                }

                //line 5081-5189
                //copy-paste START
                protected override void getUnitCitySiteGuardValues(PathFinder pPathfinder, Unit pUnit, int iMaxSteps, int iMaxTargets)
                {
                    //using var profileScope = new UnityProfileScope("UnitRoleManager.getUnitCitySiteGuardValues");
                    base.getUnitCitySiteGuardValues(pPathfinder, pUnit, iMaxSteps, iMaxTargets * 2);

/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites     START ###
  ##############################################*/
                    //don't defend starting tiles at the start of the game with your only military unit, but allow using Militia for defending free city sites.
                    if (!(Infos.Helpers.isEmergencyUnit(pUnit.getType())) || (Game.getTurn() >= ((Game.isGameOption(Infos.Globals.GAMEOPTION_PLAY_TO_WIN)) ? BAI_AI.AI_PLAY_TO_WIN_GRACE_TURNS : BAI_AI.AI_GRACE_TURNS)))
                    {
                        return;
                    }


                    //if (!pUnit.canDamage())
                    //{
                    //    return;
                    //}

                    UnitType eFoundUnit = ((BetterAIPlayer)BAI_AI.player).getCurrentFoundUnitType();

                    foreach (var p in BAI_mmapStartingTileBlockableCitySiteValues) //only the free starting city sites
                    {
                        int iCitySiteTileID = p.Key;
                        long iPriority = p.Value / 2; //we don't really need to defend it
                        Tile pCitySite = Game.tile(iCitySiteTileID);

                        if (eFoundUnit != UnitType.NONE)
                        {
                            if (!(pCitySite.canBothUnitsOccupy(eFoundUnit, BAI_AI.getPlayer(), TribeType.NONE, pUnit))) //to prevent blocking a site with a Scout
                            {
                                continue; //probably break, right
                            }
                        }
/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites       END ###
  ##############################################*/

                        if (AI.canSetMoveTarget(pUnit.getID(), iCitySiteTileID))
                        {
                            //Tile pCitySite = Game.tile(iCitySiteTileID);
                            if (AI.isTileReachable(pCitySite, pUnit))
                            {
                                long iValue = 100;

                                {
                                    int iStrength = 1;

                                    // want to be able to attack without leaving the tile
                                    foreach (int iTargetTileID in BAI_AI.getAttackTargets())
                                    {
                                        Tile pTargetTile = Game.tile(iTargetTileID);
                                        if (pUnit.canTargetTile(pCitySite, pTargetTile))
                                        {
                                            iStrength += pUnit.attackUnitStrength(pCitySite, pTargetTile, pTargetTile.defendingUnit());
                                        }
                                    }

/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites     START ###
  ##############################################*/
                                    //if (pCitySite.hasCity() && pCitySite.city().getTeam() == Player.getTeam())
                                    //{
                                    //    iStrength += pUnit.defendUnitStrength(pCitySite, null);
                                    //}
                                    //else // just holding the tile
                                    //{
                                    //    if (AI.isEmergencyUnit(pUnit.getType()))
                                    //    {
                                    //        // free the stronger units for other jobs
                                            iStrength += 1;
                                    //    }
                                    //}
/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites       END ###
  ##############################################*/

                                    iValue *= iStrength;
                                }

                                if (!pUnit.at(pCitySite))
                                {
                                    if (pUnit.AI.isAvoidMove())
                                    {
                                        iValue /= 2;
                                    }
                                }

                                int iSteps;
                                if (pUnit.at(pCitySite))
                                {
                                    iSteps = 0;
                                }
                                else if (pUnit.canPathTo(pCitySite, iMaxSteps, true, false, false, BAI_AI.Team, pPathfinder))
                                {
                                    iSteps = pPathfinder.getNumStepsTo(pCitySite);
                                }
                                else
                                {
                                    continue;
                                }
                                TargetUnitValue zValue = new TargetUnitValue(iValue);
                                zValue.miAttackPriority = (int)iPriority;

/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites     START ###
  ##############################################*/
                                //zValue.mbDanger = pUnit.AI.isInGraveDanger(pCitySite, true);
                                //it's the free site within grace turns, so no grave danger

                                zValue.mbDanger = false;
/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites       END ###
  ##############################################*/

                                zValue.miOrders = pUnit.getNumOrdersForSteps(iSteps);
                                zValue.miMoveOrders = iSteps > pUnit.getStepsToFatigue() ? pUnit.getStepsToFatigue() : zValue.miOrders;
                                zValue.miDistance = pUnit.tile().distanceTile(pCitySite);
                                zValue.miTargetID = pCitySite.getID();
                                zValue.miMoveTileID = pCitySite.getID();
                                zValue.miUnitID = pUnit.getID();

/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites     START ###
  ##############################################*/
                                //I'm sure using Baseline is enough here
                                //if ((pCitySite.isCitySiteActive() && BAI_AI.isFoundCitySafe(pCitySite)) || (pCitySite.hasCity() && pCitySite.city().getTeam() != Player.getTeam()))
                                //{
                                //    zValue.meThreat = AttackThreat.Priority;
                                //}
                                //else
                                //{
                                    zValue.meThreat = AttackThreat.Baseline;
                                //}
/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites       END ###
  ##############################################*/

                                lock (maCitySiteGuardUnitValues)
                                {
                                    maCitySiteGuardUnitValues.Add(zValue);
                                }
                            }
                        }
                    }
                }
                //copy-paste END


            }
        }
    }
}
