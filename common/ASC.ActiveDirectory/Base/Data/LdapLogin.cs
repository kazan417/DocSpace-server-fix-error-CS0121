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

namespace ASC.ActiveDirectory.Base.Data;
public class LdapLogin(string username, string domain)
{
    public string Username { get; private set; } = username;
    public string Domain { get; private set; } = domain;

    public override string ToString()
    {
        return !string.IsNullOrEmpty(Domain) ? string.Format("{0}@{1}", Username, Domain) : Username;
    }

    public static LdapLogin ParseLogin(string login)
    {
        if (string.IsNullOrEmpty(login))
        {
            return null;
        }

        string username;
        string domain = null;

        if (login.Contains('\\'))
        {
            var splited = login.Split('\\');

            if (splited.Length is 0 or not 2)
            {
                return null;
            }

            domain = splited[0];
            username = splited[1];

        }
        else if (login.Contains('@'))
        {
            var splited = login.Split('@');

            if (splited.Length is 0 or not 2)
            {
                return null;
            }

            username = splited[0];
            domain = splited[1];
        }
        else
        {
            username = login;
        }

        var result = new LdapLogin(username, domain);

        return result;
    }
}
