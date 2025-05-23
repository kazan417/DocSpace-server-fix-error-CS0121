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

namespace ASC.Web.Core.Notify;
[Scope]
public class NotificationControllerHelper(
    StudioNotifyHelper studioNotifyHelper,
    AuthContext authContext,
    BadgesSettingsHelper badgesSettingsHelper)
{
    private readonly Guid _userId = authContext.CurrentAccount.ID;

    public async Task<bool> GetNotificationStatusAsync(NotificationType notificationType)
    {
        bool isEnabled;

        switch (notificationType)
        {
            case NotificationType.Badges:
                return await badgesSettingsHelper.GetEnabledForCurrentUserAsync();
            case NotificationType.RoomsActivity:
                isEnabled = await studioNotifyHelper.IsSubscribedToNotifyAsync(_userId, Actions.RoomsActivity);
                return isEnabled;
            case NotificationType.DailyFeed:
                isEnabled = await studioNotifyHelper.IsSubscribedToNotifyAsync(_userId, Actions.SendWhatsNew);
                return isEnabled;
            case NotificationType.UsefullTips:
                isEnabled = await studioNotifyHelper.IsSubscribedToNotifyAsync(_userId, Actions.PeriodicNotify);
                return isEnabled;
            default:
                throw new Exception("Incorrect parameters");
        }
    }

    public async Task SetNotificationStatusAsync(NotificationType notificationType, bool isEnabled)
    {
        switch (notificationType)
        {
            case NotificationType.Badges:
                await badgesSettingsHelper.SetEnabledForCurrentUserAsync(isEnabled);
                break;
            case NotificationType.RoomsActivity:
                await studioNotifyHelper.SubscribeToNotifyAsync(_userId, Actions.RoomsActivity, isEnabled);
                break;
            case NotificationType.DailyFeed:
                await studioNotifyHelper.SubscribeToNotifyAsync(_userId, Actions.SendWhatsNew, isEnabled);
                break;
            case NotificationType.UsefullTips:
                await studioNotifyHelper.SubscribeToNotifyAsync(_userId, Actions.PeriodicNotify, isEnabled);
                break;
        }
    }
}

/// <summary>
/// The notification type.
/// </summary>
public enum NotificationType
{
    [SwaggerEnum("Badges")]
    Badges = 0,

    [SwaggerEnum("Rooms activity")]
    RoomsActivity = 1,

    [SwaggerEnum("Daily feed")]
    DailyFeed = 2,

    [SwaggerEnum("Usefull tips")]
    UsefullTips = 3
}
