using UnityEngine;
using System.Collections.Generic;
using System;

namespace DebtJam
{
    public class CaseManager : MonoBehaviour
    {
        public static CaseManager I { get; private set; }

        [Header("内容入口")]
        public List<DebtorProfileSO> allDebtors = new();

        [Header("运行态（只读）")]
        public Dictionary<string, CaseRuntime> runtimeById = new();

        public event Action<CaseRuntime> OnCaseChanged;   // 刷新 UI
        public CaseRuntime currentCase { get; private set; }

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            foreach (var so in allDebtors)
            {
                var rt = new CaseRuntime(so);
                runtimeById[so.debtorId] = rt;
            }

            if (allDebtors.Count > 0)
                SelectCase(allDebtors[0].debtorId);
        }

        public void SelectCase(string debtorId)
        {
            if (runtimeById.TryGetValue(debtorId, out var rt))
            {
                currentCase = rt;
                OnCaseChanged?.Invoke(rt);
            }
        }

        public DebtorProfileSO GetProfileSO(string debtorId)
        {
            return allDebtors.Find(d => d.debtorId == debtorId);
        }
    }
}