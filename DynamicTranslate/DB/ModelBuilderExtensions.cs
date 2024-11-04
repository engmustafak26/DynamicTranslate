using Microsoft.EntityFrameworkCore;

namespace DynamicTranslate.DB
{
    public static class ModelBuilderExtensions
    {


        public static void AddTranslationConfiguration(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OverrideTranslation>().ToTable("OverrideTranslations", "Translation");
            modelBuilder.Entity<OverrideTranslationDetail>().ToTable("OverrideTranslationDetails", "Translation");

            modelBuilder.Entity<OverrideTranslation>().HasKey(x => x.Id);
            modelBuilder.Entity<OverrideTranslation>().Property(x => x.LanguageCode).HasMaxLength(10).IsRequired(false);
            modelBuilder.Entity<OverrideTranslation>().Property(x => x.Entity).HasMaxLength(150).IsRequired(false);
            modelBuilder.Entity<OverrideTranslation>().Property(x => x.Property).HasMaxLength(150).IsRequired(false);
            modelBuilder.Entity<OverrideTranslation>().Property(x => x.Key).HasMaxLength(50).IsRequired(false);
            modelBuilder.Entity<OverrideTranslation>().Property(x => x.Text).HasMaxLength(3500).IsRequired(true);


            modelBuilder.Entity<OverrideTranslation>().HasIndex(x => new { x.LanguageCode, x.Entity, x.Property, x.Key }).IsUnique(true);
            modelBuilder.Entity<OverrideTranslation>().HasIndex(x => x.Text);



            modelBuilder.Entity<OverrideTranslationDetail>().HasKey(x => x.Id);
            modelBuilder.Entity<OverrideTranslationDetail>().Property(x => x.LanguageCode).HasMaxLength(10).IsRequired(true);
            modelBuilder.Entity<OverrideTranslationDetail>().Property(x => x.Translation).HasMaxLength(3500).IsRequired(true);
        }
    }
}
