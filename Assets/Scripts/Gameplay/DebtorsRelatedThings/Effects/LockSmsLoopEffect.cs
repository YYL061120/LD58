using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Effects/Set SMS Loop Lock")]
    public class LockSmsLoopEffect : EffectSO
    {
        public bool enable = true;
        public override void Apply(CaseRuntime rt)
        {
            const string k = "SMS_LockedLoop";
            if (enable) rt.milestones.Add(k);
            else rt.milestones.Remove(k);
        }
        public override string GetReadable() => $"短信循环锁 {(enable ? "开" : "关")}";
    }
}


