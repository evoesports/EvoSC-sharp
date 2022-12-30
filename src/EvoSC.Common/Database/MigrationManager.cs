﻿using System.Reflection;
using EvoSC.Common.Config.Models;
using EvoSC.Common.Interfaces;
using EvoSC.Common.Logging;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Generators;
using FluentMigrator.Runner.Processors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EvoSC.Common.Database;

public class MigrationManager : IMigrationManager
{
    private readonly IEvoScBaseConfig _config;

    public MigrationManager(IEvoScBaseConfig config)
    {
        _config = config;
    }

    public void MigrateFromAssembly(Assembly asm)
    {
        var provider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(c => c
                .AddMySql5()
                .AddSQLite()
                .AddPostgres()
                .WithGlobalConnectionString(_config.Database.GetConnectionString())
                .ScanIn(asm).For.Migrations())
            .Configure<SelectingGeneratorAccessorOptions>(x =>
                x.GeneratorId = GetDatabaseTypeIdentifier(_config.Database.Type))
            .AddEvoScLogging(_config.Logging)
            .BuildServiceProvider(false);

        provider.GetRequiredService<IMigrationRunner>()
            .MigrateUp();

        provider.Dispose();
    }

    private string GetDatabaseTypeIdentifier(IDatabaseConfig.DatabaseType databaseType)
    {
        switch (databaseType)
        {
            case IDatabaseConfig.DatabaseType.MySql:
                return "MySql5";
            case IDatabaseConfig.DatabaseType.SQLite:
                return "Sqlite";
            default:
                return "Postgres";
        }
    }
}
