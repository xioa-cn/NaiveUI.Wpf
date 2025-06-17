namespace NaiveUI.NControls.ControlsExample;

using System;
using System.Collections.Generic;

/// <summary>
/// 表示某天的日期信息
/// </summary>
public class DayInfo
{
    public int NowMonth { get; set; }

    public bool IsNowMonth
    {
        get
        {
            return this.NowMonth == this.Month;
        }
    }

    public bool IsNowDay { get
        {
            return Date.Date == DateTime.Now.Date;
        } }

    public DateTime Date { get; set; }        // 日期
    private string? weekDay;
    public string Weekday
    {
        get
        {
            if (string.IsNullOrEmpty(weekDay))
            {
                weekDay = Date.DayOfWeek.GetChineseWeekday();
            }
            return weekDay;
        }
        set
        {
            weekDay = value;
        }
    }       // 星期几（中文）
    private bool? isWeekend = null;
    public bool IsWeekend
    {
        get
        {
            if (isWeekend == null)
            {
                isWeekend = Date.DayOfWeek == DayOfWeek.Saturday ||
                            Date.DayOfWeek == DayOfWeek.Sunday;
            }
            return (bool)isWeekend;
        }
        set
        {
            isWeekend = value;
        }
    }       // 是否为周末

    public string DateString { get => Date.ToString("yyyy-MM-dd"); }
    public int Day { get => Date.Day; }

    public int Month { get => Date.Month; }
}

/// <summary>
/// 表示整个月份的信息
/// </summary>
public class MonthInfo
{
    public int Year { get; set; }             // 年份
    public int Month { get; set; }            // 月份
    public int DaysInMonth { get; set; }      // 当月天数
    public List<DayInfo> Days { get; set; }   // 每一天的详细信息

    public MonthInfo()
    {
        Days = new List<DayInfo>();
    }
}

public static class DateHelper
{
    /// <summary>
    /// 根据指定日期生成当月的完整信息
    /// </summary>
    /// <param name="date">任意一天的日期</param>
    /// <returns>包含当月所有日期信息的MonthInfo对象</returns>
    public static MonthInfo GenerateMonthInfo(DateTime date)
    {
        MonthInfo monthInfo = new MonthInfo
        {
            Year = date.Year,
            Month = date.Month,
            // 获取当月总天数（自动处理闰年2月）
            DaysInMonth = DateTime.DaysInMonth(date.Year, date.Month)
        };

        // 生成当月每一天的信息
        for (int day = 1; day <= monthInfo.DaysInMonth; day++)
        {
            DateTime currentDay = new DateTime(date.Year, date.Month, day);
            DayInfo dayInfo = new DayInfo
            {
                NowMonth = date.Month,
                Date = currentDay,
                // 将DayOfWeek转换为中文星期
                Weekday = currentDay.DayOfWeek.GetChineseWeekday(),
                // 判断是否为周末（周六或周日）
                IsWeekend = currentDay.DayOfWeek == DayOfWeek.Saturday ||
                            currentDay.DayOfWeek == DayOfWeek.Sunday
            };

            monthInfo.Days.Add(dayInfo);
        }

        return monthInfo;
    }

    /// <summary>
    /// 将DayOfWeek枚举转换为中文星期
    /// </summary>
    public static string GetChineseWeekday(this DayOfWeek dayOfWeek)
    {
        string[] weekdays = { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
        return weekdays[(int)dayOfWeek];
    }
}