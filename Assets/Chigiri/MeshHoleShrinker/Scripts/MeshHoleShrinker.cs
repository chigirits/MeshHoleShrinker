using UnityEngine;

namespace Chigiri.MeshHoleShrinker
{

    public class MeshHoleShrinker : MonoBehaviour
    {
        public SkinnedMeshRenderer targetMesh;
        public string newName = "shrink_hole";
        public float scale = 0.5f;
        public float epsilon = 1e-3f;
        public GameObject meshSnapshotPrefab;
    }

}
