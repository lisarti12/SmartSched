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
            _context.SmartSchedRunLogs.Add(new SmartSchedRunLog
            {
                StudentId = studentId,
                RunAt = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            // remove only generated suggestions, keep accepted/fixed ones
            var oldSuggestions = await _context.ScheduleSuggestions
                .Where(s => s.StudentId == studentId && s.Status == "Suggested")
                .ToListAsync();

            if (oldSuggestions.Any())
            {
                _context.ScheduleSuggestions.RemoveRange(oldSuggestions);
                await _context.SaveChangesAsync();
            }

            var tasks = await _context.TaskItems
                .Where(t => t.StudentId == studentId && t.Status != "Completed")
                .OrderBy(t => t.Deadline)
                .ToListAsync();

            if (!tasks.Any())
                return;

            var allAvailability = await _context.AvailabilityRules
                .Where(a => a.StudentId == studentId && a.IsAvailable)
                .OrderBy(a => a.AvailableDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            var holidays = await _context.HolidayBlocks
                .Where(h => h.StudentId == studentId)
                .ToListAsync();

            var settings = await _context.SystemSettings.FirstOrDefaultAsync();
            if (settings == null)
                return;

            var fixedSuggestions = await _context.ScheduleSuggestions
                .Where(s => s.StudentId == studentId && s.Status != "Suggested")
                .ToListAsync();

            var occupiedByDate = fixedSuggestions
                .GroupBy(s => s.ScheduledDate.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => new TimeRange(x.StartTime, x.EndTime)).OrderBy(x => x.Start).ToList()
                );

            var scoredTasks = tasks
                .Select(t => new
                {
                    Task = t,
                    Score = CalculateTaskScore(t)
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Task.Deadline)
                .ToList();

            foreach (var scored in scoredTasks)
            {
                var task = scored.Task;
                int remainingHours = task.EstimatedHours;
                DateTime currentDate = DateTime.Today;

                while (remainingHours > 0 && currentDate.Date <= task.Deadline.Date)
                {
                    bool isHoliday = holidays.Any(h =>
                        currentDate.Date >= h.StartDate.Date &&
                        currentDate.Date <= h.EndDate.Date);

                    if (isHoliday)
                    {
                        currentDate = currentDate.AddDays(1);
                        continue;
                    }

                    var dailyRules = GetAvailabilityForDate(studentId, currentDate, allAvailability, settings);

                    if (!dailyRules.Any())
                    {
                        currentDate = currentDate.AddDays(1);
                        continue;
                    }

                    if (!occupiedByDate.ContainsKey(currentDate.Date))
                    {
                        occupiedByDate[currentDate.Date] = new List<TimeRange>();
                    }

                    foreach (var rule in dailyRules.OrderBy(r => r.StartTime))
                    {
                        if (remainingHours <= 0)
                            break;

                        if (rule.EndTime <= rule.StartTime)
                            continue;

                        int alreadyScheduledHoursForThisDate = occupiedByDate[currentDate.Date]
                            .Sum(x => (int)Math.Max(0, Math.Floor((x.End - x.Start).TotalHours)));

                        int dailyCap = settings.DefaultMaxStudyHoursPerDay;
                        int remainingDailyCapacity = Math.Max(0, dailyCap - alreadyScheduledHoursForThisDate);

                        if (remainingDailyCapacity <= 0)
                            continue;

                        var freeBlocks = GetFreeBlocks(rule.StartTime, rule.EndTime, occupiedByDate[currentDate.Date]);

                        foreach (var block in freeBlocks)
                        {
                            if (remainingHours <= 0)
                                break;

                            int blockHours = (int)Math.Floor((block.End - block.Start).TotalHours);
                            if (blockHours <= 0)
                                continue;

                            int alloc = Math.Min(remainingHours, Math.Min(blockHours, remainingDailyCapacity));
                            if (alloc <= 0)
                                continue;

                            var start = block.Start;
                            var end = start.Add(TimeSpan.FromHours(alloc));

                            var suggestion = new ScheduleSuggestion
                            {
                                StudentId = studentId,
                                TaskItemId = task.Id,
                                ScheduledDate = currentDate.Date,
                                StartTime = start,
                                EndTime = end,
                                AllocatedHours = alloc,
                                Status = "Suggested",
                                CreatedAt = DateTime.UtcNow
                            };

                            _context.ScheduleSuggestions.Add(suggestion);
                            occupiedByDate[currentDate.Date].Add(new TimeRange(start, end));
                            occupiedByDate[currentDate.Date] = occupiedByDate[currentDate.Date]
                                .OrderBy(x => x.Start)
                                .ToList();

                            remainingHours -= alloc;
                            remainingDailyCapacity -= alloc;
                        }
                    }

                    currentDate = currentDate.AddDays(1);
                }

                task.Status = remainingHours == 0 ? "Scheduled" : "Pending";
            }

            await _context.SaveChangesAsync();
            await GenerateWorkloadMetricsAsync(studentId);
        }

        private List<AvailabilityRule> GetAvailabilityForDate(
            string studentId,
            DateTime currentDate,
            List<AvailabilityRule> allAvailability,
            SystemSetting settings)
        {
            var datedRules = allAvailability
                .Where(a => a.AvailableDate.HasValue && a.AvailableDate.Value.Date == currentDate.Date)
                .ToList();

            if (datedRules.Any())
                return datedRules;

            var legacyRules = allAvailability
                .Where(a => !a.AvailableDate.HasValue &&
                            string.Equals(a.DayOfWeek, currentDate.DayOfWeek.ToString(), StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (legacyRules.Any())
                return legacyRules;

            if (!allAvailability.Any())
            {
                return new List<AvailabilityRule>
                {
                    new AvailabilityRule
                    {
                        StudentId = studentId,
                        DayOfWeek = currentDate.DayOfWeek.ToString(),
                        AvailableDate = currentDate.Date,
                        StartTime = new TimeSpan(9, 0, 0),
                        EndTime = new TimeSpan(23, 0, 0),
                        IsAvailable = true
                    }
                };
            }

            return new List<AvailabilityRule>();
        }

        private List<TimeRange> GetFreeBlocks(TimeSpan slotStart, TimeSpan slotEnd, List<TimeRange> occupied)
        {
            var relevant = occupied
                .Where(x => x.End > slotStart && x.Start < slotEnd)
                .OrderBy(x => x.Start)
                .ToList();

            var free = new List<TimeRange>();
            var cursor = slotStart;

            foreach (var item in relevant)
            {
                if (item.Start > cursor)
                {
                    free.Add(new TimeRange(cursor, item.Start));
                }

                if (item.End > cursor)
                {
                    cursor = item.End;
                }
            }

            if (cursor < slotEnd)
            {
                free.Add(new TimeRange(cursor, slotEnd));
            }

            return free;
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

            return (task.Priority * 4) + (task.Difficulty * 2) + (deadlineUrgency * 3) + effortWeight;
        }

        private sealed record TimeRange(TimeSpan Start, TimeSpan End);
    }
}