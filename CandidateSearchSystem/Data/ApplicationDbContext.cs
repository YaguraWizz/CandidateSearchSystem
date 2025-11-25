using CandidateSearchSystem.Data.Constants;
using CandidateSearchSystem.Data.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CandidateSearchSystem.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
    {
        public DbSet<CandidateProfile> CandidateProfiles { get; set; } = null!;
        public DbSet<RecruiterProfile> RecruiterProfiles { get; set; } = null!;
        public DbSet<CandidateExperience> CandidateExperiences { get; set; } = null!;
        public DbSet<CandidateEducation> CandidateEducations { get; set; } = null!;
        public DbSet<CandidateSkill> CandidateSkills { get; set; } = null!;
        public DbSet<CandidateLanguage> CandidateLanguages { get; set; } = null!;
        public DbSet<TestResult> TestResults { get; set; } = null!;
        public DbSet<PsychometricResult> PsychometricResults { get; set; } = null!;
        public DbSet<SkillValidation> SkillValidations { get; set; } = null!;
        public DbSet<RecruiterInteraction> RecruiterInteractions { get; set; } = null!;
        public DbSet<RecruiterCandidateFavorite> RecruiterCandidateFavorites { get; set; } = null!;
        public DbSet<Contact> Contacts { get; set; } = null!;
        public DbSet<Files> Files { get; set; } = null!;
        public DbSet<NewsPost> NewsPosts { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // --------------------------------------------------------
            // 1. КОНВЕРТАЦИЯ ENUM -> STRING
            // --------------------------------------------------------

            // Common
            builder.Entity<Contact>().Property(c => c.Type).HasConversion<string>();
            builder.Entity<Files>().Property(f => f.Type).HasConversion<string>();
            builder.Entity<NewsPost>().Property(n => n.Level).HasConversion<string>();

            // Candidate Profile
            builder.Entity<CandidateProfile>().Property(p => p.SalaryCurrency).HasConversion<string>();
            builder.Entity<CandidateProfile>().Property(p => p.EmploymentType).HasConversion<string>();
            builder.Entity<CandidateProfile>().Property(p => p.WorkModel).HasConversion<string>();
            builder.Entity<CandidateProfile>().Property(p => p.WorkSchedule).HasConversion<string>();

            // Candidate Related
            builder.Entity<CandidateLanguage>().Property(l => l.ProficiencyLevel).HasConversion<string>();

            // Tests & Psychometrics
            builder.Entity<TestResult>().Property(t => t.Category).HasConversion<string>();
            builder.Entity<TestResult>().Property(t => t.ScoreUnit).HasConversion<string>();
            builder.Entity<PsychometricResult>().Property(p => p.AssessmentType).HasConversion<string>();

            // Interactions
            builder.Entity<RecruiterInteraction>().Property(i => i.Status).HasConversion<string>();
            builder.Entity<RecruiterInteraction>().Property(i => i.CandidateAction).HasConversion<string>();


            // --------------------------------------------------------
            // 2. ИНДЕКСЫ
            // --------------------------------------------------------
            builder.Entity<NewsPost>()
                .HasIndex(np => np.CreatedAt)
                .IsUnique(false)
                .IsDescending(true);


            // --------------------------------------------------------
            // 3. СВЯЗИ (RELATIONSHIPS)
            // --------------------------------------------------------

            // 3.1 One-to-One: User -> Profile
            builder.Entity<CandidateProfile>()
                .HasOne(cp => cp.User)
                .WithOne(u => u.CandidateProfile)
                .HasForeignKey<CandidateProfile>(cp => cp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RecruiterProfile>()
                .HasOne(rp => rp.User)
                .WithOne(u => u.RecruiterProfile)
                .HasForeignKey<RecruiterProfile>(rp => rp.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3.2 One-to-Many: User -> Aux Data
            builder.Entity<Contact>()
                .HasOne(c => c.User)
                .WithMany(u => u.Contacts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Files>()
                .HasOne(f => f.User)
                .WithMany(u => u.Files)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3.3 Many-to-Many: Favorites
            builder.Entity<RecruiterCandidateFavorite>()
                .HasKey(rcf => new { rcf.RecruiterProfileId, rcf.CandidateProfileId });

            builder.Entity<RecruiterCandidateFavorite>()
                .HasOne(rcf => rcf.RecruiterProfile)
                .WithMany(rp => rp.FavoriteCandidates)
                .HasForeignKey(rcf => rcf.RecruiterProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RecruiterCandidateFavorite>()
                .HasOne(rcf => rcf.CandidateProfile)
                .WithMany(cp => cp.FavoritedByRecruiters)
                .HasForeignKey(rcf => rcf.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // 3.4 Many-to-Many via Entity: Skill Validation
            builder.Entity<SkillValidation>()
                .HasKey(sv => new { sv.CandidateSkillId, sv.TestResultId });

            builder.Entity<SkillValidation>()
                .HasOne(sv => sv.CandidateSkill)
                .WithMany(cs => cs.Validations)
                .HasForeignKey(sv => sv.CandidateSkillId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SkillValidation>()
                .HasOne(sv => sv.TestResult)
                .WithMany()
                .HasForeignKey(sv => sv.TestResultId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3.5 Interactions
            builder.Entity<RecruiterInteraction>()
                .HasOne(ri => ri.Recruiter)
                .WithMany()
                .HasForeignKey(ri => ri.RecruiterId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RecruiterInteraction>()
                .HasOne(ri => ri.Candidate)
                .WithMany()
                .HasForeignKey(ri => ri.CandidateId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3.6 Candidate Cascades
            ConfigureCandidateCascades(builder);
        }

        private void ConfigureCandidateCascades(ModelBuilder builder)
        {
            builder.Entity<CandidateExperience>()
                .HasOne(ce => ce.Profile)
                .WithMany(cp => cp.Experiences)
                .HasForeignKey(ce => ce.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CandidateEducation>()
                .HasOne(ce => ce.Profile)
                .WithMany(cp => cp.Education)
                .HasForeignKey(ce => ce.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CandidateSkill>()
                .HasOne(cs => cs.Profile)
                .WithMany(cp => cp.Skills)
                .HasForeignKey(cs => cs.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CandidateLanguage>()
                .HasOne(cl => cl.Profile)
                .WithMany(cp => cp.Languages)
                .HasForeignKey(cl => cl.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TestResult>()
                .HasOne(tr => tr.CandidateProfile)
                .WithMany(cp => cp.TestResults)
                .HasForeignKey(tr => tr.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<PsychometricResult>()
                .HasOne(pr => pr.CandidateProfile)
                .WithMany(cp => cp.Psychometrics)
                .HasForeignKey(pr => pr.CandidateProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}