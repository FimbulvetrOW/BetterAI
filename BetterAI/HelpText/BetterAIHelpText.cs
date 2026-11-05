using Mohawk.SystemCore;
using Mohawk.UIInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using TenCrowns.AppCore;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using UnityEngine;
using UnityEngine.UI;
using static TenCrowns.GameCore.Player;
using static TenCrowns.GameCore.Text.TextExtensions;
using Constants = TenCrowns.GameCore.Constants;

namespace BetterAI
{
    public partial class BetterAIHelpText : HelpText
    {
        //lines 61-63
        public BetterAIHelpText(TextManager txtMgr) : base(txtMgr)
        {
        }

        //lines 5416-5484
        public override TextBuilder buildChooseGovernorHelp(TextBuilder builder, City pCity, ClientManager pManager, bool bDetails, bool bName = false)
        {
            //UnityEngine.Debug.Log("HelpText.buildChooseGovernorHelp");

            //using (new UnityProfileScope("HelpText.buildChooseGovernorHelp"))
            {
                Player pActivePlayer = pManager.activePlayer();
                Game pGame = pManager.GameClient;

                if (pCity == null)
                {
                    return builder;
                }

                if (pCity.getTeam() == pActivePlayer.getTeam())
                {
                    if (bName || bDetails)
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_CHOOSE_GOVERNOR", buildCityLinkVariable(pCity, pActivePlayer));
                    }
                    
/*####### Better Old World AI - Base DLL #######
  ### mbEnablesGovernor restoration    START ###
  ##############################################*/
                    if (!(((BetterAICity)pCity).isEnablesGovernor()))
                    {
                        using (buildWarningTextScope(builder, bDetails))
                        using (builder.BeginScope(TextBuilder.ScopeType.COMMA_OR, surroundingText: TEXTVAR_TYPE("TEXT_HELPTEXT_REQUIRES")))
                        {
                            for (EffectCityType eLoopEffectCity = 0; eLoopEffectCity < infos().effectCitiesNum(); eLoopEffectCity++)
                            {
                                if (((BetterAIInfoEffectCity)infos().effectCity(eLoopEffectCity)).mbEnablesGovernor && pGame.checkGameContent(eLoopEffectCity))
                                {
                                    builder.Add(buildEffectCitySourceLinkVariable(eLoopEffectCity, pCity, null, pGame, pActivePlayer));
                                }
                            }
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### mbEnablesGovernor restoration      END ###
  ##############################################*/

                    buildGovernorCandidatesHelp(builder, pCity, pManager, bDetails);

                    if (bDetails)
                    {
                        buildDividerText(builder);
                    }
                }

                if (bDetails)
                {
                    if (pCity.isGoverned())
                    {
                        Character pGovernor = pCity.governor();

                        if (pCity.hasGovernor())
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_CURRENT_GOVERNOR", buildCharacterLinkVariable(pGovernor, pActivePlayer, bAge: true, bArchetypeCrest: true));
                        }
                        else
                        {
                            builder.AddTEXT("TEXT_HELPTEXT_DEFAULT_GOVERNOR", buildCharacterLinkVariable(pGame.character(pCity.getDefaultGovernorID()), pActivePlayer, bAge: true, bArchetypeCrest: true));
                        }

                        if (pCity.isInfoVisible(pActivePlayer.getTeam(), pManager))
                        {
                            buildCityGovernorHelp(builder, pGovernor, pCity, pGame, !pCity.hasGovernor(), pActivePlayer);
                        }
                        else
                        {
                            using (buildWarningTextScope(builder))
                            {
                                builder.AddTEXT("TEXT_HELPTEXT_HIDDEN_WITHOUT_AGENT");
                            }
                        }

                        if (pManager?.UI?.CurrentTabOverlay != TabOverlayState.CITIES)
                        {
                            buildDividerText(builder);
                            builder.AddTEXT("TEXT_HELPTEXT_HOTKEY_SELECT_GOVERNOR");
                        }
                    }
                    else
                    {
                        builder.AddTEXT("TEXT_HELPTEXT_CURRENT_GOVERNOR", TEXTVAR_TYPE("TEXT_HELPTEXT_NONE"));
                    }
                }

                return builder;
            }
        }

        //lines 2643-2683
        public override TextBuilder buildUrbanHelp(TextBuilder builder, Game pGame)
        {
            //UnityEngine.Debug.Log("HelpText.buildUrbanHelp");

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

        //lines 2820-3377
        //something was changed here, will observe again. I hope I don't need to re-add 
        //public virtual TextBuilder buildCityYieldNetHelp

/*####### Better Old World AI - Base DLL #######
  ### misc                             START ###
  ##############################################*/
        //public struct BetterAICommaListVariableGenerator : CommaListVariableGenerator
        //{
        //}
        //structs can't inherit
        //I can't overrride a struct but at least inside the derived BetterAIHelpText class I can call this

        //lines 6639-6762
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
