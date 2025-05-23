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
// All the Product's GUI elements, including illustrations and icon sets, as well as technical
// writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode

package com.asc.registration.service.transfer.response;

import com.fasterxml.jackson.annotation.JsonProperty;
import java.io.Serializable;
import java.time.ZonedDateTime;
import java.util.Set;
import lombok.*;

/**
 * ClientInfoResponse is a Data Transfer Object (DTO) used to transfer client information in
 * responses.
 */
@Getter
@Setter
@Builder
@NoArgsConstructor
@AllArgsConstructor
public class ClientInfoResponse implements Serializable {

  /** The name of the client. */
  private String name;

  /** The unique identifier of the client. */
  @JsonProperty("client_id")
  private String clientId;

  /** The description of the client. */
  private String description;

  /** The website URL of the client. */
  @JsonProperty("website_url")
  private String websiteUrl;

  /** The terms of service URL of the client. */
  @JsonProperty("terms_url")
  private String termsUrl;

  /** The privacy policy URL of the client. */
  @JsonProperty("policy_url")
  private String policyUrl;

  /** The logo of the client. */
  @JsonProperty("logo")
  private String logo;

  /** The authentication methods supported by the client. */
  @JsonProperty("authentication_methods")
  private Set<String> authenticationMethods;

  /** The scopes assigned to the client. */
  private Set<String> scopes;

  /** Indicates if the client is public. */
  @JsonProperty("is_public")
  private boolean isPublic;

  /** The date and time when the client was created. */
  @JsonProperty("created_on")
  private ZonedDateTime createdOn;

  /** The user who created the client. */
  @JsonProperty("created_by")
  private String createdBy;

  /** The date and time when the client was last modified. */
  @JsonProperty("modified_on")
  private ZonedDateTime modifiedOn;

  /** The user who last modified the client. */
  @JsonProperty("modified_by")
  private String modifiedBy;
}
