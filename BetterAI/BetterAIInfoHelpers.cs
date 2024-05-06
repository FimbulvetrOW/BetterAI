using UnityEngine;
using System.Collections.Generic;
using System;
using Mohawk.SystemCore;
using TenCrowns.GameCore;

namespace BetterAI
{
    public class BetterAIInfoHelpers : InfoHelpers
    {
        public BetterAIInfoHelpers(Infos pInfos) : base(pInfos)
        {
        }

/*####### Better Old World AI - Base DLL #######
  ### Old (precise) collateral damage  START ###
  ##############################################*/
        public override int getAttackDamage(int iFromStrength, int iToStrength, int iPercent)
        {
            if (((BetterAIInfoGlobals)mInfos.Globals).BAI_PRECISE_COLLATERAL_DAMAGE == 0)
            {
                return base.getAttackDamage(iFromStrength, iToStrength, iPercent);
            }

            int iDamage = mInfos.Globals.BASE_DAMAGE;

            iDamage *= iFromStrength;

            bool bRoundUp = ((iFromStrength > iToStrength) && ((iDamage % iToStrength) > 0));

            iDamage /= iToStrength;

            if (iPercent != 100)
            {
                iDamage *= iPercent;
                iDamage /= 100;
            }

            if (bRoundUp)
            {
                iDamage++;
            }

            return Math.Max(1, iDamage);
        }
/*####### Better Old World AI - Base DLL #######
  ### Old (precise) collateral damage    END ###
  ##############################################*/

/*####### Better Old World AI - Base DLL #######
  ### Leader yield preview fix         START ###
  ##############################################*/
        public override int getRatingYieldRateLeader(RatingType eRating, YieldType eYield, int iValue, OpinionCharacterType eOpinionCharacter, Game pGame)
        {
            return getRatingYieldRateCourt(eRating, eYield, iValue, 0, OpinionCharacterType.NONE, pGame);
        }
/*####### Better Old World AI - Base DLL #######
 ### Leader yield preview fix           END ###
 ##############################################*/

        public override int modifyRating(int iValue, int iRating, int iOffset, Game pGame)
        {
/*####### Better Old World AI - Base DLL #######
  ### Old Competitive yield reduction  START ###
  ##############################################*/
            if (pGame == null || !pGame.isGameOption(mInfos.Globals.GAMEOPTION_LOWER_CHARACTER_YIELDS) || ((BetterAIInfoGlobals)mInfos.Globals).BAI_USE_TRIANGLE_IN_COMPETITIVE != 0)
/*####### Better Old World AI - Base DLL #######
  ### Old Competitive yield reduction    END ###
  ##############################################*/
            {
                iValue *= mInfos.utils().triangleOffset(iRating, iOffset);
            }
            else
            {
                int iEquivalentRatingValue = Math.Max(1, mInfos.Globals.RATING_EQUIVALENT_LOWER_CHARACTER_YIELDS);
                iValue *= iRating * mInfos.utils().triangleOffset(iEquivalentRatingValue, iOffset);
                iValue /= iEquivalentRatingValue;
            }
            return iValue;
        }
        public override int boostRating(int iValue, int iRating, Game pGame)
        {
/*####### Better Old World AI - Base DLL #######
  ### Old Competitive yield reduction  START ###
  ##############################################*/
            if (pGame == null || !pGame.isGameOption(mInfos.Globals.GAMEOPTION_LOWER_CHARACTER_YIELDS) || ((BetterAIInfoGlobals)mInfos.Globals).BAI_USE_TRIANGLE_IN_COMPETITIVE != 0)
/*####### Better Old World AI - Base DLL #######
  ### Old Competitive yield reduction    END ###
  ##############################################*/
            {
                iValue *= mInfos.utils().triangleBoost(iRating);
            }
            else
            {
                int iEquivalentRatingValue = Math.Max(1, mInfos.Globals.RATING_EQUIVALENT_LOWER_CHARACTER_YIELDS);
                iValue *= iRating * mInfos.utils().triangleBoost(iEquivalentRatingValue);
                iValue /= iEquivalentRatingValue;
            }
            return iValue;
        }

        protected override int getRatingYieldRateCourt(RatingType eRating, YieldType eYield, int iValue, int iModifier, OpinionCharacterType eOpinionCharacter, Game pGame)
        {
            if (iValue == 0)
            {
                return 0;
            }

            int iYield = mInfos.utils().modify(mInfos.rating(eRating).maiYieldCourtRate[eYield], iModifier, true);

            if (iYield == 0)
            {
                return 0;
            }

            iYield = modifyRating(iYield, iValue, mInfos.yield(eYield).miTriangleOffset, pGame);

            if (eOpinionCharacter != OpinionCharacterType.NONE)
            {
                iYield = mInfos.utils().modify(iYield, (mInfos.opinionCharacter(eOpinionCharacter).miRateModifier * ((yieldWarning(eYield, iValue)) ? -1 : 1)), true);
            }

/*####### Better Old World AI - Base DLL #######
  ### Old Competitive yield reduction  START ###
  ##############################################*/
            if (pGame.isGameOption(mInfos.Globals.GAMEOPTION_LOWER_CHARACTER_YIELDS))
            {
                iYield = mInfos.utils().modify(iYield, ((BetterAIInfoGlobals)mInfos.Globals).BAI_COMPETITIVE_COURT_YIELD_MODIFIER);
            }
/*####### Better Old World AI - Base DLL #######
  ### Old Competitive yield reduction    END ###
  ##############################################*/

            return iYield;
        }

    }
}
