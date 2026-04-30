using Amethyst_game_engine.Core.GameObjects.Components;
using Amethyst_game_engine.Core.Render.Settings;
using Amethyst_game_engine.Models.Components;
using Amethyst_game_engine.Models.GLBModule;
using OpenTK.Graphics.OpenGL4;

namespace Amethyst_game_engine.Core.GameObjects;

public class GLBGameObject : DrawableObject
{
    internal int GLBModelIndex { get; private set; }

    public GLBGameObject(GLBModel model) : base(model.Box)
    {
        BuildObject(model);
    }

    private void BuildObject(GLBModel model)
    {
        var meshesData = model.MeshesData;
        Mesh[] meshes = new Mesh[meshesData.Count];

        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i] = ConvertMeshDataToMesh(meshesData[i]);
        }

        Meshes = meshes;
        ModelPath = model.Path;
        GLBModelIndex = model.GLBModelIndex;
    }

    private static Mesh ConvertMeshDataToMesh(MeshData mesh)
    {
        Mesh result;
        Primitive[] primitives = new Primitive[mesh.Primitives.Length];

        for (int i = 0; i < primitives.Length; i++)
        {
            primitives[i] = ConvertPrimitiveDataToPrimitive(mesh.Primitives[i]);
        }

        unsafe
        {
            result = new(primitives, true) { Matrix = mesh.Matrix };
        }

        return result;
    }

    private static Primitive ConvertPrimitiveDataToPrimitive(PrimitiveData primitive)
    {
        Primitive result;

        int vao = GL.GenVertexArray();
        GL.BindVertexArray(vao);

        var notSupportedSettings = RenderSettings.MetallicFactor | RenderSettings.OcclusionMap |
                                   RenderSettings.MetallicRoughnessMap | RenderSettings.NormalMap;

        result = new(vao, primitive.Options)
        {
            SettingsFromModel = primitive.Material?.Flags & ~notSupportedSettings ?? RenderSettings.None
        };

        var vertexBuffer = GL.GenBuffer();
        GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

        GL.BufferData(BufferTarget.ArrayBuffer,
                      primitive.Vertices.Length,
                      primitive.Vertices,
                      BufferUsageHint.StaticDraw);

        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
        GL.EnableVertexAttribArray(0);

        result.GLBuffers.Add(vertexBuffer);

        if (primitive.Normals is not null)
        {
            var normalBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, normalBuffer);

            GL.BufferData(BufferTarget.ArrayBuffer,
                          primitive.Normals.Length,
                          primitive.Normals,
                          BufferUsageHint.StaticDraw);

            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(1);

            result.GLBuffers.Add(normalBuffer);
        }

        if (primitive.Indices is not null)
        {
            var indicesBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indicesBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer,
                          primitive.Indices.Length,
                          primitive.Indices,
                          BufferUsageHint.StaticDraw);

            result.GLBuffers.Add(indicesBuffer);
        }

        if (primitive.Material is not null)
        {
            Material resultMaterial = new();

            if (primitive.Material.AlbedoMap is not null)
            {
                TextureData albedoData = (TextureData)primitive.Material.AlbedoMap;

                Texture albedo = new(albedoData);
                resultMaterial.AlbedoMap = albedo;

                var albedoMapCoords = GL.GenBuffer();
                var uvSet = primitive.UVSets[albedoData.TexCoords];

                GL.BindBuffer(BufferTarget.ArrayBuffer, albedoMapCoords);
                GL.BufferData(BufferTarget.ArrayBuffer,
                              uvSet.Length,
                              uvSet,
                              BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(3, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                GL.EnableVertexAttribArray(3);

                result.GLBuffers.Add(albedoMapCoords);
            }

            if (primitive.Material.EmissiveMap is not null)
            {
                TextureData emissiveData = (TextureData)primitive.Material.EmissiveMap;

                Texture emissive = new(emissiveData);
                resultMaterial.EmissiveMap = emissive;

                var emissiveMapCoords = GL.GenBuffer();
                var uvSet = primitive.UVSets[emissiveData.TexCoords];

                GL.BindBuffer(BufferTarget.ArrayBuffer, emissiveMapCoords);
                GL.BufferData(BufferTarget.ArrayBuffer,
                              uvSet.Length,
                              uvSet,
                              BufferUsageHint.StaticDraw);

                GL.VertexAttribPointer(4, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);
                GL.EnableVertexAttribArray(4);

                result.GLBuffers.Add(emissiveMapCoords);
            }

            if (primitive.Material.EmissiveFactor.IsNoneColor == false)
            {
                resultMaterial.EmissiveFactor = primitive.Material.EmissiveFactor;
            }

            if (primitive.Material.RoughnessFactor != -1)
            {
                resultMaterial.RoughnessFactor = primitive.Material.RoughnessFactor;
            }

            result.Material = resultMaterial;
        }

        GL.BindVertexArray(0);

        return result;
    }
}
