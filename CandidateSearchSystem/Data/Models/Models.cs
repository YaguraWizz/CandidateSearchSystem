using CandidateSearchSystem.Data.Constants;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CandidateSearchSystem.Data.Models
{
    #region Identity & Base Users

    // Роли системы
    public class ApplicationRole : IdentityRole<Guid>
    {
        public ApplicationRole() : base() { }
        public ApplicationRole(string roleName) : base(roleName) { }
        [NotMapped] public static readonly string Admin = "Admin";
        [NotMapped] public static readonly string Recruiter = "Recruiter";
        [NotMapped] public static readonly string Candidate = "Candidate";
        [NotMapped] public static readonly string[] All = [Admin, Recruiter, Candidate];
    }

    // Основной пользователь системы
    public class ApplicationUser : IdentityUser<Guid>
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? LastName { get; set; }

        [StringLength(50)]
        public string? Patronymic { get; set; }

        public DateTimeOffset DateOfBirth { get; set; }

        [StringLength(10)]
        public string? PreferredLanguage { get; set; } = "ru";

        [StringLength(500)]
        public string? Description { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        // Навигационные свойства
        public ICollection<Contact> Contacts { get; set; } = [];
        public ICollection<Files> Files { get; set; } = [];
        public CandidateProfile? CandidateProfile { get; set; }
        public RecruiterProfile? RecruiterProfile { get; set; }
    }

    // Контактная информация пользователя
    public class Contact
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public ContactType Type { get; set; } = ContactType.None;

        [Required, StringLength(200)]
        public string Value { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsPrimary { get; set; }
    }

    // Файлы пользователя
    public class Files
    {
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        [Required, StringLength(255)]
        public string Name { get; set; } = string.Empty;

        public FileType Type { get; set; } = FileType.None;

        [Required]
        public string StoragePath { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    // Новости системы
    public class NewsPost
    {
        [Key]
        public Guid Id { get; set; }
        public string Author { get; set; } = "Admin";
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public NewsLevel Level { get; set; } = NewsLevel.Update;
    }

    #endregion

    #region Candidate Domain

    // Профиль кандидата
    public class CandidateProfile
    {
        [Key, ForeignKey("ApplicationUser")]
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        // Основная информация о поиске
        [Required, StringLength(100)]
        public string CurrentJobTitle { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string DesiredJobTitle { get; set; } = string.Empty;

        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [StringLength(50)]
        public string Country { get; set; } = string.Empty;

        public decimal DesiredSalary { get; set; }
        public Currency SalaryCurrency { get; set; } = Currency.USD;

        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
        public WorkModel WorkModel { get; set; } = WorkModel.Office;
        public WorkSchedule WorkSchedule { get; set; } = WorkSchedule.Standard;

        [StringLength(2000)]
        public string? Summary { get; set; }

        public bool IsActivelyLooking { get; set; }
        public int TotalYearsOfExperience { get; set; }
        public DateTimeOffset LastActivity { get; set; } = DateTimeOffset.UtcNow;

        // Фильтры и предпочтения
        public bool IsReadyToRelocate { get; set; }
        public bool IsReadyForBusinessTrips { get; set; }

        [StringLength(50)]
        public string? Citizenship { get; set; }

        [StringLength(100)]
        public string? WorkAuthorization { get; set; }

        public string? Keywords { get; set; }

        // Связанные сущности
        public ICollection<CandidateExperience> Experiences { get; set; } = [];
        public ICollection<CandidateEducation> Education { get; set; } = [];
        public ICollection<CandidateSkill> Skills { set; get; } = [];
        public ICollection<CandidateLanguage> Languages { get; set; } = [];
        public ICollection<TestResult> TestResults { get; set; } = [];
        public ICollection<PsychometricResult> Psychometrics { get; set; } = [];
        public ICollection<RecruiterCandidateFavorite> FavoritedByRecruiters { get; set; } = [];
        public ICollection<RecruiterInteraction> Interactions { get; set; } = [];
    }

    // Опыт работы
    public class CandidateExperience
    {
        public Guid Id { get; set; }

        [Required]
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile Profile { get; set; } = null!;

        [Required, StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? City { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }

        public string? Description { get; set; }
    }

    // Образование
    public class CandidateEducation
    {
        public Guid Id { get; set; }

        [Required]
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile Profile { get; set; } = null!;

        [Required, StringLength(100)]
        public string InstitutionName { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Degree { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string FieldOfStudy { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        [StringLength(50)]
        public string? Grade { get; set; }
    }

    // Навыки
    public class CandidateSkill
    {
        public Guid Id { get; set; }

        [Required]
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile Profile { get; set; } = null!;

        [Required, StringLength(100)]
        public string SkillName { get; set; } = string.Empty;

        [Range(1, 10)]
        public int Level { get; set; } = 5;

        public int? YearsOfExperience { get; set; }

        public ICollection<SkillValidation> Validations { get; set; } = [];
    }

    // Языки
    public class CandidateLanguage
    {
        public Guid Id { get; set; }

        [Required]
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile Profile { get; set; } = null!;

        [Required, StringLength(50)]
        public string LanguageName { get; set; } = string.Empty;

        public LanguageLevel ProficiencyLevel { get; set; } = LanguageLevel.A1_Beginner;
    }

    #endregion

    #region Validation & Assessment Domain

    // Результат тестирования
    public class TestResult
    {
        public Guid Id { get; set; }

        [Required]
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        [Required, StringLength(100)]
        public string TestName { get; set; } = string.Empty;

        public TestCategory Category { get; set; } = TestCategory.General;

        public decimal Score { get; set; }
        public ScoreUnit ScoreUnit { get; set; } = ScoreUnit.Percent;

        public DateTimeOffset CompletionDate { get; set; } = DateTimeOffset.UtcNow;
    }

    // Валидация навыка через тест
    public class SkillValidation
    {
        public Guid Id { get; set; }

        [Required]
        public Guid CandidateSkillId { get; set; }
        public CandidateSkill CandidateSkill { get; set; } = null!;

        [Required]
        public Guid TestResultId { get; set; }
        public TestResult TestResult { get; set; } = null!;

        public bool IsSuccessful { get; set; }
        public DateTimeOffset ValidatedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    // Психометрические данные
    public class PsychometricResult
    {
        public Guid Id { get; set; }

        [Required]
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public AssessmentType AssessmentType { get; set; } = AssessmentType.MBTI;

        [Required, StringLength(50)]
        public string ResultCode { get; set; } = string.Empty;

        public string? Description { get; set; }
        public DateTimeOffset CompletionDate { get; set; } = DateTimeOffset.UtcNow;
    }

    #endregion

    #region Recruiter Domain

    // Профиль рекрутера
    public class RecruiterProfile
    {
        [Key, ForeignKey("ApplicationUser")]
        public Guid UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        [Required, StringLength(100)]
        public string CompanyName { get; set; } = string.Empty;

        [StringLength(100)]
        public string JobTitle { get; set; } = string.Empty;

        public string? Bio { get; set; }

        [StringLength(255)]
        public string? CompanyWebsite { get; set; }

        [StringLength(100)]
        public string? Industry { get; set; }

        public ICollection<RecruiterCandidateFavorite> FavoriteCandidates { get; set; } = [];
        public ICollection<RecruiterInteraction> InitiatedInteractions { get; set; } = [];
    }

    // Избранные кандидаты
    public class RecruiterCandidateFavorite
    {
        [Required]
        public Guid RecruiterProfileId { get; set; }
        public RecruiterProfile RecruiterProfile { get; set; } = null!;

        [Required]
        public Guid CandidateProfileId { get; set; }
        public CandidateProfile CandidateProfile { get; set; } = null!;

        public DateTimeOffset AddedAt { get; set; } = DateTimeOffset.UtcNow;
        public string? RecruiterNotes { get; set; }
    }

    // Взаимодействие (Инвайты, офферы)
    public class RecruiterInteraction
    {
        public Guid Id { get; set; }

        [Required]
        public Guid RecruiterId { get; set; }
        [ForeignKey("RecruiterId")]
        public ApplicationUser Recruiter { get; set; } = null!;

        [Required]
        public Guid CandidateId { get; set; }
        [ForeignKey("CandidateId")]
        public ApplicationUser Candidate { get; set; } = null!;

        public InteractionStatus Status { get; set; } = InteractionStatus.Draft;

        public DateTimeOffset SentDate { get; set; } = DateTimeOffset.UtcNow;

        public string? RecruiterInvitationBody { get; set; }

        public CandidateAction CandidateAction { get; set; } = CandidateAction.None;
        public DateTimeOffset? CandidateResponseDate { get; set; }

        [StringLength(500)]
        public string? CandidateActionReason { get; set; }

        public string? ExternalReference { get; set; }
    }

    #endregion
}