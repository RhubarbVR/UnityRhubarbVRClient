using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharRenderComp : MonoBehaviour
{
    public Font font;
    public char c;
    public Color color;
    string str => c.ToString();
    Mesh mesh;
    
    void OnFontTextureRebuilt(Font changedFont)
    {
        if (changedFont != font)
            return;

        RebuildMesh();
    }

    void RebuildMesh()
    {
        // Generate a mesh for the characters we want to print.
        var vertices = new Vector3[4];
        var triangles = new int[6];
        var uv = new Vector2[4];
        var colors = new Color[4];
        Vector3 pos = Vector3.zero;

        font.GetCharacterInfo(c, out var ch, 355);

        vertices[0] = pos + new Vector3(ch.minX, ch.maxY, -1);
        vertices[1] = pos + new Vector3(ch.maxX, ch.maxY, -1);
        vertices[2] = pos + new Vector3(ch.maxX, ch.minY, -1);
        vertices[3] = pos + new Vector3(ch.minX, ch.minY, -1);

        uv[0] = ch.uvTopLeft;
        uv[1] = ch.uvTopRight;
        uv[2] = ch.uvBottomRight;
        uv[3] = ch.uvBottomLeft;

        triangles[0] = 0;
        triangles[1] = 1;
        triangles[2] = 2;

        triangles[3] = 0;
        triangles[4] = 2;
        triangles[5] = 3;
        colors[0] = colors[1] = colors[2] = colors[3] = color;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.colors = colors;
    }

    void Start()
    {

    }

    public void UpdateRender(char ch, Font fontc, Color colord)
    {
        var update = false;
        if(ch != c)
        {
            c = ch;
            update = true;
        }
        if (fontc != font)
        {
            font = fontc;
            update = true;
            font.RequestCharactersInTexture(str, 355);
        }
        if (colord != color)
        {
            color = colord;
            update = true;
        }
        if (update)
        {
            RebuildMesh();
        }
    }
    Material mit;
    public void StartCharRender(char ch, Font fontc,Color colord)
    {
        c = ch;
        font = fontc;
        color = colord;
        // Set the rebuild callback so that the mesh is regenerated on font changes.
        Font.textureRebuilt += OnFontTextureRebuilt;

        // Request characters.
        font.RequestCharactersInTexture(str,355);

        mit = new Material(font.material);
        mit.shader = EngineRunner._.FontShader;
        mit.renderQueue = 4150;
        // Set up mesh.
        mesh = new Mesh();
        gameObject.AddComponent<MeshRenderer>().material = mit;
        gameObject.AddComponent<MeshFilter>().mesh = mesh;

        // Generate font mesh.
        RebuildMesh();
    }

    void Update()
    {
        // Keep requesting our characters each frame, so Unity will make sure that they stay in the font when regenerating the font texture.
        font.RequestCharactersInTexture(str, 355);
    }

    void OnDestroy()
    {
        Font.textureRebuilt -= OnFontTextureRebuilt;
        UnityEngine.Object.Destroy(mit);
    }
}
