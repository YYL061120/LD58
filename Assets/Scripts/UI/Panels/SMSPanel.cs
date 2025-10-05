using UnityEngine;

namespace DebtJam
{
    public class SMSPanel : ActionListPanelBase
    {
        protected override bool GetAllowClick(CaseRuntime rt)
        {
            return rt.hasPhone && !string.IsNullOrWhiteSpace(rt.phoneNumber);
        }

        protected override void OnEntryClicked(ContactEntryUI entry)
        {
            if (!CaseManager.I.TryGetRuntime(entry.debtorId, out var rt)) return;

            ActionExecutor.I.TryStartAction(ActionType.SMS, rt,
                closePanel: () => gameObject.SetActive(false),
                showToast: ShowToast);
        }
    }
}
