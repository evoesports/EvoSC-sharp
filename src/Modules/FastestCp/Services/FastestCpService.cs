﻿using EvoSC.Common.Interfaces.Services;
using EvoSC.Common.Remote.EventArgsModels;
using EvoSC.Common.Services.Attributes;
using EvoSC.Common.Services.Models;
using EvoSC.Manialinks.Interfaces;
using EvoSC.Modules.Official.FastestCp.Interfaces;
using EvoSC.Modules.Official.FastestCp.Models;
using Microsoft.Extensions.Logging;

namespace EvoSC.Modules.Official.FastestCp.Services;

[Service(LifeStyle = ServiceLifeStyle.Singleton)]
public class FastestCpService : IFastestCpService
{
    private readonly ILogger<FastestCpService> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IManialinkManager _manialinkManager;
    private readonly IPlayerManagerService _playerManagerService;

    private FastestCpStore _fastestCpStore;

    public FastestCpService(IPlayerManagerService playerManagerService, IManialinkManager manialinkManager,
        ILoggerFactory loggerFactory, ILogger<FastestCpService> logger)
    {
        _playerManagerService = playerManagerService;
        _logger = logger;
        _loggerFactory = loggerFactory;
        _manialinkManager = manialinkManager;
        _fastestCpStore = GetNewFastestCpStore();
    }

    public async Task RegisterCpTime(WayPointEventArgs args)
    {
        var result = _fastestCpStore.RegisterTime(args.AccountId, args.CheckpointInRace, args.RaceTime);
        if (result)
        {
            await ShowWidget();
        }
    }

    public async Task ShowWidget()
    {
        await _manialinkManager.SendPersistentManialinkAsync("FastestCp.FastestCp",
            new { times = await GetCurrentBestCpTimes() });
        _logger.LogDebug("Update fastest cp manialink for all users");
    }

    public async Task ResetCpTimes()
    {
        _fastestCpStore = GetNewFastestCpStore();
        await _manialinkManager.HideManialinkAsync("FastestCp.FastestCp");
        _logger.LogDebug("Hide fastest cp manialink for all users");
    }

    private async Task<PlayerCpTime?[]> GetCurrentBestCpTimes()
    {
        var fastestTimes = _fastestCpStore.GetFastestTimes();

        var playerNameCache = new Dictionary<string, string>();
        await Task.WhenAll(
            fastestTimes.Where(s => s != null)
                .Select(time => time!.AccountId)
                .Distinct()
                .Select(async id =>
                {
                    var player = await _playerManagerService.GetOrCreatePlayerAsync(id);
                    playerNameCache[id] = player.StrippedNickName;
                }));

        return fastestTimes.Select(
                time => time == null
                    ? null
                    : new PlayerCpTime(playerNameCache[time.AccountId], TimeSpan.FromMilliseconds(time.RaceTime)))
            .ToArray();
    }

    private FastestCpStore GetNewFastestCpStore()
    {
        return new FastestCpStore(_loggerFactory.CreateLogger<FastestCpStore>());
    }
}