using System;
using UnityEngine;

namespace VRCapture {
    /// <summary>
    /// Serializable Vector2 to replace Unity Vector2 struct.
    /// </summary>
    [Serializable]
    public struct SVector2 {
        public float x;
        public float y;

        public SVector2(Vector2 v2) {
            x = v2.x;
            y = v2.y;
        }

        public Vector2 V2 { get { return new Vector2(x, y); } }
    }
    /// <summary>
    /// Serializable Vector3 to replace Unity Vector3 struct.
    /// </summary>
    [Serializable]
    public struct SVector3 {
        public float x;
        public float y;
        public float z;

        public SVector3(Vector3 v3) {
            x = v3.x;
            y = v3.y;
            z = v3.z;
        }

        public Vector3 V3 { get { return new Vector3(x, y, z); } }
    }
    /// <summary>
    /// Serializable Quaternion to replace Unity Quaternion struct.
    /// </summary>
    [Serializable]
    public struct SQuaternion {
        public float x;
        public float y;
        public float z;
        public float w;

        public SQuaternion(Quaternion q) {
            x = q.x;
            y = q.y;
            z = q.z;
            w = q.w;
        }

        public Quaternion Q { get { return new Quaternion(x, y, z, w); } }
    }
}