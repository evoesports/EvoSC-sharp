﻿using System.Collections.Concurrent;
using EvoSC.Common.Interfaces;
using EvoSC.Common.Interfaces.Models;
using EvoSC.Common.Interfaces.Services;
using EvoSC.Common.Interfaces.Themes;
using EvoSC.Common.Services.Attributes;
using EvoSC.Common.Services.Models;
using EvoSC.Common.Util;
using EvoSC.Common.Util.MatchSettings;
using EvoSC.Manialinks.Interfaces;
using EvoSC.Modules.Official.RoundRankingModule.Config;
using EvoSC.Modules.Official.RoundRankingModule.Interfaces;
using EvoSC.Modules.Official.RoundRankingModule.Models;
using EvoSC.Modules.Official.RoundRankingModule.Utils;

namespace EvoSC.Modules.Official.RoundRankingModule.Services;

[Service(LifeStyle = ServiceLifeStyle.Singleton)]
public class RoundRankingService(
    IRoundRankingSettings settings,
    IManialinkManager manialinkManager,
    IPlayerManagerService playerManagerService,
    IMatchSettingsService matchSettingsService,
    IThemeManager theme,
    IServerClient server
) : IRoundRankingService
{
    private const string WidgetTemplate = "RoundRankingModule.RoundRanking";

    private readonly object _pointsRepartitionMutex = new();
    private readonly PointsRepartition _pointsRepartition = [];
    private readonly CheckpointsRepository _checkpointsRepository = new();
    private readonly ConcurrentDictionary<PlayerTeam, string> _teamColors = new();
    private volatile bool _isTimeAttackMode;
    private volatile bool _isTeamsMode;

    public async Task ConsumeCheckpointAsync(string accountId, int checkpointId, int time, bool isFinish,
        bool isDnf)
    {
        _checkpointsRepository[accountId] = new CheckpointData
        {
            Player = await playerManagerService.GetOnlinePlayerAsync(accountId),
            CheckpointId = checkpointId,
            Time = RaceTime.FromMilliseconds(time),
            IsFinish = isFinish,
            IsDNF = isDnf
        };

        await SendRoundRankingWidgetAsync();
    }

    public async Task ConsumeDnfAsync(string accountId)
    {
        if (!_isTimeAttackMode)
        {
            await ConsumeCheckpointAsync(accountId, -1, 0, false, true);
            return;
        }

        _checkpointsRepository.Remove(accountId, out var removedCheckpointData);
        await SendRoundRankingWidgetAsync();
    }

    public async Task RemovePlayerCheckpointDataAsync(string accountId)
    {
        if (!_isTimeAttackMode)
        {
            //In time attack mode the entries are cleared by new round event.
            //Prevents flood of manialinks.
            return;
        }

        _checkpointsRepository.Remove(accountId, out var removedCheckpointData);
        await SendRoundRankingWidgetAsync();
    }

    public List<CheckpointData> GetSortedCheckpoints()
    {
        return _checkpointsRepository.GetSortedData();
    }

    public async Task ClearCheckpointDataAsync()
    {
        _checkpointsRepository.Clear();
        await SendRoundRankingWidgetAsync();
    }

    public async Task SendRoundRankingWidgetAsync()
    {
        var bestCheckpoints = GetSortedCheckpoints();

        if (bestCheckpoints.Count > 0)
        {
            bestCheckpoints = bestCheckpoints.Take(settings.MaxRows).ToList();

            if (settings.DisplayGainedPoints && !_isTimeAttackMode)
            {
                lock (_pointsRepartitionMutex)
                {
                    RoundRankingUtils.SetGainedPointsOnResult(
                        bestCheckpoints,
                        _pointsRepartition,
                        (string)theme.Theme.UI_AccentPrimary
                    );
                }
            }

            if (_isTeamsMode)
            {
                RoundRankingUtils.ApplyTeamColorsAsAccentColors(bestCheckpoints, _teamColors);
            }

            if (settings.DisplayTimeDifference)
            {
                RoundRankingUtils.CalculateAndSetTimeDifferenceOnResult(bestCheckpoints);
            }
        }

        var showWinnerTeam = _isTeamsMode && RoundRankingUtils.HasPlayerInFinish(bestCheckpoints);
        var winnerTeamName = "DRAW";
        var winnerTeamColor = theme.Theme?.UI_AccentSecondary ?? "000000";
        var winnerTeam = PlayerTeam.Unknown;

        if (showWinnerTeam)
        {
            winnerTeam = RoundRankingUtils.GetWinnerTeam(bestCheckpoints);

            if (winnerTeam != PlayerTeam.Unknown)
            {
                winnerTeamColor = _teamColors[winnerTeam];
                winnerTeamName = (await server.Remote.GetTeamInfoAsync((int)winnerTeam + 1)).Name;
            }
        }

        await manialinkManager.SendPersistentManialinkAsync(WidgetTemplate,
            new
            {
                settings,
                showWinnerTeam,
                winnerTeam,
                winnerTeamName,
                winnerTeamColor,
                bestCheckpoints = bestCheckpoints.Take(settings.MaxRows),
            });
    }

    public Task HideRoundRankingWidgetAsync() =>
        manialinkManager.HideManialinkAsync(WidgetTemplate);

    public async Task LoadPointsRepartitionFromSettingsAsync()
    {
        var modeScriptSettings = await matchSettingsService.GetCurrentScriptSettingsAsync();
        var pointsRepartitionString =
            (string?)modeScriptSettings?.GetValueOrDefault(PointsRepartition.ModeScriptSetting);

        if (pointsRepartitionString == null || pointsRepartitionString.Trim().Length == 0)
        {
            return;
        }

        lock (_pointsRepartitionMutex)
        {
            _pointsRepartition.Update(pointsRepartitionString);
        }
    }

    public Task SetIsTimeAttackModeAsync(bool isTimeAttackMode)
    {
        _isTimeAttackMode = isTimeAttackMode;

        return Task.CompletedTask;
    }

    public async Task DetectIsTeamsModeAsync()
    {
        var currentMode = await matchSettingsService.GetCurrentModeAsync();
        _isTeamsMode = currentMode == DefaultModeScriptName.Teams;
    }

    public async Task FetchAndCacheTeamInfoAsync()
    {
        _teamColors[PlayerTeam.Unknown] = theme.Theme.UI_AccentSecondary;
        _teamColors[PlayerTeam.Team1] = (await server.Remote.GetTeamInfoAsync((int)PlayerTeam.Team1 + 1)).RGB;
        _teamColors[PlayerTeam.Team2] = (await server.Remote.GetTeamInfoAsync((int)PlayerTeam.Team2 + 1)).RGB;
    }
}
