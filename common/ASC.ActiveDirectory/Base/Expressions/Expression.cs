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

namespace ASC.ActiveDirectory.Base.Expressions;
public class Expression : ICloneable
{
    private readonly Op _op;
    private bool _negative;
    private readonly string _attributeName;
    private readonly string _attributeValue;

    private const string EQUIAL = "=";
    private const string APPROXIMATELY_EQUIAL = "~=";
    private const string GREATER = ">";
    private const string GREATER_OR_EQUAL = ">=";
    private const string LESS = "<";
    private const string LESS_OR_EQUAL = "<=";

    internal Expression()
    {
    }

    public string Name
    {
        get { return _attributeName; }
    }

    public string Value
    {
        get { return _attributeValue; }
    }

    public Op Operation
    {
        get { return _op; }
    }

    /// <summary>
    /// To specify unary operations
    /// </summary>
    /// <param name="op">Operator</param>
    /// <param name="attrbuteName">Attribute name</param>
    public Expression(string attrbuteName, Op op)
    {
        if (op != Op.Exists && op != Op.NotExists)
        {
            throw new ArgumentException("op");
        }

        if (string.IsNullOrEmpty(attrbuteName))
        {
            throw new ArgumentException("attrbuteName");
        }

        _op = op;
        _attributeName = attrbuteName;
        _attributeValue = "*";
    }

    /// <summary>
    /// To specify binary operations
    /// </summary>
    /// <param name="op">Operator</param>
    /// <param name="attrbuteName">Attribute name</param>
    /// <param name="attrbuteValue">Attribute value</param>
    public Expression(string attrbuteName, Op op, string attrbuteValue)
    {
        if (op is Op.Exists or Op.NotExists)
        {
            throw new ArgumentException("op");
        }

        if (string.IsNullOrEmpty(attrbuteName))
        {
            throw new ArgumentException("attrbuteName");
        }

        _op = op;
        _attributeName = attrbuteName;
        _attributeValue = attrbuteValue;
    }

    /// <summary>
    /// Expression as a string
    /// </summary>
    /// <returns>Expression string</returns>
    public override string ToString()
    {
        var sop = _op switch
        {
            Op.NotExists or Op.Exists or Op.Equal or Op.NotEqual => EQUIAL,
            Op.Greater => GREATER,
            Op.GreaterOrEqual => GREATER_OR_EQUAL,
            Op.Less => LESS,
            Op.LessOrEqual => LESS_OR_EQUAL,
            _ => throw new ArgumentOutOfRangeException()
        };

        var expressionString = "({0}{1}{2}{3})";
        expressionString = string.Format(expressionString,
            //positive or negative
            (((int)_op & 0x010000) == 0x010000 || _negative) ? "!" : "", _attributeName, sop,
            EscapeLdapSearchFilter(_attributeValue));

        return expressionString;
    }

    /// <summary>
    /// Escapes the LDAP search filter to prevent LDAP injection attacks.
    /// </summary>
    /// <param name="searchFilter">The search filter.</param>
    /// <returns>The escaped search filter.</returns>
    private static string EscapeLdapSearchFilter(string searchFilter)
    {
        var escape = new StringBuilder(); // If using JDK >= 1.5 consider using StringBuilder
        foreach (var current in searchFilter)
        {
            switch (current)
            {
                case '\\':
                    escape.Append(@"\5c");
                    break;
                case '*':
                    escape.Append(@"\2a");
                    break;
                case '(':
                    escape.Append(@"\28");
                    break;
                case ')':
                    escape.Append(@"\29");
                    break;
                case '\u0000':
                    escape.Append(@"\00");
                    break;
                case '/':
                    escape.Append(@"\2f");
                    break;
                default:
                    escape.Append(current);
                    break;
            }
        }

        return escape.ToString();
    }

    /// <summary>
    /// Negation
    /// </summary>
    /// <returns>Self</returns>
    public Expression Negative()
    {
        _negative = !_negative;
        return this;
    }

    /// <summary>
    /// Existence
    /// </summary>
    /// <param name="attrbuteName"></param>
    /// <returns>New Expression</returns>
    public static Expression Exists(string attrbuteName)
    {
        return new Expression(attrbuteName, Op.Exists);
    }

    /// <summary>
    /// Non-Existence
    /// </summary>
    /// <param name="attrbuteName"></param>
    /// <returns>New Expression</returns>
    public static Expression NotExists(string attrbuteName)
    {
        return new Expression(attrbuteName, Op.NotExists);
    }

    /// <summary>
    /// Equality
    /// </summary>
    /// <param name="attrbuteName"></param>
    /// <param name="attrbuteValue"></param>
    /// <returns>New Expression</returns>
    public static Expression Equal(string attrbuteName, string attrbuteValue)
    {
        return new Expression(attrbuteName, Op.Equal, attrbuteValue);
    }

    /// <summary>
    /// Not equality
    /// </summary>
    /// <param name="attrbuteName"></param>
    /// <param name="attrbuteValue"></param>
    /// <returns></returns>
    public static Expression NotEqual(string attrbuteName, string attrbuteValue)
    {
        return new Expression(attrbuteName, Op.NotEqual, attrbuteValue);
    }

    public static Expression Parse(string origin)
    {
        string spliter = null;
        var op = Op.Equal;

        var index = origin.IndexOf(EQUIAL, StringComparison.Ordinal);

        if (index > -1)
        {
            spliter = EQUIAL;
            op = Op.Equal;
        }
        else if ((index = origin.IndexOf(GREATER, StringComparison.Ordinal)) > -1)
        {
            spliter = GREATER;
            op = Op.Greater;
        }
        else if ((index = origin.IndexOf(GREATER_OR_EQUAL, StringComparison.Ordinal)) > -1)
        {
            spliter = GREATER_OR_EQUAL;
            op = Op.GreaterOrEqual;
        }
        else if ((index = origin.IndexOf(LESS, StringComparison.Ordinal)) > -1)
        {
            spliter = LESS;
            op = Op.Less;
        }
        else if ((index = origin.IndexOf(LESS_OR_EQUAL, StringComparison.Ordinal)) > -1)
        {
            spliter = LESS_OR_EQUAL;
            op = Op.LessOrEqual;
        }
        else if ((index = origin.IndexOf(APPROXIMATELY_EQUIAL, StringComparison.Ordinal)) > -1)
        {
            spliter = APPROXIMATELY_EQUIAL;
            op = Op.Exists;
        }

        if (string.IsNullOrEmpty(spliter))
        {
            return null;
        }

        var attributeName = origin[..index];
        var attributeValue = origin[(index + 1)..];

        if (string.IsNullOrEmpty(attributeName) || string.IsNullOrEmpty(attributeValue))
        {
            return null;
        }

        return new Expression(attributeName, op, attributeValue);
    }

    #region ICloneable Members
    /// <summary>
    /// ICloneable implemetation
    /// </summary>
    /// <returns>Clone object</returns>
    public object Clone()
    {
        return MemberwiseClone();
    }

    #endregion
}