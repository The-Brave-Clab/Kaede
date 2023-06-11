using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Y3ADV
{
    public class CategoryPanel : MonoBehaviour
    {
        public GameObject ButtonPrefab;
        public GameObject DirectAccess;
        
        private List<SubCategoryButton> buttons;
        public List<SubCategoryButton> Buttons => buttons;

        public static CategoryPanel Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void PopulateButtons(int numbers)
        {
            buttons = new List<SubCategoryButton>(numbers);

            for (int i = 0; i < numbers; ++i)
            {
                GameObject button = Instantiate(ButtonPrefab, transform);
                button.name = $"CategoryButton_{i}";
                button.transform.SetSiblingIndex(i);
                SubCategoryButton component = button.GetComponent<SubCategoryButton>();
                buttons.Add(component);
            }
            
            DirectAccess.transform.SetSiblingIndex(numbers);
        }
    }
}
