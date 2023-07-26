﻿using System.Globalization;
using System.IO.Compression;
using System.Text.RegularExpressions;
using EvoSC.Common.Interfaces.Models;
using EvoSC.Common.Interfaces.Services;
using EvoSC.Common.Models.Maps;
using EvoSC.Common.Services.Attributes;
using EvoSC.Common.Services.Models;
using EvoSC.Modules.Official.MapsModule.Interfaces;
using ManiaExchange.ApiClient;
using ManiaExchange.ApiClient.Api.Models;
using ManiaExchange.ApiClient.Models;
using Microsoft.Extensions.Logging;

namespace EvoSC.Modules.Official.MapsModule.Services;

[Service(LifeStyle = ServiceLifeStyle.Transient)]
public class MxMapService : IMxMapService
{
    private readonly ILogger<MxMapService> _logger;
    private readonly MxTmApi _tmxApi = new MxTmApi("EvoSC#");

    private static readonly Regex TMXFileNamePattern = new Regex(@"(\([0-9]+\)).Map.Gbx", RegexOptions.Compiled);

    public MxMapService(ILogger<MxMapService> logger, IMapService mapService)
    {
        _logger = logger;
    }

    public async Task<MapStream?> FindAndDownloadMapAsync(int mxId, string? shortName, IPlayer actor)
    {
        var mapInfoDto = await FindMap(mxId, shortName);
        var mapFile = await DownloadMap(mxId, shortName);

        if (mapInfoDto == null || mapFile == null)
        {
            return null;
        }

        var mapMetadata = GetMapMetadataFromInfo(mapInfoDto);

        return new MapStream(mapMetadata, mapFile);
    }

    private static MapMetadata GetMapMetadataFromInfo(TmxMapInfo mapInfoDto)
    {
        var mapMetadata = new MapMetadata
        {
            MapUid = mapInfoDto.TrackUID,
            MapName = mapInfoDto.GbxMapName,
            AuthorId = mapInfoDto.AuthorLogin,
            AuthorName = mapInfoDto.Username,
            ExternalId = mapInfoDto.TrackID.ToString(),
            ExternalVersion =
                Convert.ToDateTime(mapInfoDto.UpdatedAt, NumberFormatInfo.InvariantInfo).ToUniversalTime(),
            ExternalMapProvider = MapProviders.ManiaExchange
        };
        return mapMetadata;
    }

    public async Task<Stream?> DownloadMap(int mxId, string? shortName)
    {
        var mapFile = await _tmxApi.DownloadMapAsync(mxId, shortName);

        if (mapFile == null)
        {
            _logger.LogDebug("Could not find any map stream for ID {MxId} from Trackmania Exchange", mxId);
            return null;
        }

        return mapFile;
    }

    public async Task<TmxMapInfo?> FindMap(int mxId, string? shortName)
    {
        var mapInfoDto = await _tmxApi.GetMapInfoAsync(mxId, shortName);

        if (mapInfoDto == null)
        {
            _logger.LogDebug("Could not find any map info for ID {MxId} from Trackmania Exchange", mxId);
            return null;
        }

        return mapInfoDto;
    }

    public async Task<IEnumerable<MapStream>> FindAndDownloadMapPack(int mxId, string? shortName, IPlayer actor)
    {
        var mapInfosDto = await FindMapPackMaps(mxId, shortName);
        var mapFiles = await DownloadMapPack(mxId, shortName);

        if (mapInfosDto.Length == 0 || mapFiles == null)
        {
            return Enumerable.Empty<MapStream>();
        }

        var files = mapFiles.Entries.ToDictionary(entry => TMXFileNamePattern.Match(entry.Name).Groups[1].Value,
            entry => entry);

        return mapInfosDto.Select(info =>
            new MapStream(GetMapMetadataFromInfo(info), files[info.TrackID.ToString()].Open()));
    }

    public async Task<ZipArchive?> DownloadMapPack(int mxId, string? shortName)
    {
        var mapPackFile = await _tmxApi.DownloadMappack(mxId, shortName);

        if (mapPackFile == null)
        {
            _logger.LogDebug("Could not find any map pack stream for ID {MxId} from Trackmania Exchange", mxId);
            return null;
        }

        var zip = new ZipArchive(mapPackFile);
        return zip;
    }

    public async Task<TmxMappackInfo?> FindMapPack(int mxId, string? shortName)
    {
        var mapPackInfoDto = await _tmxApi.GetMappackInfoAsync(mxId, shortName);

        if (mapPackInfoDto == null)
        {
            _logger.LogDebug("Could not find any map pack info for ID {MxId} from Trackmania Exchange", mxId);
            return null;
        }

        return mapPackInfoDto;
    }

    public async Task<TmxMappackMapInfo[]> FindMapPackMaps(int mxId, string? shortName)
    {
        var mapPackInfosDto = await _tmxApi.GetMappackMapsAsync(mxId, shortName);

        if (mapPackInfosDto == null)
        {
            _logger.LogDebug("Could not find any map pack map infos for ID {MxId} from Trackmania Exchange", mxId);
            return Array.Empty<TmxMappackMapInfo>();
        }

        return mapPackInfosDto;
    }
}
