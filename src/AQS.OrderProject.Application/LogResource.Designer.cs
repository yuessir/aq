//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AQS.OrderProject.Application {
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
    public class LogResource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal LogResource() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("AQS.OrderProject.Application.LogResource", typeof(LogResource).Assembly);
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
        ///   Looks up a localized string similar to [PlaceOrder] Prepare to process place order request, user: {0}, company: {1}, orderId: {2}, agentOrderId: {3}.
        /// </summary>
        public static string V2POCH01 {
            get {
                return ResourceManager.GetString("V2POCH01", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PlaceOrder] Duplicated orderId: {0}, CreatedTime {1}, agentOrderId: {2}.
        /// </summary>
        public static string V2POCH02 {
            get {
                return ResourceManager.GetString("V2POCH02", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PlaceOrder] OrderId: {0} Cannot find reference match with HG: {1}, SBO: {2}, IBC: {3}.
        /// </summary>
        public static string V2POCH03 {
            get {
                return ResourceManager.GetString("V2POCH03", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PlaceOrder] Duplicated task, orderId: {0}, matchMapId: {1}, marketType: {2}, agentOrderId: {3}.
        /// </summary>
        public static string V2POCH04 {
            get {
                return ResourceManager.GetString("V2POCH04", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PlaceOrder] OrderId: {0} insert order done, result: {1}.
        /// </summary>
        public static string V2POCH05 {
            get {
                return ResourceManager.GetString("V2POCH05", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PlaceOrder] OrderId: {0} insert task to redis done, result: {1}.
        /// </summary>
        public static string V2POCH06 {
            get {
                return ResourceManager.GetString("V2POCH06", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PlaceOrder] OrderId: {0} process place order request done, response: {1}.
        /// </summary>
        public static string V2POCH07 {
            get {
                return ResourceManager.GetString("V2POCH07", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PlaceOrder] OrderId: {0} insert UK not mapping order done.
        /// </summary>
        public static string V2POCH08 {
            get {
                return ResourceManager.GetString("V2POCH08", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PlaceOrder] OrderId: {0} exception raised while trying to insert UK not mapping order, msg: {1}.
        /// </summary>
        public static string V2POCH09 {
            get {
                return ResourceManager.GetString("V2POCH09", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to [PlaceOrder] OrderId: {0} insert task done, result: {1}.
        /// </summary>
        public static string V2TP01 {
            get {
                return ResourceManager.GetString("V2TP01", resourceCulture);
            }
        }
    }
}
