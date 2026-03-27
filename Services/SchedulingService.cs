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
                // 1. Remove old generated suggestions
                var oldSuggestions = await _context.ScheduleSuggestions
                    .Where(s => s.StudentId == studentId && s.Status == "Suggested")
                    .ToListAsync();

                if (oldSuggestions.Any())
                {
                    _context.ScheduleSuggestions.RemoveRange(oldSuggestions);
                    await _context.SaveChangesAsync();
                }

                // 2. Load all unfinished tasks for this student
                var tasks = await _context.TaskItems
                    .Where(t => t.StudentId == studentId && t.Status != "Completed")
                    .OrderBy(t => t.Deadline)
                    .ToListAsync();

                if (!tasks.Any())
                    return;

                // 3. Load availability rules
                var availabilityRules = await _context.AvailabilityRules
                    .Where(a => a.StudentId == studentId && a.IsAvailable)
                    .ToListAsync();

                // 4. If no availability exists, use default 9 AM - 11 PM every day
                if (!availabilityRules.Any())
                {
                    availabilityRules = new List<AvailabilityRule>
            {
                new AvailabilityRule { StudentId = studentId, DayOfWeek = "Monday",    StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(23, 0, 0), MaxStudyHours = 8, IsAvailable = true },
                new AvailabilityRule { StudentId = studentId, DayOfWeek = "Tuesday",   StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(23, 0, 0), MaxStudyHours = 8, IsAvailable = true },
                new AvailabilityRule { StudentId = studentId, DayOfWeek = "Wednesday", StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(23, 0, 0), MaxStudyHours = 8, IsAvailable = true },
                new AvailabilityRule { StudentId = studentId, DayOfWeek = "Thursday",  StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(23, 0, 0), MaxStudyHours = 8, IsAvailable = true },
                new AvailabilityRule { StudentId = studentId, DayOfWeek = "Friday",    StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(23, 0, 0), MaxStudyHours = 8, IsAvailable = true },
                new AvailabilityRule { StudentId = studentId, DayOfWeek = "Saturday",  StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(23, 0, 0), MaxStudyHours = 8, IsAvailable = true },
                new AvailabilityRule { StudentId = studentId, DayOfWeek = "Sunday",    StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(23, 0, 0), MaxStudyHours = 8, IsAvailable = true }
            };
                }

                // 5. Load holidays/unavailable date ranges
                var holidays = await _context.HolidayBlocks
                    .Where(h => h.StudentId == studentId)
                    .ToListAsync();

                // 6. Load system settings
                var settings = await _context.SystemSettings.FirstOrDefaultAsync();
                if (settings == null)
                    return;

                // 7. Score and sort tasks
                var scoredTasks = tasks
                    .Select(t => new
                    {
                        Task = t,
                        Score = CalculateTaskScore(t)
                    })
                    .OrderByDescending(x => x.Score)
                    .ThenBy(x => x.Task.Deadline)
                    .ToList();

                // 8. Schedule each task
                foreach (var scored in scoredTasks)
                {
                    var task = scored.Task;
                    int remainingHours = task.EstimatedHours;
                    DateTime currentDate = DateTime.Today;

                    while (remainingHours > 0 && currentDate.Date <= task.Deadline.Date)
                    {
                        // Skip holidays
                        bool isHoliday = holidays.Any(h =>
                            currentDate.Date >= h.StartDate.Date &&
                            currentDate.Date <= h.EndDate.Date);

                        if (isHoliday)
                        {
                            currentDate = currentDate.AddDays(1);
                            continue;
                        }

                        // Find availability rule for current day
                        string dayName = currentDate.DayOfWeek.ToString();

                        var rule = availabilityRules.FirstOrDefault(r => r.DayOfWeek == dayName && r.IsAvailable);
                        if (rule == null)
                        {
                            currentDate = currentDate.AddDays(1);
                            continue;
                        }

                        // How many hours already scheduled that day?
                        int existingHoursForDay = await _context.ScheduleSuggestions
                            .Where(s => s.StudentId == studentId && s.ScheduledDate.Date == currentDate.Date)
                            .SumAsync(s => (int?)s.AllocatedHours) ?? 0;

                        // Daily max hours
                        int maxHoursForDay = rule.MaxStudyHours > 0
                            ? rule.MaxStudyHours
                            : settings.DefaultMaxStudyHoursPerDay;

                        int remainingCapacityForDay = maxHoursForDay - existingHoursForDay;

                        if (remainingCapacityForDay <= 0)
                        {
                            currentDate = currentDate.AddDays(1);
                            continue;
                        }

                        // Determine actual time window available that day
                        TimeSpan dayStart = rule.StartTime;
                        TimeSpan dayEnd = rule.EndTime;

                        // If previous suggestions exist that day, push start after latest scheduled end time
                        var latestSuggestionEnd = await _context.ScheduleSuggestions
                            .Where(s => s.StudentId == studentId && s.ScheduledDate.Date == currentDate.Date)
                            .OrderByDescending(s => s.EndTime)
                            .Select(s => (TimeSpan?)s.EndTime)
                            .FirstOrDefaultAsync();

                        TimeSpan actualStart = latestSuggestionEnd ?? dayStart;

                        // Hours available in the clock window
                        double timeWindowHours = (dayEnd - actualStart).TotalHours;

                        if (timeWindowHours <= 0)
                        {
                            currentDate = currentDate.AddDays(1);
                            continue;
                        }

                        int availableByClock = (int)Math.Floor(timeWindowHours);
                        if (availableByClock <= 0)
                        {
                            currentDate = currentDate.AddDays(1);
                            continue;
                        }

                        // Final allocation = min(task left, daily cap left, clock time left)
                        int allocatedHours = Math.Min(remainingHours, Math.Min(remainingCapacityForDay, availableByClock));

                        if (allocatedHours <= 0)
                        {
                            currentDate = currentDate.AddDays(1);
                            continue;
                        }

                        var suggestion = new ScheduleSuggestion
                        {
                            StudentId = studentId,
                            TaskItemId = task.Id,
                            ScheduledDate = currentDate.Date,
                            StartTime = actualStart,
                            EndTime = actualStart.Add(TimeSpan.FromHours(allocatedHours)),
                            AllocatedHours = allocatedHours,
                            Status = "Suggested",
                            CreatedAt = DateTime.UtcNow
                        };

                        _context.ScheduleSuggestions.Add(suggestion);

                        remainingHours -= allocatedHours;
                        currentDate = currentDate.AddDays(1);
                    }

                    // 9. Update task status
                    task.Status = remainingHours == 0 ? "Scheduled" : "Pending";
                }

                // 10. Save generated schedule
                await _context.SaveChangesAsync();

                // 11. Recalculate workload metrics
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
