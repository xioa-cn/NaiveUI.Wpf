using System.Globalization;

namespace NaiveUI.NControls.ControlsExample;

public static class LunarCalendarConverter {
    /// <summary>
    /// 将公历日期转换为农历日期字符串
    /// </summary>
    public static string ConvertToLunarDate(this DateTime solarDate) {
        // 创建中国农历日历实例
        ChineseLunisolarCalendar lunarCalendar = new ChineseLunisolarCalendar();

        // 获取农历年份、月份、日期
        int lunarYear = lunarCalendar.GetYear(solarDate);
        int lunarMonth = lunarCalendar.GetMonth(solarDate);
        int lunarDay = lunarCalendar.GetDayOfMonth(solarDate);

        // 判断是否为闰月（注意：闰月的表示方式为月份后加“闰”字）
        bool isLeapMonth = lunarCalendar.IsLeapMonth(lunarYear, lunarMonth,
            lunarCalendar.GetEra(solarDate));
     
        string monthStr = isLeapMonth ? "闰" + GetLunarMonthName(lunarMonth) : GetLunarMonthName(lunarMonth);
        string dayStr = GetLunarDayName(lunarDay);

        // 组合农历日期字符串
        return $"农历：{monthStr}{dayStr}";
    }

    /// <summary>
    /// 获取农历月份的中文名称
    /// </summary>
    public static string GetLunarMonthName(int month) {
        var hMonth = month switch
        {
            1 => "一",
            2 => "二",
            3 => "三",
            4 => "四",
            5 => "五",
            6 => "六",
            7 => "七",
            8 => "八",
            9 => "九",
            10 => "十",
            11 => "冬",
            12 => "腊",
            13 => "一",
            _ => "未知"
        };
        return hMonth + "月";
    }

   

    /// <summary>
    /// 获取农历日期的中文名称（初一、初二...三十）
    /// </summary>
    public static string GetLunarDayName(int day) {
        string[] dayNames = [
            "初一", "初二", "初三", "初四", "初五", "初六", "初七", "初八", "初九", "初十",
            "十一", "十二", "十三", "十四", "十五", "十六", "十七", "十八", "十九", "二十",
            "廿一", "廿二", "廿三", "廿四", "廿五", "廿六", "廿七", "廿八", "廿九", "三十"
        ];

        // 确保日期在有效范围内（1-30）
        if (day < 1 || day > 30)
            return "无效日期";

        return dayNames[day - 1];
    }
}