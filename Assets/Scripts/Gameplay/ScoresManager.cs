using UnityEngine;
using System;

namespace DebtJam
{
    public class ScoreManager : MonoBehaviour
    {
        public static ScoreManager I { get; private set; }
        public int score;

        public event Action<int> OnScoreChanged;

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        public void AddScore(int delta)
        {
            score += delta;
            OnScoreChanged?.Invoke(score);
        }

        public string GetEnding()
        {
            // 简单分档，Jam 后期你可外置成表
            if (score >= 8) return "S 结局：王牌催收员";
            if (score >= 5) return "A 结局：部门骨干";
            if (score >= 3) return "B 结局：勉强完成 KPI";
            return "C 结局：试用期待改善";
        }
    }
}