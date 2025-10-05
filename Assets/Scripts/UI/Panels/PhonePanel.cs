using UnityEngine;

namespace DebtJam
{
    public class PhonePanel : ActionListPanelBase
    {
        protected override bool GetAllowClick(CaseRuntime rt)
        {
            return rt.hasPhone && !string.IsNullOrWhiteSpace(rt.phoneNumber);
        }

        protected override void OnEntryClicked(ContactEntryUI entry)
        {
            if (!CaseManager.I.TryGetRuntime(entry.debtorId, out var rt)) return;

            if (!ActionExecutor.I.TryStartAction(ActionType.Call, rt,
                closePanel: () => gameObject.SetActive(false),
                showToast: ShowToast))
            {
                // 不可执行时的提示已由 TryStartAction 回调 showToast 处理
            }
        }
    }
}
