using Mohawk.SystemCore;

using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using TenCrowns.GameCore;

namespace BetterAI
{
    public class BetterAIPlayerCache
    {
        //does not inherit from PlayerCache and is supposed to be used in addition to it, not instead

        protected ConcurrentDictionary<(YieldType, int), long> mdCityYieldValuesFlat = new ConcurrentDictionary<(YieldType, int), long>();

        public virtual void clear()
        {
            mdCityYieldValuesFlat.Clear();
        }

        public BetterAIPlayerCache()
        {
        }

        public virtual bool getCityYieldValueFlat(YieldType eYield, int iCityID, out long iValue)
        {
            return mdCityYieldValuesFlat.TryGetValue((eYield, iCityID), out iValue);
        }

        public virtual void setCityYieldValueFlat(YieldType eYield, int iCityID, long iValue)
        {
            mdCityYieldValuesFlat[(eYield, iCityID)] = iValue;
        }
    }
}
