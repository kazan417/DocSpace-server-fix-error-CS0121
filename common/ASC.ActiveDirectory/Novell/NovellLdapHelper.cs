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

namespace ASC.ActiveDirectory.Novell;

[Scope]
public class NovellLdapHelper(IServiceProvider serviceProvider, ILogger<LdapHelper> logger,
        InstanceCrypto instanceCrypto, IConfiguration configuration, NovellLdapSearcher novellLdapSearcher,
        LdapObjectExtension ldapObjectExtension)
    : LdapHelper(logger, instanceCrypto)
{
    public new void Init(LdapSettings settings)
    {
        var password = string.IsNullOrEmpty(settings.Password)
            ? GetPassword(settings.PasswordBytes)
            : settings.Password;

        novellLdapSearcher.Init(settings.Login, password, settings.Server, settings.PortNumber,
            settings.StartTls, settings.Ssl, settings.AcceptCertificate, settings.AcceptCertificateHash);

        base.Init(settings);
    }

    public override bool IsConnected
    {
        get { return novellLdapSearcher.IsConnected; }
    }

    public override void Connect()
    {
        novellLdapSearcher.Connect();

        Settings.AcceptCertificate = novellLdapSearcher.AcceptCertificate;
        Settings.AcceptCertificateHash = novellLdapSearcher.AcceptCertificateHash;
    }

    public override Dictionary<string, string[]> GetCapabilities()
    {
        return novellLdapSearcher.GetCapabilities();
    }

    public override string SearchDomain()
    {
        try
        {
            var capabilities = GetCapabilities();

            if (capabilities.Count > 0)
            {
                if (capabilities.TryGetValue("defaultNamingContext", out var capability))
                {
                    var dn = capability.FirstOrDefault(dc =>
                        !string.IsNullOrEmpty(dc) &&
                        dc.Contains("dc=", StringComparison.InvariantCultureIgnoreCase));

                    var domain = LdapUtils.DistinguishedNameToDomain(dn);

                    if (!string.IsNullOrEmpty(domain))
                    {
                        return domain;
                    }
                }

                if (capabilities.TryGetValue("rootDomainNamingContext", out var capability1))
                {
                    var dn = capability1.FirstOrDefault(dc =>
                        !string.IsNullOrEmpty(dc) &&
                        dc.Contains("dc=", StringComparison.InvariantCultureIgnoreCase));

                    var domain = LdapUtils.DistinguishedNameToDomain(dn);

                    if (!string.IsNullOrEmpty(domain))
                    {
                        return domain;
                    }
                }

                if (capabilities.TryGetValue("namingContexts", out var dnList))
                {
                    var dn = dnList.FirstOrDefault(dc =>
                        !string.IsNullOrEmpty(dc) &&
                        dc.Contains("dc=", StringComparison.InvariantCultureIgnoreCase));

                    var domain = LdapUtils.DistinguishedNameToDomain(dn);

                    if (!string.IsNullOrEmpty(domain))
                    {
                        return domain;
                    }
                }
            }
        }
        catch (Exception e)
        {
            _logger.WarnSearchDomainFailed(e);
        }

        try
        {
            var searchResult =
                novellLdapSearcher.Search(Settings.UserDN, NovellLdapSearcher.LdapScope.Sub, Settings.UserFilter, limit: 1)
                    .FirstOrDefault();

            return searchResult != null ? ldapObjectExtension.GetDomainFromDn(searchResult) : null;
        }
        catch (Exception e)
        {
            _logger.WarnSearchDomainFailed(e);
        }

        return null;
    }

    public override void CheckCredentials(string login, string password, string server, int portNumber,
        bool startTls, bool ssl, bool acceptCertificate, string acceptCertificateHash)
    {
        using var novellLdapSearcher = serviceProvider.GetRequiredService<NovellLdapSearcher>();
        novellLdapSearcher.Init(login, password, server, portNumber, startTls, ssl, acceptCertificate, acceptCertificateHash);
        novellLdapSearcher.Connect();
    }

    public override bool CheckUserDn(string userDn)
    {
        string[] attributes = [LdapConstants.ADSchemaAttributes.OBJECT_CLASS];

        var searchResult = novellLdapSearcher.Search(userDn, NovellLdapSearcher.LdapScope.Base,
            LdapConstants.OBJECT_FILTER, attributes, 1);

        if (searchResult.Count != 0)
        {
            return true;
        }

        _logger.ErrorWrongUserDnParameter(userDn);
        return false;
    }

    public override bool CheckGroupDn(string groupDn)
    {
        string[] attributes = [LdapConstants.ADSchemaAttributes.OBJECT_CLASS];

        var searchResult = novellLdapSearcher.Search(groupDn, NovellLdapSearcher.LdapScope.Base,
            LdapConstants.OBJECT_FILTER, attributes, 1);

        if (searchResult.Count != 0)
        {
            return true;
        }

        _logger.ErrorWrongGroupDnParameter(groupDn);
        return false;
    }

    public override List<LdapObject> GetUsers(string filter = null, int limit = -1)
    {
        var list = new List<LdapObject>();

        try
        {
            if (!string.IsNullOrEmpty(Settings.UserFilter) && !Settings.UserFilter.StartsWith('(') &&
                !Settings.UserFilter.EndsWith(')'))
            {
                Settings.UserFilter = string.Format("({0})", Settings.UserFilter);
            }

            if (!string.IsNullOrEmpty(filter) && !filter.StartsWith('(') &&
                !filter.EndsWith(')'))
            {
                filter = string.Format("({0})", Settings.UserFilter);
            }

            var searchfilter = string.IsNullOrEmpty(filter)
                ? Settings.UserFilter
                : string.Format("(&{0}{1})", Settings.UserFilter, filter);

            list = novellLdapSearcher.Search(Settings.UserDN, NovellLdapSearcher.LdapScope.Sub, searchfilter, limit: limit);

            return list;
        }
        catch (Exception e)
        {
            _logger.ErrorGetUsersFailed(filter, limit, e);
        }

        return list;
    }

    public override LdapObject GetUserBySid(string sid)
    {
        try
        {
            var ldapUniqueIdAttribute = configuration["ldap:unique:id"];

            Criteria criteria;

            if (ldapUniqueIdAttribute == null)
            {
                criteria = Criteria.Any(
                    Expression.Equal(LdapConstants.RfcLDAPAttributes.ENTRY_UUID, sid),
                    Expression.Equal(LdapConstants.RfcLDAPAttributes.NS_UNIQUE_ID, sid),
                    Expression.Equal(LdapConstants.RfcLDAPAttributes.GUID, sid),
                    Expression.Equal(LdapConstants.ADSchemaAttributes.OBJECT_SID, sid)
                    );
            }
            else
            {
                criteria = Criteria.All(Expression.Equal(ldapUniqueIdAttribute, sid));
            }

            var searchfilter = string.Format("(&{0}{1})", Settings.UserFilter, criteria);

            var list = novellLdapSearcher.Search(Settings.UserDN, NovellLdapSearcher.LdapScope.Sub, searchfilter, limit: 1);

            return list.FirstOrDefault();
        }
        catch (Exception e)
        {
            _logger.ErrorGetUserBySidFailed(sid, e);
        }

        return null;
    }

    public override List<LdapObject> GetGroups(Criteria criteria = null)
    {
        var list = new List<LdapObject>();

        try
        {
            if (!string.IsNullOrEmpty(Settings.GroupFilter) && !Settings.GroupFilter.StartsWith('(') &&
                !Settings.GroupFilter.EndsWith(')'))
            {
                Settings.GroupFilter = string.Format("({0})", Settings.GroupFilter);
            }

            var searchfilter = criteria == null
                ? Settings.GroupFilter
                : string.Format("(&{0}{1})", Settings.GroupFilter, criteria);


            list = novellLdapSearcher.Search(Settings.GroupDN, NovellLdapSearcher.LdapScope.Sub, searchfilter);
        }
        catch (Exception e)
        {
            _logger.ErrorGetGroupsFailed(criteria, e);
        }

        return list;
    }

    public override void Dispose()
    {
        novellLdapSearcher.Dispose();
    }
}
