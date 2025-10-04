using UnityEngine;

namespace DebtJam
{
    public enum ActionType { Call, SMS, Visit }
    public enum ContactType { Phone, Address, Email }
    public enum CaseOutcome { Pending, Collected, DeadEnd }
    public enum FactVisibility { Unknown, KnownTrue, KnownFake }
}