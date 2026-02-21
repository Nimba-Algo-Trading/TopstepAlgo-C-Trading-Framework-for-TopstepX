using System;
using System.Text.Json;
using System.Threading.Tasks;
using TopstepAlgo;

public class PairEngine
{
    private readonly Order trader;
    private readonly Positions positions;
    private readonly Ordres ordres;

    private readonly string contract1;
    private readonly string contract2;
    private readonly long accountId;

    private readonly double tickSize1;
    private readonly double tickValue1;

    private PairState state = PairState.Flat;

    private double lastBid1;
    private double lastAsk1;

    private long entryOrderId = 0;
    private long exitOrderId = 0;

    private double entryPrice1 = 0;
    private int side1 = OrderSide.Buy;

    public PairEngine(
        Order trader,
        Positions positions,
        Ordres ordres,
        Data.ContractInfo actif1,
        Data.ContractInfo actif2,
        long accountId)
    {
        if (string.IsNullOrEmpty(actif1.Id) || string.IsNullOrEmpty(actif2.Id))
            throw new Exception("Invalid contract id");

        this.trader = trader;
        this.positions = positions;
        this.ordres = ordres;

        this.contract1 = actif1.Id;
        this.contract2 = actif2.Id;
        this.accountId = accountId;

        tickSize1 = actif1.TickSize;
        tickValue1 = actif1.TickValue;
    }

    // =========================================================
    // PRICE UPDATE
    // =========================================================
    public async Task OnQuote(string contractId, double bid, double ask)
    {
        if (contractId != contract1)
            return;

        lastBid1 = bid;
        lastAsk1 = ask;

        if (state == PairState.InPosition || state == PairState.Exiting)
            await ManageExit();
    }

    // =========================================================
    // ENTRY SIGNAL
    // =========================================================
    public async Task TryEnter(double entryPrice, int side)
    {
        if (state != PairState.Flat)
            return;

        Console.WriteLine("PLACE ENTRY LIMIT");

        var res = await trader.Limit(accountId, contract1, side, Settings.Quantity, entryPrice);

        entryOrderId = ExtractOrderId(res);
        side1 = side;

        state = PairState.PendingEntry;
    }

    // =========================================================
    // ORDER FILLED EVENT
    // =========================================================
    public async Task OnOrderFilled(long orderId)
    {
        // ENTRY FILLED
        if (orderId == entryOrderId)
        {
            Console.WriteLine("ENTRY FILLED → HEDGE");

            entryPrice1 = side1 == OrderSide.Buy ? lastAsk1 : lastBid1;

            int hedgeSide = Settings.Correlation
                ? (side1 == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy)
                : side1;

            await trader.Market(accountId, contract2, hedgeSide, Settings.Quantity2);

            state = PairState.InPosition;
            return;
        }

        // EXIT FILLED
        if (orderId == exitOrderId)
        {
            Console.WriteLine("EXIT1 FILLED → CLOSE HEDGE");

            await positions.ClosePosition(accountId, contract2);

            exitOrderId = 0;
            entryOrderId = 0;
            state = PairState.Flat;
        }
    }

    // =========================================================
    // CALCUL PNL ACTIF 1
    // =========================================================
    private double GetPnL1()
    {
        if (side1 == OrderSide.Buy)
            return ((lastAsk1 - entryPrice1) / tickSize1) * tickValue1;
        else
            return ((entryPrice1 - lastBid1) / tickSize1) * tickValue1;
    }

    // =========================================================
    // GESTION SORTIE
    // =========================================================
    private async Task ManageExit()
    {
        double pnl1 = GetPnL1();

        Console.WriteLine($"PnL1 = {pnl1}");

        // TP atteint → placer LIMIT inverse
        if (pnl1 >= Settings.PROFIT)
        {
            if (exitOrderId == 0)
            {
                Console.WriteLine("PLACE EXIT LIMIT");

                int exitSide = side1 == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
                double price = side1 == OrderSide.Buy ? lastBid1 : lastAsk1;

                var res = await trader.Limit(accountId, contract1, exitSide, Settings.Quantity, price);
                exitOrderId = ExtractOrderId(res);

                state = PairState.Exiting;
            }
        }
        else
        {
            // pnl redescend → cancel
            if (exitOrderId != 0)
            {
                Console.WriteLine("CANCEL EXIT LIMIT");

                await trader.CancelOrder(accountId, exitOrderId);
                exitOrderId = 0;

                state = PairState.InPosition;
            }
        }
    }

    // =========================================================
    private long ExtractOrderId(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("orderId").GetInt64();
        }
        catch
        {
            return 0;
        }
    }
}

enum PairState
{
    Flat,
    PendingEntry,
    InPosition,
    Exiting
}