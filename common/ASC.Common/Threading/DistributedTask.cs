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

namespace ASC.Common.Threading;

/// <summary>
/// </summary>
[JsonPolymorphic(UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor)]
[JsonDerivedType(typeof(DistributedTaskProgress))]
public class DistributedTask
{
    [JsonInclude]
    protected string _exeption = String.Empty;
    
    [JsonIgnore]
    public Func<DistributedTask, Task> Publication { get; set; }

    /// <summary>Instance ID</summary>
    /// <type>System.Int32, System</type>
    public int InstanceId { get; set; }

    /// <summary>ID</summary>
    /// <type>System.String, System</type>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <summary>Status</summary>
    /// <type>ASC.Common.Threading.DistributedTaskStatus, ASC.Common</type>
    public DistributedTaskStatus Status { get; set; }

    /// <summary>Last modified date</summary>
    /// <type>System.DateTime, System</type>
    public DateTime LastModifiedOn { get; set; }

    /// <summary>Exception</summary>
    /// <type>System.Object, System</type>
    [JsonIgnore]
    public Exception Exception
    {
        get => new(_exeption);
        set => _exeption = value?.Message ?? "";
    }

    protected CancellationToken CancellationToken { get; set; }

    public virtual async Task RunJob(CancellationToken cancellationToken)
    {
        Status = DistributedTaskStatus.Running;
        CancellationToken = cancellationToken;

        await DoJob();
    }

    protected virtual Task DoJob() { return Task.CompletedTask; }

    public async Task PublishChanges()
    {
        if (Publication == null)
        {
            throw new InvalidOperationException("Publication not found.");
        }

        await Publication(this);
    }
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}