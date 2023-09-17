﻿using EvoSC.Common.Controllers;
using EvoSC.Common.Controllers.Attributes;
using EvoSC.Common.Events.Attributes;
using EvoSC.Common.Interfaces.Controllers;
using EvoSC.Common.Remote;
using EvoSC.Common.Remote.EventArgsModels;
using EvoSC.Modules.Official.WorldRecordModule.Interfaces;

namespace EvoSC.Modules.Official.WorldRecordModule.Controllers;

[Controller]
public class WorldRecordEventController : EvoScController<IEventControllerContext>
{
    private readonly IWorldRecordService _worldRecordService;

    public WorldRecordEventController(IWorldRecordService worldRecordService)
    {
        _worldRecordService = worldRecordService;
    }

    [Subscribe(ModeScriptEvent.Scores)]
    public async Task OnScores(object sender, ScoresEventArgs scoresEventArgs)
        => await _worldRecordService.DetectNewWorldRecordThroughScores(scoresEventArgs);
}
