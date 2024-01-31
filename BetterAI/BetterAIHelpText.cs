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
using TenCrowns.ClientCore;
using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using UnityEngine;
using UnityEngine.UI;
using System.Xml;

namespace BetterAI
{
    public class BetterAIHelpText : HelpText
    {
        //lines 44-48
        public BetterAIHelpText(ModSettings pModSettings) : base(pModSettings)
        {
        }


        //lines 2998-3012
        public override TextVariable buildHappinessLevelLinkVariable(City pCity, bool bShort = false)
        {
            using (new UnityProfileScope("HelpText.buildHappinessLevelLinkVariable"))
            {
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                START ###
  ##############################################*/
                if (bShort)
                {
                    //return buildLinkTextVariable(concatenateSpace(TEXTVAR_TYPE(infos().yield(pCity.getHappinessLevel() >= 0 ? infos().Globals.HAPPINESS_YIELD : infos().Globals.DISCONTENT_YIELD).meName), 
                    //    TEXTVAR(Math.Abs(pCity.getHappinessLevel()))), ItemType.HELP_LINK, nameof(LinkType.HELP_HAPPINESS_LEVEL), pCity.getID().ToStringCached());
                    return buildLinkTextVariable(concatenateSpace(TEXTVAR_TYPE(infos().yield(!(((BetterAICity)pCity).isDiscontent()) ? infos().Globals.HAPPINESS_YIELD : infos().Globals.DISCONTENT_YIELD).meName),
                        TEXTVAR(Math.Abs(pCity.getHappinessLevel()))), ItemType.HELP_LINK, nameof(LinkType.HELP_HAPPINESS_LEVEL), pCity.getID().ToStringCached());
                }
                else
                {
                    //return buildLinkTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_LINK_HAPPINESS", TEXTVAR(Math.Abs(pCity.getHappinessLevel())), TEXTVAR(pCity.getHappinessLevel() >= 0)), ItemType.HELP_LINK, nameof(LinkType.HELP_HAPPINESS_LEVEL), pCity.getID().ToStringCached());
                    return buildLinkTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_LINK_HAPPINESS", TEXTVAR(Math.Abs(pCity.getHappinessLevel())), TEXTVAR(!(((BetterAICity)pCity).isDiscontent()))), ItemType.HELP_LINK, nameof(LinkType.HELP_HAPPINESS_LEVEL), pCity.getID().ToStringCached());
                }
/*####### Better Old World AI - Base DLL #######
  ### Disconent Level 0                  END ###
  ##############################################*/
            }
        }

/*####### Better Old World AI - Base DLL #######
  ### ZOC ignore exceptions            START ###
  ##############################################*/
        //lines 3044-3047
        //public virtual TextVariable buildIgnoreZOCLinkVariable()
        //{
        //    return buildConceptLinkVariable("CONCEPT_IGNORES_ZOC");
        //}
        public virtual TextVariable buildIgnoreZOCWithExceptionsLinkVariable(UnitType eUnit)
        {
            CommaListVariableGenerator blockerList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.AND, TextManager);
            BetterAIInfoUnit UnitInfo = (BetterAIInfoUnit)infos().unit(eUnit);
            foreach (EffectUnitType ZOCBlockerEffect in UnitInfo.maeBlockZOCEffectUnits)
            {
                blockerList.AddItem(buildEffectUnitLinkVariable(ZOCBlockerEffect));
            }
            return TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_ENCLOSED_PARENTHESIS", buildIgnoreZOCLinkVariable(), TEXTVAR_TYPE("TEXT_HELPTEXT_ZOC_IGNORE_BLOCKER_EFFECT_UNITS", blockerList.Finalize()));
        }
/*####### Better Old World AI - Base DLL #######
  ### ZOC ignore exceptions              END ###
  ##############################################*/

        //lines 8684-13513
        public override TextBuilder buildWidgetHelp(TextBuilder builder, WidgetData pWidget, ClientManager pManager, bool bIncludeEncyclopediaFooter = true)
        {
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
                    buildAgentReturnText(builder, pUnit, pCity, pGame, pActivePlayer);

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

        public override TextBuilder buildYieldDebugText(TextBuilder builder, YieldType eYield, ClientManager pManager)
        {
            using (new UnityProfileScope("buildYieldDebugText"))
            {
                City pCity = pManager.Selection.getSelectedCity();
                builder.Add(TEXT("TEXT_HELPTEXT_AI_VALUE") + ": ");
                if (pCity == null)
                {
                    builder.Add(pManager.activePlayer().AI.yieldValue(eYield), true);
                }
                else
                {
                    //builder.Add(pManager.activePlayer().AI.calculateCityYieldValue(eYield, pCity), true);
                    builder.Add(((BetterAIPlayer.BetterAIPlayerAI)(pManager.activePlayer().AI)).calculateCityYieldValue(eYield, pCity, out _), true);
                }
                builder.Add("*", true);
                builder.Add(QUICKTEXTVAR(TEXT("TEXT_HELPTEXT_AI_STOCKPILE") + ": {0}*", pManager.activePlayer().AI.getModifiedYieldStockpileWhole(eYield)));
                return builder;
            }
        }

        //lines 18273-19072
        public override void buildTileTooltip(Tile pTile, ClientManager pManager, UITileTooltipData outTileData, TextBuilder remainingText)
        {
            using (new UnityProfileScope("HelpText.buildTileTooltip"))
            {
                Player pActivePlayer = pManager.activePlayer();
                Game pGame = pManager.GameClient;
                bool bShowAll = pManager.Renderer.isShowAllMap(true);
                TeamType eActiveTeam = TeamType.NONE;
                if (!bShowAll && pActivePlayer != null)
                {
                    eActiveTeam = pActivePlayer.getTeam();
                }
                bool bMouseoverVisible = pTile.isVisible(eActiveTeam);
                ResourceType eResource = pTile.getResource();

                outTileData.Clear();

                if (pTile.isBoundary())
                {
                    return;
                }

                TerrainType eTerrain = pTile.getRevealedTerrain(eActiveTeam);
                HeightType eHeight = pTile.getHeight();
                ImprovementType eImprovement = pTile.getRevealedImprovement(eActiveTeam);

                City pCityTerritory = pTile.revealedCityTerritory(eActiveTeam);
                bool bInfoVisible = pCityTerritory?.isInfoVisible(pActivePlayer.getTeam(), pManager) ?? true;
                bool bExpandTooltip = pManager.Interfaces.Hotkeys.IsHotkeyPressed(mInfos.Globals.HOTKEY_EXPAND_TOOLTIP);

                {
                    TextVariable tileTitleText = buildSlashText(buildTerrainLinkVariable(eTerrain, pTile), buildHeightLinkVariable(eHeight, pTile));

                    if (pTile.hasRevealedVegetation(eActiveTeam))
                    {
                        TextManager.TEXT(outTileData.Title, buildSlashText(tileTitleText, buildVegetationLinkVariable(pTile.getRevealedVegetation(eActiveTeam))));
                    }
                    else if (!pTile.isRevealedUrban(eActiveTeam) && (!pTile.hasRevealedImprovement(eActiveTeam) || pTile.isImprovementUnfinished()))
                    {
                        TextManager.TEXT(outTileData.Title, buildSlashText(tileTitleText, buildClearLinkVariable()));
                    }
                    else if (pTile.isCitySiteActive(eActiveTeam))
                    {
                        TextManager.TEXT(outTileData.Title, buildSlashText(tileTitleText, buildCitySiteLinkVariable()));
                    }
                    else
                    {
                        TextManager.TEXT(outTileData.Title, tileTitleText);
                    }
                }

                outTileData.TileID = pTile.getID();

                if (pTile.isRoad())
                {
                    outTileData.AddKeyValueTileData(TextManager, buildRoadLinkVariable(), TEXTVAR(""));
                }

                using (new UnityProfileScope("HelpText.buildTileTooltip.Water"))
                {
                    if (pTile.isFreshWater())
                    {
                        outTileData.AddKeyValueTileData(TextManager, buildFreshWaterLinkVariable(), TEXTVAR(""));
                    }
                    else if (pTile.isFreshWaterAccess())
                    {
                        outTileData.AddKeyValueTileData(TextManager, buildFreshWaterLinkVariable(pTile), TEXTVAR(""));
                    }
                }

                using (new UnityProfileScope("HelpText.buildTileTooltip.River"))
                {
                    if (pTile.isRiver())
                    {
                        using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                {
                                    if (pTile.isRiver(eLoopDirection))
                                    {
                                        builder.Add(TEXT(infos().direction(eLoopDirection).mName));
                                    }
                                }
                            }
                            if (pGame.isOccurrenceActive(ePlayer: pActivePlayer.getPlayer()))
                            {
                                for (int i = 0; i < pGame.getNumOccurrences(); ++i)
                                {
                                    OccurrenceData pLoopData = pGame.getOccurrenceDataAt(i);
                                    if (pLoopData.isActive())
                                    {
                                        InfoOccurrence occurrence = infos().occurrence(pLoopData.meType);
                                        if (occurrence.miRiverMovementCostBonus != 0 && pTile.isRiver())
                                        {
                                            builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_MOVEMENT_FROM", buildSignedTextVariable(-(occurrence.miRiverMovementCostBonus / infos().Globals.MOVEMENT_MULTIPLER), bColor: true), buildOccurrenceLinkVariable(pLoopData.meType)));
                                        }
                                        if (occurrence.miBaseYieldRiverModifier != 0 && pTile.isRiver())
                                        {
                                            builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(occurrence.miBaseYieldRiverModifier, true, bColor: true), buildOccurrenceLinkVariable(pLoopData.meType)));
                                        }
                                    }
                                }
                            }

                            outTileData.AddKeyValueTileData(TextManager, buildRiverLinkVariable(pTile), builder.ToTextVariable());
                        }
                    }
                }

                using (new UnityProfileScope("HelpText.buildTileTooltip.Coast"))
                {
                    if (pTile.isSaltCoastLand() || pTile.isSaltCoastWater())
                    {
                        if (pGame.isOccurrenceActive(ePlayer: pActivePlayer.getPlayer()))
                        {
                            using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                            {
                                for (int i = 0; i < pGame.getNumOccurrences(); ++i)
                                {
                                    OccurrenceData pLoopData = pGame.getOccurrenceDataAt(i);
                                    if (pLoopData.isActive())
                                    {
                                        InfoOccurrence occurrence = infos().occurrence(pLoopData.meType);
                                        if (occurrence.miBaseYieldCoastModifier != 0)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(occurrence.miBaseYieldCoastModifier, true, bColor: true), buildOccurrenceLinkVariable(pLoopData.meType));
                                        }
                                        if (occurrence.miUnitDamageCoastalFlat != 0 && pTile.isFlat())
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_DAMAGE_UNITS_FROM", buildSignedTextVariable(-occurrence.miUnitDamageCoastalFlat, bColor: true), buildTurnScaleName(pGame), buildOccurrenceLinkVariable(pLoopData.meType));
                                        }
                                    }
                                }
                                if (builder.HasContent)
                                {
                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HEIGHT_COAST"), builder.ToTextVariable());
                                }
                            }
                        }
                    }
                }

                using (new UnityProfileScope("HelpText.buildTileTooltip.Terrain"))
                {
                    if (pTile.getTerrain() != TerrainType.NONE)
                    {
                        int iValue = pTile.terrain().miUnitDamage;
                        iValue += pTile.getHeight() != HeightType.NONE ? pTile.height().miUnitDamage : 0;
                        if (iValue != 0)
                        {
                            outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_UNIT_ON_TILE"),
                                TEXTVAR_TYPE("TEXT_HELPTEXT_LINK_HELP_UNIT_DAMAGE", buildSignedTextVariable(-(iValue)), buildTurnScaleName(pGame)));
                        }
                        if (pGame.isOccurrenceActive(ePlayer: pActivePlayer.getPlayer()))
                        {
                            for (int i = 0; i < pGame.getNumOccurrences(); ++i)
                            {
                                OccurrenceData pLoopData = pGame.getOccurrenceDataAt(i);
                                if (pLoopData.isActive())
                                {
                                    InfoOccurrence occurrence = infos().occurrence(pLoopData.meType);
                                    using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        int iTerrainDamage = occurrence.maiUnitTerrainDamage[pTile.getTerrain()];
                                        if (iTerrainDamage != 0)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_DAMAGE_UNITS_FROM", buildSignedTextVariable(-iTerrainDamage, bColor: true), buildTurnScaleName(pGame), buildOccurrenceLinkVariable(pLoopData.meType));
                                        }
                                        int iTerrainMovementBonus = -(occurrence.maiTerrainMovementCostBonus[pTile.getTerrain()] / infos().Globals.MOVEMENT_MULTIPLER);
                                        if (iTerrainMovementBonus != 0)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_MOVEMENT_FROM", buildSignedTextVariable(iTerrainMovementBonus, bColor: true), buildOccurrenceLinkVariable(pLoopData.meType));
                                        }
                                        int iTerrainRevealChange = occurrence.maiTerrainRevealChange[pTile.getTerrain()];
                                        if (iTerrainRevealChange > 0)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_VISION_REDUCED_BY", buildOccurrenceLinkVariable(pLoopData.meType), true);
                                        }
                                        if (builder.HasContent)
                                        {
                                            outTileData.AddKeyValueTileData(TextManager, buildTerrainLinkVariable(pTile.getTerrain()), builder.ToTextVariable());
                                        }
                                    }
                                    using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        if (pLoopData.isAffectedTile(pTile.getID()))
                                        {
                                            int iTileUnitDamage = occurrence.miTileUnitDamage;
                                            if (iTileUnitDamage != 0)
                                            {
                                                builder.AddTEXT("TEXT_HELPTEXT_DAMAGE_UNITS_FROM", buildSignedTextVariable(-iTileUnitDamage, bColor: true), buildTurnScaleName(pGame), false);
                                            }
                                            int iTileMovementCostExtra = -(occurrence.miTileMovementCostExtra / infos().Globals.MOVEMENT_MULTIPLER);
                                            if (iTileMovementCostExtra != 0)
                                            {
                                                builder.AddTEXT("TEXT_HELPTEXT_MOVEMENT_FROM", buildSignedTextVariable(iTileMovementCostExtra, bColor: true), false);
                                            }
                                            int iTileRevealChange = occurrence.miTileRevealChange;
                                            if (iTileRevealChange > 0)
                                            {
                                                builder.AddTEXT("TEXT_HELPTEXT_VISION_REDUCED_BY", buildOccurrenceLinkVariable(pLoopData.meType), false);
                                            }
                                            int iTileBaseYieldModifier = occurrence.miTileBaseYieldModifier;
                                            if (iTileBaseYieldModifier != 0)
                                            {
                                                builder.AddTEXT("TEXT_HELPTEXT_YIELD_MODIFIER", buildSignedTextVariable(iTileBaseYieldModifier, true, bColor: true));
                                            }
                                        }
                                        int iTileUnitDamageAdjacent = occurrence.miTileUnitDamageAdjacent;
                                        if (iTileUnitDamageAdjacent != 0 && !pLoopData.isAffectedTile(pTile.getID()))
                                        {
                                            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                            {
                                                Tile pAdjacentTile = pTile.tileAdjacent(eLoopDirection);
                                                if (pAdjacentTile != null && pLoopData.isAffectedTile(pAdjacentTile.getID()))
                                                {
                                                    builder.AddTEXT("TEXT_HELPTEXT_DAMAGE_UNITS_FROM", buildSignedTextVariable(-iTileUnitDamageAdjacent, bColor: true), buildTurnScaleName(pGame), false);
                                                    break;
                                                }
                                            }
                                        }
                                        int iTileBaseYieldModifierAdjacent = occurrence.miTileBaseYieldModifierAdjacent;
                                        if (iTileBaseYieldModifierAdjacent != 0 && !pLoopData.isAffectedTile(pTile.getID()))
                                        {
                                            for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                            {
                                                Tile pAdjacentTile = pTile.tileAdjacent(eLoopDirection);
                                                if (pAdjacentTile != null && pLoopData.isAffectedTile(pAdjacentTile.getID()))
                                                {
                                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_MODIFIER", buildSignedTextVariable(iTileBaseYieldModifierAdjacent, true, bColor: true));
                                                    break;
                                                }
                                            }
                                        }
                                        if (builder.HasContent)
                                        {
                                            outTileData.AddKeyValueTileData(TextManager, buildOccurrenceLinkVariable(pLoopData.meType), builder.ToTextVariable());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (bMouseoverVisible)
                {
                    if (pTile.isSettlement())
                    {
                        for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                        {
                            EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                int iValue = infos().effectUnit(eEffectUnit).miSettlementAttackModifier;
                                if (iValue != 0)
                                {
                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_INTO_EFFECT_UNIT", buildUnitTraitLinkVariable(eLoopUnitTrait)), buildAttackValueLinkVariable(iValue, true));
                                }
                            }
                        }
                    }

                    if (eImprovement != ImprovementType.NONE)
                    {
                        for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                        {
                            EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                int iValue = infos().effectUnit(eEffectUnit).maiImprovementToModifier[eImprovement];
                                if (iValue != 0)
                                {
                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_INTO_EFFECT_UNIT", buildUnitTraitLinkVariable(eLoopUnitTrait)), buildAttackValueLinkVariable(iValue, true));
                                }
                            }
                        }
                    }

                    if (pTile.onTradeNetworkCapital(pActivePlayer.getPlayer()))
                    {
                        for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                        {
                            EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                int iValue = infos().effectUnit(eEffectUnit).miConnectedAttackModifier;
                                if (iValue != 0)
                                {
                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_INTO_EFFECT_UNIT", buildUnitTraitLinkVariable(eLoopUnitTrait)), buildAttackValueLinkVariable(iValue, true));
                                }
                            }
                        }

                        for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                        {
                            EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                int iValue = infos().effectUnit(eEffectUnit).miConnectedDefenseModifier;
                                if (iValue != 0)
                                {
                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_FROM_EFFECT_UNIT", buildUnitTraitLinkVariable(eLoopUnitTrait)), buildDefenseValueLinkVariable(iValue, true));
                                }
                            }
                        }
                    }

                    if (pTile.isUrban())
                    {
                        for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                        {
                            EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                int iValue = infos().effectUnit(eEffectUnit).miUrbanAttackModifier;
                                if (iValue != 0)
                                {
                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_INTO_EFFECT_UNIT", buildUnitTraitLinkVariable(eLoopUnitTrait)), buildAttackValueLinkVariable(iValue, true));
                                }
                            }
                        }

                        for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                        {
                            EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                int iValue = infos().effectUnit(eEffectUnit).miUrbanDefenseModifier;
                                if (iValue != 0)
                                {
                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_FROM_EFFECT_UNIT", buildUnitTraitLinkVariable(eLoopUnitTrait)), buildDefenseValueLinkVariable(iValue, true));
                                }
                            }
                        }
                    }
                    else if (pTile.hasVegetation())
                    {
                        for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                        {
                            EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                int iValue = pTile.vegetation().maiDefendEffectUnit[eEffectUnit];
                                if (iValue != 0)
                                {
                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_INTO_EFFECT_UNIT", buildUnitTraitLinkVariable(eLoopUnitTrait)), buildAttackValueLinkVariable(-iValue, true));
                                }
                            }
                        }
                    }
                    else if (eImprovement == ImprovementType.NONE)
                    {
                        for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                        {
                            EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                int iValue = infos().terrain(eTerrain).maiDefendMeleeEffectUnit[eEffectUnit];
                                if (iValue != 0)
                                {
                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_MELEE_INTO_EFFECT_UNIT", buildUnitTraitLinkVariable(eLoopUnitTrait)), buildAttackValueLinkVariable(-iValue, true));
                                }
                            }
                        }

                        for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                        {
                            EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                            if (eEffectUnit != EffectUnitType.NONE)
                            {
                                int iValue = infos().height(eHeight).maiDefendMeleeEffectUnit[eEffectUnit];
                                if (iValue != 0)
                                {
                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_MELEE_INTO_EFFECT_UNIT", buildUnitTraitLinkVariable(eLoopUnitTrait)), buildAttackValueLinkVariable(-iValue, true));
                                }
                            }
                        }
                    }
                }

                if (pTile.hasResource())
                {
                    using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                    {
                        builder.Add(buildResourceLinkVariable(pTile.getResource(), pTile));

                        if (bInfoVisible)
                        {
                            if ((pCityTerritory != null) && (eImprovement == ImprovementType.NONE))
                            {
                                using (TextBuilder yieldBuilder = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    using (yieldBuilder.BeginScope(TextBuilder.ScopeType.COMMA))
                                    {
                                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                        {
                                            int iOutput = infos().resource(pTile.getResource()).maiYieldNoImprovement[eLoopYield];
                                            if (iOutput != 0)
                                            {
                                                yieldBuilder.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                            }
                                        }
                                    }

                                    if (yieldBuilder.HasContent)
                                    {
                                        builder.Add(yieldBuilder.ToTextVariable());
                                    }
                                }
                            }
                        }

                        outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_RESOURCE"), builder.ToTextVariable());
                    }
                }

                if (pCityTerritory != null)
                {
                    foreach (EffectCityType eLoopEffectCity in pCityTerritory.getActiveEffectCity())
                    {
                        InfoEffectCity effectCity = infos().effectCity(eLoopEffectCity);
                        for (TerrainType eLoopTerrain = 0; eLoopTerrain < infos().terrainsNum(); eLoopTerrain++)
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iYield = effectCity.maaiTerrainYield[eLoopTerrain, eLoopYield];
                                if (iYield != 0 && pTile.getTerrain() == eLoopTerrain)
                                {
                                    using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        builder.Add(buildTerrainLinkVariable(pTile.getTerrain(), pTile));

                                        if (bInfoVisible)
                                        {
                                            using (TextBuilder yieldBuilder = TextBuilder.GetTextBuilder(TextManager))
                                            {
                                                using (yieldBuilder.BeginScope(TextBuilder.ScopeType.COMMA))
                                                {
                                                    yieldBuilder.Add(buildYieldValueIconLinkVariable(eLoopYield, iYield, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                                }

                                                if (yieldBuilder.HasContent)
                                                {
                                                    builder.Add(yieldBuilder.ToTextVariable());
                                                }
                                            }
                                        }

                                        outTileData.AddKeyValueTileData(TextManager, buildEffectCityLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory.governor()), builder.ToTextVariable());
                                    }
                                }
                            }
                        }
                    }
                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_OWNER"), buildCityLinkVariable(pCityTerritory, pActivePlayer));
                }

                using (new UnityProfileScope("HelpText.buildTileTooltip.Pings"))
                {
                    buildTileTooltipPingText(outTileData.PingTextColorList, pGame, pActivePlayer, pTile, pManager);
                }

                if ((eImprovement != ImprovementType.NONE) ? infos().improvement(eImprovement).mbTribe : false)
                {
                    for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); eLoopReligion++)
                    {
                        if (pTile.isReligion(eLoopReligion))
                        {
                            outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_RELIGION"), buildReligionLinkVariable(eLoopReligion, pGame, pActivePlayer));
                        }
                    }
                }

                using (new UnityProfileScope("HelpText.buildTileTooltip.Improvements"))
                {
                    if (eImprovement != ImprovementType.NONE)
                    {
                        using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (builder.BeginScope("TEXT_HELPTEXT_CONCAT_SPACE_TWO"))
                            {
                                TribeType eTribe = pTile.getTribeSettlementOrRuins();
                                if (eTribe != TribeType.NONE)
                                {
                                    builder.AddTEXT("TEXT_TRIBE_ADJECTIVE", buildTribeLinkVariable(eTribe, pGame));
                                }

                                builder.Add(buildImprovementLinkVariable(eImprovement, pGame, pTile, true));
                            }

                            if (bInfoVisible)
                            {
                                buildTileEffectsLink(builder, pManager, pTile, eImprovement, SpecialistType.NONE, true);

                                using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_ADJACENT_BONUS")))
                                {
                                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                    {
                                        int iOutput = pTile.yieldModifiedAdjacentAll(eImprovement, eLoopYield);
                                        if (iOutput != 0)
                                        {
                                            builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                        }
                                    }
                                }
                            }

                            outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT"), builder.ToTextVariable());
                        }

                        if (bInfoVisible)
                        {
                            SpecialistType eSpecialist = pTile.getSpecialist();

                            if (eSpecialist != SpecialistType.NONE)
                            {
                                using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    builder.Add(buildSpecialistLinkVariable(eSpecialist, pGame, pTile));
                                    builder.Add(buildSpecialistYieldVariable(eImprovement, eSpecialist, pTile, pGame, true));

                                    if (pTile != null)
                                    {
                                        if (pTile.hasResource())
                                        {
                                            EffectCityType eEffectCity = infos().specialistClass(infos().specialist(eSpecialist).meClass).maeResourceCityEffect[pTile.getResource()];

                                            if (eEffectCity != EffectCityType.NONE)
                                            {
                                                if (infos().effectCity(eEffectCity).mbLuxury)
                                                {
                                                    builder.Add(buildLuxuryLinkVariable(pTile.getResource()));
                                                }
                                            }
                                        }
                                    }

                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_SPECIALIST"), builder.ToTextVariable());
                                }
                            }
                        }

                        if (pTile.canDevelopImprovement(eImprovement))
                        {
                            ImprovementType eDevelopImprovement = pGame.getDevelopImprovement(eImprovement);

                            if (eDevelopImprovement != ImprovementType.NONE)
                            {
                                TextVariable tribeText = TEXTVAR(false);

                                if (infos().improvement(eDevelopImprovement).mbTribe)
                                {
                                    TribeType eTribe = pTile.getTribeSite();

                                    if (eTribe != TribeType.NONE)
                                    {
                                        tribeText = buildTribeLinkVariable(eTribe, pGame);
                                    }
                                }

                                using (remainingText.BeginScope(TextBuilder.ScopeType.INDENTED_BULLET))
                                {
                                    int iTurns = ((bMouseoverVisible) ? (pTile.improvement().miDevelopTurns - pTile.getImprovementDevelopTurns()) : -1);

                                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_DEVELOP", tribeText, buildImprovementLinkVariable(eDevelopImprovement, pGame, pTile)), buildTurnsTextVariable(iTurns, pGame));
                                }
                            }
                        }

                        if (pTile.getTradeOutpostTurnsLeft(PlayerType.NONE) > 0)
                        {
                            for (PlayerType eLoopPlayer = 0; eLoopPlayer < pGame.getNumPlayers(); eLoopPlayer++)
                            {
                                int iLoopTurns = pTile.getTradeOutpostTurnsLeft(eLoopPlayer);
                                if (iLoopTurns > 0)
                                {
                                    using (remainingText.BeginScope(TextBuilder.ScopeType.INDENTED_BULLET))
                                    {
                                        int iTurns = bMouseoverVisible ? iLoopTurns : -1;

                                        outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_TRADE_OUTPOST_DEVELOP", buildPlayerLinkVariable(pGame.player(eLoopPlayer))), buildTurnsTextVariable(iTurns, pGame));
                                        outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_TRADE_OUTPOST_INCOME"), buildMoneyTextVariable(pTile.getTradeOutpostIncome()));
                                    }
                                }
                            }
                        }

                        if (bMouseoverVisible)
                        {
                            if (pTile.isImprovementUnfinished())
                            {
                                using (remainingText.BeginScope(TextBuilder.ScopeType.INDENT))
                                {
                                    int iBuildTurns = pTile.getImprovementBuildTurnsLeft();

                                    int iTurnsLeft = pTile.calculateUnitImprovementBuildTurns();
                                    if (iTurnsLeft > 0)
                                    {
                                        outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_WORK_COMPLETED"), TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_WORK_COMPLETED_VALUE", (pTile.getImprovementBuildTurnsOriginal() - pTile.getImprovementBuildTurnsLeft()), pTile.getImprovementBuildTurnsOriginal(), pGame.turnScaleNameShort(iTurnsLeft)));
                                    }
                                    else
                                    {
                                        using (buildWarningTextScope(remainingText))
                                        {
                                            outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_DESTROYED"), TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_DISAPPEARS", buildTurnsTextVariable(pTile.getImprovementPillageTurns(), pGame)));
                                        }
                                    }
                                }
                            }
                        }

                        if (bMouseoverVisible)
                        {
                            if (pTile.getImprovementUnitTurns() > 0)
                            {
                                using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    builder.Add(buildTurnsTextVariable(pTile.getImprovementUnitTurns(), pGame));

                                    if (pTile.skipImprovementUnitTurns(eActiveTeam))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_TILE_TOOLTIP_PAUSED", skipSeparator: true);
                                    }

                                    outTileData.AddKeyValueTileData(TextManager, buildNextUnitLinkVariable(pTile.getImprovementUnitTurns()), builder.ToTextVariable());
                                }
                            }
                        }
                    }

                    if (bInfoVisible)
                    {
                        if ((eImprovement == ImprovementType.NONE) || bExpandTooltip)
                        {
/*####### Better Old World AI - Base DLL #######
  ### No BestImprovement               START ###
  ### because no cached values (no idea why) ###
  ##############################################*/
                            //ImprovementType eBestImprovement = ImprovementType.NONE;
                            //    pActivePlayer.AI.getBestImprovement(pTile, pCityTerritory, ref eBestImprovement);

                            //removing BestImprovement

                            bool bCanExpandTooltip = false;

                            for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                            {
                                if (infos().Helpers.isImprovementResourceValid(eLoopImprovement, eResource))
                                {
                                    using (TextBuilder improvementBuilder = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        if (pActivePlayer.canStartImprovementOnTile(pTile, eLoopImprovement, bTestEnabled: true, bTestTech: false, bTestAdjacent: true, bTestReligion: true))
                                        {
                                            buildTileEffectsLink(improvementBuilder, pManager, pTile, eLoopImprovement, SpecialistType.NONE, true);

                                            using (improvementBuilder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_START_ADJACENT")))
                                            {
                                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                                {
                                                    int iOutput = pTile.yieldModifiedAdjacentAll(eLoopImprovement, eLoopYield);
                                                    if (iOutput != 0)
                                                    {
                                                        improvementBuilder.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                                    }
                                                }
                                            }

                                        }
                                        else if (pActivePlayer.canStartImprovement(eLoopImprovement, pTile, bTestTech: false))
                                        {
                                            buildTileEffectsLink(improvementBuilder, pManager, pTile, eLoopImprovement, SpecialistType.NONE, false);
                                        }


                                        if (improvementBuilder.HasContent)
                                        {
                                            if ((pTile.getTeam() == pActivePlayer.getTeam()))
                                            {
                                                using (TextBuilder linkBuilder = TextBuilder.GetTextBuilder(TextManager))
                                                {
                                                    using (linkBuilder.BeginScope("TEXT_HELPTEXT_CONCAT_SPACE_TWO"))
                                                    {
                                                        linkBuilder.Add(buildResourceIconLinkVariable(eResource, pTile));
                                                        linkBuilder.Add(buildImprovementLinkVariable(eLoopImprovement, pGame, pTile));
                                                    }


                                                    if (pActivePlayer.isImprovementUnlocked(eLoopImprovement))
                                                    {
                                                        linkBuilder.AddTEXT("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_START_RECOMMENDED");
                                                    }
                                                    else
                                                    {
                                                        using (buildWarningTextScope(linkBuilder))
                                                        {
                                                            linkBuilder.AddTEXT("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_START_REQUIRES", buildTechLinkVariable(infos().improvementClass(infos().improvement(eLoopImprovement).meClass).meTechPrereq));
                                                        }
                                                    }


                                                    outTileData.AddKeyValueTileData(TextManager, linkBuilder.ToTextVariable(), improvementBuilder.ToTextVariable());
                                                }
                                            }
                                            else
                                            {
                                                bCanExpandTooltip = true;
                                                break;
                                            }
                                        }



                                    }
                                }
                            }


                            //    for (int iPass = 0; iPass < 2; iPass++)
                            //    {
                            //        for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                            //        {
                            //            bool bImprovementResource = infos().Helpers.isImprovementResourceValid(eLoopImprovement, eResource);

                            //            if ((iPass == 0) == (bImprovementResource || (eLoopImprovement == eBestImprovement)))
                            //            {
                            //                if (!(infos().improvement(eLoopImprovement).mbWonder) || (eLoopImprovement == eBestImprovement))
                            //                {
                            //                    using (TextBuilder improvementBuilder = TextBuilder.GetTextBuilder(TextManager))
                            //                    {
                            //                        if (pActivePlayer.canStartImprovementOnTile(pTile, eLoopImprovement, bTestEnabled: true, bTestTech: !bImprovementResource, bTestAdjacent: true, bTestReligion: true))
                            //                        {
                            //                            buildTileEffectsLink(improvementBuilder, pManager, pTile, eLoopImprovement, SpecialistType.NONE, true);

                            //                            using (improvementBuilder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_START_ADJACENT")))
                            //                            {
                            //                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            //                                {
                            //                                    int iOutput = pTile.yieldModifiedAdjacentAll(eLoopImprovement, eLoopYield);
                            //                                    if (iOutput != 0)
                            //                                    {
                            //                                        improvementBuilder.Add(buildYieldValueIconLinkVariable(eLoopYield, iOutput, iMultiplier: Constants.YIELDS_MULTIPLIER));
                            //                                    }
                            //                                }
                            //                            }
                            //                        }
                            //                        else if (bImprovementResource && pActivePlayer.canStartImprovement(eLoopImprovement, pTile, bTestTech: false))
                            //                        {
                            //                            buildTileEffectsLink(improvementBuilder, pManager, pTile, eLoopImprovement, SpecialistType.NONE, false);
                            //                        }

                            //                        if (improvementBuilder.HasContent)
                            //                        {
                            //                            if (((iPass == 0) && (pTile.getTeam() == pActivePlayer.getTeam())) || bExpandTooltip)
                            //                            {
                            //                                using (TextBuilder linkBuilder = TextBuilder.GetTextBuilder(TextManager))
                            //                                {
                            //                                    using (linkBuilder.BeginScope("TEXT_HELPTEXT_CONCAT_SPACE_TWO"))
                            //                                    {
                            //                                        if (bImprovementResource)
                            //                                        {
                            //                                            linkBuilder.Add(buildResourceIconLinkVariable(eResource, pTile));
                            //                                        }
                            //                                        linkBuilder.Add(buildImprovementLinkVariable(eLoopImprovement, pGame, pTile));
                            //                                    }

                            //                                    if (iPass == 0)
                            //                                    {
                            //                                        if (pActivePlayer.isImprovementUnlocked(eLoopImprovement))
                            //                                        {
                            //                                            if (!bExpandTooltip)
                            //                                            {
                            //                                                linkBuilder.AddTEXT("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_START_RECOMMENDED");
                            //                                            }
                            //                                        }
                            //                                        else
                            //                                        {
                            //                                            using (buildWarningTextScope(linkBuilder))
                            //                                            {
                            //                                                linkBuilder.AddTEXT("TEXT_HELPTEXT_TILE_TOOLTIP_IMPROVEMENT_START_REQUIRES", buildTechLinkVariable(infos().improvementClass(infos().improvement(eLoopImprovement).meClass).meTechPrereq));
                            //                                            }
                            //                                        }
                            //                                    }

                            //                                    outTileData.AddKeyValueTileData(TextManager, linkBuilder.ToTextVariable(), improvementBuilder.ToTextVariable());
                            //                                }
                            //                            }
                            //                            else
                            //                            {
                            //                                bCanExpandTooltip = true;
                            //                                break;
                            //                            }
                            //                        }
                            //                    }
                            //                }
                            //            }
                            //        }

                            //        if (bCanExpandTooltip)
                            //        {
                            //            break;
                            //        }
                            //    }
/*####### Better Old World AI - Base DLL #######
  ### No BestImprovement                 END ###
  ### because no cached values (no idea why) ###
  ##############################################*/


                            if (bCanExpandTooltip)
                            {
                                buildTextHotkeyVariableOrFalse(remainingText, "TEXT_HELPTEXT_TOOLTIP_EXPAND_TOOLTIP", pManager.Interfaces.Hotkeys, mInfos.Globals.HOTKEY_EXPAND_TOOLTIP);
                            }
                        }
                    }
                }

                if (pTile.canHarvestTile())
                {
                    TextVariable harvestResourceVariable = buildResourceHarvestHelpVariable(pActivePlayer, pTile, pActivePlayer);

                    if (!harvestResourceVariable.IsNullOrEmpty())
                    {
                        outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_HARVEST"), harvestResourceVariable);
                    }
                }

                if (pTile.hasVegetation())
                {
                    TextVariable removeVegetationText = buildYieldRemoveText(pTile, pActivePlayer, false);

                    if (!removeVegetationText.IsNullOrEmpty())
                    {
                        outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_REMOVE_VEGETATION", TEXTVAR_TYPE(pTile.vegetation().meName)), removeVegetationText);
                    }
                }

                if (pTile.hasImprovement())
                {
                    for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); ++eLoopUnit)
                    {
                        if (infos().unit(eLoopUnit).maaiImprovementYieldRate.Count > 0)
                        {
                            using (TextBuilder unitYieldBuilder = TextBuilder.GetTextBuilder(TextManager))
                            {
                                buildYieldUnitText(unitYieldBuilder, pTile, eLoopUnit, pActivePlayer);
                                if (unitYieldBuilder.HasContent)
                                {
                                    outTileData.AddKeyValueTileData(TextManager, buildUnitNameVariable(eLoopUnit, pGame), unitYieldBuilder.ToTextVariable());
                                }
                            }
                        }
                    }
                }

                if (pTile.isValidFoundLocation(pActivePlayer.getPlayer(), eActiveTeam))
                {
                    int iCost = pActivePlayer.getFoundCityCost(pTile);
                    if (iCost != 0)
                    {
                        outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_FOUND_COST"), buildYieldValueIconLinkVariable(infos().Globals.MONEY_YIELD, iCost));
                    }
                }

                if (bExpandTooltip)
                {
                    outTileData.AddKeyValueTileData(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_X_Y"), buildCommaSpaceSeparator(pTile.getX(), pTile.getY()));
                }

                buildTextHotkeyVariableOrFalse(remainingText, "TEXT_HELPTEXT_TOOLTIP_FREEZE_TOOLTIP", pManager.Interfaces.Hotkeys, mInfos.Globals.HOTKEY_FREEZE_TOOLTIP);
            }
        }

        //lines 19118-19200
        public override TextBuilder buildTileDebugText(TextBuilder builder, Tile pTile, ClientManager pManager)
        {
            using (new UnityProfileScope("buildTileDebugText"))
            {
                TeamType eActiveTeam = pManager.getActiveTeam();

                using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                {
                    for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                    {
                        if (pTile.getTradeNetwork(eActiveTeam, eLoopDirection) != -1)
                        {
                            builder.Add(buildColonSpaceOne(TEXTVAR(eLoopDirection.ToStringCached()), pTile.getTradeNetwork(eActiveTeam, eLoopDirection)));
                        }
                    }
                }

                using (builder.BeginScope("TEXT_HELPTEXT_CONCAT_SPACE_TWO"))
                {
                    builder.Add(pTile.ToTextVariable());
                    builder.Add(QUICKTEXTVAR("A{0}", pTile.getArea()));
                    builder.Add(QUICKTEXTVAR("S{0}", pTile.getLandSection()));
                    builder.Add(QUICKTEXTVAR("L{0}", pTile.getLatitude()));
                    builder.Add(QUICKTEXTVAR("H{0}", (int)pTile.getHeight()));

                    if (pTile.getTerrainStamp() != TerrainStampType.NONE)
                    {
                        builder.Add(QUICKTEXTVAR("S{0}", (int)pTile.getTerrainStamp()));
                    }

                    if (pTile.isBoundary())
                    {
                        builder.Add(QUICKTEXTVAR("BND"));
                    }
                    builder.Add("*", skipSeparator: true);
                }

                if (pTile.getNumTags() > 0)
                {
                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TILE_TOOLTIP_DEBUG_TAGS")))
                    {
                        for (int i = 0; i < pTile.getNumTags(); ++i)
                        {
                            builder.Add(QUICKTEXTVAR(infos().tileTag(pTile.getTagAt(i)).mzType));
                        }
                    }
                    builder.Add("*", skipSeparator: true);
                }

                if (pTile.hasMapGeneratorData())
                {
                    builder.Add(QUICKTEXTVAR("GT:{0}", QUICKTEXTVAR(pTile.getMapGeneratorData())));
                }

/*####### Better Old World AI - Base DLL #######
  ### No BestImprovement               START ###
  ### because no cached values (no idea why) ###
  ##############################################*/
                //ImprovementType eBestImprovement = ImprovementType.NONE;
                ////if (!((BetterAIPlayer.BetterAIPlayerAI)pManager.activePlayer().AI).BhumanCachingDone)
                ////{
                ////    ((BetterAIPlayer.BetterAIPlayerAI)pManager.activePlayer().AI).cacheImprovementValuesHuman();
                ////}
                //pManager.activePlayer().AI.getBestImprovement(pTile, pTile.cityTerritory(), ref eBestImprovement);
                //if (eBestImprovement != ImprovementType.NONE)
                //{
                //    long iAIValue = pManager.activePlayer().AI.improvementValueTile(eBestImprovement, pTile, pTile.cityTerritory(), true, true);
                //    builder.Add(QUICKTEXTVAR(TEXT("TEXT_HELPTEXT_AI_VALUE") + ": {0} ({1})*", iAIValue, TEXTVAR_TYPE(mInfos.improvement(eBestImprovement).mName)));
                //}
/*####### Better Old World AI - Base DLL #######
  ### No BestImprovement                 END ###
  ### because no cached values (no idea why) ###
  ##############################################*/

                if (pManager.Selection.isSelectedUnit())
                {
                    Unit pUnit = pManager.Selection.getSelectedUnit();
                    if (pUnit != null && pUnit.getPlayer() == pManager.getActivePlayer())
                    {
                        builder.Add(QUICKTEXTVAR(TEXT("TEXT_HELPTEXT_AI_VALUE") + ": {0} ({1})*", pUnit.AI.exploreValue(pTile), TEXTVAR("Explore")));
                    }
                }

                if (pTile.isValidFoundLocation(pManager.getActivePlayer(), TeamType.NONE, false))
                {
                    FamilyType eBestFamily = pManager.activePlayer().AI.getBestFoundFamily(pTile);
                    if (eBestFamily != FamilyType.NONE)
                    {
                        builder.Add(QUICKTEXTVAR(TEXT("TEXT_HELPTEXT_BEST_FAMILY") + ": {0}", TEXTVAR_TYPE(mInfos.family(eBestFamily).meName)));
                    }
                }

                return builder;
            }
        }




        //500 lines of copy-paste START
        //lines 19806-20253
        public override TextBuilder buildImprovementBreakdown(TextBuilder builder, ImprovementType eImprovement, SpecialistType eSpecialist, Tile pTile, ClientManager pManager)
        {
            Game pGame = pManager.GameClient;
            Player pActivePlayer = pManager.activePlayer();

            ImprovementClassType eImprovementClass = infos().improvement(eImprovement).meClass;

            ResourceType eResource = pTile.getResource();
            City pCityTerritory = pTile.cityTerritory();

            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
            {
                int iTotalOutput = pTile.yieldOutputModified(eImprovement, eSpecialist, eLoopYield, pTile.cityTerritory());
                if (iTotalOutput != 0)
                {
                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_PER_YEAR", buildYieldValueIconLinkVariable(eLoopYield, iTotalOutput, iMultiplier: Constants.YIELDS_MULTIPLIER), buildTurnScaleName(pGame));

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

                    for (int i = 0; i < pGame.getNumOccurrences(); ++i)
                    {
                        OccurrenceData pLoopData = pGame.getOccurrenceDataAt(i);
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
                            iValue = occurrence.miTileBaseYieldModifier;
                            if (iValue != 0 && pLoopData.isAffectedTile(pTile.getID()))
                            {
                                using (buildSecondaryTextScope(builder))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true, bColor: true), buildOccurrenceLinkVariable(pLoopData.meType, pLoopData));
                                }
                            }
                            iValue = occurrence.miTileBaseYieldModifierAdjacent;
                            if (iValue != 0)
                            {
                                for (DirectionType eLoopDirection = 0; eLoopDirection < DirectionType.NUM_TYPES; eLoopDirection++)
                                {
                                    Tile pAdjacentTile = pTile.tileAdjacent(eLoopDirection);
                                    if (pAdjacentTile != null && pLoopData.isAffectedTile(pAdjacentTile.getID()))
                                    {
                                        using (buildSecondaryTextScope(builder))
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_YIELD_FROM", buildSignedTextVariable(iValue, true, bColor: true), buildOccurrenceLinkVariable(pLoopData.meType, pLoopData));
                                        }
                                    }
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
                        buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvement(eImprovement).meEffectCity, false, eLoopYield, pCityTerritory, pManager, iTotalOutput);

                        if (eImprovementClass != ImprovementClassType.NONE)
                        {
                            buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvementClass(eImprovementClass).meEffectCity, false, eLoopYield, pCityTerritory, pManager, iTotalOutput);

                            if (eResource != ResourceType.NONE)
                            {
                                buildImprovementBreakdownEffectCityHelp(builder, eImprovement, infos().improvementClass(eImprovementClass).maeResourceCityEffect[eResource], true, eLoopYield, pCityTerritory, pManager);
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
        //copy-paste END

        //1k lines of copy-paste START
        //lines 20296-21274
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
                            buildEffectCityHelpYields(builder, eEffectCity, pGame, null, pActivePlayer, 1);
                        }

                        foreach (EffectCityType eEffectCity in effectListScoped.Value)
                        {
                            buildEffectCityHelpNoYields(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), pActivePlayer);
                        }

                        if (pGame?.isCharacters() ?? true)
                        {
                            int iValue = infos().improvement(eImprovement).miLegitimacy;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_HELP_LEGITIMACY", buildSignedTextVariable(iValue));
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
                                                        buildEffectCityHelp(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), true, pActivePlayer, bSkipImpossible: !bEncyclopedia);
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

                            if (infos().improvement(eImprovement).mbRemovePillage)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_REMOVE_PILLAGE");
                            }

                            if (bEncyclopedia && infos().improvement(eImprovement).mbNoAdjacentReligion)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_NO_ADJACENT_RELIGION");
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

                                if (eSpecialist != SpecialistType.NONE)
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
                                                    buildEffectCityHelp(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), true, pActivePlayer, bSkipImpossible: !bEncyclopedia);
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
                                    ImprovementType eSourceImprovement = infos().effectCity(eLoopEffectCity).meSourceImprovement;
                                    if (eSourceImprovement != ImprovementType.NONE && !infos().modSettings().App.CheckContentOwnership(infos().improvement(eSourceImprovement).meGameContentRequired, infos(), null))
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
                                        if (infos().improvementClass(eImprovementClass).maaiTheologyYieldOutput.Count > 0)
                                        {
                                            CommaListVariableGenerator yieldsList = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, TextManager);

                                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                            {
                                                int iValue = infos().improvementClass(eImprovementClass).maaiTheologyYieldOutput[eLoopTheology, eLoopYield];
                                                if (iValue != 0)
                                                {
                                                    yieldsList.AddItem(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
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


        //1k lines of copy-paste END
        //lines 21309-21715
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
                                        andList2.AddItem(buildEffectCitySourceLinkVariable(eInfoImprovement.meSecondaryUnlockEffectCityPrereq, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer));
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
                                        andList3.AddItem(buildEffectCitySourceLinkVariable(eInfoImprovement.meTertiaryUnlockEffectCityPrereq, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer));
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
                            if (eInfoImprovement.mabVegetationValid[eLoopVegetation])
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

                        for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < mInfos.effectCitiesNum(); eLoopEffectCity++)
                        {
                            foreach ((TerrainType, ImprovementType) pLoopPair in infos().effectCity(eLoopEffectCity).mlpTerrainImprovementValid)
                            {
                                if (pLoopPair.Item2 == eImprovement)
                                {
                                    orList.AddItem(buildSlashText(buildTerrainLinkVariable(pLoopPair.Item1, pTile), buildEffectCityLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory?.governor())));
                                }
                            }
                        }

                        if (orList.Count > 0)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", orList.Finalize()), ((pTile != null) && !(pTile.isImprovementValid(eImprovement, pCityTerritory)))));
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
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_HOLY_CITY", buildReligionLinkVariable(eReligionPrereq, pGame, pActivePlayer)), ((pCityTerritory != null) && !(pCityTerritory.isReligionHolyCity(eReligionPrereq)))));
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
                        FamilyType eFamilyPrereq = infos().improvement(eImprovement).meFamilyPrereq;

                        if (eFamilyPrereq != FamilyType.NONE)
                        {
                            if (pGame != null)
                            {
                                if (pGame.isCharacters())
                                {
                                    lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_FAMILY", buildFamilyLinkVariable(eFamilyPrereq, pGame), ((pCityTerritory != null) ? (pCityTerritory.getFamily() != eFamilyPrereq) : false))));
                                }
                            }
                            else
                            {
                                lRequirements.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_FAMILY", buildFamilyLinkVariable(eFamilyPrereq, pGame)));
                            }
                        }
                    }

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
                            //this is already fixed by giving classes an actual name instead of sometimes using the lvl1 improvement name for the entire improvement class
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_REQUIRES_ADJACENT_IMPROVEMENT", buildImprovementClassLinkVariable(eAdjacentImprovementClassPrereq)), ((pTile != null) ? !pTile.adjacentToCityImprovementClassFinished(eAdjacentImprovementClassPrereq) : false)));
                        }
                    }

                    {
                        EffectCityType eEffectCityPrereq = eInfoImprovement.meEffectCityPrereq;

                        if (eEffectCityPrereq != EffectCityType.NONE)
                        {
                            lRequirements.Add(buildWarningTextVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES", buildEffectCitySourceLinkVariable(eEffectCityPrereq, pCityTerritory, pCityTerritory?.governor(), pGame, pActivePlayer)), ((pCityTerritory != null) ? (pCityTerritory.getEffectCityCount(eEffectCityPrereq) == 0) : false)));
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
                        if (!infos().terrain(eLoopTerrain).mbWater) // don't clutter the help text with obvious requirements
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

                    if (infos().improvement(eImprovement).mbRequiresUrban)
                    {
                        TextVariable textvar = TEXTVAR_TYPE("TEXT_HELPTEXT_IMPROVEMENT_HELP_URBAN_BUILDING", buildUrbanLinkVariable());
                        if (pTile != null && !pTile.urbanEligible())
                        {
                            lRequirements.Add(buildWarningTextVariable(textvar));
                        }
                        else
                        {
                            lRequirements.Add(textvar);
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
                                            subText.Add(buildEffectCityLinkVariable(eLoopEffectCity, pCityTerritory, pCityTerritory.governor()));
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

        //copy-paste START
        //lines 21717-22072
        public override TextBuilder buildUnitTypeHelp(TextBuilder builder, UnitType eUnit, City pCity, Player pPlayer, TribeType eTribe, Game pGame, Player pActivePlayer, bool bName = true, bool bCosts = true, bool bStats = true, bool bDetails = true)
        {
            using (new UnityProfileScope("HelpText.buildUnitTypeHelp"))
            {
                if (bName)
                {
                    builder.Add(buildTitleVariable(buildUnitNameVariable(eUnit, pGame)));
                }

                buildUnitTraitsHelp(builder, eUnit);

                using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                {
                    if (bStats)
                    {
                        InfoUnit unit = infos().unit(eUnit);
                        int iRangeMin = unit.miRangeMin;
                        int iRangeMax = unit.miRangeMax;

                        using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                if (infos().unit(eUnit).miHPMax > 0)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_HP_MAX", TEXTVAR(unit.miHPMax));
                                }

                                if (infos().Helpers.canDamage(eUnit))
                                {
                                    builder.Add(buildUnitStrengthValueLinkVariable(unit.miStrength, false));
                                }
                                else
                                {
                                    builder.Add(buildDefenseValueLinkVariable(unit.miStrength, false));
                                }

                                builder.Add(buildMovementLinkVariable(unit.miMovement, bSigned: false));

                                if (!(infos().unit(eUnit).mbMelee))
                                {
                                    if (iRangeMax > 0)
                                    {
                                        if (iRangeMin == 0)
                                        {
                                            builder.Add(buildRangeLinkVariable(iRangeMax, false));
                                        }
                                        else if (iRangeMin > 0)
                                        {
                                            builder.Add(buildRangeMinMaxLinkVariable(iRangeMin, iRangeMax));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        int iValue = infos().unit(eUnit).miReveal;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_REVEAL_RANGE", buildSignedTextVariable(iValue));
                        }
                    }

                    {
                        int iValue = (infos().unit(eUnit).miFatigue - infos().Globals.UNIT_FATIGUE_LIMIT);
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_FATIGUE_LIMIT", buildSignedTextVariable(iValue));
                        }
                    }

                    {
                        ReligionType eReligion = infos().unit(eUnit).meRequiresReligion;

                        if (eReligion != ReligionType.NONE)
                        {
                            foreach (UnitTraitType eLoopUnitTrait in infos().unit(eUnit).maeUnitTrait)
                            {
                                if (infos().effectUnit(infos().unitTrait(eLoopUnitTrait).meEffectUnit).mbSpreadReligion)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_SPREADS_RELIGION", buildReligionLinkVariable(eReligion, pGame, null));
                                    break;
                                }
                            }
                        }
                    }

                    if (infos().unit(eUnit).mbAmphibious)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_AMPHIBIOUS");
                    }

                    if (infos().unit(eUnit).mbCaravan)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_CARAVAN_MISSION");
                    }

                    if (infos().unit(eUnit).mbUnlimber)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_MUST_UNLIMBER");
                    }

                    if (infos().unit(eUnit).mbFound)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_FOUND_CITY");
                    }

                    if (infos().unit(eUnit).mbHarvest)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_HARVEST_RESOURCE", buildHarvestResourceLinkVariable());
                    }

                    if (infos().unit(eUnit).mbBuild)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_BUILD_IMPROVEMENTS");
                    }

                    {
                        ReligionType eBuildReligion = infos().unit(eUnit).meBuildReligion;

                        if (eBuildReligion != ReligionType.NONE)
                        {
                            using (var improvementsScope = CollectionCache.GetListScoped<TextVariable>())
                            {
                                for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                                {
                                    if (infos().improvement(eLoopImprovement).meReligionPrereq == eBuildReligion)
                                    {
                                        improvementsScope.Value.Add(buildImprovementLinkVariable(eLoopImprovement, pGame));
                                    }
                                }

                                if (improvementsScope.Value.Count > 0)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_BUILD_RELIGION_IMPROVEMENT");

                                    using (builder.BeginScope(TextBuilder.ScopeType.INDENTED_BULLET))
                                    {
                                        foreach (TextVariable loopText in improvementsScope.Value)
                                        {
                                            builder.Add(loopText);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (bDetails)
                    {
                        if (infos().unit(eUnit).mbZOC)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_HAS_ZOC", buildZOCLinkVariable());
                        }

/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                  START ###
  ##############################################*/
                        if (infos().unit(eUnit).mbIgnoreZOC)
                        {
                            if (!((BetterAIInfoUnit)(infos().unit(eUnit))).bHasIngoreZOCBlocker)
                            {
                                builder.Add(buildIgnoreZOCLinkVariable());
                            }
                            else
                            {
                                builder.Add(buildIgnoreZOCWithExceptionsLinkVariable(eUnit));
                            }
                        }
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                    END ###
  ##############################################*/
                    }

                    if (infos().unit(eUnit).mbFortify)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_FORTIFY");
                    }

                    if (infos().unit(eUnit).mbTestudo)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_CAN_TESTUDO");
                    }

                    {
                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_STARTS_WITH")))
                        {
                            foreach (EffectUnitType eLoopEffectUnit in infos().unit(eUnit).maeEffectUnit)
                            {
                                builder.Add(buildEffectUnitLinkVariable(eLoopEffectUnit));
                            }

                            if (pCity != null)
                            {
                                if (infos().unit(eUnit).mbPromote)
                                {
                                    {
                                        int iValue = pCity.getBuildUnitLevels(eUnit);
                                        if (iValue > 0)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_START_LEVELS", buildSignedTextVariable(iValue));
                                        }
                                    }

                                    {
                                        int iValue = pCity.getBuildUnitXP(eUnit);
                                        if (iValue > 0)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_START_XP", buildSignedTextVariable(iValue));
                                        }
                                    }

                                    using (var hashSetScope = CollectionCache.GetHashSetScoped<PromotionType>())
                                    {
                                        HashSet<PromotionType> sePromotions = hashSetScope.Value;

                                        foreach (EffectCityType eLoopEffectCity in pCity.getActiveEffectCity())
                                        {
                                            foreach (PromotionType eLoopPromotion in infos().effectCity(eLoopEffectCity).maeFreePromotion)
                                            {
                                                if (pGame.isEffectUnitValid(eUnit, infos().promotion(eLoopPromotion).meEffectUnit))
                                                {
                                                    sePromotions.Add(eLoopPromotion);
                                                }
                                            }

                                            foreach (UnitTraitType eLoopUnitTrait in infos().unit(eUnit).maeUnitTrait)
                                            {
                                                PromotionType ePromotion = infos().effectCity(eLoopEffectCity).maeTraitPromotion[eLoopUnitTrait];

                                                if (ePromotion != PromotionType.NONE)
                                                {
                                                    sePromotions.Add(ePromotion);
                                                }
                                            }
                                        }

                                        foreach (PromotionType eLoopPromotion in sePromotions)
                                        {
                                            builder.Add(buildPromotionLinkVariable(eLoopPromotion));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (bDetails)
                    {
                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_CONSUMES")))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iValue = (infos().unit(eUnit).maiYieldConsumption[eLoopYield] * Constants.YIELDS_MULTIPLIER);
                                if (iValue != 0)
                                {
                                    builder.Add(buildYieldValueIconLinkVariable(eLoopYield, -(iValue), bRate: true, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                }
                            }
                        }

                        for (ImprovementType eLoopImprovement = 0; eLoopImprovement < infos().improvementsNum(); eLoopImprovement++)
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA_AND, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_IMPROVEMENT_YIELD", buildImprovementLinkVariable(eLoopImprovement, pGame), pGame?.turnScaleName() ?? TEXTVAR("y")))) //TODO: needs to be changed when mzScaleShort localization is added
                            {
                                for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                                {
                                    int iValue = infos().unit(eUnit).maaiImprovementYieldRate[eLoopImprovement, eLoopYield];
                                    if (iValue != 0)
                                    {
                                        builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, bRate: true, iMultiplier: Constants.YIELDS_MULTIPLIER));
                                    }
                                }
                            }
                        }
                    }

                    if (infos().Helpers.canDamage(eUnit) && !(infos().unit(eUnit).mbPromote))
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_NO_TRAIN_OR_PROMOTE");
                    }
                } // end BULLET

                if (bCosts)
                {
                    // population
                    {
                        int iCost = infos().unit(eUnit).miPopulationCost;
                        if (iCost > 0)
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
                            using (buildWarningTextScope(builder, ((pCity != null) ? (pCity.getCitizens() < iCost) : false)))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_POPULATION_COST", TEXTVAR(iCost), buildCitizenLinkVariable(null));
                            }
                        }
                    }

                    // yields
                    {
                        TextVariable production = null;

                        if (pPlayer != null)
                        {
                            using (var costsScoped = CollectionCache.GetDictionaryScoped<YieldType, int>())
                            {
                                pPlayer.getUnitYieldCost(eUnit, pCity, costsScoped.Value);
                                buildYieldCostText(builder, costsScoped.Value, ((pPlayer == pActivePlayer) ? pPlayer : null));
                            }

                            int iProduction = pPlayer.getUnitBuildCost(eUnit, pCity);
                            if (iProduction > 0)
                            {
                                production = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_PRODUCTION", TEXTVAR((pPlayer?.getUnitBuildCost(eUnit, pCity) ?? iProduction)), buildYieldIconLinkVariable(infos().unit(eUnit).meProductionType));
                            }
                        }
                        else
                        {
                            int iValue = infos().unit(eUnit).miProduction;
                            if (iValue > 0)
                            {
                                production = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_PRODUCTION", TEXTVAR(iValue), buildYieldIconLinkVariable(infos().unit(eUnit).meProductionType));
                            }
                        }
                        if (production != null)
                        {
                            //fixed in base game :)
                            int iModifier = pPlayer?.getUnitBuildCostModifier(eUnit, pCity) ?? 0;
                            TextVariable output = production;
                            if (infos().unit(eUnit).miProductionCity != 0)
                            {
                                TextVariable perCity = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_PRODUCTION_PER_CITY", TEXTVAR(infos().utils().modify(infos().unit(eUnit).miProductionCity, iModifier), NumberFormatOptions.SHOW_SIGN), buildYieldIconLinkVariable(infos().unit(eUnit).meProductionType));
                                output = buildCommaSpaceSeparator(output, perCity);
                            }
                            if (infos().unit(eUnit).miProductionPer != 0)
                            {
                                TextVariable perGlobal = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_PRODUCTION_PER_GLOBAL", TEXTVAR(infos().utils().modify(infos().unit(eUnit).miProductionPer, iModifier), NumberFormatOptions.SHOW_SIGN), buildYieldIconLinkVariable(infos().unit(eUnit).meProductionType));
                                output = buildCommaSpaceSeparator(output, perGlobal);
                            }
                            builder.Add(output);
                        }
                    }
                }

                {
                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA_OR, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_UPGRADES_FROM")))
                    {
                        for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                        {
                            if (pPlayer == null || pPlayer.canEverBuildUnit(eLoopUnit))
                            {
                                if (infos().unit(eLoopUnit).maeUpgradeUnit.Contains(eUnit))
                                {
                                    builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                                }
                            }
                            if (eTribe != TribeType.NONE)
                            {
                                if (infos().unit(eLoopUnit).maeTribeUpgradeUnit[eTribe] == eUnit)
                                {
                                    builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                                }
                            }
                            else
                            {
                                //old
                                //if (infos().unit(eLoopUnit).maeTribeUpgradeUnit.Contains(eUnit))
                                //{
                                //    builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                                //}

                                //new
                                //foreach (var p in infos().unit(eLoopUnit).maeTribeUpgradeUnit)
                                //{
                                //    if (p.Value == eUnit)
                                //    {
                                //        builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                                //        break;
                                //    }
                                //}

                                //using derivative info instead
                            }
                        }
                        if (eTribe == TribeType.NONE)
                        {
                            foreach (UnitType eLoopUnit in ((BetterAIInfoUnit)infos().unit(eUnit)).maeTribeUpgradesFromAccumulated)
                            {
                                builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                            }
                        }


                    }
                }

                {
                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA_OR, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_UPGRADES_TO")))
                    using (var unitTypesScoped = CollectionCache.GetHashSetScoped<UnitType>())
                    {
                        HashSet<UnitType> seUpgrades = unitTypesScoped.Value;

                        foreach (UnitType eLoopUnit in infos().unit(eUnit).maeUpgradeUnit)
                        {
                            if (pPlayer == null || pPlayer.canEverBuildUnit(eLoopUnit))
                            {
                                seUpgrades.Add(eLoopUnit);
                            }
                        }
                        if (eTribe != TribeType.NONE)
                        {
                            UnitType eUpgrade = infos().unit(eUnit).maeTribeUpgradeUnit[eTribe];
                            if (eUpgrade != UnitType.NONE)
                            {
                                seUpgrades.Add(eUpgrade);
                            }
                        }
                        else
                        {
                            //foreach (UnitType eLoopUnit in infos().unit(eUnit).maeTribeUpgradeUnit)
                            //{
                            //    if (eLoopUnit != UnitType.NONE)
                            //    {
                            //        seUpgrades.Add(eLoopUnit);
                            //    }
                            //}
                            
                            foreach (KeyValuePair<TribeType, UnitType> TribeUpgradeUnit in infos().unit(eUnit).maeTribeUpgradeUnit)
                            {
                                if (TribeUpgradeUnit.Value != UnitType.NONE)
                                {
                                    seUpgrades.Add(TribeUpgradeUnit.Value);
                                }
                            }

                        }
                        foreach (UnitType eLoopUnit in seUpgrades)
                        {
                            builder.Add(buildUnitTypeLinkVariable(eLoopUnit, pGame, null, pCity, eTribe));
                        }
                    }
                }


                if (infos().unit(eUnit).mltModVariables.Count > 0)
                {
                    foreach ((string First, string Second, TextType Third) modVariable in infos().unit(eUnit).mltModVariables)
                    {
                        if (modVariable.Third != TextType.NONE)
                        {
                            builder.Add(TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", TEXTVAR_TYPE(modVariable.Third), TEXTVAR(modVariable.Second)));
                        }
                    }
                }

                {
                    TextType eDescription = infos().unit(eUnit).meDescription;

                    if (eDescription != TextType.NONE)
                    {
                        builder.AddTEXT(eDescription);
                    }
                }

                return builder;
            }
        }
        //copy-paste END

        //copy-paste START
        //lines 22242-22506
        public override void buildUnitTooltip(Unit pUnit, ClientManager pManager, UIUnitTooltipData outUnitData)
        {
            using (new UnityProfileScope("HelpText.buildUnitTooltip"))
            {
                Player pActivePlayer = pManager.activePlayer();
                Game pGame = pManager.GameClient;

                // Important to note that outUnitData is cleared before passed into this method!

                if (pUnit == null)
                {
                    TextManager.TEXT(outUnitData.Title, buildTitleVariable(TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_DEAD")));
                    return;
                }

                bool bExpand = pManager.Interfaces.Hotkeys.IsHotkeyPressed(mInfos.Globals.HOTKEY_EXPAND_TOOLTIP);

                outUnitData.UnitID = pUnit.getID();

                using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager, outUnitData.Title))
                {
                    builder.Add(buildUnitTypeLinkVariable(pUnit.getType(), pGame, pUnit));
                }

                for (TeamType eLoopTeam = 0; eLoopTeam < pGame.getNumTeams(); eLoopTeam++)
                {
                    if (pUnit.isRaidTeam(eLoopTeam))
                    {
                        outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_RAIDING"), buildTeamLinkVariable(eLoopTeam, pGame, pActivePlayer));
                    }
                }

                if (pUnit.hasRebelPlayer())
                {
                    if (pUnit.rebelPlayer().isAlive())
                    {
                        outUnitData.AddStat(TextManager, buildTribeLinkVariable(infos().Globals.REBELS_TRIBE, pGame), buildPlayerLinkVariable(pUnit.rebelPlayer(), pActivePlayer));
                    }
                }

                if (pUnit.hasCaravanMissionTarget())
                {
                    City pCity = pUnit.caravanMissionTarget().capitalCity();

                    if (pCity != null)
                    {
                        outUnitData.AddStat(TextManager, buildCaravanMissionLinkVariable(), buildCityLinkVariable(pCity, pActivePlayer));
                    }
                }

                if (pUnit.isDamaged())
                {
                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_HEALTH_LABEL"), buildSlashText(pUnit.getHP(), pUnit.getHPMax()));
                }

                if (pUnit.getFortifyBonus() > 0)
                {
                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_FORTIFIED_LABEL"), buildDefenseValueLinkVariable(pUnit.getFortifyBonus(), true));
                }

                if (pUnit.getTestudoBonus() > 0)
                {
                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_TESTUDO_LABEL"), TEXTVAR("TEXT_HELPTEXT_EFFECT_UNIT_HELP_VS_UNIT_TRAIT", TextManager, buildUnitTraitLinkVariable(infos().Globals.RANGED_TRAIT), buildDefenseValueLinkVariable(pUnit.getTestudoBonus(), true), TEXTVAR(true)));
                }

                if (pUnit.isAnchored())
                {
                    outUnitData.AddStat(TextManager, TEXTVAR(""), buildAnchoredLinkVariable());
                }

                if (pUnit.isUnlimbered())
                {
                    outUnitData.AddStat(TextManager, TEXTVAR(""), buildUnlimberedLinkVariable());
                }

                if (bExpand)
                {
                    using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                    {
                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                        {
                            if (pUnit.hasZOC())
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_UNIT_TYPE_HAS_ZOC", buildZOCLinkVariable());
                            }

                            if (pUnit.hasIgnoreZOC(null))
                            {
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                  START ###
  ##############################################*/
                                if (!((BetterAIInfoUnit)pUnit.info()).bHasIngoreZOCBlocker)
                                {
                                    builder.Add(buildIgnoreZOCLinkVariable());
                                }
                                else
                                {
                                    builder.Add(buildIgnoreZOCWithExceptionsLinkVariable(pUnit.getType()));
                                }
/*####### Better Old World AI - Base DLL #######
  ### Fix ZOC display                    END ###
  ##############################################*/
                            }
                        }

                        if (builder.HasContent)
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR(""), builder.ToTextVariable());
                        }
                    }
                }

                for (EffectUnitClassType eLoopEffectUnitClass = 0; eLoopEffectUnitClass < infos().effectUnitClassesNum(); eLoopEffectUnitClass++)
                {
                    if (pUnit.hasEffectUnitClass(eLoopEffectUnitClass))
                    {
                        using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                for (EffectUnitType eLoopEffectUnit = 0; eLoopEffectUnit < infos().effectUnitsNum(); eLoopEffectUnit++)
                                {
                                    if (infos().effectUnit(eLoopEffectUnit).meClass == eLoopEffectUnitClass)
                                    {
                                        if (pUnit.hasEffectUnit(eLoopEffectUnit))
                                        {
                                            buildEffectUnitHelp(builder, eLoopEffectUnit, pGame, bRightJustify: true, bIncludeIndirect: false);
                                        }
                                    }
                                }
                            }

                            outUnitData.AddStat(TextManager, buildEffectUnitClassLinkVariable(eLoopEffectUnitClass, pUnit), builder.ToTextVariable());
                        }
                    }
                }

                foreach (EffectUnitType eLoopEffect in pUnit.getEffectUnits())
                {
                    if (infos().effectUnit(eLoopEffect).meClass == EffectUnitClassType.NONE)
                    {
                        if (!(pUnit.isGeneralEffectUnit(eLoopEffect)))
                        {
                            if (!pUnit.isEffectUnitSource(eLoopEffect, SourceEffectUnitType.UNIT))
                            {
                                using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                                    {
                                        buildEffectUnitHelp(builder, eLoopEffect, pGame, bRightJustify: true, bIncludeIndirect: false);
                                    }

                                    outUnitData.AddStat(TextManager, buildEffectUnitLinkVariable(eLoopEffect, pUnit), builder.ToTextVariable());
                                }
                            }
                            else if (bExpand)
                            {
                                using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    buildEffectUnitHelp(builder, eLoopEffect, pGame, bRightJustify: true, bIncludeIndirect: false);
                                    outUnitData.AddStat(TextManager, buildEffectUnitLinkVariable(eLoopEffect, pUnit), builder.ToTextVariable());
                                }
                            }
                        }
                    }
                }

                if (pUnit.hasGeneral())
                {
                    Character pGeneral = pUnit.general();

                    outUnitData.AddStat(TextManager, buildCharacterLinkVariable(pGeneral, pActivePlayer, false), buildGeneralRatingsAllHelpVariable(pGeneral));

                    foreach (EffectUnitType eLoopEffect in pUnit.getEffectUnits())
                    {
                        if (infos().effectUnit(eLoopEffect).meClass == EffectUnitClassType.NONE)
                        {
                            if (pUnit.isGeneralEffectUnit(eLoopEffect))
                            {
                                using (TextBuilder builder = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                                        buildEffectUnitHelp(builder, eLoopEffect, pGame, bRightJustify: true, bIncludeIndirect: false);
                                    outUnitData.AddStat(TextManager, buildEffectUnitLinkVariable(eLoopEffect, pUnit), builder.ToTextVariable());
                                }
                            }
                        }
                    }
                }

                if (pUnit.info().mbPromote)
                {
                    TextVariable levelXPVariable;
                    if (pUnit.isLevelPromotionMax())
                    {
                        levelXPVariable = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_LEVEL_MAX", TEXTVAR(pUnit.getLevelPromotionString()));
                    }
                    else
                    {
                        levelXPVariable = TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_LEVEL", TEXTVAR(pUnit.getLevelPromotionString()), TEXTVAR(pUnit.getXP()), TEXTVAR(pUnit.getXPThreshold()), TEXTVAR(pUnit.getLevelPromotion() < pUnit.getLevel()));
                    }
                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_LEVEL_LABEL"), levelXPVariable);
                }

                if (bExpand)
                {
                    if (pUnit.hasPlayer())
                    {
                        if (pUnit.hasFamily())
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_FAMILY_LABEL"), buildFamilyLinkVariable(pUnit.getFamily(), pGame));
                        }
                        else
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR(""), buildMercenaryLinkVariable());
                        }

                        if (pUnit.hasOriginalTribe())
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_TRIBE_LABEL"), buildTribeLinkVariable(pUnit.getOriginalTribe(), pGame));
                        }
                    }

                    using (TextBuilder traitsBuilder = TextBuilder.GetTextBuilder(TextManager))
                    {
                        buildUnitTraitsHelp(traitsBuilder, pUnit.getType());
                        if (traitsBuilder.HasContent)
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_TRAITS_LABEL"), traitsBuilder.ToTextVariable());
                        }
                    }

                    for (AttackType eLoopAttack = 0; eLoopAttack < infos().attacksNum(); eLoopAttack++)
                    {
                        int iValue = pUnit.attackValue(eLoopAttack);
                        if (iValue > 0)
                        {
                            TextVariable valueOrFalse = iValue > 1 ? TEXTVAR(iValue) : TEXTVAR(false);

                            outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_ATTACK_LABEL"), TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_ATTACK_VALUE_ITEM", buildAttackLinkVariable(eLoopAttack), valueOrFalse, TEXTVAR(pUnit.attackPercent(eLoopAttack))));
                        }
                    }

                    if (pUnit.getTurnSteps() > 0)
                    {
                        int iFatigueSteps = pUnit.getFatigueLimit() - pUnit.getStepsToFatigue();
                        outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_FATIGUE_LABEL"), buildSlashText(iFatigueSteps, pUnit.getFatigueLimit()));
                    }

                    if (pUnit.hasCooldown())
                    {
                        outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_COOLDOWN", TEXTVAR_TYPE(pUnit.cooldown().mName)), buildTurnsTextVariable(pUnit.getCooldownTurns(), pGame));
                    }

                    using (TextBuilder yieldListBuilder = TextBuilder.GetTextBuilder(TextManager))
                    {
                        using (yieldListBuilder.BeginScope(TextBuilder.ScopeType.COMMA))
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); eLoopYield++)
                            {
                                int iValue = pUnit.yieldConsumption(eLoopYield);
                                if (iValue != 0)
                                {
                                    yieldListBuilder.Add(buildUnitConsumesLinkVariable(buildYieldIconTextVariable(eLoopYield, -(iValue), bRate: true, iMultiplier: Constants.YIELDS_MULTIPLIER), pUnit));
                                }
                            }
                        }

                        if (yieldListBuilder.HasContent)
                        {
                            outUnitData.AddStat(TextManager, TEXTVAR_TYPE("TEXT_HELPTEXT_UNIT_TYPE_CONSUMPTION"), yieldListBuilder.ToTextVariable());
                        }
                    }


                    if (pUnit.isModVariables())
                    {
                        foreach ((string First, string Second, TextType Third) mvType in infos().unit(pUnit.getType()).mltModVariables)
                        {
                            if (mvType.Third != TextType.NONE)
                            {
                                string sVariable = pUnit.getModVariable(mvType.First);
                                if (!string.IsNullOrWhiteSpace(sVariable))
                                {
                                    outUnitData.AddStat(TextManager, TEXTVAR_TYPE(mvType.Third), TEXTVAR(sVariable));
                                }
                            }
                        }
                    }
                }
            }
        }
        //copy-paste END

        //lines 23001-23650
        public override TextBuilder buildEffectUnitHelp(TextBuilder builder, EffectUnitType eEffectUnit, Game pGame, bool bSkipIcons = false, bool bRightJustify = false, bool bIncludeIndirect = true)
        {
            //ToDo: group maiImprovementToModifier effects by improvementClasses, like specialist improvement prereqs

            builder = base.buildEffectUnitHelp(builder, eEffectUnit, pGame, bSkipIcons, bRightJustify);

/*####### Better Old World AI - Base DLL #######
  ### Land Unit Water Movement         START ###
  ##############################################*/
            BetterAIInfoEffectUnit eInfoEffectUnit = (BetterAIInfoEffectUnit)infos().effectUnit(eEffectUnit);
            if (eInfoEffectUnit.mbAmphibiousEmbark)
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
        //lines 37481-37607
        public override TextBuilder buildResourceHelp(TextBuilder builder, ResourceType eResource, Game pGame, Player pPlayer, Tile pTile, Player pActivePlayer, bool bName = true)
        {
            //using (new UnityProfileScope("HelpText.buildResourceHelp"))
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
                            int iValue = pGame.familyClass(eLoopFamily).maiLuxuryMissingOpinion[eResource];
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
                            int iValue = infos().resource(eResource).maiYieldNoImprovement[eLoopYield];
                            if (iValue > 0)
                            {
                                commaList.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
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
                                builder.Add(buildYieldValueIconLinkVariable(eLoopYield, iValue, iMultiplier: Constants.YIELDS_MULTIPLIER));
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
                            EffectCityType eEffectCity = infos().improvementClass(eLoopImprovementClass).maeResourceCityEffect[eResource];
                            if (eEffectCity != EffectCityType.NONE)
                            {
                                buildEffectCityHelp(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), false, pActivePlayer);
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
                        EffectCityType eEffectCity = infos().specialistClass(eLoopSpecialistClass).maeResourceCityEffect[eResource];
                        if (eEffectCity != EffectCityType.NONE)
                        {
                            buildEffectCityHelp(builder, eEffectCity, pGame, pCityTerritory, pCityTerritory?.governor(), false, pActivePlayer);
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
                            subText.Add(buildEffectCitySourceLinkVariable(eLoopEffectCity, null, null, pGame, pActivePlayer));
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



        //copy-paste START
        //lines 39607-39647
        public override TextBuilder buildUrbanHelp(TextBuilder builder)
        {
            using (builder.BeginScope(TextBuilder.ScopeType.BULLET))
            {
                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_URBAN");

/*####### Better Old World AI - Base DLL #######
  ### Urban restrictions Explained     START ###
  ##############################################*/
                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_URBAN_RESTRICTIONS");
/*####### Better Old World AI - Base DLL #######
  ### Urban restrictions Explained       END ###
  ##############################################*/

                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_URBAN_SPREADS_BORDERS");

                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_URBAN_TRADE_NETWORK");

                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                {
                    EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                    if (eEffectUnit != EffectUnitType.NONE)
                    {
                        int iValue = infos().effectUnit(eEffectUnit).miUrbanAttackModifier;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildUnitTraitLinkVariable(eLoopUnitTrait), buildAttackValueLinkVariable(iValue, true));
                        }
                    }
                }

                for (UnitTraitType eLoopUnitTrait = 0; eLoopUnitTrait < infos().unitTraitsNum(); eLoopUnitTrait++)
                {
                    EffectUnitType eEffectUnit = infos().unitTrait(eLoopUnitTrait).meEffectUnit;

                    if (eEffectUnit != EffectUnitType.NONE)
                    {
                        int iValue = infos().effectUnit(eEffectUnit).miUrbanDefenseModifier;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_ENTRY_COLON_SPACE_ONE", buildUnitTraitLinkVariable(eLoopUnitTrait), buildDefenseValueLinkVariable(iValue, true));
                        }
                    }
                }
            }

            return builder;
        }
        //copy-paste END

        //copy-paste START
        //lines 39783-40305
        public override TextBuilder buildCityYieldNetHelp(TextBuilder builder, City pCity, Character pGovernor, YieldType eYield, ClientManager pManager, bool bNetOnly = false, bool bReverseSign = false)
        {
            Game pGame = pManager.GameClient;
            Player pActivePlayer = pManager.activePlayer();

            if (pCity != null)
            {
                using (var effectCityCountsScoped = CollectionCache.GetDictionaryScoped<EffectCityType, int>())
                {
                    Dictionary<EffectCityType, int> dEffectCityCounts = effectCityCountsScoped.Value;

                    pCity.getEffectCityCountsForGovernor(pGovernor, dEffectCityCounts);

                    Player pPlayer = pCity.player();

                    int iRate = pCity.calculateCurrentYield(eYield);
                    int iBaseYield = pCity.getBaseYieldNetForGovernor(eYield, pGovernor);
                    int iModifiedYield = pCity.calculateModifiedYieldGovernor(eYield, pGovernor);

                    if (!bNetOnly)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_CONCAT_SPACE_TWO", buildYieldLinkVariable(eYield), buildIconTextVariable(infos().yield(eYield).meType));

                        if (pCity.isYieldBuildCurrent(eYield) && pCity.getCurrentBuild().mdYieldCosts.TryGetValue(eYield, out int value) && value > 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_USED_FOR",
                                buildColonSpaceOne(TEXTVAR(pCity.getName()), buildYieldValueIconLinkVariable(eYield, iRate, iMultiplier: Constants.YIELDS_MULTIPLIER)),
                                getQueueLinkVariable(pCity.getCurrentBuild(), pCity, pGame, true, false));
                        }
                        else
                        {
                            YieldType eDisplayYield = eYield;
                            int iDisplayRate = iRate;
                            getDisplayYield(ref eDisplayYield, ref iDisplayRate);

                            builder.Add(buildColonSpaceOne(TEXTVAR(pCity.getName()), buildYieldValueIconLinkVariable(eDisplayYield, iDisplayRate, iMultiplier: Constants.YIELDS_MULTIPLIER)));
                        }
                        builder.Add(getCityYieldProgress(pCity, eYield), skipSeparator: true);

                        int iTileBase = pCity.calculateUnmodifiedTileYield(eYield, pGovernor);
                        if (iTileBase != 0 && iTileBase != iBaseYield)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TILE_BASE_YIELD", buildYieldTextVariable(iTileBase, iMultiplier: Constants.YIELDS_MULTIPLIER));
                        }
                        // base yield included in subtotal, below
                        //if (iBaseYield != 0 && iBaseYield != iRate)
                        //{
                        //    builder.AddTEXT("TEXT_HELPTEXT_CITY_BASE_YIELD", buildYieldTextVariable(iBaseYield, iMultiplier: Constants.YIELDS_MULTIPLIER));
                        //}

                        buildDividerText(builder);
                    }

                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); ++eLoopYield)
                    {
                        if (infos().yield(eLoopYield).meSubtractFromYield == eYield)
                        {
                            builder.Add(buildYieldIconNameLinkVariable(eYield));
                            break;
                        }
                    }

                    for (int iPass = 0; iPass < 2; iPass++)
                    {
                        SetList<EffectCityType> seGovernorCityEffects = new SetList<EffectCityType>();

                        if (pGovernor != null)
                        {
                            foreach (TraitType eLoopTrait in pGovernor.getTraits())
                            {
                                EffectCityType eEffectCity = infos().trait(eLoopTrait).meGovernorEffectCity;
                                EffectCityType eEffectCityState = infos().trait(eLoopTrait).meStateReligionEffectCity;

                                if (eEffectCity != EffectCityType.NONE)
                                {
                                    int iValue = pCity.getEffectCityYieldRate(eEffectCity, eYield, pGovernor);
                                    if (bReverseSign)
                                    {
                                        iValue = -(iValue);
                                    }
                                    if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_CONCAT_COLON_ENCLOSED_PARENTHESIS",
                                            buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER),
                                            buildCharacterLinkVariable(pGovernor, pActivePlayer),
                                            buildTraitLinkVariable(eLoopTrait, pGovernor));
                                    }

                                    seGovernorCityEffects.Add(eEffectCity);
                                }

                                if (eEffectCityState != EffectCityType.NONE && pCity.hasStateReligion())
                                {
                                    int iValue = pCity.getEffectCityYieldRate(eEffectCityState, eYield, pGovernor);
                                    if (bReverseSign)
                                    {
                                        iValue = -(iValue);
                                    }
                                    if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_CONCAT_COLON_ENCLOSED_PARENTHESIS",
                                            buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER),
                                            buildCharacterLinkVariable(pGovernor, pActivePlayer),
                                            buildTraitLinkVariable(eLoopTrait, pGovernor));
                                    }

                                    seGovernorCityEffects.Add(eEffectCityState);
                                }
                            }
                        }

                        foreach (var p in dEffectCityCounts)
                        {
                            EffectCityType eLoopEffectCity = p.Key;
                            int iCount = p.Value;

                            if (seGovernorCityEffects.Contains(eLoopEffectCity))
                            {
                                iCount--;
                            }

                            if (iCount > 0)
                            {
                                int iValue = pCity.getEffectCityYieldRate(eLoopEffectCity, eYield, pGovernor);
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    TextVariable yieldEffectText = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_COLON_X_COUNT",
                                        buildYieldTextVariable((iCount * iValue), true, false, Constants.YIELDS_MULTIPLIER),
                                        buildEffectCitySourceLinkVariable(eLoopEffectCity, pCity, pGovernor, pGame, pActivePlayer),
                                        (iCount > 1 ? TEXTVAR(iCount) : TEXTVAR(false)));

                                    if (infos().effectCity(eLoopEffectCity).maaiEffectCityYieldRate.Count > 0)
                                    {
                                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                        {
                                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: buildEnclosedParenthesisIf(yieldEffectText, null), emptyScopeText: TEXTVAR(false)))
                                            {
                                                foreach (var q in dEffectCityCounts)
                                                {
                                                    EffectCityType eOtherEffectCity = q.Key;
                                                    int iOtherCount = q.Value;
                                                    int iSubValue = infos().effectCity(eLoopEffectCity).maaiEffectCityYieldRate[eOtherEffectCity, eYield];
                                                    if (iSubValue != 0)
                                                    {
                                                        subText.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_YIELD_FROM_X",
                                                            buildSignedTextVariable((iCount * iSubValue), false, Constants.YIELDS_MULTIPLIER),
                                                            buildEffectCitySourceLinkVariable(eOtherEffectCity, pCity, pGovernor, pGame, pActivePlayer),
                                                            ((iOtherCount > 1) ? TEXTVAR(iOtherCount) : TEXTVAR(false))
                                                            );
                                                    }
                                                }
                                            }

                                            builder.Add(subText.ToTextVariable());
                                        }
                                    }
                                    else
                                    {
                                        builder.Add(yieldEffectText);
                                    }
                                }
                            }
                        }

                        using (var resourceMapScoped = CollectionCache.GetDictionaryScoped<ResourceType, int>())
                        using (var improvementMapScoped = CollectionCache.GetDictionaryScoped<ImprovementType, int>())
                        using (var specialistMapScoped = CollectionCache.GetDictionaryScoped<SpecialistType, int>())
                        {
                            Dictionary<ResourceType, int> dResourceYields = resourceMapScoped.Value;
                            Dictionary<ImprovementType, int> dImprovementYields = improvementMapScoped.Value;
                            Dictionary<SpecialistType, int> dSpecialistYields = specialistMapScoped.Value;
                            foreach (int iTileID in pCity.getTerritoryTiles())
                            {
                                Tile pTile = pGame.tile(iTileID);
                                if (pTile.hasImprovementFinished())
                                {
                                    ImprovementType eImprovement = pTile.getImprovement();
                                    SpecialistType eSpecialist = pTile.getSpecialist();

                                    int iBase = pTile.yieldOutputModified(pTile.getImprovement(), SpecialistType.NONE, eYield);
                                    if (iBase != 0)
                                    {
                                        if (!dImprovementYields.ContainsKey(eImprovement))
                                        {
                                            dImprovementYields.Add(eImprovement, iBase);
                                        }
                                        else
                                        {
                                            dImprovementYields[eImprovement] += iBase;
                                        }
                                    }

                                    if (eSpecialist != SpecialistType.NONE)
                                    {
                                        int iSpecialist = (pTile.yieldOutputModified(pTile.getImprovement(), eSpecialist, eYield) - iBase);
                                        if (iSpecialist != 0)
                                        {
                                            if (!dSpecialistYields.ContainsKey(eSpecialist))
                                            {
                                                dSpecialistYields.Add(eSpecialist, iSpecialist);
                                            }
                                            else
                                            {
                                                dSpecialistYields[eSpecialist] += iSpecialist;
                                            }
                                        }
                                    }
                                }
                                else if (pTile.hasResource())
                                {
                                    ResourceType eResource = pTile.getResource();

                                    int iValue = pTile.yieldOutputModified(ImprovementType.NONE, SpecialistType.NONE, eYield);
                                    if (iValue != 0)
                                    {
                                        if (!dResourceYields.ContainsKey(eResource))
                                        {
                                            dResourceYields.Add(eResource, iValue);
                                        }
                                        else
                                        {
                                            dResourceYields[eResource] += iValue;
                                        }
                                    }
                                }
                            }
                            foreach (KeyValuePair<ResourceType, int> pPair in dResourceYields)
                            {
                                int iValue = pPair.Value;
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_YIELD_FROM_RESOURCE", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildResourceLinkVariable(pPair.Key));
                                }
                            }
                            foreach (KeyValuePair<SpecialistType, int> pPair in dSpecialistYields)
                            {
                                int iValue = pPair.Value;
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_YIELD_FROM_IMPROVEMENT", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildSpecialistLinkVariable(pPair.Key, pGame));
                                }
                            }
                            foreach (KeyValuePair<ImprovementType, int> pPair in dImprovementYields)
                            {
                                int iValue = pPair.Value;
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.Add(buildColonSpaceOne(buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildImprovementLinkVariable(pPair.Key, pGame)));
                                }
                            }
                        }

                        if (pGovernor != null && pGovernor.isLeader())
                        {
                            int iValue = infos().yield(eYield).miLeaderGovernor;
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_GOVERNOR_IS_LEADER", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER));
                            }
                        }

                        if (pCity.getTeam() != pCity.getFirstTeam())
                        {
                            int iValue = infos().yield(eYield).miForeignPopulation;
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_FOREIGN_POPULATION", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildNationLinkVariable(pCity.firstNation().meType, pCity.getFirstPlayer()));
                            }
                        }

                        {
                            int iValue = pCity.calculateYieldRateDefending(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_DEFENDING", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), TEXTVAR(pCity.hasFamily()));
                            }
                        }

                        {
                            int iValue = pCity.calculateImprovementYield(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                int iCount = pCity.getFinishedImprovementCountAll();
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_IMPROVEMENT", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), ((iCount > 1) ? TEXTVAR(iCount) : TEXTVAR(false)), buildUrbanLinkVariable());
                            }
                        }

                        {
                            int iValue = pCity.calculateCapitalDistanceYield(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_CAPITAL_DISTANCE", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildCapitalLinkVariable(pPlayer.capitalCity()));
                            }
                        }

                        if (pGovernor != null)
                        {
                            int iValue = pCity.calculateGovernorOpinionYield(eYield, pGovernor);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_GOVERNOR_OPINION", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER), buildOpinionCharacterLinkVariable(pPlayer.getCharacterOpinion(pGovernor), pGovernor, pPlayer.getPlayer()), buildCharacterLinkVariable(pGovernor, pActivePlayer));
                            }
                        }

                        if (pPlayer != null)
                        {
                            {
                                int iValue = pPlayer.calculateMissingFamilyYield(eYield);
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_MISSING_FAMILY", buildYieldTextVariable(iValue, true, false, Constants.YIELDS_MULTIPLIER));
                                }
                            }

                            for (CouncilType eLoopCouncil = 0; eLoopCouncil < infos().councilsNum(); eLoopCouncil++)
                            {
                                Character pCouncilCharacter = pPlayer.councilCharacter(eLoopCouncil);

                                if (pCouncilCharacter != null)
                                {
                                    for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                                    {
                                        int iValue = pCouncilCharacter.getRatingYieldRateCouncilCity(eLoopRating, eYield, eLoopCouncil);
                                        if (bReverseSign)
                                        {
                                            iValue = -(iValue);
                                        }
                                        if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_CONCAT_COLON_ENCLOSED_PARENTHESIS",
                                                buildSignedTextVariable(iValue, false, Constants.YIELDS_MULTIPLIER),
                                                buildCharacterLinkVariable(pCouncilCharacter, pActivePlayer, bCognomen: false, bSkipNation: true),
                                                buildCharacterRatingLinkVariable(pCouncilCharacter, eLoopRating, true));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (iBaseYield != iModifiedYield)
                    {
                        if (bReverseSign)
                        {
                            iBaseYield *= -1;
                        }
                        builder.AddTEXT("TEXT_HELPTEXT_SUBTOTAL", buildSignedTextVariable(iBaseYield, iMultiplier: Constants.YIELDS_MULTIPLIER));
                    }

                    for (int iPass = 0; iPass < 2; iPass++)
                    {
                        {
                            int iValue = pCity.getHappinessLevelYieldModifier(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.Add(buildColonSpaceOne(buildSignedTextVariable(iValue, true), buildHappinessLevelLinkVariable(pCity)));
                            }
                        }

                        {
                            int iValue = pCity.getDamageYieldModifier(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_DAMAGE_YIELD_MODIFIER", buildSignedTextVariable(iValue, true), TEXTVAR(pCity.getHP()), TEXTVAR(pCity.getHPMax()));
                            }
                        }

                        {
                            int iValue = pCity.getAssimilateYieldModifier(eYield);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_ASSIMILATE_YIELD_MODIFIER", buildSignedTextVariable(iValue, true));
                            }
                        }

                        foreach (var p in dEffectCityCounts)
                        {
                            EffectCityType eLoopEffectCity = p.Key;
                            int iCount = p.Value;
                            int iValue = (infos().effectCity(eLoopEffectCity).maiYieldModifier[eYield] * iCount);
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_YIELD_FROM_X",
                                    buildSignedTextVariable(iValue, true),
                                    buildEffectCitySourceLinkVariable(eLoopEffectCity, pCity, pGovernor, pGame, pActivePlayer),
                                    (iCount > 1 ? TEXTVAR(iCount) : TEXTVAR(false)));
                            }
                        }

                        if (pGovernor != null)
                        {
                            for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                            {
                                int iValue = pGovernor.getRatingYieldModifierGovernor(eLoopRating, eYield);
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_GOVERNOR_YIELD_MODIFIER", buildSignedTextVariable(iValue, true), buildCharacterLinkVariable(pGovernor, pActivePlayer), buildCharacterRatingLinkVariable(pGovernor, eLoopRating));
                                }
                            }
                        }

                        if (pCity.hasBuild())
                        {
                            CityQueueData pCurrentBuild = pCity.getCurrentBuild();

                            if (pCurrentBuild.meBuild == infos().Globals.PROJECT_BUILD)
                            {
                                ProjectType eProject = (ProjectType)(pCurrentBuild.miType);
                                int iValue = infos().project(eProject).maiYieldModifier[eYield];
                                if (bReverseSign)
                                {
                                    iValue = -(iValue);
                                }
                                if ((iPass == 0) ? (iValue > 0) : (iValue < 0))
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_LINK_HELP_CITY_PROJECT_YIELD_MODIFIER", buildSignedTextVariable(infos().utils().modify(pCity.getBaseYieldNet(eYield), iValue - 100), false, Constants.YIELDS_MULTIPLIER), buildProjectLinkVariable(eProject, pCity));
                                }
                            }
                        }
                    }

                    for (UnitType eLoopUnit = 0; eLoopUnit < infos().unitsNum(); eLoopUnit++)
                    {
                        int iValue = pCity.calculateUnitImprovementYield(eYield, eLoopUnit);
                        if (iValue != 0)
                        {
                            if (bReverseSign)
                            {
                                iValue = -(iValue);
                            }
                            builder.AddTEXT("TEXT_HELPTEXT_YIELD_NET_UNIT_IMPROVEMENT_YIELD", buildSignedTextVariable(iValue, false, Constants.YIELDS_MULTIPLIER), buildUnitTypeLinkVariable(eLoopUnit, pGame));
                        }
                    }

                    for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); ++eLoopYield)
                    {
                        if (infos().yield(eLoopYield).meSubtractFromYield == eYield)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TOTAL_YIELD", buildYieldLinkVariable(eYield), buildSignedTextVariable(iModifiedYield, iMultiplier: Constants.YIELDS_MULTIPLIER));
                            break;
                        }
                    }

                    {
                        for (YieldType eLoopYield = 0; eLoopYield < infos().yieldsNum(); ++eLoopYield)
                        {
                            if (infos().yield(eLoopYield).meSubtractFromYield == eYield)
                            {
                                buildDividerText(builder);
                                builder.Add(buildYieldIconNameLinkVariable(eLoopYield));
/*####### Better Old World AI - Base DLL #######
  ### don't reverse sign               START ###
  ##############################################*/
                                //buildCityYieldNetHelp(builder, pCity, eLoopYield, pManager, bNetOnly: true, bReverseSign: true);
                                //builder.AddTEXT("TEXT_HELPTEXT_TOTAL_YIELD", buildYieldLinkVariable(eLoopYield), buildSignedTextVariable(-(pCity.calculateModifiedYieldBase(eLoopYield)), iMultiplier: Constants.YIELDS_MULTIPLIER));
                                buildCityYieldNetHelp(builder, pCity, pGovernor, eLoopYield, pManager, bNetOnly: true, bReverseSign: false);
                                builder.AddTEXT("TEXT_HELPTEXT_TOTAL_YIELD", buildYieldLinkVariable(eLoopYield), buildSignedTextVariable(pCity.calculateModifiedYield(eLoopYield), iMultiplier: Constants.YIELDS_MULTIPLIER));
/*####### Better Old World AI - Base DLL #######
  ### don't reverse sign                 END ###
  ##############################################*/
                            }
                        }
                    }

                    if (!bNetOnly && pActivePlayer != null && pActivePlayer.isPlayerOption(infos().Globals.DEBUG_HELP))
                    {
                        buildYieldDebugText(builder, eYield, pManager);
                    }
                }
            }

            return builder;
        }
        //copy-paste END

/*####### Better Old World AI - Base DLL #######
  ### misc                             START ###
  ##############################################*/
        //public struct BetterAICommaListVariableGenerator : CommaListVariableGenerator
        //{
        //}
        //fuck me, structs can't inherit
        //I can't overrride a struct but at least inside the derived BetterAIHelpText class I can call this for proper japanese list generation

        //lines 44213-44336
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
