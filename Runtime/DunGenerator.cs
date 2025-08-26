using System;
using System.Collections.Generic;
using UnityEngine;

namespace RanaksDunGen
{
    // Holds static data about the dungeon
    [Icon("Packages/com.ranakofour.dungen/Editor/Gizmos/DunGeneratorIcon.png")]
    public class DunGenerator : MonoBehaviour
    {
        private bool m_Generated = false;

        [Tooltip("Size of the whole dungeon space")]
        [SerializeField]
        private Vector3 m_DungeonSize = Vector3.one;

        [Tooltip("The piece that is placed down first in the center of the dungeon")]
        [SerializeField]
        private GameObject m_StartingRoom;

        [Serializable]
        public struct DunGenRoomContainer
        {
            [Tooltip("The prefab to be used for this dungeon part")]
            [SerializeField]
            public GameObject m_Prefab;

            [Tooltip("How many times this piece can be placed into the dungeon")]
            [SerializeField]
            public int m_MaxIterations;
        }

        [Tooltip("Unique prefabs that will make up the dungeon")]
        [SerializeField]
        private List<DunGenRoomContainer> m_DungeonParts;

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
                Debug.Log("RanaksDunGen: no starting room set!");
                return;
            }

            if (m_DungeonSize == Vector3.zero)
            {
                Debug.LogWarning("RanaksDunGen: No dungeon size specified!");
                return;
            }

            if (!ArePartsValid())
            {
                Debug.LogWarning("RanaksDunGen: There are parts without DunGenRoom scripts attatched!");
                return;
            }

            if (m_Generated)
            {
                // Destroy previously created dungeon right now
                while (transform.childCount > 0)
                {
                    GameObject.DestroyImmediate(transform.GetChild(0).gameObject);
                }
            }

            // Copy m_DungeonParts
            List<DunGenRoomContainer> l_dungeonParts = new List<DunGenRoomContainer>(m_DungeonParts);

            // I tried adding an 'm_Iterations' property to DungeonParts but it wouldn't work for some reason when changing the value
            // MAXITERATION FAULT
            Dictionary<GameObject, int> l_IterationCounts = new Dictionary<GameObject, int>();
            foreach (DunGenRoomContainer dp in l_dungeonParts)
            {
                l_IterationCounts.Add(dp.m_Prefab, 0);
            }

            // Using this to check if a prefab is already occupying a coordinate in the voxel space

            //bool[,,] l_VoxelMap = new bool[(int)m_DungeonSize.x, (int)m_DungeonSize.y, (int)m_DungeonSize.z];

            // More memory efficient than aformentioned bool[,,]
            List<Vector3> l_VoxelMap = new List<Vector3>();

            Debug.Log("Dungeon Generator: Starting generation");

            // New objects created are added to the queue to have new pieces joined to them
            Queue<GameObject> l_partQueue = new Queue<GameObject>();

            GameObject l_instantiatedPiece = GameObject.Instantiate(m_StartingRoom, gameObject.transform);
            l_partQueue.Enqueue(l_instantiatedPiece);

            int l_currentPieceID = 1;
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

                    Debug.Log("DunGen: Working on ConnectionPoint ID " + l_currentPoint.ID());

                    bool l_partFits = false;
                    // Prevent deadlocking
                    int l_unsuccessfulFits = 0;

                    while (!l_partFits && l_unsuccessfulFits < 6)
                    {
                        // Cycle through parts randomly until one that can be placed is found
                        int l_newPieceIndex = UnityEngine.Random.Range(0, l_dungeonParts.Count);

                        GameObject l_newPiece = GameObject.Instantiate(l_dungeonParts[l_newPieceIndex].m_Prefab, gameObject.transform);
                        l_newPiece.name = "" + l_currentPieceID + ": " + l_newPiece.name;
                        l_newPiece.name = l_newPiece.name.Split('(')[0];

                        // Get random connectionPoint;
                        ConnectionPoint[] l_newPiecePoints = l_newPiece.GetComponentsInChildren<ConnectionPoint>();
                        ConnectionPoint l_newPoint = l_newPiecePoints[UnityEngine.Random.Range(0, l_newPiecePoints.Length)];

                        //Debug.Log("DunGen: Connecting point " + l_currentPoint.m_ID + " to point " + l_newPoint.m_ID);

                        // Using Quaternion.AngleAxis messes with l_newPiece transform, and that isn't good
                        l_newPiece.transform.Rotate(Vector3.up, l_currentPoint.transform.eulerAngles.y - l_newPoint.transform.eulerAngles.y + 180f);
                        l_newPiece.GetComponent<DunGenRoom>().ReorientSize();

                        // Accounts for difference in overlapping pieces
                        Vector3 l_translate = l_currentPoint.transform.position - l_newPoint.transform.position;
                        l_newPiece.transform.position += l_translate;

                        if (DoesObjectFit(ref l_newPiece))
                        {
                            l_partFits = true;
                            l_newPoint.Connect();
                            l_currentPoint.Connect();
                            l_partQueue.Enqueue(l_newPiece);

                            //Increment number of instances and remove from list if the maximum number if instances is reached
                            l_IterationCounts[l_dungeonParts[l_newPieceIndex].m_Prefab]++;
                            if (l_IterationCounts[l_dungeonParts[l_newPieceIndex].m_Prefab] == l_dungeonParts[l_newPieceIndex].m_MaxIterations)
                            {
                                l_dungeonParts.Remove(l_dungeonParts[l_newPieceIndex]);
                            }

                            l_currentPieceID++;
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

            m_Generated = true;

            Debug.Log("Dungeon Generator: Finished!");
        }

        // See if all prefabs have DungeonParts
        private bool ArePartsValid()
        {
            DunGenRoom l_test = m_StartingRoom.GetComponent<DunGenRoom>();
            if (!l_test)
            {
                return false;
            }

            foreach (DunGenRoomContainer dpc in m_DungeonParts)
            {
                l_test = dpc.m_Prefab.GetComponent<DunGenRoom>();
                if (!l_test)
                {
                    return false;
                }
            }

            return true;
        }

        private bool DoesObjectFit(ref GameObject _dungeonPart)
        {
            DunGenRoom l_dungenRoom = _dungeonPart.GetComponent<DunGenRoom>();

            DunGenRoom l_currentRoom;
            for (int i = 0; i < gameObject.transform.childCount - 1; i++)
            {
                l_currentRoom = gameObject.transform.GetChild(i).GetComponent<DunGenRoom>();
                if (l_dungenRoom.GetBounds().Intersects(l_currentRoom.GetBounds()))
                {
                    Debug.Log("DunGen: Failed to fit " + l_dungenRoom.gameObject.name + ", overlaps with " + l_currentRoom.gameObject.name);
                    return false;
                }
            }

            Debug.Log("DunGen: Successful fit");
            return true;
        }
    }
}