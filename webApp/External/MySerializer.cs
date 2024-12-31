using Newtonsoft.Json;

public class SingleValueArrayConverter<T> : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.StartArray)
        {
            return serializer.Deserialize<List<double>>(reader);
        }
        else
        {
            double media = serializer.Deserialize<double>(reader);
            return new List<double>() { media };
        }
    }
    public override bool CanConvert(Type objectType)
    {
        return true;
    }
}