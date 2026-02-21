# TopstepAlgo SDK – Documentation Officielle Complète

**Auteur : ALIOU BA**
Email : [daytrader221@gmail.com](mailto:daytrader221@gmail.com)

---

# 1. Objectif du projet

TopstepAlgo est un framework C# permettant de créer des algorithmes de trading automatiques pour TopstepX sans manipuler directement l’API REST.

Le projet sert de couche d’abstraction :

API Topstep → Framework → Stratégie utilisateur

Ainsi le développeur ne code jamais la communication réseau, seulement la logique de trading.

---

# 2. Philosophie d’architecture

Le framework reproduit le fonctionnement réel d’une plateforme de trading professionnelle :

1. Connexion
2. Chargement du compte
3. Chargement des instruments
4. Surveillance marché
5. Execution ordres
6. Gestion positions
7. Gestion du risque

Chaque module a un rôle unique (Single Responsibility Principle).

---

# 3. Configuration obligatoire (LOGIN)

Créer un fichier :

login.json

Contenu :
{
"username": "met ton mail topstep",
"apiKey": "ton api key topstep"
}

Placer dans :

.NET 8 : bin/Debug/net8.0/login.json
.NET 6 : bin/Debug/net6.0/login.json

Il doit être dans le même dossier que l’exécutable.

Pourquoi ?
Le framework lit ce fichier au démarrage pour authentifier toutes les requêtes API.

---

# 4. Démarrage complet du framework (BOOT)

C’est la partie la plus importante.

Ordre OBLIGATOIRE :

Connexion → Accounts → Contracts → Modules → Stratégie

Exemple complet :

```csharp
Connexion api = new Connexion();
await api.connect();

Account accountApi = new Account(api);
Data data = new Data(api);
Positions positions = new Positions(api);
Ordres ordres = new Ordres(api);
SendOrder order = new SendOrder(api);
Trades trades = new Trades(api);
```

---

# 5. Récupérer un compte (accountId)

```csharp
var accounts = await accountApi.LoadAll();
var account = accounts.First(a => a.CanTrade);
long accountId = account.Id;
```

Pourquoi ?
Toutes les actions de trading exigent l’ID compte réel et non le nom.

---

# 6. Trouver un contrat à partir d’un symbole (ES, NQ, MNQ…)

C’est une étape critique.

Un trader pense en symbole → l’API fonctionne avec contractId.

Le framework fait la conversion automatiquement.

## Obtenir le contrat actif

```csharp
var contract = await data.GetActiveContract("ES");
string contractId = contract.Id;
```

Ce que fait la fonction :

1. Recherche tous les futures ES
2. Identifie l’échéance active (ex: ESH5)
3. Retourne le contrat tradable

Donc tu n’as jamais à gérer les rollovers futures.

---

# 7. Vérifier disponibilité du marché

```csharp
bool tradable = await data.IsContractAvailable("ES");
```

Empêche d’envoyer un ordre marché fermé.

---

# 8. Obtenir les prix temps réel

```csharp
var quote = await data.GetQuote(contractId);
Console.WriteLine($"Bid {quote.bid} Ask {quote.ask}");
```

Important :
Toujours récupérer un prix avant un ordre limit ou stop.

---

# 9. Placer un trade

## Market Order

```csharp
await order.Market(accountId, contractId, OrderSide.Buy, 1);
```

## Limit Order

```csharp
await order.Limit(accountId, contractId, OrderSide.Buy, 1, price);
```

## Stop Order (breakout ou stop loss)

```csharp
await order.Stop(accountId, contractId, OrderSide.Sell, 1, stopPrice);
```

## Trailing Stop

```csharp
await order.TrailingStop(accountId, contractId, OrderSide.Sell, 1, 20);
```

## Join Bid / Ask (scalping carnet)

```csharp
await order.JoinBid(accountId, contractId, 1);
await order.JoinAsk(accountId, contractId, 1);
```

---

# 10. Comprendre la différence Ordres vs Positions

Ordre = intention d’execution
Position = exposition réelle marché

Un ordre peut exister sans position (non exécuté)
Une position peut exister sans ordre (market filled)

---

# 11. Lire les ordres ouverts

```csharp
var openOrders = await ordres.SearchOpenOrders(accountId);
var symbolOrders = await ordres.GetOpensOrders(accountId, "ES");
```

Utilité : éviter double entrée stratégie.

---

# 12. Modifier un ordre

```csharp
await ordres.ModifyOrder(accountId, orderId, size:2, limitPrice:newPrice);
```

Cas d’usage :

* déplacer TP
* déplacer SL
* pyramider

---

# 13. Gestion des positions

## Lire toutes les positions

```csharp
var pos = await positions.SearchOpenPositions(accountId);
```

## Lire positions par symbole

```csharp
var esPos = await positions.GetPositions("ES", accountId);
```

## Calculer exposition

```csharp
int exposure = esPos.Sum(p => p.Size);
```

---

# 14. Fermer une position

## Fermeture totale

```csharp
await positions.ClosePosition(accountId, contractId);
```

## Fermeture partielle

```csharp
await positions.PartialClosePosition(accountId, contractId, 1);
```

---

# 15. Cycle de vie complet d’un trade

1. Vérifier marché ouvert
2. Vérifier aucune position existante
3. Récupérer contrat actif
4. Lire prix
5. Envoyer ordre
6. Poser protection
7. Gérer sortie

---

# 16. Exemple réel complet (entrée + stop automatique)

```csharp
var account = (await accountApi.LoadAll()).First(a => a.CanTrade);
var contract = await data.GetActiveContract("NQ");

var quote = await data.GetQuote(contract.Id);

await order.Market(account.Id, contract.Id, OrderSide.Buy, 1);

await order.Stop(account.Id, contract.Id, OrderSide.Sell, 1, quote.bid - 20);
```

---

# 17. Bonnes pratiques professionnelles

Toujours :

* vérifier positions avant entrée
* limiter exposition
* prévoir sortie avant entrée
* ne jamais empiler des ordres
* gérer marché fermé

---

# 18. Ce que permet le framework

Création rapide de :

* scalping bots
* breakout bots
* hedge bots
* pair trading
* mean reversion

Sans écrire une seule requête HTTP.

---

Fin de documentation officielle

