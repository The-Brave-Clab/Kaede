using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Y3ADV.Database;

namespace Y3ADV
{
    public class SubCategoryButton : EpisodeButton
    {
        public Image IconComplete;
        public Image IconNew;

        public enum CompleteStatus
        {
            New,
            InProgress,
            Complete
        }

        public void SetCompleteStatus(CompleteStatus status)
        {
            switch (status)
            {
                case CompleteStatus.New:
                    IconNew.gameObject.SetActive(true);
                    IconComplete.gameObject.SetActive(false);
                    break;
                case CompleteStatus.InProgress:
                    IconNew.gameObject.SetActive(false);
                    IconComplete.gameObject.SetActive(false);
                    break;
                case CompleteStatus.Complete:
                    IconNew.gameObject.SetActive(false);
                    IconComplete.gameObject.SetActive(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public override void RefreshFinishedStatus()
        {
            if (subCategoryID == -1)
            {
                base.RefreshFinishedStatus();
                return;
            }
            List<adventure_books> subCategoryAdvs = DatabaseManager.Table
                .Where(x => x.category_id == DatabaseManager.CurrentCategory && x.sub_category_id == subCategoryID).ToList();
            CompleteStatus completeStatus = subCategoryAdvs.All(a => GameManager.IsScenarioFinished(a.file_id))
                ? CompleteStatus.Complete
                : subCategoryAdvs.All(a => !GameManager.IsScenarioFinished(a.file_id))
                    ? CompleteStatus.New
                    : CompleteStatus.InProgress;
            
            SetCompleteStatus(completeStatus);
        }
    }
}
