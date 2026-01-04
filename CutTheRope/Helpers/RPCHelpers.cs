using System;

using CutTheRope.Framework;
using CutTheRope.Framework.Core;
using CutTheRope.GameMain;
using CutTheRope.Framework.Visual;

using Discord;


namespace CutTheRope.Helpers
{
    public class RPCHelpers : IDisposable
    {
        public Discord.Discord discord;
        public ActivityManager activityManager;
        private long? startTimestamp;

        //disable RPC entirely (recommended for mods that don't want to go through the hassle of setting it up)
        private readonly bool RPCEnabled = true;
        public void MenuPresence()
        {
            if (activityManager == null || !RPCEnabled)
            {
                return;
            }

            Activity activity = new()
            {
                Details = Application.GetEnglishString("RPC_MENU"),
                Instance = true,
                Timestamps = new ActivityTimestamps
                {
                    Start = GetOrCreateStartTime()
                }
            };

            activityManager.UpdateActivity(activity, result =>
            {
                System.Diagnostics.Debug.WriteLine($"RPC result: {result}");
            });
        }

        public void Setup()
        {
            if (!RPCEnabled)
            {
                return;
            }
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
            if (!RPCEnabled)
            {
                return;
            }
            try
            {
                discord?.RunCallbacks();
            }
            catch (Exception)
            {
                discord = null;
            }
        }

        private long GetOrCreateStartTime()
        {
            startTimestamp ??= DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return startTimestamp.Value;
        }

        public void Dispose()
        {
            discord?.Dispose();
            discord = null;
            GC.SuppressFinalize(this);
        }

        public void SetLevelPresence(int pack, int level)
        {
            if (activityManager == null || !RPCEnabled || (Application.GetEnglishString($"BOX{pack + 1}_LABEL") == null))
            {
                return;
            }
            Activity activity = new()
            {
                Details = $"{Application.GetEnglishString($"BOX{pack + 1}_LABEL")}: {Application.GetEnglishString($"LEVEL")} {pack + 1}-{level + 1}",
                Instance = true,

                Assets = new ActivityAssets
                {
                    SmallImage = $"pack_{pack + 1}"
                },
                Timestamps = new ActivityTimestamps
                {
                    Start = GetOrCreateStartTime()
                }
            };

            activityManager.UpdateActivity(activity, result =>
            {
                System.Diagnostics.Debug.WriteLine($"RPC result: {result}");
            });
        }


    }
}
