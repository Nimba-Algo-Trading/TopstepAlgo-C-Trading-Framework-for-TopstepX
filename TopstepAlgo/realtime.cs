using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace TopstepAlgo
{
    public class Realtime
    {
        private HubConnection? userHub;
        private HubConnection? marketHub;

        private readonly string token;
        private readonly long accountId;

        public Realtime(string jwtToken, long accountId)
        {
            token = jwtToken;
            this.accountId = accountId;
        }

        // ================= EVENTS =================
        public event Action<QuoteEvent>? OnQuote;
        public event Action<OrderEvent>? OnOrderUpdate;
        public event Action<PositionEvent>? OnPositionUpdate;

        // ================= CONNECT =================
        public async Task Connect()
        {
            await ConnectUserHub();
            await ConnectMarketHub();
        }

        // ================= USER HUB =================
        private async Task ConnectUserHub()
        {
            userHub = new HubConnectionBuilder()
                .WithUrl($"https://rtc.topstepx.com/hubs/user?access_token={token}")
                .WithAutomaticReconnect()
                .Build();

            userHub.On<OrderEvent>("GatewayUserOrder", data => OnOrderUpdate?.Invoke(data));
            userHub.On<PositionEvent>("GatewayUserPosition", data => OnPositionUpdate?.Invoke(data));

            await userHub.StartAsync();
            await userHub.InvokeAsync("SubscribeAccounts", accountId);
        }

        // ================= MARKET HUB =================
        private async Task ConnectMarketHub()
        {
            marketHub = new HubConnectionBuilder()
                .WithUrl($"https://rtc.topstepx.com/hubs/market?access_token={token}")
                .WithAutomaticReconnect()
                .Build();

            marketHub.On<QuoteEvent>("GatewayQuote", data => OnQuote?.Invoke(data));

            await marketHub.StartAsync();
        }

        // ================= SUBSCRIBE =================
        public async Task SubscribeContract(string contractId)
        {
            if (marketHub == null)
                throw new Exception("MarketHub not connected");

            await marketHub.InvokeAsync("SubscribeContract", contractId);
        }
    }

    // ================= DTO =================

    public class QuoteEvent
    {
        public string contractId { get; set; } = "";
        public double bidPrice { get; set; }
        public double askPrice { get; set; }
        public double lastPrice { get; set; }
    }

    public class OrderEvent
    {
        public long id { get; set; }
        public int status { get; set; }
        public int side { get; set; }
        public int type { get; set; }
        public double? filledPrice { get; set; }
    }

    public class PositionEvent
    {
        public string contractId { get; set; } = "";
        public int size { get; set; }
        public double averagePrice { get; set; }
    }
}