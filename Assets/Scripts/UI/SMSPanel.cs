// Assets/Scripts/UI/Panels/SMSPanel.cs
using UnityEngine;

namespace DebtJam
{
    public class SMSPanel : ActionListPanelBase
    {
        protected override bool GetAllowClick(CaseRuntime rt)
        {
            // 短信也需要 phone
            return rt.hasPhone && !string.IsNullOrWhiteSpace(rt.phoneNumber);
        }

        protected override void OnEntryClicked(ContactEntryUI entry)
        {
            if (!CaseManager.I.TryGetRuntime(entry.debtorId, out var rt)) return;

            if (!GetAllowClick(rt))
            {
                ShowToast("无法选择进行行动，缺少电话号码");
                return;
            }

            rt.PushAction(ActionType.SMS);
            // TODO: 打开短信对话 UI（你的 SMSPanel 对话流程），扣 1h
            Debug.Log($"SMS -> {rt.displayName}");
        }
    }
}
