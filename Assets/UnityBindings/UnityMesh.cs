using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RNumerics;
using System.Linq;
using System.Threading.Tasks;
using static UnityEditor.Progress;

public static class MitManager
{

    public static Dictionary<(RMaterial, int, Colorf), UnityMaterialHolder> mits = new Dictionary<(RMaterial, int, Colorf), UnityMaterialHolder>();

    public static UnityMaterialHolder GetMitWithOffset(RMaterial loadingLogo, int depth, Colorf color)
    {
        var mit = (UnityMaterialHolder)loadingLogo.Target;
        if (depth == 0 && color == Colorf.White)
        {
            return mit;
        }
        if (mits.ContainsKey((loadingLogo, depth, color)))
        {
            return mits[(loadingLogo, depth, color)];
        }
        else
        {
            return AddMit(loadingLogo, depth, color);
        }
    }

    public static UnityMaterialHolder AddMit(RMaterial loadingLogo, int depth, Colorf color)
    {
        UnityMaterialHolder CreateNewMit()
        {
            var miter = new UnityMaterialHolder(EngineRunner._);
            var mit = (UnityMaterialHolder)loadingLogo.Target;
            mit.LoadIn((unitymit) =>
            {
                var e = new Material(unitymit);
                e.renderQueue = Math.Min(e.renderQueue + depth, 5000);
                e.color *= new Color(color.r, color.g, color.b, color.a);
                try
                {
                    e.enableInstancing = true;
                }
                catch
                {
                    RLog.Err("Failed to add Instancing");
                }
                miter.material = e;
                miter.ForceMaterialLoadedIn();
            });
            mits.Add((loadingLogo, depth, color), miter);
            return miter;
        }
        var currentMit = CreateNewMit();
        loadingLogo.PramChanged += (mit) =>
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityEngine.Object.Destroy(mits[(loadingLogo, depth, color)].material);
                var mit = (UnityMaterialHolder)loadingLogo.Target;
                var e = new Material(mit.material);
                e.renderQueue = Math.Min(e.renderQueue + depth, 5000);
                e.color *= new Color(color.r, color.g, color.b, color.a);
                try
                {
                    e.enableInstancing = true;
                }
                catch
                {
                    RLog.Err("Failed to add Instancing");
                }
                currentMit.material = e;
                currentMit.ForceMaterialLoadedIn();
            });
        };
        loadingLogo.OnDispose += (mit) =>
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityEngine.Object.Destroy(mits[(loadingLogo, depth, color)].material);
                mits.Remove((loadingLogo, depth, color));
            });
        };
        return currentMit;
    }
}

public class UnityMeshHolder
{
    public Mesh mesh;
    public EngineRunner EngineRunner;
    public event Action<Mesh> OnMeshLoadedIn;

    public UnityMeshHolder(EngineRunner engineRunner)
    {
        EngineRunner = engineRunner;
    }

    public void ForceMaterialLoadedIn()
    {
        OnMeshLoadedIn?.Invoke(mesh);
    }

    public UnityMeshHolder(EngineRunner engineRunner, Mesh MakeMesh)
    {
        EngineRunner = engineRunner;
        mesh = MakeMesh;
        OnMeshLoadedIn?.Invoke(mesh);
    }
    public UnityMeshHolder(EngineRunner engineRunner, Func<Mesh> MakeMesh)
    {
        EngineRunner = engineRunner;
        engineRunner.RunonMainThread(() =>
        {
            mesh = MakeMesh();
            OnMeshLoadedIn?.Invoke(mesh);
        });
    }
    public void Action(Action<Mesh> action)
    {
        EngineRunner.RunonMainThread(() => action(mesh));
    }

    public void LoadIn(Action<Mesh> value)
    {
        if (mesh is null)
        {
            OnMeshLoadedIn += value;
        }
        else
        {
            value(mesh);
        }
    }
}

public static class BoneWeightAddons
{
    public static void Normalize(this ref BoneWeight bone)
    {
        float totalWeight = bone.weight0 + bone.weight1 + bone.weight2 + bone.weight3;
        if (totalWeight > 0f)
        {
            float num = 1f / totalWeight;
            bone.weight0 *= num;
            bone.weight1 *= num;
            bone.weight2 *= num;
            bone.weight3 *= num;
        }
    }

    public static void AddBone(this ref BoneWeight bone, int boneIndex, float boneWeight)
    {
        var targetLayer = -1;
        var smallestValue = float.MaxValue;
        for (int i = 0; i < 4; i++)
        {
            var currentLayerWeight = i switch
            {
                0 => bone.weight0,
                1 => bone.weight1,
                2 => bone.weight2,
                _ => bone.weight3,
            };
            if (currentLayerWeight < smallestValue)
            {
                smallestValue = currentLayerWeight;
                targetLayer = i;
            }
        }
        if (boneWeight > smallestValue)
        {
            switch (targetLayer)
            {
                case 0:
                    bone.weight0 = boneWeight;
                    bone.boneIndex0 = boneIndex;
                    break;
                case 1:
                    bone.weight1 = boneWeight;
                    bone.boneIndex1 = boneIndex;
                    break;
                case 2:
                    bone.weight2 = boneWeight;
                    bone.boneIndex2 = boneIndex;
                    break;
                case 3:
                    bone.weight3 = boneWeight;
                    bone.boneIndex3 = boneIndex;
                    break;
                default:
                    break;
            }
        }
    }
}

public class UnityMesh : IRMesh
{
    static ulong MeshCount = 0;
    public static ulong NextValue()
    {
        lock (typeof(UnityMesh))
        {
            return MeshCount++;
        }
    }

    private IEnumerable<int> LoadIndexs(RPrimitiveType primitiveType, IEnumerable<IFace> faces)
    {
        foreach (var item in faces)
        {
            switch (primitiveType)
            {
                case RPrimitiveType.Point:
                    if (item.Indices.Count > 0)
                    {
                        yield return (item.Indices[0]);
                    }
                    break;
                case RPrimitiveType.Line:
                    int? lastPoint = null;
                    foreach (var point in item.Indices)
                    {
                        if (lastPoint is not null)
                        {
                            yield return (int)lastPoint;
                        }
                        yield return point;
                        lastPoint = point;
                    }
                    break;
                case RPrimitiveType.Triangle:
                    if (item.Indices.Count == 3)
                    {
                        yield return (item.Indices[0]);
                        yield return (item.Indices[1]);
                        yield return (item.Indices[2]);
                    }
                    else if (item.Indices.Count == 4)
                    {
                        yield return (item.Indices[0]);
                        yield return (item.Indices[1]);
                        yield return (item.Indices[2]);
                        yield return (item.Indices[0]);
                        yield return (item.Indices[2]);
                        yield return (item.Indices[3]);
                    }
                    else
                    {
                        for (int i = 1; i < (item.Indices.Count - 1); i++)
                        {
                            yield return (item.Indices[i]);
                            yield return (item.Indices[i + 1]);
                            yield return (item.Indices[0]);
                        }
                    }
                    break;
                case RPrimitiveType.Polygon:
                    foreach (var point in item.Indices)
                    {
                        yield return point;
                    }
                    break;
                default:
                    break;
            }
        }
    }


    public string Name;

    public Vector3[] cvertices;

    public Vector3[] cnormals;
    public Vector4[] ctangents;
    public Color[] ccolors;
    public List<Vector3>[] cuv;

    public BoneWeight[] BoneVertexWights;
    public Matrix4x4[] bonePoses;

    public BlendShapeFrame[] blendShapeFrames;

    public struct BlendShapeFrame
    {
        public string name;
        public float wight;
        public Vector3[] vertices;
        public Vector3[] normals;
        public Vector3[] tangents;
    }

    public int[][] Faces;
    public MeshTopology[] PrimitiveTypes;

    public void LoadMeshData(IMesh rmesh)
    {
        try
        {
            Name = Guid.NewGuid().ToString();
            cvertices = Array.Empty<Vector3>();
            cnormals = Array.Empty<Vector3>();
            ctangents = Array.Empty<Vector4>();
            ccolors = Array.Empty<Color>();
            cuv = Array.Empty<List<Vector3>>();
            blendShapeFrames = Array.Empty<BlendShapeFrame>();
            Faces = Array.Empty<int[]>();
            PrimitiveTypes = Array.Empty<MeshTopology>();
            if (rmesh is null)
            {
                return;
            }
            if (rmesh.VertexCount == 0)
            {
                return;
            }
            if (rmesh is IComplexMesh complexMesh)
            {
                cvertices = new Vector3[complexMesh.Vertices.Count];
                for (int i = 0; i < complexMesh.Vertices.Count; i++)
                {
                    cvertices[i] = new Vector3(complexMesh.Vertices[i].x, complexMesh.Vertices[i].y, complexMesh.Vertices[i].z);
                }
                cnormals = new Vector3[complexMesh.Normals.Count];
                for (int i = 0; i < complexMesh.Normals.Count; i++)
                {
                    cnormals[i] = new Vector3(complexMesh.Normals[i].x, complexMesh.Normals[i].y, complexMesh.Normals[i].z);
                }
                ctangents = new Vector4[complexMesh.Tangents.Count];
                for (int i = 0; i < complexMesh.Tangents.Count; i++)
                {
                    var tangent = complexMesh.Tangents[i];
                    var crossnt = complexMesh.Normals[i].Cross(tangent);
                    ctangents[i] = new Vector4(tangent.x, tangent.y, tangent.z, (crossnt.Dot(complexMesh.BiTangents[i]) <= 0f) ? 1 : (-1));
                }
                cuv = new List<Vector3>[complexMesh.TexCoords.Length];
                for (int i = 0; i < complexMesh.TexCoords.Length; i++)
                {
                    cuv[i] = new List<Vector3>(complexMesh.TexCoords[i].Count);
                    for (int x = 0; x < complexMesh.TexCoords[i].Count; x++)
                    {
                        cuv[i].Add(new Vector3(complexMesh.TexCoords[i][x].x, 1f - complexMesh.TexCoords[i][x].y, complexMesh.TexCoords[i][x].z));
                    }
                }
                var colorAmount = 0;
                if (complexMesh.Colors.Length > 0)
                {
                    colorAmount = complexMesh.Colors[0].Count;
                }
                ccolors = new Color[colorAmount];
                if (complexMesh.Colors.Length > 0)
                {
                    Parallel.For(0, complexMesh.Colors[0].Count, (i) =>
                    {
                        ccolors[i] = new Color(complexMesh.Colors[0][i].r, complexMesh.Colors[0][i].g, complexMesh.Colors[0][i].b, complexMesh.Colors[0][i].a);
                    });
                }
                Name = "Complex Mesh:" + complexMesh.MeshName;
                if (complexMesh.HasBones)
                {
                    BoneVertexWights = new BoneWeight[complexMesh.VertexCount];
                    bonePoses = new Matrix4x4[complexMesh.BonesCount];
                    var BoneIndex = 0;
                    foreach (var Bone in complexMesh.Bones)
                    {
                        //bonePoses[BoneIndex] = Matrix4x4.identity;
                        bonePoses[BoneIndex] = new Matrix4x4
                        {
                            m00 = Bone.OffsetMatrix.m.M11,
                            m01 = Bone.OffsetMatrix.m.M12,
                            m02 = Bone.OffsetMatrix.m.M13,
                            m03 = Bone.OffsetMatrix.m.M14,
                            m10 = Bone.OffsetMatrix.m.M21,
                            m11 = Bone.OffsetMatrix.m.M22,
                            m12 = Bone.OffsetMatrix.m.M23,
                            m13 = Bone.OffsetMatrix.m.M24,
                            m20 = Bone.OffsetMatrix.m.M31,
                            m21 = Bone.OffsetMatrix.m.M32,
                            m22 = Bone.OffsetMatrix.m.M33,
                            m23 = Bone.OffsetMatrix.m.M34,
                            m30 = Bone.OffsetMatrix.m.M41,
                            m31 = Bone.OffsetMatrix.m.M42,
                            m32 = Bone.OffsetMatrix.m.M43,
                            m33 = Bone.OffsetMatrix.m.M44
                        };
                        foreach (var vertexWe in Bone.VertexWeights)
                        {
                            BoneVertexWights[vertexWe.VertexID].AddBone(BoneIndex, vertexWe.Weight);
                        }
                        BoneIndex++;
                    }
                    for (int i = 0; i < BoneVertexWights.Length; i++)
                    {
                        BoneVertexWights[i].Normalize();
                    }
                }
                if (complexMesh.HasMeshAttachments)
                {
                    blendShapeFrames = new BlendShapeFrame[complexMesh.MeshAttachments.Count()];
                    var current = 0;
                    foreach (var item in complexMesh.MeshAttachments)
                    {
                        try
                        {
                            blendShapeFrames[current].vertices = new Vector3[complexMesh.VertexCount];
                            var smallist = Math.Min(item.Vertices.Count, complexMesh.VertexCount);
                            for (int i = 0; i < smallist; i++)
                            {
                                blendShapeFrames[current].vertices[i] = new Vector3(item.Vertices[i].x, item.Vertices[i].y, item.Vertices[i].z) - cvertices[i];
                            }
                            blendShapeFrames[current].normals = new Vector3[complexMesh.VertexCount];
                            var smallistnorm = Math.Min(item.Normals.Count, complexMesh.VertexCount);
                            for (int i = 0; i < smallistnorm; i++)
                            {
                                blendShapeFrames[current].normals[i] = new Vector3(item.Normals[i].x, item.Normals[i].y, item.Normals[i].z) - cnormals[i];
                            }
                            blendShapeFrames[current].tangents = new Vector3[complexMesh.VertexCount];
                            var smallitsTang = Math.Min(complexMesh.Tangents.Count, complexMesh.VertexCount);
                            for (int i = 0; i < smallitsTang; i++)
                            {
                                var tangent = item.Tangents[i];
                                blendShapeFrames[current].tangents[i] = new Vector3(tangent.x - ctangents[i].x, tangent.y - ctangents[i].y, tangent.z - ctangents[i].z);
                            }
                            blendShapeFrames[current].name = item.Name;
                            blendShapeFrames[current].wight = item.Weight;
                            current++;
                        }
                        catch { }
                    }
                }

                Faces = new int[complexMesh.SubMeshes.Count() + 1][];
                Faces[0] = LoadIndexs(complexMesh.PrimitiveType, complexMesh.Faces).ToArray();
                PrimitiveTypes[0] = ToUnityPrimitive(complexMesh.PrimitiveType);
                var currentIndex = 0;
                foreach (var item in complexMesh.SubMeshes)
                {
                    currentIndex++;
                    Faces[currentIndex] = LoadIndexs(item.PrimitiveType, item.Faces).ToArray();
                    PrimitiveTypes[currentIndex] = ToUnityPrimitive(item.PrimitiveType);
                }
                return;
            }

            if (!rmesh.IsTriangleMesh)
            {
                RLog.Err("Unity can only render Triangle Meshes When basic");
                return;
            }
            cvertices = new Vector3[rmesh.VertexCount];
            cnormals = new Vector3[rmesh.VertexCount];
            ctangents = new Vector4[rmesh.VertexCount];
            cuv = new List<Vector3>[1];
            ccolors = new Color[rmesh.VertexCount];

            for (int i = 0; i < rmesh.VertexCount; i++)
            {
                var vert = rmesh.GetVertexAll(i);
                cvertices[i] = new Vector3((float)vert.v.x, (float)vert.v.y, (float)vert.v.z);
                cnormals[i] = new Vector3(vert.n.x, vert.n.y, vert.n.z);
                if (vert.bHaveUV && ((vert.uv?.Length ?? 0) > 0))
                {
                    cuv[0] ??= new List<Vector3>(rmesh.VertexCount);
                    cuv[0].Add(new Vector3(vert.uv[0].x, vert.uv[0].y, 0));
                }
                else
                {
                    cuv[0] ??= new List<Vector3>(rmesh.VertexCount);
                    cuv[0].Add(new Vector3(0, 0, 0));
                }
                if (vert.bHaveC)
                {
                    ccolors[i] = new Color(vert.c.x, vert.c.y, vert.c.z, 1f);
                }
                else
                {
                    ccolors[i] = new Color(1f, 1f, 1f, 1f);
                }
            }

            Faces = new int[1][];
            PrimitiveTypes = new MeshTopology[1];
            Faces[0] = rmesh.RenderIndices().ToArray();
            PrimitiveTypes[0] = MeshTopology.Triangles;
        }
        catch (Exception ex)
        {
            RLog.Err($"Mesh Update Failed Error:{ex}");
        }
    }

    private MeshTopology ToUnityPrimitive(RPrimitiveType primitiveType)
    {
        return primitiveType switch
        {
            RPrimitiveType.Point => MeshTopology.Points,
            RPrimitiveType.Line => MeshTopology.Lines,
            RPrimitiveType.Triangle => MeshTopology.Triangles,
            RPrimitiveType.Polygon => MeshTopology.Quads,
            _ => MeshTopology.LineStrip,
        };
    }

    public void LoadMeshToRender()
    {
        unityMeshHolder.Action((mesh) =>
        {
            mesh.name = Name;
            mesh.Clear(false);
            mesh.SetVertices(cvertices);
            mesh.SetTangents(ctangents);
            mesh.SetNormals(cnormals);
            for (int i = 0; i < cuv.Length; i++)
            {
                mesh.SetUVs(i, cuv[i]);
            }
            mesh.SetColors(ccolors);
            mesh.boneWeights = BoneVertexWights;
            mesh.bindposes = bonePoses;
            mesh.ClearBlendShapes();
            foreach (var item in blendShapeFrames)
            {
                mesh.AddBlendShapeFrame(item.name,item.wight,item.vertices,item.normals,item.tangents);
            }

            if (cvertices.Length > (ushort.MaxValue))
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }
            else
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
            }
            mesh.subMeshCount = Faces.Length;
            for (int i = 0; i < Faces.Length; i++)
            {
                mesh.SetIndices(Faces[i], PrimitiveTypes[i], i);
            }
        });
    }

    public UnityMeshHolder unityMeshHolder;

    public void Draw(RMaterial loadingLogo, Matrix p, Colorf tint, int zDepth, RenderLayer layer, int submesh)
    {
        EngineRunner._.Draw(unityMeshHolder.mesh, MitManager.GetMitWithOffset(loadingLogo, zDepth, tint).material, p, layer);
    }
    RMesh rMesh1;
    public void Init(RMesh rMesh)
    {
        rMesh1 = rMesh;
        unityMeshHolder = new UnityMeshHolder(EngineRunner._, () =>
        {
            var mesh = new Mesh();
            mesh.name = Guid.NewGuid().ToString();
            if (rMesh1.Dynamic)
            {
                mesh.MarkDynamic();
            }
            return mesh;
        });
    }
    public UnityMesh(UnityMeshHolder unityMeshHolder)
    {
        this.unityMeshHolder = unityMeshHolder;
    }

    public UnityMesh()
    {

    }
}
