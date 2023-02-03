﻿using System.ComponentModel;
using System.Text;
using EvoSC.Commands;
using EvoSC.Commands.Attributes;
using EvoSC.Common.Controllers;
using EvoSC.Common.Controllers.Attributes;
using EvoSC.Common.Interfaces;
using EvoSC.Common.Interfaces.Services;
using EvoSC.Modules.Official.MatchManagerModule.Exceptions;
using EvoSC.Modules.Official.MatchManagerModule.Interfaces;
using EvoSC.Modules.Official.MatchManagerModule.Permissions;
using Microsoft.Extensions.Logging;

namespace EvoSC.Modules.Official.MatchManagerModule.Controllers;

[Controller]
public class MatchSettingsCommandsController : EvoScController<CommandInteractionContext>
{
    private readonly IMatchManagerHandlerService _matchHandler;
    private readonly ILiveModeService _liveModes;

    public MatchSettingsCommandsController(IMatchManagerHandlerService matchHandler, ILiveModeService liveModes)
    {
        _matchHandler = matchHandler;
        _liveModes = liveModes;
    }

    [ChatCommand("setmode", "Change current game mode.", MatchManagerPermissions.SetLiveMode)]
    [CommandAlias("/mode", hide: true)]
    public Task SetModeAsync(
        [Description("The mode to change to.")]
        string mode
    ) => _matchHandler.SetModeAsync(mode, Context.Player);

    [ChatCommand("loadmatchsettings", "Load a match settings file.", MatchManagerPermissions.LoadMatchSettings)]
    [CommandAlias("/loadmatch", hide: true)]
    public Task LoadMatchSettingsAsync(
        [Description("The name of the matchsettings file, without extension.")]
        string name
    ) => _matchHandler.LoadMatchSettingsAsync(name, Context.Player);
}