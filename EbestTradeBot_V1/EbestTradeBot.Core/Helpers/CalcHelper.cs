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
            int count = t1305s.Count;

            stock.익절가 = -1;
            stock.손절가 = -1;
            stock.매수가_1차 = -1;

            for (int i = 0; i < t1305s.Count; i++)
            {
                if (t1305s[i].Diff < 15) continue;

                stock.손절가 = t1305s[i].Open;
                stock.익절가 = t1305s[i].Close;

                for (int j = i + 1; j < count; j++)
                {
                    if (t1305s[j].Close < stock.익절가 && t1305s[j].Open < stock.익절가) break;

                    if (t1305s[j].Close >= stock.익절가)
                    {
                        stock.익절가 = t1305s[j].Close;
                    }
                    if (t1305s[j].Open >= stock.익절가)
                    {
                        stock.익절가 = t1305s[j].Open;
                    }
                    
                }

                break;
            }
            stock.매수가_1차 = (stock.손절가 + stock.익절가) / 2;
            if (stock.매수가_1차 == -1 || stock.익절가 == -1 || stock.손절가 == -1)
            {
                stock.매수가_1차 = -1;
                stock.익절가 = -1;
                stock.손절가 = -1;

                return;
            }
        }
    }
}
