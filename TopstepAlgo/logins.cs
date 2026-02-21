using System.IO;
using System.Text.Json;

namespace TopstepAlgo
{
    public static class InfosLogin
    {
        public static string Username { get; private set; }
        public static string ApiKey { get; private set; }

        public static void Load()
        {
            if (!File.Exists("login.json"))
                throw new FileNotFoundException("login.json introuvable");

            var json = File.ReadAllText("login.json");

            var data = JsonSerializer.Deserialize<LoginFile>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Username = data.username;
            ApiKey = data.apiKey;
        }

        private class LoginFile
        {
            public string username { get; set; }
            public string apiKey { get; set; }
        }
    }
}