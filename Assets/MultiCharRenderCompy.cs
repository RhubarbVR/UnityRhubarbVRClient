using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MultiCharRenderCompy : MonoBehaviour
{
    public Font font;

    [System.Serializable]
    public struct RenderedChar
    {
        public char c;
        public Color color;
        public Transform matrix;
    }

    public string Str()
    {
        var data = "";
        foreach (var item in renderedChars)
        {
            data += item;
        }
        return data;
    }

    Mesh mesh;
    
    void OnFontTextureRebuilt(Font changedFont)
    {
        if (changedFont != font)
            return;
        RebuildMesh();
    }

    public RenderedChar[] renderedChars;
    void RebuildMesh()
    {
        if(renderedChars == null)
        {
            return;
        }
        var length = renderedChars.Length;
        // Generate a mesh for the characters we want to print.
        var vertices = new Vector3[4 * length];
        var triangles = new int[6 * length];
        var uv = new Vector2[4 * length];
        var colors = new Color[4 * length];
        Vector3 pos = Vector3.zero;
        var charCont = 0;
        void LoadChar(char c,Color color,Transform matrix4)
        {
            var v1 = charCont * 4;
            var v2 = v1 + 1;
            var v3 = v1 + 2;
            var v4 = v1 + 3;
            font.GetCharacterInfo(c, out var ch, 355);
            pos = matrix4.TransformPoint(pos);
            vertices[v1] = pos + new Vector3(ch.minX, ch.maxY, -1);
            vertices[v2] = pos + new Vector3(ch.maxX, ch.maxY, -1);
            vertices[v3] = pos + new Vector3(ch.maxX, ch.minY, -1);
            vertices[v4] = pos + new Vector3(ch.minX, ch.minY, -1);

            uv[v1] = ch.uvTopLeft;
            uv[v2] = ch.uvTopRight;
            uv[v3] = ch.uvBottomRight;
            uv[v4] = ch.uvBottomLeft;
            var triIndex = charCont * 6;
            triangles[triIndex] = v1;
            triangles[triIndex + 1] = v2;
            triangles[triIndex + 2] = v3;

            triangles[triIndex + 3] = v1;
            triangles[triIndex + 4] = v3;
            triangles[triIndex + 5] = v4;
            colors[v1] = colors[v2] = colors[v3] = colors[v4] = color;
            pos = Vector3.zero;
            charCont++;
        }

        foreach (var item in renderedChars)
        {
            
            LoadChar(item.c, item.color,item.matrix);
        }
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.colors = colors;
    }

    void Start()
    {
        SetFont(font);
    }

    //public void UpdateRender(char ch, Font fontc, Color colord)
    //{
    //    if (fontc is null)
    //    {
    //        return;
    //    }
    //    var update = false;
    //    if(ch != c)
    //    {
    //        c = ch;
    //        update = true;
    //    }
    //    if (fontc != font)
    //    {
    //        font = fontc;
    //        update = true;
    //        font.RequestCharactersInTexture(Str(), 355);
    //    }
    //    if (colord != color)
    //    {
    //        color = colord;
    //        update = true;
    //    }
    //    if (update)
    //    {
    //        RebuildMesh();
    //    }
    //}
    Material mit;
    public Shader Shader;
    public void SetFont(Font fontc)
    {
        if (fontc is null)
        {
            return;
        }
        font = fontc;
        // Set the rebuild callback so that the mesh is regenerated on font changes.
        Font.textureRebuilt += OnFontTextureRebuilt;

        // Request characters.
        font.RequestCharactersInTexture(Str(), 355);

        mit = new Material(font.material);
        mit.shader = Shader;
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
        font.RequestCharactersInTexture(Str(), 355);
        RebuildMesh();
    }

    void OnDestroy()
    {
        Font.textureRebuilt -= OnFontTextureRebuilt;
        UnityEngine.Object.Destroy(mit);
    }
}
