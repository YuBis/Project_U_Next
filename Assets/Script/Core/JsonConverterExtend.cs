using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

public class SafeStringEnumConverter : StringEnumConverter
{
    private readonly object defaultValue;
    private readonly bool hasDefaultValue;

    public SafeStringEnumConverter()
    {
        this.hasDefaultValue = false;
    }

    public SafeStringEnumConverter(object defaultValue)
    {
        this.defaultValue = defaultValue;
        this.hasDefaultValue = true;
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null || reader.TokenType == JsonToken.Undefined)
        {
            return this.defaultValue;
        }

        try
        {
            return base.ReadJson(reader, objectType, existingValue, serializer);
        }
        catch (JsonSerializationException)
        {
            return this.hasDefaultValue ? this.defaultValue : Enum.GetValues(objectType).GetValue(0);
        }
    }
}

public class JsonConverterExtend
{

}