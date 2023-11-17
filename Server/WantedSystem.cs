using GTANetworkAPI;
using System.Timers;
using System.Collections.Generic;
using System;
using System.Linq;

public class WantedSystem
{
    private static List<Player> _watchingPlayers = new List<Player>();
    private static object _lock = new object();
    private static Timer? _timer;

    private const string WANTED_LAST_ENTRY = "dataWantedStartTime";
    private const string WANTED_CURRENT_LEVEL = "dataWantedCurrentLevel";
    private const int WANTED_LOWER_TIME = 120;

    public static void SetWantedLevel(Player player, int wantedLevel)
    {
        OnWantedLevelUpdate(player, wantedLevel);

        if (_timer == null)
        {
            _timer = new Timer(1000);
            _timer.Elapsed += OnTimerUpdate;
            _timer.Start();
        }

        lock (_lock)
        {
            if (!_watchingPlayers.Contains(player))
                _watchingPlayers.Add(player);
        }
    }

    public static void OnPlayerDisconnect(Player player)
    {
        OnWantedLevelUpdate(player, 0);
    }

    private static void OnWantedLevelUpdate(Player player, int wantedLevel)
    {
        player.SetData(WANTED_LAST_ENTRY, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        player.SetData(WANTED_CURRENT_LEVEL, wantedLevel);

        RemoveBlipToAllCops(player);

        switch (wantedLevel)
        {
            case 0:
                lock (_lock) 
                    _watchingPlayers.Remove(player);
                break;
            case 3: // 3 звезды - все копы в радиусе 20 метров получают уведомление о преступнике рядом 
                NotifyNearbyCops(player, 20);
                break;
            case 4: // 4 звезды - все копы в радиусе 100 метров получают уведомление о преступнике рядом 
                NotifyNearbyCops(player, 100);
                break;
            case 5: // 5 звезда - блип по коордам игрока на карте всем копам 
                SetBlipToAllCops(player);
                break;
        }

        player.TriggerEvent("SERVER:CLIENT::OnWantedLevelUpdate", wantedLevel);
    }

    private static void OnTimerUpdate(object sender, ElapsedEventArgs e)
    {
        // TODO Remove timer

        NAPI.Task.Run(() =>
        {
            lock (_lock)
            {
                foreach (var it in _watchingPlayers.ToList())
                {
                    if (NAPI.Player.IsPlayerConnected(it))
                    {
                        // check if WANTED_UPDATE_TIME was expired, then we can lower wanted level to player
                        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - it.GetData<long>(WANTED_LAST_ENTRY) >= WANTED_LOWER_TIME)
                        {
                            var currentWantedLevel = it.GetData<int>(WANTED_CURRENT_LEVEL);
                            OnWantedLevelUpdate(it, --currentWantedLevel);
                        }
                    }
                }
            }
        });
    }

    private static void NotifyNearbyCops(Player target, int radius)
    {
        var players = NAPI.Player.GetPlayersInRadiusOfPosition(radius, target.Position);

        foreach (var p in players)
        {
            // TODO parse proper data name from organization
            if (p.GetData<bool>("isCop") && p != target)
            {
                NAPI.Chat.SendChatMessageToPlayer(p, $"Intruder has been detected near you ({radius} meters)");
            }
        }
    }

    private static void SetBlipToAllCops(Player target)
    {
        target.SetData("isHasIntruderBlip", true);

        var players = NAPI.Pools.GetAllPlayers();
        foreach (var p in players)
        {
            // TODO parse proper data name from organization
            // TODO p != target
            if (p.GetData<bool>("isCop"))
            {
                NAPI.Chat.SendChatMessageToPlayer(p, "Last place of Intruder has been detected on your map");
                p.TriggerEvent("SERVER:CLIENT::OnBlipSetupForCop", target.Name,
                    target.Position.X, target.Position.Y, target.Position.Z);
            }
        }
    }
    private static void RemoveBlipToAllCops(Player target)
    {
        if (!target.HasData("isHasIntruderBlip"))
            return;

        target.ResetData("isHasIntruderBlip");

        var players = NAPI.Pools.GetAllPlayers();
        foreach (var p in players)
        {
            // TODO parse proper data name from organization
            // TODO p != target
            if (p.GetData<bool>("isCop"))
            {
                p.TriggerEvent("SERVER:CLIENT::OnBlipRemoveForCop", target.Name);
            }
        }
    }
}