using System;
using System.Collections.Generic;
using Mohawk.SystemCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using UnityEngine;
using TenCrowns.ClientCore;

namespace BetterAI
{
    public class BetterAIClientRenderer : ClientRenderer
    {
        public BetterAIClientRenderer(IApplication app) : base(app)
        {
        }

        protected override void drawTileOverlays()
        {
            //UnityEngine.Debug.Log("ClientRenderer.drawTileOverlays");
            MapOverlayType activeOverlay = getActiveOverlay();
            if (activeOverlay != MapOverlayType.ROADS_RIVERS)
            {
                base.drawTileOverlays();
                return;
            }
            //UnityEngine.Debug.Log("drawTileOverlays with MapOverlayType.ROADS_RIVERS - Start");

            //using (new UnityProfileScope("ClientRenderer.drawTileOverlays"))
            {
                Player pActivePlayer = ClientMgr.activePlayer();
                TeamType eActiveTeam = pActivePlayer.getTeam();
                TeamType eVisibilityTeam = isShowAllMap() ? TeamType.NONE : eActiveTeam;
                Unit pSelectedUnit = ClientMgr.Selection.getSelectedUnit();
                City pSelectedCity = ClientMgr.Selection.getSelectedCity();

                //clear tile overlays using pooling
                //using (new UnityProfileScope("ClientRenderer.drawTileOverlays.clear"))
                {
                    removeTileOverlays();
                }

                if (ClientMgr.UI.RoadToActive && pSelectedUnit != null && ClientMgr.Selection.getMouseoverTile() != null)
                {
                    using (var roadTileListScope = CollectionCache.GetListScoped<Tile>())
                    {
                        ClientMgr.roadPathfinder().GetRoadPath(pSelectedUnit.tile(), ClientMgr.Selection.getMouseoverTile(), pActivePlayer, roadTileListScope.Value);
                        foreach (Tile pTile in roadTileListScope.Value)
                        {
                            if (pTile == null)
                                break;

                            ColorType eColor = Game.infos().Globals.COLOR_CITY_CONNECTION_STRONG;
                            drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), ColorManager.GetColor(eColor));
                        }
                    }
                }

                if (ClientMgr.UI.getActiveMinimizedDecision() is PlaceBonusDecision pDecision)
                {
                    ImprovementType eImprovement = pDecision.getImprovement();
                    if (eImprovement != ImprovementType.NONE)
                    {
                        Tile pMouseoverTile = ClientMgr.Selection.getMouseoverTile();
                        if (pMouseoverTile != null && pMouseoverTile.canHaveImprovement(eImprovement, Game.city(pDecision.getCity()), ClientMgr.getActiveTeam(), bForceImprovement: true))
                        {
                            drawTileOverlay(pMouseoverTile.getID(), pMouseoverTile.getWorldPosition(), Color.white);

                            if (Infos.improvement(eImprovement).mbUrban || Infos.improvement(eImprovement).mbSpreadsBorders)
                                ClientMgr.UI.updateExpansionPreview(pMouseoverTile, Game.city(pDecision.getCity()), true);
                        }
                        else
                        {
                            ClientMgr.UI.updateExpansionPreview(null, null, false);
                        }
                    }
                    else
                    {
                        ResourceType eResource = pDecision.getResource();
                        if (eResource != ResourceType.NONE)
                        {
                            City pCity = Game.city(pDecision.getCity());

                            if (pCity != null)
                            {
                                foreach (int iTile in pCity.getTerritoryTiles())
                                {
                                    Tile pTile = Game.tile(iTile);

                                    if (ClientMgr.UI.hasValidResourceTile() ? pTile.canAddResource(eResource) : pTile.isResourceValid(eResource))
                                    {
                                        Color tileColor = (pTile == ClientMgr.Selection.getMouseoverTile()) ? Color.white : Color.white.SetAlpha(0.3f);
                                        drawTileOverlay(iTile, pTile.getWorldPosition(), tileColor);
                                    }
                                }
                            }
                        }
                    }
                }

                if (ClientMgr.UI.MissionOverlay != MissionType.NONE)
                {
                    //using var profileScope = new UnityProfileScope("ClientRenderer.drawTileOverlays.HighlightMissionTargets");

                    SubjectType eTarget = ClientMgr.Infos.mission(ClientMgr.UI.MissionOverlay).meSubjectTarget;
                    if (eTarget != SubjectType.NONE)
                    {
                        Color color = ColorManager.GetColor(Game.infos().Globals.COLOR_SELECTION);
                        SubjectClassType eSubjectClass = ClientMgr.Infos.subject(eTarget).meClass;

                        for (int i = 0; i < Game.getNumTiles(); i++)
                        {
                            Tile pTile = Game.tile(i);
                            if (pTile != null)
                            {
                                if (eSubjectClass == ClientMgr.Infos.Globals.UNIT_SUBJECTCLASS)
                                {
                                    Unit pUnit = pTile.topUnitVisible();

                                    if (pUnit != null)
                                    {
                                        if (pActivePlayer.canStartMission(ClientMgr.UI.MissionOverlay, ClientMgr.UI.MissionSubject, pUnit.getID().ToStringCached()))
                                        {
                                            drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color);
                                        }
                                    }
                                }
                                else if (eSubjectClass == ClientMgr.Infos.Globals.TILE_SUBJECTCLASS)
                                {
                                    if (pActivePlayer.canStartMission(ClientMgr.UI.MissionOverlay, ClientMgr.UI.MissionSubject, pTile.getID().ToStringCached()))
                                    {
                                        drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color);
                                    }
                                }
                                else if (eSubjectClass == ClientMgr.Infos.Globals.CITY_SUBJECTCLASS)
                                {
                                    City pCity = pTile.city();

                                    if (pCity != null)
                                    {
                                        if (pActivePlayer.canStartMission(ClientMgr.UI.MissionOverlay, ClientMgr.UI.MissionSubject, pCity.getID().ToStringCached()))
                                        {
                                            drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                {
                    //MapOverlayType activeOverlay = getActiveOverlay();

                    {
                        //draw occurrence tile overlays
                        //using (new UnityProfileScope("ClientRenderer.drawTileOverlays.occurrenceOverlays"))
                        {
                            int iHightlightOccurrenceID = ClientMgr.Selection.getMouseoverOccurrenceID();
                            if (iHightlightOccurrenceID != -1)
                            {
                                OccurrenceData pOccurrenceData = Game.getOccurrenceData(iHightlightOccurrenceID);
                                Color color = ColorManager.GetColor(Game.infos().Globals.COLOR_WHITE);
                                bool bFound = false;

                                if (pOccurrenceData.hasAffectedTiles())
                                {
                                    bFound = true;
                                    foreach (int iLoopTile in pOccurrenceData.msiAffectedTileIDs)
                                    {
                                        Tile pLoopTile = Game.tileBoundary(iLoopTile);
                                        drawTileOverlay(iLoopTile, pLoopTile.getWorldPosition(), color.SetAlpha(0.2f));
                                    }
                                }
                                if (pOccurrenceData.hasModifiedTiles())
                                {
                                    bFound = true;
                                    Color modifiedColor = ColorManager.GetColor(Game.infos().Globals.COLOR_DANGER);
                                    using (var tilesScoped = CollectionCache.GetHashSetScoped<int>())
                                    {
                                        pOccurrenceData.getModifiedTileIDs(Game, tilesScoped.Value);
                                        foreach (int iLoopTile in tilesScoped.Value)
                                        {
                                            Tile pLoopTile = Game.tileBoundary(iLoopTile);
                                            drawTileOverlay(iLoopTile, pLoopTile.getWorldPosition(), modifiedColor.SetAlpha(0.2f));
                                        }
                                    }
                                }

                                if (bFound)
                                    return;
                            }
                        }
                    }

                    //if (activeOverlay != MapOverlayType.NONE)
                    {
                        //switch (activeOverlay)
                        //{
                        //    case MapOverlayType.DANGER:
                        //        using (new UnityProfileScope("ClientRenderer.drawTileOverlays.showDanger"))
                        //        {
                        //            using var tileScope = CollectionCache.GetHashSetScoped<Tile>();

                        //            foreach (Unit pUnit in Game.getUnits())
                        //            {
                        //                if (Game.isHostileUnit(eActiveTeam, TribeType.NONE, pUnit) && isVisibleUnit(pUnit))
                        //                {
                        //                    if (pUnit.AI.canAttackNextTurn(pUnit.tile()))
                        //                    {
                        //                        Tile pTile = pUnit.tile();
                        //                        int iRange = ((pUnit.info().mbMelee) ? 1 : pUnit.rangeMax(pTile));

                        //                        if (iRange > 0)
                        //                        {
                        //                            using var listScoped = CollectionCache.GetListScoped<int>();
                        //                            pTile.getTilesInRange(iRange, listScoped.Value);

                        //                            foreach (int tileID in listScoped.Value)
                        //                            {
                        //                                Tile pLoopTile = Game.tile(tileID);

                        //                                if (pLoopTile.isRevealed(eActiveTeam))
                        //                                {
                        //                                    if (pUnit.canTargetTile(pLoopTile))
                        //                                    {
                        //                                        tileScope.Value.Add(pLoopTile);
                        //                                    }
                        //                                }
                        //                            }
                        //                        }
                        //                    }
                        //                }
                        //            }

                        //            foreach (Tile pLoopTile in tileScope.Value)
                        //            {
                        //                drawTileOverlay(pLoopTile.getID(), pLoopTile.getWorldPosition(), ColorManager.GetColor(Game.infos().Globals.COLOR_DANGER));
                        //            }
                        //        }
                        //        break;
                        //    case MapOverlayType.NETWORK:
                        //        using (new UnityProfileScope("ClientRenderer.drawTileOverlays.showNetwork"))
                        //        {
                        //            for (int i = 0; i < Game.getNumTiles(); i++)
                        //            {
                        //                Tile pTile = Game.tileBoundary(i);
                        //                if ((pTile != null) && !pTile.impassable())
                        //                {
                        //                    if (pTile.onTradeNetworkCapital(pActivePlayer.getPlayer()))
                        //                    {
                        //                        ColorType eColor = (pTile.hasCity()) ? Game.infos().Globals.COLOR_CAPITAL_CONNECTION_STRONG : Game.infos().Globals.COLOR_CAPITAL_CONNECTION;
                        //                        drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), ColorManager.GetColor(eColor));
                        //                    }
                        //                    else if (pTile.onTradeNetworkAny(pActivePlayer.getTeam()))
                        //                    {
                        //                        ColorType eColor = (pTile.hasCity()) ? Game.infos().Globals.COLOR_CITY_CONNECTION_STRONG : Game.infos().Globals.COLOR_CITY_CONNECTION;
                        //                        drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), ColorManager.GetColor(eColor));
                        //                    }
                        //                }
                        //            }
                        //        }
                        //        break;
                        //    case MapOverlayType.ROADS_RIVERS:
                        {
                            {

                                //using (new UnityProfileScope("ClientRenderer.drawTileOverlays.showRoadsRivers"))
                                {

                                    for (int i = 0; i < Game.getNumTiles(); i++)
                                    {
                                        Tile pTile = Game.tile(i);
                                        if (pTile != null)
                                        {
                                            if (pTile.isRoad() || pTile.hasCity())
                                            {
                                                drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), ColorManager.GetColor(Game.infos().Globals.COLOR_ROAD_CONNECTION));
                                            }

/*####### Better Old World AI - Base DLL #######
  ### River Edge Highlight             START ###
  ##############################################*/
                                            //else if (pTile.isRiver())
                                            //{
                                            //    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), ColorManager.GetColor(Game.infos().Globals.COLOR_RIVER_CONNECTION));
                                            //}

                                            //draw on river edges instead
                                            //handled by drawTileSelectionEdges


                                        }
                                    }

/*####### Better Old World AI - Base DLL #######
  ### River Edge Highlight               END ###
  ##############################################*/
                                }

                            }
                        }
                        //        break;
                        //    case MapOverlayType.IDLE:
                        //        using (new UnityProfileScope("ClientRenderer.drawTileOverlays.showIdle"))
                        //        using (var unitListScoped = CollectionCache.GetListScoped<int>())
                        //        {
                        //            for (int i = 0; i < Game.getNumTiles(); i++)
                        //            {
                        //                Tile pTile = Game.tile(i);
                        //                if (pTile != null)
                        //                {
                        //                    unitListScoped.Value.Clear();
                        //                    pTile.getAliveUnits(unitListScoped.Value);
                        //                    foreach (int iLoopUnit in unitListScoped.Value)
                        //                    {
                        //                        Unit pLoopUnit = Game.unit(iLoopUnit);
                        //                        if (pLoopUnit != null && pLoopUnit.canUseUnit(pActivePlayer))
                        //                        {
                        //                            if (!(pLoopUnit.isBusy()))
                        //                            {
                        //                                if (pLoopUnit.info().mbMelee)
                        //                                {
                        //                                    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), ColorManager.GetColor(Game.infos().Globals.COLOR_IDLE_MELEE).SetAlpha((pLoopUnit.isFatigued() || pLoopUnit.isSleep()) ? 0.2f : 0.5f));
                        //                                }
                        //                                else if (pLoopUnit.info().miRangeMax > 0)
                        //                                {
                        //                                    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), ColorManager.GetColor(Game.infos().Globals.COLOR_IDLE_RANGED).SetAlpha((pLoopUnit.isFatigued() || pLoopUnit.isSleep()) ? 0.2f : 0.5f));
                        //                                }
                        //                                else
                        //                                {
                        //                                    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), ColorManager.GetColor(Game.infos().Globals.COLOR_IDLE).SetAlpha((pLoopUnit.isFatigued() || pLoopUnit.isSleep()) ? 0.2f : 0.5f));
                        //                                }
                        //                            }
                        //                        }
                        //                    }
                        //                }
                        //            }
                        //        }
                        //        break;
                        //    case MapOverlayType.ZOC:
                        //        using (new UnityProfileScope("ClientRenderer.drawTileOverlays.showZOC"))
                        //        {
                        //            for (int i = 0; i < Game.getNumTiles(); i++)
                        //            {
                        //                Tile pTile = Game.tile(i);
                        //                if ((pTile != null) && (getTileVisibility(pTile) > VisibilityType.REVEALED))
                        //                {
                        //                    if (pTile.isHostileZOC(pSelectedUnit, eVisibilityTeam, bIgnoreRiver: true))
                        //                    {
                        //                        drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), ColorManager.GetColor(Game.infos().Globals.COLOR_HOSTILE_ZOC));
                        //                    }
                        //                }
                        //            }
                        //        }
                        //        break;
                        //    case MapOverlayType.VISIBILITY:
                        //        using (new UnityProfileScope("ClientRenderer.drawTileOverlays.showVisibility"))
                        //        {
                        //            PlayerType eSelectedPlayer = ClientMgr.Selection.getSelectedPlayer();
                        //            if (eSelectedPlayer == PlayerType.NONE)
                        //            {
                        //                Unit pUnit = ClientMgr.Selection.getSelectedUnit();
                        //                if (pUnit != null)
                        //                {
                        //                    eSelectedPlayer = pUnit.getPlayer();
                        //                }
                        //            }
                        //            using (var tilesScoped = CollectionCache.GetHashSetScoped<int>())
                        //            {
                        //                for (PlayerType eLoopPlayer = 0; eLoopPlayer < Game.getNumPlayers(); ++eLoopPlayer)
                        //                {
                        //                    if (eLoopPlayer == eSelectedPlayer || eSelectedPlayer == PlayerType.NONE || Game.player(eSelectedPlayer).getTeam() == pActivePlayer.getTeam())
                        //                    {
                        //                        Player pLoopPlayer = Game.player(eLoopPlayer);
                        //                        if (pLoopPlayer.getTeam() != pActivePlayer.getTeam())
                        //                        {
                        //                            Game.calculateTeamKnownVisibility(pActivePlayer.getTeam(), pLoopPlayer.getTeam(), tilesScoped.Value);
                        //                        }
                        //                    }
                        //                }
                        //                foreach (int iLoopTile in tilesScoped.Value)
                        //                {
                        //                    Tile pTile = Game.tile(iLoopTile);
                        //                    if (pTile != null)
                        //                    {
                        //                        Color color = Color.white;
                        //                        if (eSelectedPlayer != PlayerType.NONE && Game.player(eSelectedPlayer).getTeam() != pActivePlayer.getTeam())
                        //                        {
                        //                            PlayerColorType ePlayerColor = Game.player(eSelectedPlayer).getPrimaryPlayerColor(pActivePlayer);
                        //                            ColorType eColor = ePlayerColor != PlayerColorType.NONE ? Game.infos().playerColor(ePlayerColor).meAssetColor : Infos.Globals.COLOR_UNDEFINED;
                        //                            color = ColorManager.GetColor(eColor);
                        //                        }
                        //                        drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color.SetAlpha(0.3f));
                        //                    }
                        //                }
                        //            }
                        //        }
                        //        break;
                        //}
                    }
                    //else if (ClientMgr.Selection.isSelectedPlayer())
                    //{
                    //    using (new UnityProfileScope("ClientRenderer.drawTileOverlays.selectedPlayer"))
                    //    {
                    //        PlayerColorType ePlayerColor = Game.player(ClientMgr.Selection.getSelectedPlayer()).getPrimaryPlayerColor(pActivePlayer);
                    //        ColorType eColor = ePlayerColor != PlayerColorType.NONE ? Game.infos().playerColor(ePlayerColor).meAssetColor : Infos.Globals.COLOR_UNDEFINED;
                    //        Color color = ColorManager.GetColor(eColor);

                    //        for (int i = 0; i < Game.getNumTiles(); i++)
                    //        {
                    //            Tile pTile = Game.tile(i);
                    //            if (pTile != null)
                    //            {
                    //                Unit pTopVisibleUnit = pTile.topUnitVisible(eActiveTeam, ClientMgr.Renderer.isShowAllMap(), pSelectedUnit);

                    //                if ((getTileVisibility(pTile) > VisibilityType.REVEALED) && (pTopVisibleUnit != null) && (pTopVisibleUnit.getPlayer() == ClientMgr.Selection.getSelectedPlayer()))
                    //                {
                    //                    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color.SetAlpha(0.6f));
                    //                }
                    //                else if (pTile.getRevealedOwner(eVisibilityTeam) == ClientMgr.Selection.getSelectedPlayer())
                    //                {
                    //                    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color.SetAlpha(0.3f));
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //else if (ClientMgr.Selection.isSelectedTribe())
                    //{
                    //    using (new UnityProfileScope("ClientRenderer.drawTileOverlays.selectedBarbarian"))
                    //    {
                    //        InfoTribe barb = Game.infos().tribe(ClientMgr.Selection.getSelectedTribe());
                    //        PlayerColorType ePlayerColor = Game.infos().teamColor(barb.meTeamColor).maePlayerColors[0];
                    //        ColorType eColor = Game.infos().playerColor(ePlayerColor).meAssetColor;
                    //        Color color = ColorManager.GetColor(eColor);

                    //        for (int i = 0; i < Game.getNumTiles(); i++)
                    //        {
                    //            Tile pTile = Game.tile(i);
                    //            if (pTile != null)
                    //            {
                    //                Unit pTopVisibleUnit = pTile.topUnitVisible(eActiveTeam, ClientMgr.Renderer.isShowAllMap(), pSelectedUnit);

                    //                if ((getTileVisibility(pTile) > VisibilityType.REVEALED) && (pTopVisibleUnit != null) && (pTopVisibleUnit.getTribe() == ClientMgr.Selection.getSelectedTribe()))
                    //                {
                    //                    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color.SetAlpha(0.6f));
                    //                }
                    //                else if (pTile.getImprovementTribeSite(eActiveTeam) == ClientMgr.Selection.getSelectedTribe())
                    //                {
                    //                    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color.SetAlpha(0.4f));
                    //                }
                    //                else if (pTile.getRevealedOwnerTribe(eActiveTeam) == ClientMgr.Selection.getSelectedTribe())
                    //                {
                    //                    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color.SetAlpha(0.2f));
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //else if (ClientMgr.Selection.isSelectedReligion())
                    //{
                    //    using (new UnityProfileScope("ClientRenderer.drawTileOverlays.selectedReligion"))
                    //    {
                    //        ReligionType eReligion = ClientMgr.Selection.getSelectedReligion();
                    //        for (int i = 0; i < Game.getNumTiles(); i++)
                    //        {
                    //            Tile pTile = Game.tile(i);
                    //            if (pTile != null)
                    //            {
                    //                City pCityTerritory = pTile.revealedCityTerritory(eVisibilityTeam);

                    //                if (pCityTerritory != null)
                    //                {
                    //                    if (pTile.revealedCityTerritory(eVisibilityTeam).isReligionNowOrFuture(eReligion))
                    //                    {
                    //                        bool bImprovement = false;

                    //                        if (pTile.hasRevealedImprovement(eVisibilityTeam))
                    //                        {
                    //                            if (pTile.revealedImprovement(eVisibilityTeam).meReligionPrereq == eReligion || pTile.revealedImprovement(eVisibilityTeam).meReligionSpread == eReligion)
                    //                            {
                    //                                bImprovement = true;
                    //                            }
                    //                        }

                    //                        float fAlpha = bImprovement ? 1.0f : 0.3f;

                    //                        ColorType eColor = Game.religionColor(eReligion, pActivePlayer);
                    //                        Color color = ColorManager.GetColor(eColor);
                    //                        if (pTile.revealedCityTerritory(eVisibilityTeam).isReligion(eReligion))
                    //                        {
                    //                            drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color.SetAlpha(fAlpha));
                    //                        }
                    //                        else
                    //                        {
                    //                            drawTileOverlayBanded(pTile.getID(), pTile.getWorldPosition(), color.SetAlpha(fAlpha));
                    //                        }
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //else if (ClientMgr.Selection.isSelectedFamily())
                    //{
                    //    using (new UnityProfileScope("ClientRenderer.drawTileOverlays.selectedFamily"))
                    //    {
                    //        for (int i = 0; i < Game.getNumTiles(); i++)
                    //        {
                    //            Tile pTile = Game.tile(i);
                    //            if (pTile != null)
                    //            {
                    //                Unit pTopVisibleUnit = pTile.topUnitVisible(eActiveTeam, ClientMgr.Renderer.isShowAllMap(), pSelectedUnit);

                    //                if ((getTileVisibility(pTile) > VisibilityType.REVEALED) && (pTopVisibleUnit != null) && (pTopVisibleUnit.getFamily() == ClientMgr.Selection.getSelectedFamily()))
                    //                {
                    //                    TeamColorType eNationColor = pTopVisibleUnit?.player()?.nation()?.meTeamColor ?? TeamColorType.NONE;
                    //                    PlayerColorType ePlayerColor = Game.infos().teamColor(eNationColor)?.maePlayerColors[(int)pTopVisibleUnit.family().miColorIndex] ?? PlayerColorType.NONE;
                    //                    ColorType eColor = Game.infos().playerColor(ePlayerColor)?.meAssetColor ?? ColorType.NONE;
                    //                    Color color = ColorManager.GetColor(eColor);
                    //                    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color.SetAlpha(0.6f));
                    //                }
                    //                else if (pTile.hasRevealedCityTerritory(eVisibilityTeam) && (pTile.revealedCityTerritory(eVisibilityTeam).getFamily() == ClientMgr.Selection.getSelectedFamily()))
                    //                {
                    //                    TeamColorType eNationColor = pTile.revealedCityTerritory(eVisibilityTeam)?.player()?.nation()?.meTeamColor ?? TeamColorType.NONE;
                    //                    PlayerColorType ePlayerColor = Game.infos().teamColor(eNationColor)?.maePlayerColors[(int)pTile.revealedCityTerritory(eVisibilityTeam).family().miColorIndex] ?? PlayerColorType.NONE;
                    //                    ColorType eColor = Game.infos().playerColor(ePlayerColor)?.meAssetColor ?? ColorType.NONE;
                    //                    Color color = ColorManager.GetColor(eColor);
                    //                    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color.SetAlpha(0.3f));
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //else //normal overlays
                    //{
                    //    using (new UnityProfileScope("ClientRenderer.drawTileOverlays.hostileZOC"))
                    //    {
                    //        if ((pSelectedUnit != null) && !ClientMgr.Interfaces.Renderer.isUnitMoving(pSelectedUnit.getID()) && !isHideUnits())
                    //        {
                    //            Color overlayColor = ColorManager.GetColor(Game.infos().Globals.COLOR_HOSTILE_ZOC);

                    //            using var zocTilesScoped = CollectionCache.GetHashSetScoped<int>();
                    //            HashSet<int> siZocTiles = zocTilesScoped.Value;

                    //            //ZOC only happens next to hostile units and cities
                    //            foreach (Unit unit in Game.getUnits())
                    //            {
                    //                if (isVisibleUnit(unit))
                    //                {
                    //                    if (Game.isHostileUnit(pSelectedUnit.getTeam(), pSelectedUnit.getTribe(), unit))
                    //                    {
                    //                        Tile pLoopTile = unit.tile();
                    //                        for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                    //                        {
                    //                            Tile pAdjacentTile = pLoopTile.tileAdjacent(eDirection);
                    //                            if (pAdjacentTile != null && !siZocTiles.Contains(pAdjacentTile.getID()))
                    //                            {
                    //                                if (getTileVisibility(pAdjacentTile) > VisibilityType.REVEALED)
                    //                                {
                    //                                    if (pAdjacentTile.isInUnitZOC(eDirection, pSelectedUnit, eVisibilityTeam))
                    //                                    {
                    //                                        drawTileOverlay(pAdjacentTile.getID(), pAdjacentTile.getWorldPosition(), overlayColor);
                    //                                        siZocTiles.Add(pAdjacentTile.getID());
                    //                                    }
                    //                                }
                    //                            }
                    //                        }
                    //                    }
                    //                }
                    //            }

                    //            foreach (City city in Game.getCities())
                    //            {
                    //                if (Game.isHostileCity(pSelectedUnit.getTeam(), pSelectedUnit.getTribe(), city))
                    //                {
                    //                    Tile pLoopTile = city.tile();
                    //                    if (getTileVisibility(pLoopTile) > VisibilityType.REVEALED)
                    //                    {
                    //                        for (DirectionType eDirection = 0; eDirection < DirectionType.NUM_TYPES; eDirection++)
                    //                        {
                    //                            Tile pAdjacentTile = pLoopTile.tileAdjacent(eDirection);
                    //                            if (pAdjacentTile != null && !siZocTiles.Contains(pAdjacentTile.getID()))
                    //                            {
                    //                                if (getTileVisibility(pAdjacentTile) > VisibilityType.REVEALED)
                    //                                {
                    //                                    if (pAdjacentTile.isInUnitZOC(eDirection, pSelectedUnit, eVisibilityTeam))
                    //                                    {
                    //                                        drawTileOverlay(pAdjacentTile.getID(), pAdjacentTile.getWorldPosition(), overlayColor);
                    //                                        siZocTiles.Add(pAdjacentTile.getID());
                    //                                    }
                    //                                }
                    //                            }
                    //                        }

                    //                        drawTileOverlay(pLoopTile.getID(), pLoopTile.getWorldPosition(), ColorManager.GetColor(Game.infos().playerColor(city.getPrimaryPlayerColor(pActivePlayer)).meCrestColor).SetAlpha(0.7f));
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }

                    //    if (pSelectedUnit != null)
                    //    {
                    //        using (new UnityProfileScope("ClientRenderer.drawTileOverlays.specialOverlays"))
                    //        {
                    //            Color overlayColor = ColorManager.GetColor(Game.infos().Globals.COLOR_SELECTION_SPECIAL);
                    //            foreach (Tile pTile in Game.allTiles())
                    //            {
                    //                if (!pTile.isBoundary())
                    //                {
                    //                    if (isTileSelectionSpecial(pTile))
                    //                    {
                    //                        drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), overlayColor);
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                }

                //draw selected city overlay in place of any previous overlay
                if (pSelectedCity != null)
                {
                    Tile pTile = pSelectedCity.tile();
                    Color color = ColorManager.GetColor(pActivePlayer.getBorderColor(pActivePlayer)).SetAlpha(0.4f);
                    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), color);
                }

                foreach (int tileID in mTileOverlayIDs)
                {
                    drawTileOverlay(tileID, Game.tile(tileID).getWorldPosition(), Color.white.SetAlpha(0.7f));
                }
            }

            //UnityEngine.Debug.Log("drawTileOverlays with MapOverlayType.ROADS_RIVERS - End");
        }


        protected override void drawTileSelectionEdges()
        {
            //UnityEngine.Debug.Log("ClientRenderer.drawTileSelectionEdges");
            base.drawTileSelectionEdges();

/*####### Better Old World AI - Base DLL #######
  ### River Edge Highlight             START ###
  ##############################################*/
            
            MapOverlayType activeOverlay = getActiveOverlay();
            if (activeOverlay == MapOverlayType.ROADS_RIVERS || activeOverlay == MapOverlayType.NETWORK)
            {
                //UnityEngine.Debug.Log("drawTileSelectionEdges with MapOverlayType.ROADS_RIVERS/NETWORK - Start");
                Player pActivePlayer = ClientMgr.activePlayer();
                ColorType riverEdgeColor = ((BetterAIInfoGlobals)(Game.infos().Globals)).COLOR_RIVER_EDGE;
                Tile pAdjacentTile;
                bool bRiver;

                for (int i = 0; i < Game.getNumTiles(); i++)
                {
                    Tile pTile = Game.tile(i);
                    if (pTile != null)
                    {
                        //instead of tile overlay ..
                        //else if (pTile.isRiver())
                        //{
                        //    drawTileOverlay(pTile.getID(), pTile.getWorldPosition(), ColorManager.GetColor(Game.infos().Globals.COLOR_RIVER_CONNECTION));
                        //}

                        //.. draw on river edges instead
                        bRiver = false;
                        if (pTile.isRiverW())
                        {
                            Interfaces.Renderer.setTileEdge(pTile.getID(), DirectionType.W, riverEdgeColor, fAlpha: 1.0f, bDotted: false, Game, pActivePlayer.getPlayer());
                            bRiver = true;
                        }
                        if (pTile.isRiverSW())
                        {
                            Interfaces.Renderer.setTileEdge(pTile.getID(), DirectionType.SW, riverEdgeColor, fAlpha: 1.0f, bDotted: false, Game, pActivePlayer.getPlayer());
                            bRiver = true;
                        }
                        if (pTile.isRiverSE())
                        {
                            Interfaces.Renderer.setTileEdge(pTile.getID(), DirectionType.SE, riverEdgeColor, fAlpha: 1.0f, bDotted: false, Game, pActivePlayer.getPlayer());
                            bRiver = true;
                        }

                        //only next to boundary
                        pAdjacentTile = pTile.tileAdjacent(DirectionType.E, true);
                        if (pAdjacentTile != null)
                        {
                            if (pAdjacentTile.isBoundary() && pAdjacentTile.isRiverW())
                            {
                                Interfaces.Renderer.setTileEdge(pTile.getID(), DirectionType.E, riverEdgeColor, fAlpha: 1.0f, bDotted: false, Game, pActivePlayer.getPlayer());
                                bRiver = true;
                            }
                        }
                        pAdjacentTile = pTile.tileAdjacent(DirectionType.NE, true);
                        if (pAdjacentTile != null)
                        {
                            if (pAdjacentTile.isBoundary() && pAdjacentTile.isRiverSW())
                            {
                                Interfaces.Renderer.setTileEdge(pTile.getID(), DirectionType.NE, riverEdgeColor, fAlpha: 1.0f, bDotted: false, Game, pActivePlayer.getPlayer());
                                bRiver = true;
                            }
                        }
                        pAdjacentTile = pTile.tileAdjacent(DirectionType.NW, true);
                        if (pAdjacentTile != null)
                        {
                            if (pAdjacentTile.isBoundary() && pAdjacentTile.isRiverSE())
                            {
                                Interfaces.Renderer.setTileEdge(pTile.getID(), DirectionType.NW, riverEdgeColor, fAlpha: 1.0f, bDotted: false, Game, pActivePlayer.getPlayer());
                                bRiver = true;
                            }
                        }

                        if (bRiver)
                        {
                            mTileEdgeTileIDs.Add(pTile.getID());
                        }
                    }
                }

                //UnityEngine.Debug.Log("drawTileSelectionEdges with MapOverlayType.ROADS_RIVERS/NETWORK - End");

            }
/*####### Better Old World AI - Base DLL #######
  ### River Edge Highlight               END ###
  ##############################################*/

        }

    }
}
