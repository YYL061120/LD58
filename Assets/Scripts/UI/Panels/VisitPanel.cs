using UnityEngine;

namespace DebtJam
{
    public class VisitPanel : ActionListPanelBase
    {
        protected override bool GetAllowClick(CaseRuntime rt)
        {
            return rt.hasAddress && !string.IsNullOrWhiteSpace(rt.address);
        }

        protected override void OnEntryClicked(ContactEntryUI entry)
        {
            if (!CaseManager.I.TryGetRuntime(entry.debtorId, out var rt)) return;

            ActionExecutor.I.TryStartAction(ActionType.Visit, rt,
                closePanel: () => gameObject.SetActive(false),
                showToast: ShowToast);
        }
    }
}
