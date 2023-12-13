using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbestTradeBot.Core.Models
{
    public class Stock
    {
        public string Shcode { get; set; }
        public string Hname { get; set; }
        public int 손절가 { get; set; }
        public int 익절가 { get; set; }
        public int 매수가_1차 { get; set; }
        public int 매수가_2차 { get; set; }
        public int 보유량 { get; set; }
        public bool 매수2차완료여부 { get; set; }
    }
}
