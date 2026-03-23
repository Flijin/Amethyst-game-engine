namespace Amethyst_game_engine.Core.Render.Components;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
internal class GLSLMacrosAttribute : Attribute
{
    public string Macro { get; set; }
    public bool IsAll { get; set; }

    public GLSLMacrosAttribute(string macro)
    {
        Macro = macro;
        IsAll = false;
    }

    public GLSLMacrosAttribute(bool isAll)
    {
        Macro = string.Empty;
        IsAll = isAll;
    }
}
