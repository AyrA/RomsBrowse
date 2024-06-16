using System.Text.Json;

namespace RomsBrowse.Web.Extensions
{
    public static class JsonExtensions
    {
        private static readonly JsonSerializerOptions opt = new(JsonSerializerDefaults.General)
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static string ToJson<T>(this T obj) => JsonSerializer.Serialize(obj, opt);

        public static T? FromJson<T>(this string s) => JsonSerializer.Deserialize<T>(s, opt);

        public static T FromJsonRequired<T>(this string s) => s.FromJson<T>()
            ?? throw new InvalidOperationException("JSON data is null or not deserializable");
    }
}
