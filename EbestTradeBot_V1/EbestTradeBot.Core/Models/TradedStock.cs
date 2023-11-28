using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbestTradeBot.Core.Models
{
    public class TradedStock
    {
        public string Shcode { get; set; }
        public DateTime TradeDate { get; set; }
    }
}
