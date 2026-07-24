using Geranium.Reflection;
using Newtonsoft.Json;
using System.Globalization;
using System.IO;
using System.Text;

namespace ioi
{
    internal static class SerializeExtensions
    {
        public static string Serialize<T>(this JsonSerializer serializer, T value)
        {
            StringBuilder sb = new(256);
            StringWriter sw = new(sb, CultureInfo.InvariantCulture);
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = serializer.Formatting;

                serializer.Serialize(jsonWriter, value, typeof(T));
            }

            var str = sw.ToString();

            return str;
        }

        public static T Deserialize<T>(this JsonSerializer serializer, string value)
        {
            using JsonTextReader reader = new JsonTextReader(new StringReader(value));
            return serializer.Deserialize(reader, typeof(T)).As<T>();
        }
    }
}
