﻿//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Amethyst_game_engine {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Amethyst_game_engine.Resources", typeof(Resources).Assembly);
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
        ///   Ищет локализованную строку, похожую на #version 330 core
        ///
        ///#ifdef USE_COLOR
        ///in vec4 color;
        ///#endif
        ///
        ///out vec4 FragColor;
        ///
        ///void main()
        ///{
        ///#ifdef USE_COLOR
        ///    FragColor = color;
        ///#endif
        ///
        ///#ifndef USE_COLOR
        ///    FragColor = vec4(0.5, 0.5, 0.5, 1);
        ///#endif
        ///}
        ///.
        /// </summary>
        internal static string UniversalFragmentShader {
            get {
                return ResourceManager.GetString("UniversalFragmentShader", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Ищет локализованную строку, похожую на #version 330 core
        ///
        ///layout (location = 0) in vec3 _position;
        ///
        ///#ifdef USE_COLOR
        ///layout (location = 1) in vec4 _color;
        ///#endif
        ///
        ///#ifdef USE_MESH_MATRIX
        ///uniform mat4 mesh;
        ///#endif
        ///
        ///uniform mat4 model;
        ///uniform mat4 view;
        ///uniform mat4 projection;
        ///
        ///#ifdef USE_COLOR
        ///out vec4 color;
        ///#endif
        ///
        ///void main()
        ///{
        ///#ifdef USE_COLOR
        ///    color = _color;
        ///#endif
        ///
        ///#ifdef USE_MESH_MATRIX
        ///    gl_Position = vec4(_position, 1.0) * mesh * model * view * projection;
        ///#else
        ///    gl_Position = vec4(_position, 1.0) * [остаток строки не уместился]&quot;;.
        /// </summary>
        internal static string UniversalVertexShader {
            get {
                return ResourceManager.GetString("UniversalVertexShader", resourceCulture);
            }
        }
    }
}
