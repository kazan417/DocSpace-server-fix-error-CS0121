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

namespace ASC.Web.Core.Utility.Settings;

[Singleton]
public class CustomColorThemesSettingsHelper(IConfiguration configuration)
{
    public int Limit { get; set; } = configuration.GetSection("core:themelimit").Get<int>();
}

public class CustomColorThemesSettings : ISettings<CustomColorThemesSettings>
{
    public List<CustomColorThemesSettingsItem> Themes { get; set; }
    public int Selected { get; set; }

    public CustomColorThemesSettings GetDefault()
    {
        Themes = CustomColorThemesSettingsItem.Default;

        return new CustomColorThemesSettings
        {
            Themes = Themes,
            Selected = Themes.Min(r => r.Id)
        };
    }

    public DateTime LastModified { get; set; }

    [JsonIgnore]
    public Guid ID
    {
        get { return new Guid("{FE096AE9-3044-4B92-A94C-46E323F82681}"); }
    }
}

/// <summary>
/// The custom color theme settings.
/// </summary>
public class CustomColorThemesSettingsItem
{
    /// <summary>
    /// The custom color theme ID.
    /// </summary>
    [SwaggerSchemaCustom(Example = 1)]
    public int Id { get; set; }

    /// <summary>
    /// The custom color theme name.
    /// </summary>
    [SwaggerSchemaCustom(Example = "blue")]
    public string Name { get; set; }

    /// <summary>
    /// The custom color theme main colors.
    /// </summary>
    public CustomColorThemesSettingsColorItem Main { get; set; }

    /// <summary>
    /// The custom color theme text colors.
    /// </summary>
    public CustomColorThemesSettingsColorItem Text { get; set; }

    public static List<CustomColorThemesSettingsItem> Default =>
    [
        new()
        {
            Id = 1,
            Name = "blue",
            Main = new CustomColorThemesSettingsColorItem { Accent = "#4781D1", Buttons = "#5299E0" },
            Text = new CustomColorThemesSettingsColorItem { Accent = "#FFFFFF", Buttons = "#FFFFFF" }
        },

        new()
        {
            Id = 2,
            Name = "orange",
            Main = new CustomColorThemesSettingsColorItem { Accent = "#F97A0B", Buttons = "#FF9933" },
            Text = new CustomColorThemesSettingsColorItem { Accent = "#FFFFFF", Buttons = "#FFFFFF" }
        },

        new()
        {
            Id = 3,
            Name = "green",
            Main = new CustomColorThemesSettingsColorItem { Accent = "#2DB482", Buttons = "#22C386" },
            Text = new CustomColorThemesSettingsColorItem { Accent = "#FFFFFF", Buttons = "#FFFFFF" }
        },

        new()
        {
            Id = 4,
            Name = "red",
            Main = new CustomColorThemesSettingsColorItem { Accent = "#F2675A", Buttons = "#F27564" },
            Text = new CustomColorThemesSettingsColorItem { Accent = "#FFFFFF", Buttons = "#FFFFFF" }
        },

        new()
        {
            Id = 5,
            Name = "purple",
            Main = new CustomColorThemesSettingsColorItem { Accent = "#6D4EC2", Buttons = "#8570BD" },
            Text = new CustomColorThemesSettingsColorItem { Accent = "#FFFFFF", Buttons = "#FFFFFF" }
        },

        new()
        {
            Id = 6,
            Name = "light-blue",
            Main = new CustomColorThemesSettingsColorItem { Accent = "#11A4D4", Buttons = "#13B7EC" },
            Text = new CustomColorThemesSettingsColorItem { Accent = "#FFFFFF", Buttons = "#FFFFFF" }
        }
    ];
}

/// <summary>
/// The custom color theme color parameters.
/// </summary>
public class CustomColorThemesSettingsColorItem
{
    /// <summary>
    /// The accent color.
    /// </summary>
    [SwaggerSchemaCustom(Example = "#4781D1")]
    public string Accent { get; init; }
    
    /// <summary>
    /// The button color.
    /// </summary>
    [SwaggerSchemaCustom(Example = "#5299E0")]
    public string Buttons { get; init; }
}