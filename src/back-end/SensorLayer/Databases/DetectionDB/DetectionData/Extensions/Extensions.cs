using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using System.Text.Json;

namespace DetectionData.Extensions;

public static class BsonExtensions
{
    public static Dictionary<string, string> ToDictionary(this BsonDocument bson)
    {
        var dict = new Dictionary<string, string>();

        foreach (var element in bson.Elements)
        {
            if (element.Value.IsBsonDocument || element.Value.IsBsonArray)
                dict[element.Name] = element.Value.ToJson();
            else
                dict[element.Name] = element.Value?.ToString() ?? string.Empty;
        }

        return dict;
    }

    public static BsonDocument ToBsonDocument(this Dictionary<string, string> dict)
    {
        var bson = new BsonDocument();

        foreach (var kvp in dict)
        {
            var key = kvp.Key;
            var value = kvp.Value;

            if (string.IsNullOrEmpty(value))
            {
                bson[key] = BsonNull.Value;
                continue;
            }

            try
            {
                using var jsonDoc = JsonDocument.Parse(value);
                var root = jsonDoc.RootElement;

                if (root.ValueKind == JsonValueKind.Object)
                {
                    bson[key] = BsonDocument.Parse(value);
                }
                else if (root.ValueKind == JsonValueKind.Array)
                {
                    // Correct way: use BsonSerializer
                    bson[key] = BsonSerializer.Deserialize<BsonArray>(value);
                }
                else
                {
                    bson[key] = BsonValue.Create(value);
                }
            }
            catch
            {
                bson[key] = BsonValue.Create(value);
            }
        }

        return bson;
    }
}
