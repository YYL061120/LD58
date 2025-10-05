using UnityEngine;

namespace DebtJam
{
    public abstract class ConditionSO : ScriptableObject
    {
        public abstract bool Evaluate(CaseRuntime rt, GameClock clock);
        public virtual string GetReadable() => name; // 失败时的提示
    }
}
