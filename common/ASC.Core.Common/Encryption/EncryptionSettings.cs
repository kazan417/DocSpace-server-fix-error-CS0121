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

namespace ASC.Core.Encryption;

/// <summary>
/// The encryption settings.
/// </summary>
[ProtoContract]
public class EncryptionSettings
{
    [ProtoMember(1)]
    internal string Pass;

    public EncryptionSettings()
    {
        Password = string.Empty;
        Status = EncryprtionStatus.Decrypted;
        NotifyUsers = true;
    }

    /// <summary>
    /// The encryption password.
    /// </summary>
    public string Password
    {
        get => Pass;
        set => Pass = (value ?? string.Empty).Replace('#', '_');
    }

    /// <summary>
    /// The encryption status.
    /// </summary>
    [ProtoMember(2)]
    public EncryprtionStatus Status { get; set; }

    /// <summary>
    /// Specifies if the users will be notified about the encryption operation or not.
    /// </summary>
    [ProtoMember(3)]
    public bool NotifyUsers { get; set; }

}

[Scope]
public class EncryptionSettingsHelper(CoreConfiguration coreConfiguration, InstanceCrypto instanceCrypto)
{
    private const string Key = "EncryptionSettings";

    public async Task SaveAsync(EncryptionSettings encryptionSettings)
    {
        var settings = await SerializeAsync(encryptionSettings);
        await coreConfiguration.SaveSettingAsync(Key, settings);
    }

    public async Task<EncryptionSettings> LoadAsync()
    {
        var settings = await coreConfiguration.GetSettingAsync(Key);

        return await DeserializeAsync(settings);
    }

    private async Task<string> SerializeAsync(EncryptionSettings encryptionSettings)
    {
        return string.Join("#",
            string.IsNullOrEmpty(encryptionSettings.Pass) ? 
                string.Empty : 
                await instanceCrypto.EncryptAsync(encryptionSettings.Pass),
            (int)encryptionSettings.Status,
            encryptionSettings.NotifyUsers
        );
    }

    public EncryptionSettings Deserialize(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new EncryptionSettings();
        }

        var parts = value.Split('#');

        var password = string.IsNullOrEmpty(parts[0]) ? string.Empty : instanceCrypto.Decrypt(parts[0]);
        var status = int.Parse(parts[1]);
        var notifyUsers = bool.Parse(parts[2]);

        return new EncryptionSettings
        {
            Password = password,
            Status = (EncryprtionStatus)status,
            NotifyUsers = notifyUsers
        };
    }

    public async Task<EncryptionSettings> DeserializeAsync(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return new EncryptionSettings();
        }

        var parts = value.Split('#');

        var password = string.IsNullOrEmpty(parts[0]) ? string.Empty : await instanceCrypto.DecryptAsync(parts[0]);
        var status = int.Parse(parts[1]);
        var notifyUsers = bool.Parse(parts[2]);

        return new EncryptionSettings
        {
            Password = password,
            Status = (EncryprtionStatus)status,
            NotifyUsers = notifyUsers
        };
    }

    // source System.Web.Security.Membership.GeneratePassword
    public string GeneratePassword(int length, int numberOfNonAlphanumericCharacters)
    {
        var punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();

        if (length is < 1 or > 128)
        {
            throw new ArgumentException("password_length_incorrect", nameof(length));
        }

        if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
        {
            throw new ArgumentException("min_required_non_alphanumeric_characters_incorrect", nameof(numberOfNonAlphanumericCharacters));
        }

        var array2 = new char[length];
        var num = 0;

        var array = RandomNumberGenerator.GetBytes(length);

        for (var i = 0; i < length; i++)
        {
            var num2 = array[i] % 87;
            if (num2 < 10)
            {
                array2[i] = (char)(48 + num2);
                continue;
            }

            if (num2 < 36)
            {
                array2[i] = (char)(65 + num2 - 10);
                continue;
            }

            if (num2 < 62)
            {
                array2[i] = (char)(97 + num2 - 36);
                continue;
            }

            array2[i] = punctuations[num2 - 62];
            num++;
        }

        if (num < numberOfNonAlphanumericCharacters)
        {
            for (var j = 0; j < numberOfNonAlphanumericCharacters - num; j++)
            {
                int num3;
                do
                {
                    num3 = RandomNumberGenerator.GetInt32(0, length);
                }
                while (!char.IsLetterOrDigit(array2[num3]));
                array2[num3] = punctuations[RandomNumberGenerator.GetInt32(0, punctuations.Length)];
            }
        }

        return new string(array2);
    }
}
