using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SharedModels;
using RNumerics;
using RhuEngine.AssetSystem;
namespace UnityEngine
{
    public class UnitTester : MonoBehaviour
    {
        public Text Output;
        public InputField InputField;

        public void SetOutPutText(string data)
        {
            Output.text = data;
        }

        public void UnityMeshTest()
        {
            var faceAmount = int.Parse(InputField.text);
            SetOutPutText($"Loading mesh with {faceAmount} faces");
            var mesh = new Mesh();
            //This was the bloody problem the hole time wawasawswadaw duiawshbniuewsn
            mesh.indexFormat = Rendering.IndexFormat.UInt32;
            //aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa
            var vernts = new Vector3[faceAmount * 4];
            var indexes = new int[faceAmount * 3 * 2];
            var halfofface = (int)Mathf.Sqrt(faceAmount);
            var x = 0;
            var y = 0;
            for (int i = 0; i < faceAmount; i++)
            {
                if(x == halfofface)
                {
                    x = 0;
                    y++;
                }
                x++;
                var index = i*6;
                var vertIndex = i * 4;
                var pos = new Vector3(x,y) * 2;
                vernts[vertIndex] = pos + new Vector3(0.5f, 0.5f);
                vernts[vertIndex+2] = pos + new Vector3(-0.5f, 0.5f);
                vernts[vertIndex+1] = pos + new Vector3(0.5f, -0.5f);
                vernts[vertIndex+3] = pos + new Vector3(-0.5f, -0.5f);
                indexes[index] = vertIndex;
                indexes[index + 1] = vertIndex + 1;
                indexes[index + 2] = vertIndex + 2;
                indexes[index + 3] = vertIndex + 3;
                indexes[index + 4] = vertIndex + 2;
                indexes[index + 5] = vertIndex + 1;
            }
            mesh.vertices = vernts;
            mesh.SetIndices(indexes, MeshTopology.Triangles, 0);
            var rootgameobject = new GameObject("TestMesh");
            rootgameobject.AddComponent<MeshRenderer>();
            rootgameobject.AddComponent<MeshFilter>().mesh = mesh;
        }

        public void TestSaving()
        {
            SetOutPutText("Saving Test Started");
            SetOutPutText("Building Test Data");
            var mesh = new ComplexMesh();
            var testAmount = ushort.MaxValue + 5000;
            for (int i = 0; i < testAmount; i+= 4)
            {
                var face = new RFace();
                face.Indices.Add(i);
                face.Indices.Add(i+1);
                face.Indices.Add(i+2);
                face.Indices.Add(i+3);
                mesh.Vertices.Add(new Vector3f(i,i,i));
                mesh.Vertices.Add(new Vector3f(i+1, i+1, i+1));
                mesh.Vertices.Add(new Vector3f(i+2, i+2, i+2));
                mesh.Vertices.Add(new Vector3f(i+3, i+3, i+3));
                mesh.Tangents.Add(new Vector3f(i, i, i));
                mesh.Tangents.Add(new Vector3f(i + 1, i + 1, i + 1));
                mesh.Tangents.Add(new Vector3f(i + 2, i + 2, i + 2));
                mesh.Tangents.Add(new Vector3f(i + 3, i + 3, i + 3));
                mesh.BiTangents.Add(new Vector3f(i, i, i));
                mesh.BiTangents.Add(new Vector3f(i + 1, i + 1, i + 1));
                mesh.BiTangents.Add(new Vector3f(i + 2, i + 2, i + 2));
                mesh.BiTangents.Add(new Vector3f(i + 3, i + 3, i + 3));
                mesh.Faces.Add(face);
            }
            SetOutPutText("Done Building Test Data");
            var testdata = CustomAssetManager.SaveAsset(mesh, "TestAsset");
            var asset = CustomAssetManager.GetCustomAsset<ComplexMesh>(testdata);
            SetOutPutText("Reading Test Data after saving it");
            var checkMesh = asset;
            var currentIndex = 0;
            var Failed = false;
            foreach (var item in checkMesh.Faces)
            {
                if (Failed)
                {
                    break;
                }
                foreach (var indexs in item.Indices)
                {
                    if(indexs != currentIndex)
                    {
                        Failed = true;
                        Debug.LogError("Mesh is invalied");
                        break;
                    }
                    currentIndex++;
                }
            }
            if (Failed)
            {
                SetOutPutText($"Test Failed at index {currentIndex}");
            }
            else
            {
                SetOutPutText("Test is good");
            }
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}