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

        //lines 2072-3148
        public override TextBuilder buildTraitHelp(TextBuilder builder, TraitType eTrait, Game pGame, Player pActivePlayer, Character pCharacter = null, bool bName = false, bool bInvalidTraits = false, bool bRestrictions = false, bool bDetails = false, bool bRelevantOnly = false, bool bEncyclopedia = false, TextBuilder.ScopeType scopeType = TextBuilder.ScopeType.BULLET)
        {
            //UnityEngine.Debug.Log("HelpText.buildTraitHelp");

            //using (new UnityProfileScope("HelpText.buildTraitHelp"))
            {

                bool bAdvancedHelp = pActivePlayer == null || pActivePlayer.isPlayerOption(infos().Globals.ADVANCED_HELP);
                bool bShowYieldTotals = bAdvancedHelp;

                BetterAIInfoTrait pInfoTrait = (BetterAIInfoTrait)infos().trait(eTrait);

                if (bName)
                {
                    InfoTrait trait = infos().trait(eTrait);
                    TextType traitName = getGenderedTraitName(trait, pCharacter?.getGender() ?? GenderType.NONE);
                    builder.AddTEXT("TEXT_HELPTEXT_TRAIT_TITLE",
                    string.IsNullOrEmpty(trait.mzIconName) ? TEXTVAR(false) : mSpriteRepository?.GetInlineIconVariable(eTrait) ?? TEXTVAR(false),
                    TEXTVAR_TYPE(traitName),
                    TEXTVAR(trait.mbArchetype),
                    TEXTVAR(trait.mbItem));
                }

                using (builder.BeginScope(scopeType))
                {
                    if (!bRelevantOnly)
                    {
                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                        {
                            buildTraitJobs(subText, eTrait, pCharacter);
                            if (subText.HasContent)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_JOB_PREREQ", subText.ToTextVariable());
                            }
                        }
                    }

                    if (!bRelevantOnly)
                    {
                        if (infos().trait(eTrait).mbClergy)
                        {
                            if (infos().religion(infos().trait(eTrait).meReligion).meType == ReligionType.NONE)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_CLERGY_TRAIT");
                            }
                            else if (infos().religion(infos().trait(eTrait).meReligion).mePaganNation != NationType.NONE)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_PAGAN_CLERGY_TRAIT");
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_WORLD_CLERGY_TRAIT");
                            }
                        }
                    }

                    {
                        EffectCityType eEffectCity = infos().trait(eTrait).meGovernorEffectCity;

                        if (eEffectCity != EffectCityType.NONE)
                        {
                            City pCityGovernor = pCharacter?.cityGovernor();

                            if (!bRelevantOnly || pCityGovernor != null)
                            {
                                if (bRelevantOnly)
                                {
                                    buildEffectCityHelp(builder, eEffectCity, pCityGovernor?.player(), pCityGovernor, pCharacter, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowYieldTotals, bSkipZero: false);
                                }
                                else
                                {
                                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                        {
                                            buildEffectCityHelp(subText, eEffectCity, pCityGovernor?.player(), pCityGovernor, pCharacter, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowYieldTotals, bSkipZero: false);
                                        }

                                        if (subText.HasContent)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_CHARACTER_TRAIT_GOVERNOR", subText.ToTextVariable());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        EffectCityType eEffectCity = infos().trait(eTrait).meStateReligionEffectCity;

                        if (eEffectCity != EffectCityType.NONE)
                        {
                            City pCityGovernor = pCharacter?.cityGovernor();

                            if (!bRelevantOnly || (pCityGovernor != null && pCityGovernor.hasStateReligion()))
                            {
                                if (bRelevantOnly)
                                {
                                    buildEffectCityHelp(builder, eEffectCity, pCityGovernor?.player(), pCityGovernor, pCharacter, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowYieldTotals, bSkipZero: false);
                                }
                                else
                                {
                                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                        {
                                            buildEffectCityHelp(subText, eEffectCity, pCityGovernor?.player(), pCityGovernor, pCharacter, pActivePlayer, bSkipImpossible: true, bShowTotal: bShowYieldTotals, bSkipZero: false);
                                        }

                                        if (subText.HasContent)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_CHARACTER_TRAIT_GOVERNOR_STATE_RELIGION", subText.ToTextVariable());
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        EffectUnitType eEffectUnit = infos().trait(eTrait).meGeneralEffectUnit;

                        if (eEffectUnit != EffectUnitType.NONE)
                        {
                            if (!bRelevantOnly || (pCharacter?.isUnitGeneral() ?? false))
                            {
                                if (bRelevantOnly)
                                {
                                    buildEffectUnitHelp(builder, eEffectUnit, pGame);
                                }
                                else
                                {
                                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                        {
                                            buildEffectUnitHelp(subText, eEffectUnit, pGame);
                                        }

                                        if (subText.HasContent)
                                        {
                                            if ((infos().effectUnit(eEffectUnit).mabUnitTraitValid.Count > 0 || infos().effectUnit(eEffectUnit).mabUnitTraitInvalid.Count > 0) && pActivePlayer != null && pActivePlayer.isPlayerOption(infos().Globals.ADVANCED_HELP))
                                            {
                                                using (TextBuilder includeListText = TextBuilder.GetTextBuilder(TextManager))
                                                using (TextBuilder excludeListText = TextBuilder.GetTextBuilder(TextManager))
                                                {
                                                    using (includeListText.BeginScope(TextBuilder.ScopeType.COMMA_OR))
                                                    {
                                                        foreach (KeyValuePair<UnitTraitType, bool> eKeyValuePair in infos().effectUnit(eEffectUnit).mabUnitTraitValid)
                                                        {
                                                            includeListText.Add(buildUnitTraitLinkVariable(eKeyValuePair.Key));
                                                        }
                                                    }
                                                    using (excludeListText.BeginScope(TextBuilder.ScopeType.COMMA_AND))
                                                    {
                                                        foreach (KeyValuePair<UnitTraitType, bool> eKeyValuePair in infos().effectUnit(eEffectUnit).mabUnitTraitInvalid)
                                                        {
                                                            excludeListText.Add(buildUnitTraitLinkVariable(eKeyValuePair.Key));
                                                        }
                                                    }

                                                    if (includeListText.HasContent)
                                                    {
                                                        builder.AddTEXT("TEXT_HELPTEXT_CHARACTER_TRAIT_GENERAL_UNIT_TRAIT", includeListText.ToTextVariable(), subText.ToTextVariable(), excludeListText.HasContent, excludeListText.ToTextVariable());
                                                    }
                                                    else if (excludeListText.HasContent)
                                                    {
                                                        builder.AddTEXT("TEXT_HELPTEXT_CHARACTER_TRAIT_GENERAL_UNIT_TRAIT_EXCLUDES", excludeListText.ToTextVariable(), subText.ToTextVariable());
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                builder.AddTEXT("TEXT_HELPTEXT_CHARACTER_TRAIT_GENERAL", subText.ToTextVariable(), TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        EffectUnitType eEffectUnit = infos().trait(eTrait).meExplorerEffectUnit;

                        if (eEffectUnit != EffectUnitType.NONE)
                        {
                            if (!bRelevantOnly || (pCharacter?.isUnitExplorer() ?? false))
                            {
                                if (bRelevantOnly)
                                {
                                    buildEffectUnitHelp(builder, eEffectUnit, pGame);
                                }
                                else
                                {
                                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                        {
                                            buildEffectUnitHelp(subText, eEffectUnit, pGame);
                                        }

                                        if (subText.HasContent)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_CHARACTER_TRAIT_EXPLORER", subText.ToTextVariable(), TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        EffectUnitType eEffectUnit = infos().trait(eTrait).meLeaderEffectUnit;

                        if (eEffectUnit != EffectUnitType.NONE)
                        {
                            if (!bRelevantOnly || (pCharacter != null && pCharacter.isLeader() && pCharacter.isUnitGeneral()))
                            {
                                if (bRelevantOnly)
                                {
                                    buildEffectUnitHelp(builder, eEffectUnit, pGame);
                                }
                                else
                                {
                                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                        {
                                            buildEffectUnitHelp(subText, eEffectUnit, pGame);
                                        }

                                        if (subText.HasContent)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_CHARACTER_TRAIT_GENERAL_LEADER", subText.ToTextVariable(), TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        EffectPlayerType eEffectPlayer = infos().trait(eTrait).meLeaderEffectPlayer;

                        if (eEffectPlayer != EffectPlayerType.NONE)
                        {
                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    Player pCharacterPlayer = pCharacter != null ? pCharacter.player() : null;
                                    buildEffectPlayerHelp(subText, eEffectPlayer, pGame, pCharacterPlayer, pActivePlayer, bShowTotal: true);
                                }

                                if (subText.HasContent)
                                {
                                    builder.AddTEXT("TEXT_HELPTEXT_TRAIT_LEADER", subText.ToTextVariable());
                                }
                            }

                            for (CouncilType eLoopCouncil = 0; eLoopCouncil < infos().councilsNum(); eLoopCouncil++)
                            {
                                if (!(infos().council(eLoopCouncil).mbDisable))
                                {
                                    if (infos().council(eLoopCouncil).meEffectPlayerPrereq == eEffectPlayer)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_TECH_UNLOCKS_GENERIC", buildCouncilLinkVariable(eLoopCouncil));
                                    }
                                }
                            }
                        }
                    }

                    //foreach ((CouncilType eCouncil, EffectPlayerType eEffectPlayer) in infos().trait(eTrait).maeCouncilEffectPlayer)
                    foreach (KeyValuePair<CouncilType, EffectPlayerType> p in infos().trait(eTrait).maeCouncilEffectPlayer)
                    {
                        //if (!bRelevantOnly || (pCharacter != null && pCharacter.getCouncil() == eCouncil))
                        if (!bRelevantOnly || (pCharacter != null && pCharacter.getCouncil() == p.Key))
                        {
                            Player pCharacterPlayer = pCharacter?.player();

                            if (bRelevantOnly)
                            {
                                //buildEffectPlayerHelp(builder, eEffectPlayer, pGame, pCharacterPlayer, pActivePlayer, bShowTotal: bShowYieldTotals);
                                buildEffectPlayerHelp(builder, p.Value, pGame, pCharacterPlayer, pActivePlayer, bShowTotal: bShowYieldTotals);
                            }
                            else
                            {
                                using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                    {
                                        //buildEffectPlayerHelp(subText, eEffectPlayer, pGame, pCharacterPlayer, pActivePlayer, bShowTotal: bShowYieldTotals);
                                        buildEffectPlayerHelp(subText, p.Value, pGame, pCharacterPlayer, pActivePlayer, bShowTotal: bShowYieldTotals);
                                    }

                                    if (subText.HasContent)
                                    {
                                        //builder.AddTEXT("TEXT_HELPTEXT_AS_LIST", buildCouncilLinkVariable(eCouncil), subText.ToTextVariable());
                                        builder.AddTEXT("TEXT_HELPTEXT_AS_LIST", buildCouncilLinkVariable(p.Key), subText.ToTextVariable());
                                    }
                                }
                            }
                        }
                    }


/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses           START ###
  ##############################################*/
                    {
                        if (pInfoTrait.maeJobEffectPlayer.Count > 0)
                        {
                            for (JobType eLoopJob = 0; eLoopJob < infos().jobsNum(); eLoopJob++)
                            {
                                if (pGame != null ? pGame.checkGameContent(eLoopJob) : App.CheckContentOwnership(infos().job(eLoopJob).meGameContentRequired, infos(), null))
                                {
                                    EffectPlayerType eEffectPlayer = pInfoTrait.maeJobEffectPlayer[eLoopJob];
                                    if (eEffectPlayer != EffectPlayerType.NONE)
                                    {
                                        using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                        {
                                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                            {
                                                Player pCharacterPlayer = pCharacter != null ? pCharacter.player() : null;
                                                buildEffectPlayerHelp(subText, eEffectPlayer, pGame, pCharacterPlayer, pActivePlayer, bShowTotal: true);
                                            }

                                            if (subText.HasContent)
                                            {
                                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_JOB", buildJobLinkVariable(eLoopJob, pCharacter), subText.ToTextVariable());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    {
                        if (pInfoTrait.bAnyTraitEffectPlayer)
                        {
                            for (TraitType eLoopTrait = 0; eLoopTrait < infos().traitsNum(); eLoopTrait++)
                            {
                                EffectPlayerType eEffectPlayer = pInfoTrait.maeTraitEffectPlayer[eLoopTrait];
                                if (eEffectPlayer == EffectPlayerType.NONE)
                                {
                                    BetterAIInfoTrait pLoopInfoTrait = (BetterAIInfoTrait)infos().trait(eLoopTrait);
                                    eEffectPlayer = pLoopInfoTrait.maeTraitEffectPlayer[eTrait];
                                }

                                if (eEffectPlayer != EffectPlayerType.NONE)
                                {
                                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                    {
                                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA))
                                        {
                                            Player pCharacterPlayer = pCharacter != null ? pCharacter.player() : null;
                                            buildEffectPlayerHelp(subText, eEffectPlayer, pGame, pCharacterPlayer, pActivePlayer, bShowTotal: true);
                                        }

                                        if (subText.HasContent)
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_TRAIT", buildTraitLinkVariable(eLoopTrait, pCharacter), subText.ToTextVariable());
                                        }
                                    }
                                }
                            }
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### Alternative GV bonuses             END ###
  ##############################################*/

                    for (MissionType eLoopMission = 0; eLoopMission < infos().missionsNum(); eLoopMission++)
                    {
                        void addMission(TextBuilder builder, MissionType eMission, SubjectType eSubject, Character pCharacter)
                        {
                            if (eSubject != SubjectType.NONE)
                            {
                                void buildHelpLocal()
                                {
                                    if (infos().subject(eSubject).meCharacter == CharacterType.NONE || (pCharacter != null && infos().subject(eSubject).meCharacter == pCharacter.getCharacter()))
                                    {
                                        TextVariable missionVar = buildMissionLinkVariable(eMission);
                                        if (infos().subject(eSubject).mbLeader || infos().subject(eSubject).meClass == infos().Globals.PLAYER_SUBJECTCLASS)
                                        {
                                            if (!bRelevantOnly)
                                            {
                                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_LEADER_MISSION", missionVar);
                                            }
                                            else if (pCharacter != null && pCharacter.isLeader())
                                            {
                                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_MISSION", missionVar);
                                            }
                                        }
                                        else
                                        {
                                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_MISSION", missionVar);
                                        }
                                    }
                                }

                                if (infos().subject(eSubject).meTraitPrereq == eTrait)
                                {
                                    buildHelpLocal();
                                }
                                if (infos().subject(eSubject).maeTraitAny.Count > 0)
                                {
                                    foreach (TraitType eLoopTrait in infos().subject(eSubject).maeTraitAny)
                                    {
                                        if (eTrait == eLoopTrait)
                                        {
                                            buildHelpLocal();
                                        }
                                    }
                                }
                            }
                        }

                        addMission(builder, eLoopMission, infos().mission(eLoopMission).meSubjectCharacter, pCharacter);

                        foreach (SubjectType eLoopSubject in infos().mission(eLoopMission).maeSubjectCharacterOn)
                        {
                            if (eLoopSubject != infos().mission(eLoopMission).meSubjectCharacter)
                            {
                                addMission(builder, eLoopMission, eLoopSubject, pCharacter);
                            }
                        }
                        if (!bRelevantOnly || (pCharacter != null && pCharacter.isLeader()))
                        {
                            foreach (SubjectType eLoopSubject in infos().mission(eLoopMission).maeSubjectPlayerOn)
                            {
                                if (infos().subject(eLoopSubject).meClass == infos().Globals.PLAYER_SUBJECTCLASS)
                                {
                                    addMission(builder, eLoopMission, eLoopSubject, pCharacter);
                                }
                            }
                        }
                    }

                    if (!bRelevantOnly)
                    {
                        int iValue = infos().trait(eTrait).miXPTurn;
                        if (iValue > 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_XP_PER_YEAR", buildSignedTextVariable(iValue), buildTurnScaleName(pGame));

                            if (pCharacter != null)
                            {
                                builder.AddWithParenthesis(buildSlashText(pCharacter.getXP(), pCharacter.getUpgradeXPThreshold()));
                            }
                        }
                    }

                    if (!bRelevantOnly)
                    {
                        int iValue = infos().trait(eTrait).miStrengthLimitModifier;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_STRENGTH_MODIFIER", buildSignedTextVariable(iValue));
                        }
                    }

                    if (!bRelevantOnly)
                    {
                        int iValue = infos().trait(eTrait).miWeaknessLimitModifier;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_WEAKNESS_MODIFIER", buildSignedTextVariable(iValue));
                        }
                    }

                    if (!bRelevantOnly)
                    {
                        if (infos().trait(eTrait).meReligionAgent != ReligionType.NONE)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_RELIGION_AGENT", buildAgentLinkVariable(), buildReligionLinkVariable(infos().trait(eTrait).meReligionAgent, pGame, pActivePlayer));
                        }
                    }

                    if (!bRelevantOnly || (pCharacter?.isCityAgent() ?? false))
                    {
                        int iValue = infos().trait(eTrait).miAgentModifier;
                        if (iValue != 0)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_AGENT_MODIFIER", buildSignedTextVariable(iValue, true));
                        }
                    }

                    if (!bRelevantOnly)
                    {
                        foreach (TraitType eRemoveTrait in infos().trait(eTrait).maeTraitReplaces)
                        {
                            if (pCharacter?.isTrait(eRemoveTrait) ?? false)
                            {
                                builder.AddTEXT("TEXT_GAME_DO_BONUS_REPLACES", buildTraitLinkVariable(eRemoveTrait, pCharacter));
                            }
                        }
                    }

                    if (!bRelevantOnly)
                    {
                        if (pCharacter != null && pCharacter.getTeam() == pActivePlayer.getTeam())
                        {
                            using (TextBuilder subCommaText = TextBuilder.GetTextBuilder(TextManager))
                            {
                                bool bRatingValue = false;

                                using (subCommaText.BeginScope(TextBuilder.ScopeType.COMMA))
                                {
                                    TraitType eArchetype = pCharacter.getArchetype();

                                    for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                                    {
                                        int iValue = infos().trait(eTrait).maiRating[eLoopRating] + infos().trait(eTrait).maiPermanentRating[eLoopRating];

                                        if (infos().trait(eTrait).mbArchetype)
                                        {
                                            if ((eArchetype != TraitType.NONE) && (eArchetype != eTrait))
                                            {
                                                iValue -= infos().trait(eArchetype).maiRating[eLoopRating];
                                            }
                                        }

                                        foreach (TraitType eOtherTrait in infos().trait(eTrait).maeTraitReplaces)
                                        {
                                            if (pCharacter.isTrait(eOtherTrait) && (eArchetype != eOtherTrait))
                                            {
                                                iValue -= infos().trait(eOtherTrait).maiRating[eLoopRating];
                                            }
                                        }

                                        if (iValue != 0)
                                        {
                                            TextVariable ratingValueText = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", buildSignedTextVariable(iValue), buildRatingLinkVariable(eLoopRating, true));

                                            using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                                            {
                                                int iNewValue = (pCharacter.getRating(eLoopRating) + ((pCharacter.isTrait(eTrait)) ? 0 : iValue));
                                                int iOldValue = (pCharacter.getRating(eLoopRating) - ((pCharacter.isTrait(eTrait)) ? iValue : 0));

                                                buildRatingHelp(subText, eLoopRating, iNewValue, iOldValue, pGame, pActivePlayer, pCharacter, pCharacter.getPlayerOpinionCharacter(), pCharacter.isLeader(), pCharacter.isLeaderSpouse(), pCharacter.isSuccessor(), pCharacter.isCourtier(), pCharacter.isClergy(), pCharacter.getCouncil(), pCharacter.isCityGovernor(), pCharacter.cityGovernor(), pCharacter.isCityAgent(), pCharacter.cityAgent(), pCharacter.isUnitGeneral(), pCharacter.unit(), pCharacter.isUnitExplorer(), bShowTotals: bShowYieldTotals);
                                                if (subText.HasContent)
                                                {
                                                    ratingValueText = buildEnclosedParenthesis(ratingValueText, subText.ToTextVariable());
                                                    bRatingValue = true;
                                                }
                                            }

                                            if (bRatingValue)
                                            {
                                                builder.Add(ratingValueText);
                                            }
                                            else
                                            {
                                                subCommaText.AddTEXT("TEXT_HELPTEXT_CONCAT_SPACE_TWO", buildSignedTextVariable(iValue), buildRatingLinkVariable(eLoopRating, true));
                                            }
                                        }
                                    }
                                }

                                if (subCommaText.HasContent)
                                {
                                    builder.Add(subCommaText.ToTextVariable());
                                }
                            }
                        }
                        else
                        {
                            using (builder.BeginScope(TextBuilder.ScopeType.COMMA))
                            {
                                for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                                {
                                    int iValue = infos().trait(eTrait).maiRating[eLoopRating] + infos().trait(eTrait).maiPermanentRating[eLoopRating];
                                    if (iValue != 0)
                                    {
                                        builder.AddTEXT("TEXT_HELPTEXT_CONCAT_SPACE_TWO", buildSignedTextVariable(iValue), buildRatingLinkVariable(eLoopRating, true));
                                    }
                                }
                            }
                        }
                    }

                    if (bRestrictions && !bRelevantOnly)
                    {
                        if (infos().trait(eTrait).mbNoMarry)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_MARRY");
                        }

                        if (infos().trait(eTrait).mbNoBirth)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_BIRTH");
                        }

                        if (infos().trait(eTrait).mbNoSuccession)
                        {
                            if ((pCharacter != null) && pCharacter.isLeader())
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_FORFEITS");
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_HEIR", TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                            }
                        }

                        if (infos().trait(eTrait).mbNoSuccessionChildren)
                        {
                            if ((pCharacter != null) && pCharacter.isLeader())
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_FORFEITS");
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_HEIR_CHILDREN");
                            }
                        }

                        if (infos().trait(eTrait).mbNoJob)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_JOB_LIST", buildAllJobsLinkVariable(pCharacter, pGame));
                        }

                        if (infos().trait(eTrait).mbNoCouncil)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_COUNCIL");
                        }

                        if (infos().trait(eTrait).mbNoGeneral)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_GENERAL", TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                        }

                        if (infos().trait(eTrait).mbNoExplorer)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_EXPLORER", TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                        }

                        if (infos().trait(eTrait).mbNoGovernor)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_GOVERNOR", TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                        }

                        if (infos().trait(eTrait).mbNoCourtier)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_COURTIER", buildCourtierLinkVariable(eGender: pCharacter != null ? pCharacter.getGender() : GenderType.NONE), TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                        }

                        if (infos().trait(eTrait).mbNoFamilyHead)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_FAMILY_HEAD");
                        }

                        if (infos().trait(eTrait).mbNoReligionHeadNew)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_RELIGION_HEAD_NEW", TEXTVAR(pCharacter != null ? pCharacter.isMale() : true));
                        }

                        if (infos().trait(eTrait).mbNoReligion)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_RELIGION");
                        }

                        if (infos().trait(eTrait).mbNoEvents)
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_TRAIT_NO_EVENTS", buildTraitLinkVariable(eTrait, pCharacter));
                        }

                        if (infos().trait(eTrait).mbStrength)
                        {
                            if (pCharacter != null)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_STRENGTH_COUNT", pCharacter.countStrengths());
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_STRENGTH");
                            }
                        }

                        if (infos().trait(eTrait).mbWeakness)
                        {
                            if (pCharacter != null)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_WEAKNESS_COUNT", pCharacter.countWeaknesses());
                            }
                            else
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_WEAKNESS");
                            }
                        }

                        if ((pCharacter != null) ? !(pCharacter.isLeader()) : true)
                        {
                            int iValue = infos().trait(eTrait).miOpinion;
                            if (iValue != 0)
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION", buildSignedTextVariable(iValue));
                            }
                        }
                    }
                }

                if (bDetails && !bRelevantOnly && bAdvancedHelp)
                {
                    using (TextBuilder subText = TextBuilder.GetTextBuilder(TextManager))
                    {
                        using (subText.BeginScope(scopeType))
                        {
                            int iDeathProb = infos().utils().modify(infos().Helpers.getTraitDieProb(eTrait, pGame), infos().trait(eTrait).miDieModifier);
                            {
                                if (iDeathProb > 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_DEATH_CHANCE", iDeathProb);
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miDieModifier;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_DIE_MODIFIER", buildSignedTextVariable(iValue, false));
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miRemoveTurns;
                                if (iValue > 0)
                                {
                                    if ((pCharacter != null) && pCharacter.isTrait(eTrait))
                                    {
                                        int iTurns = (iValue - pCharacter.getTraitTurnLength(eTrait));
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_REMOVE_TURNS_CHARACTER", iTurns, buildTurnScaleName(pGame, iTurns));
                                    }
                                    else
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_REMOVE_TURNS", iValue, buildTurnScaleName(pGame, iValue));
                                    }
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miBirthModifier;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_BIRTH_CHANCE", buildSignedTextVariable(iValue));
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miFamilyHeadModifier;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_FAMILY_HEAD_CHANCE", buildSignedTextVariable(iValue));
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miReligionHeadModifier;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_RELIGION_HEAD_CHANCE", buildSignedTextVariable(iValue));
                                }
                            }

                            int iReplaceProbability = 0;
                            for (TraitType eLoopTrait = 0; eLoopTrait < infos().traitsNum(); eLoopTrait++)
                            {
                                int iValue = infos().utils().modify(infos().trait(eTrait).maiTraitProb[eLoopTrait], -iDeathProb);
                                if (iValue > 0 && (pCharacter == null || !pCharacter.isTrait(eLoopTrait)))
                                {
                                    iReplaceProbability += infos().trait(eTrait).maiTraitProb[eLoopTrait];
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OTHER_TRAIT_CHANCE", TEXTVAR(iValue), buildTraitLinkVariable(eLoopTrait, pCharacter));
                                }
                            }

                            {
                                int iValue = infos().utils().modify(infos().trait(eTrait).miRemoveProb, -iDeathProb);
                                iValue = infos().utils().modify(iValue, -iReplaceProbability);
                                if (iValue > 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_REMOVE_CHANCE", iValue, buildTraitLinkVariable(eTrait, pCharacter));
                                }
                            }

                            for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); eLoopReligion++)
                            {
                                int iValue = infos().trait(eTrait).maiReligionOpinion[eLoopReligion];
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_RELIGION_OPINION", buildSignedTextVariable(iValue), buildReligionLinkVariable(eLoopReligion, pGame, pActivePlayer));
                                }
                            }

                            for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); eLoopReligion++)
                            {
                                int iValue = infos().trait(eTrait).maiReligionOpinionWeighted[eLoopReligion];
                                if (iValue != 0)
                                {
                                    if (pCharacter != null)
                                    {
                                        int iChange = infos().Helpers.getReligionOpinionWeightedTrait(eLoopReligion, eTrait, pCharacter.getPlayerOpinionCharacter());
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_RELIGION_OPINION_WEIGHTED_CHARACTER", buildSignedTextVariable(iChange), buildReligionLinkVariable(eLoopReligion, pGame, pActivePlayer), buildSignedTextVariable(iValue));
                                    }
                                    else
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_RELIGION_OPINION_WEIGHTED", buildSignedTextVariable(iValue), buildReligionLinkVariable(eLoopReligion, pGame, pActivePlayer));
                                    }
                                }
                            }

                            for (FamilyClassType eLoopFamilyClass = 0; eLoopFamilyClass < infos().familyClassesNum(); eLoopFamilyClass++)
                            {
                                int iValue = infos().trait(eTrait).maiFamilyClassOpinion[eLoopFamilyClass];
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_FAMILY_CLASS_OPINION", buildSignedTextVariable(iValue), buildFamilyClassLinkVariable(eLoopFamilyClass));
                                }
                            }

                            for (LawType eLoopLaw = 0; eLoopLaw < infos().lawsNum(); eLoopLaw++)
                            {
                                int iValue = infos().trait(eTrait).maiLawOpinion[eLoopLaw];
                                if (iValue > 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_LAW_OPINION", buildSignedTextVariable(iValue), buildLawLinkVariable(eLoopLaw, pCharacter?.player()));
                                }
                            }

                            for (JobType eLoopJob = 0; eLoopJob < infos().jobsNum(); eLoopJob++)
                            {
                                if (pGame != null ? pGame.checkGameContent(eLoopJob) : App.CheckContentOwnership(infos().job(eLoopJob).meGameContentRequired, infos(), null))
                                {
                                    int iValue = infos().trait(eTrait).maiJobOpinion[eLoopJob];
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_JOB_OPINION", buildSignedTextVariable(iValue), buildJobLinkVariable(eLoopJob));
                                    }
                                }
                            }

                            for (TraitType eLoopTrait = 0; eLoopTrait < infos().traitsNum(); eLoopTrait++)
                            {
                                int iValue = infos().trait(eTrait).maiTraitOpinion[eLoopTrait];
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OTHER_TRAIT_OPINION", buildSignedTextVariable(iValue), buildTraitLinkVariable(eLoopTrait, pCharacter));
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miOpinionSame;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OTHER_TRAIT_OPINION", buildSignedTextVariable(iValue), buildTraitLinkVariable(eTrait, pCharacter));
                                }
                            }

                            if ((pCharacter != null) ? pCharacter.hasFamily() : true)
                            {
                                int iValue = infos().trait(eTrait).miOpinionFamily;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_FAMILY", buildSignedTextVariable(iValue));
                                }
                            }

                            if ((pCharacter != null) ? pCharacter.isLeader() : true)
                            {
                                {
                                    int iValue = infos().trait(eTrait).miOpinionReligion;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_RELIGION", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionProximity;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_PROXIMITY", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionStrength;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_STRENGTH", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionKnowledge;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_KNOWLEDGE", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionGenerals;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_GENERALS", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionExplorers;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_EXPLORERS", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionGovernors;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_GOVERNORS", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionWonders;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_WONDERS", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionLaws;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_LAWS", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionCognomen;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_COGNOMEN", buildSignedTextVariable(iValue));
                                    }
                                }

                                if ((pCharacter != null) ? (pCharacter.getTeam() != pActivePlayer.getTeam()) : true)
                                {
                                    int iValue = infos().trait(eTrait).miOpinionTrades;
                                    if (iValue != 0)
                                    {
                                        subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_TRADES", buildSignedTextVariable(iValue));
                                    }
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miOpinionReligionWorld;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_RELIGION_WORLD", buildSignedTextVariable(iValue));
                                }
                            }

                            {
                                int iValue = infos().trait(eTrait).miOpinionReligionPagan;
                                if (iValue != 0)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_OPINION_RELIGION_PAGAN", buildSignedTextVariable(iValue));
                                }
                            }

                            {
                                using (TextBuilder ratingText = TextBuilder.GetTextBuilder(TextManager))
                                {
                                    for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                                    {
                                        int iValue = infos().trait(eTrait).maiRatingFallback[eLoopRating];
                                        if (iValue != 0)
                                        {
                                            TextVariable ratingValueText = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", buildSignedTextVariable(iValue), buildRatingLinkVariable(eLoopRating, true));
                                            ratingText.Add(ratingValueText);
                                        }
                                    }

                                    if (ratingText.HasContent)
                                    {
                                        using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_RATING_FALLBACK_LIST")))
                                        {
                                            subText.Add(ratingText.ToTextVariable());
                                        }
                                    }
                                }
                            }

                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TRAIT_INCLUDED_IN")))
                            {
                                for (SubjectType eLoopSubject = 0; eLoopSubject < infos().subjectsNum(); eLoopSubject++)
                                {
                                    if (infos().subject(eLoopSubject).maeTraitAny.Contains(eTrait) && !infos().subject(eLoopSubject).mbHidden)
                                    {
                                        subText.Add(buildSubjectTraitAnyLinkVariable(eLoopSubject, pCharacter != null ? pCharacter.getGender() : GenderType.NONE));
                                    }
                                }
                            }

                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TRAIT_EXCUDED_FROM")))
                            {
                                for (SubjectType eLoopSubject = 0; eLoopSubject < infos().subjectsNum(); eLoopSubject++)
                                {
                                    if (infos().subject(eLoopSubject).maeTraitNone.Contains(eTrait) && !infos().subject(eLoopSubject).mbHidden)
                                    {
                                        subText.Add(buildSubjectTraitNoneLinkVariable(eLoopSubject, pCharacter != null ? pCharacter.getGender() : GenderType.NONE));
                                    }
                                }
                            }

                            using (subText.BeginScope(TextBuilder.ScopeType.COMMA, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_TRAIT_COMMON_FAMILYCLASS")))
                            {
                                if (pActivePlayer == null || !pActivePlayer.hasNation())
                                {
                                    for (FamilyClassType eLoopFamilyClass = 0; eLoopFamilyClass < infos().familyClassesNum(); ++eLoopFamilyClass)
                                    {
                                        int iValue = infos().familyClass(eLoopFamilyClass).maiTraitDie[eTrait];
                                        if (iValue > 1)
                                        {
                                            subText.Add(buildFamilyClassLinkVariable(eLoopFamilyClass));
                                        }
                                    }
                                }
                                else if (pGame != null)
                                {
                                    for (FamilyType eLoopFamily = 0; eLoopFamily < infos().familiesNum(); ++eLoopFamily)
                                    {
                                        if (infos().family(eLoopFamily).mabNation[(int)(pActivePlayer.getNation())])
                                        {
                                            FamilyClassType eFamilyClass = pGame.getFamilyClass(eLoopFamily);
                                            if (eFamilyClass != FamilyClassType.NONE)
                                            {
                                                int iValue = infos().familyClass(eFamilyClass).maiTraitDie[eTrait];
                                                if (iValue > 1)
                                                {
                                                    subText.Add(buildFamilyLinkVariable(eLoopFamily, pGame));
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            {
                                if (infos().trait(eTrait).meEncyclopediaCharacter != CharacterType.NONE)
                                {
                                    subText.AddTEXT("TEXT_HELPTEXT_TRAIT_UNIQUE_CHARACTER", buildCharacterPresetLinkVariable(infos().trait(eTrait).meEncyclopediaCharacter));
                                }
                            }

                            if (pGame != null)
                            {
                                using (subText.BeginScope(TextBuilder.ScopeType.INDENTED_BULLET, insertBeforeScopeText: TEXTVAR_TYPE("TEXT_HELPTEXT_NOTABLE_CHARACTERS")))
                                {
                                    using (var hashSetScope = CollectionCache.GetHashSetScoped<Character>())
                                    using (var charListScoped = CollectionCache.GetListScoped<Character>())
                                    {
                                        HashSet<Character> spCharactersAdded = hashSetScope.Value;

                                        pGame.getActiveCharacters(charListScoped.Value);

                                        for (int iPass = 0; iPass < InfoHelpers.NOTABLE_CHARACTER_PASSES; iPass++)
                                        {
                                            foreach (Character pLoopCharacter in charListScoped.Value)
                                            {
                                                if (!(spCharactersAdded.Contains(pLoopCharacter)))
                                                {
                                                    if (pLoopCharacter.hasContact(pActivePlayer.getTeam()))
                                                    {
                                                        if (infos().Helpers.isNotableCharacter(pLoopCharacter, iPass, pActivePlayer))
                                                        {
                                                            if (pLoopCharacter.isTrait(eTrait))
                                                            {
                                                                subText.Add(buildCharacterLinkVariable(pLoopCharacter, pActivePlayer));
                                                            }

                                                            spCharactersAdded.Add(pLoopCharacter);
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
                            buildDividerText(builder);
                            builder.Add(subText.ToTextVariable());
                        }
                    }
                }

                if (!bRelevantOnly && infos().trait(eTrait).meDescription != TextType.NONE)
                {
                    buildDividerText(builder);

                    using (buildColorTextPositiveScope(builder))
                    {
                        builder.AddTEXT(infos().trait(eTrait).meDescription);
                    }
                }

                if (bEncyclopedia && infos().trait(eTrait).meGameContentRequired != GameContentType.NONE)
                {
                    buildDividerText(builder);
                    for (AdditionalContentType eContent = 0; eContent < infos().additionalContentsNum(); eContent++)
                    {
                        if (infos().additionalContent(eContent).maeGameContent.Contains(infos().trait(eTrait).meGameContentRequired))
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_ADDED_FROM_DLC", TEXTVAR_TYPE(infos().additionalContent(eContent).meName));
                        }
                    }
                }

                return builder;
            }
        }

    }
}