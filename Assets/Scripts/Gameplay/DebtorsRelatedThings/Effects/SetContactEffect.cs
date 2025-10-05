using UnityEngine;

namespace DebtJam
{
    [CreateAssetMenu(menuName = "DebtJam/Effects/Set Contact")]
    public class SetContactEffect : EffectSO
    {
        public ContactType type;
        public string value;
        public override void Apply(CaseRuntime rt)
        {
            switch (type)
            {
                case ContactType.Phone: rt.hasPhone = true; rt.phoneNumber = value; break;
                case ContactType.Address: rt.hasAddress = true; rt.address = value; break;
                case ContactType.Email: rt.hasEmail = true; rt.email = value; break;
            }
        }
        public override string GetReadable() => $"写入{type}:{value}";
    }
}
