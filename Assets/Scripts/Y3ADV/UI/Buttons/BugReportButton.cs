using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Y3ADV
{
    public class BugReportButton : MonoBehaviour
    {
        public void SendBugNotification()
        {
            Y3ScriptModule scriptModule = FindObjectOfType<Y3ScriptModule>();
            Utils.SendBugNotification($"Manually Triggered Bug Notification");
        }
    }
}
