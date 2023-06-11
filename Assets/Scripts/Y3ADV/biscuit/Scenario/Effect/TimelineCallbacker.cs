using System;
using UnityEngine;

namespace biscuit.Scenario.Effect
{
    // Token: 0x020002D6 RID: 726
    public class TimelineCallbacker : MonoBehaviour
    {
        // Token: 0x06001747 RID: 5959 RVA: 0x00069F7C File Offset: 0x0006817C
        private void TimeLineCallbacker()
        {
            if (this.callback != null)
            {
                this.callback();
            }
        }

        // Token: 0x06001748 RID: 5960 RVA: 0x00069F94 File Offset: 0x00068194
        private void TimelineCallDestroyer()
        {
            if (!this.debugFlag)
            {
                UnityEngine.Object.Destroy(base.gameObject);
            }
        }

        // Token: 0x04001473 RID: 5235
        [SerializeField]
        private bool debugFlag;

        // Token: 0x04001474 RID: 5236
        public Action callback;
    }
}
