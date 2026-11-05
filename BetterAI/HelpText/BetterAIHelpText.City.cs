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

/*####### Better Old World AI - Base DLL #######
  ### Rel. Improvements in City list   START ###
  ##############################################*/
        //public virtual TextVariable buildCityLinkVariable(City pCity, Player pActivePlayer, bool bCrest = true, bool bAddColor = true, bool bSelect = true)
        //lines 65-67
        public virtual TextVariable buildCityLinkVariableWithReligiousImprovements(Game pGame, City pCity, Player pActivePlayer, ReligionType eReligion, List<ImprovementType> ImprovementTypes, Dictionary<ImprovementType, string> ImprovementShortNames, bool bCrest = true, bool bAddColor = true, bool bSelect = true)
        {
            //UnityEngine.Debug.Log("HelpText.buildCityLinkVariableWithReligiousImprovements");

            if (pCity == null)
            {
                return TEXTVAR(false);
            }

            TextVariable returnText = base.buildCityLinkVariable(pCity, pActivePlayer, bCrest, bAddColor, bSelect);

            if (eReligion != ReligionType.NONE && ImprovementTypes != null)
            {
                if (ImprovementTypes.Count > 0)
                {
                    CommaListVariableGenerator religiousImprovements = new CommaListVariableGenerator(CommaListVariableGenerator.ListType.NONE, CommaListVariableGenerator.EncloseType.PARENTHESIS_OUTER, bEncloseSingleItem: true, TextManager);
                    foreach (ImprovementType eLoopImprovement in ImprovementTypes)
                    {
                        if (pCity.getImprovementCount(eLoopImprovement) > 0)
                        {
                            string name = ImprovementShortNames[eLoopImprovement];

                            //counts
                            if (pCity.getActiveImprovementCount(eLoopImprovement) > 0)
                            {
                                if (pCity.getActiveImprovementCount(eLoopImprovement) > 1)
                                {
                                    name = pCity.getActiveImprovementCount(eLoopImprovement).ToStringCached() + name;
                                }
                                if (pCity.getImprovementCount(eLoopImprovement) > pCity.getActiveImprovementCount(eLoopImprovement))
                                {
                                    int iUnfinishedImprovements = pCity.getImprovementCount(eLoopImprovement) - pCity.getActiveImprovementCount(eLoopImprovement);
                                    name = name + "(+" + iUnfinishedImprovements.ToStringCached() + ")";
                                }
                            }
                            else
                            {
                                if (pCity.getImprovementCount(eLoopImprovement) > 1)
                                {
                                    name = pCity.getImprovementCount(eLoopImprovement).ToStringCached() + name;
                                }
                                name = "(" + name + ")";
                            }

                            religiousImprovements.AddItem(buildLinkTextVariable(TEXTVAR(name), ItemType.HELP_LINK, nameof(LinkType.HELP_IMPROVEMENT), infos().improvement(eLoopImprovement).SafeTypeString(), "-1"));

                        }
                    }

                    if (religiousImprovements.Count > 0)
                    {
                        returnText = TEXTVAR_TYPE("TEXT_HELPTEXT_CONCAT_SPACE_TWO", returnText, religiousImprovements.Finalize());
                    }
                }            
            }

            return returnText;
        }
/*####### Better Old World AI - Base DLL #######
  ### Rel. Improvements in City list     END ###
  ##############################################*/
    }
}