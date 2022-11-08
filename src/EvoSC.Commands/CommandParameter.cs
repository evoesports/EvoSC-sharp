﻿using System.Reflection;
using EvoSC.Commands.Interfaces;

namespace EvoSC.Commands;

public class CommandParameter : ICommandParameter
{
    public string? Description { get; init; }
    public ParameterInfo ParameterInfo { get; }

    public CommandParameter(ParameterInfo parInfo, string? description=null)
    {
        Description = description;
        ParameterInfo = parInfo;
    }
}