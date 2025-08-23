//Project by BaussHacker aka. L33TS

using ProjectX_V3_Lib.ScriptEngine;
using ProjectX_V3_Lib.Extensions;
using System.Linq;
using System.Reflection;
using MTA.Network.GamePackets;
using MTA;
using System;

namespace ProjectX_V3_Game.Database
{
    /// <summary>
    /// Description of ScriptDatabase.
    /// </summary>
    public class ScriptDatabase
    {
        private static System.Type[] GetTypesInNamespace(Assembly assembly, string nameSpace)
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
            //  cssettings.AddNamespace("MTA.Npcs"); // will add some functions to this later, not entirely npc restricted, but mostly hence why it's also used by ItemScripts!
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

            MTA.Console.WriteLine("Loading NPC Scripts...");

            ScriptSettings x = cssettings.DeepClone();

            //  x.ScriptLocation = @"\\NPCScripts";
            //   x.AddScriptType(typeof(Message));

            World.ScriptEngine = new ScriptEngine(x, 10000); // scripts updates every 10 sec.
            World.ScriptEngine.Check_Updates();

            System.Console.ForegroundColor = ConsoleColor.Green;
            MTA.Console.WriteLine(World.ScriptEngine.scriptCollection.scripts.Count + "  NPC Scripts Loaded...");


        }
    }
}
