﻿using System.Linq;
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
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.Concurrent;

namespace BetterAI
{
    public class BetterAIClientUI : ClientUI
    {
        public BetterAIClientUI(IApplication app) : base(app)
        {
        }

        private const string YIELD_ARROW = "Sprites/GuitarPickUp";
        private const string YIELD_NORMAL = "Sprites/GuitarPick";
        //copy-paste START
        //lines 9192-9436
        protected override void updateCitySelection(City pSelectedCity)
        {
            //using (new UnityProfileScope("ClientUI.updateCitySelection"))
            {
                if (pSelectedCity != null)
                {

                    Player pPlayer = pSelectedCity.player();
                    Player pActivePlayer = ClientMgr.activePlayer();

                    FieldInfo FI_YIELD_ARROW = this.GetType().BaseType.GetField("YIELD_ARROW", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    FieldInfo FI_YIELD_NORMAL = this.GetType().BaseType.GetField("YIELD_NORMAL", BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    bool bIsReflectionWorking = false;
                    if (FI_YIELD_ARROW != null && FI_YIELD_NORMAL != null)
                    {
                        string base_YIELD_ARROW = (string)FI_YIELD_ARROW.GetValue((ClientUI)this);
                        string base_YIELD_NORMAL = (string)FI_YIELD_NORMAL.GetValue((ClientUI)this);

                        if (base_YIELD_ARROW != null && base_YIELD_NORMAL != null)
                        {
                            mSelectedPanel.SetKey("ArrowShape", pPlayer == pActivePlayer ? base_YIELD_ARROW : base_YIELD_NORMAL);
                            bIsReflectionWorking = true;
                        }
                        else
                        {
                            UnityEngine.Debug.Log("base_YIELD_ARROW/base_YIELD_NORMAL null");
                        }
                    }
                    else
                    {
                        UnityEngine.Debug.Log("FI_YIELD_ARROW/FI_YIELD_NORMAL null");
                    }
                    if (!bIsReflectionWorking)
                    {
                        //fallback
                        mSelectedPanel.SetKey("ArrowShape", pPlayer == pActivePlayer ? YIELD_ARROW : YIELD_NORMAL);
                    }


                    UIAttributeTag widgetTag = UI.GetUIAttributeTag("SelectedCityWidget");
                    widgetTag.SetInt("MaxHP", pSelectedCity.getHPMax());
                    widgetTag.SetInt("CurrentHP", pSelectedCity.getHP());

                    {
                        Character pGovernor = pSelectedCity.governor();
                        bool isInteractable = pSelectedCity.canMakeGovernor();

                        bool showGovernor = Game.isCharacters();
                        if (pGovernor != null)
                        {
                            updateCityGovernorPortrait(mSelectedPanel.GetSubTag("-City"), pSelectedCity, pGovernor);
                            mSelectedPanel.SetBool("City-Governor-State", false);
                        }
                        else
                        {
                            using (var dataScope = new WidgetDataScope(nameof(ItemType.CHOOSE_GOVERNOR), pSelectedCity.getID().ToStringCached()))
                            {
                                mSelectedPanel.SetKey("City-Governor-ItemType", dataScope.Type);
                                mSelectedPanel.SetKey("City-Governor-Data", dataScope.DataList);
                                mSelectedPanel.SetBool("City-HasGovernor", false);
                                mSelectedPanel.SetBool("City-Governor-State", pSelectedCity.canHaveGovenor() && pSelectedCity.canMakeGovernor(bJobValid: false));
                            }
                        }

                        // Button
                        mSelectedPanel.SetKey("City-Governor-IsInteractable", isInteractable.ToStringCached());
                        mSelectedPanel.SetKey("City-Governor-ButtonIsVisible", showGovernor.ToStringCached());
                    }

                    int numProgressYields = 0;
                    for (YieldType eLoopYield = 0; eLoopYield < Infos.yieldsNum(); eLoopYield++)
                    {
                        InfoYield yield = Infos.yield(eLoopYield);
                        int uiPosition = yield.miUIPosition;
                        if (uiPosition != -1)
                        {
                            //var profileScope = new UnityProfileScope("ClientUI.updateCitySelection.updateYieldArrows");

                            UIAttributeTag cityYieldTag = mSelectedPanel.GetSubTag("-Yield", uiPosition);

                            int iRate = 0;

                            bool isBuildYield = pSelectedCity.isYieldBuildCurrent(eLoopYield);

                            if (!isBuildYield || !(pSelectedCity.getCurrentBuild().mdYieldCosts.TryGetValue(eLoopYield, out int value) && value > 0))
                            {
                                iRate = pSelectedCity.calculateCurrentYield(eLoopYield, true);
                            }

                            TextVariable rateVar = HelpText.buildYieldTextVariable(iRate, bRate: true, iMultiplier: Constants.YIELDS_MULTIPLIER);

                            cityYieldTag.SetTEXT("Rate", TextManager, HelpText.buildNetCityYieldTextVariable(pSelectedCity, eLoopYield, true));

                            if (ColorManager.GetColorHex(yield.meColor) == "#ffffffff" || yield.mbCanBuy)
                            {
                                if (iRate > 0)
                                {
                                    cityYieldTag.SetKey("Color", ColorManager.GetColorHex(Infos.Globals.COLOR_POSITIVE));
                                }
                                else if (iRate < 0)
                                {
                                    cityYieldTag.SetKey("Color", ColorManager.GetColorHex(Infos.Globals.COLOR_NEGATIVE));
                                }
                            }
                            else
                            {
                                cityYieldTag.SetKey("Color", ColorManager.GetColorHex(yield.meColor));
                            }

                            cityYieldTag.SetBool("Rate-IsActive", iRate != 0 || isBuildYield);

                            if (eLoopYield == Infos.Globals.MONEY_YIELD)
                            {
                                cityYieldTag.ItemType = ItemType.CITY_INCOME.ToStringCached();
                                cityYieldTag.Data = pSelectedCity.getID().ToStringCached();
                            }
                            else
                            {
                                using (var yieldDataScope = new WidgetDataScope(nameof(ItemType.HELP_LINK), nameof(LinkType.HELP_CITY_YIELD_STOCKPILE), pSelectedCity.getID().ToStringCached(), eLoopYield.ToStringCached()))
                                {
                                    cityYieldTag.SetKey("Data", yieldDataScope.DataList);
                                }
                            }
                        }
                        else if (eLoopYield == Infos.Globals.GROWTH_YIELD || eLoopYield == Infos.Globals.CULTURE_YIELD || eLoopYield == Infos.Globals.HAPPINESS_YIELD)
                        {
                            //var profileScope = new UnityProfileScope("ClientUI.updateCitySelection.updateProgressYields");

/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                START ###
  ##############################################*/
                            bool bDiscontent = ((eLoopYield == Infos.Globals.HAPPINESS_YIELD) && (((BetterAICity)pSelectedCity).isDiscontent()));
                            YieldType eDisplayYield = ((bDiscontent) ? Infos.Globals.DISCONTENT_YIELD : eLoopYield);
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                  END ###
  ##############################################*/

                            UIAttributeTag cityProgressYieldTag = mSelectedPanel.GetSubTag("-City-ProgressYield", numProgressYields);
                            cityProgressYieldTag.SetKey("Icon", Infos.yield(eDisplayYield).mzIconName);

                            if (eLoopYield == Infos.Globals.HAPPINESS_YIELD)
                            {
                                cityProgressYieldTag.SetTEXT("ExtraLabel", TextManager, HelpText.buildHappinessLevelLinkVariable(pSelectedCity));
                            }
                            else if (eLoopYield == Infos.Globals.CULTURE_YIELD)
                            {
                                cityProgressYieldTag.SetTEXT("ExtraLabel", TextManager, HelpText.buildCultureCityLinkVariable(pSelectedCity));
                            }
                            else if (eLoopYield == Infos.Globals.GROWTH_YIELD)
                            {
                                using (var citizenLabelScope = CollectionCache.GetStringBuilderScoped())
                                {
                                    using (var citizensScope = new WidgetDataScope(nameof(ItemType.CITIZENS), pSelectedCity.getID().ToStringCached()))
                                    {
                                        int numCitizens = pSelectedCity.getCitizens();
                                        HelpText.buildLinkText(citizenLabelScope.Value, TEXT("TEXT_UI_CITY_SELECTION_CITIZENS_TAB_LABEL", TEXTVAR(numCitizens)), citizensScope);
                                        cityProgressYieldTag.SetKey("ExtraLabel", citizenLabelScope.Value);
                                    }
                                }
                            }

                            ColorType eNextColor = ColorType.NONE;

                            int iRate = pSelectedCity.calculateCurrentYield(eLoopYield, true);

                            float currentProgress = pSelectedCity.getYieldProgress(eLoopYield);
                            float yieldThreshold = pSelectedCity.getYieldThreshold(eLoopYield);

                            cityProgressYieldTag.SetTEXT("Progress", TextManager, HelpText.buildSlashText(HelpText.buildValueTextVariable(pSelectedCity.getYieldProgress(eLoopYield), Constants.YIELDS_MULTIPLIER),
                                TEXTVAR(pSelectedCity.getYieldThresholdWhole(eLoopYield))));

                            float nextTurnProgress = (float)(iRate) / yieldThreshold;

                            currentProgress /= yieldThreshold;

                            if (eLoopYield == Infos.Globals.HAPPINESS_YIELD)
                            {
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                START ###
  ##############################################*/
                                if (bDiscontent)
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                  END ###
  ##############################################*/
                                {
                                    iRate = -(iRate);
                                    nextTurnProgress = -nextTurnProgress;
                                }

                                if (nextTurnProgress < 0)
                                {
                                    currentProgress += nextTurnProgress;
                                    nextTurnProgress = -nextTurnProgress;
                                    eNextColor = (eDisplayYield == Infos.Globals.HAPPINESS_YIELD) ? Infos.yield(Infos.Globals.DISCONTENT_YIELD).meColor : Infos.yield(Infos.Globals.HAPPINESS_YIELD).meColor;
                                }
                            }



                            TextVariable rateText = HelpText.buildYieldTextVariable(iRate, true, false, Constants.YIELDS_MULTIPLIER);
                            rateText = HelpText.buildColorTextOptionalVariable(rateText, !(Infos.Helpers.yieldWarning(eDisplayYield, iRate)), (iRate != 0));

                            int iTurns = pSelectedCity.getYieldTurnsLeft(eLoopYield);
                            if (iTurns > 0)
                            {
                                rateText = HelpText.buildEnclosedParenthesis(rateText, HelpText.buildTurnsTextVariable(iTurns, Game));
                            }
                            else if (pSelectedCity.isYieldBuildCurrent(eLoopYield))
                            {
                                rateText = HelpText.buildEnclosedParenthesis(rateText, HelpText.getQueueIconLinkVariable(pSelectedCity.getCurrentBuild(), pSelectedCity));
                            }

                            setProgressBarFillData(cityProgressYieldTag, currentProgress, nextTurnProgress, Infos.yield(eDisplayYield).meColor, eNextColor);

                            cityProgressYieldTag.SetTEXT("Label", TextManager, rateText);
                            using (var yieldDataScope = new WidgetDataScope(nameof(ItemType.HELP_LINK), nameof(LinkType.HELP_CITY_YIELD), pSelectedCity.getID().ToStringCached(), eLoopYield.ToStringCached()))
                            {
                                cityProgressYieldTag.SetKey("Data", yieldDataScope.DataList);
                            }

                            numProgressYields++;
                        }

                        using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                        {
                            int totalProjects = 0;

                            using (builder.BeginScope("TEXT_HELPTEXT_LINE_TWO"))
                            {
                                for (ProjectType eLoopProject = 0; eLoopProject < Infos.projectsNum(); eLoopProject++)
                                {
                                    if (!string.IsNullOrEmpty(Infos.project(eLoopProject).mzIcon))
                                    {
                                        int numProjects = pSelectedCity.getProjectCount(eLoopProject);
                                        totalProjects += numProjects;

                                        if (numProjects > 0)
                                        {
                                            if (numProjects > 1)
                                            {
                                                builder.Add(HelpText.buildSizeTextVariable(numProjects, 1.3f));
                                            }

                                            builder.Add(HelpText.buildProjectIconLinkVariable(eLoopProject, pSelectedCity), skipSeparator: numProjects > 1);
                                        }
                                    }
                                }
                            }

                            if (totalProjects > 0)
                                mSelectedPanel.SetKey("City-Projects-Label", builder.ProfiledToString());
                            else
                                mSelectedPanel.SetKey("City-Projects-Label", TEXT("TEXT_GENERIC_NONE"));
                        }
                    }

                    if (pSelectedCity.hasBuild())
                    {
                        mSelectedPanel.SetTEXT("City-Production-ProgressText", TextManager, HelpText.buildCityQueueProgress(pSelectedCity, pSelectedCity.getBuildQueueNode(0), true, false));
                    }

                    if (pPlayer == pActivePlayer)
                    {
                        int chancellorID = pActivePlayer.getCouncilCharacter(Infos.Globals.CHANCELLOR_COUNCIL);
                        using (var pacifyDataScope = new WidgetDataScope(nameof(ItemType.START_MISSION), Infos.Globals.PACIFY_CITY_MISSION.ToStringCached(), chancellorID.ToStringCached(), pSelectedCity.getID().ToStringCached(), true.ToStringCached()))
                        {
                            mSelectedPanel.SetBool("City-Pacify-IsInteractable", chancellorID != -1 &&
                                pActivePlayer.canStartMission(Infos.Globals.PACIFY_CITY_MISSION, chancellorID, pSelectedCity.getID().ToStringCached(), bControl: Interfaces.Input.isControlPressed()));
                            mSelectedPanel.SetKey("City-Pacify-Type", pacifyDataScope.Type);
                            mSelectedPanel.SetKey("City-Pacify-Data", pacifyDataScope.DataList);
                        }
                    }
                    else
                    {
                        mSelectedPanel.SetKey("City-Pacify-Type", ItemType.NONE.ToStringCached());
                    }

                    using (var luxuryListScope = CollectionCache.GetStringBuilderScoped())
                    {
                        //using var profileScope = new UnityProfileScope("ClientUI.updateCitySelection.updateLuxuryList");

                        bool bCanManageLuxuries = false;
                        for (ResourceType eLoopResource = 0; eLoopResource < Infos.resourcesNum(); eLoopResource++)
                        {
                            bool hasLuxury = pSelectedCity.isLuxury(eLoopResource);
                            bCanManageLuxuries |= pActivePlayer.canTradeCityLuxury(pSelectedCity, eLoopResource, !hasLuxury);

                            if (hasLuxury)
                            {
                                HelpText.buildResourceIconLink(luxuryListScope.Value, eLoopResource);
                            }
                        }
                        if (luxuryListScope.Value.Length == 0)
                        {
                            TEXT(luxuryListScope.Value, "TEXT_MANAGE_LUXURIES");
                        }
                        mSelectedPanel.SetKey("City-Luxuries", luxuryListScope.Value);
                        mSelectedPanel.SetBool("City-CanSendLuxuries", bCanManageLuxuries);
                    }
                }

                lastSelectedCityID = pSelectedCity.getID();
                ClientMgr.UI.updateExpansionPreview(null, null, false);
                makeDirty(DirtyType.ACTION_PANEL);
            }
        }
        //copy-paste END

/*####### Better Old World AI - Base DLL #######
  ### Worker Default List Extra Items  START ###
  ##############################################*/
        //lines 11725-11756
        public override void GetValidImprovementsForTile(List<ImprovementType> aeImprovements, Tile pTile, Unit pUnit, WorkerActionFilter eFilter = WorkerActionFilter.CURRENT_TILE)
        {
            bool bRunOriginal = true;
            if (pUnit != null && pTile != null && eFilter == WorkerActionFilter.CURRENT_TILE && ((BetterAIInfoGlobals)Infos.Globals).BAI_WORKERLIST_EXTRA > 0)
            {
                bool bWorker = pUnit.isWorker();
                Player pActivePlayer = ClientMgr.activePlayer();
                bool bTeamTest = ((pTile.hasOwner()) ? (pTile.owner().getTeam() == pActivePlayer.getTeam() || pTile.owner().getTeam() == TeamType.NONE) : false);
                bool bQueue = (pUnit.hasQueueList() || Interfaces.Input.isShiftPressed());
                bool bControl = Interfaces.Input.isControlPressed();
                bool bImprovedTile = pTile.hasImprovement() || pTile.hasCity()   //also for city center tile, because you can't build any improvement there anyway.
                    || ((BetterAIInfoGlobals)Infos.Globals).BAI_WORKERLIST_EXTRA >= 2; //option to always show extra items (Franz)
                bool bShowExtraImprovements = bWorker && !bControl && !bQueue && bTeamTest && bImprovedTile;
                BetterAICity pCityTerritory = (BetterAICity)pTile.cityTerritory();
                if (bShowExtraImprovements && (pCityTerritory != null))
                {
                    bRunOriginal = false;

                    {
                        ImprovementType eBestImprovement = ImprovementType.NONE;
                        Task.Run(() =>
                        {
                            lock (ClientMgr.TaskLock)
                            {
                                if (!pActivePlayer.AI.isBestTileImprovementsCached(pTile.cityTerritory()))
                                {
                                    pActivePlayer.AI.cacheCityImprovementValues(pTile.cityTerritory());
                                }
                                pActivePlayer.AI.getBestImprovement(pTile, pTile.cityTerritory(), ref eBestImprovement);
                            }
                            selectedWorkerRecommendedImprovementCallback(eBestImprovement);
                        });
                    }

                    List<ResourceType> cityResources = new List<ResourceType>(); //only those without improvement
                    {
                        //make the resource list
                        foreach (int iTileID in pCityTerritory.getTerritoryTiles())
                        {
                            Tile tile = ClientMgr.GameClient.tile(iTileID);
                            ResourceType tileResource = tile.getResource();
                            if (tileResource != ResourceType.NONE)
                            {
                                //skip tiles with appropriate improvement
                                ImprovementType tileImprovement = tile.getImprovement();
                                if (tileImprovement != ImprovementType.NONE)
                                {
                                    if (Infos.Helpers.isImprovementResourceValid(tileImprovement, tileResource))
                                    {
                                        continue;
                                    }
                                }
                                bool bAlreadyInList = false;
                                //check if it's in the list, if not, add it
                                int iListIndex = 0;
                                foreach (ResourceType resource in cityResources)
                                {
                                    if (resource < tileResource)
                                    {
                                        iListIndex++;
                                        continue;
                                    }
                                    else if (resource == tileResource)
                                    {
                                        bAlreadyInList = true;
                                        break;
                                    }
                                    else if (resource > tileResource)
                                    {
                                        break;
                                    }
                                    else
                                    {
                                        //should not happen
                                        iListIndex++;
                                    }
                                }
                                //add at index, so we get a sorted list
                                if (!bAlreadyInList)
                                {
                                    cityResources.Insert(iListIndex, tileResource);
                                }
                            }
                        }
                    }

                    for (ImprovementType eLoopImprovement = 0; eLoopImprovement < Infos.improvementsNum(); eLoopImprovement++)
                    {
                        bool bShowButton = false;
                        InfoImprovement pImprovementInfo = Infos.improvement(eLoopImprovement);
                        if (pImprovementInfo == null) continue;
                        if (!(pUnit.canBuildImprovementType(eLoopImprovement))) continue;

                        if (pImprovementInfo.mbShowAlways && (pImprovementInfo.mbUrban == pTile.isUrban()))
                        {
                            bShowButton = true;
                        }
                        else if (pUnit.canBuildImprovement(pTile, eLoopImprovement, pActivePlayer, isBuyGoods(), bTestEnabled: false, bTestOrders: false, bTestGoods: false))
                        {
                            bShowButton = true;
                        }
                        else
                        {
                            if (pActivePlayer.canStartImprovement(eLoopImprovement, null))
                            {
                                if (!(pCityTerritory.canCityHaveImprovement(eLoopImprovement)))
                                {
                                    continue;
                                }

                                //check valids, including resources (resources have to be in the list)
                                {

                                    if (!(pImprovementInfo.mbFreshWaterValid
                                        || pImprovementInfo.mbRiverValid
                                        || pImprovementInfo.mbCoastLandValid
                                        || pImprovementInfo.mbCoastWaterValid
                                        || pImprovementInfo.mbCityValid
                                        || pImprovementInfo.mbHolyCityValid))
                                    {
                                        bool bAnyValid = false;
                                        for (TerrainType eLoopTerrain = 0; (eLoopTerrain < Infos.terrainsNum() && !(bAnyValid)); eLoopTerrain++)
                                        {
                                            bAnyValid = bAnyValid || pImprovementInfo.mabTerrainValid[(int)eLoopTerrain];
                                        }

                                        //height, heightadjacent, vegetation
                                        for (HeightType eLoopHeight = 0; (eLoopHeight < Infos.heightsNum() && !(bAnyValid)); eLoopHeight++)
                                        {
                                            bAnyValid = bAnyValid || pImprovementInfo.mabHeightValid[(int)eLoopHeight];
                                        }

                                        for (HeightType eLoopHeight = 0; (eLoopHeight < Infos.heightsNum() && !(bAnyValid)); eLoopHeight++)
                                        {
                                            bAnyValid = bAnyValid || pImprovementInfo.mabHeightAdjacentValid[(int)eLoopHeight];
                                        }

                                        for (VegetationType eLoopVegetation = 0; (eLoopVegetation < Infos.vegetationNum() && !(bAnyValid)); eLoopVegetation++)
                                        {
                                            bAnyValid = bAnyValid || pImprovementInfo.mabVegetationValid[eLoopVegetation];
                                        }

                                        if (!(bAnyValid))
                                        {
                                            foreach (ResourceType cityResource in cityResources)
                                            {
                                                if (Infos.Helpers.isImprovementResourceValid(eLoopImprovement, cityResource))
                                                {
                                                    bAnyValid = true;
                                                    break;
                                                }
                                            }
                                            if (!bAnyValid)
                                            {
                                                continue;
                                            }
                                        }

                                    }

                                }

                                bShowButton = true;
                            }
                        }

                        if (bShowButton) aeImprovements.Add(eLoopImprovement);
                    }

                }
            }

            if (bRunOriginal)
            {
                base.GetValidImprovementsForTile(aeImprovements, pTile, pUnit, eFilter);
            }
        }
/*####### Better Old World AI - Base DLL #######
  ### Worker Default List Extra Items    END ###
  ##############################################*/

        //lines 17830-17937
        protected override void updateQueuePanel()
        {
            base.updateQueuePanel();

/*####### Better Old World AI - Base DLL #######
  ### Alternative Hurry                START ###
  ##############################################*/
            City pSelectedCity = ClientMgr.Selection.getSelectedCity();
            if (pSelectedCity == null) return;

            int i = 1;
            UIAttributeTag queueTag = UI.GetUIAttributeTag("QueuePanel-Button", i.ToStringCached());
            if ((((BetterAIInfoGlobals)Infos.Globals).BAI_HURRY_COST_REDUCED > 0) && pSelectedCity.getBuildQueueNode(0) != null && pSelectedCity.getBuildQueueNode(0).mbHurried && (pSelectedCity.getBuildQueueNode(0).miProgress > 0))
            {
                queueTag.SetBool("Arrow-IsActive", false);
            }
            else
            {
                queueTag.SetBool("Arrow-IsActive", true);
            }

            const int numButtons = 5;
            for (i = 2; i < Math.Min(pSelectedCity.getBuildCount(), numButtons); i++)
            {
                queueTag = UI.GetUIAttributeTag("QueuePanel-Button", i.ToStringCached());
                queueTag.SetBool("Arrow-IsActive", true);

                //updateActiveBuildDisplay(pSelectedCity, queueTag, pSelectedCity.getBuildQueueNode(i), i, true);
                //queueTag.IsActive = true;
                //queueTag.SetBool("Effect-IsActive", !addedToFront && lastSelectedCityID == pSelectedCity.getID() && lastSelectedCityQueue.Count <= i);
            }
        }

        //lines 18846-21341
        //Intercept and redirect illegal queue moves at doWidgetAction
        public override bool doWidgetAction(WidgetData pWidget)
        {
            ItemType eType = pWidget.GetWidgetType();
            if (eType == ItemType.BUILD_QUEUE)
            {
                City pCity = Game.city(pWidget.GetDataInt(0));

                if (pCity != null)
                {
                    if ((((BetterAIInfoGlobals)Infos.Globals).BAI_HURRY_COST_REDUCED > 0) && pCity.getBuildQueueNode(0) != null && pCity.getBuildQueueNode(0).mbHurried && (pCity.getBuildQueueNode(0).miProgress > 0))
                    {
                        if (pWidget.GetDataInt(1) == 1)
                        {
                            //do nothing, this is an illegal move
                            return false;
                        }
                        else
                        {
                            ClientMgr.sendBuildQueue(pCity, pWidget.GetDataInt(1), 1);
                        }
                    }
                    else
                    {
                        ClientMgr.sendBuildQueue(pCity, pWidget.GetDataInt(1), 0);
                    }
                }
                return true;
            }
            else
/*####### Better Old World AI - Base DLL #######
  ### Alternative Hurry                  END ###
  ##############################################*/
            {
                return base.doWidgetAction(pWidget);
            }
        }

        //lines 17072-17085
        protected override int CompareHappinessLevels(int cityID, int otherCityID)
        {
            BetterAICity city = (BetterAICity)Game.city(cityID);
            BetterAICity otherCity = (BetterAICity)Game.city(otherCityID);

            if (city.getHappinessLevel() == otherCity.getHappinessLevel())
            {
/*####### Better Old World AI - Base DLL #######
  ### Happiness Level Sort             START ###
  ##############################################*/
                int iCityRate = city.calculateCurrentYield(Infos.Globals.HAPPINESS_YIELD, true);
                int iOtherCityRate = otherCity.calculateCurrentYield(Infos.Globals.HAPPINESS_YIELD, true);
                if (iCityRate == 0 || iOtherCityRate == 0)
                {
                    return (iOtherCityRate - iCityRate);
                }
                else if ((iCityRate > 0) == (iOtherCityRate > 0)) //either both increasing or both decreasing
                {
                    if (city.getYieldTurnsLeft(Infos.Globals.HAPPINESS_YIELD) == otherCity.getYieldTurnsLeft(Infos.Globals.HAPPINESS_YIELD))
                    {
                        return (otherCity.getYieldProgress(Infos.Globals.HAPPINESS_YIELD) - city.getYieldProgress(Infos.Globals.HAPPINESS_YIELD)) * (city.isDiscontent() ? -1 : 1);
                    }
                    else
                    {
                        return (city.getYieldTurnsLeft(Infos.Globals.HAPPINESS_YIELD) - otherCity.getYieldTurnsLeft(Infos.Globals.HAPPINESS_YIELD)) * (iCityRate < 0 ? -1 : 1);
                    }
                }
                else //one is + the other - so they move in different directions
                {
                    return (iOtherCityRate - iCityRate);
                }
/*####### Better Old World AI - Base DLL #######
  ### Happiness Level Sort               END ###
  ##############################################*/
            }
            else
            {
                return otherCity.getHappinessLevel() - city.getHappinessLevel();
            }
        }

        //lines 17022-17035
        protected override int CompareCultureLevels(int cityID, int otherCityID)
        {
            //copy-paste start
            City city = Game.city(cityID);
            City otherCity = Game.city(otherCityID);

            if (city.getCulture() == otherCity.getCulture())
            {
/*####### Better Old World AI - Base DLL #######
  ### Culture Level Sort               START ###
  ##############################################*/
                if (city.getYieldTurnsLeft(Infos.Globals.CULTURE_YIELD) == otherCity.getYieldTurnsLeft(Infos.Globals.CULTURE_YIELD))
                {
                    return city.getYieldProgress(Infos.Globals.CULTURE_YIELD) - otherCity.getYieldProgress(Infos.Globals.CULTURE_YIELD);
                }
                else
                {
                    //check if either city has a yield rate of 0
                    if (city.calculateCurrentYield(Infos.Globals.CULTURE_YIELD, true) * otherCity.calculateCurrentYield(Infos.Globals.CULTURE_YIELD, true) != 0)
                    {
                        //reversed, less turns left = more culture
                        return otherCity.getYieldTurnsLeft(Infos.Globals.CULTURE_YIELD) - city.getYieldTurnsLeft(Infos.Globals.CULTURE_YIELD);
                    }
                    else
                    {
                        //reverse the reversal if either yield rate is 0
                        return city.getYieldTurnsLeft(Infos.Globals.CULTURE_YIELD) - otherCity.getYieldTurnsLeft(Infos.Globals.CULTURE_YIELD);
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### Culture Level Sort                 END ###
  ##############################################*/
            }
            else
            {
                return city.getCulture() - otherCity.getCulture();
            }
            //copy-paste end
        }

    }
}
