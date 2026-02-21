using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace TopstepAlgo
{
    public class Trades
    {
        private readonly Connexion connexion;

        public Trades(Connexion api)
        {
            connexion = api;
        }

        // ================= SEARCH TRADES =================
        public async Task<List<TradeInfo>> SearchTrades(long accountId, DateTime start, DateTime? end = null)
        {
            var url = "https://api.topstepx.com/api/Trade/search";

            var payload = new
            {
                accountId = accountId,
                startTimestamp = start.ToString("O"),
                endTimestamp = end?.ToString("O")
            };

            string json = await connexion.PostAsync(url, payload);

            return ParseTrades(json);
        }

        // ================= TODAY TRADES =================
        public async Task<List<TradeInfo>> GetTodayTrades(long accountId)
        {
            var start = DateTime.UtcNow.Date;
            var end = DateTime.UtcNow;

            return await SearchTrades(accountId, start, end);
        }

        // ================= DAILY PNL =================
        public async Task<double> GetDailyPnL(long accountId)
        {
            var trades = await GetTodayTrades(accountId);

            return trades
                .Where(t => t.ProfitAndLoss.HasValue)
                .Sum(t => t.ProfitAndLoss.Value - t.Fees);
        }

        // ================= INTERNAL PARSER =================
        private List<TradeInfo> ParseTrades(string json)
        {
            var result = JsonSerializer.Deserialize<TradeResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var list = new List<TradeInfo>();

            if (result == null || !result.success || result.trades == null)
                return list;

            foreach (var t in result.trades)
            {
                list.Add(new TradeInfo
                {
                    Id = t.id,
                    AccountId = t.accountId,
                    ContractId = t.contractId,
                    CreationTime = t.creationTimestamp,
                    Price = t.price,
                    ProfitAndLoss = t.profitAndLoss,
                    Fees = t.fees,
                    Side = t.side,
                    Size = t.size,
                    Voided = t.voided,
                    OrderId = t.orderId
                });
            }

            return list;
        }

        // ================= DTO =================
        public class TradeInfo
        {
            public long Id { get; set; }
            public long AccountId { get; set; }
            public string ContractId { get; set; }
            public DateTime CreationTime { get; set; }
            public double Price { get; set; }
            public double? ProfitAndLoss { get; set; }
            public double Fees { get; set; }
            public int Side { get; set; }
            public int Size { get; set; }
            public bool Voided { get; set; }
            public long OrderId { get; set; }

            public string SideText => Side switch
            {
                OrderSide.Buy => "BUY",
                OrderSide.Sell => "SELL",
                _ => "UNKNOWN"
            };

            public bool IsEntry => !ProfitAndLoss.HasValue;
            public bool IsExit => ProfitAndLoss.HasValue;
        }

        // ================= JSON RESPONSE =================
        private class TradeResponse
        {
            public TradeData[] trades { get; set; }
            public bool success { get; set; }
            public int errorCode { get; set; }
            public string errorMessage { get; set; }
        }

        private class TradeData
        {
            public long id { get; set; }
            public long accountId { get; set; }
            public string contractId { get; set; }
            public DateTime creationTimestamp { get; set; }
            public double price { get; set; }
            public double? profitAndLoss { get; set; }
            public double fees { get; set; }
            public int side { get; set; }
            public int size { get; set; }
            public bool voided { get; set; }
            public long orderId { get; set; }
        }
    }
}