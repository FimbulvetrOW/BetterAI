using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using UnityEngine;
using static TenCrowns.GameCore.Text.TextExtensions;

namespace BetterAI
{
    public partial class BetterAIHelpText : HelpText
    {
        //this is the only method in this part
        public override TextBuilder buildWidgetHelp(TextBuilder builder, WidgetData pWidget, ClientManager pManager, bool bIncludeEncyclopediaFooter = true)
        {
            //UnityEngine.Debug.Log("HelpText.buildWidgetHelp");

            if (pWidget.GetWidgetType() == ItemType.CREATE_AGENT_NETWORK)
            {
                Player pActivePlayer = pManager.activePlayer();
                Game pGame = pManager.GameClient;

                Unit pUnit = pGame.unit(pWidget.GetDataInt(0));
                if (pUnit != null)
                {
                    //Player pPlayer = pUnit.player();
                    City pCity = pGame.city(pWidget.GetDataInt(1));
/*####### Better Old World AI - Base DLL #######
  ### Agent Network Cost Scaling       START ###
  ##############################################*/
                    builder.AddTEXT("TEXT_HELPTEXT_WIDGET_CREATE_AGENT_NETWORK", buildAgentNetworkLinkVariable(), buildMoneyWarningTextVariable(((BetterAIUnit)pUnit).getAgentNetworkCost(pCity), pActivePlayer), buildCityLinkVariable(pCity, pActivePlayer));
/*####### Better Old World AI - Base DLL #######
  ### Agent Network Cost Scaling         END ###
  ##############################################*/

                    buildAgentUnlockText(builder, pUnit.player(), pGame, pActivePlayer);

                    buildCanActText(builder, 0, pGame, pActivePlayer, pUnit);
                }

                if (bIncludeEncyclopediaFooter && builder.HasContent && !pManager.App.UserInterface.IsPopupActive("POPUP_HELP") &&
                    hasEncyclopediaEntry(pWidget, pManager.GameClient) && hasHotkey(pManager.Interfaces.Hotkeys, infos().Globals.HOTKEY_HELP_SCREEN))
                {
                    buildDividerText(builder);
                    builder.AddTEXT("TEXT_HELPTEXT_LINK_OPEN_ENCYCLOPEDIA", buildPressHotkeyVariableOrFalse(pManager.Interfaces.Hotkeys, infos().Globals.HOTKEY_HELP_SCREEN));
                }

                return builder;
            }
            else
            {
                return base.buildWidgetHelp(builder, pWidget, pManager, bIncludeEncyclopediaFooter);
            }
        }

        //HelpText.Improvement.cs
        //lines 426-899
        public override TextBuilder buildImprovementBreakdown(TextBuilder builder, ImprovementType eImprovement, SpecialistType eSpecialist, Tile pTile, ClientManager pManager)
        {
            //UnityEngine.Debug.Log("HelpText.buildImprovementBreakdown");

            Game pGame = pManager.GameClient;
            Player pActivePlayer = pManager.activePlayer();

            ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;

            ResourceType eResource = pTile.getResource();
            City pCityTerritory = pTile.cityTerritory();

            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
            {
                int iTotalOutput = pTile.yieldOutputModified(eImprovement, eSpecialist, eLoopYield, bCityEffects: true);
                if (iTotalOutput != 0)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_PER_YEAR", buildYieldValueIconLinkVariable(eLoopYield, iTotalOutput, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame));

                    bool bShowModifiers = false;

                    {
                        int iValue = 0;
                        infos().Helpers.yieldOutputImprovement(eImprovement, eLoopYield, eResource, ref iValue, bIncludeResource: false);

/*####### Better Old World AI - Base DLL #######
  ### TerrainYield fix                 START ###
  ##############################################*/
                        int iValueTerrain = infos().improvement(eImprovement).maaiTerrainYieldOutput[pTile.getTerrain(), eLoopYield];
                        int iValueSum = iValue + iValueTerrain;
                        if (iValueSum != iTotalOutput && iValueSum != 0)
                        { 
                            if ((iValue != 0))
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, false, Constants.YIELDS_MULTIPLIER), buildImprovementLinkVariable(eImprovement, pGame, pTile));
                                }
                            }
                            if (iValueTerrain != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValueTerrain, false, Constants.YIELDS_MULTIPLIER), buildTerrainLinkVariable(pTile.getTerrain(), pTile));
                                }
                            }

                            bShowModifiers = true;
                        }
                        else if (iValueTerrain != 0)
                        {
                            using (buildSecondaryTextScope(builder))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValueTerrain, false, Constants.YIELDS_MULTIPLIER), buildTerrainLinkVariable(pTile.getTerrain(), pTile));
                            }
                        }
/*####### Better Old World AI - Base DLL #######
  ### TerrainYield fix                   END ###
  ##############################################*/
                    }

                    for (int i = 0; i < pGame.getNumOccurrences(); ++i)
                    {
                        OccurrenceData pLoopData = pGame.getOccurrenceDataAt(i);
                        if (pGame.isOccurrenceActive(pLoopData))
                        {
                            InfoOccurrence occurrence = infos().occurrence(pLoopData.meType);
                            int iValue = occurrence.miTileBaseYieldModifier;
                            if (iValue != 0 && pLoopData.isAffectedTile(pTile))
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true, bColor: true), buildOccurrenceLinkVariable(pLoopData.meType, pLoopData));
                                }
                            }
                        }
                    }

                    //{
                    //    int iValue = infos().improvement(eImprovement).maaiTerrainYieldOutput[pTile.getTerrain(), eLoopYield];
                    //    if (iValue != 0)
                    //    {
                    //        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, false, Constants.YIELDS_MULTIPLIER), buildTerrainLinkVariable(pTile.getTerrain(), pTile));
                    //    }
                    //}

                    {
                        int iValue = infos().improvement(eImprovement).maiAdjacentWonderYieldOutput[eLoopYield];
                        if (iValue != 0)
                        {
                            int iCount = pTile.countTeamAdjacentWonders();
                            if (iCount > 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT_WONDER", buildYieldTextVariable((iValue * iCount), true, false, Constants.YIELDS_MULTIPLIER));
                                }

                                bShowModifiers = true;
                            }
                        }
                    }

                    {
                        int iValue = infos().improvement(eImprovement).maiAdjacentResourceYieldOutput[eLoopYield];
                        if (iValue != 0)
                        {
                            int iCount = pTile.countTeamAdjacentResources();
                            if (iCount > 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT_RESOURCE", buildYieldTextVariable((iValue * iCount), true, false, Constants.YIELDS_MULTIPLIER));
                                }

                                bShowModifiers = true;
                            }
                        }
                    }

                    {
                        int iValue = infos().improvement(eImprovement).maiTradeNetworkYieldOutput[eLoopYield];
                        if (iValue != 0)
                        {
                            if (pCityTerritory != null)
                            {
                                if (pTile.tradeNetworkTestImprovement(pCityTerritory.getPlayer(), eImprovement))
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_TRADE_NETWORK", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER));
                                    }

                                    bShowModifiers = true;
                                }
                            }
                        }
                    }

                    for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                    {
                        int iValue = infos().improvement(eImprovement).maaiAdjacentImprovementClassYield[eLoopImprovementClass, eLoopYield];
                        if (iValue != 0)
                        {
                            int iCount = pTile.countTeamAdjacentImprovementClassFinished(eLoopImprovementClass);
                            if (iCount > 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT", buildYieldTextVariable((iValue * iCount), true, false, Constants.YIELDS_MULTIPLIER), buildImprovementClassLinkVariable(eLoopImprovementClass));
                                }

                                bShowModifiers = true;
                            }
                        }
                    }


                    for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                    {
                        int iValue = infos().improvement(eImprovement).maaiAdjacentImprovementYield[eLoopImprovement, eLoopYield];
                        if (iValue != 0)
                        {
                            int iCount = pTile.countTeamAdjacentImprovementFinished(eLoopImprovement);
                            if (iCount > 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT", buildYieldTextVariable((iValue * iCount), true, false, Constants.YIELDS_MULTIPLIER), buildImprovementLinkVariable(eLoopImprovement, pGame));
                                }

                                bShowModifiers = true;
                            }
                        }
                    }

                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        if (eResource != ResourceType.NONE)
                        {
                            int iValue = 0;
                            infos().Helpers.yieldOutputResource(eImprovementClass, eLoopYield, eResource, ref iValue);
                            if (iValue != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildResourceLinkVariable(eResource));
                                }

                                bShowModifiers = true;
                            }
                        }

                        {
                            int iValue = infos().improvementClass(eImprovementClass).maiAdjacentResourceYieldOutput[eLoopYield];
                            if (iValue != 0)
                            {
                                int iCount = pTile.countTeamAdjacentResources();
                                if (iCount > 0)
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT_RESOURCE", buildYieldTextVariable((iValue * iCount), true, false, Constants.YIELDS_MULTIPLIER));
                                    }

                                    bShowModifiers = true;
                                }
                            }
                        }

                        {
                            ReligionType eReligionPrereq = infos().improvement(eImprovement).meReligionPrereq;

                            if (eReligionPrereq != ReligionType.NONE)
                            {
                                for (TheologyType eLoopTheology = 0; eLoopTheology < infos().theologiesNum(); eLoopTheology++)
                                {
                                    if (pGame.isReligionTheology(eReligionPrereq, eLoopTheology))
                                    {
                                        int iValue = infos().improvementClass(eImprovementClass).maaiTheologyYieldOutput[eLoopTheology, eLoopYield];
                                        if (iValue != 0)
                                        {
                                            using (buildSecondaryTextScope(builder))
                                            {
                                                builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildTheologyLinkVariable(eLoopTheology, eReligionPrereq, pCityTerritory?.player() ?? pActivePlayer, true));
                                            }

                                            bShowModifiers = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (pCityTerritory != null)
                        {
                            foreach (EffectCityType eLoopEffectCity in pCityTerritory.getActiveEffectCity())
                            {
                                int iCount = pCityTerritory.getEffectCityCount(eLoopEffectCity);
                                int iValue = infos().effectCity(eLoopEffectCity).maaiImprovementYield[eImprovement, eLoopYield] + infos().effectCity(eLoopEffectCity).maaiImprovementClassYield[eImprovementClass, eLoopYield];
                                if (iValue != 0)
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_X",
                                            buildSignedTextVariable((iCount * iValue), false, Constants.YIELDS_MULTIPLIER),
                                            buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory.governor(), pGame, pActivePlayer),
                                            (iCount > 1 ? TEXTVAR(iCount) : TEXTVAR(false)));
                                    }

                                    bShowModifiers = true;
                                }
                            }
                        }
                    }

                    if (bShowModifiers)
                    {
                        if (pCityTerritory != null)
                        {
                            foreach (EffectCityType eLoopEffectCity in pCityTerritory.getActiveEffectCity())
                            {
                                int iCount = pCityTerritory.getEffectCityCount(eLoopEffectCity);
                                int iValue = pTile.getTileEffectCityModifier(eLoopEffectCity, eImprovement);

                                if (iValue != 0)
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_X",
                                            buildSignedTextVariable((iCount * iValue), true),
                                            buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory.governor(), pGame, pActivePlayer),
                                            (iCount > 1 ? TEXTVAR(iCount) : TEXTVAR(false)));
                                    }
                                }
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).maaiTerrainYieldModifier[pTile.getTerrain(), eLoopYield];
                            if (iValue != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true), buildTerrainLinkVariable(pTile.getTerrain(), pTile));
                                }
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).maaiHeightYieldModifier[pTile.getHeight(), eLoopYield];
                            if (iValue != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true), buildHeightLinkVariable(pTile.getHeight(), pTile));
                                }
                            }
                        }

                        if (pTile.isFreshWaterAccess())
                        {
                            int iValue = infos().improvement(eImprovement).miFreshWaterModifier + infos().improvement(eImprovement).maiYieldFreshWaterModifier[eLoopYield];
                            if (iValue != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true), buildFreshWaterLinkVariable(pTile));
                                }
                            }
                        }

                        if (pTile.isRiver())
                        {
                            int iValue = infos().improvement(eImprovement).miRiverModifier + infos().improvement(eImprovement).maiYieldRiverModifier[eLoopYield];
                            if (iValue != 0)
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true), buildRiverLinkVariable(pTile));
                                }
                            }
                        }

                        for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                        {
                            Tile pAdjacentTile = pTile.tileAdjacent(eLoopDirection, true);

                            if (pAdjacentTile != null)
                            {
                                {
                                    int iValue = infos().improvement(eImprovement).maaiAdjacentHeightYieldModifier[pAdjacentTile.getHeight(), eLoopYield];
                                    if (iValue != 0)
                                    {
                                        using (buildSecondaryTextScope(builder))
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT", buildSignedTextVariable(iValue, true), buildHeightLinkVariable(pAdjacentTile.getHeight()));
                                        }
                                    }
                                }

                                if (pCityTerritory != null)
                                {
                                    if (pAdjacentTile.getTeam() == pCityTerritory.getTeam())
                                    {
                                        if (pAdjacentTile.hasActiveImprovement())
                                        {
                                            int iValue = infos().Helpers.adjacentYieldOutputImprovementModifier(eImprovement, pAdjacentTile.getImprovement());
                                            if (iValue != 0)
                                            {
                                                using (buildSecondaryTextScope(builder))
                                                {
                                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_ADJACENT", buildSignedTextVariable(iValue, true), buildImprovementLinkVariable(pAdjacentTile.getImprovement(), pGame, pAdjacentTile));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (eSpecialist != SpecialistType.NONE)
                        {
                            if (eImprovementClass != ImprovementClassType.NONE)
                            {
                                int iValue = infos().specialist(eSpecialist).maiImprovementClassModifier[eImprovementClass];
                                if (iValue != 0)
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_PERCENT_FROM", buildValueTextVariable((iValue + 100)), buildSpecialistLinkVariable(eSpecialist, pGame));
                                    }
                                }
                            }
                        }
                    }

                    if (pCityTerritory != null)
                    {
                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvement(eImprovement).meEffectCity, false, eLoopYield, pCityTerritory, pManager, iTotalOutput, pTile);

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvementClass(eImprovementClass).meEffectCity, false, eLoopYield, pCityTerritory, pManager, iTotalOutput);

                            if (eResource != ResourceType.NONE)
                            {
                                buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvementClass(eImprovementClass).maeResourceCityEffect[eResource], true, eLoopYield, pCityTerritory, pManager, pTile: pTile);


                                if (pActivePlayer.isPlayerOption(infos().Globals.ADVANCED_HELP) && infos().resource(eResource).maiYieldNoImprovement[eLoopYield] != 0)
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_REPLACES_UNIMPROVED", buildYieldIconTextVariable(eLoopYield, infos().resource(eResource).maiYieldNoImprovement[eLoopYield], true, false, Constants.YIELDS_MULTIPLIER), buildResourceLinkVariable(eResource));
                                    }
                                }
                            }

                            {
                                ReligionType ePrereqReligion = infos().improvement(eImprovement).meReligionPrereq;

                                if (ePrereqReligion != ReligionType.NONE)
                                {
                                    for (TheologyType eLoopTheology = 0; eLoopTheology < infos().theologiesNum(); eLoopTheology++)
                                    {
                                        if (pGame.isReligionTheology(ePrereqReligion, eLoopTheology))
                                        {
                                            EffectCityType eEffectCity = mInfos.improvementClass(eImprovementClass).maeTheologyCityEffect[eLoopTheology];

                                            if (eEffectCity != EffectCityType.NONE)
                                            {
                                                buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCity, true, eLoopYield, pCityTerritory, pManager);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        {
                            EffectPlayerType eEffectPlayer = infos().improvement(eImprovement).meEffectPlayer;

                            if (eEffectPlayer != EffectPlayerType.NONE)
                            {
                                {
                                    EffectCityType eEffectCity = infos().effectPlayer(eEffectPlayer).meEffectCity;

                                    if (eEffectCity != EffectCityType.NONE)
                                    {
                                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCity, true, eLoopYield, pCityTerritory, pManager);
                                    }
                                }
                                {
                                    EffectCityType eEffectCityExtra = infos().effectPlayer(eEffectPlayer).meEffectCityExtra;

                                    if (eEffectCityExtra != EffectCityType.NONE)
                                    {
                                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCityExtra, true, eLoopYield, pCityTerritory, pManager);
                                    }
                                }
                                if (pCityTerritory.isCapital())
                                {
                                    EffectCityType eEffectCity = infos().effectPlayer(eEffectPlayer).meCapitalEffectCity;

                                    if (eEffectCity != EffectCityType.NONE)
                                    {
                                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCity, true, eLoopYield, pCityTerritory, pManager);
                                    }
                                }
                                if (pCityTerritory.isConnected())
                                {
                                    EffectCityType eEffectCity = infos().effectPlayer(eEffectPlayer).meConnectedEffectCity;

                                    if (eEffectCity != EffectCityType.NONE)
                                    {
                                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCity, true, eLoopYield, pCityTerritory, pManager);
                                    }
                                }
                                if (!pCityTerritory.hasGovernor())
                                {
                                    EffectCityType eEffectCity = infos().effectPlayer(eEffectPlayer).meNoGovernorEffectCity;

                                    if (eEffectCity != EffectCityType.NONE)
                                    {
                                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCity, true, eLoopYield, pCityTerritory, pManager);
                                    }
                                }
                                if (pCityTerritory.hasPlayer() && pCityTerritory.player().hasStateReligion())
                                {
                                    EffectCityType eEffectCity = infos().effectPlayer(eEffectPlayer).meStateReligionEffectCity;

                                    if (eEffectCity != EffectCityType.NONE)
                                    {
                                        ReligionType eReligion = pCityTerritory.player().stateReligion().meType;

                                        if (eReligion != ReligionType.NONE && pCityTerritory.isReligion(eReligion))
                                        {
                                            buildImprovementBreakdownEffectCityHelp(builder, eImprovement, eEffectCity, true, eLoopYield, pCityTerritory, pManager);
                                        }
                                    }
                                }
                            }
                        }

                        if (eSpecialist != SpecialistType.NONE)
                        {
                            buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().specialist(eSpecialist).meEffectCity, true, eLoopYield, pCityTerritory, pManager);
                            buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().specialist(eSpecialist).meEffectCityExtra, true, eLoopYield, pCityTerritory, pManager);
                            if (infos().specialist(eSpecialist).meClass != SpecialistClassType.NONE)
                            {
                                buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().specialistClass(infos().specialist(eSpecialist).meClass).meEffectCity, true, eLoopYield, pCityTerritory, pManager);
                            }

                            if (eResource != ResourceType.NONE)
                            {
                                buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().specialistClass(infos().specialist(eSpecialist).meClass).maeResourceCityEffect[eResource], true, eLoopYield, pCityTerritory, pManager);
                            }
                        }
                    }
                }
            }
            return builder;
        }

        //lines 943-1861 + 1864-2183 (buildImprovementPotentialBonuses)
        public override TextBuilder buildImprovementHelp(TextBuilder builder, ImprovementType eImprovement, Tile pTile, ClientManager pManager, bool bName = true, bool bCosts = true, bool bDetails = true, bool bEncyclopedia = false, bool bShowTotal = false, TextBuilder.ScopeType scopeType = TextBuilder.ScopeType.NONE)
        {
            //UnityEngine.Debug.Log("HelpText.buildImprovementHelp");

            //using (new UnityProfileScope("HelpText.buildImprovementHelp"))
            using (var effectListScoped = CollectionCache.GetListScoped<EffectCityType>())
            {
                //bool bInclucePotentialBonusesTitle = true;

                Game pGame = pManager.GameClient;

                Player pPlayer = ((pTile != null) ? pTile.owner() : null);

                Player pActivePlayer = pManager.activePlayer();
                if (pPlayer == null)
                {
                    pPlayer = pActivePlayer;
                }

                Unit pSelectedUnit = pManager.Selection.getSelectedUnit();

                City pCityTerritory = ((pTile != null) ? pTile.cityTerritory() : null);

                ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;

                ReligionType eReligionPrereq = infos().improvement(eImprovement).meReligionPrereq;

                getImprovementEffectCityList(eImprovement, pManager, effectListScoped.Value);

                BetterAIInfoImprovement pImprovementInfo = ((BetterAIInfoImprovement)(infos().improvement(eImprovement)));

                using (builder.BeginScope(scopeType))
                {
                    if (bName)
                    {
                        builder.Add(buildTitleVariable(TEXTVAR_TYPE(mInfos.Helpers.getImprovementName(eImprovement, pGame))));
                    }

                    if ((pTile != null) && pTile.isInfoVisible(pActivePlayer.getTeam(), pManager))
                    {
                        if (pActivePlayer.isPlayerOption(infos().Globals.ADVANCED_HELP))
                        {
                            if (eImprovement != pTile.getImprovement())
                            {
                                TextVariable netYieldVariable = buildAddImprovementYieldVariable(eImprovement, pTile, pGame);
                                if (!netYieldVariable.IsNullOrEmpty())
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_SPECIALIST_NET_YIELD", netYieldVariable);
                                    buildDividerText(builder);
                                }
                            }
                        }

                        using (TextBuilder subBuilder = TextBuilder.GetTextBuilder(TextManager))
                        {
                            buildImprovementBreakdown(subBuilder, eImprovement, pTile.getFreeSpecialist(eImprovement), pTile, pManager);

                            if (subBuilder.HasContent)
                            {
                                builder.Add(subBuilder.ProfiledToString());
                                buildDividerText(builder);
                            }
                        }
                    }

                    using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                    {
                        {
                            int iValue = infos().improvement(eImprovement).miDefenseModifier;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_DEFENSE_MODIFIER", buildDefenseValueLinkVariable(iValue, true));
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miDefenseModifierFriendly;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_DEFENSE_MODIFIER_FRIENDLY", buildDefenseValueLinkVariable(iValue, true));
                            }
                        }

                        if (infos().improvement(eImprovement).mbFreshWaterSource)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_FRESH_WATER_SOURCE", buildFreshWaterLinkVariable(pTile));
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miVisionChange;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_VISION_CHANGE", buildSignedTextVariable(iValue));
                            }
                        }

                        if (infos().improvement(eImprovement).mbTribe)
                        {
                            if ((pTile != null) && pTile.isTribeSite())
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_PRODUCES_TRIBE_UNITS", buildTribeLinkVariable(pTile.getTribeSite(), pGame));
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_PRODUCES_GENERIC_TRIBE_UNITS");
                            }
                        }

                        if (infos().improvement(eImprovement).mbBonus)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_BONUS");
                        }

                        if ((pTile == null) || !(pTile.isInfoVisible(pActivePlayer.getTeam(), pManager)))
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_OUTPUT_YIELDS", buildTurnScaleName(pGame)), scopeKey: "YIELD-LIST"))
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iOutput = 0;
                                    infos().Helpers.yieldOutputImprovement(eImprovement, eLoopYield, pTile?.getResource() ?? ResourceType.NONE, ref iOutput, bIncludeResource: false);
                                    if (iOutput != 0)
                                    {
                                        builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }

                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_CONSUMPTION_YIELDS", buildTurnScaleName(pGame)), scopeKey: "YIELD-LIST"))
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iOutput = infos().improvement(eImprovement).maiYieldConsumption[eLoopYield];
                                    if (iOutput != 0)
                                    {
                                        builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }

                        foreach (EffectCityType eEffectCity in effectListScoped.Value)
                        {
                            buildEffectCityHelpYields(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), pActivePlayer, bShowTotal);
                        }

                        foreach (EffectCityType eEffectCity in effectListScoped.Value)
                        {
                            buildEffectCityHelpNoYields(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), pActivePlayer);
                        }

                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                foreach (ImprovementType eAdjacentImprovement in infos().improvement(eImprovement).maeAdjacentImprovementSpecialists)
                                {
                                    SpecialistType eSpecialist = infos().improvement(eAdjacentImprovement).meSpecialist;
                                    if (eSpecialist != SpecialistType.NONE)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_BONUS_FREE_IMPROVEMENT_SPECIALIST_ADJACENT", buildSpecialistLinkVariable(eSpecialist, pGame), buildImprovementLinkVariable(eAdjacentImprovement, pGame));
                                    }
                                }
                            }
                        }

                        if (infos().improvement(eImprovement).maeAdjacentImprovementValid.Count > 0)
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_VALID_ADJACENT_IMPROVEMENTS")))
                            {
                                foreach (ImprovementType eLoopImprovement in infos().improvement(eImprovement).maeAdjacentImprovementValid)
                                {
                                    builder.Add(buildImprovementLinkVariable(eLoopImprovement, pGame, pTile));
                                }
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miUnitHeal;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_HEALS_UNIT", buildSignedTextVariable(iValue));
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miAdjacentUnitXP;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_ADJACENT_UNIT_XP", buildSignedTextVariable(iValue), buildTurnScaleName(pGame));
                            }
                        }

                        {
                            using (var hashSetScope = CollectionCache.GetHashSetScoped<UnitTraitType>())
                            {
                                HashSet<UnitTraitType> seUnitTraitsAdded = hashSetScope.Value;

                                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                                {
                                    if (!(seUnitTraitsAdded.Contains(eLoopUnitTrait)))
                                    {
                                        int iValue = infos().improvement(eImprovement).maiUnitTraitHeal[eLoopUnitTrait];
                                        if (iValue != 0)
                                        {
                                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_UNIT_TRAIT_HEALS_UNIT", buildSignedTextVariable(iValue))))
                                            {
                                                {
                                                    builder.Add(buildUnitTraitLinkVariable(eLoopUnitTrait));
                                                    seUnitTraitsAdded.Add(eLoopUnitTrait);
                                                }

                                                for (UnitTraitType eOtherUnitTrait = 0; eOtherUnitTrait < infos().unitTraitsNum(); eOtherUnitTrait++)
                                                {
                                                    if (eOtherUnitTrait != eLoopUnitTrait)
                                                    {
                                                        if (infos().improvement(eImprovement).maiUnitTraitHeal[eOtherUnitTrait] == iValue)
                                                        {
                                                            builder.Add(buildUnitTraitLinkVariable(eOtherUnitTrait));
                                                            seUnitTraitsAdded.Add(eOtherUnitTrait);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        {
                            using (var hashSetScope = CollectionCache.GetHashSetScoped<UnitTraitType>())
                            {
                                HashSet<UnitTraitType> seUnitTraitsAdded = hashSetScope.Value;

                                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                                {
                                    if (!(seUnitTraitsAdded.Contains(eLoopUnitTrait)))
                                    {
                                        int iValue = infos().improvement(eImprovement).maiUnitTraitXP[eLoopUnitTrait];
                                        if (iValue != 0)
                                        {
                                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_UNIT_TRAIT_XP", buildSignedTextVariable(iValue), buildTurnScaleName(pGame))))
                                            {
                                                {
                                                    builder.Add(buildUnitTraitLinkVariable(eLoopUnitTrait));
                                                    seUnitTraitsAdded.Add(eLoopUnitTrait);
                                                }

                                                for (UnitTraitType eOtherUnitTrait = 0; eOtherUnitTrait < infos().unitTraitsNum(); eOtherUnitTrait++)
                                                {
                                                    if (eOtherUnitTrait != eLoopUnitTrait)
                                                    {
                                                        if (infos().improvement(eImprovement).maiUnitTraitXP[eOtherUnitTrait] == iValue)
                                                        {
                                                            builder.Add(buildUnitTraitLinkVariable(eOtherUnitTrait));
                                                            seUnitTraitsAdded.Add(eOtherUnitTrait);
                                                        }
                                                    }
                                                }
                                            }
                                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_EFFECT_CITY_HELP_SPAWN_POINT_UNIT_TRAIT")))
                                            {
                                                foreach (UnitTraitType eXpTrait in seUnitTraitsAdded)
                                                {
                                                    builder.Add(buildUnitTraitLinkVariable(eXpTrait));
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            {
                                EffectPlayerType eEffectPlayer = infos().improvement(eImprovement).meEffectPlayer;

                                if (eEffectPlayer != EffectPlayerType.NONE)
                                {
                                    buildEffectPlayerHelp(builder, eEffectPlayer, pGame, pPlayer, pActivePlayer, bAllCities: true);
                                }
                            }

                            {
                                BonusType eBonus = infos().improvement(eImprovement).meBonus;

                                if (eBonus != BonusType.NONE)
                                {
                                    buildBonusHelp(builder, eBonus, pGame, pPlayer, pActivePlayer, pTile, bName: false, startLineVariable: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_BONUS_ON_COMPLETION"));
                                }
                            }

                            {
                                BonusType eBonusCities = infos().improvement(eImprovement).meBonusCities;

                                if (eBonusCities != BonusType.NONE)
                                {
                                    buildBonusHelp(builder, eBonusCities, pGame, pPlayer, pActivePlayer, bShowCity: false, startLineVariable: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_BONUS_ALL_CITIES"));
                                }
                            }

                            if (bDetails)
                            {
                                {
                                    ReligionType eReligionSpread = pGame?.getImprovementReligionSpread(eImprovement) ?? infos().improvement(eImprovement).meReligionSpread;

                                    if (eReligionSpread != ReligionType.NONE)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_SPREADS_RELIGION", buildReligionLinkVariable(eReligionSpread, pGame, pActivePlayer));

                                        {
                                            EffectCityType eHolyCityEffectCity = infos().religion(eReligionSpread).meHolyCityEffectCity;

                                            if (eHolyCityEffectCity != EffectCityType.NONE && !pGame.isReligionFounded(eReligionSpread))
                                            {
                                                using (builder.BeginScope(TextBuilder.ScopeType.INDENTED_BULLET))
                                                {
                                                    buildEffectCityHelp(builder, eHolyCityEffectCity, pCityTerritory, pActivePlayer, bSkipImpossible: !bEncyclopedia, bShowTotal: false, bSkipZero: !bEncyclopedia);
                                                }
                                            }

                                            EffectCityType eEffectCity = infos().religion(eReligionSpread).meEffectCity;

                                            if (eEffectCity != EffectCityType.NONE)
                                            {
                                                using (builder.BeginScope(TextBuilder.ScopeType.INDENTED_BULLET))
                                                {
                                                    buildEffectCityHelp(builder, eEffectCity, pCityTerritory, pActivePlayer, bSkipImpossible: !bEncyclopedia, bShowTotal: false, bSkipZero: !bEncyclopedia);
                                                }
                                            }
                                        }
                                    }
                                }

                                {
                                    int iValue = infos().improvement(eImprovement).miOpinionReligion;
                                    if (iValue != 0)
                                    {
                                        if (pGame?.isCharacters() ?? true)
                                        {
                                            ReligionType eReligionSpread = pGame?.getImprovementReligionSpread(eImprovement) ?? infos().improvement(eImprovement).meReligionSpread;

                                            if ((eReligionSpread != ReligionType.NONE) || (eReligionPrereq != ReligionType.NONE))
                                            {
                                                if (eReligionSpread == eReligionPrereq)
                                                {
                                                    builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_OPINION_RELIGION", buildSignedTextVariable(iValue), buildReligionLinkVariable(eReligionSpread, pGame, pActivePlayer));
                                                }
                                                else
                                                {
                                                    if (eReligionSpread != ReligionType.NONE)
                                                    {
                                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_OPINION_RELIGION", buildSignedTextVariable(iValue), buildReligionLinkVariable(eReligionSpread, pGame, pActivePlayer));
                                                    }
                                                    if (eReligionPrereq != ReligionType.NONE)
                                                    {
                                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_OPINION_RELIGION", buildSignedTextVariable(iValue), buildReligionLinkVariable(eReligionPrereq, pGame, pActivePlayer));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }

/*####### Better Old World AI - Base DLL #######
  ### leaving in some info             START ###
  ##############################################*/
                            if (bEncyclopedia && infos().improvement(eImprovement).mbNoAdjacentReligion)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_ADJACENT_RELIGION");
                            }
/*####### Better Old World AI - Base DLL #######
  ### leaving in some info               END ###
  ##############################################*/

                            for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                            {
                                if (infos().unit(eLoopUnit).meImprovementPrereq == eImprovement)
                                {
                                    if ((pPlayer != null) ? pPlayer.canEverBuildUnit(eLoopUnit) : true)
                                    {
                                        if (isContentEnabled(infos().unit(eLoopUnit).meGameContentRequired, pGame))
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_UNLOCKS_UNIT", buildUnitTypeLinkVariable(eLoopUnit, pGame, pCity: pCityTerritory));
                                        }
                                    }
                                }
                            }

                            if (infos().improvement(eImprovement).mbHeal)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HEAL");
                            }

                            if (infos().improvement(eImprovement).mbRemovePillage)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_REMOVE_PILLAGE");
                            }

                            if (infos().improvement(eImprovement).mbCanal)
                            {
                                builder.Add(buildCanalLinkVariable());
                            }

                            if (infos().improvement(eImprovement).mbRoadFree)
                            {
                                builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ADDS_ROAD", buildRoadLinkVariable()));
                            }

                            if (infos().improvement(eImprovement).mbIgnoreZOC)
                            {
                                builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_IGNORES_ZOC", buildZOCLinkVariable()));
                            }


/*####### Better Old World AI - Base DLL #######
  ### Improvement gives Unit Effect    START ###
  ##############################################*/
                            if (pImprovementInfo.meApplyEffectUnit != EffectUnitType.NONE)
                            {
                                builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_EFFECTUNIT_FRIENDLY", buildEffectUnitLinkVariable(pImprovementInfo.meApplyEffectUnit)));
                            }
                            
/*####### Better Old World AI - Base DLL #######
  ### Improvement gives Unit Effect      END ###
  ##############################################*/

                            if (!(infos().improvement(eImprovement).mbTribe))
                            {
                                int iValue = infos().improvement(eImprovement).miUnitTurns;
                                if (iValue > 0)
                                {
                                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA_OR, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_CREATES_UNITS", iValue, buildTurnScaleName(pGame, iValue))))
                                    {
                                        for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                                        {
                                            if (infos().improvement(eImprovement).maiUnitDie[eLoopUnit] > 0)
                                            {
                                                builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, pCity: pCityTerritory));
                                            }
                                        }
                                    }
                                }
                            }

                            {
                                SpecialistType eSpecialist = infos().improvement(eImprovement).meSpecialist;

                                if (eSpecialist != SpecialistType.NONE && (pTile == null || pTile.getFreeSpecialist(eImprovement) == SpecialistType.NONE))
                                {
                                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_ENABLES", buildSpecialistLinkVariable(eSpecialist, pGame).SetKey("LINK"));

                                        if (pTile != null)
                                        {
                                            if (pTile.hasResource())
                                            {
                                                EffectCityType eEffectCity = infos().specialistClass(infos().specialist(eSpecialist).meClass).maeResourceCityEffect[pTile.getResource()];

                                                if (eEffectCity != EffectCityType.NONE)
                                                {
                                                    buildEffectCityHelp(builder, eEffectCity, pCityTerritory, pActivePlayer, bSkipImpossible: !bEncyclopedia, bShowTotal: false, bSkipZero: !bEncyclopedia);
                                                }
                                            }
                                        }
                                    }
/*####### Better Old World AI - Base DLL #######
  ### show Specialist Prod Discount    START ###
  ##############################################*/
                                    {
                                        int iSpecialistCostExtra = infos().improvement(eImprovement).miSpecialistCost;
                                        if (iSpecialistCostExtra != 0)
                                        {
                                            SpecialistClassType eSpecialistClass = infos().specialist(eSpecialist).meClass;
                                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_SPECIALIST_COST", buildYieldValueIconLinkVariable(infos().Globals.CIVICS_YIELD, iSpecialistCostExtra), (eSpecialistClass != SpecialistClassType.NONE) ? buildSpecialistClassLinkVariable(eSpecialistClass, pTile) : buildSpecialistLinkVariable(eSpecialist, pGame, pTile));
                                        }
                                    }
/*####### Better Old World AI - Base DLL #######
  ### show Specialist Prod Discount      END ###
  ##############################################*/
                                }
                            }

                            if (bDetails)
                            {
                                using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_ENABLES"), scopeKey: "LINK"))
                                {
                                    for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                                    {
                                        if (infos().improvement(eLoopImprovement).meImprovementPrereq == eImprovement)
                                        {
                                            builder.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
                                        }

                                        if (infos().improvement(eLoopImprovement).meEffectCityPrereq != EffectCityType.NONE && infos().improvement(eImprovement).meEffectPlayer != EffectPlayerType.NONE)
                                        {
                                            if (infos().improvement(eLoopImprovement).meEffectCityPrereq == infos().effectPlayer(infos().improvement(eImprovement).meEffectPlayer).meEffectCity)
                                            {
                                                builder.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
                                            }
                                        }
                                    }
                                }
                            }

                            using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementType>())
                            {
                                HashSet<ImprovementType> seImprovementsAdded = hashSetScope.Value;
                                for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                                {
                                    if (!(seImprovementsAdded.Contains(eLoopImprovement)) && (eLoopImprovement != eImprovement))
                                    {
                                        if ((pPlayer != null) ? pPlayer.isNationImprovement(eLoopImprovement) : true)
                                        {
                                            int iValue = infos().improvement(eImprovement).maiAdjacentImprovementModifier[eLoopImprovement];
                                            if (iValue != 0)
                                            {
                                                using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_FOR_ADJACENT_IMPROVEMENT_MODIFIER", buildSignedTextVariable(iValue, true))))
                                                {
                                                    builder.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
                                                    seImprovementsAdded.Add(eLoopImprovement);

                                                    for (ImprovementType eOtherImprovement = 0; eOtherImprovement < infos().improvementsNum(); eOtherImprovement++)
                                                    {
                                                        if (eOtherImprovement != eLoopImprovement)
                                                        {
                                                            if ((pPlayer != null) ? pPlayer.isNationImprovement(eOtherImprovement) : true)
                                                            {
                                                                if (infos().improvement(eImprovement).maiAdjacentImprovementModifier[eOtherImprovement] == iValue)
                                                                {
                                                                    if (isContentEnabled(infos().improvement(eImprovement).meGameContentRequired, pGame))
                                                                    {
                                                                        builder.Add(buildImprovementLinkVariable(eOtherImprovement, pGame));
                                                                        seImprovementsAdded.Add(eOtherImprovement);
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
                            }

                            using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementClassType>())
                            {
                                HashSet<ImprovementClassType> seImprovementClassessAdded = hashSetScope.Value;
                                for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                                {
                                    if (!(seImprovementClassessAdded.Contains(eLoopImprovementClass)) && (eLoopImprovementClass != eImprovementClass))
                                    {
                                        int iValue = infos().improvement(eImprovement).maiAdjacentImprovementClassModifier[eLoopImprovementClass];
                                        if (iValue != 0)
                                        {
                                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_FOR_ADJACENT_IMPROVEMENT_CLASS_MODIFIER", buildSignedTextVariable(iValue, true))))
                                            {
                                                builder.Add(buildImprovementClassLinkVariable(eLoopImprovementClass));
                                                seImprovementClassessAdded.Add(eLoopImprovementClass);

                                                for (ImprovementClassType eOtherImprovementClass = 0; eOtherImprovementClass < infos().improvementClassesNum(); eOtherImprovementClass++)
                                                {
                                                    if (eOtherImprovementClass != eLoopImprovementClass)
                                                    {
                                                        if (infos().improvement(eImprovement).maiAdjacentImprovementClassModifier[eOtherImprovementClass] == iValue)
                                                        {
                                                            if (isContentEnabled(infos().improvement(eImprovement).meGameContentRequired, pGame))
                                                            {
                                                                builder.Add(buildImprovementClassLinkVariable(eOtherImprovementClass));
                                                                seImprovementClassessAdded.Add(eOtherImprovementClass);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }



                            if (eImprovementClass != ImprovementClassType.NONE)
                            {
                                using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementClassType>())
                                {
                                    HashSet<ImprovementClassType> seImprovementClassessAdded = hashSetScope.Value;
                                    for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                                    {
                                        if (!(seImprovementClassessAdded.Contains(eLoopImprovementClass)) && (eLoopImprovementClass != eImprovementClass))
                                        {
                                            int iValue = infos().improvementClass(eImprovementClass).maiAdjacentImprovementClassModifier[eLoopImprovementClass];
                                            if (iValue != 0)
                                            {
                                                using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_FOR_ADJACENT_IMPROVEMENT_CLASS_MODIFIER", buildSignedTextVariable(iValue, true))))
                                                {
                                                    builder.Add(buildImprovementClassLinkVariable(eLoopImprovementClass));
                                                    seImprovementClassessAdded.Add(eLoopImprovementClass);

                                                    for (ImprovementClassType eOtherImprovementClass = 0; eOtherImprovementClass < infos().improvementClassesNum(); eOtherImprovementClass++)
                                                    {
                                                        if (eOtherImprovementClass != eLoopImprovementClass)
                                                        {
                                                            if (infos().improvementClass(eImprovementClass).maiAdjacentImprovementClassModifier[eOtherImprovementClass] == iValue)
                                                            {
                                                                builder.Add(buildImprovementClassLinkVariable(eOtherImprovementClass));
                                                                seImprovementClassessAdded.Add(eOtherImprovementClass);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (infos().improvementClass(eImprovementClass).maeResourceCityEffect.Count > 0 && pCityTerritory != null && pTile != null && pTile.hasResource())
                                {
                                    EffectCityType eLoopEffectCity = infos().improvementClass(eImprovementClass).maeResourceCityEffect[pTile.getResource()];
                                    if (eLoopEffectCity != EffectCityType.NONE)
                                    {
                                        using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                                        {
                                            foreach (EffectCityType eOtherEffectCity in pCityTerritory.getActiveEffectCity())
                                            {
                                                EffectCityType eEffectCity = infos().effectCity(eOtherEffectCity).maeEffectCityEffectCity[eLoopEffectCity];
                                                if (eEffectCity != EffectCityType.NONE)
                                                {
                                                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_NO_YIELDS_EFFECT_CITY_EFFECT_CITY", buildEffectCityLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory.governor()), buildEffectCityLinkVariable(eEffectCity, pCityTerritory, pCityTerritory.governor()));
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                        {
                            ImprovementType eDevelopImprovement = mInfos.Helpers.getDevelopImprovement(eImprovement, pGame?.getTribeLevel() ?? TribeLevelType.NONE);

                            if (eDevelopImprovement != ImprovementType.NONE)
                            {
                                TextVariable developImprovement = buildImprovementLinkVariable(eDevelopImprovement, pGame, pTile);

                                int iDevelopTurns = infos().improvement(eImprovement).miDevelopTurns;
                                if (pCityTerritory != null && infos().improvement(eImprovement).meClass != ImprovementClassType.NONE)
                                {
                                    iDevelopTurns -= pCityTerritory.getImprovementClassDevelopChange(infos().improvement(eImprovement).meClass);
                                }

                                if (pTile != null && pTile.getImprovement() == eImprovement)
                                {
                                    int iTurnsLeft = infos().improvement(eImprovement).miDevelopTurns - pTile.getImprovementDevelopTurns();
                                    builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_DEVELOP_TURNS", developImprovement, TEXTVAR(iTurnsLeft), buildTurnScaleName(pGame, iTurnsLeft));
                                }
                                else if (infos().improvement(eImprovement).miDevelopRand != 0)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_DEVELOP_TURNS_RANDOM", developImprovement, TEXTVAR(iDevelopTurns - infos().improvement(eImprovement).miDevelopRand), iDevelopTurns);
                                }
                                else
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_DEVELOP_TURNS", developImprovement, TEXTVAR(iDevelopTurns), buildTurnScaleName(pGame, iDevelopTurns));
                                }
                            }
                        }

                        if (infos().improvement(eImprovement).meEffectPlayerPrereq == EffectPlayerType.NONE || infos().Helpers.canEverHaveEffectPlayer(pPlayer, infos().improvement(eImprovement).meEffectPlayerPrereq))
                        {
                            if ((pTile == null) || (pTile.getImprovement() != eImprovement))
                            {
                                if (bCosts)
                                {
                                    buildImprovementCostsHelp(builder, eImprovement, pTile, pManager, true);
                                }

                                if (bEncyclopedia)
                                {
                                    using (TextBuilder subCommaText = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        bool bSelectedCantBuild = (pSelectedUnit != null && !pSelectedUnit.canBuildImprovementType(eImprovement));
                                        using (subCommaText.BeginScope(TextBuilder.ScopeType.COMMA))
                                        {
                                            for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                                            {
                                                if (infos().Helpers.canUnitBuildImprovement(eLoopUnit, eImprovement, bIncludeWorkers: bSelectedCantBuild))
                                                {
                                                    subCommaText.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame));
                                                }
                                            }
                                        }

                                        if (subCommaText.HasContent)
                                        {
                                            TextVariable requiresUnit = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_BUILT_BY", subCommaText.ToTextVariable());
                                            if (bSelectedCantBuild)
                                            {
                                                requiresUnit = buildWarningTextVariable(requiresUnit);
                                            }
                                            builder.Add(requiresUnit);
                                        }
                                    }
                                }
                            }
                        }
                    }

/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement       START ###
  ##############################################*/
                    //BetterAIInfoImprovement pImprovementInfo = ((BetterAIInfoImprovement)(infos().improvement(eImprovement)));

                    if (pImprovementInfo.meBonusAdjacentImprovement != ImprovementType.NONE)
                    {
                        builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ADDS_ADJACENT_BONUS_IMPROVEMENT", TEXTVAR_TYPE(infos().improvement(pImprovementInfo.meBonusAdjacentImprovement).mName)));
                    }
                    else if (pImprovementInfo.meBonusAdjacentImprovementClass != ImprovementClassType.NONE)
                    {
                        builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ADDS_ADJACENT_BONUS_IMPROVEMENT", TEXTVAR_TYPE(infos().improvementClass(pImprovementInfo.meBonusAdjacentImprovementClass).mName)));
                    }
/*####### Better Old World AI - Base DLL #######
  ### Bonus adjacent Improvement         END ###
  ##############################################*/

                    if (infos().improvement(eImprovement).mbWonder)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_WONDER");
                    }

                    if (bDetails)
                    {
                        if (infos().improvement(eImprovement).mbUrban)
                        {
                            if (!(infos().improvement(eImprovement).mbRequiresUrban) && infos().improvement(eImprovement).mbBuild)
                            {
                                TextVariable urbanVariable = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_URBAN_BUILDING_ANYWHERE", buildTerrainLinkVariable(infos().Globals.URBAN_TERRAIN));
                                builder.Add(urbanVariable);
                            }
                        }
                        else if (infos().improvement(eImprovement).mbSpreadsBorders)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_SPREADS_BORDERS");
                        }

                        if (infos().improvement(eImprovement).mbPermanent && !infos().improvement(eImprovement).mbRemoveBorder)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_PERMANENT_BUILDING");
                        }

                        if (bEncyclopedia)
                        {
                            if (!infos().improvement(eImprovement).mbBuild)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_NO_BUILD");
                            }
                        }
                    }
                }

                using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                using (TextBuilder potentialText = TextBuilder.GetTextBuilder(TextManager))
                {
                    using (subText.BeginScope(scopeType))
                    using (subText.BeginScope(TextBuilder.ScopeType.BULLET))
                    {
                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_WONDER", buildTurnScaleName(pGame))))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iOutput = infos().improvement(eImprovement).maiAdjacentWonderYieldOutput[eLoopYield];
                                if (iOutput != 0)
                                {
                                    subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                }
                            }
                        }

                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_RESOURCE", buildTurnScaleName(pGame))))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iOutput = infos().improvement(eImprovement).maiAdjacentResourceYieldOutput[eLoopYield];
                                if (iOutput != 0)
                                {
                                    subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                }
                            }
                        }

                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_TRADE_NETWORK")))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iOutput = infos().improvement(eImprovement).maiTradeNetworkYieldOutput[eLoopYield];
                                if (iOutput != 0)
                                {
                                    subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                }
                            }
                        }

                        for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_IMPROVEMENTCLASS", buildTurnScaleName(pGame), buildImprovementClassLinkVariable(eLoopImprovementClass))))
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iOutput = infos().improvement(eImprovement).maaiAdjacentImprovementClassYield[eLoopImprovementClass, eLoopYield];
                                    if (iOutput != 0)
                                    {
                                        subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }

                        for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                        {
                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_IMPROVEMENTCLASS", buildTurnScaleName(pGame), buildImprovementLinkVariable(eLoopImprovement, pGame))))
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iOutput = infos().improvement(eImprovement).maaiAdjacentImprovementYield[eLoopImprovement, eLoopYield];
                                    if (iOutput != 0)
                                    {
                                        subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
/*####### Better Old World AI - Base DLL #######
  ### leaving in some info             START ###
  ##############################################*/
                            if (pTile == null)
                            {
                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                {
                                    if (!isContentEnabled(infos().resource(eLoopResource).meGameContentRequired))
                                    {
                                        continue;
                                    }
                                    using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: buildColonSpaceOne(buildResourceLinkVariable(eLoopResource), null))) // arg1 for buildColonSpaceOne will be filled with list
                                    {
                                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                        {
                                            int iValue = 0;
                                            infos().Helpers.yieldOutputResource(eImprovementClass, eLoopYield, eLoopResource, ref iValue);
                                            if (iValue != 0)
                                            {
                                                subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                            }
                                        }
/*####### Better Old World AI - Base DLL #######
  ### show abNoBaseOutput              START ###
  ##############################################*/
                                        if (infos().improvement(eImprovement).mabNoBaseOutput[(int)eLoopResource])
                                        {
                                            subText.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_REPLACES_BASE_IMPROVEMENT_OUTPUT"));
                                        }
/*####### Better Old World AI - Base DLL #######
  ### show abNoBaseOutput                END ###
  ##############################################*/
                                    }
                                }
                            }
/*####### Better Old World AI - Base DLL #######
  ### leaving in some info               END ###
  ##############################################*/

                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_RESOURCE", buildTurnScaleName(pGame))))
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iOutput = infos().improvementClass(eImprovementClass).maiAdjacentResourceYieldOutput[eLoopYield];
                                    if (iOutput != 0)
                                    {
                                        subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }

                            for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: buildColonSpaceOne(buildTerrainLinkVariable(eLoopTerrain), null)))
                                {
                                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                    {
                                        int iOutput = infos().improvement(eImprovement).maaiTerrainYieldOutput[eLoopTerrain, eLoopYield];
                                        if (iOutput != 0)
                                        {
                                            subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                        }
                                    }
                                }
                            }

                            for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iValue = infos().improvement(eImprovement).maaiTerrainYieldModifier[eLoopTerrain, eLoopYield];
                                    if (iValue != 0)
                                    {
                                        subText.Add(buildColonSpaceOne(buildTerrainLinkVariable(eLoopTerrain), buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, bPercent: true)));
                                    }
                                }
                            }

                            for (HeightType eLoopHeight = 0; eLoopHeight < infos().heightsNum(); eLoopHeight++)
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iValue = infos().improvement(eImprovement).maaiHeightYieldModifier[eLoopHeight, eLoopYield];
                                    if (iValue != 0)
                                    {
                                        subText.Add(buildColonSpaceOne(buildHeightLinkVariable(eLoopHeight), buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, bPercent: true)));
                                    }
                                }
                            }
                        }

                        for (HeightType eLoopHeight = 0; eLoopHeight < infos().heightsNum(); eLoopHeight++)
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iValue = infos().improvement(eImprovement).maaiAdjacentHeightYieldModifier[eLoopHeight, eLoopYield];
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_ADJACENT_COLON", buildHeightLinkVariable(eLoopHeight), buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, bPercent: true));
                                }
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miFreshWaterModifier;
                            if (iValue != 0)
                            {
                                subText.Add(buildColonSpaceOne(buildFreshWaterLinkVariable(), buildSignedTextVariable(iValue, true)));
                            }
                        }

                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            int iValue = infos().improvement(eImprovement).maiYieldFreshWaterModifier[eLoopYield];
                            if (iValue != 0)
                            {
                                subText.Add(buildColonSpaceOne(buildFreshWaterLinkVariable(), buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, bPercent: true)));
                            }
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miRiverModifier;
                            if (iValue != 0)
                            {
                                subText.Add(buildColonSpaceOne(buildRiverLinkVariable(), buildSignedTextVariable(iValue, true)));
                            }
                        }

                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            int iValue = infos().improvement(eImprovement).maiYieldRiverModifier[eLoopYield];
                            if (iValue != 0)
                            {
                                subText.Add(buildColonSpaceOne(buildRiverLinkVariable(), buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, bPercent: true)));
                            }
                        }

                        //buildImprovementPotentialBonuses

                        if (bDetails)
                        {
                            using (var cityListScoped = CollectionCache.GetListScoped<(City, Character, int)>())
                            {
                                if (pCityTerritory != null)
                                {
                                    cityListScoped.Value.Add((pCityTerritory, pCityTerritory.governor(), 1));
                                }

                                foreach (EffectCityType eEffectCity in effectListScoped.Value)
                                {
                                    buildEffectCityHelpYieldsPotential(subText, eEffectCity, pGame, cityListScoped.Value, pActivePlayer, bSkipImpossible: !bEncyclopedia, bShowTotal: false, bSkipZero: !bEncyclopedia, bSkipNonZero: false);
                                }
                            }

                            for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < mInfos.effectCitiesNum(); eLoopEffectCity++)
                            {
                                if ((infos().effectCity(eLoopEffectCity).maaiImprovementYield.Count > 0) || (infos().effectCity(eLoopEffectCity).maaiImprovementClassYield.Count > 0))
                                {
                                    if (!isSourceContentEnabled(eLoopEffectCity, pGame))
                                    {
                                        continue;
                                    }
                                    CommaListVariableGenerator yieldsList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, TextManager);

                                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                    {
                                        int iValue = infos().effectCity(eLoopEffectCity).maaiImprovementYield[eImprovement, eLoopYield];
                                        if (eImprovementClass != ImprovementClassType.NONE)
                                        {
                                            iValue += infos().effectCity(eLoopEffectCity).maaiImprovementClassYield[eImprovementClass, eLoopYield];
                                        }
                                        if (iValue != 0)
                                        {
                                            if (bEncyclopedia || (pActivePlayer == null || pActivePlayer.canEverHaveEffectCity(eLoopEffectCity)))
                                            {
                                                yieldsList.AddItem(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                            }
                                        }
                                    }

                                    if (yieldsList.Count > 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_YIELDS_EFFECT_CITY_YIELD_RATE", buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer), yieldsList.Finalize(), TEXTVAR(false), TEXTVAR(!(infos().effectCity(eLoopEffectCity).mbSingle)));
                                    }
                                }
                            }

                            for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < mInfos.effectCitiesNum(); eLoopEffectCity++)
                            {
                                if ((infos().effectCity(eLoopEffectCity).maiImprovementModifier.Count > 0) || (infos().effectCity(eLoopEffectCity).maiImprovementClassModifier.Count > 0))
                                {
                                    if ((infos().effectCity(eLoopEffectCity).meSourceLaw != LawType.NONE) ||
                                        (infos().effectCity(eLoopEffectCity).meSourceImprovement != ImprovementType.NONE) ||
                                        (infos().effectCity(eLoopEffectCity).meSourceImprovementClass != ImprovementClassType.NONE))
                                    {
                                        ImprovementType eSourceImprovement = infos().effectCity(eLoopEffectCity).meSourceImprovement;
                                        if (eSourceImprovement != ImprovementType.NONE && !infos().modSettings().App.CheckContentOwnership(infos().improvement(eSourceImprovement).meGameContentRequired, infos(), null))
                                        {
                                            continue;
                                        }
                                        int iValue = infos().effectCity(eLoopEffectCity).maiImprovementModifier[eImprovement];
                                        if (eImprovementClass != ImprovementClassType.NONE)
                                        {
                                            iValue += infos().effectCity(eLoopEffectCity).maiImprovementClassModifier[eImprovementClass];
                                        }
                                        if (iValue != 0)
                                        {
                                            if (bEncyclopedia || (pActivePlayer == null || pActivePlayer.canEverHaveEffectCity(eLoopEffectCity)))
                                            {
                                                subText.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_YIELDS_EFFECT_CITY_YIELD_MODIFIER", buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer), buildSignedTextVariable(iValue, true), TEXTVAR(!(infos().effectCity(eLoopEffectCity).mbSingle)));
                                            }
                                        }
                                    }
                                }
                            }

                            if (eImprovementClass != ImprovementClassType.NONE)
                            {
                                if (eReligionPrereq != ReligionType.NONE)
                                {
                                    for (TheologyType eLoopTheology = 0; eLoopTheology < infos().theologiesNum(); eLoopTheology++)
                                    {
                                        if (infos().improvementClass(eImprovementClass).maeTheologyCityEffect[eLoopTheology] != EffectCityType.NONE)
                                        {
                                            using (TextBuilder effectCityText = TextBuilder.GetTextBuilder(TextManager))
                                            {
                                                using (effectCityText.BeginScope(TextBuilder.ScopeType.COMMA))
                                                {
                                                    buildEffectCityHelp(effectCityText, infos().improvementClass(eImprovementClass).maeTheologyCityEffect[eLoopTheology], pGame, pPlayer, pActivePlayer, bSkipImpossible: !bEncyclopedia, bShowTotal: true, bSkipZero: !bEncyclopedia);
                                                }
                                                if (effectCityText.HasContent)
                                                {
                                                    subText.AddTEXT("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildTheologyLinkVariable(eLoopTheology, eReligionPrereq, pPlayer), effectCityText.ToTextVariable());
                                                }
                                            }
                                        }

                                        if (infos().improvementClass(eImprovementClass).maaiTheologyYieldOutput.Count > 0)
                                        {
                                            using (TextBuilder theologyYieldText = TextBuilder.GetTextBuilder(TextManager))
                                            {
                                                using (theologyYieldText.BeginScope(TextBuilder.ScopeType.COMMA))
                                                {
                                                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                                    {
                                                        int iValue = infos().improvementClass(eImprovementClass).maaiTheologyYieldOutput[eLoopTheology, eLoopYield];
                                                        if (iValue != 0)
                                                        {
                                                            theologyYieldText.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                                        }
                                                    }
                                                }
                                                if (theologyYieldText.HasContent)
                                                {
                                                    subText.AddTEXT("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildTheologyLinkVariable(eLoopTheology, eReligionPrereq, pPlayer), theologyYieldText.ToTextVariable());
                                                }
                                            }
                                        }

                                    }
                                }
                            }

                            using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementType>())
                            {
                                HashSet<ImprovementType> seImprovementsAdded = hashSetScope.Value;
                                for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                                {
                                    if (!(seImprovementsAdded.Contains(eLoopImprovement)))
                                    {
                                        if (((pPlayer != null) ? pPlayer.isNationImprovement(eLoopImprovement) : true) && infos().improvement(eLoopImprovement).mbBuild && isContentEnabled(infos().improvement(eLoopImprovement).meGameContentRequired, pGame))
                                        {
                                            int iValue = infos().improvement(eLoopImprovement).maiAdjacentImprovementModifier[eImprovement];
                                            if (iValue != 0)
                                            {
                                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_PER_ADJACENT_IMPROVEMENT_MODIFIER", buildSignedTextVariable(iValue, true))))
                                                {
                                                    subText.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
                                                    seImprovementsAdded.Add(eLoopImprovement);

                                                    for (ImprovementType eOtherImprovement = 0; eOtherImprovement < infos().improvementsNum(); eOtherImprovement++)
                                                    {
                                                        if (eOtherImprovement != eLoopImprovement)
                                                        {
                                                            if (((pPlayer != null) ? pPlayer.isNationImprovement(eOtherImprovement) : true) && infos().improvement(eOtherImprovement).mbBuild && isContentEnabled(infos().improvement(eOtherImprovement).meGameContentRequired, pGame))
                                                            {
                                                                if (infos().improvement(eOtherImprovement).maiAdjacentImprovementModifier[eImprovement] == iValue)
                                                                {
                                                                    subText.Add(buildImprovementLinkVariable(eOtherImprovement, pGame));
                                                                    seImprovementsAdded.Add(eOtherImprovement);
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

                            if (eImprovementClass != ImprovementClassType.NONE)
                            {
                                using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementType>())
                                {
                                    HashSet<ImprovementType> seImprovementsAdded = hashSetScope.Value;
                                    for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                                    {
                                        if (!(seImprovementsAdded.Contains(eLoopImprovement)))
                                        {
                                            if (((pPlayer != null) ? pPlayer.isNationImprovement(eLoopImprovement) : true) && infos().improvement(eLoopImprovement).mbBuild && isContentEnabled(infos().improvement(eLoopImprovement).meGameContentRequired, pGame))
                                            {
                                                int iValue = infos().improvement(eLoopImprovement).maiAdjacentImprovementClassModifier[eImprovementClass];
                                                if (iValue != 0)
                                                {
                                                    using (subText.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_PER_ADJACENT_IMPROVEMENT_MODIFIER", buildSignedTextVariable(iValue, true))))
                                                    {
                                                        subText.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
                                                        seImprovementsAdded.Add(eLoopImprovement);

                                                        for (ImprovementType eOtherImprovement = 0; eOtherImprovement < infos().improvementsNum(); eOtherImprovement++)
                                                        {
                                                            if (eOtherImprovement != eLoopImprovement)
                                                            {
                                                                if (((pPlayer != null) ? pPlayer.isNationImprovement(eOtherImprovement) : true) && infos().improvement(eOtherImprovement).mbBuild && isContentEnabled(infos().improvement(eOtherImprovement).meGameContentRequired, pGame))
                                                                {
                                                                    if (infos().improvement(eOtherImprovement).maiAdjacentImprovementClassModifier[eImprovementClass] == iValue)
                                                                    {
                                                                        subText.Add(buildImprovementLinkVariable(eOtherImprovement, pGame));
                                                                        seImprovementsAdded.Add(eOtherImprovement);
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

                                using (var hashSetScope = CollectionCache.GetHashSetScoped<ImprovementClassType>())
                                {
                                    HashSet<ImprovementClassType> seImprovementClassessAdded = hashSetScope.Value;
                                    for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                                    {
                                        if (!(seImprovementClassessAdded.Contains(eLoopImprovementClass)))
                                        {
                                            int iValue = infos().improvementClass(eLoopImprovementClass).maiAdjacentImprovementClassModifier[eImprovementClass];
                                            if (iValue != 0)
                                            {
                                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_PER_ADJACENT_IMPROVEMENT_CLASS_MODIFIER", buildSignedTextVariable(iValue, true))))
                                                {
                                                    subText.Add(buildImprovementClassLinkVariable(eLoopImprovementClass));
                                                    seImprovementClassessAdded.Add(eLoopImprovementClass);

                                                    for (ImprovementClassType eOtherImprovementClass = 0; eOtherImprovementClass < infos().improvementClassesNum(); eOtherImprovementClass++)
                                                    {
                                                        if (eOtherImprovementClass != eLoopImprovementClass)
                                                        {
                                                            if (infos().improvementClass(eOtherImprovementClass).maiAdjacentImprovementClassModifier[eImprovementClass] == iValue)
                                                            {
                                                                subText.Add(buildImprovementClassLinkVariable(eOtherImprovementClass));
                                                                seImprovementClassessAdded.Add(eOtherImprovementClass);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (infos().improvementClass(eImprovementClass).maeResourceCityEffect.Count > 0)
                                {
                                    using (TextBuilder effectCityBuilder = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                        {
                                            if (!isContentEnabled(infos().resource(eLoopResource).meGameContentRequired, pGame))
                                            {
                                                continue;
                                            }

                                            EffectCityType eLoopEffectCity = infos().improvementClass(eImprovementClass).maeResourceCityEffect[eLoopResource];
                                            if (eLoopEffectCity != EffectCityType.NONE)
                                            {
                                                for (EffectCityType eOtherEffectCity = 0; eOtherEffectCity < infos().effectCitiesNum(); eOtherEffectCity++)
                                                {
                                                    EffectCityType eEffectCity = infos().effectCity(eOtherEffectCity).maeEffectCityEffectCity[eLoopEffectCity];
                                                    if (eEffectCity != EffectCityType.NONE)
                                                    {
                                                        effectCityBuilder.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_NO_YIELDS_EFFECT_CITY_EFFECT_CITY", buildEffectCityLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory?.governor()), buildEffectCityLinkVariable(eEffectCity, pCityTerritory, pCityTerritory?.governor()));
                                                        subText.Add(buildColonSpaceOne(buildEffectCityLinkVariable(eOtherEffectCity, pCityTerritory, pCityTerritory?.governor()), effectCityBuilder.ToTextVariable()));
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (subText.HasContent)
                    {
                        if (bDetails)
                        {
                            buildDividerText(builder);
                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_POTENTIAL_BONUSES");
                        }

                        builder.Add(subText.ToTextVariable());
                    }
                }


                {
                    TextType eDescription = infos().improvement(eImprovement).meDescription;

                    if (eDescription != TextType.NONE)
                    {
                        builder.AddTEXT(eDescription);
                    }
                }

                using (TextBuilder extraText = TextBuilder.GetTextBuilder(TextManager))
                {
                    extraText.AddTEXT(infos().improvement(eImprovement).mExtraHelp);

                    if (extraText.HasContent)
                    {
                        buildDividerText(builder);
                        builder.Add(extraText.ToTextVariable());
                    }
                }

                return builder;
            }
        }
    }
}