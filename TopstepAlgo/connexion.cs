using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TopstepAlgo
{
    public class Connexion
    {
        private static readonly HttpClient http = new HttpClient();
        private CancellationTokenSource _cts;

        public bool OK { get; private set; } = false;
        public string Token { get; private set; }

        private const string LOGIN_URL = "https://api.topstepx.com/api/Auth/loginKey";
        private const string VALIDATE_URL = "https://api.topstepx.com/api/Auth/validate";

        // ================= CONNECT =================
        public async Task connect()
        {
            InfosLogin.Load();

            OK = await LoginAsync(InfosLogin.Username, InfosLogin.ApiKey);

            if (!OK)
            {
                Console.WriteLine("Connexion échouée");
                return;
            }

            Console.WriteLine("Connecté à TopstepX ✔");

            _cts = new CancellationTokenSource();
            _ = Task.Run(() => SessionKeeper(_cts.Token));
        }

        // ================= LOGIN =================
        private async Task<bool> LoginAsync(string username, string apiKey)
        {
            try
            {
                var payload = new { userName = username, apiKey = apiKey };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await http.PostAsync(LOGIN_URL, content);
                var body = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<AuthResult>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null || !result.success)
                {
                    Console.WriteLine("AUTH FAILED: " + result?.errorMessage);
                    return false;
                }

                Token = result.token;

                http.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Token);

                http.DefaultRequestHeaders.Accept.Clear();
                http.DefaultRequestHeaders.Accept.Add(
                    new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/plain"));

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Login error: " + ex.Message);
                return false;
            }
        }

        // ================= VALIDATE =================
        private async Task<bool> ValidateAsync()
        {
            try
            {
                var response = await http.PostAsync(VALIDATE_URL,
                    new StringContent("", Encoding.UTF8, "application/json"));

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        // ================= KEEP SESSION =================
        private async Task SessionKeeper(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(10), ct);

                if (!await ValidateAsync())
                {
                    Console.WriteLine("Session expirée → reconnect...");
                    OK = await LoginAsync(InfosLogin.Username, InfosLogin.ApiKey);
                }
                else
                {
                    Console.WriteLine("Session OK");
                }
            }
        }
        // ================= HTTP HELPERS =================
        // ================= HTTP HELPERS =================
        public async Task<string> GetAsync(string url)
        {
            try
            {
                var response = await http.GetAsync(url);
                var body = await response.Content.ReadAsStringAsync();

                //Console.WriteLine("GET " + url);
                //Console.WriteLine("STATUS: " + (int)response.StatusCode + " " + response.StatusCode);
                //Console.WriteLine("BODY: " + (string.IsNullOrWhiteSpace(body) ? "<EMPTY>" : body));
                //Console.WriteLine();

                return body;
            }
            catch (Exception ex)
            {
                return "GET ERROR: " + ex.Message;
            }
        }

        public async Task<string> PostAsync(string url, object data)
        {
            try
            {
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await http.PostAsync(url, content);
                var body = await response.Content.ReadAsStringAsync();

               // Console.WriteLine("POST " + url);
                //Console.WriteLine("STATUS: " + (int)response.StatusCode + " " + response.StatusCode);
                //Console.WriteLine("BODY: " + (string.IsNullOrWhiteSpace(body) ? "<EMPTY>" : body));
                //Console.WriteLine();

                return body;
            }
            catch (Exception ex)
            {
                return "POST ERROR: " + ex.Message;
            }
        }
        private class AuthResult
        {
            public string token { get; set; }
            public bool success { get; set; }
            public int errorCode { get; set; }
            public string errorMessage { get; set; }
        }
    }
}