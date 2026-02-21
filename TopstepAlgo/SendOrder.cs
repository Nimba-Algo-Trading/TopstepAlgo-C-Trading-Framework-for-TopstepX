namespace TopstepAlgo
{
    public class Order
    {
        private readonly Connexion connexion;
        private const string URL = "https://api.topstepx.com/api/Order/place";

        public Order(Connexion api)
        {
            connexion = api;
        }

        // MARKET
        public Task<string> Market(long accountId, string contractId, int side, int size, string tag = null)
            => Send(accountId, contractId, OrderType.Market, side, size, null, null, null, tag);

        // LIMIT
        public Task<string> Limit(long accountId, string contractId, int side, int size, double price, string tag = null)
            => Send(accountId, contractId, OrderType.Limit, side, size, price, null, null, tag);

        // STOP
        public Task<string> Stop(long accountId, string contractId, int side, int size, double stop, string tag = null)
            => Send(accountId, contractId, OrderType.Stop, side, size, null, stop, null, tag);

        // TRAILING
        public Task<string> TrailingStop(long accountId, string contractId, int side, int size, double trail, string tag = null)
            => Send(accountId, contractId, OrderType.TrailingStop, side, size, null, null, trail, tag);

        // JOIN BID
        public Task<string> JoinBid(long accountId, string contractId, int size, string tag = null)
            => Send(accountId, contractId, OrderType.JoinBid, OrderSide.Buy, size, null, null, null, tag);

        // JOIN ASK
        public Task<string> JoinAsk(long accountId, string contractId, int size, string tag = null)
            => Send(accountId, contractId, OrderType.JoinAsk, OrderSide.Sell, size, null, null, null, tag);
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
        private async Task<string> Send(long accountId, string contractId, int type, int side, int size,
                                        double? limit, double? stop, double? trail, string tag)
        {
            var payload = new
            {
                accountId = accountId,
                contractId = contractId,
                type = type,
                side = side,
                size = size,
                limitPrice = limit,
                stopPrice = stop,
                trailPrice = trail,
                customTag = tag
            };

            return await connexion.PostAsync(URL, payload);
        }

        // ================= CANCEL ORDER =================
        public async Task<string> CancelOrder(long accountId, long orderId)
        {
            var url = "https://api.topstepx.com/api/Order/cancel";

            var payload = new
            {
                accountId = accountId,
                orderId = orderId
            };

            return await connexion.PostAsync(url, payload);
        }
    }
}