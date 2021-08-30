using Tuna;
using UnityEngine;

namespace PicoGraffiti.UI
{
    public class UIMain: TunaBehaviour
    {
        [SerializeField] private Transform _root = null;
        public Transform Root => _root;
    }
}