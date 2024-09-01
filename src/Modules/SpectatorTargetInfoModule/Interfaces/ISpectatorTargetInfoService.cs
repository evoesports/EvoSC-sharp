﻿using EvoSC.Common.Remote.EventArgsModels;
using EvoSC.Modules.Official.SpectatorTargetInfoModule.Models;

namespace EvoSC.Modules.Official.SpectatorTargetInfoModule.Interfaces;

public interface ISpectatorTargetInfoService
{
    public Task AddCheckpointAsync(string playerLogin, int checkpointIndex, int checkpointTime);

    public Task ClearCheckpointsAsync();

    public Task<string?> GetLoginOfDedicatedPlayerAsync(int targetPlayerIdDedicated);
    
    public Task UpdateSpectatorTargetAsync(string spectatorLogin, int targetPlayerIdDedicated);
   
    public Task UpdateSpectatorTargetAsync(string spectatorLogin, string targetLogin);
    
    public Task RemovePlayerFromSpectatorsListAsync(string spectatorLogin);

    public IEnumerable<string> GetLoginsSpectatingTarget(string targetPlayerLogin);
    
    public Task UpdateWidgetAsync(List<string> playerLogins, CheckpointData leadingCheckpointData, CheckpointData targetCheckpointData, int targetPlayerRank);
    
    public SpectatorInfo ParseSpectatorStatus(int spectatorStatus);
    
    public int GetRankFromCheckpointList(List<CheckpointData> sortedCheckpointTimes, string targetPlayerLogin);

    public int GetTimeDifference(CheckpointData leadingCheckpointData, CheckpointData targetCheckpointData);
   
    public int GetTimeDifference(int leadingCheckpointTime, int targetCheckpointTime);

    public Task<Dictionary<int, List<CheckpointData>>> GetCheckpointTimesAsync();
    
    /// <summary>
    /// Sends the manialink to all players.
    /// </summary>
    public Task SendManiaLinkAsync();
    
    /// <summary>
    /// Sends the manialink to a specific player.
    /// </summary>
    public Task SendManiaLinkAsync(string playerLogin);
    
    /// <summary>
    /// Hides the manialink for all players.
    /// </summary>
    public Task HideManiaLinkAsync();
    
    /// <summary>
    /// Hides the default spectator info.
    /// </summary>
    public Task HideNadeoSpectatorInfoAsync();
    
    /// <summary>
    /// Shows the default spectator info.
    /// </summary>
    public Task ShowNadeoSpectatorInfoAsync();
    
    /// <summary>
    /// Maps wayPointEventArgs and sends data to clients.
    /// </summary>
    public Task ForwardCheckpointTimeToClientsAsync(WayPointEventArgs wayPointEventArgs);
    
    /// <summary>
    /// Clears the checkpoint times for the clients.
    /// </summary>
    public Task ResetCheckpointTimesAsync();
    
    /// <summary>
    /// Sends players DNF to clients.
    /// </summary>
    public Task ForwardDnfToClientsAsync(PlayerUpdateEventArgs playerUpdateEventArgs);

    public Task AddFakePlayerAsync();
}
