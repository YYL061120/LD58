// Assets/Scripts/UI/Panels/VisitPanel.cs
using UnityEngine;

namespace DebtJam
{
    public class VisitPanel : ActionListPanelBase
    {
        protected override bool GetAllowClick(CaseRuntime rt)
        {
            // “显示所有可见的人”，但如果没地址，置灰禁点
            return rt.hasAddress && !string.IsNullOrWhiteSpace(rt.address);
        }

        protected override void OnEntryClicked(ContactEntryUI entry)
        {
            if (!CaseManager.I.TryGetRuntime(entry.debtorId, out var rt)) return;

            if (!GetAllowClick(rt))
            {
                ShowToast("无法选择进行行动，缺少地址");
                return;
            }

            rt.PushAction(ActionType.Visit);
            // TODO: 切动画 → 门口立绘 → 选项 → 扣 3h
            Debug.Log($"Visit -> {rt.displayName}");
        }
    }
}
