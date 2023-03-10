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
                protected BetterAIPlayerAI BAI_AI { get { return (BetterAIPlayer.BetterAIPlayerAI)Player?.AI; } }
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
                        if (Game.isTeamContact(Player.getTeam(), Game.getPlayerTeam(eLoopToPlayer)) && Game.isTeamAlive(Game.getPlayerTeam(eLoopToPlayer)))
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
                public override void reset(Player player, Unit.MovePriority eMovePriority, int saveOrders)
                {
                    base.reset(player, eMovePriority, saveOrders);
                    BAI_mmapStartingTileBlockableCitySiteValues.Clear();
                }
/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites       END ###
  ##############################################*/

                //lines 885
                protected override void calculateBlockableCitySiteValues()
                {
                    //using var profileScope = new UnityProfileScope("UnitRoleManager.calculateBlockableCitySiteValues");

                    foreach (int iTileID in BAI_AI.getValidCitySites())
                    {
/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites     START ###
  ##############################################*/
                        //don't bother defending free city sites at the start of the game with your only military unit, but allow using Militia for that purpose
                        if (((BetterAIPlayer)Player).getStartingTiles().Contains(iTileID) && 
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
                            mmapBlockableCitySiteValues.Add(pLoopCity.getTileID(), pLoopCity.getPopulation() * BAI_AI.getTileSettleValue(pLoopCity.tile()));
                        }
                    }
                }

                //line 5036
                //copy-paste START
                protected override void getUnitCitySiteGuardValues(PathFinder pPathfinder, Unit pUnit, int iMaxSteps, int iMaxTargets)
                {
                    //using var profileScope = new UnityProfileScope("UnitRoleManager.getUnitCitySiteGuardValues");
                    base.getUnitCitySiteGuardValues(pPathfinder, pUnit, iMaxSteps, iMaxTargets * 2);

/*####### Better Old World AI - Base DLL #######
  ### Don't defend free City Sites     START ###
  ##############################################*/
                    //don't defend starting tiles at the start of the game with your only military unit, but allow using Militia for defending free city sites.
                    if (!(AI.isEmergencyUnit(pUnit.getType())) || (Game.getTurn() >= ((Game.isGameOption(Infos.Globals.GAMEOPTION_PLAY_TO_WIN)) ? BAI_AI.AI_PLAY_TO_WIN_GRACE_TURNS : BAI_AI.AI_GRACE_TURNS)))
                    {
                        return;
                    }


                    //if (!pUnit.canDamage())
                    //{
                    //    return;
                    //}

                    UnitType eFoundUnit = ((BetterAIPlayer)Player).getCurrentFoundUnitType();

                    foreach (var p in BAI_mmapStartingTileBlockableCitySiteValues) //only the free starting city sites
                    {
                        int iCitySiteTileID = p.Key;
                        long iPriority = p.Value / 2; //we don't really need to defend it
                        Tile pCitySite = Game.tile(iCitySiteTileID);

                        if (eFoundUnit != UnitType.NONE)
                        {
                            if (!(pCitySite.canBothUnitsOccupy(eFoundUnit, Player.getPlayer(), pUnit))) //to prevent blocking a site with a Scout
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
                                else if (pUnit.canPathTo(pCitySite, iMaxSteps, true, false, false, Player.getTeam(), pPathfinder))
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
