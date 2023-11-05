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
                public override void reset(Player player, Tribe tribe, Unit.MovePriority eMovePriority, int saveOrders)
                {
                    BAI_AI = (BetterAIPlayerAI)(player?.AI ?? tribe?.AI);
                    base.reset(player, tribe, eMovePriority, saveOrders);
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
                protected override void assignAttackUnits(AttackTactics eTactics, AttackThreat eMinThreat, bool bExpansionOnly, bool bNoFatigue, bool bPlayerOnly, int iMinPowerPercent, int iMaxPowerPercent = int.MaxValue)
                {
                    using var profileScope = new UnityProfileScope("UnitRoleManager.assignAttackUnits");

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
                        if (pTargetTile.isCitySiteActive() || pTargetTile.hasCity())
                        {
                            if (eTactics == AttackTactics.Kill)
                            {
                                continue; // don't clear settlements if you're not going to capture them
                            }
                        }
                        else
                        {
                            if (eTactics == AttackTactics.Capture)
                            {
                                continue;
                            }
                        }
                        if (val.IsFatigue && bNoFatigue)
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
                                                iMin = 120;
                                            }
                                            else
                                            {
                                                iMin = 100;
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
                            if (!pUnit.AI.isProtectedTile(pMoveTile, true, Unit.UnitAI.UNIT_HEAL_HEALTH_PERCENT, iExtraDanger))
                            {
                                if (!bExpansionOnly && !pUnit.isHealPossibleTile(pMoveTile))
                                {
                                    continue;
                                }
                            }
                        }

                        if (assignUnitstoTarget(eTactics, pTargetTile, eMinThreat, bNoFatigue, iMinPowerPercent))
                        {
                            --iTargetIndex;
                        }
                        targetsChecked.Add(iTargetTile);
                    }
                }

                //Less retreating, more attacking (Just an attempt, will probably be removed in the next update)
                //lines 2299-2519
                public override void assignUnitRoles(Player player, Tribe tribe, Unit.MovePriority ePriorityFlags, int saveOrders)
                {
                    using var profileScope = new UnityProfileScope("UnitRoleManager.assignUnitRoles");

                    reset(player, tribe, ePriorityFlags, saveOrders);
                    updateMoveTiles();
                    BAI_AI.updateUnitAttacksInPlace();
                    findAvailableUnits();
                    findTerrainTiles();
                    findBestRoadTiles();
                    calculateBlockableCitySiteValues();
                    calculateSortedUnitRoleValues();

                    TileType eClaimTileType = TileType.CapturedCity;
                    if (!AI.isWarDefending())
                    {
                        eClaimTileType |= TileType.CitySite;
                    }

                    bool bPlayerOnly = !ePriorityFlags.HasFlag(Unit.MovePriority.TribeKill);

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Military))
                    {
                        // stun first to focus on other threats
                        assignAttackUnits(AttackTactics.Stun, AttackThreat.CityThreat, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 100);

                        // push city threats to expose them
                        assignAttackUnits(AttackTactics.Push, AttackThreat.CityThreat, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 100);

                        // deal with city threats and captures, even if there is danger
                        assignAttackUnits(AttackTactics.Kill, AttackThreat.CityThreat, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 0, iMaxPowerPercent: 100);
                        assignAttackUnits(AttackTactics.Capture, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 100);

                        // kill other important targets
                        assignAttackUnits(AttackTactics.Stun, AttackThreat.Priority, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 75);
                        assignAttackUnits(AttackTactics.Push, AttackThreat.Priority, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 100);
                        assignAttackUnits(AttackTactics.Kill, AttackThreat.Priority, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 0);

                        // guard city sites and captured cities
                        assignCitySiteGuardUnits(AttackThreat.Priority, Unit.RoleType.CLAIM_TILE, Unit.SubRoleType.URGENT, bNoFatigue: true, eClaimTileType, iMinPowerPercent: 100);
                    }

/*####### Better Old World AI - Base DLL #######
  ### AI must ATTACK                   START ###
  ##############################################*/
                    //removing high priority retreating
                    //moved into Military NonCritical

                    //if (ePriorityFlags.HasFlag(Unit.MovePriority.Retreat) || ePriorityFlags.HasFlag(Unit.MovePriority.Military))
                    //{
                    //    // retreat from grave danger
                    //    assignRetreatUnits(bFromCombat: true, iMinPowerPercent: 75, false);
                    //}

/*####### Better Old World AI - Base DLL #######
  ### AI must ATTACK                     END ###
  ##############################################*/

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Improve) && !ePriorityFlags.HasFlag(Unit.MovePriority.NonCritical))
                    {
                        // finish wonders
                        assignWorkers(bBuyGoods: false, AttackThreat.Priority);
                    }

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Military) && ePriorityFlags.HasFlag(Unit.MovePriority.NonCritical))
                    {
                        // hit important targets
                        assignAttackUnits(AttackTactics.Group, AttackThreat.Priority, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 50);
                        assignAttackUnits(AttackTactics.Individual, AttackThreat.Priority, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 75);
                        // kill something, anything
                        assignAttackUnits(AttackTactics.Stun, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 100);
                        assignAttackUnits(AttackTactics.Push, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 100);
                        assignAttackUnits(AttackTactics.Kill, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: false, iMinPowerPercent: 75);

                        // cities in danger
                        assignCitySiteGuardUnits(AttackThreat.Baseline, Unit.RoleType.DEFEND_TILE, Unit.SubRoleType.URGENT, bNoFatigue: false, TileType.OurCity, iMinPowerPercent: 0, iMaxPowerPercent: 100);

                        // city sites that need march to claim
                        assignCitySiteGuardUnits(AttackThreat.Priority, Unit.RoleType.CLAIM_TILE, Unit.SubRoleType.NONE, bNoFatigue: AI.isWarDefending(), eClaimTileType, iMinPowerPercent: 100);

                        // kill with march
                        if (AI.canForceMarch())
                        {
                            assignAttackUnits(AttackTactics.Kill, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: false, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 100);
                        }


/*####### Better Old World AI - Base DLL #######
  ### AI must ATTACK                   START ###
  ##############################################*/
                        //removing high priority retreating
                        //instead, this is now just part of non-critical military

                        //if (ePriorityFlags.HasFlag(Unit.MovePriority.Retreat) || ePriorityFlags.HasFlag(Unit.MovePriority.Military))
                        {
                            // retreat from grave danger
                            assignRetreatUnits(bFromCombat: true, iMinPowerPercent: 75, false);
                        }
/*####### Better Old World AI - Base DLL #######
  ### AI must ATTACK                     END ###
  ##############################################*/
                        // move towards threats
                        assignAttackUnits(AttackTactics.Approach, AttackThreat.Priority, bExpansionOnly: false, bNoFatigue: false, bPlayerOnly: false, iMinPowerPercent: 0, iMaxPowerPercent: 100);

                        // protect threatened cities
                        assignCityDefendUnits(BAI_AI.AI_CITY_MIN_DANGER, Unit.RoleType.DEFEND_CITY, Unit.SubRoleType.NONE, bNoFatigue: false, iMinPowerPercent: 0);

                        // expansion targets are highest priority
                        if (!AI.isWarDefending())
                        {
/*####### Better Old World AI - Base DLL #######
  ### AI must not be timid when expanding START#
  ##############################################*/
                            //assignAttackUnits(AttackTactics.Group, AttackThreat.Baseline, bExpansionOnly: true, bNoFatigue: true, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 150);
                            //assignAttackUnits(AttackTactics.Individual, AttackThreat.Baseline, bExpansionOnly: true, bNoFatigue: true, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 150);
                            assignAttackUnits(AttackTactics.Group, AttackThreat.Baseline, bExpansionOnly: true, bNoFatigue: true, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 130);
                            assignAttackUnits(AttackTactics.Individual, AttackThreat.Baseline, bExpansionOnly: true, bNoFatigue: true, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 140);
/*####### Better Old World AI - Base DLL #######
  ### AI must not be timid when expanding END###
  ##############################################*/
                            assignAttackUnits(AttackTactics.Approach, AttackThreat.Baseline, bExpansionOnly: true, bNoFatigue: true, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 0);
                        }

                        // attack other targets
                        assignAttackUnits(AttackTactics.Group, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 150);
                        assignAttackUnits(AttackTactics.Individual, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 150);
                        assignAttackUnits(AttackTactics.Approach, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: false, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 100);

                        // forts outside our borders
                        assignFortUnits(Unit.RoleType.DEFEND_TILE, AttackThreat.Baseline, iMinPowerPercent: 100);

                        // retreat any units not retreated earlier
                        assignRetreatUnits(bFromCombat: true, iMinPowerPercent: 100, bInTheWayOnly: false);

/*####### Better Old World AI - Base DLL #######
  ### AI must ATTACK                   START ###
  ##############################################*/
                        //moved back to up here
                        // not retreating, so attack something, anything
                        assignAttackUnits(AttackTactics.Individual, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 0);
/*####### Better Old World AI - Base DLL #######
  ### AI must ATTACK                     END ###
  ##############################################*/

                    }


                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Settle))
                    {
                        assignFoundCityUnits();
                    }

                    if ((ePriorityFlags.HasFlag(Unit.MovePriority.Improve) && ePriorityFlags.HasFlag(Unit.MovePriority.NonCritical)) || ePriorityFlags.HasFlag(Unit.MovePriority.Automated))
                    {
                        // workers
                        assignChoppers(YieldShortage.Current);
                        assignWorkers(bBuyGoods: false, AttackThreat.Baseline);
                        assignChoppers(YieldShortage.Expected);
                        assignWorkers(bBuyGoods: true, AttackThreat.Baseline);
                        assignChoppers(YieldShortage.None);
                    }

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Religion))
                    {
                        // desciples
                        assignDesciples();
                    }

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Trade))
                    {
                        assignCaravans();
                    }

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Transport) && BAI_AI.getPlayer() != PlayerType.NONE)
                    {
                        // Pop goody huts nearby
                        assignCitySiteGuardUnits(AttackThreat.None, Unit.RoleType.BONUS_IMPROVEMENT, Unit.SubRoleType.NONE, bNoFatigue: false, TileType.Occupy, iMinPowerPercent: 100);
                    }

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Transport) && ePriorityFlags.HasFlag(Unit.MovePriority.Military) && ePriorityFlags.HasFlag(Unit.MovePriority.NonCritical))
                    {
                        if (!AI.isWarDefending())
                        {
                            // guard city sites we are not planning to settle
                            if (Game.isGameOption(Infos.Globals.GAMEOPTION_PLAY_TO_WIN))
                            {
                                assignCitySiteGuardUnits(AttackThreat.Baseline, Unit.RoleType.CLAIM_TILE, Unit.SubRoleType.NONE, bNoFatigue: true, TileType.CitySite, iMinPowerPercent: 100);
                            }

                            // Move towards a target
                            assignAttackUnits(AttackTactics.Approach, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: false, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 0);
                        }
                        else
                        {
                            assignAttackUnits(AttackTactics.Approach, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: false, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 50 + AI.getWarEnemyPowerPercent() / 2);
                        }

                        // kill any workers, if nothing else interesting is happening
                        assignAttackUnits(AttackTactics.Kill, AttackThreat.None, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: true, iMinPowerPercent: 100);

                        // retreat units not in mortal danger
                        assignRetreatUnits(bFromCombat: false, iMinPowerPercent: 100, bInTheWayOnly: false);

                        // garrison in-border forts
                        assignFortUnits(Unit.RoleType.DEFEND_TILE, AttackThreat.None, iMinPowerPercent: 100);

                        // guard potentially threatened cities
                        assignCityDefendUnits(0, Unit.RoleType.REINFORCE_CITY, Unit.SubRoleType.NONE, bNoFatigue: false, iMinPowerPercent: 100);

                        // Ships
                        assignWaterControlUnits();

                        // garrison empty cities
                        assignCityDefendUnits(-1, Unit.RoleType.GARRISON, Unit.SubRoleType.NONE, bNoFatigue: false, iMinPowerPercent: 100);

                        // Harrass opponents
                        assignRaidUnits();

/*####### Better Old World AI - Base DLL #######
  ### AI must ATTACK                   START ###
  ##############################################*/
                        //this part is moved back up to the other attacking stuff
                        // attack something, anything
                        //assignAttackUnits(AttackTactics.Individual, AttackThreat.Baseline, bExpansionOnly: false, bNoFatigue: true, bPlayerOnly: bPlayerOnly, iMinPowerPercent: 0);
/*####### Better Old World AI - Base DLL #######
  ### AI must ATTACK                     END ###
  ##############################################*/
                    }

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Transport))
                    {
                        // Workers
                        assignWorkerTransfer();

                        // Scouts
                        assignAgentUnits();
                        assignSentryUnits();
                    }

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Transport) || ePriorityFlags.HasFlag(Unit.MovePriority.Automated))
                    {
                        assignExploreUnits();
                    }

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Transport) && !ePriorityFlags.HasFlag(Unit.MovePriority.Military))
                    {
                        // sit on yield producing tiles
                        assignTileYieldProducers();
                    }

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Military) && ePriorityFlags.HasFlag(Unit.MovePriority.NonCritical))
                    {
                        assignUrgentMoveUnits();
                    }

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Military))
                    {
                        assignRetreatUnits(bFromCombat: false, iMinPowerPercent: 100, bInTheWayOnly: true);
                        assignUnitsInTheWay();
                    }

                    if (ePriorityFlags.HasFlag(Unit.MovePriority.Default))
                    {
                        foreach (int iUnitID in BAI_AI.getUnits())
                        {
                            if (msiAvailableUnits.Contains(iUnitID))
                            {
                                assignDefaultRoleIfUnassigned(Game.unit(iUnitID));
                            }
                        }
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
                    if (!(AI.isEmergencyUnit(pUnit.getType())) || (Game.getTurn() >= ((Game.isGameOption(Infos.Globals.GAMEOPTION_PLAY_TO_WIN)) ? BAI_AI.AI_PLAY_TO_WIN_GRACE_TURNS : BAI_AI.AI_GRACE_TURNS)))
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
                            if (!(pCitySite.canBothUnitsOccupy(eFoundUnit, BAI_AI.getPlayer(), pUnit))) //to prevent blocking a site with a Scout
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
