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
