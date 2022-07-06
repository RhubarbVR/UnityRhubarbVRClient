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


public class UnityMesh : IRMesh
{
    public static Mesh MakeQuad()
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
        return mesh;
    }

    public Mesh LoadedQuad = MakeQuad();

    public RMesh Quad => new(LoadedQuad);

    public EngineRunner EngineRunner { get; }

    public void Draw(string id, object mesh, RMaterial loadingLogo, Matrix p, Colorf tint, int gueu, RenderLayer layer)
    {
        EngineRunner.Draw(id, (Mesh)mesh, MitManager.GetMitWithOffset(loadingLogo, gueu, tint).material, p, layer);
    }
    static ulong MeshCount = 0;
    public static ulong NextValue()
    {
        lock (typeof(UnityMesh))
        {
            return MeshCount++;
        }
    }

    public void LoadMesh(RMesh meshtarget, IMesh rmesh)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            try
            {
                if (meshtarget.mesh is null)
                {
                    meshtarget.mesh = new Mesh();
                    ((Mesh)meshtarget.mesh).name = "BasicMesh." + NextValue();
                }
                Mesh mesh = (Mesh)meshtarget.mesh;
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
                            cuv[i].Add(new Vector3(complexMesh.TexCoords[i][x].x, complexMesh.TexCoords[i][x].y, complexMesh.TexCoords[i][x].z));
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
                        var BoneWights = new BoneWeight[complexMesh.BonesCount];
                        float GetCurrentBoneWeight(int currentBone, int index)
                        {
                            return index switch
                            {
                                0 => BoneWights[currentBone].weight0,
                                1 => BoneWights[currentBone].weight1,
                                2 => BoneWights[currentBone].weight2,
                                3 => BoneWights[currentBone].weight3,
                                _ => throw new NotSupportedException(),
                            };
                        }
                        Parallel.ForEach(complexMesh.Bones, (Bone, _, BoneIndex) =>
                        {
                            var vertweights = Bone.VertexWeights.ToArray();
                            for (int i = 0; i < vertweights.Length; i++)
                            {
                                var currentBone = (int)BoneIndex;
                                var weight = vertweights[i].Weight;
                                int largest = -1;
                                float weighMaxCheck = float.MaxValue;
                                for (int currentWeght = 0; currentWeght < 4; currentWeght++)
                                {
                                    float weight2 = GetCurrentBoneWeight(currentBone, currentWeght);
                                    if (weight2 < weighMaxCheck)
                                    {
                                        weighMaxCheck = weight2;
                                        largest = currentWeght;
                                    }
                                }
                                if (weight > weighMaxCheck)
                                {
                                    switch (largest)
                                    {
                                        case 0:
                                            BoneWights[currentBone].boneIndex0 = currentBone;
                                            BoneWights[currentBone].weight0 = weight;
                                            break;
                                        case 1:
                                            BoneWights[currentBone].boneIndex1 = currentBone;
                                            BoneWights[currentBone].weight1 = weight;
                                            break;
                                        case 2:
                                            BoneWights[currentBone].boneIndex2 = currentBone;
                                            BoneWights[currentBone].weight2 = weight;
                                            break;
                                        case 3:
                                            BoneWights[currentBone].boneIndex3 = currentBone;
                                            BoneWights[currentBone].weight3 = weight;
                                            break;
                                    }
                                }
                            }
                        });
                        mesh.boneWeights = BoneWights;
                    }
                    if (complexMesh.HasMeshAttachments)
                    {
                        mesh.ClearBlendShapes();
                        foreach (var item in complexMesh.MeshAttachments)
                        {
                            var svertices = new Vector3[item.Vertices.Count];
                            Parallel.For(0, item.Vertices.Count, (i) =>
                            {
                                svertices[i] = cvertices[i] - new Vector3(item.Vertices[i].x, item.Vertices[i].y, item.Vertices[i].z);
                            });
                            var snormals = new Vector3[item.Normals.Count];
                            Parallel.For(0, item.Normals.Count, (i) =>
                            {
                                snormals[i] = cnormals[i] - new Vector3(item.Normals[i].x, item.Normals[i].y, item.Normals[i].z);
                            });
                            var stangents = new Vector3[complexMesh.Tangents.Count];
                            Parallel.For(0, complexMesh.Tangents.Count, (i) =>
                            {
                                var tangent = item.Tangents[i];
                                stangents[i] = new Vector3(ctangents[i].x - tangent.x, ctangents[i].y - tangent.y, ctangents[i].z - tangent.z);
                            });
                            mesh.AddBlendShapeFrame(item.Name, item.Weight, svertices, snormals, stangents);
                        }
                    }
                    var indexCount = 0;
                    switch (complexMesh.PrimitiveType)
                    {
                        case RPrimitiveType.Point:
                            indexCount = 1;
                            break;
                        case RPrimitiveType.Line:
                            indexCount = 2;
                            break;
                        case RPrimitiveType.Triangle:
                            indexCount = 3;
                            break;
                        case RPrimitiveType.Polygon:
                            indexCount = 4;
                            break;
                        default:
                            RLog.Err("Multi Primitives Are not supported in Unity");
                            return;
                    }
                    var indexs = new int[complexMesh.TriangleCount * indexCount];
                    Parallel.ForEach(complexMesh.Faces, (Face, _, index) => {
                        var indexStart = index * indexCount;
                        for (int i = 0; i < indexCount; i++)
                        {
                            indexs[indexStart + i] = Face.Indices[i];
                        }
                    });
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
            catch
            {
                RLog.Err("Mesh Update Failed");
            }
        });
    }

    public UnityMesh(EngineRunner engineRunner)
    {
        EngineRunner = engineRunner;
    }
}
