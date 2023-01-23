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
using static TenCrowns.ClientCore.ClientUI;
using System.Xml;
using static BetterAI.BetterAIInfos;
using System.IO.Ports;

namespace BetterAI
{
    public class BetterAIHelpText : HelpText
    {
        public BetterAIHelpText(Infos pInfos, TextManager textManager) : base(pInfos, textManager)
        {
        }

        public override TextBuilder buildWidgetHelp(TextBuilder builder, WidgetData pWidget, ClientManager pManager, bool bIncludeEncyclopediaFooter = true)
        {
            if (pWidget.GetWidgetType() == ItemType.CREATE_AGENT_NETWORK)
            {
                Player pActivePlayer = pManager.activePlayer();
                Game pGame = pManager.GameClient;

                Unit pUnit = pGame.unit(pWidget.GetDataInt(0));
                if (pUnit != null)
                {
                    Player pPlayer = pUnit.player();
                    City pCity = pGame.city(pWidget.GetDataInt(1));
/*####### Better Old World AI - Base DLL #######
  ### Agent Network Cost Scaling       START ###
  ##############################################*/
                    builder.AddTEXT("TEXT_HELPTEXT_WIDGET_CREATE_AGENT_NETWORK", buildAgentNetworkLinkVariable(), buildMoneyWarningTextVariable(((BetterAIUnit)pUnit).getAgentNetworkCost(pCity), pActivePlayer), buildCityLinkVariable(pCity, pActivePlayer));
/*####### Better Old World AI - Base DLL #######
  ### Agent Network Cost Scaling         END ###
  ##############################################*/
                    buildAgentReturnText(builder, pUnit, pCity, pGame, pActivePlayer);

                    buildAgentUnlockText(builder, pUnit.player(), pGame, pActivePlayer);

                    buildCanActText(builder, 0, pGame, pActivePlayer, pUnit);
                }

                if (bIncludeEncyclopediaFooter && builder.HasContent && !pManager.Interfaces.UserInterface.IsPopupActive("POPUP_HELP") && hasEncyclopediaEntry(pWidget, pManager.GameClient))
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

        //500 lines of copy-paste START

        public override TextBuilder buildImprovementBreakdown(TextBuilder builder, ImprovementType eImprovement, SpecialistType eSpecialist, Tile pTile, ClientManager pManager)
        {
            Game pGame = pManager.GameClient;
            Player pActivePlayer = pManager.activePlayer();

            ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;

            ResourceType eResource = pTile.getResource();
            City pCityTerritory = pTile.cityTerritory();

            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
            {
                int iTotalOutput = pGame.tileYieldOutputModified(eImprovement, eSpecialist, eLoopYield, pTile, true);
                if (iTotalOutput != 0)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_PER_YEAR", buildYieldValueIconLinkVariable(infos().Helpers.finalYield(eLoopYield), iTotalOutput, true, false, Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame));

                    bool bShowModifiers = false;

                    {
                        int iValue = 0;
                        infos().Helpers.yieldOutputImprovement(eImprovement, eLoopYield, ResourceType.NONE, pGame, ref iValue);

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
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, false, Constants.YIELDS_MULTIPLIER), buildImprovementLinkVariable(eImprovement, pGame));
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

                    foreach (OccurrenceData pLoopData in pGame.getOccurrenceDataList())
                    {
                        if (pLoopData.isActive())
                        {
                            InfoOccurrence occurrence = infos().occurrence(pLoopData.meType);
                            int iValue = occurrence.miBaseYieldCoastModifier;
                            if (iValue != 0 && (pTile.isSaltCoastLand() || pTile.isSaltCoastWater()))
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true, bColor: true), buildOccurrenceLinkVariable(pLoopData.meType, pLoopData));
                                }
                            }
                            iValue = occurrence.miBaseYieldRiverModifier;
                            if (iValue != 0 && pTile.isRiver())
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
                        int iValue = infos().improvement(eImprovement).maiAdjacentWonderYieldOutput[(int)eLoopYield];
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
                        int iValue = infos().improvement(eImprovement).maiAdjacentResourceYieldOutput[(int)eLoopYield];
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
                        int iValue = infos().improvement(eImprovement).maiTradeNetworkYieldOutput[(int)eLoopYield];
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
                            int iValue = infos().improvementClass(eImprovementClass).maiAdjacentResourceYieldOutput[(int)eLoopYield];
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
                                                builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildTheologyLinkVariable(eLoopTheology, eReligionPrereq, true));
                                            }

                                            bShowModifiers = true;
                                        }
                                    }
                                }
                            }
                        }

                        if (pCityTerritory != null)
                        {
                            foreach (var p in pCityTerritory.getEffectCityCounts())
                            {
                                EffectCityType eLoopEffectCity = p.Key;
                                int iCount = p.Value;
                                int iValue = infos().effectCity(eLoopEffectCity).maaiImprovementYield[eImprovement, eLoopYield] + infos().effectCity(eLoopEffectCity).maaiImprovementClassYield[eImprovementClass, eLoopYield];
                                if (iValue != 0)
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_X",
                                            buildSignedTextVariable((iCount * iValue), false, Constants.YIELDS_MULTIPLIER),
                                            buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pGame, pActivePlayer),
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
                            foreach (var p in pCityTerritory.getEffectCityCounts())
                            {
                                EffectCityType eLoopEffectCity = p.Key;
                                int iCount = p.Value;
                                int iValue = pGame.getTileEffectCityModifier(eLoopEffectCity, pTile, eImprovement);
                                if (iValue != 0)
                                {
                                    using (buildSecondaryTextScope(builder))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM_X",
                                            buildSignedTextVariable((iCount * iValue), true),
                                            buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pGame, pActivePlayer),
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
                            int iValue = infos().improvement(eImprovement).miFreshWaterModifier + infos().improvement(eImprovement).maiYieldFreshWaterModifier[(int)eLoopYield];
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
                            int iValue = infos().improvement(eImprovement).miRiverModifier + infos().improvement(eImprovement).maiYieldRiverModifier[(int)eLoopYield];
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
                                        if (pAdjacentTile.hasImprovementFinished())
                                        {
                                            int iValue = pAdjacentTile.improvement().maiAdjacentImprovementModifier[eImprovement];
                                            if (pAdjacentTile.hasImprovementClass() && (eImprovementClass != ImprovementClassType.NONE))
                                            {
                                                iValue += pAdjacentTile.improvement().maiAdjacentImprovementClassModifier[eImprovementClass];
                                                iValue += pAdjacentTile.improvementClass().maiAdjacentImprovementClassModifier[eImprovementClass];
                                            }
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
                                int iValue = infos().specialist(eSpecialist).maiImprovementClassModifier[(int)eImprovementClass];
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
                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvement(eImprovement).meEffectCity, false, eLoopYield, pCityTerritory, pManager, iTotalOutput);

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvementClass(eImprovementClass).meEffectCity, false, eLoopYield, pCityTerritory, pManager, iTotalOutput);

                            if (eResource != ResourceType.NONE)
                            {
                                buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvementClass(eImprovementClass).maeResourceCityEffect[(int)eResource], true, eLoopYield, pCityTerritory, pManager);
                            }

                            {
                                ReligionType ePrereqReligion = infos().improvement(eImprovement).meReligionPrereq;

                                if (ePrereqReligion != ReligionType.NONE)
                                {
                                    for (TheologyType eLoopTheology = 0; eLoopTheology < infos().theologiesNum(); eLoopTheology++)
                                    {
                                        if (pGame.isReligionTheology(ePrereqReligion, eLoopTheology))
                                        {
                                            EffectCityType eEffectCity = mInfos.improvementClass(eImprovementClass).maeTheologyCityEffect[(int)eLoopTheology];

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

                            if (eResource != ResourceType.NONE)
                            {
                                buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().specialistClass(infos().specialist(eSpecialist).meClass).maeResourceCityEffect[(int)eResource], true, eLoopYield, pCityTerritory, pManager);
                            }
                        }
                    }
                }
            }
            return builder;
        }
        //copy-paste END

        //1k lines of copy-paste START
        public override TextBuilder buildImprovementHelp(TextBuilder builder, ImprovementType eImprovement, Tile pTile, ClientManager pManager, bool bName = true, bool bCosts = true, bool bDetails = true, bool bEncyclopedia = false, TextBuilder.ScopeType scopeType = TextBuilder.ScopeType.NONE)
        {
            using (new UnityProfileScope("HelpText.buildImprovementHelp"))
            using (var effectListScoped = CollectionCache.GetListScoped<EffectCityType>())
            {
                Game pGame = pManager.GameClient;

                Player pPlayer = ((pTile != null) ? pTile.owner() : null);

                Player pActivePlayer = pManager.activePlayer();
                if (pPlayer == null)
                {
                    pPlayer = pActivePlayer;
                }

                City pCityTerritory = ((pTile != null) ? pTile.cityTerritory() : null);

                ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;

                ReligionType eReligionPrereq = infos().improvement(eImprovement).meReligionPrereq;

                getImprovementEffectCityList(eImprovement, pManager, effectListScoped.Value);

                using (builder.BeginScope(scopeType))
                {
                    if (bName)
                    {
                        builder.Add(buildTitleVariable(TEXTVAR_TYPE(mInfos.improvement(eImprovement).mName)));
                    }

                    if ((pTile != null) && pTile.isInfoVisible(pActivePlayer.getTeam(), pManager))
                    {
                        buildImprovementBreakdown(builder, eImprovement, SpecialistType.NONE, pTile, pManager);
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

                        if (bDetails)
                        {
                            if (infos().improvement(eImprovement).mbUrban)
                            {
                                TextVariable urbanVariable = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_URBAN_BUILDING", buildUrbanLinkVariable());

                                if (!(infos().improvement(eImprovement).mbRequiresUrban))
                                {
                                    urbanVariable = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_URBAN_BUILDING_ANYWHERE", urbanVariable);
                                }
                                builder.Add(urbanVariable);
                            }
                            else if (infos().improvement(eImprovement).mbSpreadsBorders)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_SPREADS_BORDERS");
                            }

                            if (infos().improvement(eImprovement).mbPermanent)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_PERMANENT_BUILDING");
                            }
                        }

                        if (infos().improvement(eImprovement).mbFreshWaterSource)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_FRESH_WATER_SOURCE", buildFreshWaterLinkVariable(pTile));
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miVP;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_VP", buildSignedTextVariable(iValue));
                            }
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
                                    infos().Helpers.yieldOutputImprovement(eImprovement, eLoopYield, ResourceType.NONE, pGame, ref iOutput);
                                    if ((infos().yield(eLoopYield).mbWarning || bDetails) ? (iOutput != 0) : (iOutput > 0))
                                    {
                                        builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, true, false, Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }

                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_CONSUMPTION_YIELDS", buildTurnScaleName(pGame)), scopeKey: "YIELD-LIST"))
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iOutput = infos().improvement(eImprovement).maiYieldConsumption[(int)eLoopYield];
                                    if (iOutput != 0)
                                    {
                                        builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, true, false, Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }

                        foreach (EffectCityType eEffectCity in effectListScoped.Value)
                        {
                            buildEffectCityHelpYields(builder, eEffectCity, pGame, null, pActivePlayer, 1);
                        }

                        foreach (EffectCityType eEffectCity in effectListScoped.Value)
                        {
                            buildEffectCityHelpNoYields(builder, eEffectCity, pGame, pCityTerritory, pPlayer, pActivePlayer);
                        }

                        {
                            int iValue = infos().improvement(eImprovement).miLegitimacy;
                            if (iValue != 0)
                            {
                                if (pGame?.isCharacters() ?? true)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_LEGITIMACY", buildSignedTextVariable(iValue));
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
                            using (var hashSetScope = CollectionCache.GetHashSetScoped<UnitTraitType>())
                            {
                                HashSet<UnitTraitType> seUnitTraitsAdded = hashSetScope.Value;

                                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                                {
                                    if (!(seUnitTraitsAdded.Contains(eLoopUnitTrait)))
                                    {
                                        int iValue = infos().improvement(eImprovement).maiUnitTraitHeal[(int)eLoopUnitTrait];
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
                                                        if (infos().improvement(eImprovement).maiUnitTraitHeal[(int)eOtherUnitTrait] == iValue)
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
                                        int iValue = infos().improvement(eImprovement).maiUnitTraitXP[(int)eLoopUnitTrait];
                                        if (iValue != 0)
                                        {
                                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_UNIT_TRAIT_XP", buildSignedTextVariable(iValue), buildTurnScaleName(pGame), buildIdleLinkVariable())))
                                            {
                                                {
                                                    builder.Add(buildUnitTraitLinkVariable(eLoopUnitTrait));
                                                    seUnitTraitsAdded.Add(eLoopUnitTrait);
                                                }

                                                for (UnitTraitType eOtherUnitTrait = 0; eOtherUnitTrait < infos().unitTraitsNum(); eOtherUnitTrait++)
                                                {
                                                    if (eOtherUnitTrait != eLoopUnitTrait)
                                                    {
                                                        if (infos().improvement(eImprovement).maiUnitTraitXP[(int)eOtherUnitTrait] == iValue)
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
                                    buildBonusHelp(builder, eBonusCities, pGame, pPlayer, pActivePlayer, bName: false, bShowCity: false, startLineVariable: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_BONUS_ALL_CITIES"));
                                }
                            }

                            if (bDetails)
                            {
                                {
                                    ReligionType eReligionSpread = infos().improvement(eImprovement).meReligionSpread;

                                    if (eReligionSpread != ReligionType.NONE)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_SPREADS_RELIGION", buildReligionLinkVariable(eReligionSpread, pGame, pActivePlayer));

                                        {
                                            EffectCityType eEffectCity = infos().religion(eReligionSpread).meEffectCity;

                                            if (eEffectCity != EffectCityType.NONE)
                                            {
                                                using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                                {
                                                    using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                                    {
                                                        buildEffectCityHelp(subText, eEffectCity, pGame, null, pCityTerritory, pActivePlayer, bSkipImpossible: !bEncyclopedia);
                                                    }

                                                    if (subText.HasContent)
                                                    {
                                                        builder.AddWithParenthesis(subText.ToTextVariable());
                                                    }
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
                                            ReligionType eReligionSpread = infos().improvement(eImprovement).meReligionSpread;

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

                            for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                            {
                                if (infos().unit(eLoopUnit).meImprovementPrereq == eImprovement)
                                {
                                    if ((pPlayer != null) ? pPlayer.canEverBuildUnit(eLoopUnit) : true)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_UNLOCKS_UNIT", buildUnitTypeLinkVariable(eLoopUnit, pGame, pCity: pCityTerritory));
                                    }
                                }
                            }

                            if (infos().improvement(eImprovement).mbWonder)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_WONDER");
                            }

                            if (infos().improvement(eImprovement).mbHeal)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HEAL");
                            }

                            if (infos().improvement(eImprovement).mbCitySite)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_CITY_SITE");
                            }

                            if (infos().improvement(eImprovement).mbRemovePillage)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_REMOVE_PILLAGE");
                            }

                            if (!(infos().improvement(eImprovement).mbTribe))
                            {
                                int iValue = infos().improvement(eImprovement).miUnitTurns;
                                if (iValue > 0)
                                {
                                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA_OR, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_CREATES_UNITS", iValue, buildTurnScaleName(pGame, iValue))))
                                    {
                                        for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                                        {
                                            if (infos().improvement(eImprovement).maiUnitDie[(int)eLoopUnit] > 0)
                                            {
                                                builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, pCity: pCityTerritory));
                                            }
                                        }
                                    }
                                }
                            }

                            {
                                SpecialistType eSpecialist = infos().improvement(eImprovement).meSpecialist;

                                if (eSpecialist != SpecialistType.NONE)
                                {
                                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_ENABLES", buildSpecialistLinkVariable(eSpecialist, pGame).SetKey("LINK"));

                                        if (pTile != null)
                                        {
                                            if (pTile.hasResource())
                                            {
                                                EffectCityType eEffectCity = infos().specialistClass(infos().specialist(eSpecialist).meClass).maeResourceCityEffect[(int)(pTile.getResource())];

                                                if (eEffectCity != EffectCityType.NONE)
                                                {
                                                    buildEffectCityHelp(builder, eEffectCity, pGame, null, pCityTerritory, pActivePlayer, bSkipImpossible: !bEncyclopedia);
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
                        }

                        if ((pTile == null) || (pTile.getImprovement() != eImprovement))
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                            {
                                ImprovementType eDevelopImprovement = mInfos.Helpers.getDevelopImprovement(eImprovement, pGame?.getTribeLevel() ?? TribeLevelType.NONE);

                                if (eDevelopImprovement != ImprovementType.NONE)
                                {
                                    TextVariable developImprovement = buildImprovementLinkVariable(eDevelopImprovement, pGame);

                                    int iDevelopTurns = infos().improvement(eImprovement).miDevelopTurns;

                                    if (infos().improvement(eImprovement).miDevelopRand != 0)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_DEVELOP_TURNS_RANDOM", developImprovement, TEXTVAR(iDevelopTurns - infos().improvement(eImprovement).miDevelopRand), TEXTVAR(iDevelopTurns));
                                    }
                                    else
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_DEVELOP_TURNS", developImprovement, TEXTVAR(iDevelopTurns), buildTurnScaleName(pGame, iDevelopTurns));
                                    }
                                }
                            }

                            if (bCosts)
                            {
                                buildImprovementCostsHelp(builder, eImprovement, pTile, pManager, true);
                            }

                            if (bDetails)
                            {
                                if (eReligionPrereq != ReligionType.NONE)
                                {
                                    using (TextBuilder subCommaText = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        using (subCommaText.BeginScope(TextBuilder.ScopeType.COMMA))
                                        {
                                            for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                                            {
                                                if (infos().unit(eLoopUnit).meBuildReligion == eReligionPrereq)
                                                {
                                                    subCommaText.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame));
                                                }
                                            }
                                        }

                                        if (subCommaText.HasContent)
                                        {
                                            builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_BUILT_BY", subCommaText.ToTextVariable()));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                {
                    using (subText.BeginScope(scopeType))
                    using (subText.BeginScope(TextBuilder.ScopeType.BULLET))
                    {
                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_WONDER", buildTurnScaleName(pGame))))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iOutput = infos().improvement(eImprovement).maiAdjacentWonderYieldOutput[(int)eLoopYield];
                                if (iOutput != 0)
                                {
                                    subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, true, false, Constants.YIELDS_MULTIPLIER));
                                }
                            }
                        }

                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_RESOURCE", buildTurnScaleName(pGame))))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iOutput = infos().improvement(eImprovement).maiAdjacentResourceYieldOutput[(int)eLoopYield];
                                if (iOutput != 0)
                                {
                                    subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, true, false, Constants.YIELDS_MULTIPLIER));
                                }
                            }
                        }

                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_TRADE_NETWORK")))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iOutput = infos().improvement(eImprovement).maiTradeNetworkYieldOutput[(int)eLoopYield];
                                if (iOutput != 0)
                                {
                                    subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, true, false, Constants.YIELDS_MULTIPLIER));
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
                                        subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, true, false, Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            if (pTile == null)
                            {
                                for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                                {
                                    using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: buildColonSpaceOne(buildResourceLinkVariable(eLoopResource), null))) // arg1 for buildColonSpaceOne will be filled with list
                                    {
                                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                        {
                                            int iValue = 0;
                                            infos().Helpers.yieldOutputResource(eImprovementClass, eLoopYield, eLoopResource, ref iValue);
                                            if (iValue != 0)
                                            {
                                                subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, true, false, Constants.YIELDS_MULTIPLIER));
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

                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_YIELDS_ADJACENT_RESOURCE", buildTurnScaleName(pGame))))
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iOutput = infos().improvementClass(eImprovementClass).maiAdjacentResourceYieldOutput[(int)eLoopYield];
                                    if (iOutput != 0)
                                    {
                                        subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, true, false, Constants.YIELDS_MULTIPLIER));
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
                                            subText.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, true, false, Constants.YIELDS_MULTIPLIER));
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
                                        subText.Add(buildColonSpaceOne(buildTerrainLinkVariable(eLoopTerrain), buildYieldValueIconLinkVariable(eLoopYield, iValue, true, true)));
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
                                        subText.Add(buildColonSpaceOne(buildHeightLinkVariable(eLoopHeight), buildYieldValueIconLinkVariable(eLoopYield, iValue, true, true)));
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
                                    subText.AddTEXT("TEXT_HELPTEXT_ADJACENT_COLON", buildHeightLinkVariable(eLoopHeight), buildYieldValueIconLinkVariable(eLoopYield, iValue, true, true));
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
                            int iValue = infos().improvement(eImprovement).maiYieldFreshWaterModifier[(int)eLoopYield];
                            if (iValue != 0)
                            {
                                subText.Add(buildColonSpaceOne(buildFreshWaterLinkVariable(), buildYieldValueIconLinkVariable(eLoopYield, iValue, true, true)));
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
                            int iValue = infos().improvement(eImprovement).maiYieldRiverModifier[(int)eLoopYield];
                            if (iValue != 0)
                            {
                                subText.Add(buildColonSpaceOne(buildRiverLinkVariable(), buildYieldValueIconLinkVariable(eLoopYield, iValue, true, true)));
                            }
                        }

                        if (bDetails)
                        {
                            foreach (EffectCityType eEffectCity in effectListScoped.Value)
                            {
                                buildEffectCityHelpYieldsPotential(subText, eEffectCity, pGame, null, pActivePlayer, 1);
                            }

                            for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < mInfos.effectCitiesNum(); eLoopEffectCity++)
                            {
                                if ((infos().effectCity(eLoopEffectCity).maaiImprovementYield.Count > 0) || (infos().effectCity(eLoopEffectCity).maaiImprovementClassYield.Count > 0))
                                {
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
                                                yieldsList.AddItem(buildYieldValueIconLinkVariable(eLoopYield, iValue, true, false, Constants.YIELDS_MULTIPLIER));
                                            }
                                        }
                                    }

                                    if (yieldsList.Count > 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_YIELDS_EFFECT_CITY_YIELD_RATE", buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pGame, pActivePlayer), yieldsList.Finalize(), TEXTVAR(false), TEXTVAR(!(infos().effectCity(eLoopEffectCity).mbSingle)));
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
                                        int iValue = infos().effectCity(eLoopEffectCity).maiImprovementModifier[(int)eImprovement];
                                        if (eImprovementClass != ImprovementClassType.NONE)
                                        {
                                            iValue += infos().effectCity(eLoopEffectCity).maiImprovementClassModifier[(int)eImprovementClass];
                                        }
                                        if (iValue != 0)
                                        {
                                            if (bEncyclopedia || (pActivePlayer == null || pActivePlayer.canEverHaveEffectCity(eLoopEffectCity)))
                                            {
                                                subText.AddTEXT("TEXT_HELPTEXT_EFFECT_CITY_HELP_YIELDS_EFFECT_CITY_YIELD_MODIFIER", buildEffectCitySourceLinkVariable(eLoopEffectCity, pCityTerritory, pGame, pActivePlayer), buildSignedTextVariable(iValue, true), TEXTVAR(!(infos().effectCity(eLoopEffectCity).mbSingle)));
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
                                        if (infos().improvementClass(eImprovementClass).maaiTheologyYieldOutput.Count > 0)
                                        {
                                            CommaListVariableGenerator yieldsList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, TextManager);

                                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                            {
                                                int iValue = infos().improvementClass(eImprovementClass).maaiTheologyYieldOutput[eLoopTheology, eLoopYield];
                                                if (iValue != 0)
                                                {
                                                    yieldsList.AddItem(buildYieldValueIconLinkVariable(eLoopYield, iValue, true, false, Constants.YIELDS_MULTIPLIER));
                                                }
                                            }

                                            if (yieldsList.Count > 0)
                                            {
                                                subText.AddTEXT("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildTheologyLinkVariable(eLoopTheology, eReligionPrereq), yieldsList.Finalize());
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
                                        if (((pPlayer != null) ? pPlayer.isNationImprovement(eLoopImprovement) : true) && infos().improvement(eLoopImprovement).mbBuild)
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
                                                            if (((pPlayer != null) ? pPlayer.isNationImprovement(eOtherImprovement) : true) && infos().improvement(eOtherImprovement).mbBuild)
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
                                            if (((pPlayer != null) ? pPlayer.isNationImprovement(eLoopImprovement) : true) && infos().improvement(eLoopImprovement).mbBuild)
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
                                                                if (((pPlayer != null) ? pPlayer.isNationImprovement(eOtherImprovement) : true) && infos().improvement(eOtherImprovement).mbBuild)
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


        //1k lines of copy-paste END

        public override void buildImprovementRequiresHelp(List<TextVariable> lRequirements, ImprovementType eImprovement, Game pGame, Player pActivePlayer, Tile pTile, bool bUpgradeImprovement = false)
        {
            BetterAIInfoImprovement eInfoImprovement = (BetterAIInfoImprovement)infos().improvement(eImprovement);
            {
                //here goes the copy-pasting
                using (new UnityProfileScope("HelpText.buildImprovementRequiresHelp"))
                {
                    ImprovementClassType eImprovementClass = eInfoImprovement.meClass;

                    ReligionType eReligionPrereq = eInfoImprovement.meReligionPrereq;

                    BetterAICity pCityTerritory = ((pTile != null) ? (BetterAICity)pTile.cityTerritory() : null);

                    if (pTile != null)
                    {
                        if (pTile.isHarvested())
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_WARN_HARVESTED")));
                        }
                    }



                    //if (eImprovementClass != ImprovementClassType.NONE)
                    //{
                    //    TechType ePrereqTech = infos().improvementClass(eImprovementClass).meTechPrereq;

                    //    if (ePrereqTech != TechType.NONE)
                    //    {
                    //        if (pActivePlayer == null || !(pActivePlayer.isTechAcquired(ePrereqTech)))
                    //        {
                    //            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildTechLinkVariable(ePrereqTech)), pActivePlayer != null));
                    //        }
                    //    }
                    //}
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                     START ###
  ##############################################*/
                    {
                        bool bHasPrimaryUnlock = false;
                        CommaListVariableGenerator andList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.AND, TextManager);
                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            TechType ePrereqTech = infos().improvementClass(eImprovementClass).meTechPrereq;

                            if (ePrereqTech != TechType.NONE)
                            {
                                //andList.AddItem(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildTechLinkVariable(ePrereqTech)), pActivePlayer != null));
                                andList.AddItem(buildTechLinkVariable(ePrereqTech));
                                bHasPrimaryUnlock = true;
                            }
                        }

                        CultureType eCulturePrereq = eInfoImprovement.meCulturePrereq;

                        if (eCulturePrereq != CultureType.NONE)
                        {
                            //andList.AddItem(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(eCulturePrereq, pCityTerritory)), ((pCityTerritory != null) ? (pCityTerritory.getCulture() < eCulturePrereq) : false)));
                            andList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(eCulturePrereq, pCityTerritory)));
                            bHasPrimaryUnlock = true;
                        }
                        if (bHasPrimaryUnlock)
                        {
                            if (eInfoImprovement.isAnySecondaryPrereq() || eInfoImprovement.isAnyTertiaryPrereq())
                            {
                                CommaListVariableGenerator orLineBreakList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.OR_LINEBREAK, CommaListVariableGenerator.EncloseType.PARENTHESIS, TextManager);
                                orLineBreakList.AddItem(andList.Finalize());
                                if (eInfoImprovement.isAnySecondaryPrereq())
                                {
                                    CommaListVariableGenerator andList2 = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.AND, TextManager);
                                    
                                    if (eInfoImprovement.meSecondaryUnlockTechPrereq != TechType.NONE)
                                    {
                                        //andList2.AddItem(buildWarningTextVariable(buildTechLinkVariable(eInfoImprovement.meSecondaryUnlockTechPrereq), pActivePlayer != null));
                                        andList2.AddItem(buildTechLinkVariable(eInfoImprovement.meSecondaryUnlockTechPrereq));
                                    }
                                    if (eInfoImprovement.meSecondaryUnlockCulturePrereq != CultureType.NONE)
                                    {
                                        //andList2.AddItem(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(eInfoImprovement.meSecondaryUnlockCulturePrereq, pCityTerritory)), ((pCityTerritory != null) ? (pCityTerritory.getCulture() < eInfoImprovement.meSecondaryUnlockCulturePrereq) : false)));
                                        andList2.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(eInfoImprovement.meSecondaryUnlockCulturePrereq, pCityTerritory)));
                                    }
                                    if (eInfoImprovement.miSecondaryUnlockPopulationPrereq > 0)
                                    {
                                        TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildConceptLinkVariable("CONCEPT_POPULATION"), TEXTVAR_TYPE("TEXT_HELPTEXT_MIN", eInfoImprovement.miSecondaryUnlockPopulationPrereq));
                                        if (pCityTerritory != null)
                                        {
                                            //reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pCityTerritory.getPopulation()) , TEXTVAR(eInfoImprovement.miSecondaryUnlockPopulationPrereq));
                                            //andList2.AddItem(buildWarningTextVariable(reqItem, (pCityTerritory.getPopulation() < eInfoImprovement.miSecondaryUnlockPopulationPrereq)));
                                            andList2.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pCityTerritory.getPopulation()), TEXTVAR(eInfoImprovement.miSecondaryUnlockPopulationPrereq)));
                                        }
                                        else
                                        {
                                            //andList2.AddItem(buildWarningTextVariable(reqItem, false));
                                            andList2.AddItem(reqItem);
                                        }
                                    }
                                    if (eInfoImprovement.meSecondaryUnlockEffectCityPrereq != EffectCityType.NONE)
                                    {
                                        //andList2.AddItem(buildWarningTextVariable(buildEffectCitySourceLinkVariable(eInfoImprovement.meSecondaryUnlockEffectCityPrereq, pCityTerritory, pGame, pActivePlayer), ((pCityTerritory != null) ? (pCityTerritory.getEffectCityCount(eInfoImprovement.meSecondaryUnlockEffectCityPrereq) == 0) : false)));
                                        andList2.AddItem(buildEffectCitySourceLinkVariable(eInfoImprovement.meSecondaryUnlockEffectCityPrereq, pCityTerritory, pGame, pActivePlayer));
                                    }
                                    orLineBreakList.AddItem(andList2.Finalize());
                                }

                                if (eInfoImprovement.isAnyTertiaryPrereq())
                                {
                                    CommaListVariableGenerator andList3 = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.AND, TextManager);

                                    if (eInfoImprovement.meTertiaryUnlockFamilyClassPrereq != FamilyClassType.NONE)
                                    {
                                        TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildConceptLinkVariable("CONCEPT_FAMILY_CLASS"), TEXTVAR(infos().familyClass(eInfoImprovement.meTertiaryUnlockFamilyClassPrereq).meName, TextManager));
                                        //reqItem = buildWarningTextVariable(reqItem, ((pCityTerritory != null) ? (pCityTerritory.getFamilyClass() != eInfoImprovement.meTertiaryUnlockFamilyClassPrereq) : false));
                                        if (eInfoImprovement.mbTertiaryUnlockSeatOnly)
                                        {
                                            //reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", reqItem, buildWarningTextVariable(buildConceptLinkVariable("CONCEPT_FAMILY_SEAT"), ((pCityTerritory != null) ? pCityTerritory.isFamilySeat() : false)));
                                            reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", reqItem, buildConceptLinkVariable("CONCEPT_FAMILY_SEAT"));
                                        }
                                        andList3.AddItem(reqItem);
                                    }

                                    if (eInfoImprovement.meTertiaryUnlockTechPrereq != TechType.NONE)
                                    {
                                        //andList3.AddItem(buildWarningTextVariable(buildTechLinkVariable(eInfoImprovement.meTertiaryUnlockTechPrereq), pActivePlayer != null));
                                        andList3.AddItem(buildTechLinkVariable(eInfoImprovement.meTertiaryUnlockTechPrereq));
                                    }

                                    if (eInfoImprovement.meTertiaryUnlockCulturePrereq != CultureType.NONE)
                                    {
                                        //andList3.AddItem(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(eInfoImprovement.meTertiaryUnlockCulturePrereq, pCityTerritory)), ((pCityTerritory != null) ? (pCityTerritory.getCulture() < eInfoImprovement.meTertiaryUnlockCulturePrereq) : false)));
                                        andList3.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_ALSO_REQUIRES_CULTURE", buildCultureLinkVariable(eInfoImprovement.meTertiaryUnlockCulturePrereq, pCityTerritory)));
                                    }

                                    if (eInfoImprovement.meTertiaryUnlockEffectCityPrereq != EffectCityType.NONE)
                                    {
                                        //andList3.AddItem(buildWarningTextVariable(buildEffectCitySourceLinkVariable(eInfoImprovement.meTertiaryUnlockEffectCityPrereq, pCityTerritory, pGame, pActivePlayer), ((pCityTerritory != null) ? (pCityTerritory.getEffectCityCount(eInfoImprovement.meTertiaryUnlockEffectCityPrereq) == 0) : false)));
                                        andList3.AddItem(buildEffectCitySourceLinkVariable(eInfoImprovement.meTertiaryUnlockEffectCityPrereq, pCityTerritory, pGame, pActivePlayer));
                                    }

                                    orLineBreakList.AddItem(andList3.Finalize());
                                }
                                //lRequirements.Add(orLineBreakList.Finalize());
                                bool bPrimaryUnlock = true;
                                lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", orLineBreakList.Finalize()), ((pCityTerritory != null) ? !(pCityTerritory.ImprovementUnlocked(eImprovement, ref bPrimaryUnlock)) : false)));

                            }
                            else
                            {
                                //lRequirements.Add(andList.Finalize());
                                bool bPrimaryUnlock = true;
                                lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", andList.Finalize()), ((pCityTerritory != null) ? !(pCityTerritory.ImprovementUnlocked(eImprovement, ref bPrimaryUnlock)) : false)));
                            }
                            //DEBUG HERE
                            //CommaListVariableGenerator debugList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, TextManager);
                            ////string sDebugMsg = "DEBUG: HasPrimaryUnlock";

                            //debugList.AddItem(TEXTVAR("DEBUG: HasPrimaryUnlock"));
                            //if (pCityTerritory != null)
                            //{
                            //    //debugList.AddItem(TEXTVAR(""));
                            //    debugList.AddItem(TEXTVAR("City != NULL"));
                            //    bool bPrimaryUnlock = true;
                            //    if (pCityTerritory.ImprovementUnlocked(eImprovement, ref bPrimaryUnlock))
                            //    {
                            //        debugList.AddItem(TEXTVAR("Improvement unlocked: " + ((bPrimaryUnlock) ? "Primary" : "not Primary") ));

                            //    }
                            //    else
                            //    {
                            //        debugList.AddItem(TEXTVAR("Improvement NOT unlocked"));
                            //    }

                            //}
                            //else
                            //{
                            //    debugList.AddItem(TEXTVAR("City == NULL"));
                            //}
                            //lRequirements.Add(debugList.Finalize());
                            //DEBUG END

                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### Early Unlock                       END ###
  ##############################################*/

                    //mbHolyCity is for specific religions, so it was moved down to religion check in version 1.0.62422
                    //for any Holy City: mbHolyCityValid
                    //if (eInfoImprovement.mbHolyCity)
                    //{
                    //    if (eReligionPrereq != ReligionType.NONE)
                    //    {
                    //        if (pTile != null)
                    //        {
                    //            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildHolyCityLinkVariable(pCityTerritory)), ((pCityTerritory != null) && !(pCityTerritory.isReligionHolyCity(eReligionPrereq)))));
                    //        }
                    //        else
                    //        {
                    //            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_HOLY_CITY", buildReligionLinkVariable(eReligionPrereq, pGame, pActivePlayer)), ((pCityTerritory != null) && !(pCityTerritory.isReligionHolyCityAny()))));
                    //        }
                    //    }
                    //}

                    {
                        CommaListVariableGenerator orList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.OR, TextManager);

                        if (eInfoImprovement.mbFreshWaterValid)
                        {
                            orList.AddItem(buildFreshWaterLinkVariable(pTile));
                        }

                        if (eInfoImprovement.mbRiverValid)
                        {
                            orList.AddItem(buildRiverLinkVariable(pTile));
                        }

                        if (eInfoImprovement.mbCoastLandValid)
                        {
                            orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_COAST_LAND"));
                        }

                        if (eInfoImprovement.mbCoastWaterValid)
                        {
                            orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_COAST_WATER"));
                        }

                        if (eInfoImprovement.mbCityValid)
                        {
                            orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_CITY"));
                        }

                        if (eInfoImprovement.mbHolyCityValid)
                        {
                            orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_HOLY_CITY_ANY"));
                        }

                        for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                        {
                            if (eInfoImprovement.mabTerrainValid[(int)eLoopTerrain])
                            {
                                orList.AddItem(buildTerrainLinkVariable(eLoopTerrain));
                            }
                        }

                        for (HeightType eLoopHeight = 0; eLoopHeight < infos().heightsNum(); eLoopHeight++)
                        {
                            if (eInfoImprovement.mabHeightValid[(int)eLoopHeight])
                            {
                                orList.AddItem(buildHeightLinkVariable(eLoopHeight));
                            }
                        }

                        for (HeightType eLoopHeight = 0; eLoopHeight < infos().heightsNum(); eLoopHeight++)
                        {
                            if (eInfoImprovement.mabHeightAdjacentValid[(int)eLoopHeight])
                            {
                                orList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_HEIGHT", buildHeightLinkVariable(eLoopHeight)));
                            }
                        }

                        for (VegetationType eLoopVegetation = 0; eLoopVegetation < infos().vegetationNum(); eLoopVegetation++)
                        {
                            if (eInfoImprovement.mabVegetationValid[(int)eLoopVegetation])
                            {
                                orList.AddItem(buildVegetationLinkVariable(eLoopVegetation));
                            }
                        }

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            for (ResourceType eLoopResource = 0; eLoopResource < infos().resourcesNum(); eLoopResource++)
                            {
                                if (infos().improvementClass(eImprovementClass).mabResourceValid[(int)eLoopResource])
                                {
                                    orList.AddItem(buildResourceLinkVariable(eLoopResource, pTile));
                                }
                            }
                        }

                        if (orList.Count > 0)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", orList.Finalize()), ((pTile != null) && !(pTile.isImprovementValid(eImprovement)))));
                        }
                    }

                    {
                        int iRequiresLaws = eInfoImprovement.miPrereqLaws;

                        if (iRequiresLaws > 0)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_LAWS", iRequiresLaws), ((pActivePlayer != null) ? (pActivePlayer.countActiveLaws() < iRequiresLaws) : false)));
                        }
                    }

                    if (eReligionPrereq != ReligionType.NONE)
                    {
                        if (infos().improvement(eImprovement).mbHolyCity)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_HOLY_CITY", buildReligionLinkVariable(eReligionPrereq, pGame, pActivePlayer)), ((pCityTerritory != null) && !(pCityTerritory.isReligionHolyCityAny()))));
                        }
                        else
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_RELIGION", buildReligionLinkVariable(eReligionPrereq, pGame, pActivePlayer)), ((pCityTerritory != null) ? !(pCityTerritory.isReligion(eReligionPrereq)) : false)));
                        }
                    }

                    //culture is now part of new code, with tech
                    //{
                    //    CultureType eCulturePrereq = eInfoImprovement.meCulturePrereq;

                    //    if (eCulturePrereq != CultureType.NONE)
                    //    {
                    //        lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CULTURE", buildCultureLinkVariable(eCulturePrereq, pCityTerritory)), ((pCityTerritory != null) ? (pCityTerritory.getCulture() < eCulturePrereq) : false)));
                    //    }
                    //}


                    {
                        ImprovementType eImprovementPrereq = eInfoImprovement.meImprovementPrereq;

                        if (eImprovementPrereq != ImprovementType.NONE)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_IMPROVEMENT", buildImprovementLinkVariable(eImprovementPrereq, pGame, pTile)), ((pCityTerritory != null) ? (pCityTerritory.getFinishedImprovementCount(eImprovementPrereq) == 0) : false)));
                        }
                    }

                    if (!bUpgradeImprovement)
                    {
                        ImprovementType eAdjacentImprovementPrereq = eInfoImprovement.meAdjacentImprovementPrereq;

                        if (eAdjacentImprovementPrereq != ImprovementType.NONE)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_IMPROVEMENT", buildImprovementLinkVariable(eAdjacentImprovementPrereq, pGame)), ((pTile != null) ? !pTile.adjacentToCityImprovementFinished(eAdjacentImprovementPrereq) : false)));
                        }
                    }

                    if (!bUpgradeImprovement)
                    {
                        ImprovementClassType eAdjacentImprovementClassPrereq = eInfoImprovement.meAdjacentImprovementClassPrereq;

                        if (eAdjacentImprovementClassPrereq != ImprovementClassType.NONE)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_IMPROVEMENT", buildImprovementClassLinkVariable(eAdjacentImprovementClassPrereq)), ((pTile != null) ? !pTile.adjacentToCityImprovementClassFinished(eAdjacentImprovementClassPrereq) : false)));
                        }
                    }

                    {
                        EffectCityType eEffectCityPrereq = eInfoImprovement.meEffectCityPrereq;

                        if (eEffectCityPrereq != EffectCityType.NONE)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildEffectCitySourceLinkVariable(eEffectCityPrereq, pCityTerritory, pGame, pActivePlayer)), ((pCityTerritory != null) ? (pCityTerritory.getEffectCityCount(eEffectCityPrereq) == 0) : false)));
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ##############################################*/
                    {
                        CityBiomeType eCityBiomeType = eInfoImprovement.meCityBiomePrereq;
                        if (eCityBiomeType != CityBiomeType.NONE)
                        {
                            TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildConceptLinkVariable("CONCEPT_CITY_BIOME"), TEXTVAR(((BetterAIInfos)infos()).cityBiome(eCityBiomeType).mName, TextManager));
                            if (pCityTerritory != null)
                            {

                                reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", reqItem, TEXTVAR(((BetterAIInfos)infos()).cityBiome(pCityTerritory.getCityBiome()).mName, TextManager));
                            }
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", reqItem), ((pCityTerritory != null) ? (pCityTerritory.getCityBiome() != eCityBiomeType) : false)));
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/


                    for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                    {
                        if (eLoopTerrain != infos().Globals.WATER_TERRAIN) // don't clutter the help text with obvious requirements
                        {
                            if (eInfoImprovement.mabTerrainInvalid[(int)eLoopTerrain] && (pTile == null || pTile.getTerrain() == eLoopTerrain))
                            {
                                lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_TERRAIN", buildTerrainLinkVariable(eLoopTerrain))));
                            }
                        }
                    }

                    for (HeightType eLoopHeight = 0; eLoopHeight < infos().heightsNum(); eLoopHeight++)
                    {
                        if (eInfoImprovement.mabHeightInvalid[(int)eLoopHeight] && (pTile == null || pTile.getHeight() == eLoopHeight))
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_TERRAIN", buildHeightLinkVariable(eLoopHeight))));
                        }
                    }

                    if (pTile != null)
                    {
                        if (eInfoImprovement.mbTerritoryOnly)
                        {
                            if (pActivePlayer != null)
                            {
                                if (pActivePlayer.getTeam() != pTile.getTeam())
                                {
                                    lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_TEAM")));
                                }
                            }
                        }

                        if (eInfoImprovement.mbRequiresUrban)
                        {
                            if (!(pTile.urbanEligible()))
                            {
                                lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_URBAN", TEXTVAR(pTile.isAnyCoastLand()), buildUrbanLinkVariable())));
                            }
                        }

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            if (infos().improvementClass(eImprovementClass).mbNoAdjacent && !(pTile.notAdjacentToImprovementClass(eImprovementClass)))
                            {
                                lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_ADJACENT", buildImprovementClassLinkVariable(eImprovementClass))));
                            }
                        }

                        if (eReligionPrereq != ReligionType.NONE)
                        {
                            if (eInfoImprovement.mbNoAdjacentReligion)
                            {
                                if (pTile.adjacentToOtherImprovementReligion(eReligionPrereq))
                                {
                                    lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_ADJACENT_RELIGION")));
                                }
                            }
                        }
                    }

                    {
                        CommaListVariableGenerator req = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, TextManager);

                        {
                            int iCount = ((pCityTerritory != null) ? pCityTerritory.getImprovementCount(eImprovement) : 0);
                            int iValue = eInfoImprovement.miMaxCityCount;

                            if (iValue > ((iCount > 0) ? 0 : 1))
                            {
                                TextVariable reqItem = buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CITY_IMPROVEMENT_COUNT", TEXTVAR(iValue)), (iCount >= iValue));

                                if (pCityTerritory != null)
                                {
                                    reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(iCount), TEXTVAR(iValue));
                                }

                                req.AddItem(reqItem);
                            }

/*####### Better Old World AI - Base DLL #######
  ### City Biome                       START ###
  ### not directy related but used for Baths ###
  ##############################################*/
                            //new: Class too
                            if (eImprovementClass != ImprovementClassType.NONE)
                            {
                                int iCountClass = ((pCityTerritory != null) ? pCityTerritory.getImprovementClassCount(eImprovementClass) : 0);
                                int iValueClass = ((BetterAIInfoImprovementClass)infos().improvementClass(eImprovementClass)).miMaxCityCount;
                                if (iValueClass == 0 || (iValue > 0 && iValue <= iValueClass && iCount >= iCountClass))
                                {
                                    //skip
                                }
                                else
                                {
                                    TextVariable reqItem = buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CITY_IMPROVEMENT_CLASS_COUNT", TEXTVAR(iValueClass)), (iCountClass >= iValueClass));

                                    if (pCityTerritory != null)
                                    {
                                        reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(iCountClass), TEXTVAR(iValueClass));
                                    }

                                    req.AddItem(reqItem);
                                }
                            }
/*####### Better Old World AI - Base DLL #######
  ### City Biome                         END ###
  ##############################################*/
                        }

                        {
                            int iValue = eInfoImprovement.miMaxFamilyCount;
                            if (iValue > 0)
                            {
                                TextVariable reqItem = buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_FAMILY_IMPROVEMENT_COUNT", TEXTVAR(iValue)), ((pCityTerritory != null) && (iValue <= pGame.countFamilyImprovements(pCityTerritory.getFamily(), eImprovement))));

                                if (pCityTerritory != null)
                                {
                                    if (pCityTerritory.hasFamily())
                                    {
                                        reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pGame.countFamilyImprovements(pCityTerritory.getFamily(), eImprovement)), TEXTVAR(iValue));
                                    }
                                }

                                req.AddItem(reqItem);
                            }
                        }

                        {
                            int iValue = eInfoImprovement.miMaxPlayerCount;
                            if (iValue > 0)
                            {
                                if ((eImprovementClass != ImprovementClassType.NONE) ? ((pCityTerritory == null) || !(pCityTerritory.isNoImprovementClassMaxUnlock(eImprovementClass))) : true)
                                {
                                    TextVariable reqItem = buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NATION_IMPROVEMENT_COUNT", TEXTVAR(iValue)), ((pActivePlayer != null) && (iValue <= pActivePlayer.getImprovementCount(eImprovement))));

                                    if (pActivePlayer != null)
                                    {
                                        reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pActivePlayer.getImprovementCount(eImprovement)), TEXTVAR(iValue));
                                    }

                                    req.AddItem(reqItem);
                                }
                            }
                        }

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            int iValue = infos().improvementClass(eImprovementClass).miMaxCultureCount;
                            if (iValue > 0)
                            {
                                if (pCityTerritory != null)
                                {
                                    TextVariable reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_CULTURE_IMPROVEMENT_COUNT", TEXTVAR(iValue), buildImprovementClassLinkVariable(eImprovementClass), buildCultureLevelLinkVariable(pCityTerritory));
                                    int maxImprovements = (int)pCityTerritory.getCulture() + (int)pCityTerritory.getCultureStep() + 1;
                                    reqItem = buildWarningTextVariable(reqItem, ((pCityTerritory != null) && maxImprovements <= pCityTerritory.getImprovementClassCount(eImprovementClass)));

                                    if (pCityTerritory != null)
                                    {
                                        reqItem = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS_FRACTION", reqItem, TEXTVAR(pCityTerritory.getImprovementClassCount(eImprovementClass)), TEXTVAR(maxImprovements));
                                    }

                                    req.AddItem(reqItem);
                                }
                            }
                        }

                        if (req.Count > 0)
                        {
                            lRequirements.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_MAX_LIST", req.Finalize()));
                        }
                    }

                    if (eImprovementClass != ImprovementClassType.NONE)
                    {
                        if (pCityTerritory != null)
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    foreach (EffectCityType eLoopEffectCity in infos().improvementClass(eImprovementClass).maeEffectCityDisabled)
                                    {
                                        if (pCityTerritory.getEffectCityCount(eLoopEffectCity) > 0)
                                        {
                                            subText.Add(buildEffectCityLinkVariable(eLoopEffectCity, pCityTerritory));
                                        }
                                    }
                                }

                                if (subText.HasContent)
                                {
                                    lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_DISABLED_BY", subText.ToTextVariable())));
                                }
                            }
                        }
                    }
                }
                //end of copy-paste region
            }
        }

        //how to create LinkTextVariable things? idk, maybe I'll try to figure this out later.
        //but it looks like a lot of work when adding something competely new like cityBiome
        //because I need something to link to that doesn't exist yet
        //example for link creatioon:
        //public virtual TextVariable buildEffectCityLinkVariable(EffectCityType eEffectCity, City pCity)
        //{
        //    using (new UnityProfileScope("HelpText.buildEffectCityLinkVariable"))
        //    {
        //        InfoEffectCity effectCity = infos().effectCity(eEffectCity);
        //        TextType effectCityName = effectCity.mName;
        //        GrammaticalGenderType eCityGrammaticalGender = pCity != null ? pCity.getGrammaticalGender() : GrammaticalGenderType.NONE;
        //        if (eCityGrammaticalGender != GrammaticalGenderType.NONE)
        //        {
        //            foreach (PairStruct<GrammaticalGenderType, TextType> pair in infos().effectCity(eEffectCity).mlpGrammaticalGenderNames)
        //            {
        //                GrammaticalGenderType eGrammaticalGender = pair.First;
        //                if (eGrammaticalGender == eCityGrammaticalGender)
        //                {
        //                    effectCityName = pair.Second;
        //                }
        //            }
        //        }
        //        return buildLinkTextVariable(TEXTVAR_TYPE(effectCityName), ItemType.HELP_LINK, nameof(LinkType.HELP_EFFECT_CITY), effectCity.SafeTypeString(), ((pCity != null) ? pCity.getID() : -1).ToStringCached());
        //    }
        //}
        // then there's buildWidgetHelp in HelpText and buildEffectCityHelp
        // and I assume a widget also needs UI components

        //120 lines copy&paste START
        public override TextBuilder buildResourceHelp(TextBuilder builder, ResourceType eResource, Game pGame, Player pPlayer, Tile pTile, Player pActivePlayer, bool bName = true)
        {
            using (new UnityProfileScope("HelpText.buildResourceHelp"))
            {
                City pCityTerritory = pTile?.cityTerritory();

                if (bName)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_NAME", TEXTVAR_TYPE(infos().resource(eResource).mName), TEXTVAR((pTile != null) ? pTile.isHarvested() : false));
                }

                if (pPlayer != null)
                {
                    for (FamilyType eLoopFamily = 0; eLoopFamily < infos().familiesNum(); eLoopFamily++)
                    {
                        if (pPlayer.isFamilyStarted(eLoopFamily))
                        {
                            int iValue = pGame.familyClass(eLoopFamily).maiLuxuryMissingOpinion[(int)eResource];
                            if (iValue != 0)
                            {
                                builder.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_MISSING", buildFamilyLinkVariable(eLoopFamily, pGame), TEXTVAR(iValue)), !pPlayer.isFamilyLuxury(eLoopFamily, eResource)));
                            }
                        }
                    }
                }


                using (TextBuilder commaList = TextBuilder.GetTextBuilder(TextManager))
                {
                    using (commaList.BeginScope(TextBuilder.ScopeType.COMMA))
                    {
                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            int iValue = infos().resource(eResource).maiYieldNoImprovement[(int)eLoopYield];
                            if (iValue > 0)
                            {
                                commaList.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, true, iMultiplier: Constants.YIELDS_MULTIPLIER));
                            }
                        }
                    }

                    if (commaList.HasContent)
                    {
                        builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_NO_IMPROVEMENT", commaList.ToTextVariable(), buildTurnScaleName(pGame)));
                    }
                }

                for (ImprovementClassType eLoopImprovementClass = 0; eLoopImprovementClass < infos().improvementClassesNum(); eLoopImprovementClass++)
                {
                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_FROM_LIST", buildImprovementClassLinkVariable(eLoopImprovementClass))))
                    {
                        bool bAdded = false;

                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                        {
                            int iValue = 0;
                            infos().Helpers.yieldOutputResource(eLoopImprovementClass, eLoopYield, eResource, ref iValue);
                            if (iValue != 0)
                            {
                                builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, true, false, Constants.YIELDS_MULTIPLIER));
                                bAdded = true;
                            }
                        }


/*####### Better Old World AI - Base DLL #######
  ### show abNoBaseOutput              START ###
  ##############################################*/
                        for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                        {
                            if (infos().improvement(eLoopImprovement).meClass == eLoopImprovementClass)
                            {
                                if (infos().improvement(eLoopImprovement).mabNoBaseOutput[(int)eResource])
                                {
                                    builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_REPLACES_BASE_IMPROVEMENT_OUTPUT"));
                                }

                                //assumption: mabNoBaseOutput is the same for all improvements in improvement class, so I only need to check the first
                                break;
                            }
                        }
/*####### Better Old World AI - Base DLL #######
  ### show abNoBaseOutput                END ###
  ##############################################*/

                        {
                            EffectCityType eEffectCity = infos().improvementClass(eLoopImprovementClass).maeResourceCityEffect[(int)eResource];
                            if (eEffectCity != EffectCityType.NONE)
                            {
                                buildEffectCityHelp(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory, pActivePlayer);
                                bAdded = true;
                            }
                        }

                        if (bAdded)
                        {
                            TechType ePrereqTech = infos().improvementClass(eLoopImprovementClass).meTechPrereq;

                            if (ePrereqTech != TechType.NONE)
                            {
                                if ((pActivePlayer != null) && !(pActivePlayer.isTechAcquired(ePrereqTech)))
                                {
                                    builder.AddWithParenthesis(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildTechLinkVariable(ePrereqTech))));
                                }
                            }
                        }
                    }
                }

                for (SpecialistClassType eLoopSpecialistClass = 0; eLoopSpecialistClass < infos().specialistClassesNum(); eLoopSpecialistClass++)
                {
                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_FROM_LIST", buildSpecialistClassLinkVariable(eLoopSpecialistClass, pTile))))
                    {
                        EffectCityType eEffectCity = infos().specialistClass(eLoopSpecialistClass).maeResourceCityEffect[(int)eResource];
                        if (eEffectCity != EffectCityType.NONE)
                        {
                            buildEffectCityHelp(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory, pActivePlayer);
                        }
                    }
                }

                {
                    TextVariable harvestText = buildResourceHarvestHelpVariable(pPlayer, pTile, pActivePlayer);

                    if (!harvestText.IsNullOrEmpty())
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_HARVEST", harvestText);
                    }
                }

                using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                {
                    for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < infos().effectCitiesNum(); eLoopEffectCity++)
                    {
                        if (infos().effectCity(eLoopEffectCity).meLuxuryResource == eResource)
                        {
                            subText.Add(buildEffectCitySourceLinkVariable(eLoopEffectCity, null, pGame, pActivePlayer));
                        }
                    }

                    if (subText.HasContent)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_FROM", subText.ToTextVariable());
                    }
                }

/*####### Better Old World AI - Base DLL #######
  ### show total Resource count on map START ###
  ##############################################*/
                if (pGame != null && ( (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT + ((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COUNT + ((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES) > 0 ) )
                {
                    if (pGame.getResourceCount(eResource) <= 0)
                    {
                        if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT > 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_COUNT_ON_MAP", pGame.getResourceCount(eResource));
                        }
                    }
                    else if (pPlayer != null)
                    {
                        int iTotal = 0;
                        int iPlayerTerritory = 0;
                        int iTeamTerritory = 0; //not player, other team member
                        int iRevealed = 0;
                        int iFog = 0;

                        CommaListVariableGenerator tileTerritoryList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.NONE, TextManager);
                        CommaListVariableGenerator tileTeamList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.PARENTHESIS, true, TextManager);
                        CommaListVariableGenerator tileRevealedList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.PARENTHESIS, true, TextManager);
                        CommaListVariableGenerator tileUnrevealedList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.PARENTHESIS_OUTER, true, TextManager);
                        for (int iI = 0; iI < pGame.getNumTiles(); ++iI)
                        {
                            Tile pLoopTile = pGame.tile(iI);
                            if (pLoopTile != null)
                            {
                                if (pLoopTile.getResource() == eResource)
                                {
                                    if (pLoopTile.getTeam() == pPlayer.getTeam())
                                    {
                                        
                                        pCityTerritory = pLoopTile.cityTerritory();
                                        if (pCityTerritory == null) //this should not be possible
                                        {
                                            if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES > 0)
                                            {
                                                tileTerritoryList.AddItem(buildLocationLinkVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_LINK_TILE", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY())), pLoopTile));
                                            }
                                        }
                                        else
                                        {
                                            TextVariable name = null;
                                            if (pCityTerritory.getNameType() != CityNameType.NONE)
                                            {
                                                name = TEXTVAR(infos().cityName(pCityTerritory.getNameType()).mName, TextManager);
                                            }
                                            else
                                            {
                                                name = TEXTVAR(pCityTerritory.getName());
                                            }
                                            if (name == null) name = TEXTVAR_TYPE("TEXT_HELPTEXT_LINK_TILE", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY()));

                                            //1. a Specialist could be in build queue
                                            TextVariable CityResource = null;
                                            if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES > 0)
                                            { 
                                                SpecialistType eTileSpecialist = SpecialistType.NONE;
                                                bool bSpecialistQueued = false;
                                                bool bSpecialistBuilding = false;
                                                ImprovementType eTileImprovement = ImprovementType.NONE;
                                                bool bImprovementBuilding = false;
                                                bool bImprovementPillaged = false;
                                                int iTurnsLeft = 0;
                                                if (pCityTerritory.getBuildCount() > 0)
                                                {
                                                    for (int i = pCityTerritory.getBuildCount() - 1; i >= 0; i--)
                                                    {
                                                        CityQueueData data = pCityTerritory.getBuildQueueNode(i);
                                                        if (data.meBuild == infos().Globals.SPECIALIST_BUILD && data.miData == pLoopTile.getID())
                                                        {
                                                            eTileSpecialist = (SpecialistType)(data.miType);
                                                            if (i == 0)
                                                            {
                                                                bSpecialistBuilding = true;
                                                                iTurnsLeft = pCityTerritory.getBuildTurnsLeft(data);
                                                            }
                                                            else
                                                            {
                                                                bSpecialistQueued = true;
                                                            }

                                                            break;
                                                        }
                                                    }
                                                }
                                                if (!bSpecialistQueued && !bSpecialistBuilding)
                                                {
                                                    //2. a Specialist could already be there
                                                    if (pLoopTile.getSpecialist() != SpecialistType.NONE)
                                                    {
                                                        eTileSpecialist = pLoopTile.getSpecialist();
                                                    }
                                                }
                                                if (eTileSpecialist == SpecialistType.NONE)
                                                {
                                                    //3. there could at least be an improvement
                                                    eTileImprovement = pLoopTile.getImprovement();
                                                    if (eTileImprovement != ImprovementType.NONE)
                                                    {
                                                        if (pLoopTile.isPillaged())
                                                        {
                                                            bImprovementPillaged = true;
                                                            iTurnsLeft = pLoopTile.getImprovementPillageTurns();
                                                        }
                                                        else if (pLoopTile.getImprovementBuildTurnsLeft() > 0)
                                                        {
                                                            bImprovementBuilding = true;
                                                            iTurnsLeft = pLoopTile.getImprovementBuildTurnsLeft();
                                                        }
                                                    }
                                                }

                                                //now the fun part: building the text
                                                if (bSpecialistQueued)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_IN_QUEUE");
                                                }
                                                else if (bSpecialistBuilding)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_SPECIALIST_IN_TRAINING");
                                                }
                                                else if (bImprovementBuilding)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_UNDER_CONSTRUCTION");
                                                }
                                                else if (bImprovementPillaged)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_PILLAGED_IMPROVEMENT");
                                                }

                                                if (iTurnsLeft > 0)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", CityResource, TEXTVAR_TYPE("TEXT_HELPTEXT_TURNS_TEXT_SHORT", TEXTVAR(iTurnsLeft), TEXTVAR(true), pGame.turnScaleNameShort(iTurnsLeft)));
                                                }

                                                if (eTileSpecialist != SpecialistType.NONE)
                                                {
                                                    if (CityResource != null)
                                                    {
                                                        CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", TEXTVAR_TYPE(infos().specialist(eTileSpecialist).meName), CityResource);
                                                    }
                                                    else
                                                    {
                                                        CityResource = TEXTVAR_TYPE(infos().specialist(eTileSpecialist).meName);
                                                    }
                                                }
                                                else if (eTileImprovement != ImprovementType.NONE)
                                                {
                                                    if (CityResource != null)
                                                    {
                                                        CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", TEXTVAR_TYPE(infos().improvement(eTileImprovement).mName), CityResource);
                                                    }
                                                    else
                                                    {
                                                        CityResource = TEXTVAR_TYPE(infos().improvement(eTileImprovement).mName); ;
                                                    }
                                                }

                                                if (CityResource == null)
                                                {
                                                    CityResource = TEXTVAR_TYPE("TEXT_HELPTEXT_COMMA_SPACE_TWO", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY()));
                                                }

                                            }

                                            if (pCityTerritory.getPlayer() == pPlayer.getPlayer())
                                            {
                                                iPlayerTerritory++;
                                                if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES >= 1)
                                                {
                                                    tileTerritoryList.AddItem(buildLocationLinkVariable((TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_TILE_IN_CITY", CityResource, name)), pLoopTile));
                                                }
                                            }
                                            else 
                                            {
                                                iTeamTerritory++;
                                                if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES >= 2)
                                                {
                                                    tileTeamList.AddItem(buildLocationLinkVariable((TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_TILE_IN_CITY", CityResource, name)), pLoopTile));
                                                }
                                            }
                                        }

                                    }
                                    else if (pLoopTile.isRevealed(pPlayer.getTeam()))
                                    {
                                        iRevealed++;
                                        if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES >= 3)
                                        {
                                            tileRevealedList.AddItem(buildLocationLinkVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_COMMA_SPACE_TWO", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY())), pLoopTile));
                                        }
                                    }
                                    else
                                    {
                                        iFog++;
                                        if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES >= 4)
                                        {
                                            if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COORDINATES >= 5)
                                            {
                                                tileRevealedList.AddItem(buildLocationLinkVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_COMMA_SPACE_TWO", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY())), pLoopTile));
                                            }
                                            else
                                            {
                                                tileUnrevealedList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_COMMA_SPACE_TWO", TEXTVAR(pLoopTile.getX()), TEXTVAR(pLoopTile.getY())));
                                            }
                                        }
                                    }

                                    iTotal++;
                                    if (iTotal == pGame.getResourceCount(eResource))
                                    {
                                        break;
                                    }
                                }
                            }
                        }

                        CommaListVariableGenerator countList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.NONE, TextManager);
                        //if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT > 0)
                        //{
                        //    countList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_ON_MAP", pGame.getResourceCount(eResource)));
                        //}

                        if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COUNT >= 1)
                        {
                            countList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_PLAYER", iPlayerTerritory));

                            if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COUNT >= 2)
                            {
                                if (iTeamTerritory > 0) countList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_TEAM", iTeamTerritory));

                                if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COUNT >= 3)
                                {
                                    if (iRevealed > 0) countList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_REVEALED", iRevealed));

                                    if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_COUNT >= 4)
                                    {
                                        if (iFog > 0) countList.AddItem(TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_FOG", iFog));
                                    }
                                }
                            }
                        }
                        if (countList.Count > 0)
                        {
                            if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT > 0)
                            {
                                builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_ON_MAP", pGame.getResourceCount(eResource)), countList.Finalize()));
                            }
                            else
                            {
                                builder.Add(countList.Finalize());
                            }
                        }
                        else if (((BetterAIInfoGlobals)infos().Globals).BAI_SHOW_RESOURCE_TILE_TOTAL_COUNT > 0)
                        {
                            builder.Add((TEXTVAR_TYPE("TEXT_HELPTEXT_RESOURCE_COUNT_ON_MAP", pGame.getResourceCount(eResource))));
                        }


                        CommaListVariableGenerator tileList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.NONE, TextManager);
                        if (tileTerritoryList.Count > 0)
                        {
                            tileList.AddItem(tileTerritoryList.Finalize());
                        }
                        if (tileRevealedList.Count > 0)
                        {
                            tileList.AddItem(tileRevealedList.Finalize());
                        }
                        if (tileUnrevealedList.Count > 0)
                        {
                            tileList.AddItem(tileUnrevealedList.Finalize());
                        }
                        if (tileList.Count > 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_RESOURCE_TILES_ON_MAP", tileList.Finalize());
                        }
                    }
                }
/*####### Better Old World AI - Base DLL #######
  ### show total Resource count on map   END ###
  ##############################################*/

                return builder;
            }
        }
        //120 line copy paste END

        public override TextBuilder buildEffectUnitHelp(TextBuilder builder, EffectUnitType eEffectUnit, Game pGame, bool bSkipIcons = false, bool bRightJustify = false)
        {
            builder = base.buildEffectUnitHelp(builder, eEffectUnit, pGame, bSkipIcons, bRightJustify);

/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
            BetterAIInfoEffectUnit eInfoEffectUnit = (BetterAIInfoEffectUnit)infos().effectUnit(eEffectUnit);
            if (eInfoEffectUnit.mbAmphibious)
            {
                if (((BetterAIInfoGlobals)infos().Globals).BAI_AMPHIBIOUS_RIVER_CROSSING_DISCOUNT > 0 && ((BetterAIInfoGlobals)infos().Globals).RIVER_CROSSING_COST_EXTRA > 0)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_UNIT_AMPHIBIOUS_RIVER_CROSSING");
                }
                if (((BetterAIInfoGlobals)infos().Globals).BAI_EMBARKING_COST_EXTRA > 0 && ((BetterAIInfoGlobals)infos().Globals).BAI_HARBOR_OR_AMPHIBIOUS_EMBARKING_DISCOUNT > 0)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_EFFECT_UNIT_AMPHIBIOUS_EMBARKING");
                }
            }
/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement           END ###
  ##############################################*/

            return builder;
        }


/*####### Better Old World AI - Base DLL #######
  ### misc                             START ###
  ##############################################*/
        //public struct BetterAICommaListVariableGenerator : CommaListVariableGenerator
        //{
        //}
        //fuck me, structs can't inherit
        //I can't overrride a struct but at least inside the derived BetterAIHelpText class I can call this for proper japanese list generation
        new public struct CommaListVariableGenerator
        {
            public enum ListType
            {
                NONE = 0,
                OR,
                AND,
                LINEBREAK,
                OR_LINEBREAK,
                AND_LINEBREAK
            }
            public enum EncloseType
            {
                NONE = 0,
                PARENTHESIS,
                PARENTHESIS_OUTER
            }

            private ListType listType;
            private EncloseType encloseType;
            private int numItems;
            private bool bEncloseSingleItem;
            private TextVariable result;
            private TextVariable prevItem;
            private TextManager textManager;
            private string firstItemTextType;
            private string separatorTextType;
            private string lastItemSeparatorTextType;

            public int Count { get { return numItems; } }

            public CommaListVariableGenerator(string firstItemTextType, string listSeparatorTextType, string lastItemSeparatorTextType, TextManager textManager)
            {
                this.listType = ListType.NONE;
                this.encloseType = EncloseType.NONE;
                this.textManager = textManager;
                this.bEncloseSingleItem = false;

                numItems = 0;
                prevItem = null;
                result = null;
                this.firstItemTextType = firstItemTextType;
                separatorTextType = listSeparatorTextType;
                this.lastItemSeparatorTextType = lastItemSeparatorTextType;
            }
            public CommaListVariableGenerator(string firstItemTextType, string listSeparatorTextType, TextManager textManager)
            {
                this.listType = ListType.NONE;
                this.encloseType = EncloseType.NONE;
                this.textManager = textManager;
                this.bEncloseSingleItem = false;

                numItems = 0;
                prevItem = null;
                result = null;
                this.firstItemTextType = firstItemTextType;
                separatorTextType = listSeparatorTextType;
                this.lastItemSeparatorTextType = separatorTextType;
            }

            public CommaListVariableGenerator(ListType listType, EncloseType encloseType, bool bEncloseSingleItem, TextManager textManager)
            {
                this.listType = listType;
                this.encloseType = encloseType;
                this.textManager = textManager;
                this.bEncloseSingleItem = bEncloseSingleItem;

                numItems = 0;
                prevItem = null;
                result = null;
                firstItemTextType = null;
                separatorTextType = "TEXT_HELPTEXT_COMMA_SPACE_TWO";
                lastItemSeparatorTextType = separatorTextType;
            }

            public CommaListVariableGenerator(ListType listType, EncloseType encloseType, TextManager textManager)
            {
                this.listType = listType;
                this.encloseType = encloseType;
                this.textManager = textManager;
                this.bEncloseSingleItem = false;

                numItems = 0;
                prevItem = null;
                result = null;
                firstItemTextType = null;
                separatorTextType = "TEXT_HELPTEXT_COMMA_SPACE_TWO";
                lastItemSeparatorTextType = separatorTextType;
            }

            public CommaListVariableGenerator(ListType listType, TextManager textManager)
            {
                this.listType = listType;
                this.encloseType = EncloseType.NONE;
                this.textManager = textManager;
                this.bEncloseSingleItem = false;

                numItems = 0;
                prevItem = null;
                result = null;
                firstItemTextType = null;
                separatorTextType = "TEXT_HELPTEXT_COMMA_SPACE_TWO";
                lastItemSeparatorTextType = separatorTextType;
            }

            public void AddItem(TextVariable item)
            {
                if (prevItem != null)
                {
                    if (numItems == 1)
                    {
                        if (encloseType != EncloseType.NONE)
                        {
                            switch (encloseType)
                            {
                                case EncloseType.PARENTHESIS: firstItemTextType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                                case EncloseType.PARENTHESIS_OUTER: firstItemTextType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS_OUTER"; break;
                                default: firstItemTextType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                            }
                        }

                        if (string.IsNullOrEmpty(firstItemTextType))
                        {
                            result = prevItem;

                        }
                        else
                        {
                            result = TEXTVAR(firstItemTextType, textManager, prevItem);
                        }
                    }
                    else
                    {
                        //new code for fixed japanese lists
                        string textType;
                        if (encloseType != EncloseType.NONE)
                        {
                            switch (encloseType)
                            {
                                case EncloseType.PARENTHESIS: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                                case EncloseType.PARENTHESIS_OUTER: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS_OUTER"; break;
                                default: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                            }
                            prevItem = TEXTVAR(textType, textManager, prevItem);
                        }
    
                        switch (listType)
                        {
                            case ListType.AND: textType = "TEXT_HELPTEXT_AND_MIDLIST"; break;
                            case ListType.OR: textType = "TEXT_HELPTEXT_OR_MIDLIST"; break;
                            case ListType.AND_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_AND_MIDLIST"; break;
                            case ListType.OR_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_OR_MIDLIST"; break;
                            case ListType.LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_TWO"; break;
                            default: textType = separatorTextType; break;
                        }
                        result = TEXTVAR(textType, textManager, result, prevItem);
                        //end of new code

                        //old code
                        //result = TEXTVAR(separatorTextType, textManager, result, prevItem);
                    }
                }

                prevItem = item;
                numItems++;
            }

            public TextVariable Finalize()
            {
                if (numItems == 0)
                    return TEXTVAR("");

                if (numItems == 1)
                {
                    if (string.IsNullOrEmpty(firstItemTextType) && !bEncloseSingleItem)
                    {
                        result = prevItem;
                        //no enclosing if there's just 1 item
                    }
                    else
                    {
                        if (encloseType != EncloseType.NONE)
                        {
                            string textType;
                            switch (encloseType)
                            {
                                case EncloseType.PARENTHESIS: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                                case EncloseType.PARENTHESIS_OUTER: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS_OUTER"; break;
                                default: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                            }
                            result = TEXTVAR(textType, textManager, prevItem);
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(firstItemTextType)) //implies bEcloseSingleItem
                            {
                                result = prevItem; //no EncloseType chosen, no firstItemTextType set
                            }
                            else
                            {
                                result = TEXTVAR(firstItemTextType, textManager, prevItem);
                            }
                        }
                    }
                }
                else if (numItems == 2)
                {
                    string textType;

                    if (encloseType != EncloseType.NONE)
                    {
                        switch (encloseType)
                        {
                            case EncloseType.PARENTHESIS: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                            case EncloseType.PARENTHESIS_OUTER: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS_OUTER"; break;
                            default: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                        }
                        prevItem = TEXTVAR(textType, textManager, prevItem);
                    }

                    switch (listType)
                    {
                        case ListType.AND: textType = "TEXT_HELPTEXT_AND_TWO"; break;
                        case ListType.OR: textType = "TEXT_HELPTEXT_OR_TWO"; break;
                        case ListType.AND_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_AND_TWO"; break;
                        case ListType.OR_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_OR_TWO"; break;
                        case ListType.LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_TWO"; break;
                        default: textType = separatorTextType; break;
                    }
                    result = TEXTVAR(textType, textManager, result, prevItem);
                }
                else
                {
                    string textType;
                    if (encloseType != EncloseType.NONE)
                    {
                        switch (encloseType)
                        {
                            case EncloseType.PARENTHESIS: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                            case EncloseType.PARENTHESIS_OUTER: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS_OUTER"; break;
                            default: textType = "TEXT_HELPTEXT_LISTITEM_ENCLOSED_PARENTHESIS"; break;
                        }
                        prevItem = TEXTVAR(textType, textManager, prevItem);
                    }

                    switch (listType)
                    {
                        case ListType.AND: textType = "TEXT_HELPTEXT_COMMA_AND_TWO"; break;
                        case ListType.OR: textType = "TEXT_HELPTEXT_COMMA_OR_TWO"; break;
                        case ListType.AND_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_AND_TWO"; break;
                        case ListType.OR_LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_OR_TWO"; break;
                        case ListType.LINEBREAK: textType = "TEXT_HELPTEXT_COMMA_LINEBREAK_TWO"; break;
                        default: textType = lastItemSeparatorTextType; break;
                    }
                    result = TEXTVAR(textType, textManager, result, prevItem);
                }

                return result;
            }
        }
/*####### Better Old World AI - Base DLL #######
  ### misc                               END ###
  ##############################################*/

    }
}
