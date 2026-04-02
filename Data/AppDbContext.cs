using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SmartSched.Api.Models;

namespace SmartSched.Api.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Announcement> Announcements => Set<Announcement>();
        public DbSet<StudentTaskNotification> StudentTaskNotifications => Set<StudentTaskNotification>();
        public DbSet<TaskItem> TaskItems => Set<TaskItem>();
        public DbSet<AvailabilityRule> AvailabilityRules => Set<AvailabilityRule>();
        public DbSet<ScheduleSuggestion> ScheduleSuggestions => Set<ScheduleSuggestion>();
        public DbSet<WorkloadMetric> WorkloadMetrics => Set<WorkloadMetric>();
        public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
        public DbSet<CourseClass> CourseClasses => Set<CourseClass>();
        public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
        public DbSet<CourseContentItem> CourseContentItems => Set<CourseContentItem>();
        public DbSet<LectureItem> LectureItems => Set<LectureItem>();
        public DbSet<StudentNotification> StudentNotifications => Set<StudentNotification>();
        public DbSet<HolidayBlock> HolidayBlocks => Set<HolidayBlock>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Announcement>()
                .HasOne(a => a.Professor)
                .WithMany()
                .HasForeignKey(a => a.ProfessorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StudentTaskNotification>()
                .HasOne(sn => sn.Student)
                .WithMany()
                .HasForeignKey(sn => sn.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TaskItem>()
                .HasOne(t => t.Student)
                .WithMany()
                .HasForeignKey(t => t.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<AvailabilityRule>()
                .HasOne(a => a.Student)
                .WithMany()
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ScheduleSuggestion>()
                .HasOne(s => s.Student)
                .WithMany()
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<ScheduleSuggestion>()
                .HasOne(s => s.TaskItem)
                .WithMany()
                .HasForeignKey(s => s.TaskItemId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<WorkloadMetric>()
                .HasOne(w => w.Student)
                .WithMany()
                .HasForeignKey(w => w.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CourseClass>()
                .HasOne(c => c.Professor)
                .WithMany()
                .HasForeignKey(c => c.ProfessorId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CourseEnrollment>()
                .HasOne(e => e.CourseClass)
                .WithMany()
                .HasForeignKey(e => e.CourseClassId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CourseEnrollment>()
                .HasOne(e => e.Student)
                .WithMany()
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<CourseContentItem>()
                .HasOne(ci => ci.CourseClass)
                .WithMany()
                .HasForeignKey(ci => ci.CourseClassId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<LectureItem>()
                .HasOne(l => l.CourseClass)
                .WithMany()
                .HasForeignKey(l => l.CourseClassId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<TaskItem>()
                .HasOne(t => t.CourseContentItem)
                .WithMany()
                .HasForeignKey(t => t.CourseContentItemId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StudentTaskNotification>()
                .HasOne(n => n.Student)
                .WithMany()
                .HasForeignKey(n => n.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StudentTaskNotification>()
                .HasOne(n => n.CourseClass)
                .WithMany()
                .HasForeignKey(n => n.CourseClassId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StudentTaskNotification>()
                .HasOne(n => n.CourseContentItem)
                .WithMany()
                .HasForeignKey(n => n.CourseContentItemId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<HolidayBlock>()
                .HasOne(h => h.Student)
                .WithMany()
                .HasForeignKey(h => h.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StudentNotification>()
                .HasOne(sn => sn.Student)
                .WithMany()
                .HasForeignKey(sn => sn.StudentId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<StudentNotification>()
                .HasOne(sn => sn.Announcement)
                .WithMany()
                .HasForeignKey(sn => sn.AnnouncementId)
                .OnDelete(DeleteBehavior.NoAction);

            // NEW: filtered unique index to avoid duplicate availability rows
            builder.Entity<AvailabilityRule>()
                .HasIndex(a => new { a.StudentId, a.AvailableDate, a.StartTime, a.EndTime })
                .IsUnique()
                .HasFilter("[AvailableDate] IS NOT NULL");

            // Optional helpful index for holiday lookups
            builder.Entity<HolidayBlock>()
                .HasIndex(h => new { h.StudentId, h.StartDate, h.EndDate });
        }
    }
}