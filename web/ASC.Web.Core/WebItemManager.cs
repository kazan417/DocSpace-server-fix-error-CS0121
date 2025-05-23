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

namespace ASC.Web.Core;

[Flags]
public enum ItemAvailableState
{
    Normal = 1,
    Disabled = 2,
    All = Normal | Disabled
}

[Singleton]
public class WebItemManager
{
    private readonly ILogger _log;

    private ConcurrentDictionary<Guid, IWebItem> _items;
    private ConcurrentDictionary<Guid, IWebItem> Items
    {
        get
        {
            if (_lazyItems.IsValueCreated)
            {
                return _items;
            }

            return _items = _lazyItems.Value;
        }
    }
    private readonly Lazy<ConcurrentDictionary<Guid, IWebItem>> _lazyItems;
    private readonly List<string> _disableItem;

    public static Guid CommunityProductID
    {
        get { return new Guid("{EA942538-E68E-4907-9394-035336EE0BA8}"); }
    }

    public static Guid ProjectsProductID
    {
        get { return new Guid("{1e044602-43b5-4d79-82f3-fd6208a11960}"); }
    }

    public static Guid CRMProductID
    {
        get { return new Guid("{6743007C-6F95-4d20-8C88-A8601CE5E76D}"); }
    }

    public static Guid DocumentsProductID
    {
        get { return new Guid("{E67BE73D-F9AE-4ce1-8FEC-1880CB518CB4}"); }
    }

    public static Guid PeopleProductID
    {
        get { return new Guid("{F4D98AFD-D336-4332-8778-3C6945C81EA0}"); }
    }

    public static Guid MailProductID
    {
        get { return new Guid("{2A923037-8B2D-487b-9A22-5AC0918ACF3F}"); }
    }

    public static Guid CalendarProductID
    {
        get { return new Guid("{32D24CB5-7ECE-4606-9C94-19216BA42086}"); }
    }

    public static Guid BirthdaysProductID
    {
        get { return new Guid("{37620AE5-C40B-45ce-855A-39DD7D76A1FA}"); }
    }

    public static Guid TalkProductID
    {
        get { return new Guid("{BF88953E-3C43-4850-A3FB-B1E43AD53A3E}"); }
    }

    private readonly IServiceProvider _serviceProvider;


    public IWebItem this[Guid id]
    {
        get
        {
            Items.TryGetValue(id, out var i);
            return i;
        }
    }

    public WebItemManager(IServiceProvider serviceProvider, IConfiguration configuration, ILoggerProvider options)
    {
        _serviceProvider = serviceProvider;
        _log = options.CreateLogger("ASC.Web");
        _disableItem = (configuration["web:disabled-items"] ?? "").Split(",").ToList();
        _lazyItems = new Lazy<ConcurrentDictionary<Guid, IWebItem>>(LoadItems, LazyThreadSafetyMode.ExecutionAndPublication);
    }

    private ConcurrentDictionary<Guid, IWebItem> LoadItems()
    {
        var result = new ConcurrentDictionary<Guid, IWebItem>();

        foreach (var webItem in _serviceProvider.GetServices<IWebItem>())
        {
            var file = webItem.ID.ToString();
            try
            {
                if (DisabledWebItem(file))
                {
                    continue;
                }

                RegistryItem(result, webItem);
            }
            catch (Exception exc)
            {
                _log.ErrorCouldntLoadWebItem(file, exc);
            }
        }

        return result;
    }

    private void RegistryItem(ConcurrentDictionary<Guid, IWebItem> result, IWebItem webItem)
    {
        if (webItem != null && !result.TryGetValue(webItem.ID, out _))
        {
            switch (webItem)
            {
                case IAddon addon:
                    addon.Init();
                    break;
                case IProduct product:
                    product.Init();
                    break;
                case IModule { Context.SearchHandler: not null }:
                    //TODO
                    //SearchHandlerManager.Registry(module.Context.SearchHandler);
                    break;
            }

            result.TryAdd(webItem.ID, webItem);
            _log.DebugWebItemLoaded(webItem.Name);
        }
    }

    public Guid GetParentItemId(Guid itemId)
    {
        return this[itemId] is IModule m ? m.ProjectId : Guid.Empty;
    }

    public int GetSortOrder(IWebItem item)
    {
        return item is { Context: not null } ? item.Context.DefaultSortOrder : 0;
    }

    public List<IWebItem> GetItemsAll()
    {
        var list = Items.Values.ToList();
        list.Sort((x, y) => GetSortOrder(x).CompareTo(GetSortOrder(y)));
        return list;
    }

    public List<T> GetItemsAll<T>() where T : IWebItem
    {
        return GetItemsAll().OfType<T>().ToList();
    }

    private bool DisabledWebItem(string name)
    {
        return _disableItem.Contains(name);
    }
}

[Scope]
public class WebItemManagerSecurity(WebItemSecurity webItemSecurity, AuthContext authContext, WebItemManager webItemManager)
{
    public List<IWebItem> GetItems(WebZoneType webZone, ItemAvailableState availableState =  ItemAvailableState.Normal)
    {
        var copy = webItemManager.GetItemsAll().ToList();
        var list = copy.Where(item =>
            {
                if ((availableState & ItemAvailableState.Disabled) != ItemAvailableState.Disabled && item.IsDisabledAsync(webItemSecurity, authContext).Result)
                {
                    return false;
                }
                var attribute = (WebZoneAttribute)Attribute.GetCustomAttribute(item.GetType(), typeof(WebZoneAttribute), true);
                return attribute != null && (attribute.Type & webZone) != 0;
            }).ToList();

        list.Sort((x, y) => webItemManager.GetSortOrder(x).CompareTo(webItemManager.GetSortOrder(y)));
        return list;
    }
}
