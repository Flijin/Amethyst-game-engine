﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace UnitTests {
    using System;
    
    
    /// <summary>
    ///   Класс ресурса со строгой типизацией для поиска локализованных строк и т.д.
    /// </summary>
    // Этот класс создан автоматически классом StronglyTypedResourceBuilder
    // с помощью такого средства, как ResGen или Visual Studio.
    // Чтобы добавить или удалить член, измените файл .ResX и снова запустите ResGen
    // с параметром /str или перестройте свой проект VS.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Возвращает кэшированный экземпляр ResourceManager, использованный этим классом.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("UnitTests.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Перезаписывает свойство CurrentUICulture текущего потока для всех
        ///   обращений к ресурсу с помощью этого класса ресурса со строгой типизацией.
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
        ///   Ищет локализованную строку, похожую на  {
        ///  &quot;Key1&quot; : &quot;Value1&quot;,
        ///  &quot;Key2&quot; : {},
        ///  &quot;Key3&quot; : [],
        ///  &quot;Key4&quot; : true,
        ///  &quot;Key5&quot; : false,
        ///  &quot;Key6&quot;  :  null  ,  
        ///&quot;Key7&quot;:&quot;Value7&quot;,
        ///	&quot;Key8&quot;	:	&quot;Value8&quot;	,	
        ///&quot;Key9&quot;
        ///:
        ///&quot;Value9&quot;
        ///,
        ///  &quot;Key10&quot; : 10,
        ///  &quot;Key11&quot; : -10,
        ///  &quot;Key12&quot; : 1.0,
        ///  &quot;Key13&quot; : -1.0,
        ///  &quot;Key14&quot; : -1.0e-05,
        ///  &quot;Key15&quot; :  1.0e-5,
        ///  &quot;Key16&quot; : -1.0e05,
        ///  &quot;Key17&quot; : -1.0e5,
        ///  &quot;Key18&quot; : -1.0e+05,
        ///  &quot;Key19&quot; : -1.0e+5,
        ///  &quot;Key20&quot; : -1.0E+5,
        ///  &quot;Key21&quot; : [1, null,true	, false,
        ///{
        ///  &quot;Key001&quot; : &quot;Value001&quot;,
        ///  &quot;Key002&quot; : [1, 2,]
        ///}],
        ///} .
        /// </summary>
        internal static string Correct_file {
            get {
                return ResourceManager.GetString("Correct file", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///,
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_1 {
            get {
                return ResourceManager.GetString("Incorrect file case 1", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; :: &quot;value&quot;
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_10 {
            get {
                return ResourceManager.GetString("Incorrect file case 10", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : &quot;value&quot;,,
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_11 {
            get {
                return ResourceManager.GetString("Incorrect file case 11", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : &quot;value&quot;
        ///.
        /// </summary>
        internal static string Incorrect_file_case_12 {
            get {
                return ResourceManager.GetString("Incorrect file case 12", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	A&quot;key&quot; : &quot;value&quot;
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_13 {
            get {
                return ResourceManager.GetString("Incorrect file case 13", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot;A : &quot;value&quot;
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_14 {
            get {
                return ResourceManager.GetString("Incorrect file case 14", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : A&quot;value&quot;
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_15 {
            get {
                return ResourceManager.GetString("Incorrect file case 15", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : &quot;value&quot;A
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_16 {
            get {
                return ResourceManager.GetString("Incorrect file case 16", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на A{
        ///	&quot;key&quot; : &quot;value&quot;
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_17 {
            get {
                return ResourceManager.GetString("Incorrect file case 17", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : &quot;value&quot;
        ///}A.
        /// </summary>
        internal static string Incorrect_file_case_18 {
            get {
                return ResourceManager.GetString("Incorrect file case 18", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : [, 1]
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_19 {
            get {
                return ResourceManager.GetString("Incorrect file case 19", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	key : &quot;value&quot;
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_2 {
            get {
                return ResourceManager.GetString("Incorrect file case 2", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : [1 ,, 1]
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_20 {
            get {
                return ResourceManager.GetString("Incorrect file case 20", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : [1, 1,,]
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_21 {
            get {
                return ResourceManager.GetString("Incorrect file case 21", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : &quot;value&quot;,
        ///	&quot;key&quot; : &quot;value&quot;,
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_22 {
            get {
                return ResourceManager.GetString("Incorrect file case 22", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : &quot;value&quot;,
        ///	&quot;key&quot; : &quot;value&quot;
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_23 {
            get {
                return ResourceManager.GetString("Incorrect file case 23", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на &quot;key&quot; : &quot;value&quot;,.
        /// </summary>
        internal static string Incorrect_file_case_24 {
            get {
                return ResourceManager.GetString("Incorrect file case 24", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key : &quot;value&quot;
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_3 {
            get {
                return ResourceManager.GetString("Incorrect file case 3", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	key&quot; : &quot;value&quot;
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_4 {
            get {
                return ResourceManager.GetString("Incorrect file case 4", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : value
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_5 {
            get {
                return ResourceManager.GetString("Incorrect file case 5", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : value&quot;
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_6 {
            get {
                return ResourceManager.GetString("Incorrect file case 6", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : &quot;value
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_7 {
            get {
                return ResourceManager.GetString("Incorrect file case 7", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : True
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_8 {
            get {
                return ResourceManager.GetString("Incorrect file case 8", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на {
        ///	&quot;key&quot; : --1
        ///}.
        /// </summary>
        internal static string Incorrect_file_case_9 {
            get {
                return ResourceManager.GetString("Incorrect file case 9", resourceCulture);
            }
        }
    }
}
