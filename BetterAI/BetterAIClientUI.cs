using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TenCrowns.AppCore;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using UnityEngine;
using UnityEngine.UI;
using static TenCrowns.GameCore.Text.TextExtensions;
using static UnityEngine.UI.DefaultControls;
using Constants = TenCrowns.GameCore.Constants;
using Enum = System.Enum;

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
        protected override void updateCitySelection(City pSelectedCity, bool bUpdateActions)
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

                            //If discontent, + means more discontent, - means more happiness. This was already clear before
                            //TextVariable rateText;
                            if (eLoopYield == Infos.Globals.HAPPINESS_YIELD)
                            {
                                //if (iRate >= 0)
                                //{
                                //    rateText = HelpText.buildYieldIconTextVariable(eLoopYield, iRate, false, false, Constants.YIELDS_MULTIPLIER);
                                //}
                                //else
                                //{
                                //    rateText = HelpText.buildYieldIconTextVariable(Infos.Globals.DISCONTENT_YIELD, -iRate, false, false, Constants.YIELDS_MULTIPLIER);
                                //}
                                //
                                //rateText = HelpText.buildColorTextOptionalVariable(rateText, iRate > 0, (iRate != 0));

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
                            //else
                            //{
                            //    rateText = HelpText.buildYieldTextVariable(iRate, true, false, Constants.YIELDS_MULTIPLIER);
                            //    rateText = HelpText.buildColorTextOptionalVariable(rateText, !(Infos.Helpers.yieldWarning(eDisplayYield, iRate)), (iRate != 0));
                            //}

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

                    using (var builder = TextBuilder.GetTextBuilder(TextManager))
                    {
                        //using var profileScope = new UnityProfileScope("ClientUI.updateCitySelection.updateLuxuryList");

                        bool bCanManageLuxuries = false;
                        for (ResourceType eLoopResource = 0; eLoopResource < Infos.resourcesNum(); eLoopResource++)
                        {
                            bool hasLuxury = pSelectedCity.isLuxury(eLoopResource);
                            bCanManageLuxuries |= pActivePlayer.canTradeCityLuxury(pSelectedCity, eLoopResource, !hasLuxury, false);

                            if (hasLuxury)
                            {
                                builder.Add(HelpText.buildLuxuryIconLinkVariable(eLoopResource, pSelectedCity.getID()), skipSeparator: true);
                            }
                        }
                        if (!builder.HasContent)
                        {
                            builder.AddTEXT("TEXT_MANAGE_LUXURIES");
                        }
                        mSelectedPanel.SetKey("City-Luxuries", builder.ProfiledToString());
                        mSelectedPanel.SetBool("City-CanSendLuxuries", bCanManageLuxuries);
                    }
                }

                lastSelectedCityID = pSelectedCity.getID();
                updateExpansionPreview(null, null, false);

                if (bUpdateActions)
                {
                    makeDirty(DirtyType.ACTION_PANEL);
                }
            }
        }
        //copy-paste END

        //lines 10893-11081
        protected override void updateReligionSelection(ReligionType eReligion, bool bUpdateActions)
        {
            //using var profileScope = new UnityProfileScope("ClientUI.updateReligionSelection");

            //damn protection levels
            int maxSelectedCharacterButtonsCount = (int)base.GetType().GetField("maxSelectedCharacterButtonsCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);
            int maxSelectedCityButtonsCount = (int)base.GetType().GetField("maxSelectedCityButtonsCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this);

            if (eReligion != ReligionType.NONE)
            {
                Player pActivePlayer = ClientMgr.activePlayer();
                InfoReligion infoReligion = Infos.religion(eReligion);

                string name = getReligionName(eReligion);

                if (pActivePlayer.getStateReligion() == eReligion)
                {
                    mSelectedPanel.SetTEXT("Name", TextManager, HelpText.buildEnclosedParenthesis(TEXTVAR(name), HelpText.buildStateReligionLinkVariable(eReligion, pActivePlayer)));
                }
                else
                {
                    mSelectedPanel.SetKey("Name", name);
                }

                bool hasHead = Game.hasReligionHead(eReligion);
                if (hasHead)
                {
                    mSelectedPanel.SetKey("CharacterLabel", "TEXT_UI_RELIGION_HEAD");
                    mSelectedPanel.SetInt("HighlightCharacter-ID", Game.getReligionHeadID(eReligion));
                }
                mSelectedPanel.SetBool("HighlightCharacter-Inactive", !hasHead);

                mSelectedPanel.SetKey("Rename-Data", infoReligion.SafeTypeString());
                mSelectedPanel.SetBool("Rename-IsActive", Game.getReligionFounder(eReligion) == pActivePlayer.getPlayer());
                mSelectedPanel.SetKey("Image", infoReligion.mzIconName);
                mSelectedPanel.SetKey("Image-Color", getHolyCityHexString(eReligion));
                using (var iconScope = new WidgetDataScope(nameof(ItemType.HELP_LINK), nameof(LinkType.HELP_RELIGION), eReligion.ToStringCached()))
                {
                    mSelectedPanel.ItemType = iconScope.Type;
                    mSelectedPanel.DataSB = iconScope.DataList;
                }

                int iOpinion = pActivePlayer.getReligionOpinionRate(eReligion);
                OpinionReligionType eOpinion = pActivePlayer.getReligionOpinion(eReligion);
                string zColor = ColorManager.GetColorHex(Infos.opinionReligion(eOpinion).meColor);
                TextVariable opinionText = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", HelpText.buildOpinionReligionLinkVariable(eOpinion, eReligion, pActivePlayer.getPlayer()), HelpText.buildSignedTextVariable(iOpinion));
                mSelectedPanel.SetTEXT("Info", TextManager, HelpText.buildColorTextVariable(opinionText, zColor));
                mSelectedPanel.SetBool("Info-IsActive", true);

                bool bCanHaveTheology = infoReligion.mePaganNation == NationType.NONE;
                if (bCanHaveTheology)
                {
                    int iMaxTier = -1;
                    for (TheologyType eLoopTheology = 0; eLoopTheology < Infos.theologiesNum(); eLoopTheology++)
                    {
                        int iTier = Infos.theology(eLoopTheology).miTier;
                        if (iTier > iMaxTier)
                            iMaxTier = iTier;
                    }

                    for (int iLoopTier = 0; iLoopTier <= iMaxTier; iLoopTier++)
                    {
                        UIAttributeTag theologyTag = mSelectedPanel.GetSubTag("-ListEntry", iLoopTier);
                        theologyTag.SetBool("ShowType", true);
                        theologyTag.SetKey("TierLabel", TEXT("TEXT_UI_SELECTED_RELIGION_THEOLOGY", TEXTVAR(iLoopTier + 1)));
                        theologyTag.SetTEXT("Label", TextManager, HelpText.buildAdoptTheologyTierLinkVariable(Game, eReligion, iLoopTier, pActivePlayer));
                    }
                    mSelectedPanel.SetKey("ListEntries-Label", "TEXT_UI_SELECTED_RELIGION_THEOLOGIES");
                    mSelectedPanel.SetInt("ListEntries-Count", iMaxTier + 1);
                }
                mSelectedPanel.SetBool("ListEntries-IsVisible", bCanHaveTheology);

                int memberIndex = 0;
                {
                    using (var familyScope = CollectionCache.GetStringBuilderScoped())
                    {
                        bool bAnyFamily = false;
                        for (FamilyType eLoopFamily = 0; eLoopFamily < Infos.familiesNum(); eLoopFamily++)
                        {
                            if (pActivePlayer.isFamilyStarted(eLoopFamily) && pActivePlayer.getFamilyReligion(eLoopFamily) == eReligion)
                            {
                                HelpText.buildFamilyCrestLink(familyScope.Value, eLoopFamily, Game).Append("  ");
                                bAnyFamily = true;
                            }
                        }

                        if (bAnyFamily)
                        {
                            UIAttributeTag entryTag = mSelectedPanelMembers.GetSubTag("-ListEntry", memberIndex);
                            entryTag.SetKey("TypeLabel", "TEXT_UI_SELECTED_RELIGION_FOLLOWERS_FAMILIES");
                            entryTag.SetKey("DescLabel", familyScope.Value.ProfiledToString());
                            memberIndex++;
                        }
                    }
                    
                    
                    using (var nationScope = CollectionCache.GetStringBuilderScoped())
                    {
                        bool bAnyNation = false;
                        foreach (Player pPlayer in Game.getPlayers())
                        {
                            if (pPlayer.isAlive())
                            {
                                if (pPlayer.getStateReligion() == eReligion)
                                {
                                    HelpText.buildPlayerCrestLink(nationScope.Value, pPlayer.getPlayer(), Game, pActivePlayer).Append("  ");
                                    bAnyNation = true;
                                }
                            }
                        }
                        if (bAnyNation)
                        {
                            UIAttributeTag entryTag = mSelectedPanelMembers.GetSubTag("-ListEntry", memberIndex);
                            entryTag.SetKey("TypeLabel", "TEXT_UI_SELECTED_RELIGION_FOLLOWERS_NATIONS");
                            entryTag.SetKey("DescLabel", nationScope.Value.ProfiledToString());
                            memberIndex++;
                        }
                    }

                    
                    using (var tribeScope = CollectionCache.GetStringBuilderScoped())
                    {
                        bool bAnyTribe = false;
                        for (TribeType eLoopTribe = 0; eLoopTribe < Infos.tribesNum(); eLoopTribe++)
                        {
                            if (Game.isDiplomacyTribeAlive(eLoopTribe))
                            {
                                if (Game.getTribeReligion(eLoopTribe) == eReligion)
                                {
                                    HelpText.buildTribeCrestLink(tribeScope.Value, eLoopTribe, Game).Append("  ");
                                    bAnyTribe = true;
                                }
                            }
                        }
                        if (bAnyTribe)
                        {
                            UIAttributeTag entryTag = mSelectedPanelMembers.GetSubTag("-ListEntry", memberIndex);
                            entryTag.SetKey("TypeLabel", "TEXT_UI_SELECTED_RELIGION_FOLLOWERS_TRIBES");
                            entryTag.SetKey("DescLabel", tribeScope.Value.ProfiledToString());
                            memberIndex++;
                        }
                    }

                    mSelectedPanel.SetInt("Members-ListEntries-Count", memberIndex);
                }

                int numCharacters = 0;
                using (var characterScope = CollectionCache.GetListScoped<int>())
                {
                    List<int> activeCharacters = characterScope.Value;
                    pActivePlayer.getActiveCharacters(activeCharacters);

                    foreach (int charID in activeCharacters)
                    {
                        Character pCharacter = Game.character(charID);
                        if (pCharacter.getReligion() == eReligion && !pCharacter.isReligionHead())
                        {
                            UIAttributeTag characterTag = mSelectedPanelMembers.GetSubTag("-Character", numCharacters);
                            characterTag.SetInt("ID", charID);
                            characterTag.IsActive = true;
                            numCharacters++;
                        }
                    }

                    if (numCharacters > maxSelectedCharacterButtonsCount)
                    {
                        maxSelectedCharacterButtonsCount = numCharacters;
                        mSelectedPanelMembers.SetInt("Characters-Count", numCharacters);
                    }
                    else
                    {
                        for (int i = numCharacters; i < maxSelectedCharacterButtonsCount; i++)
                        {
                            UIAttributeTag characterTag = mSelectedPanelMembers.GetSubTag("-Character", i);
                            characterTag.IsActive = false;
                        }
                    }
                }
                mSelectedPanelMembers.SetBool("Characters-IsVisible", numCharacters > 0);

                int numCities = 0;
/*####### Better Old World AI - Base DLL #######
  ### Rel. Improvements in City list   START ###
  ##############################################*/
                using (var ImprovementTypeListScope = CollectionCache.GetListScoped<ImprovementType>())
                using (var ImprovementClassCountScope = CollectionCache.GetDictionaryScoped<ImprovementClassType, int>())
                using (var ImprovementShortNameScope = CollectionCache.GetDictionaryScoped<ImprovementType, string>())
                {
                    List<ImprovementType> ImprovementTypes = ImprovementTypeListScope.Value;
                    Dictionary<ImprovementClassType, int> ImprovementClassCounts = ImprovementClassCountScope.Value;
                    Dictionary<ImprovementType, string> ImprovementShortNames = ImprovementShortNameScope.Value;

                    for (ImprovementType eLoopImprovement = 0; eLoopImprovement < Infos.improvementsNum(); eLoopImprovement++)
                    {
                        if ((Game?.getImprovementReligionSpread(eLoopImprovement) ?? Infos.improvement(eLoopImprovement).meReligionSpread) == eReligion || Infos.improvement(eLoopImprovement).meReligionPrereq == eReligion)
                        {
                            ImprovementTypes.Add(eLoopImprovement);
                            if (Infos.improvement(eLoopImprovement).meClass != ImprovementClassType.NONE)
                            {
                                if (ImprovementClassCounts.ContainsKey(Infos.improvement(eLoopImprovement).meClass))
                                {
                                    ImprovementClassCounts.Add(Infos.improvement(eLoopImprovement).meClass, 1);
                                }
                                else
                                {
                                    ImprovementClassCounts[Infos.improvement(eLoopImprovement).meClass] += 1;
                                }
                            }
                        }
                    }

                    foreach (ImprovementType eLoopImprovement in ImprovementTypes)
                    {
                        string shortName;

                        if (Infos.improvement(eLoopImprovement).meClass != ImprovementClassType.NONE && ImprovementClassCounts[Infos.improvement(eLoopImprovement).meClass] == 1)
                        {
                            //use improvement class name
                            shortName = TEXT(Infos.improvementClass(Infos.improvement(eLoopImprovement).meClass).mName);

                        }
                        else
                        {
                            //use improvement name
                            shortName = TEXT(Infos.improvement(eLoopImprovement).mName);
                        }

                        //if there are multiple words separated by spaces in the name, use the last word. Shrine of Poseidon: "P"
                        int iStartSubString = 0;
                        if (shortName.LastIndexOf(" ") != -1)
                        {
                            iStartSubString = shortName.LastIndexOf(" ") + 1;
                        }
                        shortName = shortName.Substring(iStartSubString, 1);

                        ImprovementShortNames.Add(eLoopImprovement, shortName);
                    }
/*####### Better Old World AI - Base DLL #######
  ### Rel. Improvements in City list     END ###
  ##############################################*/

                    foreach (int iCity in pActivePlayer.getCities())
                    {
                        City pCity = Game.city(iCity);
                        if (pCity.isReligion(eReligion))
                        {
                            UIAttributeTag cityTag = mSelectedPanelMembers.GetSubTag("-City", numCities);
/*####### Better Old World AI - Base DLL #######
  ### Rel. Improvements in City list   START ###
  ##############################################*/
                            //cityTag.SetTEXT("Name", TextManager, HelpText.buildCityLinkVariable(pCity, pActivePlayer, true, false));
                            //cityTag.SetTEXT("Label", TextManager, HelpText.buildCityTextVariable(pCity, pActivePlayer, true));
                            cityTag.SetTEXT("Label", TextManager, ((BetterAIHelpText)HelpText).buildCityLinkVariableWithReligiousImprovements(Game, pCity, pActivePlayer, eReligion, ImprovementTypes, ImprovementShortNames, bCrest: true, bAddColor: false));
/*####### Better Old World AI - Base DLL #######
  ### Rel. Improvements in City list     END ###
  ##############################################*/

                            cityTag.IsActive = true;
                            using (var dataScope = new WidgetDataScope(nameof(ItemType.HELP_LINK), nameof(LinkType.HELP_CITY_SELECTED_RELIGION), eReligion.ToStringCached(), pCity.getID().ToStringCached()))
                            {
                                cityTag.DataSB = dataScope.DataList;
                            }
                            numCities++;
                        }
                    }

                    if (numCities > maxSelectedCityButtonsCount)
                    {
                        maxSelectedCityButtonsCount = numCities;
                        mSelectedPanelMembers.SetInt("Cities-Count", numCities);
                    }
                    else
                    {
                        for (int i = numCities; i < maxSelectedCityButtonsCount; i++)
                        {
                            UIAttributeTag cityTag = mSelectedPanelMembers.GetSubTag("-City", i);
                            cityTag.IsActive = false;
                        }
                    }
                }
                mSelectedPanelMembers.SetBool("Cities-IsVisible", numCities > 0);

                bool bAnyFollowers = memberIndex > 0 || numCharacters > 0 || numCities > 0;
                mSelectedPanel.SetBool("Members-IsVisible", bAnyFollowers);

                if (bUpdateActions)
                {
                    makeDirty(DirtyType.ACTION_PANEL);
                }

                mSelectedPanel.SetBool("ScrollRect-IsVisible", bAnyFollowers || bCanHaveTheology);
            }
        }



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

                    //{
                    //    ImprovementType eBestImprovement = ImprovementType.NONE;
                    //    Task.Run(() =>
                    //    {
                    //        lock (ClientMgr.TaskLock)
                    //        {
                    //            if (!pActivePlayer.AI.isBestTileImprovementsCached(pTile.cityTerritory()))
                    //            {
                    //                pActivePlayer.AI.cacheCityImprovementValues(pTile.cityTerritory());
                    //            }
                    //            pActivePlayer.AI.getBestImprovement(pTile, pTile.cityTerritory(), ref eBestImprovement);
                    //        }
                    //        selectedWorkerRecommendedImprovementCallback(eBestImprovement);
                    //    });
                    //}

                    //List<ResourceType> cityResources = new List<ResourceType>(); //only those without improvement

                    //List<ResourceType> cityResourcesScope = new List<ResourceType>(); //only those without improvement
                    using (var remainingTerrainTargetScope = CollectionCache.GetListScoped<TerrainTargetType>())
                    using (var cityTerrainTargetSetScoped = CollectionCache.GetHashSetScoped<TerrainTargetType>())
                    using (var cityResourcesScope = CollectionCache.GetHashSetScoped<ResourceType>())
                    {
                        {
                            for (TerrainTargetType eLoopTerrainTarget = 0; eLoopTerrainTarget < Infos.terrainTargetsNum(); eLoopTerrainTarget++)
                            {
                                remainingTerrainTargetScope.Value.Add(eLoopTerrainTarget);
                            }
                            //make the resource list
                            foreach (int iTileID in pCityTerritory.getTerritoryTiles())
                            {
                                Tile tile = ClientMgr.GameClient.tile(iTileID);

                                ResourceType eTileResource = tile.getResource();
                                ImprovementType eTileImprovement = tile.getImprovement();

                                if (eTileResource != ResourceType.NONE)
                                {
                                    if (eTileImprovement == ImprovementType.NONE || Infos.Helpers.isImprovementResourceValid(eTileImprovement, eTileResource))
                                    {
                                        continue;
                                    }

                                    cityResourcesScope.Value.Add(eTileResource);
                                }

                                if (eTileImprovement != ImprovementType.NONE || eTileResource != ResourceType.NONE)
                                {
                                    for (int i = remainingTerrainTargetScope.Value.Count - 1; i >= 0; i--)
                                    {
                                        if (tile.isTerrainTarget(remainingTerrainTargetScope.Value[i]))
                                        {
                                            cityTerrainTargetSetScoped.Value.Add(remainingTerrainTargetScope.Value[i]);
                                            remainingTerrainTargetScope.Value.RemoveAt(i);
                                        }
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

                            else if (pUnit.canBuildImprovement(pTile, eLoopImprovement, pActivePlayer, isBuyGoods(), bTestEnabled: false, bTestOrders: false, bTestGoods: false))
                            {
                                bShowButton = true;
                            }
                            else
                            {
                                if (pActivePlayer.canStartImprovement(eLoopImprovement, null))
                                {
                                    if (!(pCityTerritory.canCityHaveImprovement(eLoopImprovement, bTestEnabled: false)))
                                    {
                                        continue;
                                    }

                                    //check valids, including resources (resources have to be in the list)
                                    {

                                        if (!(pImprovementInfo.mbCityValid))
                                        {
                                            //bool bAnyValid = false;

                                            if (pImprovementInfo.mbCityValid)
                                            {
                                                //bShowButton = true;
                                                aeImprovements.Add(eLoopImprovement);
                                                continue;
                                            }

                                            if (pImprovementInfo.mbHolyCityValid && pCityTerritory.isReligionHolyCityAny())
                                            {
                                                //bShowButton = true;
                                                aeImprovements.Add(eLoopImprovement);
                                                continue;
                                            }

                                            //for (int i = 0; i < pImprovementInfo.maeTerrainValid.Count; i++)
                                            foreach (TerrainTargetType eLoopTerrainTarget in pImprovementInfo.maeTerrainValid)
                                            {
                                                if (cityTerrainTargetSetScoped.Value.Contains(eLoopTerrainTarget))
                                                {
                                                    //bShowButton = true;
                                                    aeImprovements.Add(eLoopImprovement);
                                                    continue;
                                                }
                                            }

                                            ImprovementClassType eImprovementClass = pImprovementInfo.meClass;
                                            if (eImprovementClass != ImprovementClassType.NONE)
                                            {
                                                InfoImprovementClass pImprovementClassInfo = Infos.improvementClass(eImprovementClass);

                                                foreach (ResourceType cityResource in cityResourcesScope.Value)
                                                {
                                                    if (pImprovementClassInfo.mabResourceValid[(int)cityResource])
                                                    {
                                                        bShowButton = true;
                                                        aeImprovements.Add(eLoopImprovement);
                                                        break;
                                                    }
                                                }
                                                if (bShowButton) continue;

                                                if (pImprovementClassInfo.mbAdjacentValid && pCityTerritory.getImprovementClassCount(eImprovementClass) > 0)
                                                {
                                                    //bShowButton = true;
                                                    aeImprovements.Add(eLoopImprovement);
                                                    continue;
                                                }

                                            }

                                            //if (!bAnyValid)
                                            //{
                                            //    continue;
                                            //}


                                        }

                                    }

                                    //bShowButton = true;
                                }
                            }

                            //if (bShowButton) aeImprovements.Add(eLoopImprovement);
                        }
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


        //lines 16872-17142
        protected override void updateCharacters(bool updateCharacterList = true)
        {
            if (((BetterAIInfoGlobals)Infos.Globals).BAI_ALT_CHARACTER_SORT == 0)
            {
                base.updateCharacters(updateCharacterList);
                return;
            }
            else
            {

                //using (new UnityProfileScope("ClientUI.updateCharacters"))
                {
                    Player pActivePlayer = ClientMgr.activePlayer();
                    PlayerType eActivePlayer = pActivePlayer.getPlayer();
                    miNumCouncil = 0;

                    if (pActivePlayer.hasLeader())
                    {
                        UIAttributeTag leaderTag = mPlayerFamily.GetSubTag("-CurrentLeader");
                        UIAttributeTag characterTabTag = UI.GetUIAttributeTag("Court");
                        Character playerLeader = pActivePlayer.leader();
                        updateCharacterSlotData(leaderTag, playerLeader, RoleType.LEADER, playerLeader != null);

                        int iLivingSpouses = 0;
                        for (int iLoopSpouse = 0; iLoopSpouse < playerLeader.getNumSpouses(); iLoopSpouse++)
                        {
                            Character pLoopCharacter = playerLeader.getSpouseAtIndex(iLoopSpouse);

                            if (pLoopCharacter.isAlive())
                            {
                                UIAttributeTag spouseSlotTag = leaderTag.GetSubTag("-Spouse", iLivingSpouses);
                                updateCharacterSlotData(spouseSlotTag, pLoopCharacter, RoleType.SPOUSE, (pLoopCharacter != null));
                                iLivingSpouses++;
                                miNumCouncil++;
                            }
                        }
                        characterTabTag.SetInt("Spouse-Count", Math.Max(iLivingSpouses, 1));
                        characterTabTag.SetBool("Spouse-IsActive", iLivingSpouses > 0);

                        if (iLivingSpouses == 0)
                        {
                            UIAttributeTag spouseSlotTag = leaderTag.GetSubTag("-Spouse", 0);
                            updateCharacterSlotData(spouseSlotTag, null, RoleType.SPOUSE, false);
                        }

                        //Heir
                        {
                            Character pNextLeader = pActivePlayer.heir();
                            UIAttributeTag heirSlotTag = leaderTag.GetSubTag("-Heir");
                            updateCharacterSlotData(heirSlotTag, pNextLeader, RoleType.HEIR, (pNextLeader != null));
                            miNumCouncil++;
                        }

                        //Council
                        {
                            for (CouncilType eLoopCouncil = 0; eLoopCouncil < Infos.councilsNum(); eLoopCouncil++)
                            {
                                if (!(Infos.council(eLoopCouncil).mbDisable))
                                {
                                    UIAttributeTag councilListTag = leaderTag.GetSubTag("-CouncilList", (int)eLoopCouncil);
                                    Character pCharacter = pActivePlayer.councilCharacter(eLoopCouncil);

                                    if (pCharacter != null && pCharacter.isDead() && pCharacter.getDeadCouncil() == eLoopCouncil)
                                        pCharacter = null;

                                    updateCharacterSlotData(councilListTag, pCharacter, RoleType.COUNCIL, pActivePlayer.isCouncilUnlock(eLoopCouncil), eLoopCouncil);
                                    maiCouncilMembers[(int)eLoopCouncil] = pCharacter?.getID() ?? -1;
                                }
                                else
                                {
                                    leaderTag.GetSubTag("-CouncilList", (int)eLoopCouncil).IsActive = false;
                                }

                            }
                            leaderTag.SetInt("CouncilList-Count", (int)Infos.councilsNum());
                        }

                        mCharacters.SetTEXT("CurrentFilter", TextManager, GetCharacterListFilterName(currentCharListFilter));

                        if (updateCharacterList)
                        {
                            //Heirs
                            {
                                int numHeirs = 0;
                                foreach (int iLoopSuccession in pActivePlayer.getSuccession())
                                {
                                    Character pLoopSuccession = Game.character(iLoopSuccession);

                                    if (pLoopSuccession.isAlive())
                                    {
                                        UIAttributeTag currentLeaderHeirListTag = characterTabTag.GetSubTag("-Heir", numHeirs);
                                        currentLeaderHeirListTag.SetKey("State", GetCharacterCardBG(iLoopSuccession));
                                        currentLeaderHeirListTag.SetInt("ID", iLoopSuccession);
                                        UI.GetUIAttributeTag("Character", iLoopSuccession).SetKey("PlaceInLine-Label", (numHeirs + 1).ToStringCached());
                                        numHeirs++;
                                    }
                                }
                                characterTabTag.SetInt("Heir-Count", numHeirs);
                                characterTabTag.SetBool("Heir-IsActive", numHeirs > 0);
                            }

                            //Court
                            {
                                int numCourtiers = 0;

                                using (var charListScoped = CollectionCache.GetListScoped<int>())
                                {
                                    pActivePlayer.getActiveCharacters(charListScoped.Value);

                                    foreach (int iLoopCharacter in charListScoped.Value)
                                    {
                                        Character pLoopCharacter = Game.character(iLoopCharacter);

                                        if (pLoopCharacter.isCourtier() && !pLoopCharacter.isLeaderOrSpouseOrHeir())
                                        {
                                            UIAttributeTag currentLeaderCourtierListTag = characterTabTag.GetSubTag("-Courtier", numCourtiers);
                                            currentLeaderCourtierListTag.SetKey("State", GetCharacterCardBG(iLoopCharacter));
                                            currentLeaderCourtierListTag.SetInt("ID", iLoopCharacter);
                                            numCourtiers++;
                                        }
                                    }
                                }

                                characterTabTag.SetInt("Courtier-Count", numCourtiers);
                                characterTabTag.SetBool("Courtier-IsActive", numCourtiers > 0);
                            }

                            //Clergy
                            {
                                int numClergy = 0;

                                using (var charListScoped = CollectionCache.GetListScoped<int>())
                                {
                                    pActivePlayer.getActiveCharacters(charListScoped.Value);

                                    foreach (int iLoopCharacter in charListScoped.Value)
                                    {
                                        Character pLoopCharacter = Game.character(iLoopCharacter);

                                        if (pLoopCharacter.isClergy() && !pLoopCharacter.isLeaderOrSpouseOrHeir())
                                        {
                                            UIAttributeTag currentLeaderClergyListTag = characterTabTag.GetSubTag("-Clergy", numClergy);
                                            currentLeaderClergyListTag.SetKey("State", GetCharacterCardBG(iLoopCharacter));
                                            currentLeaderClergyListTag.SetInt("ID", iLoopCharacter);
                                            numClergy++;
                                        }
                                    }
                                }

                                characterTabTag.SetInt("Clergy-Count", numClergy);
                                characterTabTag.SetBool("Clergy-IsActive", numClergy > 0);
                            }

                            //using (new UnityProfileScope("ClientUI.updateCharacters.addCharacterFamily"))
                            {

/*####### Better Old World AI - Base DLL #######
  ### Alternative Character Sorting    START ###
  ##############################################*/
                                //using (var checkedIDSetScoped = CollectionCache.GetHashSetScoped<int>())
                                using (var charIDListScoped = CollectionCache.GetListScoped<int>())
                                using (var ordererIDListScoped = CollectionCache.GetListScoped<int>())
                                {
                                    //HashSet<int> checkedIDSet = checkedIDSetScoped.Value;
                                    List<int> charIDList = charIDListScoped.Value;
                                    List<int> orderedIDList = ordererIDListScoped.Value;

                                    pActivePlayer.getActiveCharacters(charIDList);
                                    //pActivePlayer.getActiveCharacters(orderedIDList);


                                    //charIDList.Sort(CompareAges);
                                    //
                                    //int CompareAges(int charID, int otherCharID)
                                    //{
                                    //    int charAge = Game.character(charID).getAge();
                                    //    int otherCharAge = Game.character(otherCharID).getAge();
                                    //
                                    //    if (charAge != otherCharAge)
                                    //    {
                                    //        return otherCharAge - charAge;
                                    //    }
                                    //
                                    //    return charID - otherCharID;
                                    //}

                                    if (currentCharListFilter == CharacterListFilterType.NOTABLE)
                                    {
                                        foreach (int iLoopCharacter in charIDList)
                                        {
                                            Character pLoopCharacter = Game.character(iLoopCharacter);
                                            if (pLoopCharacter.isLeader())
                                            {
                                                orderedIDList.Insert(0, iLoopCharacter);
                                            }
                                            else if (pLoopCharacter.isPinned(eActivePlayer))
                                            {
                                                int iInsertIndex = orderedIDList.Count;
                                                for (int i = orderedIDList.Count - 1; i >= 0; i--)
                                                {
                                                    if (Game.character(iLoopCharacter).getPinnedTurn(eActivePlayer) <= pLoopCharacter.getPinnedTurn(eActivePlayer))
                                                    {
                                                        break;
                                                    }

                                                    iInsertIndex--;
                                                }
                                                orderedIDList.Insert(iInsertIndex, iLoopCharacter);
                                            }
                                        }
                                    }
                                    else
                                    {

                                        foreach (int iLoopCharacter in charIDList)
                                        {
                                            Character pLoopCharacter = Game.character(iLoopCharacter);
                                            if (validCharacterFamily(pLoopCharacter))
                                            {
                                                orderedIDList.Add(iLoopCharacter);
                                            }
                                        }

                                        orderedIDList.Sort(OrderCharacters);

                                        int OrderCharacters(int charID, int otherCharID)
                                        {
                                            Character first = Game.character(charID);
                                            Character second = Game.character(otherCharID);

                                            //#1 leader
                                            if (first.isLeader() /* && !second.isLeader() */)
                                            {
                                                return -1;
                                            }
                                            else
                                            {
                                                if (second.isLeader() /* && !first.isLeader()*/)
                                                {
                                                    return 1;
                                                }
                                            }

                                            //if (currentCharListFilter == CharacterListFilterType.NOTABLE)
                                            //{
                                            //    if (first.getPinnedTurn() != second.getPinnedTurn())
                                            //    {
                                            //        return first.getPinnedTurn() - second.getPinnedTurn();
                                            //    }
                                            //}

                                            if (currentCharListFilter == CharacterListFilterType.NONE || currentCharListFilter == CharacterListFilterType.COURT || currentCharListFilter == CharacterListFilterType.NOTABLE)
                                            {
                                                //#2 Leader Spouse(s)
                                                if (first.isLeaderSpouse())
                                                {
                                                    if (!second.isLeaderSpouse())
                                                    {
                                                        return -1;
                                                    }
                                                }
                                                else if (second.isLeaderSpouse())
                                                {
                                                    return 1;
                                                }

                                                //#3 Heirs 
                                                if (first.isSuccessor())
                                                {
                                                    if (second.isSuccessor())
                                                    {
                                                        return pActivePlayer.getSuccession().IndexOf(charID) - pActivePlayer.getSuccession().IndexOf(otherCharID);
                                                    }
                                                    else
                                                    {
                                                        return -1;
                                                    }
                                                }
                                                else if (second.isSuccessor())
                                                {
                                                    return 1;
                                                }
                                            }


                                            //#4 Council
                                            if (first.isCouncil())
                                            {
                                                if (second.isCouncil())
                                                {
                                                    int iFirstCharacterCouncilRank;
                                                    int iSecondCharacterCoucilRank;

                                                    if (first.getCouncil() == Infos.Globals.AMBASSADOR_COUNCIL)
                                                    {
                                                        //iFirstCharacterCouncilRank = 1;
                                                        return -1;
                                                    }
                                                    else if (first.getCouncil() == Infos.Globals.CHANCELLOR_COUNCIL)
                                                    {
                                                        iFirstCharacterCouncilRank = 2;
                                                    }
                                                    else if (first.getCouncil() == Infos.Globals.SPYMASTER_COUNCIL)
                                                    {
                                                        iFirstCharacterCouncilRank = 3;
                                                    }
                                                    else
                                                    {
                                                        iFirstCharacterCouncilRank = 4;
                                                    }

                                                    if (second.getCouncil() == Infos.Globals.AMBASSADOR_COUNCIL)
                                                    {
                                                        iSecondCharacterCoucilRank = 1;
                                                    }
                                                    else if (second.getCouncil() == Infos.Globals.CHANCELLOR_COUNCIL)
                                                    {
                                                        iSecondCharacterCoucilRank = 2;
                                                    }
                                                    else if (second.getCouncil() == Infos.Globals.SPYMASTER_COUNCIL)
                                                    {
                                                        iSecondCharacterCoucilRank = 3;
                                                    }
                                                    else
                                                    {
                                                        //other
                                                        iSecondCharacterCoucilRank = 4;
                                                    }

                                                    if (iFirstCharacterCouncilRank != iSecondCharacterCoucilRank)
                                                    {
                                                        return iFirstCharacterCouncilRank - iSecondCharacterCoucilRank;
                                                    }

                                                }
                                                else
                                                {
                                                    return -1;
                                                }
                                            }
                                            else if (second.isCouncil())
                                            {
                                                return 1;
                                            }

                                            ////#5 notable
                                            //if (currentCharListFilter == CharacterListFilterType.NONE)
                                            //{
                                            //    if (first.isPinned())
                                            //    {
                                            //        if (!second.isPinned())
                                            //        {
                                            //            return -1;
                                            //        }
                                            //    }
                                            //    else if (second.isPinned())
                                            //    {
                                            //        return 1;
                                            //    }
                                            //}

                                            //#6 Courtiers
                                            if (first.isCourtier())
                                            {
                                                if (!second.isCourtier())
                                                {
                                                    return -1;
                                                }
                                            }
                                            else if (second.isCourtier())
                                            {
                                                return 1;
                                            }

                                            //#7 family
                                            bool bFirstIgnoreFamily = false;
                                            bool bSecondIgnoreFamily = false;
                                            if (currentCharListFilter == CharacterListFilterType.GENERALS)
                                            {
                                                if (first.hasGeneralAll())
                                                {
                                                    bFirstIgnoreFamily = true;
                                                }

                                                if (second.hasGeneralAll())
                                                {
                                                    bSecondIgnoreFamily = true;
                                                }
                                            }
                                            else if (currentCharListFilter == CharacterListFilterType.GOVERNORS)
                                            {
                                                if (first.hasGovernorAll())
                                                {
                                                    bFirstIgnoreFamily = true;
                                                }

                                                if (second.hasGovernorAll())
                                                {
                                                    bSecondIgnoreFamily = true;
                                                }
                                            }


                                            {
                                                if (!first.hasFamily() || bFirstIgnoreFamily)
                                                {
                                                    //iFirstFamilyRank = 0;
                                                    if (second.hasFamily() && !bSecondIgnoreFamily)
                                                    {
                                                        return -1;
                                                    }
                                                }
                                                else if (!second.hasFamily() || bSecondIgnoreFamily)
                                                {
                                                    return 1;
                                                }
                                                else
                                                {
                                                    if (first.getFamily() != second.getFamily())
                                                    {
                                                        int iFirstFamilyRank = 0;
                                                        int iSecondFamilyRank = 0;
                                                        int i = 1;
                                                        foreach (FamilyType eLoopFamily in pActivePlayer.getFamilies())
                                                        {
                                                            if (first.getFamily() == eLoopFamily)
                                                            {
                                                                iFirstFamilyRank = i;
                                                            }

                                                            if (second.getFamily() == eLoopFamily)
                                                            {
                                                                iSecondFamilyRank = i;
                                                            }

                                                            i++;
                                                        }

                                                        //if (iFirstFamilyRank != iSecondFamilyRank)
                                                        {
                                                            return iFirstFamilyRank - iSecondFamilyRank;
                                                        }
                                                    }
                                                    else /* if (currentCharListFilter == CharacterListFilterType.NONE) */
                                                    {
                                                        if (first.isFamilyHead())
                                                        {
                                                            return -1;
                                                        }
                                                        if (second.isFamilyHead())
                                                        {
                                                            return 1;
                                                        }
                                                    }

                                                }
                                            }

                                            //#8 (possible) jobs: Generals, Governors, Agents
                                            if (first.isJob())
                                            {
                                                if (second.isJob())
                                                {
                                                    if (first.getJob() != second.getJob())
                                                    {
                                                        if (first.getJob() == Infos.Globals.GENERAL_JOB)
                                                        {
                                                            return -1;
                                                        }
                                                        else if (second.getJob() == Infos.Globals.GENERAL_JOB)
                                                        {
                                                            return 1;
                                                        }
                                                        else if (first.getJob() == Infos.Globals.GOVERNOR_JOB)
                                                        {
                                                            return -1;
                                                        }
                                                        else if (second.getJob() == Infos.Globals.GOVERNOR_JOB)
                                                        {
                                                            return 1;
                                                        }
                                                        else if (first.getJob() == Infos.Globals.AGENT_JOB)
                                                        {
                                                            return -1;
                                                        }
                                                        else if (second.getJob() == Infos.Globals.AGENT_JOB)
                                                        {
                                                            return 1;
                                                        }
                                                        else
                                                        {
                                                            //precaution for more jobs
                                                            return (int)first.getJob() - (int)second.getJob();
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    return -1;
                                                }
                                            }
                                            else
                                            {
                                                if (second.isJob())
                                                {
                                                    return 1;
                                                }
                                                else
                                                {
                                                    //both no job: look at filter settings
                                                    if (currentCharListFilter == CharacterListFilterType.GENERALS || currentCharListFilter == CharacterListFilterType.NONE)
                                                    {
                                                        if (first.hasGeneralAll())
                                                        {
                                                            if (!second.hasGeneralAll())
                                                            {
                                                                return -1;
                                                            }
                                                        }
                                                        else if (second.hasGeneralAll())
                                                        {
                                                            return 1;
                                                        }
                                                    }

                                                    if (currentCharListFilter == CharacterListFilterType.GOVERNORS || currentCharListFilter == CharacterListFilterType.NONE)
                                                    {
                                                        //hasGovernorAll
                                                        if (first.hasGovernorAll())
                                                        {
                                                            if (!second.hasGovernorAll())
                                                            {
                                                                return -1;
                                                            }
                                                        }
                                                        else if (second.hasGovernorAll())
                                                        {
                                                            return 1;
                                                        }

                                                        //duplicating hasGovernorPrereq
                                                        if (first.hasGovernorPrereq())
                                                        {
                                                            if (second.hasGovernorPrereq())
                                                            {

                                                            }
                                                            else
                                                            {
                                                                return -1;
                                                            }
                                                        }
                                                        else if (second.hasGovernorPrereq())
                                                        {
                                                            return 1;
                                                        }
                                                    }

                                                    //hasGeneralPrereq
                                                    if (first.hasGeneralPrereq())
                                                    {
                                                        if (second.hasGeneralPrereq())
                                                        {

                                                        }
                                                        else
                                                        {
                                                            return -1;
                                                        }
                                                    }
                                                    else if (second.hasGeneralPrereq())
                                                    {
                                                        return 1;
                                                    }

                                                    //hasGovernorPrereq
                                                    if (first.hasGovernorPrereq())
                                                    {
                                                        if (second.hasGovernorPrereq())
                                                        {

                                                        }
                                                        else
                                                        {
                                                            return -1;
                                                        }
                                                    }
                                                    else if (second.hasGovernorPrereq())
                                                    {
                                                        return 1;
                                                    }

                                                    //hasAgentPrereq: inlcuding religion agents here
                                                    if (first.hasAgentPrereq(bIncludeReligion: true))
                                                    {
                                                        if (second.hasAgentPrereq(bIncludeReligion: true))
                                                        {

                                                        }
                                                        else
                                                        {
                                                            return -1;
                                                        }
                                                    }
                                                    else if (second.hasAgentPrereq(bIncludeReligion: true))
                                                    {
                                                        return 1;
                                                    }

                                                }
                                            }


                                            //#9 age: old first
                                            int charAge = first.getAge();
                                            int otherCharAge = second.getAge();

                                            if (charAge != otherCharAge)
                                            {
                                                return otherCharAge - charAge;
                                            }

                                            //#10 character IDs: lowest number = oldest entry first
                                            return charID - otherCharID;
                                        }
                                    }

                                    //foreach (FamilyType eLoopFamily in pActivePlayer.getFamilies())
                                    //{
                                    //    Character pHead = pActivePlayer.familyHead(eLoopFamily);
                                    //    if (pHead != null && validCharacterFamily(pHead))
                                    //    {
                                    //        checkedIDSet.Add(pHead.getID());
                                    //        orderedIDList.Add(pHead.getID());
                                    //        //addCharacterFamily(pHead, checkedIDSet, charIDList, orderedIDList);
                                    //    }
                                    //
                                    //    foreach (int iLoopCharacter in charIDList)
                                    //    {
                                    //        if (checkedIDSet.Contains(iLoopCharacter))
                                    //        {
                                    //            continue;
                                    //        }
                                    //
                                    //        Character pLoopCharacter = Game.character(iLoopCharacter);
                                    //
                                    //        if (pLoopCharacter.getFamily() == eLoopFamily && validCharacterFamily(pLoopCharacter))
                                    //        {
                                    //            checkedIDSet.Add(pLoopCharacter.getID());
                                    //            orderedIDList.Add(pLoopCharacter.getID());
                                    //            //addCharacterFamily(pLoopCharacter, checkedIDSet, charIDList, orderedIDList);
                                    //        }
                                    //    }
                                    //}
                                    //
                                    //foreach (int iLoopCharacter in charIDList)
                                    //{
                                    //    if (!checkedIDSet.Contains(iLoopCharacter) && validCharacterFamily(Game.character(iLoopCharacter)))
                                    //    {
                                    //        checkedIDSet.Add(iLoopCharacter);
                                    //        orderedIDList.Add(iLoopCharacter);
                                    //        //addCharacterFamily(Game.character(iLoopCharacter), checkedIDSet, charIDList, orderedIDList);
                                    //    }
                                    //}
/*####### Better Old World AI - Base DLL #######
  ### Alternative Character Sorting      END ###
  ##############################################*/


                                    int numCharacters = 0;
                                    foreach (int iLoopCharacter in orderedIDList)
                                    {
                                        using (new UnityProfileScope("ClientUI.updateCharacters.SetUIData"))
                                        {
                                            UIAttributeTag characterListTag = UI.GetUIAttributeTag("CharacterListCharacter", numCharacters);
                                            characterListTag.SetKey("State", GetCharacterCardBG(iLoopCharacter));
                                            characterListTag.SetInt("ID", iLoopCharacter);
                                            characterListTag.IsActive = true;
                                            numCharacters++;
                                        }
                                    }

                                    if (numCharacters > maxCharacterListCount)
                                    {
                                        maxCharacterListCount = numCharacters;
                                        UI.SetUIAttribute("CharacterList-Count", maxCharacterListCount.ToStringCached());
                                    }
                                    else
                                    {
                                        for (int i = numCharacters; i < maxCharacterListCount; i++)
                                        {
                                            UIAttributeTag characterListTag = UI.GetUIAttributeTag("CharacterListCharacter", i);
                                            characterListTag.IsActive = false;
                                        }
                                    }
                                }

                            }

                            mPlayerFamily.SetBool("CurrentNation-ShowNation", false);
                        }

                        Interfaces?.CIQ?.SetToGameState();
                        Interfaces?.CIQ?.ProcessColor(pActivePlayer.getPrimaryPlayerColor(pActivePlayer));
                    }
                    else
                    {
                        UI.SetUIAttribute("TabPanel-Characters-IsVisible", false.ToStringCached());
                        UI.SetUIAttribute("TabPanel-Characters-Court-IsVisible", false.ToStringCached());
                    }
                }
            }
        }



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

            const int numButtons = 10;
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
