using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace TopstepAlgo
{
    public class Ordres
    {
        private readonly Connexion connexion;

        public Ordres(Connexion api)
        {
            connexion = api;
        }

        // ================= SEARCH ORDERS (HISTORY) =================
        public async Task<List<OrderInfo>> SearchOrders(long accountId, DateTime start, DateTime? end = null)
        {
            var url = "https://api.topstepx.com/api/Order/search";

            var payload = new
            {
                accountId = accountId,
                startTimestamp = start.ToString("O"),
                endTimestamp = end?.ToString("O")
            };

            string json = await connexion.PostAsync(url, payload);

            return ParseOrders(json);
        }

        // ================= SEARCH OPEN ORDERS =================
        public async Task<List<OrderInfo>> SearchOpenOrders(long accountId)
        {
            var url = "https://api.topstepx.com/api/Order/searchOpen";

            var payload = new { accountId = accountId };

            string json = await connexion.PostAsync(url, payload);

            return ParseOrders(json);
        }

        // ================= FILTER SYMBOL OPEN ORDERS =================
        public async Task<List<OrderInfo>> GetOpensOrders(long accountId, string symbol)
        {
            var list = await SearchOpenOrders(accountId);
            var filtered = new List<OrderInfo>();

            foreach (var o in list)
            {
                if (o.ContractId.Contains($".{symbol}.") &&
                    o.Status != OrderStatus.Filled &&
                    o.Status != OrderStatus.Cancelled)
                {
                    filtered.Add(o);
                }
            }

            return filtered;
        }

        // ================= MODIFY ORDER =================
        public async Task<string> ModifyOrder(long accountId, long orderId,
                                              int? size = null,
                                              double? limitPrice = null,
                                              double? stopPrice = null,
                                              double? trailPrice = null)
        {
            var url = "https://api.topstepx.com/api/Order/modify";

            var payload = new
            {
                accountId = accountId,
                orderId = orderId,
                size = size,
                limitPrice = limitPrice,
                stopPrice = stopPrice,
                trailPrice = trailPrice
            };

            return await connexion.PostAsync(url, payload);
        }

        // ================= INTERNAL PARSER =================
        private List<OrderInfo> ParseOrders(string json)
        {
            var result = JsonSerializer.Deserialize<OrderResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var list = new List<OrderInfo>();

            if (result == null || !result.success || result.orders == null)
                return list;

            foreach (var o in result.orders)
            {
                list.Add(new OrderInfo
                {
                    Id = o.id,
                    AccountId = o.accountId,
                    ContractId = o.contractId,
                    SymbolId = o.symbolId,
                    CreationTime = o.creationTimestamp,
                    UpdateTime = o.updateTimestamp,
                    Status = o.status,
                    Type = o.type,
                    Side = o.side,
                    Size = o.size,
                    LimitPrice = o.limitPrice,
                    StopPrice = o.stopPrice,
                    FilledPrice = o.filledPrice,
                    FillVolume = o.fillVolume,
                    CustomTag = o.customTag
                });
            }

            return list;
        }

        // ================= DTO =================
        public class OrderInfo
        {
            public long Id { get; set; }
            public long AccountId { get; set; }
            public string ContractId { get; set; }
            public string SymbolId { get; set; }
            public DateTime CreationTime { get; set; }
            public DateTime UpdateTime { get; set; }

            public int Status { get; set; }
            public int Type { get; set; }
            public int Side { get; set; }

            public int Size { get; set; }
            public double? LimitPrice { get; set; }
            public double? StopPrice { get; set; }
            public double? FilledPrice { get; set; }
            public int FillVolume { get; set; }
            public string CustomTag { get; set; }

            // ===== STATE HELPERS =====
            public bool IsWorking => Status == OrderStatus.Working;
            public bool IsFilled => Status == OrderStatus.Filled;
            public bool IsClosed => Status == OrderStatus.Cancelled || Status == OrderStatus.Filled;

            // ===== TEXT HELPERS =====
            public string SideText => Side switch
            {
                OrderSide.Buy => "BUY",
                OrderSide.Sell => "SELL",
                _ => "UNKNOWN"
            };

            public string TypeText => Type switch
            {
                OrderType.Market => "MARKET",
                OrderType.Limit => "LIMIT",
                OrderType.Stop => "STOP",
                OrderType.TrailingStop => "TRAILING STOP",
                OrderType.JoinBid => "JOIN BID",
                OrderType.JoinAsk => "JOIN ASK",
                _ => $"TYPE({Type})"
            };

            public string StatusText => Status switch
            {
                OrderStatus.Pending => "PENDING",
                OrderStatus.Working => "WORKING",
                OrderStatus.Filled => "FILLED",
                OrderStatus.Cancelled => "CANCELLED",
                OrderStatus.Rejected => "REJECTED",
                _ => $"STATUS({Status})"
            };
        }

        // ================= JSON RESPONSE =================
        private class OrderResponse
        {
            public OrderData[] orders { get; set; }
            public bool success { get; set; }
            public int errorCode { get; set; }
            public string errorMessage { get; set; }
        }

        private class OrderData
        {
            public long id { get; set; }
            public long accountId { get; set; }
            public string contractId { get; set; }
            public string symbolId { get; set; }
            public DateTime creationTimestamp { get; set; }
            public DateTime updateTimestamp { get; set; }
            public int status { get; set; }
            public int type { get; set; }
            public int side { get; set; }
            public int size { get; set; }
            public double? limitPrice { get; set; }
            public double? stopPrice { get; set; }
            public int fillVolume { get; set; }
            public double? filledPrice { get; set; }
            public string customTag { get; set; }
        }
    }
}