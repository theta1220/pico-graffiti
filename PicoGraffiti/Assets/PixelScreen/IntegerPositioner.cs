using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PixelScreen
{
    // [ExecuteInEditMode()]
    public class IntegerPositioner : MonoBehaviour
    {
        void ToInt()
        {
            foreach (Transform child in transform)
            {
                var buf = new Vector3();
                var pos = child.position;
                buf.x = Mathf.FloorToInt(pos.x);
                buf.y = Mathf.FloorToInt(pos.y);
                buf.z = Mathf.FloorToInt(pos.z);
                child.position = buf;
            }
        }

        void LateUpdate()
        {
            ToInt();
        }
    }
}