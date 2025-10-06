using UnityEngine;

namespace DebtJam
{
    public class SMSPanel : ActionListPanelBase
    {
        public ContactEntryUI itemPrefab;

        void OnEnable() { CaseManager.I.OnRosterChanged += Rebuild; Rebuild(); }
        void OnDisable() { CaseManager.I.OnRosterChanged -= Rebuild; }

        void Rebuild()
        {
            foreach (Transform t in contentRoot) Destroy(t.gameObject);
            foreach (var rt in CaseManager.I.GetVisiblePendingCases())
            {
                var so = CaseManager.I.GetSO(rt.debtorId);
                var item = Instantiate(itemPrefab, contentRoot);
                item.Setup(rt, so);
                item.SetSubtitle(rt.phoneNumber ?? "—"); // ★ 显示手机号
                item.SetInteractable(rt.hasPhone);
                item.BindClick(() => OnEntryClicked(item));
            }
        }

        void OnItemClicked(ContactEntryUI entry) => OnEntryClicked(entry);

        protected override bool GetAllowClick(CaseRuntime rt) => rt.hasPhone;

        protected override void OnEntryClicked(ContactEntryUI entry)
        {
            if (!CaseManager.I.TryGetRuntime(entry.debtorId, out var rt)) return;
            if (!GetAllowClick(rt)) return;

            ActionExecutor.I.TryStartAction(ActionType.SMS, rt,
                closePanel: () => gameObject.SetActive(false),
                showToast: ShowToast);
        }
    }
}
