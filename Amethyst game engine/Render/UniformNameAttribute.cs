namespace Amethyst_game_engine.Render;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
internal class UniformNameAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}
