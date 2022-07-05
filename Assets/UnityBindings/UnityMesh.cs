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
        var mit = (UnityMaterialHolder)loadingLogo?.Target;
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
            var miter = new UnityMaterialHolder(EngineRunner._, () =>
            {
                var mit = (UnityMaterialHolder)loadingLogo?.Target;
                var e = new Material(mit.material);
                e.renderQueue += depth;
                e.color *= new Color(color.r, color.g, color.b, color.a);
                return e;
            });
            mits.Add((loadingLogo, depth, color), miter);
            return miter;
        }

        loadingLogo.PramChanged += (mit) =>
        {
            EngineRunner._.RunonMainThread(() =>
            {
                UnityEngine.Object.Destroy(mits[(loadingLogo, depth, color)].material);
                mits.Remove((loadingLogo, depth, color));
                CreateNewMit();
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
        return CreateNewMit();
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

    public void LoadMesh(RMesh meshtarget, IMesh rmesh)
    {
        EngineRunner._.RunonMainThread(() =>
        {
            try
            {
                if (meshtarget.mesh is null)
                {
                    meshtarget.mesh = new Mesh();
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
