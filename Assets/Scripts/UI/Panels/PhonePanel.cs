using UnityEngine;

namespace DebtJam
{
    public class PhonePanel : ActionListPanelBase
    {
        [Header("Prefab")]
        public ContactEntryUI itemPrefab;

        void OnEnable() { if (CaseManager.I) CaseManager.I.OnRosterChanged += Rebuild; Rebuild(); }
        void OnDisable() { if (CaseManager.I) CaseManager.I.OnRosterChanged -= Rebuild; }

        void Rebuild()
        {
            foreach (Transform t in contentRoot) Destroy(t.gameObject);

            foreach (var rt in CaseManager.I.GetVisiblePendingCases())
            {
                var so = CaseManager.I.GetSO(rt.debtorId);
                var item = Instantiate(itemPrefab, contentRoot);
                item.Setup(rt, so);
                item.SetSubtitle(rt.phoneNumber ?? "—");  // ★ 显示手机号
                item.SetInteractable(rt.hasPhone && !string.IsNullOrWhiteSpace(rt.phoneNumber));
                item.BindClick(() => OnEntryClicked(item));
            }
        }

        void OnItemClicked(ContactEntryUI entry) => OnEntryClicked(entry);

        protected override bool GetAllowClick(CaseRuntime rt)
            => rt.hasPhone && !string.IsNullOrWhiteSpace(rt.phoneNumber);

        protected override void OnEntryClicked(ContactEntryUI entry)
        {
            if (!CaseManager.I.TryGetRuntime(entry.debtorId, out var rt)) return;
            if (!GetAllowClick(rt)) return;

            if (!ActionExecutor.I.TryStartAction(ActionType.Call, rt,
                closePanel: () => gameObject.SetActive(false),
                showToast: ShowToast))
            {
                // 不可执行时的提示已由 TryStartAction 回调 showToast 处理
            }
        }
    }
}
