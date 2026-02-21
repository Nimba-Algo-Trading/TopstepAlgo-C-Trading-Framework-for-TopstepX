using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace TopstepAlgo
{
    public class Account
    {
        private readonly Connexion connexion;

        public Account(Connexion api)
        {
            connexion = api;
        }

        // ===== LOAD ALL ACCOUNTS =====
        public async Task<List<AccountInfo>> LoadAll(bool onlyActiveAccounts = true)
        {
            var url = "https://api.topstepx.com/api/Account/search";
            var payload = new { onlyActiveAccounts = onlyActiveAccounts };

            string json = await connexion.PostAsync(url, payload);

            var result = JsonSerializer.Deserialize<AccountResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var list = new List<AccountInfo>();

            if (result == null || !result.success || result.accounts == null)
                return list;

            foreach (var a in result.accounts)
            {
                list.Add(new AccountInfo
                {
                    Id = a.id,
                    Name = a.name,
                    Balance = a.balance,
                    CanTrade = a.canTrade,
                    Simulated = a.simulated
                });
            }

            return list;
        }
        public async Task<AccountInfo?> GetById(long id)
        {
            var list = await LoadAll();

            foreach (var a in list)
                if (a.Id == id)
                    return a;

            return null;
        }
        // ===== DTO PUBLIC (les comptes retournés) =====
        public class AccountInfo
        {
            public long Id { get; set; }
            public string Name { get; set; }
            public decimal Balance { get; set; }
            public bool CanTrade { get; set; }
            public bool Simulated { get; set; }
        }

        // ===== JSON MODELS =====
        private class AccountResponse
        {
            public AccountData[] accounts { get; set; }
            public bool success { get; set; }
            public int errorCode { get; set; }
            public string errorMessage { get; set; }
        }

        private class AccountData
        {
            public long id { get; set; }
            public string name { get; set; }
            public decimal balance { get; set; }
            public bool canTrade { get; set; }
            public bool isVisible { get; set; }
            public bool simulated { get; set; }
        }
    }
}