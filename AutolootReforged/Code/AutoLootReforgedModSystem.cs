using AutoLootReforged.Config;
using HarmonyLib;
using System;
using System.Text;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace AutoLootReforged.Code
{
    public class AutoLootReforgedModSystem : ModSystem
    {
        public const string ConfigName = "AutoLootReforgedConfig.json";
        public static ModConfig Config { get; private set; }

        private Harmony harmony;
        public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

        internal static ICoreClientAPI ClientAPI { get; private set; }

        public override void StartClientSide(ICoreClientAPI api)
        {
            ClientAPI = api;
            try
            {
                Config = api.LoadModConfig<ModConfig>(ConfigName);
                if(Config == null)
                {
                    Config = new ModConfig();
                    api.StoreModConfig(Config, ConfigName);
                }
            }
            catch (Exception e)
            {
                api.Logger.Error($"Failed to load Autoloot, using default config. Exception: {e}");
                Config = new ModConfig();
            }


            if (!Harmony.HasAnyPatches(Mod.Info.ModID))
            {
                harmony = new Harmony(Mod.Info.ModID);
                harmony.PatchAllUncategorized();
            }
        }

        public override void Dispose()
        {
            Config = null;
            ClientAPI = null;
            harmony?.UnpatchAll(Mod.Info.ModID);
        }

        public static void Log(StringBuilder stringBuilder)
        {
            if(stringBuilder == null) return;
            var message = stringBuilder.ToString();

            if (Config.LogToConsole) ClientAPI.Logger.Notification(message);

            if (Config.LogToChat) ClientAPI.ShowChatMessage(message);
        }
    }
}
