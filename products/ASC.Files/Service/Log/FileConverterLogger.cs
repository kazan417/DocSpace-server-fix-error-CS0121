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

namespace ASC.Files.Core.Log;
public static partial class FileConverterLogger
{
    [LoggerMessage(LogLevel.Debug, "Run CheckConvertFilesStatus: count {count}")]
    public static partial void DebugRunCheckConvertFilesStatus(this ILogger logger, int count);

    [LoggerMessage(LogLevel.Debug, "CheckConvertFilesStatus iteration continue")]
    public static partial void DebugCheckConvertFilesStatusIterationContinue(this ILogger logger);

    [LoggerMessage(LogLevel.Debug, "CheckConvertFilesStatus iteration end")]
    public static partial void DebugCheckConvertFilesStatusIterationEnd(this ILogger logger);

    [LoggerMessage(LogLevel.Error, "Error convert {fileId} with url {url}")]
    public static partial void ErrorConvertFileWithUrl(this ILogger logger, string fileId, string url, Exception exception);

    [LoggerMessage(LogLevel.Error, "{operationResultError} ConvertUrl: {convertedFileUrl} fromUrl: {fileUri} ConvertedFileType: {convertedFileType}")]
    public static partial void ErrorOperation(this ILogger logger, string operationResultError, string convertedFileUrl, string fileUri, string convertedFileType, Exception exception);
    
    [LoggerMessage(LogLevel.Error, "CheckConvertFilesStatus timeout: {fileId} ({contentLengthString})")]
    public static partial void ErrorCheckConvertFilesStatus(this ILogger logger, string fileId, long contentLengthString);

    [LoggerMessage(LogLevel.Debug, "FileConverterService is starting.")]
    public static partial void DebugFileConverterServiceRuning(this ILogger logger);

    [LoggerMessage(LogLevel.Debug, "FileConverterService is stopping")]
    public static partial void DebugFileConverterServiceStopping(this ILogger logger);

}
