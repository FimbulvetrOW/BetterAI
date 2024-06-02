using System;
using System.Collections.Generic;
using UnityEngine;
using TenCrowns.ClientCore;
using TenCrowns.GameCore;
using TenCrowns.GameCore.Text;
using static TenCrowns.GameCore.Text.TextExtensions;
using Mohawk.SystemCore;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Xml;
using Random = Mohawk.SystemCore.Random;
using BinaryReader = Mohawk.SystemCore.BinaryReader;
using BinaryWriter = Mohawk.SystemCore.BinaryWriter;

namespace BetterAI
{
    public class BetterAIGame : Game
    {
        public BetterAIGame(ModSettings pModSettings, IApplication pApp, bool bShowGame) : base(pModSettings, pApp, bShowGame)
        {
        }

        //lines 11904-11983
        public override ReligionType doReligionFound(PlayerType ePlayer, bool bTestPrereq, City pCity = null)
        {
            ReligionType eBestReligion = ReligionType.NONE;
            City pBestCity = null;
            int iBestValue = 0;

            for (int iPass = 0; iPass < 2; iPass++)
            {
                for (ReligionType eLoopReligion = 0; eLoopReligion < infos().religionsNum(); eLoopReligion++)
                {
                    if (infos().religion(eLoopReligion).mbHidden)
                    {
                        continue;
                    }

                    if (isWorldReligion(eLoopReligion))
                    {
                        if ((iPass == 0) ? canFoundReligion(eLoopReligion, true) : true)
                        {
                            foreach (City pLoopCity in getCities())
                            {
                                if ((ePlayer != PlayerType.NONE) ? (pLoopCity.getPlayer() == ePlayer) : true)
                                {
                                    if (pLoopCity.canFoundReligion(eLoopReligion, bTestPrereq))
                                    {
                                        int iValue = randomNext(1000) + 1;

/*####### Better Old World AI - Base DLL #######
  ### Respect Preferred Religion       START ###
  ##############################################*/
                                        if (pLoopCity == pCity)
                                        {
                                            //iValue += 16000;
                                            iValue += 64000;
                                        }

                                        if (!(pLoopCity.isReligionHolyCityAny()))
                                        {
                                            //iValue += 8000;
                                            iValue += 32000;
                                        }

                                        if (pLoopCity.player().countWorldReligionsWithHolyCities() == 0)
                                        {
                                            //iValue += 4000;
                                            iValue += 16000;
                                        }

                                        if (!(pLoopCity.isCapital()))
                                        {
                                            //iValue += 2000;
                                            iValue += 8000;
                                        }

                                        if (pLoopCity.isHuman())
                                        {
                                            //iValue += 1000;
                                            iValue += 4000;
                                        }

                                        //if (ePlayer != PlayerType.NONE && player(ePlayer).hasDynasty() && player(ePlayer).dynasty().mePreferredReligion == eLoopReligion)
                                        //{
                                        //    iValue = int.MaxValue;
                                        //}

                                        //Dynasty has no preference: +1000
                                        //Dynasty has preference, Religion is preference: +2000
                                        //Dynasty has preference, Religion is not preference: +0
                                        iValue += 1000;
                                        if (pLoopCity.getPlayer() != PlayerType.NONE && pLoopCity.player().hasDynasty() && pLoopCity.player().dynasty().mePreferredReligion != ReligionType.NONE)
                                        {
                                            if (pLoopCity.player().dynasty().mePreferredReligion == eLoopReligion)
                                            {
                                                iValue += 1000;
                                            }
                                            else
                                            {
                                                iValue -= 1000;
                                            }
                                        }
/*####### Better Old World AI - Base DLL #######
  ### Respect Preferred Religion         END ###
  ##############################################*/

                                        if (iValue > iBestValue)
                                        {
                                            eBestReligion = eLoopReligion;
                                            pBestCity = pLoopCity;
                                            iBestValue = iValue;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if ((eBestReligion != ReligionType.NONE) && (pBestCity != null))
                {
                    pBestCity.foundReligion(eBestReligion, bTestPrereq);

                    return eBestReligion;
                }
            }

            return ReligionType.NONE;
        }

    }
}
