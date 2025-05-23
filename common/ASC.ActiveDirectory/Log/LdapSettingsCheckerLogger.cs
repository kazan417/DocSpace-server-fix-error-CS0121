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

namespace ASC.ActiveDirectory.Log;
internal static partial class LdapSettingsCheckerLogger
{
    [LoggerMessage(LogLevel.Error, "CheckSettings(acceptCertificate={acceptCertificate}): NovellLdapTlsCertificateRequestedException")]
    public static partial void ErrorNovellLdapTlsCertificateRequestedException(this ILogger<LdapSettingsChecker> logger, bool acceptCertificate, Exception exception);

    [LoggerMessage(LogLevel.Error, "CheckSettings(): NotSupportedException")]
    public static partial void ErrorNotSupportedException(this ILogger<LdapSettingsChecker> logger, Exception exception);

    [LoggerMessage(LogLevel.Error, "CheckSettings(): SocketException")]
    public static partial void ErrorSocketException(this ILogger<LdapSettingsChecker> logger, Exception exception);

    [LoggerMessage(LogLevel.Error, "CheckSettings(): ArgumentException")]
    public static partial void ErrorArgumentException(this ILogger<LdapSettingsChecker> logger, Exception exception);

    [LoggerMessage(LogLevel.Error, "CheckSettings(): SecurityException")]
    public static partial void ErrorSecurityException(this ILogger<LdapSettingsChecker> logger, Exception exception);

    [LoggerMessage(LogLevel.Error, "CheckSettings(): SystemException")]
    public static partial void ErrorSystemException(this ILogger<LdapSettingsChecker> logger, Exception exception);

    [LoggerMessage(LogLevel.Error, "CheckSettings(): Exception")]
    public static partial void ErrorCheckSettingsException(this ILogger<LdapSettingsChecker> logger, Exception exception);

    [LoggerMessage(LogLevel.Error, "Wrong User DN parameter: {userDn}")]
    public static partial void ErrorWrongUserDn(this ILogger<LdapSettingsChecker> logger, string userDn, Exception exception);

    [LoggerMessage(LogLevel.Error, "Wrong Group DN parameter: {groupDn}")]
    public static partial void ErrorWrongGroupDn(this ILogger<LdapSettingsChecker> logger, string groupDn, Exception exception);
}
