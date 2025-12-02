using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace MTA.Game.Npcs.ScriptEngine.Extensions
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
        public static T DeepClone<T>(this T a)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}

