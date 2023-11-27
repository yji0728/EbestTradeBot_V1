using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbestTradeBot.Core.Models;
using XA_DATASETLib;

namespace EbestTradeBot.Core.Helpers
{
    public class CalcHelper
    {
        public static void CalcPrice(ref Stock stock, List<Data_t1305> t1305s)
        {
            // 기준봉 구하기
            int start = -1;
            int end = -1;
            int count = t1305s.Count;
            double diff = -1;

            for (int i = 0; i < t1305s.Count; i++)
            {
                diff = t1305s[i].Diff;

                if (diff < 15) continue;

                start = i;

                for (int j = i + 1; j < count; j++)
                {
                    diff = t1305s[j].Diff;

                    if (diff >= 15)
                    {
                        start = j;

                    }
                    else
                    {
                        break;
                    }
                }

                for (int j = i - 1; j > -1; j--)
                {
                    diff = t1305s[j].Diff;
                    if (diff < 0)
                    {
                        int iClose = t1305s[j + 1].Close;
                        int iOpen = t1305s[j].Open;

                        if (iOpen > iClose)
                        {
                            end = j;
                        }
                        else
                        {
                            end = j + 1;
                        }
                        break;
                    }
                }

                if (end == -1)
                {
                    end = start;
                }

                break;
            }

            if (start == -1 || end == -1)
            {
                stock.매수가 = -1;
                stock.익절가 = -1;
                stock.손절가 = -1;

                return;
            }

            int open = t1305s[start].Open;
            int close = -1;
            int endClose = t1305s[end].Close;
            int endOpen = t1305s[end].Open; 

            if (endClose >= endOpen)
            {
                close = endClose;
            }
            else
            {
                close = endOpen;
            }

            stock.매수가 = (open + close) / 2;
            stock.손절가 = open;
            stock.익절가 = close;
        }
    }
}
