﻿using EvoSC.Core.Services;

namespace PluginSample;

public class SampleService : ISampleService
{
    public string GetName()
    {
        return "Sample";
    }

    public string Ping()
    {
        return "Pong";
    }
}