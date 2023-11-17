using GTANetworkAPI;
using System;

class Commands : Script
{
    [Command("wanted_level")]
    public void Cmd_UpdateWantedLevel(Player player, int wantedLevel)
    {
        wantedLevel = Math.Min(5, Math.Max(0, wantedLevel));
        WantedSystem.SetWantedLevel(player, wantedLevel);

        NAPI.Chat.SendChatMessageToPlayer(player, $"You are set wanted level to {wantedLevel}");
    }

    [Command("cop")]
    public void Cmd_CopSwitcher(Player player)
    {
        if (player.HasData("isCop"))
        {
            player.ResetData("isCop");
            NAPI.Chat.SendChatMessageToPlayer(player, $"You are not cop anymore");
            return;
        }

        player.SetData("isCop", true);
        NAPI.Chat.SendChatMessageToPlayer(player, $"Wecome to the cop job");
    }
}