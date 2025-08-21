using UnityEngine;

namespace RanaksDunGen
{
    [Icon("Packages/com.ranakofour.dungen/Editor/Gizmos/ConnectionPointIcon.png")]
    [Tooltip("Place at points where the prefab can be connected with. Must face outwards from prefab.")]
    public class ConnectionPoint : MonoBehaviour
    {
        // If true, the connection has been used
        [SerializeField]
        private bool m_Connected;
        private int m_ID = 0;

        private void Awake()
        {
            m_Connected = false;
        }

        public void Connect()
        {
            m_Connected = true;
        }

        public bool Connected()
        {
            return m_Connected;
        }

        public void Hide()
        {
            if (transform.childCount > 0)
            {
                transform.GetChild(0).gameObject.SetActive(false);
            }
        }

        public void ID(int _id)
        {
            m_ID = _id;
        }

        public int ID()
        {
            return m_ID;
        }
    }
}