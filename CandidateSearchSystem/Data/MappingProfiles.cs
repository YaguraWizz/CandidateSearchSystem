using AutoMapper;
using CandidateSearchSystem.Data.DTOs;
using CandidateSearchSystem.Data.Models;

namespace CandidateSearchSystem.Data
{
    public class AppMappingProfile : Profile
    {
        public AppMappingProfile()
        {
            ConfigureUserMapping();
            ConfigureContactMapping();
            ConfigureFileMapping();
            ConfigureNewsMapping();

            // Новые группы маппинга
            ConfigureCandidateMapping();
            ConfigureRecruiterMapping();
            ConfigureInteractionMapping();
        }

        private void ConfigureUserMapping()
        {
            // ApplicationUser <-> ApplicationUserDto
            CreateMap<ApplicationUser, ApplicationUserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Patronymic, opt => opt.MapFrom(src => src.Patronymic))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
                .ForMember(dest => dest.PreferredLanguage, opt => opt.MapFrom(src => src.PreferredLanguage))
                .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => src.UpdatedAt))
                .ForMember(dest => dest.DeletedAt, opt => opt.MapFrom(src => src.DeletedAt))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Role, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id != Guid.Empty))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore())
                .ForMember(dest => dest.Files, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore())
                .ForMember(dest => dest.RecruiterProfile, opt => opt.Ignore());

            // ApplicationUserEditDto -> ApplicationUser
            CreateMap<ApplicationUserEditDto, ApplicationUser>()
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTimeOffset.UtcNow))
                .ForMember(dest => dest.FirstName, opt => opt.Condition(src => src.FirstName != null))
                .ForMember(dest => dest.LastName, opt => opt.Condition(src => src.LastName != null))
                .ForMember(dest => dest.Patronymic, opt => opt.Condition(src => src.Patronymic != null))
                .ForMember(dest => dest.DateOfBirth, opt =>
                {
                    // 1. Условие: Маппим только если в DTO есть значение
                    opt.Condition(src => src.DateOfBirth.HasValue);

                    // 2. Логика маппинга:
                    // Берем чистую дату (Date) из DateTime?
                    // Создаем DateTimeOffset, устанавливая время в полночь (00:00:00) 
                    // и смещение (offset) в UTC (TimeSpan.Zero).
                    opt.MapFrom(src => src.DateOfBirth.HasValue
                        ? new DateTimeOffset(src.DateOfBirth.Value.Date, TimeSpan.Zero)
                        : default);
                })
                .ForMember(dest => dest.Description, opt => opt.Condition(src => src.Description != null))
                .ForMember(dest => dest.PreferredLanguage, opt => opt.Condition(src => src.PreferredLanguage != null))
                // Ignore fields that should not be updated
                .ForMember(dest => dest.UserName, opt => opt.Ignore())
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore())
                .ForMember(dest => dest.PhoneNumber, opt => opt.Ignore())
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Contacts, opt => opt.Ignore())
                .ForMember(dest => dest.Files, opt => opt.Ignore())
                .ForMember(dest => dest.CandidateProfile, opt => opt.Ignore())
                .ForMember(dest => dest.RecruiterProfile, opt => opt.Ignore());

            // UserSummaryDto (для краткого отображения в списках)
            CreateMap<ApplicationUser, UserSummaryDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));
        }

        private void ConfigureContactMapping()
        {
            CreateMap<Contact, ContactDto>()
                .ReverseMap()
                .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id != Guid.Empty))
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.User, opt => opt.Ignore());
        }

        private void ConfigureFileMapping()
        {
            // Entity -> DTO
            CreateMap<Files, FileDto>();

            // DTO -> Entity
            CreateMap<FileDto, Files>()
                .ForMember(dest => dest.Id, opt => opt.Condition(src => src.Id != Guid.Empty))
                .ForMember(dest => dest.UserId, opt => opt.Ignore())        // Не меняем UserId
                .ForMember(dest => dest.User, opt => opt.Ignore())          // Навигационное свойство
                .ForMember(dest => dest.StoragePath, opt => opt.Ignore())   // Пусть хранится старый путь
                .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())     // Логическое удаление не трогаем
                .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())     // Дата удаления не трогаем
                .ForMember(dest => dest.UploadedAt, opt => opt.Ignore());   // Дата загрузки не трогаем
        }

        private void ConfigureNewsMapping()
        {
            CreateMap<NewsPost, NewsPostDto>().ReverseMap();
        }

        private void ConfigureCandidateMapping()
        {
            // Candidate Profile (Main)
            CreateMap<CandidateProfile, CandidateProfileDto>()
                .ForMember(dest => dest.Experience, opt => opt.MapFrom(src => src.Experiences))
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore()); // UserId обычно берется из контекста или URL

            // Candidate Summary (для списков и поиска)
            CreateMap<CandidateProfile, CandidateProfileSummaryDto>()
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.User.LastName));

            // Nested Collections
            CreateMap<CandidateExperience, CandidateExperienceDto>().ReverseMap();
            CreateMap<CandidateEducation, CandidateEducationDto>().ReverseMap();
            CreateMap<CandidateSkill, CandidateSkillDto>().ReverseMap();
            CreateMap<CandidateLanguage, CandidateLanguageDto>().ReverseMap();

            // Validation & Tests
            CreateMap<TestResult, TestResultDto>().ReverseMap();
            CreateMap<PsychometricResult, PsychometricResultDto>().ReverseMap();
            CreateMap<SkillValidation, SkillValidationDto>().ReverseMap();
        }

        private void ConfigureRecruiterMapping()
        {
            // Recruiter Profile
            CreateMap<RecruiterProfile, RecruiterProfileDto>()
                .ForMember(dest => dest.RecruiterInteractions, opt => opt.MapFrom(src => src.InitiatedInteractions))
                .ReverseMap()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.FavoriteCandidates, opt => opt.Ignore());

            // Favorites
            CreateMap<RecruiterCandidateFavorite, RecruiterCandidateFavoriteDto>()
                .ForMember(dest => dest.Candidate, opt => opt.MapFrom(src => src.CandidateProfile));
        }

        private void ConfigureInteractionMapping()
        {
            // Recruiter Interaction
            // AutoMapper автоматически использует маппинг ApplicationUser -> UserSummaryDto для полей Recruiter и Candidate
            CreateMap<RecruiterInteraction, RecruiterInteractionDto>()
                .ReverseMap()
                .ForMember(dest => dest.Recruiter, opt => opt.Ignore())
                .ForMember(dest => dest.Candidate, opt => opt.Ignore());
        }
    }
}