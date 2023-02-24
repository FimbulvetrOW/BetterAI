using Mohawk.SystemCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using UnityEngine;

namespace BetterAI
{
    public class BetterAIClientRenderer : ClientRenderer
    {
        public BetterAIClientRenderer(ClientManager manager) : base(manager)
        { }

        protected override void drawTileOverlays()
        {
            base.drawTileOverlays();
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                  START ###
  ##############################################*/
            if (getActiveOverlay() == MapOverlayType.NONE && mManager.Selection.isSelectedUnit())
            {
                Unit pSelectedUnit = mManager.Selection.getSelectedUnit();
                if ((pSelectedUnit != null) && !mManager.Interfaces.Renderer.isUnitMoving(pSelectedUnit.getID()) && pSelectedUnit.hasIgnoreZOC(pSelectedUnit.tile()) && !isHideUnits())
                {
                    if (((BetterAIInfoUnit)pSelectedUnit.info()).bHasIngoreZOCBlocker)
                    {
                        Player pActivePlayer = mManager.activePlayer();
                        TeamType eActiveTeam = pActivePlayer.getTeam();
                        TeamType eVisibilityTeam = isShowAllMap() ? TeamType.NONE : eActiveTeam;

                        Color overlayColor = ColorManager.GetColor(mGame.infos().Globals.COLOR_HOSTILE_ZOC);

                        var zocTilesScoped = CollectionCache.GetHashSetScoped<int>();
                        HashSet<int> siZocTiles = zocTilesScoped.Value;

                        //ZOC only happens next to hostile units and cities
                        foreach (Unit unit in mGame.getUnits())
                        {
                            if (unit.isVisibleTo(pSelectedUnit.getTeam()))
                            {
                                if (mGame.isHostileUnit(pSelectedUnit.getTeam(), pSelectedUnit.getTribe(), unit))
                                {
                                    Tile pLoopTile = unit.tile();
                                    for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                                    {
                                        Tile pAdjacentTile = pLoopTile.tileAdjacent(eDirection);
                                        if (pAdjacentTile != null && !siZocTiles.Contains(pAdjacentTile.getID()))
                                        {
                                            if (getTileVisibility(pAdjacentTile) > VisibilityType.REVEALED)
                                            {
                                                if (pAdjacentTile.isDirectionHostileZOC(mInfos.utils().directionOpposite(eDirection), pSelectedUnit, pSelectedUnit.getTeam(), pSelectedUnit.getTribe(), eVisibilityTeam, false))
                                                {
                                                    drawTileOverlay(pAdjacentTile.getID(), pAdjacentTile.getWorldPosition(), overlayColor);
                                                    siZocTiles.Add(pAdjacentTile.getID());
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                    END ###
  ##############################################*/
        }


    }
}
