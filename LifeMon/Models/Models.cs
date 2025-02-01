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


public class UserInfo
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
