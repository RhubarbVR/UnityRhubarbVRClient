using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RNumerics;
using System.Linq;
public static class MitManager
{

    public static Dictionary<(RMaterial, int,Colorf), Material> mits = new Dictionary<(RMaterial, int, Colorf), Material>();

    public static Material GetMitWithOffset(RMaterial loadingLogo, int depth, Colorf color)
    {
        var mit = (Material)loadingLogo?.Target;
        if (depth == 0 && color == Colorf.White)
        {
            return mit;
        }
        if (mits.ContainsKey((loadingLogo, depth, color)))
        {
            return mits[(loadingLogo, depth,color)];
        }
        else
        {
            return AddMit(loadingLogo, depth,color);
        }
    }

    public static Material AddMit(RMaterial loadingLogo, int depth,Colorf color)
    {
        Material CreateNewMit()
        {
            var mit = (Material)loadingLogo?.Target;
            var e = new Material(mit);
            e.renderQueue += depth;
            e.color = new Color(color.r, color.g, color.b, color.a);
            mits.Add((loadingLogo, depth,color), e);
            return mit;
        }

        loadingLogo.PramChanged += (mit) => {
            UnityEngine.Object.Destroy(mits[(loadingLogo, depth,color)]);
            mits.Remove((loadingLogo, depth,color));
            CreateNewMit();
        };
        loadingLogo.OnDispose += (mit) => {
            UnityEngine.Object.Destroy(mits[(loadingLogo, depth, color)]);
            mits.Remove((loadingLogo, depth, color));
        };
        return CreateNewMit();
    }
}


public class UnityMesh : IRMesh
{
    public static Mesh MakeQuad()
    {
        Mesh mesh = new Mesh();

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
            new Vector2(1, -1),
            new Vector2(0, -1),
            new Vector2(0, 0),
            new Vector2(1, 0)
        };
        mesh.uv = uv;
        return mesh;
    }

    public Mesh LoadedQuad = MakeQuad();

    public RMesh Quad => new (LoadedQuad);

    public EngineRunner EngineRunner { get; }

    public void Draw(string id, object mesh, RMaterial loadingLogo, Matrix p, Colorf tint,int gueu)
    {
        EngineRunner.Draw(id, (Mesh)mesh,MitManager.GetMitWithOffset(loadingLogo,gueu,tint), p);
    }

    public void LoadMesh(RMesh meshtarget, IMesh rmesh)
    {
        if(meshtarget.mesh is null)
        {
            meshtarget.mesh = new Mesh();
        }
        Mesh mesh = (Mesh)meshtarget.mesh;
        if (rmesh is null)
        {
            return;
        }

        var vertices = new Vector3[rmesh.VertexCount];
        var normals = new Vector3[rmesh.VertexCount];
        var uv = new Vector2[rmesh.VertexCount];
        var colors = new Color[rmesh.VertexCount];

        for (var i = 0; i < rmesh.VertexCount; i++)
        {
            var vert = rmesh.GetVertexAll(i);
            vertices[i] = new Vector3((float)vert.v.x, (float)vert.v.y, (float)vert.v.z);
            normals[i] = new Vector3((float)vert.n.x, (float)vert.n.y, (float)vert.n.z);
            if (vert.bHaveUV && ((vert.uv?.Length ?? 0) > 0))
            {
                uv[i] = new Vector3((float)vert.uv[0].x, -(float)vert.uv[0].y);
            }
            if (vert.bHaveC)
            {
                colors[i] = new Color(vert.c.x, vert.c.y, vert.c.z, 1);
            }
            else
            {
                colors[i] = new Color(1, 1, 1, 1);
            }
        }

        mesh.SetTriangles(Array.Empty<int>(), 0);

        mesh.SetVertices(vertices);

        mesh.SetNormals(normals);

        mesh.SetUVs(0, uv);

        mesh.SetColors(colors);

        mesh.SetTriangles(rmesh.RenderIndices().ToArray(),0);

        mesh.RecalculateBounds();
    }

    public UnityMesh(EngineRunner engineRunner)
    {
        EngineRunner = engineRunner;
    }
}
