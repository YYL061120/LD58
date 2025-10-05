// Assets/Scripts/Gameplay/Enums.cs
namespace DebtJam
{
    // 行为类型：和 LastActionWasCondition / ActionExecutor 对齐
    public enum ActionType { None = 0, Call, SMS, Visit }

    // 联系方式种类：和 SetContactEffect / CaseRuntime.SetContact 对齐
    public enum ContactKind { Phone, Address }

    // 案件终局：和 CaseManager/CaseRuntime/UI 对齐
    public enum CaseOutcome { Pending, Collected, DeadEnd }

    // 运行时事实状态（效果层和 UI 读取它）
    public enum FactState { Unknown, KnownTrue, Fake }

    // 仅 ScriptableObject 初始配置用的“可见性”
    public enum FactVisibility { Unknown, KnownTrue }
}
