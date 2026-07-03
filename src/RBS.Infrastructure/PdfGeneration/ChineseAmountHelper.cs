namespace RBS.Infrastructure.PdfGeneration;

/// <summary>
/// 中文金额大写转换工具 — 人民币金额转大写（如 "壹仟贰佰叁拾肆元伍角陆分"）
/// </summary>
public static class ChineseAmountHelper
{
    private static readonly string[] Digits = { "零", "壹", "贰", "叁", "肆", "伍", "陆", "柒", "捌", "玖" };
    private static readonly string[] Radices = { "", "拾", "佰", "仟" };
    private static readonly string[] BigRadices = { "", "万", "亿", "万亿" };

    /// <summary>金额转中文大写（支持最大 9999 亿）</summary>
    public static string Convert(decimal amount)
    {
        if (amount == 0) return "零元整";

        var numStr = amount.ToString("F2");
        var parts = numStr.Split('.');
        var integerPart = parts[0];
        var decimalPart = parts[1];

        var result = ConvertInteger(integerPart) + "元";
        result += ConvertDecimal(decimalPart);

        return result;
    }

    private static string ConvertInteger(string numStr)
    {
        // 去掉前导零
        numStr = numStr.TrimStart('0');
        if (string.IsNullOrEmpty(numStr)) return "零";

        var len = numStr.Length;
        if (len > 14) throw new ArgumentOutOfRangeException(nameof(numStr), "金额超出支持范围（最大 9999 亿）");

        var result = "";
        var pos = 0;

        if (len > 12) // 亿级部分
        {
            var part = len - 12;
            result += ConvertSection(numStr.Substring(0, part)) + "亿";
            numStr = numStr.Substring(part);
            len = numStr.Length;
            pos = 1;
        }

        if (len > 8) // 万级部分
        {
            var part = len - 8;
            var section = ConvertSection(numStr.Substring(0, part));
            if (section.Length > 0)
                result += section + "万";
            else if (pos == 1 && result.EndsWith("亿"))
                result += "零";
            numStr = numStr.Substring(part);
            len = numStr.Length;
            pos = 2;
        }

        if (len > 4) // 个级部分（前面的万级）
        {
            var part = len - 4;
            var section = ConvertSection(numStr.Substring(0, part));
            if (section.Length > 0)
                result += section + "万";
            else if (pos >= 1)
                result += "零";
            numStr = numStr.Substring(part);
            len = numStr.Length;
            pos = 3;
        }

        var lastSection = ConvertSection(numStr);
        if (lastSection.Length > 0)
        {
            if (pos >= 1 && lastSection.Length == 1 && lastSection[0] == '零')
                result += lastSection;
            else
                result += lastSection;
        }
        else if (pos >= 1)
        {
            // 末尾节为零，补零
            if (!result.EndsWith("零"))
                result += "零";
        }

        return result;
    }

    private static string ConvertSection(string numStr)
    {
        var len = numStr.Length;
        if (len == 0) return "";

        var result = "";
        var zero = true;

        for (int i = 0; i < len; i++)
        {
            var n = numStr[i] - '0';
            if (n != 0)
            {
                if (zero)
                {
                    result += Digits[n];
                    zero = false;
                }
                else
                {
                    result += Digits[n];
                }
                result += Radices[len - 1 - i];
            }
            else
            {
                if (!zero)
                {
                    result += "零";
                    zero = true;
                }
            }
        }

        return result;
    }

    private static string ConvertDecimal(string decimalPart)
    {
        var jiao = decimalPart[0] - '0';
        var fen = decimalPart[1] - '0';

        if (jiao == 0 && fen == 0) return "整";

        var result = "";
        if (jiao > 0)
            result += Digits[jiao] + "角";
        else if (fen > 0)
            result += "零";

        if (fen > 0)
            result += Digits[fen] + "分";

        return result;
    }
}
