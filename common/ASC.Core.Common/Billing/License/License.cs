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

namespace ASC.Core.Billing;

/// <summary>
/// The license parameters.
/// </summary>
[DebuggerDisplay("{DueDate}")]
public class License
{
    /// <summary>
    /// The original license.
    /// </summary>
    public string OriginalLicense { get; set; }

    /// <summary>
    /// Specifies if the license supports branding or not.
    /// </summary>
    public bool Branding { get; set; }

    /// <summary>
    /// Specifies if the license is customizable or not.
    /// </summary>
    public bool Customization { get; set; }

    /// <summary>
    /// Specifies if the license is time limited or not.
    /// </summary>
    public bool TimeLimited { get; set; }

    /// <summary>
    /// The license due date.
    /// </summary>
    [JsonPropertyName("end_date")]
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Specifies if the license is trial or not.
    /// </summary>
    public bool Trial { get; set; }

    /// <summary>
    /// The customer ID.
    /// </summary>
    [JsonPropertyName("customer_id")]
    public string CustomerId { get; set; }

    /// <summary>
    /// The resource key.
    /// </summary>
    [JsonPropertyName("resource_key")]
    public string ResourceKey { get; set; }

    /// <summary>
    /// The number of ONLYOFFICCE Docs users.
    /// </summary>
    [JsonPropertyName("users_count")]
    public int DSUsersCount { get; set; }

    /// <summary>
    /// The number of users whose licenses have expired.
    /// </summary>
    [JsonPropertyName("users_expire")]
    public int DSUsersExpire { get; set; }

    /// <summary>
    /// The number of ONLYOFFICCE Docs connections.
    /// </summary>
    [JsonPropertyName("connections")]
    public int DSConnections { get; set; }

    /// <summary>
    /// The license signature.
    /// </summary>
    [JsonPropertyName("signature")]
    public string Signature { get; set; }

    /// <summary>
    /// Indicates whether the license is Developer or not.
    /// </summary>
    [JsonPropertyName("docspace_dev")] 
    public bool Developer { get; set; }

    public static License Parse(string licenseString)
    {
        if (string.IsNullOrEmpty(licenseString))
        {
            throw new BillingNotFoundException("License file is empty");
        }

        try
        {
            var options = new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };

            options.Converters.Add(new LicenseConverter());

            var license = JsonSerializer.Deserialize<License>(licenseString, options);

            if (license == null)
            {
                throw new BillingNotFoundException("Can't parse license");
            }

            license.OriginalLicense = licenseString;

            return license;
        }
        catch (Exception)
        {
            throw new BillingNotFoundException("Can't parse license");
        }
    }
}

public class LicenseConverter : JsonConverter<object>
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(int) == typeToConvert ||
               typeof(bool) == typeToConvert;
    }

    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (typeToConvert == typeof(int))
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var i = reader.GetString();
                if (!int.TryParse(i, out var result))
                {
                    return 0;
                }
                return result;
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return reader.GetInt32();
            }


        }

        if (typeToConvert == typeof(bool))
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var i = reader.GetString();
                if (!bool.TryParse(i, out var result))
                {
                    return false;
                }

                return result;
            }

            return reader.GetBoolean();
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
    }
}
