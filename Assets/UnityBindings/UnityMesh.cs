using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RhuEngine;
using RhuEngine.Linker;
using System;
using RNumerics;
using System.Linq;

public class UnityMesh : IRMesh
{
    public static Mesh MakeQuad()
    {
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(0, 0, 0),
            new Vector3(1, 0, 0),
            new Vector3(0, 1, 0),
            new Vector3(1, 1, 0)
        };
        mesh.vertices = vertices;

        int[] tris = new int[6]
        {
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
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
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.uv = uv;
        return mesh;
    }

    public Mesh LoadedQuad = MakeQuad();

    public RMesh Quad => new (LoadedQuad);

    public EngineRunner EngineRunner { get; }

    public void Draw(string id, object mesh, RMaterial loadingLogo, Matrix p, Colorf tint)
    {
        EngineRunner.Draw(id, (Mesh)mesh, (Material)loadingLogo.Target, p, tint);
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
                uv[i] = new Vector3((float)vert.uv[0].x, (float)vert.uv[0].y);
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
