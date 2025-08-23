using System;
using System.Collections.Generic;
using System.Text;

namespace TrialModePlugin
{
    class VersionChecker
    {
        static int currentRun = -100;
        static string RegistryPath = "Software\\Ma†rixSource";
        static string TrailVersionKey = "TVS";
        static string FullVersionKey = "Jµ§†-Abdôµ-Ma†®ix";
        const int TRAIL_RUNS = 100;

        public static bool IsValidVersion()
        {
            RegistryComponent registry = new RegistryComponent(RegistryPath, false);

            string strFullVersion = registry.Read(FullVersionKey);
            string strTrailVersion = registry.Read(TrailVersionKey);

            // If this exist then the user is perhaps using the full version
            if (strFullVersion == null)
            {
                //this could be the first run
                if (strTrailVersion == null)
                {
                    //this is the first run definitly
                    registry.Write(TrailVersionKey, currentRun.ToString());
                }
                else
                {
                    //not the first run
                    currentRun = Convert.ToInt32(strTrailVersion);
                    currentRun += 1;

                    registry.Write(TrailVersionKey, currentRun.ToString());

                    //Let us check whether is has exausted all his trail attempts or not
                    if (currentRun > TRAIL_RUNS)
                    {
                        //The user has exhausted all his trail runs, dont let him in
                        return false;
                    }
                }
            }

            //User is either using the full version or still has some trails left, let him pass through
            return true;
        }
        public static void TrialEnd()
        {
            RegistryComponent registry = new RegistryComponent(RegistryPath, false);
            registry.DeleteKey(TrailVersionKey);
            //   registry.Write(FullVersionKey, currentRun.ToString());            
        }
    }
}
