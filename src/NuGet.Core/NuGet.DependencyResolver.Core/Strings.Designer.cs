﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NuGet.DependencyResolver {
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
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NuGet.DependencyResolver.Strings", typeof(Strings).Assembly);
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
        ///   Looks up a localized string similar to The feed &apos;{0}&apos; lists package &apos;{1}&apos; but multiple attempts to download the nupkg have failed. The feed is either invalid or required packages were removed while the current operation was in progress. Verify the package exists on the feed and try again..
        /// </summary>
        internal static string Error_PackageNotFoundWhenExpected {
            get {
                return ResourceManager.GetString("Error_PackageNotFoundWhenExpected", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package namespace matches found for package ID &apos;{0}&apos; are: &apos;{1}&apos;.
        /// </summary>
        internal static string Log_MatchingSourceFoundForPackage {
            get {
                return ResourceManager.GetString("Log_MatchingSourceFoundForPackage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Package Namespaces are configured but no matching source found for &apos;{0}&apos; package..
        /// </summary>
        internal static string Log_NoMatchingSourceFoundForPackage {
            get {
                return ResourceManager.GetString("Log_NoMatchingSourceFoundForPackage", resourceCulture);
            }
        }
    }
}
