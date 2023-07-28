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

        public bool mbAmphibious = false;
        public int miAmphibiousCount = 0;

        //lines 3989-4005
        protected override void changeEffectUnitDictionary(EffectUnitType eKey, int iChange)
        {
            base.changeEffectUnitDictionary(eKey, iChange);
            if (((BetterAIInfoEffectUnit)infos().effectUnit(eKey)).mbAmphibious)
            {
                miAmphibiousCount += iChange;
                if (iChange > 0)
                {
                    mbAmphibious = true;
                }
                else
                {
                    if (miAmphibiousCount <= 0)
                    {
                        mbAmphibious = false;
                        //miAmphibiousCount = 0;
                    }
                    else mbAmphibious = true; //iCount is > 0 anyway
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
            using (new UnityProfileScope("Unit.getMovementCost"))
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
                        if (mbAmphibious)
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
                    if (mbAmphibious)
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

        //copy-paste START
        //lines 6661-6697
        public override bool canSwapUnits(Tile pTile, Player pActingPlayer, bool bMarch)
        {
            if (!canAct(pActingPlayer, (1 + ((isFatigued()) ? infos().Globals.UNIT_FATIGUE_COST : 0))))
            {
                return false;
            }

            if (isFatigued() && !isMarch())
            {
                if ((bMarch) ? !canMarch(pActingPlayer) : true)
                {
                    return false;
                }
            }

            if (!canOccupyTile(pTile, getTeam(), bTestTheirUnits: true, bTestOurUnits: false))
            {
                return false;
            }

            if (!(pTile.isTileAdjacent(tile())))
            {
                return false;
            }

            if (tile().isHostileZOC(this) && pTile.isHostileZOC(this))
            {
                return false;
            }

/*####### Better Old World AI - Base DLL #######
  ### Fix Swap Unit Exploit            START ###
  ##############################################*/
            BetterAIUnit pUnit = (BetterAIUnit)swapUnit(pTile, pActingPlayer);
            if (pUnit == null)
            {
                return false;
            }
            else
            {
                if (pUnit.getStepsToFatigue() < ((BetterAIInfoGlobals)infos().Globals).BAI_SWAP_UNIT_FATIGUE_COST || pUnit.isMarch()) //getStepsToFatigue() is always at least 0
                {
                    return false;
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### Fix Swap Unit Exploit              END ###
  ##############################################*/

            return true;
        }

        //lines 6698-6721
        public override void swapUnits(Tile pTile, Player pActingPlayer, bool bMarch)
        {
            MohawkAssert.Assert(canSwapUnits(pTile, pActingPlayer, bMarch));

            int iCost = (1 + ((isFatigued()) ? infos().Globals.UNIT_FATIGUE_COST : 0));

            if (bMarch)
            {
                if (isFatigued() && !isMarch())
                {
                    march(pActingPlayer);
                }
            }

            BetterAIUnit pUnit = (BetterAIUnit)swapUnit(pTile, pActingPlayer);

            pUnit.setTileID(getTileID());

/*####### Better Old World AI - Base DLL #######
  ### Fix Swap Unit Exploit            START ###
  ##############################################*/

            pUnit.changeTurnSteps(((BetterAIInfoGlobals)infos().Globals).BAI_SWAP_UNIT_FATIGUE_COST);

/*####### Better Old World AI - Base DLL #######
  ### Fix Swap Unit Exploit              END ###
  ##############################################*/

            pUnit.wake();

            setTileID(pTile.getID());
            changeTurnSteps(1);
            act(pActingPlayer, iCost);
            wake();
        }
        //copy-paste END

        //lines 9063-9089
        public override bool canPillage(Tile pTile, Player pActingPlayer, bool bTestEnabled = true)
        {
            if (base.canPillage(pTile, pActingPlayer, bTestEnabled))
            {
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

                return true;
            }
            else return false;
        }

        //lines 10445-10488
        public override void buyTile(City pNewCity, YieldType eYield, Player pActingPlayer)
        {
            MohawkAssert.Assert(canBuyTile(tile(), pNewCity, eYield, pActingPlayer));

            Tile pTile = tile();

            City pOldCity = pTile.cityTerritory();

            int iTileCost = pNewCity.getBuyTileCost(pTile, eYield);

            using (var tileExpansionScoped = CollectionCache.GetListScoped<PairStruct<int, CityTerritory>>())
            using (var tileTerritoryScoped = CollectionCache.GetDictionaryScoped<int, CityTerritory>())
            {
/*####### Better Old World AI - Base DLL #######
  ### Border growth fix                START ###
  ##############################################*/
                if (pOldCity != null)
                {
                    tileExpansionScoped.Value.Add(PairStruct.Create(pTile.getID(), new CityTerritory(pNewCity)));
                    game().setTilesOwner(tileExpansionScoped.Value, !(game().areTeamsAllied(pOldCity.getTeam(), pNewCity.getTeam())));
                }
                else
/*####### Better Old World AI - Base DLL #######
  ### Border growth fix                  END ###
  ##############################################*/
                {
                    pTile.getExpansionTiles(tileExpansionScoped.Value, 0, new CityTerritory(pNewCity), TeamType.NONE, tileTerritoryScoped.Value);
                    game().setTilesOwner(tileExpansionScoped.Value, true);
                }

            }

            pNewCity.incrementBuyTileCount();
            pNewCity.player().incrementBuyTileCount();

            if ((pOldCity != null) && (pOldCity.getFamily() != pNewCity.getFamily()))
            {
                if (pOldCity.hasPlayer() && pOldCity.hasFamily())
                {
                    pOldCity.player().addMemoryFamily(infos().Globals.GAVE_AWAY_TILE_MEMORY_FAMILY, pOldCity.getFamily());
                }
                if (pNewCity.hasFamily())
                {
                    pNewCity.player().addMemoryFamily(infos().Globals.GIVEN_TILE_MEMORY_FAMILY, pNewCity.getFamily());
                }
            }

            pActingPlayer.changeYieldStockpileWhole(eYield, -(iTileCost));

            List<TileText> azTileTexts = null;
            act(pActingPlayer, infos().Globals.UNIT_BUY_TILE_COST, ref azTileTexts);
            doCooldown(infos().Globals.BUY_TILE_COOLDOWN);

            UnitBuyTileAction unitAction = new UnitBuyTileAction(getID());
            unitAction.meActingPlayer = pActingPlayer?.getPlayer() ?? PlayerType.NONE;
            unitAction.maTileTexts = azTileTexts;
            game().sendUnitAction(unitAction, true);
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

    }
}
