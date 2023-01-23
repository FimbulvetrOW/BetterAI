using System.Linq;
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
using System;
using System.IO.Ports;
using System.Collections.Generic;

namespace BetterAI
{
    public class BetterAIClientUI : ClientUI
    {
        public BetterAIClientUI(ClientManager manager) : base(manager)
        {
        }

        protected override void updateQueuePanel()
        {
            base.updateQueuePanel();

/*####### Better Old World AI - Base DLL #######
  ### Alternative Hurry                START ###
  ##############################################*/
            City pSelectedCity = mManager.Selection.getSelectedCity();
            if (pSelectedCity == null) return;

            int i = 1;
            UIAttributeTag queueTag = ui.GetUIAttributeTag("QueuePanel-Button", i.ToStringCached());
            if ((((BetterAIInfoGlobals)Infos.Globals).BAI_HURRY_COST_REDUCED_BY_PRODUCTION == 1) && pSelectedCity.getBuildQueueNode(0) != null && (pSelectedCity.getBuildThreshold(pSelectedCity.getBuildQueueNode(0)) == pSelectedCity.getBuildQueueNode(0).miProgress) && (pSelectedCity.getBuildQueueNode(0).miProgress > 0))
            {
                queueTag.SetBool("Arrow-IsActive", false);
            }
            else
            {
                queueTag.SetBool("Arrow-IsActive", true);
            }

            const int numButtons = 5;
            for (i = 2; i < Math.Min(pSelectedCity.getBuildCount(), numButtons); i++)
            {
                queueTag = ui.GetUIAttributeTag("QueuePanel-Button", i.ToStringCached());
                queueTag.SetBool("Arrow-IsActive", true);

                //updateActiveBuildDisplay(pSelectedCity, queueTag, pSelectedCity.getBuildQueueNode(i), i, true);
                //queueTag.IsActive = true;
                //queueTag.SetBool("Effect-IsActive", !addedToFront && lastSelectedCityID == pSelectedCity.getID() && lastSelectedCityQueue.Count <= i);
            }
        }

        //Intercept and redirect illegal queue moves at doWidgetAction
        public override void doWidgetAction(WidgetData pWidget)
        {
            ItemType eType = pWidget.GetWidgetType();
            if (eType == ItemType.BUILD_QUEUE)
            {
                City pCity = Game.city(pWidget.GetDataInt(0));

                if (pCity != null)
                {
                    if ((((BetterAIInfoGlobals)Infos.Globals).BAI_HURRY_COST_REDUCED_BY_PRODUCTION == 1) && pCity.getBuildQueueNode(0) != null && (pCity.getBuildThreshold(pCity.getBuildQueueNode(0)) == pCity.getBuildQueueNode(0).miProgress) && (pCity.getBuildQueueNode(0).miProgress > 0))
                    {
                        if (pWidget.GetDataInt(1) == 1)
                        {
                            //do nothing, this is an illegal move
                        }
                        else
                        {
                            mManager.sendBuildQueue(pCity, pWidget.GetDataInt(1), 1);
                        }
                    }
                    else
                    {
                        mManager.sendBuildQueue(pCity, pWidget.GetDataInt(1), 0);
                    }
                }
            }
            else
/*####### Better Old World AI - Base DLL #######
  ### Alternative Hurry                  END ###
  ##############################################*/
            {
                base.doWidgetAction(pWidget);
            }
        }

        protected override void updateCityListPanel()
        {
            if (currentCityListSort == CityListSortType.CULTURE_LEVEL)
            {
                //copy-paste start
                using (new UnityProfileScope("ClientUI.updateCityListPanel"))
                {
                    Player pActivePlayer = mManager.activePlayer();

                    using (var colorScope = CollectionCache.GetStringBuilderScoped())
                    using (var cityListScoped = CollectionCache.GetListScoped<int>())
                    {
                        StringBuilder colorSB = colorScope.Value;
                        List<int> cityList = cityListScoped.Value;
                        for (int iI = 0; iI < pActivePlayer.getNumCities(); ++iI)
                        {
                            cityList.Add(pActivePlayer.cityAt(iI).getID());
                        }

                        if (!isCityListInitialized)
                        {
                            for (YieldType eLoopYield = 0; eLoopYield < Infos.yieldsNum(); eLoopYield++)
                            {
                                UIAttributeTag yieldHeader = ui.GetUIAttributeTag("CityList-Yield", (int)eLoopYield);
                                yieldHeader.SetTEXT("Data", TextManager, HelpText.buildCommaData(TEXTVAR((int)CityListSortType.YIELD), TEXTVAR((int)eLoopYield)));
                                yieldHeader.SetKey("Icon", Infos.yield(eLoopYield).mzIconName);

                                if (eLoopYield == Infos.Globals.MAINTENANCE_YIELD)
                                {
                                    yieldHeader.IsActive = false;
                                }
                            }

                            ui.SetUIAttribute("CityList-GovernorActive", (Game.isCharacters()).ToStringCached());
                            ui.SetUIAttribute("CityList-FamilyActive", (Game.isCharacters()).ToStringCached());
                            isCityListInitialized = true;
                        }

                        //switch (currentCityListSort)
                        //{
                        //    case CityListSortType.YIELD:
                        //        cityList.Sort((x, y) => isCitySortAscending ? CompareYieldProduction(x, y, currentYieldSort) : CompareYieldProduction(y, x, currentYieldSort));
                        //        break;

                        //    case CityListSortType.DISCONTENT_LEVEL:
                        //        cityList.Sort((x, y) => isCitySortAscending ? CompareDiscontentLevels(x, y) : CompareDiscontentLevels(y, x));
                        //        break;

                        //    case CityListSortType.CULTURE_LEVEL:
                        //        cityList.Sort((x, y) => isCitySortAscending ? CompareCultureLevels(x, y) : CompareCultureLevels(y, x));
                        //        break;

                        //    case CityListSortType.FAMILY:
                        //        cityList.Sort((x, y) => isCitySortAscending ? CompareFamily(x, y) : CompareFamily(y, x));
                        //        break;

                        //    case CityListSortType.PRODUCTION:
                        //        cityList.Sort((x, y) => isCitySortAscending ? CompareProduction(x, y) : CompareProduction(y, x));
                        //        break;

                        //    case CityListSortType.RELIGION:
                        //        cityList.Sort((x, y) => isCitySortAscending ? CompareReligion(x, y) : CompareReligion(y, x));
                        //        break;

                        //    case CityListSortType.GOVERNOR:
                        //        cityList.Sort((x, y) => isCitySortAscending ? CompareGovernor(x, y) : CompareGovernor(y, x));
                        //        break;

                        //    case CityListSortType.POPULATION:
                        //        cityList.Sort((x, y) => isCitySortAscending ? ComparePopulation(x, y) : ComparePopulation(y, x));
                        //        break;
                        //}

/*####### Better Old World AI - Base DLL #######
  ### Culture Level Sort               START ###
  ##############################################*/
                        cityList.Sort((x, y) => isCitySortAscending ? CompareCultureLevels(x, y) : CompareCultureLevels(y, x));
/*####### Better Old World AI - Base DLL #######
  ### Culture Level Sort                 END ###
  ##############################################*/

                        for (int i = 0; i < cityList.Count; i++)
                        {
                            UIAttributeTag cityAttribute = ui.GetUIAttributeTag("CityListCity", i.ToStringCached());
                            City city = Game.city(cityList[i]);

                            cityAttribute.SetInt("ID", cityList[i]);
                            cityAttribute.SetKey("OddRow", (i % 2 == 0).ToStringCached());
                            cityAttribute.SetInt("SelectionState", city.getID());
                        }

                        ui.SetUIAttribute("CityList-NumCities", cityList.Count.ToStringCached());
                        ui.SetUIAttribute("CityList-Tab-IsActive", (cityList.Count > 0).ToStringCached());
                    }

                    if (!isShowOwnCities)
                    {
                        updateAgentCityList();
                    }

                    ui.SetUIAttribute("TabPanel-Cities-IsVisible", (mCurrentTabPanel == TabPanelState.CITIES && isShowOwnCities).ToStringCached());
                    ui.SetUIAttribute("TabPanel-Agents-IsVisible", (mCurrentTabPanel == TabPanelState.CITIES && !isShowOwnCities).ToStringCached());
                }

                //copy-paste end
            }
            else
            {
                base.updateCityListPanel();
            }
        }


        private int CompareCultureLevels(int cityID, int otherCityID)
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
                    //reversed, less turns left = more culture
                    return otherCity.getYieldTurnsLeft(Infos.Globals.CULTURE_YIELD) - city.getYieldTurnsLeft(Infos.Globals.CULTURE_YIELD) ;
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
