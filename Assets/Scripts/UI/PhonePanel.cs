// Assets/Scripts/UI/Panels/PhonePanel.cs
using UnityEngine;

namespace DebtJam
{
    public class PhonePanel : ActionListPanelBase
    {
        protected override bool GetAllowClick(CaseRuntime rt)
        {
            // 电话必须有 phone
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

            // 这里触发你的“打电话行动”——仅示例：
            rt.PushAction(ActionType.Call);
            // TODO: 打开电话对话 UI、走选项树、扣时长等
            Debug.Log($"Call -> {rt.displayName}");
        }
    }
}
