using UnityEngine;

namespace Chigiri.MeshHoleShrinker
{

    public class MeshHoleShrinker : MonoBehaviour
    {
        public SkinnedMeshRenderer targetRenderer;
        public Mesh sourceMesh;
        public string newName = "shrink_hole";
        public float scale = 0.5f;
        public Vector3 offset = Vector3.zero;
        public float epsilon = 1e-3f;
        public bool useMeshCollider = false;
        public GameObject meshSnapshotPrefab;
    }

}
