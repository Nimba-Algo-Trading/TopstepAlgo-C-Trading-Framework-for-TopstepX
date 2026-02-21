using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;

namespace TopstepAlgo
{
    public class MarketPrice
    {
        private HubConnection hub;
        private readonly string token;

        public double Bid { get; private set; }
        public double Ask { get; private set; }
        public double Last { get; private set; }

        public bool IsReady => Bid > 0 && Ask > 0;

        public MarketPrice(string jwtToken)
        {
            token = jwtToken;
        }

        public async Task Connect(string contractId)
        {
            hub = new HubConnectionBuilder()
                .WithUrl($"https://rtc.topstepx.com/hubs/market?access_token={token}")
                .WithAutomaticReconnect()
                .Build();

            // réception prix temps réel
            hub.On<QuoteDto>("GatewayQuote", q =>
            {
                if (q.contractId == contractId)
                {
                    Bid = q.bidPrice;
                    Ask = q.askPrice;
                    Last = q.lastPrice;
                }
            });

            await hub.StartAsync();
            await hub.InvokeAsync("SubscribeContract", contractId);
        }
    }

    public class QuoteDto
    {
        public string contractId { get; set; }
        public double bidPrice { get; set; }
        public double askPrice { get; set; }
        public double lastPrice { get; set; }
    }
}