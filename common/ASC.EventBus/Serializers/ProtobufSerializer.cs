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

#nullable enable

using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ASC.EventBus.Serializers;

public class ProtobufSerializer : IIntegrationEventSerializer
{
    private readonly SynchronizedCollection<string> _processedProtoTypes;

    public ProtobufSerializer()
    {
        _processedProtoTypes = [];

        Array.ForEach(AppDomain.CurrentDomain.GetAssemblies(), BuildTypeModelFromAssembly);
    }

    private void BuildTypeModelFromAssembly(Assembly assembly)
    {
        var name = assembly.GetName().Name;
        if (name == null || !name.StartsWith("ASC."))
        {
            return;
        }

        var types = assembly.GetExportedTypes()
                  .Where(t => t.GetCustomAttributes<ProtoContractAttribute>().Any());

        foreach (var type in types)
        {
            ProcessProtoType(type);
        }
    }


    /// <inheritdoc/>
    public byte[] Serialize<T>(T? item)
    {
        if (item == null)
        {
            return [];
        }

        using var ms = new MemoryStream();

        Serializer.Serialize(ms, item);

        return ms.ToArray();
    }

    /// <inheritdoc/>
    public T Deserialize<T>(byte[] serializedObject)
    {
        using var ms = new MemoryStream(serializedObject);

        return Serializer.Deserialize<T>(ms);
    }

    /// <inheritdoc/>
    public object Deserialize(byte[] serializedObject, Type returnType)
    {
        using var ms = new MemoryStream(serializedObject);

        return Serializer.Deserialize(returnType, ms);
    }

    private void ProcessProtoType(Type protoType)
    {
        if (protoType.FullName == null || _processedProtoTypes.Contains(protoType.FullName))
        {
            return;
        }

        if (protoType.BaseType == null || protoType.BaseType == typeof(object))
        {
            return;
        }

        var itemType = RuntimeTypeModel.Default[protoType];

        var baseType = RuntimeTypeModel.Default[protoType.BaseType];

        if (baseType.GetSubtypes().All(s => s.DerivedType != itemType))
        {
            var hashed = MD5.HashData(Encoding.UTF8.GetBytes(protoType.FullName));
            var hashedAsInt32 = BitConverter.ToInt32(hashed, 0);

            // restriction: https://developers.google.com/protocol-buffers/docs/proto3
            var fieldNumber = (int)Math.Abs((hashedAsInt32 % (Math.Pow(2, 29) - 1)));

            baseType.AddSubType(fieldNumber, protoType);

            _processedProtoTypes.Add(protoType.FullName);
        }
    }

}
