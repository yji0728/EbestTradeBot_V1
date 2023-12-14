using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbestTradeBot.Core.Helpers
{
    public class TimeHelper
    {
        public static bool IsMarketOpen()
        {
            DateTime now = DateTime.Now;
            TimeSpan currentTime = now.TimeOfDay;
            TimeSpan startTime = new TimeSpan(9, 1, 0);
            TimeSpan endTime = new TimeSpan(15, 31, 0);

            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                // 주말이면 false 리턴
                return false;
            }

            return currentTime >= startTime && currentTime < endTime;
        }
    }
}
