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
using static BetterAI.BetterAIInfos;
using System.Text.RegularExpressions;
using System.IO;

namespace BetterAI
{
    public partial class BetterAIUnit : Unit
    {
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
        //this whole DirtyType part is still opague to me, so I'm hoping this works without

        public bool mbAmphibiousEmbark = false;
        public int miAmphibiousEmbarkCount = 0;

        //lines 3989-4005
        protected override void changeEffectUnitDictionary(EffectUnitType eKey, int iChange)
        {
            base.changeEffectUnitDictionary(eKey, iChange);
            if (((BetterAIInfoEffectUnit)infos().effectUnit(eKey)).mbAmphibiousEmbark)
            {
                miAmphibiousEmbarkCount += iChange;
                if (iChange > 0)
                {
                    mbAmphibiousEmbark = true;
                }
                else
                {
                    if (miAmphibiousEmbarkCount <= 0)
                    {
                        mbAmphibiousEmbark = false;
                        //miAmphibiousCount = 0;
                    }
                    else mbAmphibiousEmbark = true;
                }
            }
        }
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement           END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### No Family for Enlisted Units     START ###
  ##############################################*/
        //lines 4438-4473
        public override Unit convert(PlayerType ePlayer, TribeType eTribe, bool bEnlisted = false)
        {
            Unit pUnit = base.convert(ePlayer, eTribe, bEnlisted);
            if (pUnit != null)
            {
                if ((ePlayer != PlayerType.NONE) && ((BetterAIInfoGlobals)infos().Globals).BAI_ENLIST_NO_FAMILY == 1)
                {
                    if (pUnit.hasFamily())
                    {
                        pUnit.setPlayerFamily(ePlayer, FamilyType.NONE);
                    }
                }
            }

            return pUnit;
        }
/*####### Better Old World AI - Base DLL #######
  ### No Family for Enlisted Units     START ###
  ##############################################*/

        //copy-pasted from Unit.cs START

        //lines 6143-6233
        // used by pathfinder
        public override int getMovementCost(Tile pFromTile, Tile pToTile, int iCostSoFar)
        {
            //using var profileScope = new UnityProfileScope("Unit.getMovementCost");
            //using (new UnityProfileScope("Unit.getMovementCost"))
            {
                int iCost = 0;

                if (pToTile.isWater() && !(info().mbWater))
                {
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
                    if (!pFromTile.isWater())
                    {
                        iCost += ((BetterAIInfoGlobals)infos().Globals).BAI_EMBARKING_COST_EXTRA;
                        if (mbAmphibiousEmbark)
                        {
                            iCost -= ((BetterAIInfoGlobals)infos().Globals).BAI_HARBOR_OR_AMPHIBIOUS_EMBARKING_DISCOUNT;
                        }
                        else
                        {
                            if (pToTile.getImprovement() != ImprovementType.NONE)
                            {
                                if (pToTile.improvement().mbRotateTowardsLand) //this is how I detect harbor-type Improvements. True only for Harbor and Colossus
                                {
                                    iCost -= ((BetterAIInfoGlobals)infos().Globals).BAI_HARBOR_OR_AMPHIBIOUS_EMBARKING_DISCOUNT;
                                }
                            }
                        }
                    }
                    if (getTeam() != TeamType.NONE)
                    {
                        if (pToTile.isWaterMovement(getTeam(), getTribe(), TeamType.NONE))
                        {
                            return Math.Max(iCost, movement());
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement           END ###
  ##############################################*/
                }

                DirectionType eDirection = pFromTile.getDirection(pToTile);

                bool bFriendly = (!(pFromTile.isHostileUnit(this)) && !(pToTile.isHostileUnit(this)));
                bool bRiver = pFromTile.isRiver(eDirection);

                //moved to top
                //int iCost = 0;

                if (pToTile.getTerrain() != TerrainType.NONE)
                {
                    iCost += pToTile.terrain().miMovementCost;
                }

                if (pToTile.getHeight() != HeightType.NONE && !hasIgnoreHeightCost(pToTile.getHeight()))
                {
                    iCost += pToTile.height().miMovementCost;
                }

                if (pToTile.hasVegetation())
                {
                    iCost += infos().vegetation(pToTile.getVegetation()).miMovementCost;
                }

                iCost += game().getTerrainCostBonus(pToTile.getTerrain());
                iCost += pToTile.getMovementCostExtra();

                bool bRiverCrossing = (bRiver && !pFromTile.isBridge(pToTile, getPlayer()));
                if (bRiverCrossing)
                {
                    iCost += infos().Globals.RIVER_CROSSING_COST_EXTRA;

/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
                    if (mbAmphibiousEmbark)
                    {
                        iCost -= ((BetterAIInfoGlobals)infos().Globals).BAI_AMPHIBIOUS_RIVER_CROSSING_DISCOUNT;
                    }
                    else
                    {
                        if (pFromTile.isRoad() && pToTile.isRoad())
                        {
                            if (getTeam() != TeamType.NONE && pFromTile.getTeam() != TeamType.NONE && pToTile.getTeam() != TeamType.NONE)
                            {
                                //if (getTeam() == pFromTile.getTeam() && getTeam() == pToTile.getTeam())
                                //if (game().areTeamsAllied(pFromTile.getTeam(), getTeam()) && game().areTeamsAllied(pToTile.getTeam(), getTeam()))
                                if (isAlliedWith(pFromTile.getTeam(), TribeType.NONE) && isAlliedWith(pToTile.getTeam(), TribeType.NONE))
                                {
                                    iCost -= ((BetterAIInfoGlobals)infos().Globals).BAI_TEAM_TERRITORY_ROAD_RIVER_CROSSING_DISCOUNT;
                                }
                            }
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement           END ###
  ##############################################*/
                }

                if (pToTile.isRiver())
                {
                    iCost += game().getRiverCostBonus();
                }

                if (bFriendly)
                {
                    if (infos().Globals.RIVER_MOVEMENT_COST != -1)
                    {
                        if (hasPlayer())
                        {
                            if (player().isRiverMovementUnlock() || (game().areTeamsAllied(pFromTile.getTeam(), getTeam()) && game().areTeamsAllied(pToTile.getTeam(), getTeam())))
                            {
                                if (pFromTile.isRiverMovement(pToTile, eDirection, this))
                                {
                                    iCost = Math.Min(iCost, infos().Globals.RIVER_MOVEMENT_COST + game().getRiverCostBonus());
                                }
                            }
                        }
                    }

                    if (!bRiverCrossing)
                    {
                        int iFinishedMovementFrom = 0;

                        if (pFromTile.isRoad())
                        {
                            iFinishedMovementFrom = infos().utils().modify(infos().Globals.ROAD_MOVEMENT_COST, getRoadMovementModifier());
                        }

                        int iFinishedMovementTo = 0;

                        if (pToTile.isRoad())
                        {
                            iFinishedMovementTo = infos().utils().modify(infos().Globals.ROAD_MOVEMENT_COST, getRoadMovementModifier());
                        }

                        if ((iFinishedMovementFrom != 0) && (iFinishedMovementTo != 0))
                        {
                            iCost = Math.Min(iCost, Math.Max(iFinishedMovementFrom, iFinishedMovementTo));
                        }
                    }
                }

                return iCost;
            }
        }
        //copy-pasted from Unit.cs END

/*####### Better Old World AI - Base DLL #######
  ### Better bounce tile search        START ###
  ##############################################*/
        public virtual bool isHiddenTileFrom(TeamType eTeam, Tile pTile, bool bSameArea)
        {

            if (bSameArea && pTile.getArea() != tile().getArea())
            {
                return false;
            }

            return base.isHiddenTileFrom(eTeam, pTile);
        }
        public virtual bool isHiddenTileFromAndCloser(TeamType eTeam, Tile pTestTile, HashSet<Tile> otherTiles, bool bSameArea = false)
        {
            if (pTestTile.impassable())
            {
                return false;
            }

            if (bSameArea && pTestTile.getArea() != tile().getArea())
            {
                return false;
            }

            if (!(isHiddenTileFrom(eTeam, pTestTile)))
            {
                return false;
            }

            if (!(isTileCloserOrTeamTerritory(eTeam, pTestTile, otherTiles)))
            {
                return false;
            }

            return true;
        }

        public virtual bool isTileCloserOrTeamTerritory(TeamType eTeam, Tile pTestTile, HashSet<Tile> otherTiles, bool bSameArea = false)
        {
            if (tile() == pTestTile)
            {
                return true;
            }

            if (pTestTile.impassable())
            {
                return false;
            }

            if (otherTiles == null || otherTiles.Count() == 0)
            {
                return true;
            }

            if (bSameArea) //for land units on water, or water units on land, bSameArea will always be false, so info().mbWater implies tile().isWater() here
            {
                if (info().mbWater ? pTestTile.getArea() != tile().getArea() : pTestTile.getLandSection() != tile().getLandSection())
                {
                    return false;
                }

                //no check for team territory here. Could have somewhat undesired results if city territory crosses blockers but that should be extremely rare.

                if (!(((BetterAITile)pTestTile).isPathShorterToOtherTiles(tile(), otherTiles)))
                {
                    return false;
                }
            }
            else
            {
                if (tile().getTeam() != TeamType.NONE && tile().getTeam() == getTeam())
                {
                    return true;
                }

                if (!(((BetterAITile)pTestTile).isDistanceCloserToOtherTiles(tile(), otherTiles)))
                {
                    return false;
                }
            }

            return true;
        }
/*####### Better Old World AI - Base DLL #######
  ### Better bounce tile search          END ###
  ##############################################*/

        //lines 6929-6939
        public override Tile bounceTile(TeamType eTeamTerritoryAvoid = TeamType.NONE, Tile pAvoidTile = null, Unit pIgnoreUnit = null)
        {
            if (((BetterAIInfoGlobals)infos().Globals).BAI_BETTER_BOUNCE == 0 || player() == null || player().getCities() == null || player().getCities().Count() == 0)
            {
                return base.bounceTile(eTeamTerritoryAvoid, pAvoidTile, pIgnoreUnit);
            }

/*####### Better Old World AI - Base DLL #######
  ### Better bounce tile search        START ###
  ##############################################*/
            //collect tile IDs of cities on the same land section / water area
            using (var citytileListScoped = CollectionCache.GetHashSetScoped<Tile>())
            {
                HashSet<Tile> aiCityTiles = citytileListScoped.Value;
                bool bSameArea = true;
                int iRequiresArea = tile().getArea();
                Tile pTile;

                bool switchOffAreaLimit()
                {
                    if (bSameArea)
                    {
                        bSameArea = false;
                        iRequiresArea = -1;

                        aiCityTiles.Clear();

                        //if we couldn't place it on the same continent, add all other remaining cities too
                        foreach (int iCityID in player().getCities())
                        {
                            Tile pLoopCityTile = game().city(iCityID).tile();
                            aiCityTiles.Add(pLoopCityTile);
                        }

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (tile().isLand())
                {
                    foreach (int iCityID in player().getCities())
                    {
                        Tile pLoopCityTile = game().city(iCityID).tile();
                        if (tile().getLandSection() == pLoopCityTile.getLandSection())
                        {
                            aiCityTiles.Add(pLoopCityTile);
                        }
                    }
                }
                else //isWater
                {
                    foreach (int iCityID in player().getCities())
                    {
                        foreach (int iLoopTileID in game().city(iCityID).getTerritoryTiles())
                        {
                            Tile pLoopTile = game().tile(iLoopTileID);
                            if (pLoopTile.isWater() && tile().getArea() == pLoopTile.getArea())
                            {
                                aiCityTiles.Add(pLoopTile);
                            }
                        }
                    }
                }
                if (aiCityTiles.Count() == 0 || tile().isWater() != info().mbWater)
                {
                    switchOffAreaLimit();
                }

                //Tile pTile = game().findUnitTileNearby(getType(), tile(), getPlayer(), getTribe(), eTeamTerritoryAvoid, false, false, -1, null, x => isHiddenTileFrom(TeamType.NONE, x));

                //first, try to stay in the same area
                do
                {
                    if (isHiddenTileFrom(TeamType.NONE, tile()))
                    {
                        pTile = game().findUnitTileNearby(getType(), tile(), getPlayer(), getTribe(), eTeamTerritoryAvoid, false, false, iRequiresArea, null, x => isHiddenTileFromAndCloser(TeamType.NONE, x, aiCityTiles, bSameArea: bSameArea), pAvoidTile: pAvoidTile, pIgnoreUnit: pIgnoreUnit);
                        if (pTile != null) return pTile;

                        pTile = game().findUnitTileNearby(getType(), tile(), getPlayer(), getTribe(), eTeamTerritoryAvoid, false, false, iRequiresArea, null, x => isHiddenTileFrom(TeamType.NONE, x), pAvoidTile: pAvoidTile, pIgnoreUnit: pIgnoreUnit);
                        if (pTile != null) return pTile;
                    }
                    else
                    {
                        pTile = game().findUnitTileNearby(getType(), tile(), getPlayer(), getTribe(), eTeamTerritoryAvoid, false, false, iRequiresArea, null, x => isTileCloserOrTeamTerritory(TeamType.NONE, x, aiCityTiles, bSameArea: bSameArea), pAvoidTile: pAvoidTile, pIgnoreUnit: pIgnoreUnit);
                        if (pTile != null) return pTile;

                        //units won't jump to different land masses/water bodies if there is any tile available, even if it's further away.
                        pTile = game().findUnitTileNearby(getType(), tile(), getPlayer(), getTribe(), eTeamTerritoryAvoid, false, false, iRequiresArea, null, predicate: null, pAvoidTile: pAvoidTile, pIgnoreUnit: pIgnoreUnit);
                        if (pTile != null) return pTile;
                    }
                } while (switchOffAreaLimit());

/*####### Better Old World AI - Base DLL #######
  ### Better bounce tile search          END ###
  ##############################################*/

                pTile = game().findUnitTileNearby(getType(), tile(), getPlayer(), getTribe(), eTeamTerritoryAvoid, false, false, -1, null, predicate: null, pAvoidTile: pAvoidTile, pIgnoreUnit: pIgnoreUnit);

                return pTile;
            }
        }

        //lines 6933-7061
        public override bool canTargetTile(Tile pFromTile, Tile pToTile)
        {
            if (base.canTargetTile(pFromTile, pToTile))
            {
                if (!info().mbMelee && getCooldown() == infos().Globals.ROUT_COOLDOWN && ((BetterAIInfoGlobals)infos().Globals).BAI_RANGED_UNIT_ROUTING_REQUIRES_MELEE_RANGE == 1)
                {
                    return pFromTile.isTileAdjacent(pToTile);
                }
                return true;
            }
            return false;
        }

        //lines 8008-8324
        public override void attackUnitOrCity(Tile pToTile, Player pActingPlayer)
        {
            MohawkAssert.Assert(canAttackUnitOrCity(pToTile, pActingPlayer));

            bool bSettlementAttack = pToTile.hasImprovementTribeSite();
            bool bCityAttack = canDamageCity(pToTile);
            bool bOwnerActing = pActingPlayer != null && pActingPlayer == player();

            BetterAIUnit pDefendingUnit = (BetterAIUnit)pToTile.defendingUnit();
            List<int> aiAdditionalDefendingUnits = null;
            List<AttackOutcome> aeAdditionalDefendingUnitOutcomes = null;
            City pCity = pToTile.city();

            int cityHpBeforeAttacked = -1;
            if (pCity != null)
            {
                cityHpBeforeAttacked = pCity.getHP();
            }

            game().startClientMessageQueuing();

            Tile pFromTile = tile();

            using (var unitListScoped = CollectionCache.GetListScoped<int>())
            using (var playerSetScoped = CollectionCache.GetHashSetScoped<PlayerType>())
            {
                pToTile.getAliveUnits(unitListScoped.Value);
                foreach (int iLoopUnit in unitListScoped.Value)
                {
                    Unit pLoopUnit = game().unit(iLoopUnit);
                    if (pLoopUnit.hasPlayer())
                    {
                        playerSetScoped.Value.Add(pLoopUnit.getPlayer());
                    }
                }
                if (pCity != null && pCity.hasPlayer())
                {
                    playerSetScoped.Value.Add(pCity.getPlayer());
                }
                for (PlayerType ePlayer = 0; ePlayer < game().getNumPlayers(); ++ePlayer)
                {
                    if (playerSetScoped.Value.Contains(ePlayer))
                    {
                        Player pLoopPlayer = game().player(ePlayer);
                        pFromTile.startVisibleTime(pLoopPlayer, 1);
                    }
                }
            }

            bool bEvent = false;

            List<TileText> azTileTexts = null;

            int iCounterDamage = getCounterAttackDamage((bCityAttack) ? null : pDefendingUnit, pToTile);

            int iKills = attackTile(pFromTile, pToTile, true, 100, pActingPlayer, ref azTileTexts, out AttackOutcome eOutcome, ref bEvent);

            if (bOwnerActing && (pDefendingUnit != null) && (eOutcome == AttackOutcome.NORMAL) && !bEvent)
            {
                bEvent = player().doEventTrigger(((isWaterAttack(pDefendingUnit)) ? infos().Globals.UNIT_COMBAT_WATER_EVENTTRIGGER : infos().Globals.UNIT_COMBAT_EVENTTRIGGER), this, pDefendingUnit);
            }

            bool bAdvance = canAdvanceAfterAttack(pFromTile, pToTile, pDefendingUnit, true, pActingPlayer);

            for (AttackType eLoopAttack = 0; eLoopAttack < infos().attacksNum(); eLoopAttack++)
            {
                int iValue = attackValue(eLoopAttack);
                if (iValue > 0)
                {
                    using (var tilesScoped = CollectionCache.GetListScoped<int>())
                    {
                        pFromTile.getAttackTiles(tilesScoped.Value, pToTile, getType(), eLoopAttack, iValue);

                        foreach (int iLoopTile in tilesScoped.Value)
                        {
                            Tile pLoopTile = game().tile(iLoopTile);

                            if (canDamageUnitOrCity(pLoopTile) && pLoopTile.isVisible(getTeam()))
                            {
                                BetterAIUnit pLoopDefendingUnit = (BetterAIUnit)pLoopTile.defendingUnit();

                                iKills += attackTile(pFromTile, pLoopTile, false, attackPercent(eLoopAttack), pActingPlayer, ref azTileTexts, out AttackOutcome eLoopOutcome, ref bEvent);

                                if (pLoopDefendingUnit != null)
                                {
                                    if (aiAdditionalDefendingUnits == null)
                                    {
                                        aiAdditionalDefendingUnits = new List<int>();
                                    }
                                    aiAdditionalDefendingUnits.Add(pLoopDefendingUnit.getID());
                                    if (aeAdditionalDefendingUnitOutcomes == null)
                                    {
                                        aeAdditionalDefendingUnitOutcomes = new List<AttackOutcome>();
                                    }
                                    aeAdditionalDefendingUnitOutcomes.Add(eLoopOutcome);
                                }
                            }
                        }
                    }
                }
            }

            changeDamageText(iCounterDamage, ref azTileTexts);

            if (info().mbMelee && pDefendingUnit != null)
            {
                if (pDefendingUnit.isFortify())
                {
                    pDefendingUnit.changeFortifyTurns(-1);
                }
                if (pDefendingUnit.isTestudo())
                {
                    pDefendingUnit.changeTestudoTurns(-1);
                }
            }

            if (hasStun(pToTile))
            {
                if ((pDefendingUnit != null) && pDefendingUnit.isAlive())
                {
                    pDefendingUnit.doCooldown(infos().Globals.STUNNED_COOLDOWN, bTestTurn: true, bForce: true);

                    game().addTileTextAllPlayers(ref azTileTexts, pToTile.getID(), () => TextManager.TEXT("TEXT_GAME_UNIT_STUNNED"));
                }
            }

            if (hasPush(pToTile))
            {
                using (var unitListScoped = CollectionCache.GetListScoped<int>())
                {
                    pToTile.getAliveUnits(unitListScoped.Value);

                    foreach (int iLoopUnit in unitListScoped.Value)
                    {
                        BetterAIUnit pLoopUnit = (BetterAIUnit)game().unit(iLoopUnit);

                        if (canDamageUnit(pLoopUnit))
                        {
                            Tile pPushTile = getPushTile(pLoopUnit, pFromTile, pToTile);
                            if (pPushTile != null)
                            {
                                pLoopUnit.setTileID(pPushTile.getID(), true, true, pActingPlayer, ref azTileTexts);
                                player()?.AI.clearLastSeenUnitState();

                                if ((pLoopUnit.getCooldown() == infos().Globals.UNLIMBERED_COOLDOWN) ||
                                    (pLoopUnit.getCooldown() == infos().Globals.ANCHORED_COOLDOWN))
                                {
                                    pLoopUnit.doCooldown(infos().Globals.ATTACKED_COOLDOWN, bForce: true);
                                }

/*####### Better Old World AI - Base DLL #######
  ### Protect against Null Ref         START ###
  ##############################################*/
                                //game().addTileTextAllPlayers(ref azTileTexts, pLoopUnit.getTileID(), () => TextManager.TEXT(infos().effectUnit(getPushEffectUnit()).mName));
                                game().addTileTextAllPlayers(ref azTileTexts, pLoopUnit.getTileID(), () => TextManager.TEXT("TEXT_EFFECTUNIT_PANIC"));
/*####### Better Old World AI - Base DLL #######
  ### Protect against Null Ref           END ###
  ##############################################*/
                            }
                            else
                            {
                                pLoopUnit.doCooldown(infos().Globals.STUNNED_COOLDOWN, bTestTurn: true, bForce: true);

                                game().addTileTextAllPlayers(ref azTileTexts, pLoopUnit.getTileID(), () => TextManager.TEXT("TEXT_GAME_UNIT_STUNNED"));
                            }
                        }
                    }
                }
            }

            act(pActingPlayer);

            if (getHP() > 0)
            {
                doXP(iKills, ref azTileTexts);

                if (bAdvance)
                {
                    setTileID(pToTile.getID(), true, true, pActingPlayer, ref azTileTexts);
                }

                if ((pDefendingUnit != null) && (pDefendingUnit.getHP() == 0) && ((bAdvance) ? canHaveRoutCooldown(pToTile, pToTile, pDefendingUnit, pActingPlayer) : ((pFromTile.hasCity() || pFromTile.isCitySiteActive() || canDamageUnit(pToTile) || pDefendingUnit.isAlive()) && pFromTile.isTileAdjacent(pToTile) && canHaveRoutCooldown(pToTile, pFromTile, pDefendingUnit, pActingPlayer))))
                {
                    doCooldown(infos().Globals.ROUT_COOLDOWN);

                    if (hasPlayer())
                    {
                        changeRoutChain(1);
                        if (bOwnerActing)
                        {
                            player().doEventTrigger(infos().Globals.UNIT_ROUT_EVENTTRIGGER, this);
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### Protect against Null Ref         START ###
  ##############################################*/
                    //game().addTileTextAllPlayers(ref azTileTexts, pFromTile.getID(), () => TextManager.TEXT(infos().effectUnit(getRoutEffectUnit()).mName));
                    game().addTileTextAllPlayers(ref azTileTexts, pFromTile.getID(), () => TextManager.TEXT("TEXT_EFFECTUNIT_ROUT"));
/*####### Better Old World AI - Base DLL #######
  ### Protect against Null Ref           END ###
  ##############################################*/
                }
                else
                {
                    if (getCooldown() == infos().Globals.ROUT_COOLDOWN)
                    {
                        changeRoutChain(1);
                        // don't allow infinite attacks after rout
                        setCooldown(infos().Globals.ATTACK_COOLDOWN);
                    }

                    doCooldown(infos().Globals.ATTACK_COOLDOWN, bTestTurn: true);
                }

                if (getRoutChain() >= 5)
                {
                    game().doAchievement(infos().getType<AchievementType>("ACHIEVEMENT_FIVE_ROUTS"), player());
                }
            }
            else
            {
                killBones();
            }

            if (hasPlayer())
            {
                pToTile.incrementRecentAttacks(getPlayer());

                if (pDefendingUnit != null)
                {
                    if (isHuman() && pDefendingUnit.isHuman())
                    {
                        game().incrementRecentHumanAttacks();
                    }

                    if (pDefendingUnit.isTribe())
                    {
                        player().addMemoryTribe(infos().Globals.TRIBE_ATTACKED_UNIT_MEMORY, pDefendingUnit.getTribe());
                    }
                    else
                    {
                        player().addMemoryPlayer(infos().Globals.PLAYER_ATTACKED_UNIT_MEMORY, pDefendingUnit.getPlayer());
                    }

                    if (pDefendingUnit.isTribe())
                    {
                        for (TeamType eLoopTeam = 0; eLoopTeam < game().getNumTeams(); eLoopTeam++)
                        {
                            if (game().isTeamAlive(eLoopTeam) && !(game().teamDiplomacy(getTeam(), eLoopTeam).mbHostile))
                            {
                                if (getTeam() != eLoopTeam)
                                {
                                    if ((pDefendingUnit.getRebelTeam() == eLoopTeam) || pDefendingUnit.isRaidTeam(eLoopTeam))
                                    {
                                        player().addMemoryTeam(infos().Globals.PLAYER_ATTACKED_ENEMY_MEMORY, eLoopTeam);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        for (TeamType eLoopTeam = 0; eLoopTeam < game().getNumTeams(); eLoopTeam++)
                        {
                            if (game().isTeamAlive(eLoopTeam) && !(game().teamDiplomacy(getTeam(), eLoopTeam).mbHostile))
                            {
                                if ((getTeam() != eLoopTeam) && (pDefendingUnit.getTeam() != eLoopTeam))
                                {
                                    if (game().isHostileUnit(eLoopTeam, TribeType.NONE, pDefendingUnit))
                                    {
                                        player().addMemoryTeam(infos().Globals.PLAYER_ATTACKED_ENEMY_MEMORY, eLoopTeam);
                                    }
                                }
                            }
                        }

                        for (TribeType eLoopTribe = 0; eLoopTribe < infos().tribesNum(); eLoopTribe++)
                        {
                            if (game().isDiplomacyTribeAlive(eLoopTribe) && !(game().tribeDiplomacy(eLoopTribe, getTeam()).mbHostile))
                            {
                                if (game().isHostileUnit(TeamType.NONE, eLoopTribe, pDefendingUnit))
                                {
                                    player().addMemoryTribe(infos().Globals.TRIBE_ATTACKED_ENEMY_MEMORY, eLoopTribe);
                                }
                            }
                        }
                    }
                }

                if (bCityAttack)
                {
                    if (pCity.isTribe())
                    {
                        player().addMemoryTribe(infos().Globals.TRIBE_ATTACKED_SETTLEMENT_MEMORY, pCity.getTribe());
                    }
                    else
                    {
                        player().addMemoryPlayer(infos().Globals.PLAYER_ATTACKED_CITY_MEMORY, pCity.getPlayer());
                    }

                    if (!(pCity.isTribe()))
                    {
                        for (TeamType eLoopTeam = 0; eLoopTeam < game().getNumTeams(); eLoopTeam++)
                        {
                            if (game().isTeamAlive(eLoopTeam) && !(game().teamDiplomacy(getTeam(), eLoopTeam).mbHostile))
                            {
                                if ((getTeam() != eLoopTeam) && (pCity.getTeam() != eLoopTeam))
                                {
                                    if (game().isHostileCity(eLoopTeam, TribeType.NONE, pCity))
                                    {
                                        player().addMemoryTeam(infos().Globals.PLAYER_ATTACKED_ENEMY_MEMORY, eLoopTeam);
                                    }
                                }
                            }
                        }

                        for (TribeType eLoopTribe = 0; eLoopTribe < infos().tribesNum(); eLoopTribe++)
                        {
                            if (game().isDiplomacyTribeAlive(eLoopTribe) && !(game().tribeDiplomacy(eLoopTribe, getTeam()).mbHostile))
                            {
                                if (game().isHostileCity(TeamType.NONE, eLoopTribe, pCity))
                                {
                                    player().addMemoryTribe(infos().Globals.TRIBE_ATTACKED_ENEMY_MEMORY, eLoopTribe);
                                }
                            }
                        }
                    }
                }
            }

            game().sendUnitBattleAction(this, pDefendingUnit, pFromTile, pToTile, pDefendingUnit?.tile(), eOutcome, azTileTexts, pActingPlayer?.getPlayer() ?? PlayerType.NONE, bSettlementAttack, cityHpBeforeAttacked, aiAdditionalDefendingUnits, aeAdditionalDefendingUnitOutcomes);    // send asap, since unit may have been removed
            game().doNetwork(); // to update health bars
            game().sendPendingClientMessages();
        }


        //lines 8432-8470
        public override Tile getPushTile(Unit pUnit, Tile pFromTile, Tile pToTile)
        {
            DirectionType eDirection = pFromTile.getDirection(pToTile);

            if (eDirection != DirectionType.NONE)
            {
                bool bPositive = (pFromTile.turnSeed() % 2 == 0);

/*####### Better Old World AI - Base DLL #######
  ### Push Tile: don't use bounce      START ###
  ##############################################*/
                //Tile pFirstTile = pToTile.tileAdjacent(eDirection);
                //Tile pSecondTile = pToTile.tileAdjacent(infos().utils().wrapDirection(eDirection, ((bPositive) ? 1 : -1)));
                //Tile pThirdTile = pToTile.tileAdjacent(infos().utils().wrapDirection(eDirection, ((bPositive) ? -1 : 1)));

                //if ((pFirstTile != null) && pFirstTile.canUnitOccupy(pUnit, TeamType.NONE, true, true, true, false))
                //{
                //    return pFirstTile;
                //}
                //else if ((pSecondTile != null) && pSecondTile.canUnitOccupy(pUnit, TeamType.NONE, true, true, true, false))
                //{
                //    return pSecondTile;
                //}
                //else if ((pThirdTile != null) && pThirdTile.canUnitOccupy(pUnit, TeamType.NONE, true, true, true, false))
                //{
                //    return pThirdTile;
                //}
                //else
                //{
                //    Tile pBounceTile = pUnit.bounceTile(pAvoidTile: pFromTile, pIgnoreUnit: this);

                //    if (pBounceTile != null)
                //    {
                //        if (pBounceTile.isTileAdjacent(pToTile))
                //        {
                //            return pBounceTile;
                //        }
                //    }
                //}


                Tile pPushTile = pToTile.tileAdjacent(eDirection);
                if ((pPushTile != null && pPushTile != pFromTile) && pPushTile.canUnitOccupy(pUnit, TeamType.NONE, true, true, true, false))
                {
                    return pPushTile;
                }
                for (int i = 1; i < (int)DirectionType.NUM_TYPES / 2; i++)
                {
                    pPushTile = pToTile.tileAdjacent(infos().utils().wrapDirection(eDirection, ((bPositive) ? i : -i)));
                    if ((pPushTile != null && pPushTile != pFromTile) && pPushTile.canUnitOccupy(pUnit, TeamType.NONE, true, true, true, false))
                    {
                        return pPushTile;
                    }

                    pPushTile = pToTile.tileAdjacent(infos().utils().wrapDirection(eDirection, ((bPositive) ? -i : i)));
                    if ((pPushTile != null && pPushTile != pFromTile) && pPushTile.canUnitOccupy(pUnit, TeamType.NONE, true, true, true, false))
                    {
                        return pPushTile;
                    }

                    bPositive = !bPositive;
                }
/*####### Better Old World AI - Base DLL #######
  ### Push Tile: don't use bounce        END ###
  ##############################################*/

            }
            return null;
        }


/*####### Better Old World AI - Base DLL #######
  ### Attack Heal                      START ###
  ##############################################*/
        public virtual int getAttackHeal(Tile pFromTile, Tile pToTile)
        {
            int iValue = 0;
            if (pFromTile != null && pToTile != null)
            {
                if (pFromTile.isTileAdjacent(pToTile) && pFromTile.isWater() == pToTile.isWater())
                {
                    foreach (EffectUnitType eLoopEffectUnit in getEffectUnits())
                    {
                        iValue += ((BetterAIInfoEffectUnit)infos().effectUnit(eLoopEffectUnit)).miHealAttack;
                    }
                }
            }

            return iValue;
        }
/*####### Better Old World AI - Base DLL #######
  ### Attack Heal                        END ###
  ##############################################*/

        //lines 8844-8898
        public override int getCounterAttackDamage(Tile pFromTile, Unit pToUnit, Tile pToTile)
        {
            //slightly rearranged to fit in Attack Heal
            if (pToUnit != null)
            {
                if (pToUnit.info().mbWater != info().mbWater)
                {
                    return 0;
                }

                if (!(pToUnit.canDamage()))
                {
                    return 0;
                }
            }

            if (getHP() == 0)
            {
                return 0;
            }

            int iValue = 0;

            if (pToUnit == null)
            {
                if (!(info().mbMelee))
                {
                    return 0;
                }

                iValue += infos().Globals.COUNTER_CITY_DAMAGE;
            }
            else if (info().mbMelee)
            {
                if ((pToUnit.info().mbUnlimber) ? pToUnit.isUnlimbered() : true)
                {
                    if (pToUnit.hasMeleeCounter() ||
                        pToUnit.isFortifyMax() ||
                        pToUnit.isTestudoMax())
                    {
                        iValue += pToUnit.attackUnitDamage(pFromTile, pToTile, this, false);
                    }
                    else
                    {
                        iValue += pToUnit.counterAttackMelee();
                    }
                }
            }

            if (getCooldown() == infos().Globals.ROUT_COOLDOWN)
            {
                iValue += infos().Globals.COUNTER_ROUT_DAMAGE;
            }

/*####### Better Old World AI - Base DLL #######
  ### Attack Heal                      START ###
  ##############################################*/
            int iAttackHeal = getAttackHeal(pFromTile, pToTile);
            if (iAttackHeal != 0)
            {
                iValue -= iAttackHeal;
                iValue = Math.Max(iValue, getHP() - getHPMax());
            }
/*####### Better Old World AI - Base DLL #######
  ### Attack Heal                        END ###
  ##############################################*/

            return Math.Min(iValue, (getHP() - 1));
        }

        //lines 9063-9089
        public override bool canPillage(Tile pTile, Player pActingPlayer, bool bTestEnabled = true)
        {
            if (base.canPillage(pTile, pActingPlayer, bTestEnabled))
            {
/*####### Better Old World AI - Base DLL #######
  ### No Raider Ships                  START ###
  ##############################################*/
                TribeType eTribe = getTribe();
                if (eTribe != TribeType.NONE && pTile.isWater() && !(info().mbWater) && infos().tribe(eTribe).mbWaterMove
                    && ((BetterAIInfoGlobals)infos().Globals).BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS > 0)
                {
                    //stolen from City.doDistantRaidTurn
                    if (game().getTurn() < (game().tribeLevel().miRaidStartTurn + ((BetterAIInfoGlobals)infos().Globals).BAI_RAIDER_WATER_PILLAGE_DELAY_TURNS))
                    {
                        return false;
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### No Raider Ships                    END ###
  ##############################################*/

                return true;
            }
            else return false;
        }

/*####### Better Old World AI - Base DLL #######
  ### Agent Network Cost Scaling       START ###
  ##############################################*/
        //lines 10543-10553
        public virtual int getAgentNetworkCost(City pCity)
        {
            int iCost = base.getAgentNetworkCost();

            iCost += ((BetterAIInfoGlobals)infos().Globals).BAI_AGENT_NETWORK_COST_PER_CULTURE_LEVEL * (((int)pCity.getCulture()) + pCity.getCultureStep() + 1);

            return iCost;
        }

        //lines 10554-10613
        public override bool canCreateAgentNetwork(Tile pTile, City pCity, Player pActingPlayer, bool bTestEnabled = true)
        {
            bool bNoTest = base.canCreateAgentNetwork(pTile, pCity, pActingPlayer, false);
            if (!bNoTest)
            {
                return false;
            }
            else
            {
                if (bTestEnabled)
                {
                    if (pActingPlayer.getMoneyWhole() < getAgentNetworkCost(pCity)) //now with a parameter
                    {
                        return false;
                    }

                    if (!canUseUnit(pActingPlayer))
                    {
                        return false;
                    }

                    if (!canAct(pActingPlayer, 0))
                    {
                        return false;
                    }

                    if (!(player().isAgentUnlock()))
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        //lines 10618-10628
        public override void createAgentNetwork(City pCity, Player pActingPlayer)
        {
            MohawkAssert.Assert(canCreateAgentNetwork(tile(), pCity, pActingPlayer));

            pActingPlayer.changeMoneyWhole(-(getAgentNetworkCost(pCity)));

            pCity.setAgentTurn(getPlayer(), getAgentNetworkTurns());
            pCity.setAgentTileID(getPlayer(), getTileID());

            kill();
        }
/*####### Better Old World AI - Base DLL #######
  ### Agent Network Cost Scaling         END ###
  ##############################################*/

        public override bool canUpgradeFromToUnit(UnitType eFromUnit, UnitType eToUnit, int iTimeout = 0)
        {
/*####### Better Old World AI - Base DLL #######
  ### Circular Upgrades                START ###
  ##############################################*/
            if (eFromUnit == eToUnit)
            {
                return false;
            }

            //if (iTimeout > 100)
            //{
            //    MohawkAssert.Assert(false, "unit upgrade circular reference");
            //    return false;
            //}


            //if (infos().unit(eFromUnit).maeUpgradeUnit.Contains(eToUnit))
            //{
            //    return true;
            //}
            
            //if (infos().unit(eFromUnit).maeDirectUpgradeUnit.Contains(eToUnit))
            //{
            //    return true;
            //}

            ////tribe upgrade moved to the end

            //foreach (UnitType eLoopUnit in infos().unit(eFromUnit).maeUpgradeUnit)
            //{
            //    if (canUpgradeFromToUnit(eLoopUnit, eToUnit, (iTimeout + 1)))
            //    {
            //        return true;
            //    }
            //}

            if (((BetterAIInfoUnit)infos().unit(eFromUnit)).mseUpgradeUnitAccumulated.Contains(eToUnit))
            {
                return true;
            }
/*####### Better Old World AI - Base DLL #######
  ### Circular Upgrades                  END ###
  ##############################################*/

            if (hasOriginalTribe())
            {
                if (infos().unit(eFromUnit).maeTribeUpgradeUnit[getOriginalTribe()] == eToUnit)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
