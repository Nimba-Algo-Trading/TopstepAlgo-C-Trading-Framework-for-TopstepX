namespace TopstepAlgo
{
    // ===== ORDER TYPES =====
    public static class OrderType
    {
        public const int Limit = 1;
        public const int Market = 2;
        public const int Stop = 4;
        public const int TrailingStop = 5;
        public const int JoinBid = 6;
        public const int JoinAsk = 7;
    }

    // ===== ORDER SIDE =====
    public static class OrderSide
    {
        public const int Buy = 0;
        public const int Sell = 1;
    }

    // ===== ORDER STATUS =====
    public static class OrderStatus
    {
        public const int Pending = 0;
        public const int Working = 1;
        public const int Filled = 2;
        public const int Cancelled = 3;
        public const int Rejected = 4;
    }
}