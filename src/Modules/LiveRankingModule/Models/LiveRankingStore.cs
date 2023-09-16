﻿using System.Collections.Concurrent;
using EvoSC.Common.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace EvoSC.Modules.Official.LiveRankingModule.Models;

internal class LiveRankingStore
{
    private ConcurrentDictionary<string, LiveRankingPosition> _curLiveRanking = new();
    private ConcurrentDictionary<string, LiveRankingPosition> _prevLiveRanking = new();
    private readonly ILogger<LiveRankingStore> _logger;
    private readonly IPlayerManagerService _playerManager;
    private MatchInfo _matchInfo = new();

    internal LiveRankingStore(ILogger<LiveRankingStore> logger, IPlayerManagerService playerManager)
    {
        _logger = logger;
        _playerManager = playerManager;
        _logger.LogDebug("Instantiated LiveRankingStore");
    }

    internal async Task ResetLiveRankingsAsync()
    {
        _curLiveRanking.Clear();
        // var onlinePlayers = await _playerManager.GetOnlinePlayersAsync();
        // foreach (var player in onlinePlayers)
        // {
        //     if (player.State == PlayerState.Playing)
        //     {
        //         _curLiveRanking.Add(new LiveRankingPosition(player.AccountId, 0, -1, false, false));
        //     }
        // }
    }

    internal void RegisterTime(string accountId, int cpIndex, int cpTime, bool isFinish)
    {
        _prevLiveRanking = new ConcurrentDictionary<string, LiveRankingPosition>(_curLiveRanking);
        var liveRankingPosition = new LiveRankingPosition(accountId, cpTime, cpIndex, false, isFinish);

        _curLiveRanking.AddOrUpdate(accountId, _ => liveRankingPosition, (_, _) => liveRankingPosition);
    }

    internal void RegisterPlayerGiveUp(string accountId)
    {
        _prevLiveRanking = new ConcurrentDictionary<string, LiveRankingPosition>(_curLiveRanking);

        _curLiveRanking.AddOrUpdate(accountId, _ => new LiveRankingPosition(accountId, 0, 0, true, false),
            (_, arg) => new LiveRankingPosition(accountId, arg.cpTime, arg.cpIndex, true, false));
    }

    internal ConcurrentDictionary<string, LiveRankingPosition> GetCurrentLiveRanking()
    {
        return _curLiveRanking;
    }

    internal ConcurrentDictionary<string, LiveRankingPosition> GetPreviousLiveRanking()
    {
        return _prevLiveRanking;
    }

    /// <summary>
    /// This sorts the live ranking based on the following criteria:
    /// - DNFd players should always be at the bottom
    /// - Players with a higher cpIndex should always be at the top
    /// - Players with the faster CP time at the same CP index should be in a higher position
    /// </summary>
    internal static List<ExpandedLiveRankingPosition> SortLiveRanking(IEnumerable<ExpandedLiveRankingPosition> positions)
    {
        return positions
            .OrderBy(a => a.isDNF)
            .ThenByDescending(a => a.cpIndex)
            .ThenBy(a => a.cpTime).ToList();
    }

    internal async Task<List<ExpandedLiveRankingPosition>> GetFullLiveRankingAsync()
    {
        List<ExpandedLiveRankingPosition> expandedLiveRanking = new();
        foreach (var rank in _curLiveRanking)
        {
            var player = await _playerManager.GetOnlinePlayerAsync(rank.Value.accountId);
            expandedLiveRanking.Add(new ExpandedLiveRankingPosition
            {
                player = player,
                cpTime = rank.Value.cpTime,
                cpIndex = rank.Value.cpIndex,
                isDNF = rank.Value.isDNF,
                isFinish = rank.Value.isFinish
            });
        }

        var sortedExpandedLiveRanking = SortLiveRanking(expandedLiveRanking);

        return sortedExpandedLiveRanking;
    }

    internal async Task<List<ExpandedLiveRankingPosition>> GetFullPreviousLiveRankingAsync()
    {
        List<ExpandedLiveRankingPosition> expandedLiveRanking = new();
        foreach (var rank in _prevLiveRanking)
        {
            var player = await _playerManager.GetOnlinePlayerAsync(rank.Value.accountId);
            expandedLiveRanking.Add(new ExpandedLiveRankingPosition
            {
                player = player,
                cpTime = rank.Value.cpTime,
                cpIndex = rank.Value.cpIndex,
                isDNF = rank.Value.isDNF,
                isFinish = rank.Value.isFinish
            });
        }

        return expandedLiveRanking;
    }

    internal void SetCurrentMap(string name)
    {
        _matchInfo.MapName = name;
    }

    internal void SetWorldRecord(string wrHolder, string wrTime)
    {
        _matchInfo.WrHolderName = wrHolder;
        _matchInfo.WrTime = wrTime;
    }

    internal void IncreaseRoundCounter()
    {
        _matchInfo.NumRound++;
    }

    internal void ResetRoundCounter()
    {
        _matchInfo.NumRound = 0;
    }

    internal void IncreaseTrackCounter()
    {
        _matchInfo.NumTrack++;
    }

    internal void ResetTrackCounter()
    {
        _matchInfo.NumTrack = 0;
    }

    internal MatchInfo GetMatchInfo()
    {
        return _matchInfo;
    }
}