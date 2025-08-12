using System;
using System.Collections.Generic;
using UnityEngine;

namespace RanaksDunGen
{
    [Serializable]
    struct DungeonPartContainer
    {
        [Tooltip("The prefab to be used for this dungeon part")]
        [SerializeField]
        public GameObject m_Prefab;

        [Tooltip("How many times this piece can be placed into the dungeon")]
        [SerializeField]
        public int m_MaxIterations;
    }

    // Holds static data about the dungeon
    public class DungeonGenerator : MonoBehaviour
    {
        [Tooltip("Size of individual voxels that compose the dungeon parts")]
        [SerializeField]
        public Vector3 m_VoxelSize = Vector3.one;

        [Tooltip("Size of the whole dungeon space in voxels")]
        [SerializeField]
        private Vector3 m_DungeonSize = Vector3.one;

        [Tooltip("The piece that is placed down first in the center of the dungeon")]
        [SerializeField]
        private GameObject m_StartingRoom;

        [Tooltip("Unique prefabs that will make up the dungeon")]
        [SerializeField]
        private List<DungeonPartContainer> m_DungeonParts;

        private void Awake()
        {
            
        }

        private void Start()
        {
            
        }

        /// <summary>
        /// Generates a dungeon from prefabs in the DungeonParts list.
        /// </summary>
        public void Generate()
        {
            if (m_StartingRoom == null)
            {
                Debug.Log("Dungeon Generator: no starting room set!");
                return;
            }

            if (m_VoxelSize == Vector3.zero)
            {
                Debug.LogWarning("Dungeon Generator: No voxel size specified!");
                return;
            }

            if (m_DungeonSize == Vector3.zero)
            {
                Debug.LogWarning("Dungeon Generator: No dungeon size specified!");
                return;
            }

            // Destroy previously created dungeon right now
            while (transform.childCount > 0)
            {
                GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
            }

            Debug.Log("Dungeon Generator: Starting generation");

            // Copy m_DungeonParts
            List<DungeonPartContainer> l_dungeonParts = new List<DungeonPartContainer>(m_DungeonParts);

            // I tried adding an 'm_Iterations' property to DungeonParts but it wouldn't work for some reason when changing the value
            // MAXITERATION FAULT
            Dictionary<GameObject, int> l_IterationCounts = new Dictionary<GameObject, int>();
            foreach (DungeonPartContainer dp in l_dungeonParts)
            {
                l_IterationCounts.Add(dp.m_Prefab, 0);
            }

            // Using this to check if a prefab is already occupying a coordinate in the voxel space

            //bool[,,] l_VoxelMap = new bool[(int)m_DungeonSize.x, (int)m_DungeonSize.y, (int)m_DungeonSize.z];

            // More memory efficient than aformentioned bool[,,]
            List<Vector3> l_VoxelMap = new List<Vector3>();

            // New objects created are added to the queue to have new pieces joined to them
            Queue<GameObject> l_partQueue = new Queue<GameObject>();

            GameObject l_instantiatedPiece = GameObject.Instantiate(m_StartingRoom, gameObject.transform);
            l_partQueue.Enqueue(l_instantiatedPiece);
            FillMap(ref l_instantiatedPiece, ref l_VoxelMap);

            // Place down new parts until we run out of parts to place or we run out of connections to put them on
            while (l_partQueue.Count > 0 && l_dungeonParts.Count > 0)
            {
                GameObject l_currentPiece = l_partQueue.Dequeue();
                ConnectionPoint[] l_connectionPoints = l_currentPiece.GetComponentsInChildren<ConnectionPoint>();

                foreach (ConnectionPoint l_currentPoint in l_connectionPoints)
                {
                    // If we run out of parts midway through the foreach
                    if (l_dungeonParts.Count == 0) { l_currentPoint.Hide(); break; }

                    // Skip connected points and exit
                    if (l_currentPoint.Connected()) { l_currentPoint.Hide(); continue; }


                    bool l_partFits = false;
                    // Prevent deadlocking
                    int l_unsuccessfulFits = 0;

                    while (!l_partFits && l_unsuccessfulFits < 6)
                    {
                        // Cycle through parts randomly until one that can be placed is found
                        int l_newPieceIndex = UnityEngine.Random.Range(0, l_dungeonParts.Count);

                        GameObject l_newPiece = GameObject.Instantiate(l_dungeonParts[l_newPieceIndex].m_Prefab, gameObject.transform);
                        ConnectionPoint l_newPoint = l_newPiece.GetComponentInChildren<ConnectionPoint>();

                        // Using Quaternion.AngleAxis messes with l_newPiece transform, and that isn't good
                        l_newPiece.transform.Rotate(Vector3.up, l_currentPoint.transform.eulerAngles.y - l_newPoint.transform.eulerAngles.y + 180f);
                        l_newPiece.GetComponent<DungeonPart>().ReorientSize();

                        // Accounts for difference in overlapping pieces
                        Vector3 l_translate = l_currentPoint.transform.position - l_newPoint.transform.position;
                        l_newPiece.transform.position += l_translate;

                        if (DoesObjectFit(ref l_newPiece, ref l_VoxelMap))
                        {
                            l_partFits = true;
                            FillMap(ref l_newPiece, ref l_VoxelMap);
                            l_newPoint.Connect();
                            l_currentPoint.Connect();
                            l_partQueue.Enqueue(l_newPiece);

                            //Increment number of instances and remove from list if the maximum number if instances is reached
                            l_IterationCounts[l_dungeonParts[l_newPieceIndex].m_Prefab]++;
                            if (l_IterationCounts[l_dungeonParts[l_newPieceIndex].m_Prefab] == l_dungeonParts[l_newPieceIndex].m_MaxIterations)
                            {
                                l_dungeonParts.Remove(l_dungeonParts[l_newPieceIndex]);
                            }
                        }
                        else
                        {
                            GameObject.Destroy(l_newPiece);
                            l_unsuccessfulFits += 1;
                        }
                    }

                    l_currentPoint.Hide();
                }
            }

            // Clear all remaining conenction points
            if (l_partQueue.Count > 0)
            {
                while (l_partQueue.Count > 0)
                {
                    GameObject l_part = l_partQueue.Dequeue();
                    ConnectionPoint[] l_points = l_part.GetComponentsInChildren<ConnectionPoint>();

                    // NOO DON'T ABBREVIATE CONNECTIONPOINT!!
                    foreach (ConnectionPoint cp in l_points)
                    {
                        cp.Hide();
                    }
                }
            }

            Debug.Log("Dungeon Generator: Finished!");
        }
        
        /// <summary>
        /// Test method so see if prefabs pass the boundary check.
        /// </summary>
        public void BoundCheck()
        {
            int i = 0;
            Vector3 l_center = Vector3.zero;

            while (l_center.magnitude < 100)
            {
                // Check each piece for 
                for (int j = 0; j < m_DungeonParts.Count; j++)
                {
                    GameObject l_newPiece = GameObject.Instantiate(m_DungeonParts[j].m_Prefab, gameObject.transform);
                    DungeonPart l_prefab = l_newPiece.GetComponent<DungeonPart>();
                    l_prefab.ReorientSize();

                    l_center -= Vector3.one * 0.5f;
                    l_prefab.GetCoordinates(l_center);

                    GameObject.Destroy(l_newPiece);
                }

                i++;
                if (i % 2 == 0)
                {
                    l_center = -l_center;
                }
                else
                {
                    if (l_center.x > 0)
                    {
                        l_center += Vector3.one * 0.5f;
                    }
                    else
                    {
                        l_center -= Vector3.one * 0.5f;
                    }
                }
            }
        }

        private bool DoesObjectFit(ref GameObject _dungeonPart, ref List<Vector3> _VoxelMap)
        {
            DungeonPart l_prefab = _dungeonPart.GetComponent<DungeonPart>();
            Vector3 l_shapeCenter = new Vector3(
                                           _dungeonPart.transform.position.x / m_VoxelSize.x,
                                           _dungeonPart.transform.position.y / m_VoxelSize.y,
                                           _dungeonPart.transform.position.z / m_VoxelSize.z
                                           );
            
            List<Vector3> l_prefabCoords = l_prefab.GetCoordinates(l_shapeCenter);
            Vector3 l_currentCoord;
            foreach (Vector3 coordinate in l_prefabCoords)
            {
                l_currentCoord = new Vector3(
                    (int)(coordinate.x + (m_DungeonSize.x * 0.5f)),
                    (int)(coordinate.y + (m_DungeonSize.y * 0.5f)),
                    (int)(coordinate.z + (m_DungeonSize.z * 0.5f))
                );

                bool l_xCheck = l_currentCoord.x < 0 || l_currentCoord.x >= m_DungeonSize.x;
                bool l_yCheck = l_currentCoord.y < 0 || l_currentCoord.y >= m_DungeonSize.y;
                bool l_zCheck = l_currentCoord.z < 0 || l_currentCoord.z >= m_DungeonSize.z;

                if (l_xCheck ||
                    l_yCheck ||
                    l_zCheck)
                {
                    //Debug.Log("Dungeon Generator: Object does not fit. Error coord: " + l_currentCoord);
                    return false;
                }

                Debug.Log("Voxel map check at: " + l_currentCoord[0] + ", " + l_currentCoord[1] + ", " + l_currentCoord[2]);
                if (_VoxelMap.Contains(l_currentCoord))
                {
                    //Debug.Log("Dungeon Generator: Object overlaps another. Error coord: " + l_currentCoord);
                    return false;
                }
            }

            //Debug.Log("Dungeon Generator: Object fits");
            return true;
        }

        private void FillMap(ref GameObject _dungeonPart, ref List<Vector3> _VoxelMap)
        {
            DungeonPart l_prefab = _dungeonPart.GetComponent<DungeonPart>();

            // Voxel position of prefabs center
            Vector3 l_shapeCenter = new Vector3(
                                           _dungeonPart.transform.position.x / m_VoxelSize.x,
                                           _dungeonPart.transform.position.y / m_VoxelSize.y,
                                           _dungeonPart.transform.position.z / m_VoxelSize.z
                                           );
            List<Vector3> l_prefabCoords = l_prefab.GetCoordinates(l_shapeCenter);

            Vector3 l_currentCoord;
            for (int i = 0; i < l_prefabCoords.Count; i++)
            {
                l_currentCoord = new Vector3(
                    (int)(l_prefabCoords[i].x + (m_DungeonSize.x * 0.5f)),
                    (int)(l_prefabCoords[i].y + (m_DungeonSize.y * 0.5f)),
                    (int)(l_prefabCoords[i].z + (m_DungeonSize.z * 0.5f))
                );

                _VoxelMap.Add(l_currentCoord);
                Debug.Log("Dungeon Generator: Position filled: (" + l_currentCoord[0] + ", " + l_currentCoord[1] + ", " + l_currentCoord[2] + ")");
            }
        }

        private void PrintVoxelMap(ref bool[,,] _VoxelMap)
        {
            string message = "Dungeon Generator: Voxel Map Output:\n";

            for (int y = 0; y < m_DungeonSize.y; y++)
            {
                message += "For y = " + y + ":\n";
                for (int x = 0; x < m_DungeonSize.x; x++)
                {
                    for (int z = 0; z < m_DungeonSize.z; z++)
                    {
                        if (_VoxelMap[x, y, z])
                        {
                            message += "1  ";
                        }
                        else
                        {
                            message += "0 ";
                        }
                    }
                    message += "\n";
                }
            }

            Debug.Log(message);
        }
    }
}