using System;
using System.Collections.Generic;
using Mohawk.SystemCore;
using UnityEngine;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using static TenCrowns.GameCore.Text.TextExtensions;

namespace BetterAI
{
    public partial class BetterAIPlayer : Player
    {

        //lines 157-188
        public override void doEventPlayer()
        {
            if (isTeamHuman() || !(game().isCharacters()))
            {
                return;
            }

            using (var mapDieScoped = CollectionCache.GetListScoped<(EventPlayerType, int)>())
            {
                List<(EventPlayerType, int)> mapEventDie = mapDieScoped.Value;

                for (EventPlayerType eLoopEventPlayer = 0; eLoopEventPlayer < infos().eventPlayersNum(); eLoopEventPlayer++)
                {
                    if (canDoBonus(infos().eventPlayer(eLoopEventPlayer).meBonus))
                    {
                        if (infos().eventPlayer(eLoopEventPlayer).miDie > 0)
                        {
                            mapEventDie.Add((eLoopEventPlayer, infos().eventPlayer(eLoopEventPlayer).miDie));
                        }
                    }
                }

                {
                    EventPlayerType eEventPlayer = infos().utils().randomDieMap(mapEventDie, game().nextSeed(), EventPlayerType.NONE);

                    if (eEventPlayer != EventPlayerType.NONE)
                    {

/*####### Better Old World AI - Base DLL #######
  ### Stat Bonus to Primary Stat       START ###
  ##############################################*/
                        if (((BetterAIInfoGlobals)(infos().Globals)).BAI_PLAYEREVENT_STAT_BONUS_GOES_TO_PRIMARY_STAT_PERCENT != 0)
                        {
                            int ratingsCount(SparseList<RatingType, int> aiRatings)
                            {
                                int iValue = 0;

                                for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                                {
                                    iValue += aiRatings[eLoopRating];
                                }

                                return iValue;
                            }

                            int iRatingsCount = ratingsCount(infos().bonus(infos().eventPlayer(eEventPlayer).meBonus).maiRatings);

                            if (iRatingsCount > 0)
                            {
                                if (game().randomNext(100) < ((BetterAIInfoGlobals)(infos().Globals)).BAI_PLAYEREVENT_STAT_BONUS_GOES_TO_PRIMARY_STAT_PERCENT)
                                {
                                    int iBestRatingValue = 1;
                                    RatingType eBestRating = RatingType.NONE;
                                    for (RatingType eLoopRating = 0; eLoopRating < infos().ratingsNum(); eLoopRating++)
                                    {
                                        if (leader().getRating(eLoopRating) > iBestRatingValue)
                                        {
                                            eBestRating = eLoopRating;
                                            iBestRatingValue = leader().getRating(eLoopRating);
                                        }
                                    }

                                    //select the  pseudo-event for the bonus wit the best stat
                                    if (eBestRating != RatingType.NONE)
                                    {
                                        for (EventPlayerType eLoopEventPlayer = 0; eLoopEventPlayer < infos().eventPlayersNum(); eLoopEventPlayer++)
                                        {
                                            if (canDoBonus(infos().eventPlayer(eLoopEventPlayer).meBonus))
                                            {
                                                if (infos().bonus(infos().eventPlayer(eLoopEventPlayer).meBonus).maiRatings[eBestRating] > 0)
                                                {
                                                    doBonus(infos().eventPlayer(eLoopEventPlayer).meBonus);
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
/*####### Better Old World AI - Base DLL #######
  ### Stat Bonus to Primary Stat         END ###
  ##############################################*/

                        doBonus(infos().eventPlayer(eEventPlayer).meBonus);
                    }
                }
            }
        }

    }
}
