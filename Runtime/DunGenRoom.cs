using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace RanaksDunGen
{
    [Icon("Packages/com.ranakofour.dungen/Editor/Gizmos/DunGenRoomIcon.png")]
    public class DunGenRoom : MonoBehaviour
    {
        [Tooltip("Size in voxels of the part")]
        [SerializeField]
        public Vector3Int m_Size = Vector3Int.one;

        [Tooltip("Offset of the shape from the center of the prefab")]
        [SerializeField]
        public Vector3Int m_Offset = Vector3Int.zero;

        private List<Vector3> m_Coordinates;

        private bool m_Dirty;

        private bool m_Init;

        public void Awake()
        {
            m_Dirty = true;
            m_Init = false;
            Init();
        }

        public void Start()
        {
            m_Dirty = true;
            m_Init = false;
            Init();
        }

        private void Init()
        {
            if (m_Init) return;

            m_Init = true;

            ConnectionPoint[] l_connectionPoints = gameObject.GetComponentsInChildren<ConnectionPoint>();
            for (int i = 0; i < l_connectionPoints.Length; i++)
            {
                l_connectionPoints[i].ID(i);
            }

            //Debug.Log("DunGen: ConnectionIDs Set");
        }

        [ExecuteInEditMode]
        public void OnDrawGizmos()
        {
            GameObject[] l_objects = SceneManager.GetActiveScene().GetRootGameObjects();
            Vector3 l_voxSize = Vector3.one;
            Gizmos.color = Color.red;

            foreach (GameObject obj in l_objects)
            {
                if (obj.TryGetComponent(out DunGenerator _generator))
                {
                    l_voxSize = _generator.GetVoxelSize();
                    Gizmos.color = Color.blue;
                }
            }

            Vector3 l_sizeToDraw = new Vector3(
                m_Size.x / l_voxSize.x,
                m_Size.y / l_voxSize.y,
                m_Size.z / l_voxSize.x
            );

            l_sizeToDraw.x *= l_voxSize.x;
            l_sizeToDraw.y *= l_voxSize.y;
            l_sizeToDraw.z *= l_voxSize.x;

            Gizmos.DrawWireCube(transform.position + m_Offset, l_sizeToDraw);
            Gizmos.color = Color.white;
        }

        public void ReorientSize()
        {
            //string l_debugMessage = "Dungeon Generator: Reorienting Shape of Size " + m_Size;

            Vector3 l_eulers = transform.rotation.eulerAngles;

            //l_debugMessage += "\nEulers: " + l_eulers;

            // Leaving this as a vec3 incase other rotations are wanted later
            Vector3 l_quarterTurns = new Vector3(
                l_eulers.x / 90.0f,
                l_eulers.y / 90.0f,
                l_eulers.z / 90.0f
            );

            // Makes rotation positive for % operation to work
            if (l_eulers.y < 0)
            {
                l_eulers.y += 4.0f;
            }

            //l_debugMessage += "\nQuarter Turns: " + l_quarterTurns;

            // Switch lengths if needed
            switch (Mathf.Round(l_quarterTurns.y))
            {
                case 1:
                    m_Offset.x *= -1;
                    (m_Size.x, m_Size.z) = (m_Size.z, m_Size.x);

                    // Swap offset axis on quarter turns
                    (m_Offset.x, m_Offset.z) = (m_Offset.z, m_Offset.x);
                    break;
                case 3:
                    (m_Size.x, m_Size.z) = (m_Size.z, m_Size.x);
                    // Swap offset axis on quarter turns
                    (m_Offset.x, m_Offset.z) = (m_Offset.z, m_Offset.x);
                    break;

                case 2:
                    m_Offset.x *= -1;
                    break;
            }

            // Switch lengths if needed
            // if (Mathf.Round(l_quarterTurns.y) % 2 == 1)
            // {
            //     (m_Size.x, m_Size.z) = (m_Size.z, m_Size.x);

            //     // Swap offset axis on quarter turns
            //     (m_Offset.x, m_Offset.z) = (m_Offset.z, m_Offset.x);


            //     m_Offset.z *= -1;
            // }

            // // Flip offsets on half turns
            // if (Mathf.Round(l_quarterTurns.y) == 2)
            // {
            //     m_Offset.x *= -1;
            // }

            //l_debugMessage += "\nNew Size: " + m_Size;

            //Debug.Log(l_debugMessage);

            m_Dirty = true;
        }

        public List<Vector3> GetCoordinates(ref Vector3 _center, ref Vector3 _voxelSize)
        {
            string l_debugMessage = "Dungeon Generator: GetCoordinates of object center: " + _center;

            if (m_Dirty)
            {
                m_Coordinates = new List<Vector3>();

                Vector3Int l_adjustedSize = new Vector3Int(
                    m_Size.x / (int)_voxelSize.x,
                    m_Size.y / (int)_voxelSize.y,
                    m_Size.z / (int)_voxelSize.z
                );

                int l_voxelCount = l_adjustedSize.x * l_adjustedSize.y * l_adjustedSize.z;

                Vector3 l_halfSize = (l_adjustedSize + Vector3.one) / 2;
                Vector3Int l_negBound = new Vector3Int(
                    RoundToZero(_center.x - l_halfSize.x),
                    RoundToZero(_center.y - l_halfSize.y),
                    RoundToZero(_center.z - l_halfSize.z)
                );

                l_debugMessage += "\n Lower Bound: " + l_negBound;

                // Iterate through each coordinate in the discrete grid

                for (int x = 0; x < l_adjustedSize.x; x++)
                    for (int y = 0; y < l_adjustedSize.y; y++)
                        for (int z = 0; z < l_adjustedSize.z; z++)
                        {
                            Vector3Int localPos = new Vector3Int(x, y, z);
                            Vector3Int worldPos = l_negBound + localPos;

                            l_debugMessage += "\n" + worldPos;
                            m_Coordinates.Add(worldPos);
                        }


                if (m_Coordinates.Count != l_voxelCount)
                {
                    int l_diff = (int)l_voxelCount - m_Coordinates.Count;
                    l_debugMessage = "Dungeon Generator: " + gameObject.name + " Failed Coordinate Count by " + l_diff + "\n" + l_debugMessage;
                    Debug.LogWarning(l_debugMessage);
                }

                m_Dirty = false;
            }

            //Debug.Log(l_debugMessage);

            return m_Coordinates;
        }

        // Rounds numbers down towards zero i.e. 3.5 -> 3.0, -3.5 -> -3.0
        private int RoundToZero(float value)
        {
            return (int)(value < 0 ? Mathf.Floor(value + 0.5f) : Mathf.Ceil(value - 0.5f));
        }
    }
}