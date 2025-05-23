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

namespace ASC.EventBus.RabbitMQ.Log;
internal static partial class DefaultRabbitMQPersistentConnectionLogger
{
    [LoggerMessage(LogLevel.Critical, "DefaultRabbitMQPersistentConnection")]
    public static partial void CriticalDefaultRabbitMQPersistentConnection(this ILogger<DefaultRabbitMQPersistentConnection> logger, Exception exception);

    [LoggerMessage(LogLevel.Information, "RabbitMQ Client is trying to connect")]
    public static partial void InformationRabbitMQTryingConnect(this ILogger<DefaultRabbitMQPersistentConnection> logger);

    [LoggerMessage(LogLevel.Warning, "RabbitMQ Client could not connect after {timeOut}s")]
    public static partial void WarningRabbitMQCouldNotConnect(this ILogger<DefaultRabbitMQPersistentConnection> logger, double timeOut, Exception exception);

    [LoggerMessage(LogLevel.Information, "RabbitMQ Client acquired a persistent connection to '{hostName}' and is subscribed to failure events")]
    public static partial void InformationRabbitMQAcquiredPersistentConnection(this ILogger<DefaultRabbitMQPersistentConnection> logger, string hostName);

    [LoggerMessage(LogLevel.Critical, "FATAL ERROR: RabbitMQ connections could not be created and opened")]
    public static partial void CriticalRabbitMQCouldNotBeCreated(this ILogger<DefaultRabbitMQPersistentConnection> logger);

    [LoggerMessage(LogLevel.Warning, "A RabbitMQ connection is shutdown. Trying to re-connect...")]
    public static partial void WarningRabbitMQConnectionShutdown(this ILogger<DefaultRabbitMQPersistentConnection> logger);

    [LoggerMessage(LogLevel.Warning, "A RabbitMQ connection throw exception. Trying to re-connect...")]
    public static partial void WarningRabbitMQConnectionThrowException(this ILogger<DefaultRabbitMQPersistentConnection> logger);

    [LoggerMessage(LogLevel.Warning, "A RabbitMQ connection is on shutdown. Trying to re-connect...")]
    public static partial void WarningRabbitMQConnectionIsOnShutDown(this ILogger<DefaultRabbitMQPersistentConnection> logger);
}
