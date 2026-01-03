using System;
using System.IO.Packaging;

using Discord;

namespace CutTheRope.Helpers
{
    public class RPCHelpers : IDisposable
    {
        public Discord.Discord discord;
        public ActivityManager activityManager;
        public void MenuPresence()
        {
            if (activityManager == null)
            {
                return;
            }

            Activity activity = new()
            {
                Details = "Browsing Menus",
                Instance = true
            };

            activityManager.UpdateActivity(activity, result =>
            {
                System.Diagnostics.Debug.WriteLine($"RPC result: {result}");
            });
        }

        public void Setup()
        {
            try
            {
                discord = new(
                    1457063659724603457,
                    (ulong)CreateFlags.NoRequireDiscord
                );
                activityManager = discord.GetActivityManager();
                MenuPresence();
            }
            catch (Exception)
            {
                discord = null;
                activityManager = null;
            }
        }

        public void RunCallbacks()
        {
            try
            {
                discord?.RunCallbacks();
            }
            catch (Exception)
            {
                discord = null;
            }
        }

        public void Dispose()
        {
            discord?.Dispose();
            discord = null;
        }

        public void SetLevelPresence(int pack, int level)
        {
            if (activityManager == null)
            {
                return;
            }

            Activity activity = new()
            {
                Details = $"Level {pack + 1} - {level + 1}",
                Instance = true
            };

            activityManager.UpdateActivity(activity, result =>
            {
                System.Diagnostics.Debug.WriteLine($"RPC result: {result}");
            });
        }


    }
}
