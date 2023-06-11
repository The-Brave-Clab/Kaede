using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Y3ADV.Database;

namespace Y3ADV
{
    public class DirectAccessButton : MonoBehaviour
    {
        public TMP_InputField InputField;

        public void DirectAccess()
        {
            // TODO: Check scenario availability
            DatabaseManager.Instance.SelectScenario(InputField.text);
        }
    }
}