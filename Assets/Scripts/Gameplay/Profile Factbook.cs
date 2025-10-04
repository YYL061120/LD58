// 静态：策划表/配置里定义；不进存档
using System;
using System.Collections.Generic;

[Serializable]
public class FactDefinition
{
    public string FactId;          // 稳定ID，永不改名（存档锚点）
    public string TitleLocKey;     // 本地化Key/描述模板Key
    //public FactValueType ValueType;// Text/Number/Enum/Date/...（用于渲染）
}

// 运行态：进入存档
[Serializable]
public class FactState
{
    public string FactId;          // 对应定义
    public FactVisibility Visibility; // 可见性状态（枚举见下）
    public float Progress;         // 0~1（用于“模糊→逐步清晰”）
    public string ValueJson;       // 事实“值”（序列化成字符串保存，避免直存展示文本）
    public int LastUpdatedDay;     // 第几天被更新（可用于时间线回放）
}

public enum FactVisibility
{
    Unknown = 0,   // 未知（界面模糊/问号）
    Hinted = 1,   // 有线索（可显示片段/模糊度降低）
    Revealed = 2,  // 已揭示（清晰可见）
    Disproved = 3  // 被证伪（可走特殊UI）
}

// 一本“事实书”，按ID索引
[Serializable]
public class FactBook
{
    public List<FactState> States = new();
    // 运行时可建个 Dictionary<string, FactState> 索引加速；索引不进入存档
}
