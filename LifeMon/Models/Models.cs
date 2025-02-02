using System.Text.Json.Serialization;
using DotnetGeminiSDK.Model;
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

public class ImageRequest
{
    public required string Base64Image { get; set; }
    public required ImageMimeType MimeType { get; set; }

    public required string UserId
    {
        get; set;
    }
}

public class LifeMon
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("userId")]
    public required ObjectId UserId { get; set; }

    [BsonElement("image")]
    public required string Image { get; set; }

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("speed")]
    public required int Speed { get; set; }

    [BsonElement("hp")]
    public required int Hp { get; set; }

    [BsonElement("attack")]
    public required int Attack { get; set; }

    [BsonElement("species")]
    public required string Species { get; set; }

    [BsonElement("defense")]
    public required int Defense { get; set; }

    [BsonElement("specialAttack")]
    public required int SpecialAttack { get; set; }

    [BsonElement("specialDefense")]
    public required int SpecialDefense { get; set; }

    [BsonElement("description")]
    public required string Description { get; set; }

    [BsonElement("type")]
    public required int[] Type { get; set; }

    [BsonElement("move")]

    public required List<Move> Move { get; set; }
}

public class Move
{
    public required string Name { get; set; }

    public required int Type { get; set; }

    public int Power { get; set; }

    public int Accuracy { get; set; }

    public required string Category { get; set; }

    public required string Description { get; set; }
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
    public required List<LifeMon> LifeMons { get; set; }
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

    public required string Image { get; set; }

    public required int Speed { get; set; }

    public required int Hp { get; set; }

    public required int Attack { get; set; }

    public required string Species { get; set; }

    public required int Defense { get; set; }

    public required int SpecialAttack { get; set; }

    public required int SpecialDefense { get; set; }

    public required string Description { get; set; }

    public required int[] Type { get; set; }

    public required List<Move> Move { get; set; }

}


public class TeamInfo
{
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public required List<string> LifeMonNames { get; set; }
}


public class LifeMonBattle
{
    public required LifeMon Lifemon { get; set; }
    public required bool IsDead { get; set; }
    public required int CurrentHp { get; set; }
    public required int? AttackBoost { get; set; }
    public required int? DefenseBoost { get; set; }
    public required bool IsInTheGame { get; set; }

}

public class BattleInfo
{
    public required string Player1Id { get; set; }
    public required string Player2Id { get; set; }

    public required string Connection1Id { get; set; }
    public required string Connection2Id { get; set; }
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
    public required ObjectId PokemonId { get; set; }
    public required string? AttackId { get; set; }
    public required string? NewPokemonId { get; set; }
}

public enum TurnType
{
    Attack = 0,
    Change = 1,
    Surrender = 2
}


public class Waiting
{
    public required string ConnectionId { get; set; }
    public required string UserId { get; set; }
}




public class Root
{
    [JsonPropertyName("candidates")]
    public List<Candidate> Candidates { get; set; }

    [JsonPropertyName("usageMetadata")]
    public UsageMetadata UsageMetadata { get; set; }

    [JsonPropertyName("modelVersion")]
    public string ModelVersion { get; set; }
}

public class Candidate
{
    [JsonPropertyName("content")]
    public Content Content { get; set; }

    [JsonPropertyName("finishReason")]
    public string FinishReason { get; set; }

    [JsonPropertyName("avgLogprobs")]
    public double AvgLogprobs { get; set; }
}

public class Content
{
    [JsonPropertyName("parts")]
    public List<Part> Parts { get; set; }

    [JsonPropertyName("role")]
    public string Role { get; set; }
}

public class Part
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class UsageMetadata
{
    [JsonPropertyName("promptTokenCount")]
    public int PromptTokenCount { get; set; }

    [JsonPropertyName("candidatesTokenCount")]
    public int CandidatesTokenCount { get; set; }

    [JsonPropertyName("totalTokenCount")]
    public int TotalTokenCount { get; set; }

    [JsonPropertyName("promptTokensDetails")]
    public List<TokenDetail> PromptTokensDetails { get; set; }

    [JsonPropertyName("candidatesTokensDetails")]
    public List<TokenDetail> CandidatesTokensDetails { get; set; }
}

public class TokenDetail
{
    [JsonPropertyName("modality")]
    public string Modality { get; set; }

    [JsonPropertyName("tokenCount")]
    public int TokenCount { get; set; }
}


public class LifeMonJson
{

    [BsonElement("name")]
    public required string Name { get; set; }

    [BsonElement("speed")]
    public required int Speed { get; set; }

    [BsonElement("hp")]
    public required int Hp { get; set; }

    [BsonElement("attack")]
    public required int Attack { get; set; }

    [BsonElement("species")]
    public required string Species { get; set; }

    [BsonElement("defense")]
    public required int Defense { get; set; }

    [BsonElement("specialAttack")]
    public required int SpecialAttack { get; set; }

    [BsonElement("specialDefense")]
    public required int SpecialDefense { get; set; }

    [BsonElement("description")]
    public required string Description { get; set; }

    [BsonElement("type")]
    public required int[] Type { get; set; }

    [BsonElement("move")]
    public required List<Move> Move { get; set; }
}
