//using System.Text.RegularExpressions;

//namespace DebtJam
//{
//    public static class ContactFormat
//    {
//        public static string CleanPhone(string raw)
//        {
//            if (string.IsNullOrWhiteSpace(raw)) return "";
//            return Regex.Replace(raw, @"\s+", "");
//        }

//        public static string PrettyPhone(string raw)
//        {
//            raw = CleanPhone(raw);
//            // 你可以按地区定制，这里做个简单例子
//            if (raw.Length == 10) return $"({raw[..3]}) {raw.Substring(3, 3)}-{raw.Substring(6)}";
//            if (raw.Length == 11 && raw.StartsWith("1")) return $"1 {raw[1..4]} {raw.Substring(4, 3]} {raw.Substring(7)}";
//            return raw;
//        }
//    }
//}
// Assets/Scripts/Utils/ContactFormat.cs
using System.Linq;

namespace DebtJam
{
    /// <summary>
    /// 统一的联系人字段格式化。需要更复杂的地区格式时在这里扩展即可。
    /// </summary>
    public static class ContactFormat
    {
        /// <summary>
        /// 友好化手机/电话。会保留数字并尝试插入分隔符；失败则原样返回。
        /// </summary>
        public static string PrettyPhone(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;

            // 仅保留数字
            var digits = new string(raw.Where(char.IsDigit).ToArray());
            if (digits.Length == 0) return raw.Trim();

            // 常见 11/10 位格式（示例：1 234 567 8901 或 234 567 8901）
            if (digits.Length == 11 && digits[0] == '1')
            {
                var d = digits.Substring(1);
                return $"{d.Substring(0, 3)}-{d.Substring(3, 3)}-{d.Substring(6)}";
            }
            if (digits.Length == 10)
            {
                return $"{digits.Substring(0, 3)}-{digits.Substring(3, 3)}-{digits.Substring(6)}";
            }

            // 其它长度：不强行格式化，直接返回原值（去两端空格）
            return raw.Trim();
        }

        /// <summary>
        /// 友好化地址。这里先简单 Trim；后面你要拆省市区、邮编等，都集中改这个函数。
        /// </summary>
        public static string PrettyAddress(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            return raw.Trim();
        }
    }
}