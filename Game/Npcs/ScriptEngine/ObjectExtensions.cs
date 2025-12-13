using System.Text.Json;

namespace MTA.Game.Npcs.ScriptEngine
{
    /// <summary>
    /// Extensions for System.Object
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// Performas a deep copy for an object.
        /// </summary>
        /// <param name="a">The object to copy.</param>
        /// <returns>Returns the new copy.</returns>
        public static T? DeepClone<T>(this T obj)
        {
            if (obj == null)
                return default;

            var json = JsonSerializer.Serialize(obj);
            return JsonSerializer.Deserialize<T>(json);
        }
    }
}

