﻿using EvoSC.Common.Controllers;
using EvoSC.Common.Controllers.Attributes;
using EvoSC.Common.Events.Attributes;
using EvoSC.Common.Interfaces.Controllers;
using EvoSC.Common.Models;
using EvoSC.Common.Remote;
using EvoSC.Common.Remote.EventArgsModels;
using EvoSC.Modules.Official.MatchRankingModule.Interfaces;

namespace EvoSC.Modules.Official.MatchRankingModule.Controllers;

[Controller]
public class MatchRankingEventController : EvoScController<IEventControllerContext>
{
    private readonly IMatchRankingService _matchRankingService;

    public MatchRankingEventController(IMatchRankingService matchRankingService)
    {
        _matchRankingService = matchRankingService;
    }

    [Subscribe(ModeScriptEvent.Scores)]
    public async Task OnScores(object data, ScoresEventArgs eventArgs)
    {
        if (eventArgs.Section is ModeScriptSection.EndMatch)
        {
            await _matchRankingService.ResetMatchData();
            await _matchRankingService.HideManialink();
            return;
        }
        
        await _matchRankingService.UpdateAndShowScores(eventArgs);
    }

    [Subscribe(ModeScriptEvent.StartRoundStart)]
    public async Task OnBeginMapAsync(object sender, RoundEventArgs args)
    {
        await _matchRankingService.SendManialink();
    }

    [Subscribe(ModeScriptEvent.StartMatchStart)]
    public async Task OnStartMatch(object sender, MatchEventArgs eventArgs)
    {
        await _matchRankingService.ResetMatchData();
        await _matchRankingService.SendManialink();
    }

    [Subscribe(ModeScriptEvent.PodiumStart)]
    public async Task OnPodiumStart(object sender, PodiumEventArgs args)
    {
        await _matchRankingService.HideManialink();
    }
}