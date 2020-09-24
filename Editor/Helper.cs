using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class Helper
{

    public static string Chomp(string s)
    {
        if (s.EndsWith("\n")) return s.Substring(0, s.Length - 1);
        return s;
    }

    public static string SanitizeFileName(string name)
    {
        var reg = new Regex("[\\/:\\*\\?<>\\|\\\"]");
        return reg.Replace(name, "_");
    }

    // From https://forum.unity.com/threads/bakemesh-scales-wrong.442212/#post-2860559
    public static Mesh GetPosedMesh(SkinnedMeshRenderer skin, Mesh mesh)
    {
        float epsilon = 0.00001f;
        var vertices = mesh.vertices;
        var newVertices = new Vector3[vertices.Length];

        for (int i = 0; i < vertices.Length; i++)
        {
            var w = mesh.boneWeights[i];

            if (epsilon < Mathf.Abs(w.weight0))
            {
                var p = mesh.bindposes[w.boneIndex0].MultiplyPoint3x4(vertices[i]);
                var q = skin.bones[w.boneIndex0].transform.localToWorldMatrix.MultiplyPoint3x4(p);
                newVertices[i] += skin.transform.InverseTransformPoint(q) * w.weight0;
            }
            if (epsilon < Mathf.Abs(w.weight1))
            {
                var p = mesh.bindposes[w.boneIndex1].MultiplyPoint3x4(vertices[i]);
                var q = skin.bones[w.boneIndex1].transform.localToWorldMatrix.MultiplyPoint3x4(p);
                newVertices[i] += skin.transform.InverseTransformPoint(q) * w.weight1;
            }
            if (epsilon < Mathf.Abs(w.weight2))
            {
                var p = mesh.bindposes[w.boneIndex2].MultiplyPoint3x4(vertices[i]);
                var q = skin.bones[w.boneIndex2].transform.localToWorldMatrix.MultiplyPoint3x4(p);
                newVertices[i] += skin.transform.InverseTransformPoint(q) * w.weight2;
            }
            if (epsilon < Mathf.Abs(w.weight3))
            {
                var p = mesh.bindposes[w.boneIndex3].MultiplyPoint3x4(vertices[i]);
                var q = skin.bones[w.boneIndex3].transform.localToWorldMatrix.MultiplyPoint3x4(p);
                newVertices[i] += skin.transform.InverseTransformPoint(q) * w.weight3;
            }
        }

        var result = new Mesh();
        result.vertices = newVertices;
        result.triangles = skin.sharedMesh.triangles;
        result.RecalculateBounds();
        return result;
    }

}
