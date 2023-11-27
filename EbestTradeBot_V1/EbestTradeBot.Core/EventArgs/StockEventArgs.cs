using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EbestTradeBot.Core.Models;

namespace EbestTradeBot.Core.EventArgs
{
    public class StockEventArgs : System.EventArgs
    {
        public List<Stock> Stocks { get; }

        public StockEventArgs(List<Stock> stocks)
        {
            Stocks = stocks;
        }
    }
}
