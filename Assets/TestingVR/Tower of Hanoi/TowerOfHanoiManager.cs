using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TestingVR.Tower_of_Hanoi
{
    public class TowerOfHanoiManager : MonoBehaviour
    {
        private const float MIN_SLICE_EXPANSION = 0.1f;
        private const float MAX_SLICE_EXPANSION = 0.2f;
        
        private const float SLICE_HEIGHT = 0.008f;
        
        [SerializeField]
        private SliceController _slicePrefab;
        
        [SerializeField, Range(3,6)]
        private int sliceCount = 3;
        
        [SerializeField]
        private RodController _rod1;

        [SerializeField] 
        private List<Color> sliceColors;

        void Start()
        {
            SpawnSlices();  
        }

        private async Task SpawnSlices()
        {
            for (var i = 0; i != sliceCount; i++)
            {
                var slice = Instantiate(_slicePrefab, transform);
                var expansion = Mathf.Lerp(MAX_SLICE_EXPANSION, MIN_SLICE_EXPANSION, i/(sliceCount - 1f));
                slice.transform.localScale = new Vector3(expansion, SLICE_HEIGHT, expansion);
                slice.GetComponent<MeshRenderer>().material.color = sliceColors[i];
                _rod1.StackSlice(slice);
                await Task.Delay(500);
            }  
        }
    }
}
