using DynamicTranslate.Demo.Domain;
using Microsoft.EntityFrameworkCore;
using DynamicTranslate.DB;
namespace DynamicTranslate.Demo.Infrastructure
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<LookupCategory> LookupCategories { get; set; }
        public DbSet<LookupMaster> LookupMasters { get; set; }
        public DbSet<LookupDetail> LookupDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.AddTranslationConfiguration();

            base.OnModelCreating(modelBuilder);

            // LookupCategory Configuration
            modelBuilder.Entity<LookupCategory>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);


                entity.HasMany(e => e.LookupMasters)
                    .WithOne(e => e.Category)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LookupMaster Configuration
            modelBuilder.Entity<LookupMaster>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(e => e.Category)
                    .WithMany(e => e.LookupMasters)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.LookupDetails)
                    .WithOne(e => e.Master)
                    .HasForeignKey(e => e.MasterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LookupDetail Configuration
            modelBuilder.Entity<LookupDetail>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.HasOne(e => e.Master)
                    .WithMany(e => e.LookupDetails)
                    .HasForeignKey(e => e.MasterId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ============ SEED DATA ============

            modelBuilder.Entity<LookupCategory>().HasData(
                new LookupMaster { Id = 1, Name = "books" }
            );

            // Seed LookupMasters (Book Categories)
            modelBuilder.Entity<LookupMaster>().HasData(
                new LookupMaster { Id = 1, Name = "Computer Science", CategoryId = 1 },
                new LookupMaster { Id = 2, Name = "Physics", CategoryId = 1 },
                new LookupMaster { Id = 3, Name = "Mathematics", CategoryId = 1 },
                new LookupMaster { Id = 4, Name = "Biology", CategoryId = 1 },
                new LookupMaster { Id = 5, Name = "History", CategoryId = 1 }
            );

            // Seed LookupDetails (Book Names)
            modelBuilder.Entity<LookupDetail>().HasData(
                // Computer Science Books (MasterId = 1)
                new LookupDetail { Id = 1, Name = "Clean Code: A Handbook of Agile Software Craftsmanship", MasterId = 1 },
                new LookupDetail { Id = 2, Name = "Design Patterns: Elements of Reusable Object-Oriented Software", MasterId = 1 },
                new LookupDetail { Id = 5, Name = "Introduction to Algorithms", MasterId = 1 },
                new LookupDetail { Id = 6, Name = "The Pragmatic Programmer", MasterId = 1 },
                new LookupDetail { Id = 7, Name = "Computer Networking: A Top-Down Approach", MasterId = 1 },

                // Physics Books (MasterId = 2)
                new LookupDetail { Id = 3, Name = "Quantum Physics", MasterId = 2 },
                new LookupDetail { Id = 4, Name = "Hardware", MasterId = 2 },
                new LookupDetail { Id = 8, Name = "The Feynman Lectures on Physics", MasterId = 2 },
                new LookupDetail { Id = 9, Name = "Introduction to Quantum Mechanics", MasterId = 2 },
                new LookupDetail { Id = 10, Name = "Classical Mechanics", MasterId = 2 },
                new LookupDetail { Id = 11, Name = "Thermodynamics and Statistical Mechanics", MasterId = 2 },

                // Mathematics Books (MasterId = 3)
                new LookupDetail { Id = 12, Name = "Calculus: Early Transcendentals", MasterId = 3 },
                new LookupDetail { Id = 13, Name = "Linear Algebra and Its Applications", MasterId = 3 },
                new LookupDetail { Id = 14, Name = "Probability and Statistics", MasterId = 3 },
                new LookupDetail { Id = 15, Name = "Discrete Mathematics and Its Applications", MasterId = 3 },

                // Biology Books (MasterId = 4)
                new LookupDetail { Id = 16, Name = "Molecular Biology of the Cell", MasterId = 4 },
                new LookupDetail { Id = 17, Name = "Genetics: Analysis of Genes and Genomes", MasterId = 4 },
                new LookupDetail { Id = 18, Name = "Ecology: Concepts and Applications", MasterId = 4 },

                // History Books (MasterId = 5)
                new LookupDetail { Id = 19, Name = "Sapiens: A Brief History of Humankind", MasterId = 5 },
                new LookupDetail { Id = 20, Name = "The History of the Ancient World", MasterId = 5 },
                new LookupDetail { Id = 21, Name = "A People's History of the United States", MasterId = 5 }
            );
        }
    }
}
