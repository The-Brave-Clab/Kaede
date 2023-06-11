using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class Y3Live2DRender : MonoBehaviour
    {
        private void OnPostRender()
        {
            Y3Live2DManager.Render();
        }
    }
}
