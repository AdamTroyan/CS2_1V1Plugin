using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using System.Reflection;

namespace _1v1Round;

public class OneVsOneRound : BasePlugin
{
    public override string ModuleName => "1V1 Plugin";
    public override string ModuleVersion => "1.0.0";

    // Fields
    private bool _enabled = true; // Default enabled
    private bool _isDuelActive = false;
    private bool _isDuelFinished = false;
    private float _lastCleanupTime = 0;

    private Dictionary<ulong, PlayerWeaponState> _playerSnapshots = new Dictionary<ulong, PlayerWeaponState>();

    // --- Lifecycle ---
    public override void Load(bool hotReload)
    {
        AddCommand("css_admin1v1", "Toggle 1V1 system", OnToggleAdmin1v1);

        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);

        RegisterListener<Listeners.OnTick>(OnTickUpdate);
    }

    // -- Event/Listeners/Commands Handlers --

    private void OnToggleAdmin1v1(CCSPlayerController? player, CommandInfo info)
    {
        _enabled = !_enabled;
        _isDuelActive = false;

        Server.PrintToChatAll($" {ChatColors.Green}[1V1] {ChatColors.Default}Status: {(_enabled ? "Enabled" : "Disabled")}");
    }

    private HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        if (!_enabled || _isDuelActive) return HookResult.Continue;

        var deadPlayer = @event.Userid;

        var ctPlayers = Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive && p.TeamNum == 3 && p != deadPlayer).ToList();
        var tPlayers = Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive && p.TeamNum == 2 && p != deadPlayer).ToList();

        if (ctPlayers.Count == 1 && tPlayers.Count == 1)
        {
            StartDuel(ctPlayers[0], tPlayers[0]);
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        // Ensure that both field have been reset!
        _isDuelFinished = false;
        _isDuelActive = false;

        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd @event, GameEventInfo info)
    {
        if (!_isDuelActive) return HookResult.Continue;

        _isDuelActive = false;
        _isDuelFinished = true;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive))
        {
            RemoveAllWeapons(p);
        }

        AddTimer(0.1f, () =>
        {
            var survivors = Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive).ToList();

            if (survivors.Count == 1)
            {
                var winner = survivors[0];
                Server.PrintToChatAll($" {ChatColors.Green}[1v1] {ChatColors.Default}Player {ChatColors.Gold}{winner.PlayerName} {ChatColors.Default}won the duel!");
            }
            else if (survivors.Count > 1)
            {
                Server.PrintToChatAll($" {ChatColors.Green}[1v1] {ChatColors.Default}Round ended with both players alive!");
            }

        });

        return HookResult.Continue;
    }

    private HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        _isDuelActive = false;
        bool hasC4 = false;

        var player = @event.Userid;
        var pawn = player?.Pawn.Value;

        if (pawn == null || pawn.WeaponServices == null) return HookResult.Continue;
        if (player == null || !player.IsValid || player.IsBot || player.SteamID == 0) return HookResult.Continue;


        Server.NextFrame(() =>
        {
            if (!player.IsValid) return;

            if (pawn.WeaponServices.MyWeapons.Any(w => w.Value?.DesignerName == "weapon_c4"))
            {
                hasC4 = true;
            }

            if (_playerSnapshots.TryGetValue(player.SteamID, out var state))
            {

                player.RemoveWeapons();
                player.GiveNamedItem("weapon_knife");
                if (hasC4)
                {
                    player.GiveNamedItem("weapon_c4");
                }


                foreach (var w in state.Weapons)
                {
                    player.GiveNamedItem(w);
                }

                _playerSnapshots.Remove(player.SteamID);
                player.PrintToChat($" {ChatColors.Green}[1v1] {ChatColors.Default}Restored: {ChatColors.Gold}{string.Join(", ", state.Weapons)}");
            }
        });

        return HookResult.Continue;
    }

    // Check every tick if the players hold/have a deagle 
    private void OnTickUpdate()
    {
        if (_isDuelFinished)
        {
            foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive))
            {
                if (p.IsBot || !_playerSnapshots.ContainsKey(p.SteamID)) continue;

                var pawn = p.Pawn.Value;
                if (pawn?.WeaponServices == null) continue;

                bool hasDeagle = pawn.WeaponServices.MyWeapons.Any(w => w.Value?.DesignerName == "weapon_deagle");
                if (hasDeagle)
                {
                    RemoveAllWeapons(p);

                }
            }

            _isDuelFinished = false;
            return;
        }

        if (!_isDuelActive) return;

        foreach (var p in Utilities.GetPlayers().Where(p => p.IsValid && p.PawnIsAlive))
        {
            var pawn = p.Pawn.Value;
            if (pawn?.WeaponServices == null) continue;

            var activeWeapon = pawn.WeaponServices.ActiveWeapon.Value;
            if (activeWeapon?.DesignerName == "weapon_deagle") continue;

            bool hasDeagle = pawn.WeaponServices.MyWeapons.Any(w => w.Value?.DesignerName == "weapon_deagle");
            if (!hasDeagle)
            {
                p.PrintToCenter("Please don't drop your deagle.");
                p.GiveNamedItem("weapon_deagle");
            }
        }

        if (Server.CurrentTime >= _lastCleanupTime + 0.1f)
        {
            RemoveWeaponsOnGround();
            _lastCleanupTime = Server.CurrentTime;
        }
    }

    // --- Core Duel Logic ---

    private void StartDuel(CCSPlayerController p1, CCSPlayerController p2)
    {
        _isDuelActive = true;
        _isDuelFinished = false;

        RemoveWeaponsOnGround();

        PreparePlayer(p1);
        PreparePlayer(p2);

        Server.NextFrame(() =>
        {
            Server.PrintToChatAll($" {ChatColors.Green}[1V1] {ChatColors.Default}The duel between {ChatColors.Gold}{p1.PlayerName} {ChatColors.Default}And {ChatColors.Gold}{p2.PlayerName} {ChatColors.Default}has started! Deagle Only.");
        });
    }

    // --- Helper Methods ---

    private void UpdatePlayerSnapshot(CCSPlayerController player)
    {
        if (player == null || !player.IsValid || player.SteamID == 0) return;

        var pawn = player.Pawn.Value;
        if (pawn?.WeaponServices == null) return;

        var newState = new PlayerWeaponState
        {
            Weapons = pawn.WeaponServices.MyWeapons
                .Select(w => w.Value)
                .Where(w => w != null && w.IsValid)
                .Select(w => GetWeaponName(w!))
                .Where(name => !string.IsNullOrEmpty(name)
                    && name != "weapon_knife"
                    && name != "weapon_knife_t"
                    && name != "weapon_c4")
                .ToList()
        };

        string weaponsString = string.Join(", ", newState.Weapons);
        Console.WriteLine(weaponsString + " - saved");

        _playerSnapshots[player.SteamID] = newState;
    }

    private void PreparePlayer(CCSPlayerController player)
    {
        UpdatePlayerSnapshot(player);

        var pawn = player.PlayerPawn.Value;
        if (pawn == null) return;


        RemoveAllWeapons(player);

        player.GiveNamedItem("weapon_deagle");
        pawn.Health = 100;

        Server.NextFrame(() =>
        {
            if (player.IsValid) player.ExecuteClientCommand("slot2");
        });
    }

    private static string GetWeaponName(CBasePlayerWeapon weapon)
    {
        var designer = weapon.DesignerName ?? string.Empty;

        var item = weapon.AttributeManager?.Item;
        if (item == null) return designer;

        var defIndex = item.ItemDefinitionIndex;

        return defIndex switch
        {
            61 => "weapon_usp_silencer",   // USP-S
            63 => "weapon_cz75a",          // CZ75-Auto
            60 => "weapon_m4a1_silencer",  // M4A1-S
            _ => designer
        };
    }

    private void RemoveWeaponsOnGround()
    {
        var entities = Utilities.FindAllEntitiesByDesignerName<CBaseEntity>("weapon_");
        foreach (var entity in entities)
        {
            if (entity != null && entity.IsValid && entity.OwnerEntity.Value == null)
            {
                entity.Remove();
            }
        }
    }

    private void RemoveAllWeapons(CCSPlayerController player)
    {
        var pawn = player.PlayerPawn.Value;
        if (pawn?.WeaponServices == null) return;

        foreach (var weapon in pawn.WeaponServices.MyWeapons)
        {
            if (weapon.Value != null && weapon.Value.IsValid)
            {
                weapon.Value.Remove();
            }
        }
    }
}

//      mp_freezetime 0; mp_warmup_end; bot_add