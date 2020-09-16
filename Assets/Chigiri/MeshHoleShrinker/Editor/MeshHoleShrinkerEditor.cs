using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace Chigiri.MeshHoleShrinker.Editor
{

    [CustomEditor(typeof(MeshHoleShrinker))]
    public class MeshHoleShrinkerEditor : UnityEditor.Editor
    {

        SerializedProperty targetRenderer;
        SerializedProperty sourceMesh;
        SerializedProperty newName;
        SerializedProperty scale;
        SerializedProperty offset;
        SerializedProperty epsilon;
        SerializedProperty useMeshCollider;
        SerializedProperty meshSnapshotPrefab;

        SkinnedMeshRenderer prevTargetRenderer;
        bool isAdvancedOpen;
        Collider collider;
        float sqrEpsilon;

        [MenuItem("Chigiri/Create MeshHoleShrinker")]
        public static void CreateMeshHoleShrinker()
        {
            var path = AssetDatabase.GUIDToAssetPath("272fdb9e09fc6d34e89748854b0ff3a1");
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            instance.transform.SetAsLastSibling();
            Undo.RegisterCreatedObjectUndo(instance, "Create MeshHoleShrinker");
        }

        private void OnEnable()
        {
            targetRenderer = serializedObject.FindProperty("targetRenderer");
            sourceMesh = serializedObject.FindProperty("sourceMesh");
            newName = serializedObject.FindProperty("newName");
            scale = serializedObject.FindProperty("scale");
            offset = serializedObject.FindProperty("offset");
            epsilon = serializedObject.FindProperty("epsilon");
            useMeshCollider = serializedObject.FindProperty("useMeshCollider");
            meshSnapshotPrefab = serializedObject.FindProperty("meshSnapshotPrefab");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Target を変更したときに Source Mesh が空なら自動設定
            if (prevTargetRenderer != self.targetRenderer && self.targetRenderer != null && self.sourceMesh == null)
            {
                self.sourceMesh = self.targetRenderer.sharedMesh;
            }
            prevTargetRenderer = self.targetRenderer;

            // バリデーション
            var error = "";
            error += ValidateTargetRenderer();
            error += ValidateSourceMesh();
            error += ValidateMeshSnapshotPrefab();
            error += ValidateNewName();

            // UI描画

            EditorGUILayout.PropertyField(targetRenderer, new GUIContent("Target", "操作対象のSkinnedMeshRenderer"));
            EditorGUILayout.PropertyField(sourceMesh, new GUIContent("Source Mesh", "オリジナルのメッシュ"));
            EditorGUILayout.PropertyField(newName, new GUIContent("New Shape Key Name", "新しく追加するシェイプキーの名前"));
            EditorGUILayout.Slider(scale, 0f, 1f, new GUIContent("Scale", "穴のサイズ比率（0で完全にふさがります）"));
            EditorGUILayout.PropertyField(offset, new GUIContent("Offset", "穴の中心点を偏らせるオフセット量"));

            isAdvancedOpen = EditorGUILayout.Foldout(isAdvancedOpen, "Advanced");
            if (isAdvancedOpen)
            {
                EditorGUI.indentLevel++;
                {
                    EditorGUILayout.Slider(epsilon, 1e-5f, 0.1f, new GUIContent("Epsilon"));
                    EditorGUILayout.PropertyField(useMeshCollider, new GUIContent("Use MeshCollider"));
                    EditorGUILayout.PropertyField(meshSnapshotPrefab, new GUIContent("MeshSnapshot (DO NOT EDIT)"));
                }
                EditorGUI.indentLevel--;
            }

            if (error != "")
            {
                EditorGUILayout.HelpBox(Chomp(error), MessageType.Error, true);
            }

            var isRevertTargetEnable = self.targetRenderer != null && self.sourceMesh != null;
            if (isRevertTargetEnable)
            {
                EditorGUILayout.HelpBox("Undo 時にメッシュが消えた場合は Revert Target ボタンを押してください。", MessageType.Info, true);
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.BeginDisabledGroup(error != "");
                {
                    if (GUILayout.Button(new GUIContent("Process And Save As...", "新しいメッシュを生成し、保存ダイアログを表示します。"))) DoProcess();
                }
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(!isRevertTargetEnable);
                {
                    if (GUILayout.Button(new GUIContent("Revert Target", "Target の SkinnedMeshRenderer にアタッチされていたメッシュを元に戻します。"))) RevertTarget();
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }

        string ValidateTargetRenderer()
        {
            if (self.targetRenderer == null)
            {
                return "Target に操作対象となる SkinnedMeshRenderer を指定してください。\n";
            }
            return "";
        }

        string ValidateSourceMesh()
        {
            if (self.sourceMesh == null)
            {
                return "Source Mesh に操作対象となるメッシュを指定してください（通常は Target の SkinnedMeshRenderer にもともとアタッチされていたメッシュです）。\n";
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
            if (self.sourceMesh != null && 0 <= self.sourceMesh.GetBlendShapeIndex(self.newName))
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

        void DoProcess()
        {
            // 新しいメッシュを作成
            if (self.sourceMesh == null) return;
            var resultMesh = AddBlendShape(self.sourceMesh);
            if (resultMesh == null) return;
            resultMesh.name = self.sourceMesh.name + ".HoleShrinkable";

            // 保存ダイアログを表示
            string dir = AssetDatabase.GetAssetPath(self.targetRenderer.sharedMesh);
            if (dir == "") dir = AssetDatabase.GetAssetPath(self.sourceMesh);
            dir = dir == "" ? "Assets" : Path.GetDirectoryName(dir);
            string path = EditorUtility.SaveFilePanel("Save the new mesh as", dir, SanitizeFileName(resultMesh.name), "asset");
            if (path.Length == 0) return;

            // 保存
            if (!path.StartsWith(Application.dataPath))
            {
                Debug.LogError("Invalid path: Path must be under " + Application.dataPath);
                return;
            }
            path = path.Replace(Application.dataPath, "Assets");
            AssetDatabase.CreateAsset(resultMesh, path);
            Debug.Log("Asset exported: " + path);

            // Targetのメッシュを差し替えてシェイプキーのウェイトを設定
            Undo.RecordObject(self.targetRenderer, "Process (MeshHoleShrinker)");
            self.targetRenderer.sharedMesh = resultMesh;
            int index = resultMesh.GetBlendShapeIndex(self.newName);
            self.targetRenderer.SetBlendShapeWeight(index, 100f);
            // Selection.activeGameObject = self.targetRenderer.gameObject;
        }

        bool HitTest(Vector3 p)
        {
            Vector3 q;

            if (self.useMeshCollider)
            {
                q = collider.ClosestPoint(p);
                return (q - p).sqrMagnitude < sqrEpsilon;
            }

            q = self.transform.InverseTransformPoint(p);
            if (1f < Mathf.Abs(q.y)) return false;
            return Mathf.Sqrt(q.x * q.x + q.z * q.z) <= 0.5f;
        }

        // From https://forum.unity.com/threads/bakemesh-scales-wrong.442212/#post-2860559
        static Mesh GetPosedMesh(SkinnedMeshRenderer skin)
        {
            float MIN_VALUE = 0.00001f;

            Mesh mesh = new Mesh();
            Mesh sharedMesh = skin.sharedMesh;

            GameObject root = skin.gameObject;

            Vector3[] vertices = sharedMesh.vertices;
            Matrix4x4[] bindposes = sharedMesh.bindposes;
            BoneWeight[] boneWeights = sharedMesh.boneWeights;
            Transform[] bones = skin.bones;
            Vector3[] newVert = new Vector3[vertices.Length];

            Vector3 localPt;

            for (int i = 0; i < vertices.Length; i++)
            {
                BoneWeight bw = boneWeights[i];

                if (Mathf.Abs(bw.weight0) > MIN_VALUE)
                {
                    localPt = bindposes[bw.boneIndex0].MultiplyPoint3x4(vertices[i]);
                    newVert[i] +=
                        root.transform.InverseTransformPoint
                            (
                        bones[bw.boneIndex0].transform.localToWorldMatrix.MultiplyPoint3x4(localPt)) * bw.weight0;
                }
                if (Mathf.Abs(bw.weight1) > MIN_VALUE)
                {
                    localPt = bindposes[bw.boneIndex1].MultiplyPoint3x4(vertices[i]);
                    newVert[i] +=
                        root.transform.InverseTransformPoint
                            (
                        bones[bw.boneIndex1].transform.localToWorldMatrix.MultiplyPoint3x4(localPt)) * bw.weight1;
                }
                if (Mathf.Abs(bw.weight2) > MIN_VALUE)
                {
                    localPt = bindposes[bw.boneIndex2].MultiplyPoint3x4(vertices[i]);
                    newVert[i] += root.transform.InverseTransformPoint
                        (
                        bones[bw.boneIndex2].transform.localToWorldMatrix.MultiplyPoint3x4(localPt)) * bw.weight2;
                }
                if (Mathf.Abs(bw.weight3) > MIN_VALUE)
                {
                    localPt = bindposes[bw.boneIndex3].MultiplyPoint3x4(vertices[i]);
                    newVert[i] +=
                        root.transform.InverseTransformPoint
                            (
                        bones[bw.boneIndex3].transform.localToWorldMatrix.MultiplyPoint3x4(localPt)) * bw.weight3;
                }

            }

            mesh.vertices = newVert;
            mesh.triangles = skin.sharedMesh.triangles;
            mesh.RecalculateBounds();
            return mesh;
        }


        Mesh AddBlendShape(Mesh baseMesh)
        {
            collider = self.transform.Find("Cylinder").GetComponent<MeshCollider>();
            sqrEpsilon = self.epsilon * self.epsilon;

            // SkinnedMeshRenderer の現在の形状を MeshRenderer に変換
            var snapshot = Instantiate(self.meshSnapshotPrefab).GetComponent<MeshFilter>();
            snapshot.transform.parent = null;
            snapshot.transform.position = self.targetRenderer.transform.position;
            snapshot.transform.rotation = self.targetRenderer.transform.rotation;
            snapshot.transform.localScale = self.targetRenderer.transform.lossyScale;
            var snapshotMesh = GetPosedMesh(self.targetRenderer);
            snapshot.sharedMesh = snapshotMesh;

            // 範囲内の点群の中心座標を算出
            var center = Vector3.zero;
            var pointNum = 0;
            for (var j = 0; j < baseMesh.vertexCount; j++)
            {
                var p = snapshot.transform.TransformPoint(snapshotMesh.vertices[j]);
                if (HitTest(p))
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
                if (HitTest(p))
                {
                    vertices[j] = (center - baseMesh.vertices[j]) * (1f - self.scale) + self.offset;
                }
            }
            ret.AddBlendShapeFrame(self.newName, 100f, vertices, null, null);

            DestroyImmediate(snapshot.gameObject);
            return ret;
        }

        void RevertTarget()
        {
            Undo.RecordObject(self.targetRenderer, "Revert Target (MeshHoleShrinker)");
            self.targetRenderer.sharedMesh = self.sourceMesh;
        }

    }

}
