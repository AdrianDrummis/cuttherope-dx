using System;
using CutTheRope.Framework.Core;
using DiscordRPC;


namespace CutTheRope.Helpers
{
    public class RPCHelpers : IDisposable
    {
        public DiscordRpcClient Client { get; private set; }
        private DateTime? startTimestamp;

        //disable RPC entirely (recommended for mods that don't want to go through the hassle of setting it up)
        private readonly bool RPCEnabled = true;
        private const string DISCORD_APP_ID = "1457063659724603457";

        public void MenuPresence()
        {
            if (Client == null || !RPCEnabled)
            {
                return;
            }
            Client.SetPresence(new RichPresence()
            {
                Type = ActivityType.Playing,
                Details = Application.GetEnglishString("RPC_MENU"),
                Timestamps = new Timestamps()
                {
                    Start = GetOrCreateStartTime()
                }
            });

        }

        public void Setup()
        {
            if (!RPCEnabled)
            {
                return;
            }
            Client = new DiscordRpcClient(DISCORD_APP_ID);
            Client.Initialize();
        }

        private DateTime GetOrCreateStartTime()
        {
            startTimestamp ??= DateTime.UtcNow;
            return startTimestamp.Value;
        }

        public void Dispose()
        {
            Client?.Dispose();
            Client = null;
            GC.SuppressFinalize(this);
        }

        public void SetLevelPresence(int pack, int level,int stars)
        {
            if (Client == null || !RPCEnabled || (Application.GetEnglishString($"BOX{pack + 1}_LABEL") == null))
            {
                return;
            }

            Client.SetPresence(new RichPresence()
            {
                Details = $"{Application.GetEnglishString($"BOX{pack + 1}_LABEL")}: {Application.GetEnglishString($"LEVEL")} {pack + 1}-{level + 1}",
                State = $"Stars: {stars}/3",
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
