﻿// (c) Copyright Ascensio System SIA 2009-2025
// 
// This program is a free software product.
// You can redistribute it and/or modify it under the terms
// of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
// Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
// to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of
// any third-party rights.
// 
// This program is distributed WITHOUT ANY WARRANTY, without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see
// the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
// 
// You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
// 
// The  interactive user interfaces in modified source and object code versions of the Program must
// display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
// 
// Pursuant to Section 7(b) of the License you must retain the original Product logo when
// distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under
// trademark law for use of our trademarks.
// 
// All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode

using ASC.Core.Common;
using ASC.Files.Service.Services.Thumbnail;

namespace ASC.Files.Service.IntegrationEvents.EventHandling;

[Scope]
public class ThumbnailRequestedIntegrationEventHandler : IIntegrationEventHandler<ThumbnailRequestedIntegrationEvent>
{
    private readonly ILogger _logger;
    private readonly ChannelWriter<FileData<int>> _channelWriter;
    private readonly ITariffService _tariffService;
    private readonly TenantManager _tenantManager;
    private readonly IDbContextFactory<FilesDbContext> _dbContextFactory;
    private readonly BaseCommonLinkUtility _baseCommonLinkUtility;

    private ThumbnailRequestedIntegrationEventHandler()
    {

    }

    public ThumbnailRequestedIntegrationEventHandler(
        ILogger<ThumbnailRequestedIntegrationEventHandler> logger,
        IDbContextFactory<FilesDbContext> dbContextFactory,
        ITariffService tariffService,
        TenantManager tenantManager,
        ChannelWriter<FileData<int>> channelWriter,
        BaseCommonLinkUtility baseCommonLinkUtility)
    {
        _logger = logger;
        _channelWriter = channelWriter;
        _tariffService = tariffService;
        _tenantManager = tenantManager;
        _dbContextFactory = dbContextFactory;
        _baseCommonLinkUtility = baseCommonLinkUtility;
    }

    private async Task<IEnumerable<FileData<int>>> GetFreezingThumbnailsAsync()
    {
        await using var filesDbContext = await _dbContextFactory.CreateDbContextAsync();

        var files = await Queries.DbFilesAsync(filesDbContext).ToListAsync();

        if (files.Count == 0)
        {
            return new List<FileData<int>>();
        }

        foreach (var f in files)
        {
            f.ThumbnailStatus = Thumbnail.Waiting;
        }

        filesDbContext.UpdateRange(files);
        await filesDbContext.SaveChangesAsync();

        return await files.ToAsyncEnumerable().SelectAwait(async r =>
        {
            _ = await _tenantManager.SetCurrentTenantAsync(r.TenantId);
            var tariff = await _tariffService.GetTariffAsync(r.TenantId);
            var baseUrl = _baseCommonLinkUtility.GetFullAbsolutePath(string.Empty);
            
            var fileData = new FileData<int>(r.TenantId, r.ModifiedBy, r.Id, baseUrl, tariff.State);

            return fileData;
        }).ToListAsync();
    }


    public async Task Handle(ThumbnailRequestedIntegrationEvent @event)
    {
        CustomSynchronizationContext.CreateContext();
        using (_logger.BeginScope(new[] { new KeyValuePair<string, object>("integrationEventContext", $"{@event.Id}-{Program.AppName}") }))
        {
            var tenant = await _tenantManager.GetTenantAsync(@event.TenantId);

            if (tenant.Status != TenantStatus.Active)
            {
                return;
            }

            _logger.InformationHandlingIntegrationEvent(@event.Id, Program.AppName, @event);

            var freezingThumbnails = await GetFreezingThumbnailsAsync();

            _tenantManager.SetCurrentTenant(tenant);

            var tariff = await _tariffService.GetTariffAsync(@event.TenantId);

            var data = @event.FileIds.Select(fileId => new FileData<int>(@event.TenantId, @event.CreateBy, Convert.ToInt32(fileId), @event.BaseUrl, tariff.State))
                          .Union(freezingThumbnails);


            if (await _channelWriter.WaitToWriteAsync())
            {
                foreach (var item in data)
                {
                    await _channelWriter.WriteAsync(item);
                }
            }
        }
    }
}


static file class Queries
{
    public static readonly Func<FilesDbContext, IAsyncEnumerable<DbFile>> DbFilesAsync =
        EF.CompileAsyncQuery(
            (FilesDbContext ctx) =>
                ctx.Files
                    .Join(ctx.Tenants, f => f.TenantId, t => t.Id, (file, tenant) => new { file, tenant })
                    .Where(r => r.tenant.Status == TenantStatus.Active)
                    .Where(r => r.file.CurrentVersion && r.file.ThumbnailStatus == Thumbnail.Creating &&
                                EF.Functions.DateDiffMinute(r.file.ModifiedOn, DateTime.UtcNow) > 5)
                    .Select(r => r.file));
}