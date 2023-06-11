using System;
using UnityEngine;

namespace biscuit.Scenario.Effect
{
    // Token: 0x02000816 RID: 2070
    public class EffectDestroyer : MonoBehaviour
    {
        // Token: 0x060037F3 RID: 14323 RVA: 0x000D7EB4 File Offset: 0x000D60B4
        private void Update()
        {
            this.timer += Time.deltaTime;
            if (this.timer >= this.deathTimer)
            {
                UnityEngine.Object.Destroy(base.gameObject);
            }
        }

        // Token: 0x04002B8D RID: 11149
        [SerializeField]
        private float deathTimer = 2f;

        // Token: 0x04002B8E RID: 11150
        private float timer;
    }
}
