// (c) Copyright Ascensio System SIA 2009-2025
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

using Profile = AutoMapper.Profile;

namespace ASC.Core.Tenants;

/// <summary>
/// The tenant parameters.
/// </summary>
[ProtoContract]
public class Tenant : IMapFrom<DbTenant>
{
    public const int DefaultTenant = -1;

    public static readonly string HostName = Dns.GetHostName().ToLowerInvariant();
    public const string LocalHost = "localhost";

    private List<string> _domains;

    public Tenant()
    {
        Id = DefaultTenant;
        TimeZone = TimeZoneInfo.Utc.Id;
        Language = CultureInfo.CurrentCulture.Name;
        TrustedDomains = [];
        TrustedDomainsType = TenantTrustedDomainsType.None;
        CreationDateTime = DateTime.UtcNow;
        Status = TenantStatus.Active;
        StatusChangeDate = DateTime.UtcNow;
        VersionChanged = DateTime.UtcNow;
        Industry = TenantIndustry.Other;
    }

    public Tenant(string alias)
        : this()
    {
        Alias = alias.ToLowerInvariant();
    }

    public Tenant(int id, string alias)
        : this(alias)
    {
        Id = id;
    }

    [ProtoMember(1)]
    public string AffiliateId { get; set; }
    
    [ProtoMember(2)]
    public string Alias { get; set; }
    
    [ProtoMember(3)]
    public bool Calls { get; set; }
    
    [ProtoMember(4)]
    public string Campaign { get; set; }
    
    [ProtoMember(5)]
    public DateTime CreationDateTime { get; set; }
    
    [ProtoMember(6)]
    public string HostedRegion { get; set; }
    
    [ProtoMember(7)]
    public int Id { get; set; }
    
    [ProtoMember(8)]
    public TenantIndustry Industry { get; set; }
    
    [ProtoMember(9)]
    public string Language { get; set; }
    
    [ProtoMember(10)]
    public DateTime LastModified { get; set; }
    
    [ProtoMember(11)]
    public string MappedDomain { get; set; }
    
    [ProtoMember(12)]
    public string Name { get; set; }
    
    [ProtoMember(13)]
    public Guid OwnerId { get; set; }
    
    [ProtoMember(14)]
    public string PartnerId { get; set; }
    
    [ProtoMember(15)]
    public string PaymentId { get; set; }
    
    [ProtoMember(16)]
    public TenantStatus Status { get; set; }
    
    [ProtoMember(17)]
    public DateTime StatusChangeDate { get; set; }
    
    [ProtoMember(18)]
    public string TimeZone { get; set; }
    
    [ProtoMember(19)]
    public List<string> TrustedDomains
    {
        get
        {
            if (_domains.Count == 0 && !string.IsNullOrEmpty(TrustedDomainsRaw))
            {
                _domains = TrustedDomainsRaw.Split('|',
                    StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            return _domains;
        }
        
        set => _domains = value;
    }

    [ProtoMember(20)]
    public string TrustedDomainsRaw { get; set; }
    
    [ProtoMember(21)]
    public TenantTrustedDomainsType TrustedDomainsType { get; set; }
    
    [ProtoMember(22)]
    public int Version { get; set; }
    
    [ProtoMember(23)]
    public DateTime VersionChanged { get; set; }
    
    public override bool Equals(object obj)
    {
        return obj is Tenant t && t.Id == Id;
    }

    public CultureInfo GetCulture() => !string.IsNullOrEmpty(Language) ? CultureInfo.GetCultureInfo(Language.Trim()) : CultureInfo.CurrentCulture;
    
    public override int GetHashCode()
    {
        return Id;
    }

    public string GetTenantDomain(CoreSettings coreSettings, bool allowMappedDomain = true)
    {
        var baseHost = coreSettings.GetBaseDomain(HostedRegion);

        if (string.IsNullOrEmpty(baseHost) && !string.IsNullOrEmpty(HostedRegion))
        {
            baseHost = HostedRegion;
        }

        string result;
        if (baseHost == "localhost" || Alias == "localhost")
        {
            //single tenant on local host
            Alias = "localhost";
            result = Alias;
        }
        else
        {
            result = $"{Alias}.{baseHost}".TrimEnd('.').ToLowerInvariant();
        }

        if (string.IsNullOrEmpty(MappedDomain) || !allowMappedDomain)
        {
            return result;
        }

        if (MappedDomain.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase))
        {
            MappedDomain = MappedDomain[7..];
        }
        if (MappedDomain.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
        {
            MappedDomain = MappedDomain[8..];
        }
        
        result = MappedDomain.ToLowerInvariant();

        return result;
    }

    public void SetStatus(TenantStatus status)
    {
        Status = status;
        StatusChangeDate = DateTime.UtcNow;
    }

    public override string ToString()
    {
        return Alias;
    }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<DbTenant, Tenant>()
            .ForMember(r => r.TrustedDomainsType, opt => opt.MapFrom(src => src.TrustedDomainsEnabled))
            .ForMember(r => r.AffiliateId, opt => opt.MapFrom(src => src.Partner.AffiliateId))
            .ForMember(r => r.PartnerId, opt => opt.MapFrom(src => src.Partner.PartnerId))
            .ForMember(r => r.Campaign, opt => opt.MapFrom(src => src.Partner.Campaign));

        profile.CreateMap<TenantUserSecurity, Tenant>()
            .IncludeMembers(src => src.DbTenant);
    }

    internal string GetTrustedDomains()
    {
        TrustedDomains.RemoveAll(string.IsNullOrEmpty);
        if (TrustedDomains.Count == 0)
        {
            return null;
        }

        return string.Join("|", TrustedDomains.ToArray());
    }

    internal void SetTrustedDomains(string trustedDomains)
    {
        if (string.IsNullOrEmpty(trustedDomains))
        {
            TrustedDomains.Clear();
        }
        else
        {
            TrustedDomains.AddRange(trustedDomains.Split(['|'], StringSplitOptions.RemoveEmptyEntries));
        }
    }
}
