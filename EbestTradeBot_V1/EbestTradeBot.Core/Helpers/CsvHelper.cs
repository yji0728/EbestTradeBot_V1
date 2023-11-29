using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;
using EbestTradeBot.Core.Models;

namespace EbestTradeBot.Core.Helpers
{
    public class CsvHelper
    {
        public static List<TradedStock> ReadTradedStockCsv(string filePath)
        {
            FileCheck(filePath);
            using(var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                return csv.GetRecords<TradedStock>().ToList();
            }
        }

        public static void WriteCsv(string filePath, TradedStock data)
        {
            using (var writer = new StreamWriter(filePath, File.Exists(filePath), Encoding.UTF8))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                List<TradedStock> stocks = new List<TradedStock>();
                stocks.Add(data);
                csv.WriteRecords(stocks);
            }
        }

        private static void FileCheck(string filePath)
        {
            if (!File.Exists(filePath))
            {
                using (File.Create(filePath)) ;
            }
        }
    }
}
