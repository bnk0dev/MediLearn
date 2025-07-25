using Medilearn.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Medilearn.Data.Contexts
{
    public class MedilearnDbContext : DbContext
    {
        public MedilearnDbContext(DbContextOptions<MedilearnDbContext> options)
            : base(options)
        {
        }
        public DbSet<User> Users { get; set; } 
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<CourseMaterial> CourseMaterials { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .Property(u => u.Status)
                .HasDefaultValue(Data.Enums.UserStatus.Pending);
            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Personnel)
                .WithMany()  
                .HasForeignKey(e => e.PersonnelTCNo)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Enrollment>()
                .HasOne(e => e.Course)
                .WithMany(c => c.Enrollments)  
                .HasForeignKey(e => e.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor) 
                .WithMany()                
                .HasForeignKey(c => c.InstructorTCNo) 
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
