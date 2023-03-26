﻿using Microsoft.Extensions.Logging;

namespace EvoSC.Modules.Official.FastestCp.Models;

internal class FastestCpStore
{
    private readonly List<AccountIdCpTime?> _fastestTimes = new();
    private readonly ILogger<FastestCpStore> _logger;

    public FastestCpStore(ILogger<FastestCpStore> logger)
    {
        _logger = logger;
    }

    public bool RegisterTime(string accountId, int cpIndex, int cpTime)
    {
        var accountIdCpTime = new AccountIdCpTime(accountId, cpTime);

        lock (_fastestTimes)
        {
            if (cpIndex >= _fastestTimes.Count)
            {
                _logger.LogDebug(
                    "Extending fastest checkpoint list from {oldSize} to {newSize} to insert first time driven ({cpTime}) at checkpoint {cpIndex}",
                    _fastestTimes.Count, cpIndex + 1, cpTime, cpIndex);
                _fastestTimes.AddRange(
                    new AccountIdCpTime[cpIndex - _fastestTimes.Count + 1]);
                _fastestTimes[cpIndex] = new AccountIdCpTime(accountId, cpTime);
                return true;
            }

            if (_fastestTimes[cpIndex] == null)
            {
                _logger.LogDebug(
                    "Inserting first checkpoint time ({cpTime}) at checkpoint {cpIndex} driven by account {accountId}",
                    cpTime, cpIndex, accountId);
                _fastestTimes[cpIndex] = new AccountIdCpTime(accountId, cpTime);
                return true;
            }

            if (_fastestTimes[cpIndex]!.RaceTime > cpTime)
            {
                _logger.LogDebug(
                    "Update fastest checkpoint time ({cpTime}) at checkpoint {cpIndex} driven by account {accountId}",
                    cpTime, cpIndex, accountId);
                _fastestTimes[cpIndex] = accountIdCpTime;
                return true;
            }

            _logger.LogTrace(
                "Do not update slower checkpoint time ({cpTime}) at checkpoint {cpIndex} driven by account {accountId}",
                cpTime, cpIndex, accountId);
            return false;
        }
    }

    public List<AccountIdCpTime?> GetFastestTimes()
    {
        return _fastestTimes.ToList();
    }
}
