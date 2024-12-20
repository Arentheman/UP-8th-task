using System;
using System.Data;

namespace curs_trsbd
{
    public static class AvarageTime
    {
        static string result;
        public static string UpdateAverageCompletionTime(DataTable dataTable)
        {
            try
            {
                TimeSpan totalDuration = TimeSpan.Zero;
                int completedCount = 0;

                foreach (DataRow row in dataTable.Rows)
                {
                    if (row["completionDate"] != DBNull.Value && row["startDate"] != DBNull.Value)
                    {
                        DateTime startDate = Convert.ToDateTime(row["startDate"]);
                        DateTime completionDate = Convert.ToDateTime(row["completionDate"]);

                        totalDuration += (completionDate - startDate);
                        completedCount++;
                    }
                }

                if (completedCount > 0)
                {
                    double avgDuration = totalDuration.TotalDays / completedCount;
                    return $"Среднее время выполнения: {avgDuration:F1} дней";
                }
                else
                {
                    return "Среднее время выполнения: нет завершенных заявок";
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка при вычислении среднего времени выполнения: {ex.Message}";
            }
        }
    }
}
