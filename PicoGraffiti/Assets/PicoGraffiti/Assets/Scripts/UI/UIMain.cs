using PicoGraffiti.Model;
using Tuna;
using UnityEngine;

namespace PicoGraffiti.UI
{
    public class UIMain: TunaBehaviour
    {
        [SerializeField] private Transform _root = null;
        [SerializeField] private Transform _content = null;
        [SerializeField] private Transform _volumeRoot = null;
        public Transform Root => _root;
        public Transform Content => _content;
        public Transform VolumeRoot => _volumeRoot;
    }
}