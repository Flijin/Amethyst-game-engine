namespace Amethyst_game_engine.Render;

[Flags]
public enum RenderSettings
{
    None = 0,
    UseColors = 1, //Атрибуты
    UseAlbedoMap = 2, //карта
    UseMetallicRoughness = 4, //Коэф, карта
    UseNormalMap = 8, //карта
    UseNormals = 16, //Атрибуты
    UseOcclusionMap = 32, //Коэф, карта
    UseEmissiveMap = 64, //Коэф, карта
}

//USE_COLOR = 1, //Атрибуты
//UseAlbedoMap = 2, //карта
//UseMetallicRoughness = 4, //Коэф, карта
//UseNormalMap = 8, //карта
//UseNormals = 16, //Атрибуты
//Occlusion = 32, //Коэф, карта
//Emissive = 64, //Коэф, карта

//USE_MESH_MATRIX = 256
//USE_COLOR_5_BITS = 512