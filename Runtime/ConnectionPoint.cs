using UnityEngine;

namespace RanaksDunGen
{
    [Tooltip("Place at points where the prefab can be connected with. Must face outwards from prefab.")]
    public class ConnectionPoint : MonoBehaviour
    {
        // If true, the connection has been used
        [SerializeField]
        private bool m_Connected;

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
            transform.GetChild(0).gameObject.SetActive(false);
            // MeshRenderer l_renderer = GetComponentInChildren<MeshRenderer>();
            // MeshCollider l_collider = GetComponentInChildren<MeshCollider>();
            // l_renderer.enabled = false;
            // l_collider.enabled = false;
        }
    }
}