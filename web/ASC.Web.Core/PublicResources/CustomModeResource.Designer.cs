﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ASC.Web.Core.PublicResources {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class CustomModeResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal CustomModeResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ASC.Web.Core.PublicResources.CustomModeResource", typeof(CustomModeResource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Hello, $UserName!
        ///
        ///Welcome to ${LetterLogoText} DocSpace! Your user profile has been successfully added to &quot;${__VirtualRootPath}&quot;:&quot;${__VirtualRootPath}&quot;. Now you can:
        ///
        ///* Work with other users in the room you are invited to: *collaboration rooms for real-time co-authoring or custom rooms with flexible settings for any purpose*.
        ///
        ///* *Work with files of different formats*: text documents, spreadsheets, presentations, digital forms, PDFs, e-books, multimedia.
        ///
        ///* *Collaborate on documents* with two co-edi [rest of string was truncated]&quot;;.
        /// </summary>
        public static string pattern_enterprise_whitelabel_user_welcome_custom_mode_v1 {
            get {
                return ResourceManager.GetString("pattern_enterprise_whitelabel_user_welcome_custom_mode_v1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Dear $UserName,
        ///
        ///The process of removing data from user &quot;$FromUserName&quot;:&quot;$FromUserLink&quot; has been successfully completed.
        ///
        ///The deletion of personal data allowed to free $DocsSpace.
        /// </summary>
        public static string pattern_remove_user_data_completed_custom_mode {
            get {
                return ResourceManager.GetString("pattern_remove_user_data_completed_custom_mode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to h3.New portal has been registered
        ///
        ///# Portal URL: &quot;${__VirtualRootPath}&quot;:&quot;${__VirtualRootPath}&quot;
        ///# First name: $UserName
        ///# Last name: $UserLastName
        ///# Email: $UserEmail
        ///# Phone: $Phone
        ///# Creation date: $Date.
        /// </summary>
        public static string pattern_saas_custom_mode_reg_data {
            get {
                return ResourceManager.GetString("pattern_saas_custom_mode_reg_data", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Do the same as a user|Link Dropbox, Box and other accounts in the &apos;Common&apos; section|Set up access rights to the documents and folders in the &apos;Common&apos; section.
        /// </summary>
        public static string ProductAdminOpportunitiesCustomMode {
            get {
                return ResourceManager.GetString("ProductAdminOpportunitiesCustomMode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Create and edit own documents as well as the shared documents with the proper access rights|Give reading/editing access to other users to the documents and folders|Link Dropbox, Box and other accounts in the &apos;My documents&apos; section.
        /// </summary>
        public static string ProductUserOpportunitiesCustomMode {
            get {
                return ResourceManager.GetString("ProductUserOpportunitiesCustomMode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ${LetterLogoText}. Remove user data is completed.
        /// </summary>
        public static string subject_remove_user_data_completed_custom_mode {
            get {
                return ResourceManager.GetString("subject_remove_user_data_completed_custom_mode", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to New portal has been registered.
        /// </summary>
        public static string subject_saas_custom_mode_reg_data {
            get {
                return ResourceManager.GetString("subject_saas_custom_mode_reg_data", resourceCulture);
            }
        }
    }
}
