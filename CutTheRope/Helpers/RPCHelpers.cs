using System;

using CutTheRope.Framework.Core;
using CutTheRope.GameMain;

using DiscordRPC;


namespace CutTheRope.Helpers
{
    public class RPCHelpers : IDisposable
    {
        public DiscordRpcClient Client { get; private set; }
        private DateTime? startTimestamp;

        // Check if RPC is enabled in the save file
        // By default, RPC is enabled
        // Exposing in a save file is to make way for later setting UI integration
        private static bool IsRpcEnabled =>
            Preferences.GetBooleanForKey(CTRPreferences.PREFS_RPC_ENABLED);

        //replace with your own Discord Application ID if needed
        private readonly string DISCORD_APP_ID = "1457063659724603457";

        public void MenuPresence()
        {
            if (Client == null || !IsRpcEnabled || !Client.IsInitialized)
            {
                return;
            }
            Client.SetPresence(new RichPresence()
            {
                Details = Application.GetString("RPC_MENU", forceEnglish: true),
                State = $"⭐ Total: {CTRPreferences.GetTotalStars()}",
                Timestamps = new Timestamps()
                {
                    Start = GetOrCreateStartTime()
                }
            });

        }

        public void Setup()
        {
            if (!IsRpcEnabled)
            {
                return;
            }

            Client = new DiscordRpcClient(DISCORD_APP_ID);
            _ = Client.Initialize();

            if (!Client.IsInitialized)
            {
                return;
            }
            Client.SetPresence(new RichPresence()
            {
                Type = ActivityType.Playing,
                Timestamps = new Timestamps()
                {
                    Start = GetOrCreateStartTime()
                }
            });
        }

        private DateTime GetOrCreateStartTime()
        {
            startTimestamp ??= DateTime.UtcNow;
            return startTimestamp.Value;
        }

        public void Dispose()
        {
            Client?.ClearPresence();
            Client?.Dispose();
            Client = null;
            GC.SuppressFinalize(this);
        }

        public void SetLevelPresence(int pack, int level, int stars)
        {
            if (Client == null || !IsRpcEnabled || !Client.IsInitialized || (Application.GetString($"BOX{pack + 1}_LABEL", forceEnglish: true) == null))
            {
                return;
            }

            Client.SetPresence(new RichPresence()
            {
                Details = $"{Application.GetString($"BOX{pack + 1}_LABEL", forceEnglish: true)}: {Application.GetString($"LEVEL", forceEnglish: true)} {pack + 1}-{level + 1}",
                State = $"⭐ {stars}/3",
                Assets = new Assets()
                {
                    SmallImageKey = $"pack_{pack + 1}",
                    //this library has a bug where it doesn't allow you to only set the small icon so it flickers when loading the new large image :(
                    LargeImageKey = "icon"
                },
                Timestamps = new Timestamps()
                {
                    Start = GetOrCreateStartTime()
                }
            });
        }
    }
}
