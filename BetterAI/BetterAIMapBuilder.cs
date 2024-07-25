using System;
using System.Collections.Generic;
using UnityEngine;
using Mohawk.SystemCore;
using TenCrowns.GameCore;

namespace BetterAI
{
    public class BetterAIMapBuilder : MapBuilder
    {
        protected override void assignPlayerCitySites(Game pGame)
        {
            //using var profileScope = new UnityProfileScope("MapBuilder.assignPlayerCitySites");

            assignPlayerStarts(pGame);

            for (int i = 0; i < azSortedCitySites.Count; ++i)
            {
                Player pPlayer = azSortedCitySites[i].pPlayer;

                if (pPlayer.doesStartWithCities() && !pPlayer.isStartCityNumberFlexible())
                {
                    if (pPlayer.getNumStartingTiles() < pPlayer.getMinStartingCities())
                    {
                        pPlayer.addStartingTileID(azSortedCitySites[i].pTile.getID());
                        claimCitySite(i);
                        --i;
                        azSortedCitySites.Sort(); // sort after each iteration because claiming a sity changes things
                    }
                }
            }

            int iNumAI = 0;
            for (PlayerType ePlayer = 0; ePlayer < pGame.getNumPlayers(); ++ePlayer)
            {
                Player pLoopPlayer = pGame.player(ePlayer);
                if (pLoopPlayer.isStartCityNumberFlexible())
                {
                    ++iNumAI;
                }
            }

            if (iNumAI == 0)
            {
                return;
            }

            // Number of cities to place depends on the Development setting from game launch: base value plus possible modifiers.
            int iNumTargetCities = iNumAI * (pGame.development().miAvgCities - 1); // Subtract the capitals 

            if (iNumTargetCities < 1)
            {
                return;
            }

/*####### Better Old World AI - Base DLL #######
  ### Less development cities variation START ##
  ##############################################*/
            int iMaxAIStartingCities = ((100 + ((BetterAIInfoGlobals)(pGame.infos().Globals)).BAI_PLAYER_MAX_EXTRA_DEVELOPMENT_CITIES_PERCENT) * pGame.development().miAvgCities) / 100;

            // place cities (not necessarily the same number for each player)
            for (int i = 0; i < azSortedCitySites.Count && iNumTargetCities > 0; ++i)
            {
                Player pPlayer = azSortedCitySites[i].pPlayer;

                if (pPlayer.isStartCityNumberFlexible())
                {
                    pPlayer.addStartingTileID(azSortedCitySites[i].pTile.getID());
                    --iNumTargetCities;
                    claimCitySite(i);
                    --i;



                    //// Cap player cities to twice the average
                    //if (pPlayer.getNumStartingTiles() >= 2 * pGame.development().miAvgCities)
                    if (pPlayer.getNumStartingTiles() >= iMaxAIStartingCities)
                    {
                        for (int j = azSortedCitySites.Count - 1; j >= 0; --j)
                        {
                            if (azSortedCitySites[j].pPlayer == pPlayer)
                            {
                                azSortedCitySites.RemoveAt(j);
                            }
                        }
                    }
/*####### Better Old World AI - Base DLL #######
  ### Less development cities variation  END ###
  ##############################################*/

                    azSortedCitySites.Sort(); // sort after each iteration because claiming a sity changes things
                }
            }
        }
    }
}
