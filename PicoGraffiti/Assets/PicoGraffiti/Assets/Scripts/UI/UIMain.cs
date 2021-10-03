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
        [SerializeField] private Transform _scoreValuesRoot = null;
        [SerializeField] private UILoading _loading;
        [SerializeField] private Transform _monitor = null;
        public Transform Root => _root;
        public Transform Content => _content;
        public Transform VolumeRoot => _volumeRoot;
        public Transform ScoreValuesRoot => _scoreValuesRoot;
        public UILoading Loading => _loading;
        public Transform Monitor => _monitor;
    }
}