using Microsoft.AspNetCore.SignalR;

public class BattleHub : Hub
{
    private static readonly List<string> waitingPlayers = [];
    private static readonly List<LifeMonBattle> pokemonBattles = [];

    // Method for players to log in and wait
    public async Task Login(string playerId)
    {
        waitingPlayers.Add(playerId);
        Console.WriteLine(waitingPlayers.Count);

        // Try to find a match
        if (waitingPlayers.Count >= 2)
        {
            var player1 = waitingPlayers[0];
            var player2 = waitingPlayers[1];
            waitingPlayers.RemoveAt(0);
            waitingPlayers.RemoveAt(0);

            // Notify the players they have been matched


            //get pokemon info and team





            await Clients.All.SendAsync("MatchFound", player2);
            await Clients.All.SendAsync("MatchFound", player1);
        }
        else
        {
            // Notify the player they are waiting
            await Clients.All.SendAsync("WaitingForMatch");
        }
    }




    // Method for players to log in and wait
    public async Task TakeTurn(string playerId)
    {
        waitingPlayers.Add(playerId);
        Console.WriteLine(waitingPlayers.Count);

        // Try to find a match
        if (waitingPlayers.Count >= 2)
        {
            var player1 = waitingPlayers[0];
            var player2 = waitingPlayers[1];
            waitingPlayers.RemoveAt(0);
            waitingPlayers.RemoveAt(0);

            // Notify the players they have been matched


            //get pokemon info and team



            await Clients.All.SendAsync("MatchFound", player2);
            await Clients.All.SendAsync("MatchFound", player1);
        }
        else
        {
            // Notify the player they are waiting
            await Clients.All.SendAsync("WaitingForMatch");
        }
    }




}
