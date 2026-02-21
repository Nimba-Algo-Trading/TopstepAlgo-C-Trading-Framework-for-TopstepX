using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace TopstepAlgo
{
    public class Data
    {
        private readonly Connexion connexion;

        public Data(Connexion api)
        {
            connexion = api;
        }

        // ===== SEARCH CONTRACTS =====
        public async Task<List<ContractInfo>> SearchContracts(string searchText, bool live = false)
        {
            var url = "https://api.topstepx.com/api/Contract/search";

            var payload = new
            {
                live = live,
                searchText = searchText
            };

            string json = await connexion.PostAsync(url, payload);

            var result = JsonSerializer.Deserialize<ContractResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var list = new List<ContractInfo>();

            if (result == null || !result.success || result.contracts == null)
                return list;

            foreach (var c in result.contracts)
            {
                list.Add(new ContractInfo
                {
                    Id = c.id,
                    Name = c.name,
                    Description = c.description,
                    TickSize = c.tickSize,
                    TickValue = c.tickValue,
                    Active = c.activeContract,
                    SymbolId = c.symbolId
                });
            }

            return list;
        }
        public async Task<ContractInfo?> SearchContractById(string contractId)
        {
            var url = "https://api.topstepx.com/api/Contract/searchById";

            var payload = new
            {
                contractId = contractId
            };

            string json = await connexion.PostAsync(url, payload);

            var result = JsonSerializer.Deserialize<ContractByIdResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || !result.success || result.contract == null)
                return null;

            var c = result.contract;

            return new ContractInfo
            {
                Id = c.id,
                Name = c.name,
                Description = c.description,
                TickSize = c.tickSize,
                TickValue = c.tickValue,
                Active = c.activeContract,
                SymbolId = c.symbolId
            };
        }
        public async Task<ContractInfo?> GetActiveContract(string symbol, bool live = false)
        {
            var contracts = await SearchContracts(symbol, live);

            foreach (var c in contracts)
            {
                if (c.Active)
                    return c;
            }

            return null;
        }
        public async Task<bool> IsContractAvailable(string symbol, bool live = false)
        {
            var url = "https://api.topstepx.com/api/Contract/available";

            var payload = new
            {
                live = live
            };

            string json = await connexion.PostAsync(url, payload);

            var result = JsonSerializer.Deserialize<AvailableContractsResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (result == null || !result.success || result.contracts == null)
                return false;

            foreach (var c in result.contracts)
            {
                // ex: MNQU5 -> MNQ
                if (c.name.StartsWith(symbol))
                    return true;
            }

            return false;
        }
        public async Task<(double bid, double ask, double last)> GetQuote(string contractId)
        {
            var url = "https://api.topstepx.com/api/Quote/subscribeSnapshot";

            var payload = new { contractId = contractId };

            string json = await connexion.PostAsync(url, payload);

            // 🔴 protection anti réponse vide
            if (string.IsNullOrWhiteSpace(json))
            {
                Console.WriteLine("Quote API returned empty response");
                return (0, 0, 0);
            }

            try
            {
                var result = JsonSerializer.Deserialize<QuoteResponse>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null || !result.success || result.quote == null)
                    return (0, 0, 0);

                return (result.quote.bidPrice, result.quote.askPrice, result.quote.lastPrice);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Quote parse error: " + json);
                return (0, 0, 0);
            }
        }
        private class QuoteResponse
        {
            public QuoteData quote { get; set; }
            public bool success { get; set; }
            public int errorCode { get; set; }
            public string errorMessage { get; set; }
        }

        private class QuoteData
        {
            public string contractId { get; set; }
            public double bidPrice { get; set; }
            public double askPrice { get; set; }
            public double lastPrice { get; set; }
        }
        // ===== DTO PUBLIC =====
        public class ContractInfo
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public double TickSize { get; set; }
            public double TickValue { get; set; }
            public bool Active { get; set; }
            public string SymbolId { get; set; }
        }

        // ===== JSON MODELS =====
        private class ContractResponse
        {
            public ContractData[] contracts { get; set; }
            public bool success { get; set; }
            public int errorCode { get; set; }
            public string errorMessage { get; set; }
        }

        private class ContractData
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public double tickSize { get; set; }
            public double tickValue { get; set; }
            public bool activeContract { get; set; }
            public string symbolId { get; set; }
        }
        private class ContractByIdResponse
        {
            public ContractData contract { get; set; }
            public bool success { get; set; }
            public int errorCode { get; set; }
            public string errorMessage { get; set; }
        }
        private class AvailableContractsResponse
        {
            public ContractData[] contracts { get; set; }
            public bool success { get; set; }
            public int errorCode { get; set; }
            public string errorMessage { get; set; }
        }
    }
}