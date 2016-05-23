﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace EdFi.Ods.Api.Services.Metadata {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Other {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Other() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("EdFi.Ods.Api.Services.Metadata.Other", typeof(Other).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;apiVersion&quot;: &quot;1.0.0.0&quot;,
        ///  &quot;swaggerVersion&quot;: &quot;1.2&quot;,
        ///  &quot;basePath&quot;: &quot;%BASE_URL%/metadata/other/api-docs&quot;,
        ///  &quot;apis&quot;: [
        ///    {
        ///      &quot;path&quot;: &quot;/bulkOperations&quot;,
        ///      &quot;description&quot;: &quot;Manage bulk operation sessions&quot;
        ///    },
        ///    {
        ///      &quot;path&quot;: &quot;/bulkOperationsExceptions&quot;,
        ///      &quot;description&quot;: &quot;Retrieve bulk operations exceptions&quot;
        ///    },
        ///    {
        ///      &quot;path&quot;: &quot;/identities&quot;,
        ///      &quot;description&quot;: &quot;Retrieve or create Unique Ids for a person, and add or update their information&quot;
        ///    },
        ///    {
        ///      &quot;path&quot;: &quot;/schoolIden [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string other_api_docs_json {
            get {
                return ResourceManager.GetString("other.api-docs.json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///{
        ///  &quot;apiVersion&quot;: &quot;1.0.0.0&quot;,
        ///  &quot;swaggerVersion&quot;: &quot;1.2&quot;,
        ///  &quot;basePath&quot;: &quot;%BASE_URL%/api/v2.0&quot;,
        ///  &quot;resourcePath&quot;: &quot;/bulkoperations&quot;,
        ///  &quot;produces&quot;: [
        ///    &quot;application/json&quot;
        ///  ],
        ///  &quot;apis&quot;: [
        ///    {
        ///      &quot;path&quot;: &quot;/bulkOperations&quot;,
        ///      &quot;description&quot;: &quot;Manage bulk operation sessions&quot;,
        ///      &quot;operations&quot;: [
        ///        {
        ///          &quot;method&quot;: &quot;POST&quot;,
        ///          &quot;nickname&quot;: &quot;post&quot;,
        ///          &quot;notes&quot;: &quot;This creates a session, during which XML interchange files are uploaded, committed, and processed. An Operation Identifie [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string other_bulkOperations_json {
            get {
                return ResourceManager.GetString("other.bulkOperations.json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///{
        ///  &quot;apiVersion&quot;: &quot;1.0.0.0&quot;,
        ///  &quot;swaggerVersion&quot;: &quot;1.2&quot;,
        ///  &quot;basePath&quot;: &quot;%BASE_URL%/api/v2.0&quot;,
        ///  &quot;resourcePath&quot;: &quot;/bulkoperationsexceptions&quot;,
        ///  &quot;produces&quot;: [
        ///    &quot;application/json&quot;
        ///  ],
        ///  &quot;apis&quot;: [
        ///    {
        ///      &quot;path&quot;: &quot;/bulkoperations/{id}/exceptions/{uploadid}&quot;,
        ///      &quot;description&quot;: &quot;Retrieve bulk operations exceptions&quot;,
        ///      &quot;operations&quot;: [
        ///        {
        ///          &quot;method&quot;: &quot;GET&quot;,
        ///          &quot;nickname&quot;: &quot;get&quot;,
        ///          &quot;summary&quot;: &quot;Retrieves collection of exceptions from a bulk operation.&quot;,
        ///          &quot;type&quot;: &quot; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string other_bulkOperationsExceptions_json {
            get {
                return ResourceManager.GetString("other.bulkOperationsExceptions.json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to {
        ///  &quot;apiVersion&quot;: &quot;1.0.0.0&quot;,
        ///  &quot;swaggerVersion&quot;: &quot;1.2&quot;,
        ///  &quot;basePath&quot;: &quot;%BASE_URL%/api/v2.0&quot;,
        ///  &quot;resourcePath&quot;: &quot;/identities&quot;,
        ///  &quot;produces&quot;: [
        ///    &quot;application/json&quot;
        ///  ],
        ///  &quot;apis&quot;: [
        ///    {
        ///      &quot;path&quot;: &quot;/identities/{id}&quot;,
        ///      &quot;description&quot;: &quot;Retrieve or create Unique Ids for a Identity, and add or update their information&quot;,
        ///      &quot;operations&quot;: [
        ///        {
        ///          &quot;method&quot;: &quot;GET&quot;,
        ///          &quot;nickname&quot;: &quot;getById&quot;,
        ///          &quot;summary&quot;: &quot;Retrieve a single person record from their Unique Id.&quot; [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string other_identities_json {
            get {
                return ResourceManager.GetString("other.identities.json", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///{
        ///  &quot;apiVersion&quot;: &quot;1.0.0.0&quot;,
        ///  &quot;swaggerVersion&quot;: &quot;1.2&quot;,
        ///  &quot;basePath&quot;: &quot;%BASE_URL%/api/v2.0&quot;,
        ///  &quot;resourcePath&quot;: &quot;/uploads&quot;,
        ///  &quot;produces&quot;: [
        ///    &quot;application/json&quot;
        ///  ],
        ///  &quot;apis&quot;: [
        ///    {
        ///      &quot;path&quot;: &quot;/uploads/{uploadid}/chunk&quot;,
        ///      &quot;description&quot;: &quot;Upload interchange XML files&quot;,
        ///      &quot;operations&quot;: [
        ///        {
        ///          &quot;method&quot;: &quot;POST&quot;,
        ///          &quot;nickname&quot;: &quot;postChunk&quot;,
        ///          &quot;summary&quot;: &quot;Allows for the upload of files parts of a larger upload file.&quot;,
        ///          &quot;type&quot;: &quot;upload&quot;,
        ///          &quot;responseCl [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string other_uploads_json {
            get {
                return ResourceManager.GetString("other.uploads.json", resourceCulture);
            }
        }
    }
}
