using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RNumerics;
using System.Linq;
using System.Threading.Tasks;

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
                e.renderQueue = Math.Min(e.renderQueue + depth,5000);
                e.color *= new Color(color.r, color.g, color.b, color.a);
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
    public static UnityMeshHolder MakeQuad()
    {
        Mesh mesh = new();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f,-0.5f,0),
            new Vector3(0.5f,-0.5f,0),
            new Vector3(0.5f, 0.5f,0),
            new Vector3(-0.5f, 0.5f,0)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]
        {
            2, 1, 0,
            3, 2, 0
        };
        mesh.triangles = tris;

        Vector3[] normals = new Vector3[4]
        {
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        mesh.normals = normals;

        Vector2[] uv = new Vector2[4]
        {
            new Vector2(1, 1),
            new Vector2(0, 1),
            new Vector2(0, 0),
            new Vector2(1, 0)
        };
        mesh.uv = uv;
        return new UnityMeshHolder(EngineRunner._, mesh);
    }

    public UnityMeshHolder LoadedQuad = MakeQuad();

    public RMesh Quad => new(LoadedQuad);

    public EngineRunner EngineRunner { get; }

    public void Draw(string id, object mesh, RMaterial loadingLogo, Matrix p, Colorf tint, int gueu, RenderLayer layer)
    {
        EngineRunner.Draw(id, ((UnityMeshHolder)mesh).mesh, MitManager.GetMitWithOffset(loadingLogo, gueu, tint).material, p, layer);
    }
    static ulong MeshCount = 0;
    public static ulong NextValue()
    {
        lock (typeof(UnityMesh))
        {
            return MeshCount++;
        }
    }


    public void MeshLoadAction(Mesh unityMesh, IMesh rmesh)
    {
        try
        {
            Mesh mesh = unityMesh;
            mesh.Clear();
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
                var cvertices = new Vector3[complexMesh.Vertices.Count];
                Parallel.For(0, complexMesh.Vertices.Count, (i) =>
                {
                    cvertices[i] = new Vector3(complexMesh.Vertices[i].x, complexMesh.Vertices[i].y, complexMesh.Vertices[i].z);
                });
                var cnormals = new Vector3[complexMesh.Normals.Count];
                Parallel.For(0, complexMesh.Normals.Count, (i) =>
                {
                    cnormals[i] = new Vector3(complexMesh.Normals[i].x, complexMesh.Normals[i].y, complexMesh.Normals[i].z);
                });
                var ctangents = new Vector4[complexMesh.Tangents.Count];
                Parallel.For(0, complexMesh.Tangents.Count, (i) =>
                {
                    var tangent = complexMesh.Tangents[i];
                    var crossnt = complexMesh.Normals[i].Cross(tangent);
                    ctangents[i] = new Vector4(tangent.x, tangent.y, tangent.z, (crossnt.Dot(complexMesh.BiTangents[i]) <= 0f) ? 1 : (-1));
                });
                var cuv = new List<Vector3>[complexMesh.TexCoords.Length];
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
                var ccolors = new Color[colorAmount];
                if (complexMesh.Colors.Length > 0)
                {
                    Parallel.For(0, complexMesh.Colors[0].Count, (i) =>
                    {
                        ccolors[i] = new Color(complexMesh.Colors[0][i].r, complexMesh.Colors[0][i].g, complexMesh.Colors[0][i].b, complexMesh.Colors[0][i].a);
                    });
                }
                mesh.name = "Complex Mesh:" + complexMesh.MeshName;
                mesh.SetVertices(cvertices);
                mesh.SetTangents(ctangents);
                mesh.SetNormals(cnormals);
                for (int i = 0; i < cuv.Length; i++)
                {
                    mesh.SetUVs(i, cuv[i]);
                }
                mesh.SetColors(ccolors);
                if (complexMesh.HasBones)
                {
                    var BoneVertexWights = new BoneWeight[complexMesh.VertexCount];
                    var bonePoses = new Matrix4x4[complexMesh.BonesCount];
                    var BoneIndex = 0;
                    foreach (var Bone in complexMesh.Bones)
                    {
                        //bonePoses[BoneIndex] = Matrix4x4.identity;
                        bonePoses[BoneIndex] = new Matrix4x4
                        {
                            m00 = Bone.OffsetMatrix.m.M11,
                            m01 = Bone.OffsetMatrix.m.M12,
                            m02 = Bone.OffsetMatrix.m.M13,
                            m03 = Bone.OffsetMatrix.m.M21,
                            m10 = Bone.OffsetMatrix.m.M22,
                            m11 = Bone.OffsetMatrix.m.M23,
                            m12 = Bone.OffsetMatrix.m.M24,
                            m13 = Bone.OffsetMatrix.m.M31,
                            m20 = Bone.OffsetMatrix.m.M32,
                            m21 = Bone.OffsetMatrix.m.M33,
                            m22 = Bone.OffsetMatrix.m.M34,
                            m23 = Bone.OffsetMatrix.m.M41,
                            m31 = Bone.OffsetMatrix.m.M42,
                            m32 = Bone.OffsetMatrix.m.M43,
                            m33 = Bone.OffsetMatrix.m.M44,
                        };
                        foreach (var vertexWe in Bone.VertexWeights)
                        {
                            BoneVertexWights[vertexWe.VertexID].AddBone(BoneIndex, vertexWe.Weight);
                        }
                        BoneIndex++;
                    }
                    mesh.boneWeights = BoneVertexWights;
                    mesh.bindposes = bonePoses;
                }
                if (complexMesh.HasMeshAttachments)
                {
                    mesh.ClearBlendShapes();
                    foreach (var item in complexMesh.MeshAttachments)
                    {
                        var svertices = new Vector3[complexMesh.VertexCount];
                        Parallel.For(0, Math.Min(item.Vertices.Count, complexMesh.VertexCount), (i) =>
                        {
                            svertices[i] = new Vector3(item.Vertices[i].x, item.Vertices[i].y, item.Vertices[i].z) - cvertices[i];
                        });
                        var snormals = new Vector3[complexMesh.VertexCount];
                        Parallel.For(0, Math.Min(item.Normals.Count, complexMesh.VertexCount), (i) =>
                        {
                            snormals[i] = new Vector3(item.Normals[i].x, item.Normals[i].y, item.Normals[i].z) - cnormals[i];
                        });
                        var stangents = new Vector3[complexMesh.VertexCount];
                        Parallel.For(0, Math.Min(complexMesh.Tangents.Count, complexMesh.VertexCount), (i) =>
                        {
                            var tangent = item.Tangents[i];
                            stangents[i] = new Vector3(tangent.x - ctangents[i].x,tangent.y - ctangents[i].y, tangent.z - ctangents[i].z);
                        });
                        mesh.AddBlendShapeFrame(item.Name, item.Weight, svertices, snormals, stangents);
                    }
                }
                var indexs = LoadIndexs(complexMesh).ToArray();
                RLog.Info($"Loaded Mesh PrimitiveType{complexMesh.PrimitiveType}");
                switch (complexMesh.PrimitiveType)
                {
                    case RPrimitiveType.Point:
                        mesh.SetIndices(indexs, MeshTopology.Points, 0);
                        break;
                    case RPrimitiveType.Line:
                        mesh.SetIndices(indexs, MeshTopology.Lines, 0);
                        break;
                    case RPrimitiveType.Triangle:
                        mesh.SetIndices(indexs, MeshTopology.Triangles, 0);
                        break;
                    case RPrimitiveType.Polygon:
                        mesh.SetIndices(indexs, MeshTopology.Quads, 0);
                        break;
                    default:
                        break;
                }
                return;
            }

            if (!rmesh.IsTriangleMesh)
            {
                RLog.Err("Unity can only render Triangle Meshes When basic");
                return;
            }
            var vertices = new Vector3[rmesh.VertexCount];
            var normals = new Vector3[rmesh.VertexCount];
            var uv = new Vector2[rmesh.VertexCount];
            var colors = new Color[rmesh.VertexCount];

            Parallel.For(0, rmesh.VertexCount, (i) =>
            {

                var vert = rmesh.GetVertexAll(i);
                vertices[i] = new Vector3((float)vert.v.x, (float)vert.v.y, (float)vert.v.z);
                normals[i] = new Vector3(vert.n.x, vert.n.y, vert.n.z);
                if (vert.bHaveUV && ((vert.uv?.Length ?? 0) > 0))
                {
                    uv[i] = new Vector2(vert.uv[0].x, vert.uv[0].y);
                }
                if (vert.bHaveC)
                {
                    colors[i] = new Color(vert.c.x, vert.c.y, vert.c.z, 1f);
                }
                else
                {
                    colors[i] = new Color(1f, 1f, 1f, 1f);
                }
            });

            mesh.SetVertices(vertices);

            mesh.SetNormals(normals);

            mesh.SetUVs(0, uv);

            mesh.SetColors(colors);
            mesh.SetTriangles(rmesh.RenderIndices().ToArray(), 0);

            mesh.RecalculateBounds();
        }
        catch(Exception ex)
        {
            RLog.Err($"Mesh Update Failed Error:{ex}");
        }
    }

    private IEnumerable<int> LoadIndexs(IComplexMesh rmesh)
    {
        foreach (var item in rmesh.Faces)
        {
            switch (rmesh.PrimitiveType)
            {
                case RPrimitiveType.Point:
                    if (item.Indices.Count > 0)
                    {
                        yield return item.Indices[0];
                    }
                    break;
                case RPrimitiveType.Line:
                    int? lastPoint = null;
                    foreach (var point in item.Indices)
                    {
                        if(lastPoint is not null)
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
                        yield return item.Indices[0];
                        yield return item.Indices[1];
                        yield return item.Indices[2];
                    }
                    else if (item.Indices.Count >= 4)
                    {
                        yield return item.Indices[0];
                        yield return item.Indices[1];
                        yield return item.Indices[2];
                        yield return item.Indices[0];
                        yield return item.Indices[2];
                        yield return item.Indices[3];
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

    public void LoadMesh(RMesh meshtarget, IMesh rmesh)
    {
        if(meshtarget.mesh is not null)
        {
            ((UnityMeshHolder)meshtarget.mesh).Action((mesh) =>
            {
                MeshLoadAction(mesh, rmesh);
            });
        }
        else
        {
            meshtarget.mesh = new UnityMeshHolder(EngineRunner, () =>
            {
                var mesh = new Mesh();
                MeshLoadAction(mesh, rmesh);
                return mesh;
            });
        } 
    }

    public UnityMesh(EngineRunner engineRunner)
    {
        EngineRunner = engineRunner;
    }
}
