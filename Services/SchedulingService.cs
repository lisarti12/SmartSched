using Microsoft.EntityFrameworkCore;
using SmartSched.Api.Data;
using SmartSched.Api.Models;

namespace SmartSched.Api.Services
{
    public class SchedulingService
    {
        private readonly AppDbContext _context;

        public SchedulingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task GenerateScheduleForStudentAsync(string studentId)
        {
            var existingSuggestions = await _context.ScheduleSuggestions
                .Where(s => s.StudentId == studentId && s.Status == "Suggested")
                .ToListAsync();

            if (existingSuggestions.Any())
            {
                _context.ScheduleSuggestions.RemoveRange(existingSuggestions);
                await _context.SaveChangesAsync();
            }

            var tasks = await _context.TaskItems
                .Where(t => t.StudentId == studentId && t.Status != "Completed")
                .OrderBy(t => t.Deadline)
                .ToListAsync();

            var availabilityRules = await _context.AvailabilityRules
                .Where(a => a.StudentId == studentId && a.IsAvailable)
                .ToListAsync();

            var settings = await _context.SystemSettings.FirstOrDefaultAsync();

            if (settings == null || !tasks.Any() || !availabilityRules.Any())
                return;

            var scoredTasks = tasks
                .Select(t => new
                {
                    Task = t,
                    Score = CalculateTaskScore(t)
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Task.Deadline)
                .ToList();

            foreach (var item in scoredTasks)
            {
                var task = item.Task;
                var remainingHours = task.EstimatedHours;
                var currentDate = DateTime.Today;

                while (remainingHours > 0 && currentDate.Date <= task.Deadline.Date)
                {
                    var dayName = currentDate.DayOfWeek.ToString();

                    var rule = availabilityRules.FirstOrDefault(r => r.DayOfWeek == dayName);
                    if (rule == null)
                    {
                        currentDate = currentDate.AddDays(1);
                        continue;
                    }

                    var existingHoursForDay = await _context.ScheduleSuggestions
                        .Where(s => s.StudentId == studentId && s.ScheduledDate.Date == currentDate.Date)
                        .SumAsync(s => (int?)s.AllocatedHours) ?? 0;

                    var maxHoursForDay = rule.MaxStudyHours > 0
                        ? rule.MaxStudyHours
                        : settings.DefaultMaxStudyHoursPerDay;

                    var availableHoursForDay = maxHoursForDay - existingHoursForDay;

                    if (availableHoursForDay <= 0)
                    {
                        currentDate = currentDate.AddDays(1);
                        continue;
                    }

                    var startHour = rule.StartTime.Hours;
                    var startMinute = rule.StartTime.Minutes;
                    var allocatedHours = Math.Min(remainingHours, availableHoursForDay);

                    var suggestion = new ScheduleSuggestion
                    {
                        StudentId = studentId,
                        TaskItemId = task.Id,
                        ScheduledDate = currentDate.Date,
                        StartTime = new TimeSpan(startHour, startMinute, 0),
                        EndTime = new TimeSpan(startHour + allocatedHours, startMinute, 0),
                        AllocatedHours = allocatedHours,
                        Status = "Suggested",
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.ScheduleSuggestions.Add(suggestion);

                    remainingHours -= allocatedHours;
                    currentDate = currentDate.AddDays(1);
                }

                task.Status = remainingHours == 0 ? "Scheduled" : "Pending";
            }

            await _context.SaveChangesAsync();
            await GenerateWorkloadMetricsAsync(studentId);
        }

        public async Task GenerateWorkloadMetricsAsync(string studentId)
        {
            var oldMetrics = await _context.WorkloadMetrics
                .Where(w => w.StudentId == studentId)
                .ToListAsync();

            if (oldMetrics.Any())
            {
                _context.WorkloadMetrics.RemoveRange(oldMetrics);
                await _context.SaveChangesAsync();
            }

            var groupedSuggestions = await _context.ScheduleSuggestions
                .Where(s => s.StudentId == studentId)
                .GroupBy(s => s.ScheduledDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    TotalHours = g.Sum(x => x.AllocatedHours)
                })
                .ToListAsync();

            foreach (var day in groupedSuggestions)
            {
                var workloadScore = day.TotalHours * 2;
                var level = "Low";
                var warning = "";

                if (day.TotalHours >= 6)
                {
                    level = "High";
                    warning = "Overload risk detected.";
                }
                else if (day.TotalHours >= 4)
                {
                    level = "Medium";
                    warning = "Moderate workload.";
                }

                _context.WorkloadMetrics.Add(new WorkloadMetric
                {
                    StudentId = studentId,
                    Date = day.Date,
                    TotalScheduledHours = day.TotalHours,
                    WorkloadScore = workloadScore,
                    WorkloadLevel = level,
                    WarningMessage = warning
                });
            }

            await _context.SaveChangesAsync();
        }

        private int CalculateTaskScore(TaskItem task)
        {
            var daysUntilDeadline = (task.Deadline.Date - DateTime.Today).Days;

            int deadlineUrgency = daysUntilDeadline switch
            {
                <= 1 => 5,
                <= 3 => 4,
                <= 7 => 3,
                <= 14 => 2,
                _ => 1
            };

            int effortWeight = task.EstimatedHours >= 4 ? 2 : 1;

            return task.Priority * 3 + deadlineUrgency + effortWeight + task.Difficulty;
        }
    }
}
