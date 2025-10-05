using UnityEngine;
using System;

namespace DebtJam
{
    public class MoneyManager : MonoBehaviour
    {
        public static MoneyManager I { get; private set; }
        public int totalCollected;
        public event Action<int> OnMoneyChanged;

        void Awake()
        {
            if (I && I != this) { Destroy(gameObject); return; }
            I = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Add(int amount)
        {
            totalCollected += Mathf.Max(0, amount);
            OnMoneyChanged?.Invoke(totalCollected);
        }

        public string GetEnding()
        {
            int m = totalCollected;
            if (m >= 50000) return "S 结局：破纪录";
            if (m >= 40000) return "A 结局：王牌";
            if (m >= 25000) return "B 结局：骨干";
            if (m >= 10000) return "C 结局：达标";
            return "D 结局：未达标";
        }
    }
}