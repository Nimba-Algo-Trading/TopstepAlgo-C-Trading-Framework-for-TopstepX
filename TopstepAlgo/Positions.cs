using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace TopstepAlgo
{
    public class Positions
    {
        private readonly Connexion connexion;

        public Positions(Connexion api)
        {
            connexion = api;
        }

        // ================= SEARCH OPEN POSITIONS =================
        public async Task<List<PositionInfo>> SearchOpenPositions(long accountId)
        {
            var url = "https://api.topstepx.com/api/Position/searchOpen";

            var payload = new
            {
                accountId = accountId
            };

            string json = await connexion.PostAsync(url, payload);

            return ParsePositions(json);
        }

        // ================= FILTER BY SYMBOL =================
        public async Task<List<PositionInfo>> GetPositions(string symbol, long accountId)
        {
            var list = await SearchOpenPositions(accountId);
            var filtered = new List<PositionInfo>();

            foreach (var p in list)
            {
                if (p.ContractId.Contains($".{symbol}."))
                    filtered.Add(p);
            }

            return filtered;
        }

        // ================= INTERNAL PARSER =================
        private List<PositionInfo> ParsePositions(string json)
        {
            var result = JsonSerializer.Deserialize<PositionResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var list = new List<PositionInfo>();

            if (result == null || !result.success || result.positions == null)
                return list;

            foreach (var p in result.positions)
            {
                list.Add(new PositionInfo
                {
                    Id = p.id,
                    AccountId = p.accountId,
                    ContractId = p.contractId,
                    CreationTime = p.creationTimestamp,
                    Type = p.type,
                    Size = p.size,
                    AveragePrice = p.averagePrice
                });
            }

            return list;
        }
        // ================= CLOSE POSITION =================
        public async Task<string> ClosePosition(long accountId, string contractId)
        {
            var url = "https://api.topstepx.com/api/Position/closeContract";

            var payload = new
            {
                accountId = accountId,
                contractId = contractId
            };

            return await connexion.PostAsync(url, payload);
        }
        // ================= PARTIAL CLOSE POSITION =================
        public async Task<string> PartialClosePosition(long accountId, string contractId, int size)
        {
            var url = "https://api.topstepx.com/api/Position/partialCloseContract";

            var payload = new
            {
                accountId = accountId,
                contractId = contractId,
                size = size
            };

            return await connexion.PostAsync(url, payload);
        }
        // ================= DTO =================
        public class PositionInfo
        {
            public long Id { get; set; }
            public long AccountId { get; set; }
            public string ContractId { get; set; }
            public DateTime CreationTime { get; set; }

            public int Type { get; set; } // 1 long, 2 short
            public int Size { get; set; }
            public double AveragePrice { get; set; }

            // ===== HELPERS =====
            public bool IsLong => Type == 1;
            public bool IsShort => Type == 2;

            public string SideText => Type switch
            {
                1 => "LONG",
                2 => "SHORT",
                _ => "UNKNOWN"
            };
        }

        // ================= JSON RESPONSE =================
        private class PositionResponse
        {
            public PositionData[] positions { get; set; }
            public bool success { get; set; }
            public int errorCode { get; set; }
            public string errorMessage { get; set; }
        }

        private class PositionData
        {
            public long id { get; set; }
            public long accountId { get; set; }
            public string contractId { get; set; }
            public DateTime creationTimestamp { get; set; }
            public int type { get; set; }
            public int size { get; set; }
            public double averagePrice { get; set; }
        }
    }
}