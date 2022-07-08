﻿using EvoSC.Core.Commands.Chat.Interfaces;
using EvoSC.Core.Commands.Generic.Interfaces;
using EvoSC.Domain.Players;
using EvoSC.Interfaces.Chat;
using EvoSC.Interfaces.Players;
using GbxRemoteNet;
using Microsoft.AspNetCore.Hosting.Server;

namespace EvoSC.Core.Commands.Chat;

public class ChatCommandContext : IChatCommandContext
{
    public GbxRemoteClient Client { get; }
    public IServerPlayer Player { get; }
    public IServerServerChatMessage Message { get; }

    public ChatCommandContext(GbxRemoteClient client, IServerPlayer player, IServerServerChatMessage message)
    {
        Client = client;
        Player = player;
        Message = message;
    }
}
