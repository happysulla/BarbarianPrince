﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace BarbarianPrince.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.11.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("1")]
        public double ZoomCanvas {
            get {
                return ((double)(this["ZoomCanvas"]));
            }
            set {
                this["ZoomCanvas"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double ScrollViewerHeight {
            get {
                return ((double)(this["ScrollViewerHeight"]));
            }
            set {
                this["ScrollViewerHeight"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public double ScrollViewerWidth {
            get {
                return ((double)(this["ScrollViewerWidth"]));
            }
            set {
                this["ScrollViewerWidth"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string GameOptions {
            get {
                return ((string)(this["GameOptions"]));
            }
            set {
                this["GameOptions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string GameDirectoryName {
            get {
                return ((string)(this["GameDirectoryName"]));
            }
            set {
                this["GameDirectoryName"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute(@"<?xml version=""1.0"" encoding=""utf-16""?><WindowPlacement xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><length>44</length><flags>2</flags><showCmd>3</showCmd><minPosition><X>-1</X><Y>-1</Y></minPosition><maxPosition><X>-1</X><Y>-1</Y></maxPosition><normalPosition><Left>260</Left><Top>260</Top><Right>2180</Right><Bottom>1340</Bottom></normalPosition></WindowPlacement>
")]
        public string WindowPlacement {
            get {
                return ((string)(this["WindowPlacement"]));
            }
            set {
                this["WindowPlacement"] = value;
            }
        }
    }
}