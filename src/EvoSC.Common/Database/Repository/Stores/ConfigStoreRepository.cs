﻿using EvoSC.Common.Database.Models.Config;
using EvoSC.Common.Interfaces.Database;
using EvoSC.Common.Interfaces.Database.Repository;
using RepoDb;

namespace EvoSC.Common.Database.Repository.Stores;

public class ConfigStoreRepository : EvoScDbRepository, IConfigStoreRepository
{
    public ConfigStoreRepository(IDbConnectionFactory connectionFactory) : base(connectionFactory)
    {
    }
    
    public async Task<DbConfigOption?> GetConfigOptionsByKeyAsync(string keyName)
    {
        var option = await Database.QueryAsync<DbConfigOption>(e => e.Key == keyName);
        return option.FirstOrDefault();
    }
    
    public async Task AddConfigOptionAsync(DbConfigOption option) => await Database.InsertAsync(option);

    public async Task UpdateConfigOptionAsync(DbConfigOption option) => await Database.UpdateAsync(option);
}