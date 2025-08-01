using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MK.EdgeDetection.Examples
{
    public class MKEdgeDetectionRotateGear : MonoBehaviour
    {
        public float speed = 10;

        private bool rotate = true;

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.R))
                rotate = !rotate;
            
            if(rotate)
                transform.Rotate(Vector3.up * Time.smoothDeltaTime * speed);
        }
    }
}