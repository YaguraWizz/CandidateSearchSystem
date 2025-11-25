namespace CandidateSearchSystem.Data.Constants
{
    // ============================================================
    // ОБЩИЕ ПЕРЕЧИСЛЕНИЯ
    // ============================================================

    // Типы контактов пользователя
    public enum ContactType
    {
        None,
        Phone,
        Email,
        Telegram,
        WhatsApp,
        LinkedIn,
        Github,
        Portfolio,
        Other
    }

    // Типы загружаемых файлов
    public enum FileType
    {
        None,
        ProfileAvatar,
        Resume,
        CoverLetter,
        Certificate,
        PortfolioWork,
        Other
    }

    // Валюты зарплатных ожиданий
    public enum Currency
    {
        USD,
        EUR,
        RUB,
        KZT,
        UZS,
        GEL
    }

    // ============================================================
    // КАНДИДАТ И РАБОТА
    // ============================================================

    // Модель работы (где работает сотрудник)
    public enum WorkModel
    {
        Office,
        Remote,
        Hybrid
    }

    // Тип занятости (нагрузка)
    public enum EmploymentType
    {
        FullTime,
        PartTime,
        Contract,
        Freelance,
        Internship
    }

    // График работы
    public enum WorkSchedule
    {
        Standard,
        Flexible,
        ShiftWork,
        ProjectBased
    }

    // Уровень владения языком (CEFR + описательные)
    public enum LanguageLevel
    {
        A1_Beginner,
        A2_Elementary,
        B1_Intermediate,
        B2_UpperIntermediate,
        C1_Advanced,
        C2_Proficiency,
        Native
    }

    // ============================================================
    // ТЕСТИРОВАНИЕ И ОЦЕНКА
    // ============================================================

    // Категория теста
    public enum TestCategory
    {
        HardSkill,
        SoftSkill,
        Language,
        Logic,
        General
    }

    // Единица измерения результата теста
    public enum ScoreUnit
    {
        Percent,
        Points,
        Grade
    }

    // Тип психометрической оценки
    public enum AssessmentType
    {
        MBTI,
        DISC,
        BigFive,
        Adizes
    }

    // ============================================================
    // ВЗАИМОДЕЙСТВИЕ (HR <-> КАНДИДАТ)
    // ============================================================

    // Статус взаимодействия (процесс найма)
    public enum InteractionStatus
    {
        Draft,
        Sent,
        Viewed,
        Accepted,
        Declined,
        InterviewScheduled,
        OfferSent,
        Hired,
        Rejected
    }

    // Действие кандидата в ответ на приглашение
    public enum CandidateAction
    {
        None,
        Accept,
        Decline,
        AskDetails
    }

    // ============================================================
    // СИСТЕМНЫЕ
    // ============================================================

    // Уровень важности новости
    public enum NewsLevel
    {
        HotFix,
        Release,
        Update,
        Announcement
    }
}