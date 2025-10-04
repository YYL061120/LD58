using UnityEngine;

namespace DebtJam
{
    public abstract class EffectSO : ScriptableObject
    {
        public abstract void Apply(CaseRuntime rt);
        public virtual string GetReadable() => name;
    }
}