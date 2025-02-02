using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Driver;

public class BattleHub : Hub
{

    private static readonly List<Waiting> waitingPlayers = [];
    private static readonly List<BattleInfo> lifeMonBattles = [];
    private readonly IMongoDatabase _database;

    public BattleHub(IMongoDatabase database)
    {
        _database = database;
    }

    // Method for players to log in and wait
    public async Task Login(string playerId)
    {


        waitingPlayers.Add(new Waiting
        {
            ConnectionId = Context.ConnectionId,
            UserId = playerId
        });

        // Try to find a match
        if (waitingPlayers.Count >= 2)
        {
            var player1 = waitingPlayers[0];
            var player2 = waitingPlayers[1];
            waitingPlayers.RemoveAt(0);
            waitingPlayers.RemoveAt(0);

            // Notify the players they have been matched


            //get pokemon info and team

            var teamsCollection = _database.GetCollection<Team>("Teams");
            var lifeMonCollection = _database.GetCollection<LifeMon>("LifeMons");

            var player1Team = await teamsCollection
                .Find(t => t.UserId.ToString() == player1.UserId)
                .FirstOrDefaultAsync();

            var player2Team = await teamsCollection
                .Find(t => t.UserId.ToString() == player2.UserId)
                .FirstOrDefaultAsync();


            BattleInfo battleInfo = new()
            {
                Player1Id = player1.UserId,
                Connection1Id = player1.ConnectionId,
                Connection2Id = player2.ConnectionId,
                Player2Id = player2.UserId,
                Turns = [],
                Player1LifeMons = [.. player1Team.LifeMons.Select((t, index) => new LifeMonBattle
               {
                   AttackBoost = 0,
                   DefenseBoost = 0,
                   IsDead = false,
                   CurrentHp = t.Hp,
                   Lifemon = t,
                   IsInTheGame = index == 0,
               })],
                Player2LifeMons = [.. player2Team.LifeMons.Select((t, index) => new LifeMonBattle
               {
                   AttackBoost = 0,
                   DefenseBoost = 0,
                   IsDead = false,
                    CurrentHp = t.Hp,
                   Lifemon = t,
                   IsInTheGame = index == 0,
               })],
            };

            lifeMonBattles.Add(battleInfo);


            await Clients.Client(player1.ConnectionId).SendAsync("MatchFound", player2.UserId);
            await Clients.Client(player2.ConnectionId).SendAsync("MatchFound", player1.UserId);
        }
        else
        {
            // Notify the player they are waiting
            await Clients.Client(Context.ConnectionId).SendAsync("WaitingForMatch");
        }
    }



    // Method for players to log in and wait
    public async Task TakeTurn(string playerId, TurnType turnType, ObjectId pokemonId, string? newPokemonId, string? attackId)
    {

        //1- take the record concerned
        var pokemonBattle = lifeMonBattles.FirstOrDefault((pok) => pok.Player1Id == playerId || pok.Player2Id == playerId);




        if (pokemonBattle is null)
        {
            return;
        }


        //If he surrenders, tell everyone
        if (turnType == TurnType.Surrender)
        {
            var connectionWin = pokemonBattle.Connection1Id != Context.ConnectionId ? pokemonBattle.Connection1Id : pokemonBattle.Connection2Id;
            await Clients.Client(connectionWin).SendAsync("Win");
            await Clients.Client(Context.ConnectionId).SendAsync("Lose");
            return;
        }





        //2- vérifier si c'est un new turn (les 2 users ont jouée ce tour)
        Turn? turn = pokemonBattle.Turns.LastOrDefault();


        if (turn is null || (turn.Player1Turn is not null && turn.Player2Turn is not null))
        {
            turn = new Turn();
            var turnsList = pokemonBattle.Turns.ToList();
            turnsList.Add(turn);
            pokemonBattle.Turns = [.. turnsList];
        }


        //3- fais les jouer leur tour
        if (playerId == pokemonBattle.Player1Id)
        {
            turn.Player1Turn = new TurnInfo
            {
                PlayerId = playerId,
                PokemonId = pokemonId,
                AttackId = attackId,
                TurnType = turnType,
                NewPokemonId = newPokemonId
            };
        }
        else
        {
            turn.Player2Turn = new TurnInfo
            {
                PlayerId = playerId,
                PokemonId = pokemonId,
                AttackId = attackId,
                TurnType = turnType,
                NewPokemonId = newPokemonId
            };
        }


        //4- Si l'autre personne a joué, attend. Sinon, joue le tour et avertie les 2 personnes
        if (turn.Player1Turn is not null && turn.Player2Turn is not null)
        {
            await MakeTurn(pokemonBattle, turn.Player1Turn, turn.Player2Turn);
        }
        else
        {
            await Clients.Client(Context.ConnectionId).SendAsync("WaitingForPlayer", pokemonBattle.Player1Id, pokemonBattle.Player2Id);
        }
    }




    // Method for players to log in and wait
    public async Task Forfeit()
    {

        //1- take the record concerned
        var pokemonBattle = lifeMonBattles.FirstOrDefault((pok) => pok.Connection1Id == Context.ConnectionId || pok.Player2Id == Context.ConnectionId);


        if (pokemonBattle is null)
        {
            return;
        }

        var connectionWin = pokemonBattle.Connection1Id != Context.ConnectionId ? pokemonBattle.Connection1Id : pokemonBattle.Connection2Id;
        await Clients.Client(connectionWin).SendAsync("Win");
        await Clients.Client(Context.ConnectionId).SendAsync("Lose");
        return;
    }



    // Method for players to log in and wait
    public async Task Attack(string moveName)
    {

        //1- take the record concerned
        var pokemonBattle = lifeMonBattles.FirstOrDefault((pok) => pok.Connection1Id == Context.ConnectionId || pok.Player2Id == Context.ConnectionId);


        if (pokemonBattle is null)
        {
            return;
        }


        //2- vérifier si c'est un new turn (les 2 users ont jouée ce tour)
        Turn? turn = pokemonBattle.Turns.LastOrDefault();


        if (turn is null || (turn.Player1Turn is not null && turn.Player2Turn is not null))
        {
            turn = new Turn();
            var turnsList = pokemonBattle.Turns.ToList();
            turnsList.Add(turn);
            pokemonBattle.Turns = [.. turnsList];
        }




        //3- fais les jouer leur tour
        if (Context.ConnectionId == pokemonBattle.Connection1Id)
        {
            turn.Player1Turn = new TurnInfo
            {
                PlayerId = pokemonBattle.Player1Id,
                PokemonId = pokemonBattle.Player1LifeMons.FirstOrDefault(t => t.IsInTheGame).Lifemon.Id,
                AttackId = moveName,
                TurnType = TurnType.Attack,
                NewPokemonId = null
            };
        }
        else
        {
            turn.Player2Turn = new TurnInfo
            {
                PlayerId = pokemonBattle.Player2Id,
                PokemonId = pokemonBattle.Player2LifeMons.FirstOrDefault(t => t.IsInTheGame).Lifemon.Id,
                AttackId = moveName,
                TurnType = TurnType.Attack,
                NewPokemonId = null
            };
        }


        //4- Si l'autre personne a joué, attend. Sinon, joue le tour et avertie les 2 personnes
        if (turn.Player1Turn is not null && turn.Player2Turn is not null)
        {
            await MakeTurn(pokemonBattle, turn.Player1Turn, turn.Player2Turn);
        }
        else
        {
            await Clients.Client(Context.ConnectionId).SendAsync("WaitingForPlayer", pokemonBattle.Player1Id, pokemonBattle.Player2Id);
        }
    }




    // Method for players to log in and wait
    public async Task Switch(string pokemonId)
    {

        //1- take the record concerned
        var pokemonBattle = lifeMonBattles.FirstOrDefault((pok) => pok.Connection1Id == Context.ConnectionId || pok.Player2Id == Context.ConnectionId);


        if (pokemonBattle is null)
        {
            return;
        }


        var connectionWin = pokemonBattle.Connection1Id != Context.ConnectionId ? pokemonBattle.Connection1Id : pokemonBattle.Connection2Id;
        await Clients.Client(connectionWin).SendAsync("Win");
        await Clients.Client(Context.ConnectionId).SendAsync("Lose");
        return;
    }




    // Method for players to log in and wait
    public async Task GetBattleInfo()
    {
        var battleInfo = lifeMonBattles.FirstOrDefault((r) => r.Connection2Id == Context.ConnectionId || r.Connection1Id == Context.ConnectionId);




        if (battleInfo is not null)
        {
            await Clients.Client(battleInfo.Connection1Id).SendAsync("BattleInfo", battleInfo);
            await Clients.Client(battleInfo.Connection2Id).SendAsync("BattleInfo", battleInfo);

        }
    }



    // You can also implement a Disconnect method to remove players from the waiting list on disconnect
    public async override Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;

        waitingPlayers.RemoveAll(e => e.ConnectionId == connectionId);

        var matchInProgress = lifeMonBattles.Find(e => e.Connection1Id == connectionId || e.Connection2Id == connectionId);
        if (matchInProgress is not null)
        {
            var connection1 = matchInProgress.Connection1Id;
            var connection2 = matchInProgress.Connection2Id;
            lifeMonBattles.RemoveAll(e => e.Connection1Id == connectionId || e.Connection2Id == connectionId);
            await Clients.Client(connection1).SendAsync("Win");
            await Clients.Client(connection2).SendAsync("Win");

        }
        await base.OnDisconnectedAsync(exception);
    }






    public async Task MakeTurn(BattleInfo pokemonBattle, TurnInfo turnInfoPlayer1, TurnInfo turnInfoPlayer2)
    {
        //1- Vérifier si un user change de pokemon



        Console.WriteLine("TURN IN PROGRESS");
        if (turnInfoPlayer1.TurnType == TurnType.Change && turnInfoPlayer1.NewPokemonId is not null)
        {
            var lifemon = pokemonBattle.Player1LifeMons.First((lif) => lif.IsInTheGame);
            lifemon.IsInTheGame = false;
            var newLifemon = pokemonBattle.Player1LifeMons.First((lif) => lif.Lifemon.Id.ToString() == turnInfoPlayer1.NewPokemonId);
        }

        if (turnInfoPlayer2.TurnType == TurnType.Change && turnInfoPlayer2.NewPokemonId is not null)
        {
            var lifemon = pokemonBattle.Player2LifeMons.First((lif) => lif.IsInTheGame);
            lifemon.IsInTheGame = false;
            var newLifemon = pokemonBattle.Player2LifeMons.First((lif) => lif.Lifemon.Id.ToString() == turnInfoPlayer2.NewPokemonId);
        }

        var lifeMonPlayer1 = pokemonBattle.Player1LifeMons.First((lif) => lif.IsInTheGame);
        var lifeMonPlayer2 = pokemonBattle.Player2LifeMons.First((lif) => lif.IsInTheGame);


        //2- Vérifier qui attack en premier
        var firstPlayer = lifeMonPlayer1.Lifemon.Speed > lifeMonPlayer2.Lifemon.Speed
            ? new { LifeMon = lifeMonPlayer1, canPlay = turnInfoPlayer1.TurnType != TurnType.Change }
            : new { LifeMon = lifeMonPlayer2, canPlay = turnInfoPlayer2.TurnType != TurnType.Change };

        var secondPlayer = lifeMonPlayer1.Lifemon.Speed > lifeMonPlayer2.Lifemon.Speed
            ? new { LifeMon = lifeMonPlayer2, canPlay = turnInfoPlayer2.TurnType != TurnType.Change }
            : new { LifeMon = lifeMonPlayer1, canPlay = turnInfoPlayer1.TurnType != TurnType.Change };




        //3- Laisser le premier attaquer s'il peut
        if (firstPlayer.canPlay)
        {
            secondPlayer.LifeMon.CurrentHp -= firstPlayer.LifeMon.Lifemon.Attack;
            if (secondPlayer.LifeMon.CurrentHp <= 0)
            {
                secondPlayer.LifeMon.IsDead = true;
            }
        }

        //4- Laisser le deuxième attaquer s'il peut
        if (secondPlayer.canPlay && !secondPlayer.LifeMon.IsDead)
        {
            firstPlayer.LifeMon.CurrentHp -= secondPlayer.LifeMon.Lifemon.Attack;
            if (firstPlayer.LifeMon.CurrentHp <= 0)
            {
                firstPlayer.LifeMon.IsDead = true;
            }
        }

        await Clients.Client(pokemonBattle.Connection1Id).SendAsync("BattleInfo", pokemonBattle);
        await Clients.Client(pokemonBattle.Connection2Id).SendAsync("BattleInfo", pokemonBattle);

    }

}
