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
            if (now.DayOfWeek == DayOfWeek.Saturday || now.DayOfWeek == DayOfWeek.Sunday)
            {
                // 주말이면 false 리턴
                return false;
            }
            if (now.Hour < 9 || now.Hour > 15)
            {
                // 9시부터 15시 사이가 아니면 false 리턴
                return false;
            }
            if (now.Hour == 15 && now.Minute > 10)
            {
                // 15시 10분 이후면 false 리턴
                return false;
            }

            if (now.Hour == 9 && now.Second > 5)
            {
                return false;
            }
            return true;
        }
    }
}
