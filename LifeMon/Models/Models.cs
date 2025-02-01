using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class User
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("username")]
    public required string Username { get; set; }

    [BsonElement("password")]
    public required string Password { get; set; }
}

public class LifeMon
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("userId")]
    public required ObjectId UserId { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("speed")]
    public required int Speed { get; set; }

    [BsonElement("hp")]
    public required int Hp { get; set; }

    [BsonElement("attack")]
    public required int Attack { get; set; }
}

public class Team
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("userId")]
    public required ObjectId UserId { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("lifemons")]
    public required List<string> LifeMons { get; set; }
}



public class UserInfo
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}


public class LifeMonInfo
{
    public required string UserId { get; set; }
    public required string Name { get; set; }
}


public class TeamInfo
{
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public required string[] LifeMonNames { get; set; }
}


public class LifeMonBattle
{
    public required LifeMon Lifemon { get; set; }
    public required bool IsDead { get; set; }
    public required int? AttackBoost { get; set; }
    public required int? DefenseBoost { get; set; }
    public required bool IsInTheGame { get; set; }

}

public class BattleInfo
{
    public required string Player1Id { get; set; }
    public required string Player2Id { get; set; }
    public required LifeMonBattle[] Player1LifeMons { get; set; }
    public required LifeMonBattle[] Player2LifeMons { get; set; }
    public required Turn[] Turns { get; set; }
}

public class Turn
{
    public TurnInfo? Player1Turn { get; set; }
    public TurnInfo? Player2Turn { get; set; }
}



public class TurnInfo
{
    public required string PlayerId { get; set; }
    public required TurnType TurnType { get; set; }
    public required string PokemonId { get; set; }
    public required string? AttackId { get; set; }
    public required string? NewPokemonId { get; set; }
}

public enum TurnType
{
    Attack = 0,
    Change = 1,
    Surrender = 2
}
