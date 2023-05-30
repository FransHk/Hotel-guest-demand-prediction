using System.Globalization;

namespace Hotel_Demand_Blazor.Models
{
    public class DateTimeHelper
    { 
        public string DayStr
        {
            get { return dayStr; }
        }
        public string YearStr
        {
            get { return yearStr; }
        }
        public string MonthStr
        {
            get { return monthStr; }
        }
        public string MonthNameStr
        {
            get { return monthNameStr; }
        }
        public string MonthDayStr
        {
            get { return monthDayStr; }
        }
        public DateTime Date
        {
            get { return date; }
        }

        private DateTime date;
        private string dayStr;
        private string yearStr;
        private string monthStr;
        private string monthNameStr;
        private string monthDayStr;

        public DateTimeHelper(string dateString, int addDays=0)
        {
            date = DateTime.Parse(dateString);
            date = date.AddDays(addDays);
            dayStr = date.Day.ToString();
            yearStr = date.Year.ToString();
            monthStr = date.Month.ToString();
            monthNameStr = date.ToString("MMM", CultureInfo.InvariantCulture);
            monthDayStr = date.ToString("MMM dd", CultureInfo.InvariantCulture);
        }

        public DateTimeHelper(DateTime date, int addDays = 0)
        {
            this.date = date;
            date = date.AddDays(1);
            dayStr = date.Day.ToString();
            yearStr = date.Year.ToString();
            monthStr = date.Month.ToString();
            monthNameStr = date.ToString("MMM", CultureInfo.InvariantCulture);
            monthDayStr = date.ToString("MMM dd", CultureInfo.InvariantCulture);
        }


        public void AddDays(int days)
        {
            date = date.AddDays(days);
            dayStr = date.Day.ToString();
            yearStr = date.Year.ToString();
            monthStr = date.Month.ToString();
            monthNameStr = date.ToString("MMM", CultureInfo.InvariantCulture);
            monthDayStr = date.ToString("MMM dd", CultureInfo.InvariantCulture);
        }

    }
}
