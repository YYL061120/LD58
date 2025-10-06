using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace DebtJam
{
    [DefaultExecutionOrder(-100)] // 确保先于 UI 初始化
    public class CaseManager : MonoBehaviour
    {
        public static CaseManager I { get; private set; }

        [Header("All Debtors (SO)")]
        public List<DebtorProfileSO> allDebtors = new();

        // 运行态字典（效果/条件会访问它）
        public readonly Dictionary<string, CaseRuntime> runtimeById = new();

        public CaseRuntime currentCase { get; private set; }

        // UI 订阅
        public event System.Action OnRosterChanged;
        public event System.Action<CaseRuntime> OnCaseChanged;

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this;
        }

        void Start() { BuildRuntime(); }

        // ========= 构建运行态 =========
        public void BuildRuntime()
        {
            runtimeById.Clear();

            foreach (var so in allDebtors.Where(s => s != null))
            {
                bool visibleAtStart = (so.visibleAfterCollectedTheseIds == null || so.visibleAfterCollectedTheseIds.Count == 0);
                if (string.IsNullOrWhiteSpace(so.debtorId))
                {
                    Debug.LogError($"[CaseManager] Debtor '{so.name}' has empty debtorId!");
                    continue;
                }
                runtimeById[so.debtorId] = new CaseRuntime(so, visibleAtStart);
            }

            currentCase = runtimeById.Values.FirstOrDefault(r => r.isVisible && r.outcome == CaseOutcome.Pending);
            Debug.Log($"[CaseManager] built {runtimeById.Count} cases, current={(currentCase != null ? currentCase.debtorId : "<null>")}");

            OnRosterChanged?.Invoke();
            if (currentCase != null) OnCaseChanged?.Invoke(currentCase);
        }

        // ========= 提供便捷访问 =========
        public DebtorProfileSO GetSO(string id) => allDebtors.FirstOrDefault(d => d && d.debtorId == id);
        public bool TryGetRuntime(string id, out CaseRuntime rt) => runtimeById.TryGetValue(id, out rt);

        public void SelectCase(string debtorId)
        {
            if (!runtimeById.TryGetValue(debtorId, out var rt)) return;
            currentCase = rt;
            OnCaseChanged?.Invoke(rt);
        }

        // ========= 被效果调用：修改 outcome，并推动解锁 =========
        public void SetOutcome(string debtorId, CaseOutcome outcome)
        {
            if (!runtimeById.TryGetValue(debtorId, out var rt)) return;
            if (rt.outcome == outcome) return;
            rt.outcome = outcome;

            UnlockByProgress();
            OnRosterChanged?.Invoke();

            // 若当前已完结，切到下一个可见 & Pending 的
            if (currentCase == null || currentCase.outcome != CaseOutcome.Pending)
            {
                var next = runtimeById.Values.FirstOrDefault(r => r.isVisible && r.outcome == CaseOutcome.Pending);
                if (next != null) { currentCase = next; OnCaseChanged?.Invoke(currentCase); }
            }
        }

        /// <summary>谁被收款（Collected）之后，满足前置的人变可见</summary>
        public void UnlockByProgress()
        {
            var collected = runtimeById.Values.Where(r => r.outcome == CaseOutcome.Collected)
                                              .Select(r => r.debtorId).ToHashSet();
            bool changed = false;

            foreach (var so in allDebtors.Where(s => s != null))
            {
                if (!runtimeById.TryGetValue(so.debtorId, out var rt)) continue;
                if (rt.isVisible) continue;

                bool allPreCollected = (so.visibleAfterCollectedTheseIds == null || so.visibleAfterCollectedTheseIds.Count == 0)
                    || so.visibleAfterCollectedTheseIds.All(id => collected.Contains(id));

                if (allPreCollected) { rt.isVisible = true; changed = true; }
            }

            if (changed) OnRosterChanged?.Invoke();
        }

        public bool AllCasesFinished() => runtimeById.Values.All(r => r.outcome != CaseOutcome.Pending);

        // ========= 给效果/条件通知“运行态变化”（可让 UI 刷新）=========
        public void NotifyCaseChanged(CaseRuntime rt)
        {
            if (rt == null) return;
            if (currentCase == null || currentCase.debtorId == rt.debtorId)
                OnCaseChanged?.Invoke(rt); // 当前对象被改 → 刷新 UI
        }

        // CaseManager.cs 内追加
        public System.Collections.Generic.IEnumerable<CaseRuntime> GetVisiblePendingCases()
        {
            foreach (var rt in runtimeById.Values)
                if (rt.isVisible && rt.outcome == CaseOutcome.Pending)
                    yield return rt;
        }
    }
}
