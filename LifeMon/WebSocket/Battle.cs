using Microsoft.AspNetCore.SignalR;
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

            var player1LifeMons = await lifeMonCollection
                .Find(lm => player1Team.LifeMons.Contains(lm.Id.ToString()))
                .ToListAsync();


            var player2LifeMons = await lifeMonCollection
                .Find(lm => player1Team.LifeMons.Contains(lm.Id.ToString()))
                .ToListAsync();

            BattleInfo battleInfo = new()
            {
                Player1Id = player1.UserId,
                Player2Id = player2.UserId,
                Turns = [],
                Player1LifeMons = [.. player1LifeMons.Select((t, index) => new LifeMonBattle
                {
                    AttackBoost = 0,
                    DefenseBoost = 0,
                    IsDead = false,
                    Lifemon = t,
                    IsInTheGame = index == 0,
                })],
                Player2LifeMons = [.. player2LifeMons.Select((t, index) => new LifeMonBattle
                {
                    AttackBoost = 0,
                    DefenseBoost = 0,
                    IsDead = false,
                    Lifemon = t,
                    IsInTheGame = index == 0,
                })],
            };


            await Clients.Client(player1.ConnectionId).SendAsync("MatchFound", player2);
            await Clients.Client(player2.ConnectionId).SendAsync("MatchFound", player1);
        }
        else
        {
            // Notify the player they are waiting
            await Clients.Client(Context.ConnectionId).SendAsync("WaitingForMatch");
        }
    }



    // Method for players to log in and wait
    public async Task TakeTurn(string playerId, TurnType turnType, string pokemonId, string? newPokemonId, string? attackId)
    {

        //If he surrenders, tell everyone
        if (turnType == TurnType.Surrender)
        {
            await Clients.All.SendAsync("Surrender", playerId);
        }



        //1- take the record concerned
        var pokemonBattle = lifeMonBattles.FirstOrDefault((pok) => pok.Player1Id == playerId || pok.Player2Id == playerId);


        if (pokemonBattle is null)
        {
            return;
        }


        //2- vérifier si c'Est un new turn (les 2 users ont jouée ce tour)
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
            await Clients.All.SendAsync("WaitingForPlayer", pokemonBattle.Player1Id, pokemonBattle.Player2Id);
        }
    }



    // You can also implement a Disconnect method to remove players from the waiting list on disconnect
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;




        return base.OnDisconnectedAsync(exception);
    }






    public async Task MakeTurn(BattleInfo pokemonBattle, TurnInfo turnInfoPlayer1, TurnInfo turnInfoPlayer2)
    {
        //1- Vérifier si un user change de pokemon

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
            secondPlayer.LifeMon.Lifemon.Hp -= firstPlayer.LifeMon.Lifemon.Attack;
            if (secondPlayer.LifeMon.Lifemon.Hp <= 0)
            {
                secondPlayer.LifeMon.IsDead = true;
            }
        }

        //4- Laisser le deuxième attaquer s'il peut
        if (secondPlayer.canPlay && !secondPlayer.LifeMon.IsDead)
        {
            firstPlayer.LifeMon.Lifemon.Hp -= secondPlayer.LifeMon.Lifemon.Attack;
            if (firstPlayer.LifeMon.Lifemon.Hp <= 0)
            {
                firstPlayer.LifeMon.IsDead = true;
            }
        }

        await Clients.All.SendAsync("Turn", pokemonBattle.Player1Id, pokemonBattle.Player2Id, pokemonBattle);



    }

}
