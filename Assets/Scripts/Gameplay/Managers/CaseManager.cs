using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

namespace DebtJam
{
    public class CaseManager : MonoBehaviour
    {
        public static CaseManager I { get; private set; }

        public event System.Action<CaseRuntime> OnCaseChanged;

        [Header("内容入口")]
        public List<DebtorProfileSO> allDebtors = new();

        [Header("运行态")]
        public Dictionary<string, CaseRuntime> runtimeById = new();
        public CaseRuntime currentCase { get; private set; }

        public event Action OnRosterChanged;

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this; DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            foreach (var so in allDebtors)
            {
                bool visible = so.visibleAfterCollectedTheseIds.Count == 0; // 没前置则开局可见（A）
                runtimeById[so.debtorId] = new CaseRuntime(so, visible);
            }
            var first = runtimeById.Values.FirstOrDefault(r => r.isVisible);
            if (first != null) SelectCase(first.debtorId);
        }

        public void SelectCase(string debtorId)
        {
            if (runtimeById.TryGetValue(debtorId, out var rt))
            { currentCase = rt; OnCaseChanged?.Invoke(rt); }
        }

        public DebtorProfileSO GetSO(string id) => allDebtors.Find(d => d.debtorId == id);

        // 在某个案件收款成功后调用：按 “谁已 Collected” 解锁后续
        public void UnlockByProgress()
        {
            var collected = runtimeById.Values.Where(r => r.outcome == CaseOutcome.Collected)
                                              .Select(r => r.debtorId).ToHashSet();
            bool changed = false;

            foreach (var so in allDebtors)
            {
                var rt = runtimeById[so.debtorId];
                if (rt.isVisible) continue;
                if (so.visibleAfterCollectedTheseIds.All(id => collected.Contains(id)))
                { rt.isVisible = true; changed = true; }
            }
            if (changed) OnRosterChanged?.Invoke();
        }

        public void NotifyCaseChanged(CaseRuntime rt)
        {
            OnCaseChanged?.Invoke(rt);
        }

        public bool AllCasesFinished() => runtimeById.Values.All(r => r.outcome != CaseOutcome.Pending);
    }
}

