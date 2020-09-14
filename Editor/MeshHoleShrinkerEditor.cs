using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace Chigiri.MeshHoleShrinker.Editor
{

    [CustomEditor(typeof(MeshHoleShrinker))]
    public class MeshHoleShrinkerEditor : UnityEditor.Editor
    {

        SerializedProperty targetMesh;
        SerializedProperty newName;
        SerializedProperty scale;
        SerializedProperty epsilon;
        SerializedProperty meshSnapshotPrefab;

        bool isAdvancedOpen;

        private void OnEnable()
        {
            targetMesh = serializedObject.FindProperty("targetMesh");
            newName = serializedObject.FindProperty("newName");
            scale = serializedObject.FindProperty("scale");
            epsilon = serializedObject.FindProperty("epsilon");
            meshSnapshotPrefab = serializedObject.FindProperty("meshSnapshotPrefab");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(targetMesh, new GUIContent("Target"));
            EditorGUILayout.PropertyField(newName, new GUIContent("New Shape Key Name"));
            EditorGUILayout.Slider(scale, 0f, 1f, new GUIContent("Scale"));
            isAdvancedOpen = EditorGUILayout.Foldout(isAdvancedOpen, "Advanced");
            if (isAdvancedOpen)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Slider(epsilon, 1e-5f, 0.1f, new GUIContent("Epsilon"));
                EditorGUILayout.PropertyField(meshSnapshotPrefab, new GUIContent("MeshSnapshot (DO NOT EDIT)"));
                EditorGUI.indentLevel--;
            }

            var error = "";
            error += ValidateTargetMesh();
            error += ValidateMeshSnapshotPrefab();
            error += ValidateNewName();
            if (error != "") EditorGUILayout.HelpBox(Chomp(error), MessageType.Error, true);

            EditorGUI.BeginDisabledGroup(error != "");
            EditorGUILayout.Space();
            if (GUILayout.Button("Process And Save As..."))
            {
                DoProcess();
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();
        }

        string ValidateTargetMesh()
        {
            if (self.targetMesh == null)
            {
                return "Target に操作対象となる SkinnedMeshRenderer を指定してください。\n";
            }
            return "";
        }

        string ValidateMeshSnapshotPrefab()
        {
            if (self.meshSnapshotPrefab == null)
            {
                return "MeshSnapshot のラベル部分を右クリックし、Revert してください。\n";
            }
            return "";
        }

        string ValidateNewName()
        {
            if (newName.stringValue == "")
            {
                return "New Shape Key Name に新しいシェイプキーの名前を指定してください。\n";
            }
            var baseMesh = getBaseMesh();
            if (baseMesh != null && 0 <= baseMesh.GetBlendShapeIndex(self.newName))
            {
                return "New Shape Key Name に指定されているシェイプキーは既に存在します。別の名前を指定してください。\n";
            }
            return "";
        }

        MeshHoleShrinker self
        {
            get { return target as MeshHoleShrinker; }
        }

        static string SanitizeFileName(string name)
        {
            var reg = new Regex("[\\/:\\*\\?<>\\|\\\"]");
            return reg.Replace(name, "_");
        }

        static string Chomp(string s)
        {
            if (s.EndsWith("\n")) return s.Substring(0, s.Length - 1);
            return s;
        }

        Mesh getBaseMesh()
        {
            if (self.targetMesh == null)
            {
                Debug.LogError("Target is not selected");
                return null;
            }
            var baseMesh = self.targetMesh.sharedMesh;
            if (baseMesh == null)
            {
                Debug.LogError("Target has no valid mesh");
                return null;
            }
            return baseMesh;
        }

        void DoProcess()
        {
            var baseMesh = getBaseMesh();
            if (baseMesh == null) return;
            var result = AddBlendShape(baseMesh);
            if (result == null) return;

            string path = EditorUtility.SaveFilePanel("Save the new mesh as", "Assets", SanitizeFileName(baseMesh.name), "asset");
            if (path.Length == 0) return;

            var dataPath = Application.dataPath;
            if (!path.StartsWith(dataPath))
            {
                Debug.LogError("Invalid path: Path must be under " + dataPath);
                return;
            }

            path = path.Replace(dataPath, "Assets");
            AssetDatabase.CreateAsset(result, path);
            Debug.Log("Asset exported: " + path);

            // Targetのメッシュを差し替えてシェイプキーのウェイトを設定
            Undo.RecordObject(self.targetMesh, "MeshHoleShrinker");
            self.targetMesh.sharedMesh = result;
            int index = result.GetBlendShapeIndex(self.newName);
            self.targetMesh.SetBlendShapeWeight(index, 100f);
            Selection.activeGameObject = self.targetMesh.gameObject;
        }

        Mesh AddBlendShape(Mesh baseMesh)
        {
            var sqrEpsilon = self.epsilon * self.epsilon;

            // SkinnedMeshRenderer の現在の形状を MeshRenderer に変換
            var snapshot = Instantiate(self.meshSnapshotPrefab).GetComponent<MeshFilter>();
            snapshot.transform.parent = null;
            snapshot.transform.position = self.targetMesh.transform.position;
            snapshot.transform.rotation = self.targetMesh.transform.rotation;
            snapshot.transform.localScale = Vector3.one;
            var snapshotMesh = new Mesh();
            snapshot.sharedMesh = snapshotMesh;
            self.targetMesh.BakeMesh(snapshotMesh);

            // 範囲内の点群の中心座標を算出
            var collider = self.transform.Find("Cylinder").GetComponent<MeshCollider>();
            var center = Vector3.zero;
            var pointNum = 0;
            for (var j = 0; j < baseMesh.vertexCount; j++)
            {
                var p = snapshot.transform.TransformPoint(snapshotMesh.vertices[j]);
                var q = collider.ClosestPoint(p);
                var d2 = (q - p).sqrMagnitude;
                if (d2 < sqrEpsilon)
                {
                    center += baseMesh.vertices[j];
                    pointNum++;
                }
            }
            if (0 < pointNum) center /= pointNum;

            // 新しいシェイプキーを作成
            Mesh ret = Instantiate(baseMesh);
            ret.name = baseMesh.name;
            var src = Instantiate(ret);
            var vertices = new Vector3[baseMesh.vertexCount];
            for (var j = 0; j < baseMesh.vertexCount; j++)
            {
                var p = snapshot.transform.TransformPoint(snapshotMesh.vertices[j]);
                var q = collider.ClosestPoint(p);
                var d2 = (q - p).sqrMagnitude;
                if (d2 < sqrEpsilon)
                {
                    vertices[j] = (center - baseMesh.vertices[j]) * (1f - self.scale);
                }
            }
            ret.AddBlendShapeFrame(self.newName, 100f, vertices, null, null);

            DestroyImmediate(snapshot.gameObject);
            return ret;
        }

    }

}
