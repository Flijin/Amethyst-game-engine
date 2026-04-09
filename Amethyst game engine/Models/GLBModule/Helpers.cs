namespace Amethyst_game_engine.Models.GLBModule;

internal static class Helpers
{
    public static int GetNumberOfComponents(string typeName)
    {
        return typeName switch
        {
            "SCALAR" => 1,
            "VEC2" => 2,
            "VEC3" => 3,
            "VEC4" => 4,
            "MAT2" => 4,
            "MAT3" => 9,
            "MAT4" => 16,

            _ => -1
        };
    }

    public static int SizeOfComponent(int componentType)
    {
        return componentType switch
        {
            5120 => 1,
            5121 => 1,
            5122 => 2,
            5123 => 2,
            5125 => 4,
            5126 => 4,

            _ =>  -1
        };
    }
}
