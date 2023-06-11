using System.Collections;
using System.Collections.Generic;
using SQLite4Unity3d;
using UnityEngine;

namespace Y3ADV.Database
{
    public class adventure_books
    {
        [PrimaryKey]
        public int id { get; set; }
        public string file_id { get; set; }
        public string category { get; set; }
        public int category_id { get; set; }
        public string sub_category { get; set; }
        public int sub_category_id { get; set; }
        public string episode { get; set; }
        public int episode_id { get; set; }
        public string name { get; set; }
        public string chapter_name { get; set; }
        public string display_name { get; set; }
        public string label { get; set; }
    }

    public class special_chapters
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
        public string short_name { get; set; }
        public int image_id { get; set; }
        public int banner_id { get; set; }
        public string is_event { get; set; } // TRUE Only
        public int event_type { get; set; }
        public string layout_id { get; set; }
        public int priority { get; set; }
        public string special_event { get; set; }
        public string special_attack_limit { get; set; }
        public string event_url { get; set; }
        public string episode_navi_id { get; set; }
        public string episode_dialog_id { get; set; }
        public int sancho_days { get; set; }
        
    }
}
