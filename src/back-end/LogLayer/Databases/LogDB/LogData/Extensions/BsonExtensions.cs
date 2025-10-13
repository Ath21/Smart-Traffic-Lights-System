using MongoDB.Bson;

namespace LogData.Extensions;

public static class BsonExtensions
{
    public static Dictionary<string, string> ToDictionary(this BsonDocument bson)
    {
        var dict = new Dictionary<string, string>();

        foreach (var element in bson.Elements)
        {
            // Convert nested BsonDocuments and arrays to JSON strings
            if (element.Value.IsBsonDocument || element.Value.IsBsonArray)
                dict[element.Name] = element.Value.ToJson();
            else
                dict[element.Name] = element.Value?.ToString() ?? string.Empty;
        }

        return dict;
    }
}
