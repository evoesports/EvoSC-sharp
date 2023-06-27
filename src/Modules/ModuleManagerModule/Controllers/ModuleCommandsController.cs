﻿using EvoSC.Commands;
using EvoSC.Commands.Attributes;
using EvoSC.Common.Controllers;
using EvoSC.Common.Controllers.Attributes;
using EvoSC.Modules.Interfaces;
using EvoSC.Modules.Official.ModuleManagerModule.Interfaces;

namespace EvoSC.Modules.Official.ModuleManagerModule.Controllers;

[Controller]
public class ModuleCommandsController : EvoScController<CommandInteractionContext>
{
    private readonly IModuleManagerService _moduleManagerService;
    
    public ModuleCommandsController(IModuleManagerService moduleManagerService)
    {
        _moduleManagerService = moduleManagerService;
    }

    [ChatCommand("enablemodule", "[Command.EnableModule]", ModuleManagerPermissions.ActivateModule)]
    public Task EnableModuleAsync(IModuleLoadContext module) => _moduleManagerService.EnableModuleAsync(module);

    [ChatCommand("disablemodule", "[Command.DisableModule]", ModuleManagerPermissions.ActivateModule)]
    public Task DisableModuleAsync(IModuleLoadContext module) => _moduleManagerService.DisableModuleAsync(module);

    [ChatCommand("modules", "[Command.Modules]")]
    public Task ListLoadedModulesAsync() => _moduleManagerService.ListModulesAsync(Context.Player);
}