using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RanaksDunGen
{
    [Icon("Packages/com.ranakofour.dungen/Editor/Gizmos/DunGenRoomIcon.png")]
    public class DunGenRoom : MonoBehaviour
    {
        [SerializeField]
        private Bounds m_Bounds;

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
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(m_Bounds.center, m_Bounds.size);
            Gizmos.color = Color.white;
        }

        public void ReorientSize()
        {
            string l_debugMessage = "Dungeon Generator: Reorienting Shape " + gameObject.name + "\nSize " + m_Bounds.size + "\nOffset " + m_Bounds.center;

            Vector3 l_eulers = transform.rotation.eulerAngles;

            l_debugMessage += "\nEulers: " + l_eulers;
            l_debugMessage += "\nQuaternions: " + transform.rotation;

            // Leaving this as a vec3 incase other rotations are wanted later
            float l_quarterTurns = l_eulers.y / 90.0f;

            // Makes rotation positive for % operation to work

            l_debugMessage += "\nQuarter Turns: " + l_quarterTurns;

            Vector3 l_Size = m_Bounds.size;
            Vector3 l_Center = m_Bounds.center;

            // Switch lengths if needed
            switch (Mathf.Round(l_quarterTurns))
            {
                case 1:
                    (l_Size.x, l_Size.z) = (l_Size.z, l_Size.x);

                    // Swap offset axis on quarter turns
                    l_Center.x *= -1;
                    (l_Center.x, l_Center.z) = (l_Center.z, l_Center.x);

                    l_debugMessage += "\n Case 1";
                    break;

                case 2:
                    l_Center.x *= -1;
                    l_Center.z *= -1;
                    l_debugMessage += "\n Case 2";
                    break;

                case 3:
                    (l_Size.x, l_Size.z) = (l_Size.z, l_Size.x);

                    // Swap offset axis on quarter turns
                    (l_Center.x, l_Center.z) = (l_Center.z, l_Center.x);
                    l_Center.x *= -1;

                    l_debugMessage += "\n Case 3";
                    break;
            }

            m_Bounds.center = l_Center;
            m_Bounds.size = l_Size;

            l_debugMessage += "\nNew Size: " + l_Size + "\nNew Offset: " + l_Center;

            //Debug.Log(l_debugMessage);
        }

        public Bounds GetBounds()
        {
            // This will only run the first time
            if (m_Dirty)
            {
                // Shrink bounds slightly so objects next to eachother don't overlap
                m_Bounds.size = new Vector3(
                m_Bounds.size.x - 0.1f,
                m_Bounds.size.y - 0.1f,
                m_Bounds.size.z - 0.1f
                );

                // Place the center where it should be
                m_Bounds.center = transform.position + m_Bounds.center;

                m_Dirty = false;
            }

            return m_Bounds;
        }
    }
}