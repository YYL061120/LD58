using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace DebtJam
{
    /// <summary>
    /// 只显示可见且未完结(Pending)的欠债人（不提供点击）。
    /// Content 节点用 VerticalLayoutGroup + ContentSizeFitter 布局。
    /// 文本使用 TextMeshPro。
    /// </summary>
    public class ProfilePanel_DisplayOnly_TMP : MonoBehaviour
    {
        [Header("UI")]
        [Tooltip("承载条目的父节点（挂 VerticalLayoutGroup + ContentSizeFitter）")]
        public Transform contentRoot;

        [Tooltip("条目预制体（需要挂 ProfileItemDisplay_TMP 脚本）")]
        public ProfileItemDisplay_TMP itemPrefab;

        [Header("选项")]
        [Tooltip("是否隐藏 DeadEnd（默认 true：只显示 Pending）")]
        public bool hideDeadEnd = true;

        [Tooltip("无可展示档案时的占位物体（可选）")]
        public GameObject emptyStateGO;

        private readonly List<ProfileItemDisplay_TMP> _spawned = new();

        void OnEnable()
        {
            if (CaseManager.I != null)
                CaseManager.I.OnRosterChanged += Refresh;
            Refresh();
        }

        void OnDisable()
        {
            if (CaseManager.I != null)
                CaseManager.I.OnRosterChanged -= Refresh;
        }

        public void Refresh()
        {
            Debug.Log($"Roster count = {CaseManager.I?.runtimeById?.Count}");
            if (!contentRoot || !itemPrefab || CaseManager.I == null) return;

            // 1) 清空 Content 下的所有子物体（包括误摆进去的模板）
            for (int i = contentRoot.childCount - 1; i >= 0; i--)
                Destroy(contentRoot.GetChild(i).gameObject);
            _spawned.Clear();

            var cm = CaseManager.I;

            IEnumerable<CaseRuntime> source = cm.runtimeById.Values.Where(r => r.isVisible);
            if (hideDeadEnd)
                source = source.Where(r => r.outcome == CaseOutcome.Pending);
            else
                source = source.Where(r => r.outcome != CaseOutcome.Collected);

            var list = source.OrderBy(r => r.debtorId).ToList();

            if (emptyStateGO) emptyStateGO.SetActive(list.Count == 0);

            foreach (var rt in list)
            {
                var so = cm.GetSO(rt.debtorId);
                var item = Instantiate(itemPrefab, contentRoot);
                item.Setup(rt, so);            // ← 这里会把名字/金额/词条全部写进去
                _spawned.Add(item);
            }
        }
    }
}
