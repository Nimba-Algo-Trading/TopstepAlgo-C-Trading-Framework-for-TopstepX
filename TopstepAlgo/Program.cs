using System;
using System.Drawing;
using System.Threading.Tasks;
using TopstepAlgo;

namespace TopstepAlgo
{

    class Program
    {

        static int Volume(List<Positions.PositionInfo> pos)
        {
            return pos.Sum(p => Math.Abs(p.Size));
        }

        static bool HasWorkingOrder(List<Ordres.OrderInfo> orders)
        {
            return orders.Any(o =>
                o.Status == OrderStatus.Working ||
                o.Status == OrderStatus.Pending);
        }

        static private Connexion topstep = new Connexion();
        static private Account account = null;
        static private double ask = 0;
        static private double bid = 0;
        static private double last = 0;
        static private double ask2 = 0;
        static private double bid2 = 0;
        static private double last2 = 0;

        static private bool update1= false;
        static private bool update2= false;

        static async Task Main()
        {
            await Pair_Trading();
        }






        static async Task Pair_Trading()
        {
            await topstep.connect();
            if (!topstep.OK)
                return;

            account ??= new Account(topstep);
            await Task.Delay(1000);

            var data = new Data(topstep);
            var ordres = new Ordres(topstep);
            var positions = new Positions(topstep);
            var trader = new Order(topstep);
            var trades = new Trades(topstep);
            var market = new MarketPrice(topstep.Token);
            //var market2 = new MarketPrice(topstep.Token);

            var compte = await account.GetById(Settings.CompteId);
            await Task.Delay(500);

            Console.WriteLine($" compte: {compte.Id}, name:{compte.Name} capital:{compte.Balance} sim: {compte.Simulated} peut trader: {compte.CanTrade} ");

            if (!compte.CanTrade || compte.Balance <= 0)
            {
                Console.WriteLine("Compte non éligible pour le trading");
                return;
            }

            var ACTIF1 = await data.GetActiveContract(Settings.Symbol1);
            await Task.Delay(500);

            if (ACTIF1 == null || ACTIF2 == null)
            {
                Console.WriteLine("Aucun contrat actif");
                return;
            }

            bool tradable1 = await data.IsContractAvailable(Settings.Symbol1);
            await Task.Delay(1000);
            bool tradable2 = await data.IsContractAvailable(Settings.Symbol2);
            await Task.Delay(1000);


            if (!tradable1 || !tradable2)
            {
                Console.WriteLine("Un des contrats n'est pas disponible pour le trading");
                return;
            }

            Console.WriteLine($"Actif 1: {ACTIF1.Name} | {ACTIF1.Id} | tick size: {ACTIF1.TickSize} | tick value: {ACTIF1.TickValue}");
            Console.WriteLine($"Actif 2: {ACTIF2.Name} | {ACTIF2.Id} | tick size: {ACTIF2.TickSize} | tick value: {ACTIF2.TickValue}");

            await market.Connect(ACTIF1.Id);
            await Task.Delay(500);
            await market2.Connect(ACTIF2.Id);
            // ================= BOUCLE PRINCIPALE =================
            while(true)
            {
                await Task.Delay(1000);
            }
        }

        static async void ResetFlagAfterDelay(Action resetAction, int milliseconds)
        {
            await Task.Delay(milliseconds);
            resetAction();
        }
    }
}