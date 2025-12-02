using System;
using System.Linq;
using System.Reflection;
using MTA.Game.Npcs.ScriptEngine.Extensions;

namespace MTA.Game.Npcs.ScriptEngine
{
    /// <summary>
    /// Description of ScriptDatabase.
    /// </summary>
    public class ScriptDatabase
    {
        private static Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
        {
            return assembly.GetTypes().Where(t => String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)).ToArray();
        }

        public static ScriptSettings cssettings;

        /// <summary>
        /// Loads the global script settings.
        /// </summary>
        public static void LoadSettings()
        {
            MTA.Console.WriteLine("Loading Script Settings...");

            cssettings = new ScriptSettings();
            cssettings.AddNamespace("System");
            cssettings.AddNamespace("System.Collections.Generic");
            cssettings.AddNamespace("System.Linq");
            cssettings.AddNamespace("System.Text");
            cssettings.AddNamespace("MTA.Client");
            cssettings.AddNamespace("MTA.Network");
            cssettings.AddNamespace("MTA.Interfaces");
            cssettings.AddNamespace("MTA.Network.GamePackets");
            cssettings.AddNamespace("MTA.Game.ConquerStructures.Society");
            cssettings.AddNamespace("MTA.Game.ConquerStructures");
            cssettings.AddNamespace("MTA.Database");
            cssettings.AddNamespace("System.Text.RegularExpressions");
            cssettings.AddNamespace("System.Drawing");
            cssettings.AddNamespace("MTA.Game");
            ScriptEngine.SetNamespaces(cssettings);

            cssettings.Framework = "v4.0";
            cssettings.Language = ScriptLanguage.CSharp;

            MTA.Console.WriteLine(cssettings._namespaces.Count + " Script Settings Loaded...");
        }

        /// <summary>
        /// Loads all the npc scripts.
        /// </summary>
        public static void LoadNPCScripts()
        {
            Console.WriteLine("Loading NPC Scripts...");

            ScriptSettings x = cssettings.DeepClone();

            World.ScriptEngine = new ScriptEngine(x, 10000); // scripts updates every 10 sec.
            World.ScriptEngine.Check_Updates();

            Console.WriteLine(World.ScriptEngine.scriptCollection.scripts.Count + "  NPC Scripts Loaded...");
        }
    }
}

