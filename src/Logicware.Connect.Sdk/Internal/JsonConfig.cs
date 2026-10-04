using System.Text.Json;
using System.Text.Json.Serialization;

namespace Logicware.Connect.Sdk.Internal;

internal static class JsonConfig
{
    // Single shared options instance — API uses camelCase on the wire, we
    // tolerate extra fields (server may add new ones), and we serialize
    // enums as strings so roundtripping reads like the JSON you'd get from
    // curl.
    public static readonly JsonSerializerOptions Default = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() },
        PropertyNameCaseInsensitive = true,
    };
}
