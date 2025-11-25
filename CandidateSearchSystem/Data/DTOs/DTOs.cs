using CandidateSearchSystem.Data.Constants;
using CandidateSearchSystem.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CandidateSearchSystem.Data.DTOs
{
    // ============================================================
    // AUTH & USERS
    // ============================================================
    
    // DTO для Общей Информации
    public class ApplicationUserEditDto
    {
        [Required(ErrorMessage = "Имя обязательно.")]
        [StringLength(50, ErrorMessage = "Имя не может превышать 50 символов.")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Фамилия обязательна.")]
        [StringLength(50, ErrorMessage = "Фамилия не может превышать 50 символов.")]
        public string? LastName { get; set; }

        [StringLength(50, ErrorMessage = "Отчество не может превышать 50 символов.")]
        public string? Patronymic { get; set; }

        [StringLength(500, ErrorMessage = "Описание не может превышать 500 символов.")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [Required(ErrorMessage = "Дата рождения обязательна.")]
        public DateTime? DateOfBirth { get; set; }

        [StringLength(10)]
        [RegularExpression("^(en|ru|es|fr|de|zh|ja|it|pt|ar|hi)?$", ErrorMessage = "Недопустимый язык.")]
        public string? PreferredLanguage { get; set; } = "ru";
    }

    // DTO для Смены Пароля
    public class ChangePasswordDto
    {
        [Required]
        [DataType(DataType.Password)]
        public string? CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть не менее 6 символов.")]
        [DataType(DataType.Password)]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword), ErrorMessage = "Новый пароль и его подтверждение не совпадают.")]
        public string? ConfirmPassword { get; set; }
    }


    public class LoginFormDTO
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }

    public class RegisterFormDTO
    {
        public string FirstName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ApplicationUserDto
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? Patronymic { get; set; }
        public DateTimeOffset DateOfBirth { get; set; }
        public string PreferredLanguage { get; set; } = "ru";
        public string? Description { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }
        public DateTimeOffset DeletedAt { get; set; }

        [NotMapped] public ApplicationRole Role { get; set; } = new ApplicationRole(ApplicationRole.Candidate);
        public CandidateProfileDto? CandidateProfile { get; set; }
        public RecruiterProfileDto? RecruiterProfile { get; set; }
    }

    // ============================================================
    // CANDIDATE PROFILE
    // ============================================================

    public class CandidateProfileDto
    {
        public Guid UserId { get; set; }
        public string DesiredJobTitle { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        // Используем Enum
        public decimal DesiredSalary { get; set; } = 0;
        public Currency SalaryCurrency { get; set; } = Currency.USD;
        public EmploymentType EmploymentType { get; set; } = EmploymentType.FullTime;
        public WorkModel WorkModel { get; set; } = WorkModel.Office;

        public bool IsActivelyLooking { get; set; } = false;
        public int TotalYearsOfExperience { get; set; } = 0;
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        public List<CandidateExperienceDto> Experience { get; set; } = [];
        public List<CandidateEducationDto> Education { get; set; } = [];
        public List<CandidateSkillDto> Skills { get; set; } = [];
        public List<CandidateLanguageDto> Languages { get; set; } = [];
        public List<TestResultDto> TestResults { get; set; } = [];
        public List<PsychometricResultDto> Psychometrics { get; set; } = [];
    }

    public class CandidateExperienceDto
    {
        public Guid Id { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set; }
        public bool IsCurrent { get; set; } = false;
        public string? Description { get; set; }
    }

    public class CandidateEducationDto
    {
        public Guid Id { get; set; }
        public string InstitutionName { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public string? Specialization { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsFinished { get; set; } = false;
    }

    public class CandidateSkillDto
    {
        public Guid Id { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public int ProficiencyLevel { get; set; } = 1;
        public List<SkillValidationDto> Validations { get; set; } = [];
    }

    public class CandidateLanguageDto
    {
        public Guid Id { get; set; }
        public string LanguageName { get; set; } = string.Empty;
        public LanguageLevel ProficiencyLevel { get; set; } = LanguageLevel.A1_Beginner;
    }

    public class TestResultDto
    {
        public Guid Id { get; set; }
        public string TestName { get; set; } = string.Empty;
        public DateTimeOffset CompletionDate { get; set; }
        public decimal Score { get; set; }
        public ScoreUnit ScoreUnit { get; set; }
        public string? ExternalLink { get; set; }
    }

    public class PsychometricResultDto
    {
        public Guid Id { get; set; }
        public AssessmentType AssessmentType { get; set; }
        public DateTimeOffset CompletionDate { get; set; }
        public string ResultCode { get; set; } = string.Empty;
        public string? Summary { get; set; }
    }

    public class SkillValidationDto
    {
        public Guid Id { get; set; }
        public Guid ValidatorUserId { get; set; }
        public DateTimeOffset ValidatedAt { get; set; }
        public int Score { get; set; }
    }

    // ============================================================
    // RECRUITER PROFILE & INTERACTIONS
    // ============================================================

    public class RecruiterProfileDto
    {
        public Guid UserId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? CompanyWebsite { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public List<RecruiterInteractionDto> RecruiterInteractions { get; set; } = [];
    }

    public class RecruiterCandidateFavoriteDto
    {
        public Guid RecruiterProfileId { get; set; }
        public CandidateProfileSummaryDto Candidate { get; set; } = null!;
        public DateTimeOffset AddedAt { get; set; }
        public string? RecruiterNotes { get; set; }
    }

    public class RecruiterInteractionDto
    {
        public Guid Id { get; set; }
        public UserSummaryDto Recruiter { get; set; } = null!;
        public UserSummaryDto Candidate { get; set; } = null!;
        public InteractionStatus Status { get; set; } = InteractionStatus.Sent;
        public DateTimeOffset SentDate { get; set; }
        public string? RecruiterInvitationBody { get; set; }
        public CandidateAction CandidateAction { get; set; }
        public DateTimeOffset? CandidateResponseDate { get; set; }
        public string? CandidateActionReason { get; set; }
    }

    // ============================================================
    // SEARCH & SUMMARIES
    // ============================================================

    public class CandidateProfileSummaryDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string DesiredJobTitle { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public decimal DesiredSalary { get; set; }
        public Currency SalaryCurrency { get; set; }
        public int TotalYearsOfExperience { get; set; }
        public bool IsActivelyLooking { get; set; } = false;
        public WorkModel WorkModel { get; set; }
        public DateTimeOffset LastActivity { get; set; }
    }

    public class UserSummaryDto
    {
        public Guid UserId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string? LastName { get; set; }
        public string? UserName { get; set; }
    }

    public class SearchCandidateDto
    {
        public string? DesiredJobTitle { get; set; }
        public string? City { get; set; }
        public int MinExperienceYears { get; set; } = 0;
        public decimal? MinDesiredSalary { get; set; }
        public Currency? SalaryCurrency { get; set; }
        public bool IsActivelyLooking { get; set; } = false;
        public WorkModel? WorkModel { get; set; }
    }

    // ============================================================
    // SHARED
    // ============================================================

    public class ContactDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public ContactType Type { get; set; } = ContactType.None;
        public string Value { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
        public bool IsPrimary { get; set; } = false;
    }

    public class FileDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public FileType Type { get; set; } = FileType.None;
        public string StoragePath { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
    }

    public class FileCreationDto
    {
        public FileType Type { get; set; } = FileType.None;
        public string? Description { get; set; }
    }

    public class NewsPostDto
    {
        [Key]
        public Guid Id { get; set; }
        public string Author { get; set; } = "Admin";
        public string Text { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public NewsLevel Level { get; set; } = NewsLevel.Update;
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    }
}