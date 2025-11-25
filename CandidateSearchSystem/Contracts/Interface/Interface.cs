using CandidateSearchSystem.Contracts.Utils;
using CandidateSearchSystem.Data.Constants;
using CandidateSearchSystem.Data.DTOs;
using Microsoft.AspNetCore.Components.Forms;

namespace CandidateSearchSystem.Contracts.Interface
{
   
    public interface IAccountService
    {
        #region [I. Аутентификация и Регистрация]

        /// <summary>
        /// Регистрирует нового пользователя на основе предоставленных данных.
        /// </summary>
        /// <param name="dto">DTO с данными для регистрации (Email, Password).</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, содержащий DTO созданного пользователя (ApplicationUserDto) или строку с ошибкой (например, "Email занят").</returns>
        Task<Result<ApplicationUserDto, string>> RegisterAsync(RegisterFormDTO dto, CancellationToken token = default);

        /// <summary>
        /// Выполняет аутентификацию пользователя. 
        /// Если аутентификация успешна, устанавливает необходимые куки/токены.
        /// </summary>
        /// <param name="dto">DTO с данными для входа (UserNameOrEmail, Password, RememberMe).</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>EmptyResult, указывающий на успех (Unit) или строку с ошибкой (например, "Неверный пароль/логин").</returns>
        Task<EmptyResult> LoginAsync(LoginFormDTO dto, CancellationToken token = default);

        /// <summary>
        /// Выполняет выход текущего пользователя из системы.
        /// </summary>
        /// <returns>EmptyResult, указывающий на успех (Unit) или строку с ошибкой.</returns>
        Task<EmptyResult> LogoutAsync();

        #endregion

        #region [II. Управление Профилем (CRUD)]

        /// <summary>
        /// Возвращает детали пользователя по его уникальному Id.
        /// </summary>
        /// <param name="userId">Уникальный Id пользователя.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, содержащий DTO с полной информацией о пользователе (ApplicationUserDto) или строку с ошибкой (например, "Пользователь не найден").</returns>
        Task<Result<ApplicationUserDto, string>> GetByIdAsync(Guid Id, CancellationToken token = default);

        /// <summary>
        /// Обновляет метаданные профиля пользователя (Имя, Фамилия, Никнейм, Дата рождения).
        /// </summary>
        /// <param name="userId">Id пользователя, чей профиль обновляется.</param>
        /// <param name="dto">DTO с обновленными данными.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, содержащий DTO обновленного пользователя (ApplicationUserEditDto) или строку с ошибкой.</returns>
        Task<Result<ApplicationUserDto, string>> UpdateAsync(Guid Id, ApplicationUserEditDto dto, CancellationToken token = default);

        /// <summary>
        /// Удаляет пользователя и все связанные с ним данные (включая файлы).
        /// </summary>
        /// <param name="userId">Id пользователя для удаления.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>EmptyResult, указывающий на успех (Unit) или строку с ошибкой (например, "Пользователь не найден").</returns>
        Task<EmptyResult> DeleteAsync(Guid Id, CancellationToken token = default);

        /// <summary>
        /// Изменяет пароль пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя, чей пароль меняется.</param>
        /// <param name="dto">DTO с текущим и новым паролями.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>EmptyResult, указывающий на успех (Unit) или строку с ошибкой.</returns>
        Task<EmptyResult> ChangePasswordAsync(Guid Id, ChangePasswordDto dto, CancellationToken token = default);
        #endregion

    }

    public interface IContactService
    {
        #region [I. Управление Контактами (CRUD)]

        /// <summary>
        /// Добавляет новый контакт для указанного пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя, к которому добавляется контакт.</param>
        /// <param name="dto">DTO с данными нового контакта.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, указывающий на успех (Unit) или строку с ошибкой.</returns>
        Task<Result<ContactDto, string>> AddAsync(Guid userId, ContactDto dto, CancellationToken token = default);

        /// <summary>
        /// Обновляет существующий контакт пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя, чей контакт обновляется.</param>
        /// <param name="dto">DTO с обновленными данными контакта (должен содержать Id контакта).</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, указывающий на успех (Unit) или строку с ошибкой.</returns>
        Task<Result<ContactDto, string>> UpdateAsync(Guid userId, ContactDto dto, CancellationToken token = default);

        /// <summary>
        /// Удаляет указанный контакт пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя, чей контакт удаляется.</param>
        /// <param name="contactId">Id контакта для удаления.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>EmptyResult, указывающий на успех (Unit) или строку с ошибкой.</returns>
        Task<EmptyResult> DeleteAsync(Guid userId, Guid contactId, CancellationToken token = default);

        /// <summary>
        /// Возвращает список всех контактов для указанного пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя, чьи контакты нужно получить.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, содержащий коллекцию ContactDto или строку с ошибкой (например, "Пользователь не найден").</returns>
        Task<Result<IEnumerable<ContactDto>, string>> GetByUserIdAsync(Guid userId, CancellationToken token = default);

        #endregion
    }

    public interface IFileService
    {
        #region [I. Управление Файлами (CRUD)]

        /// <summary>
        /// Добавляет новый файл для указанного пользователя. 
        /// Осуществляет загрузку файла и сохранение метаданных.
        /// </summary>
        /// <param name="userId">Id пользователя, которому принадлежит файл.</param>
        /// <param name="file">Объект файла из браузера (IBrowserFile).</param>
        /// <param name="FileCreationDto">Объект методанных файла.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, содержащий DTO созданного файла (FileDto).</returns>
        Task<Result<FileDto, string>> AddAsync(Guid userId, IBrowserFile file, FileCreationDto metadata, CancellationToken token = default);

        /// <summary>
        /// Обновляет метаданные существующего файла (например, имя, описание). 
        /// Не предназначен для замены самого файла.
        /// </summary>
        /// <param name="userId">Id пользователя, которому принадлежит файл.</param>
        /// <param name="dto">DTO с обновленными метаданными файла.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, содержащий DTO обновленного файла (FileDto).</returns>
        Task<Result<FileDto, string>> UpdateAsync(Guid userId, FileDto dto, CancellationToken token = default);

        /// <summary>
        /// Удаляет файл и соответствующие метаданные.
        /// </summary>
        /// <param name="userId">Id пользователя, которому принадлежит файл.</param>
        /// <param name="dto">DTO, содержащий Id файла для удаления.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>EmptyResult, указывающий на успех (Unit) или строку с ошибкой.</returns>
        Task<EmptyResult> DeleteAsync(Guid userId, FileDto dto, CancellationToken token = default);

        /// <summary>
        /// Возвращает список всех файлов, связанных с указанным пользователем.
        /// </summary>
        /// <param name="userId">Id пользователя, чьи файлы нужно получить.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, содержащий коллекцию FileDto или строку с ошибкой.</returns>
        Task<Result<IEnumerable<FileDto>, string>> GetByUserIdAsync(Guid userId, CancellationToken token = default);

        /// <summary>
        /// Возвращает список всех файлов указанного типа для конкретного пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя, чьи файлы нужно получить.</param>
        /// <param name="fileType">Тип файла (например, ProfileAvatar, Resume, Document).</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, содержащий коллекцию FileDto или строку с ошибкой.</returns>
        Task<Result<IEnumerable<FileDto>, string>> GetFilesByUserIdAndTypeAsync(Guid userId, FileType fileType, CancellationToken token = default);

        #endregion


        /// <summary>
        /// Возвращает имя директории для загрузки файлов.
        /// </summary>
        /// <returns></returns>
        string GetUploadDirectoryName();
    }

    public interface INewsService
    {
        #region [I. Управление Новостями (CRUD)]

        /// <summary>
        /// Добавляет новую новость. 
        /// Осуществляет создание и сохранение поста новости в хранилище.
        /// </summary>
        /// <param name="userId">Id пользователя, создающего новость.</param>
        /// <param name="dto">DTO с данными для создания новости (NewsPostDto).</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Task, содержащий EmptyResult (успех) или строку с ошибкой.</returns>
        Task<EmptyResult> AddAsync(Guid userId, NewsPostDto dto, CancellationToken token = default);

        /// <summary>
        /// Обновляет существующую новость.
        /// Обновляет поля новости, такие как заголовок или текст.
        /// </summary>
        /// <param name="userId">Id пользователя, которому принадлежит новость и который ее обновляет.</param>
        /// <param name="dto">DTO с обновленными данными новости (NewsPostDto).</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Task, содержащий EmptyResult (успех) или строку с ошибкой.</returns>
        Task<EmptyResult> UpdateAsync(Guid userId, NewsPostDto dto, CancellationToken token = default);

        /// <summary>
        /// Удаляет новость.
        /// Удаляет пост новости из хранилища по его идентификатору.
        /// </summary>
        /// <param name="userId">Id пользователя, который удаляет новость.</param>
        /// <param name="dto">DTO, содержащий Id новости для удаления.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Task, содержащий EmptyResult (успех) или строку с ошибкой.</returns>
        Task<EmptyResult> DeleteAsync(Guid userId, NewsPostDto dto, CancellationToken token = default);

        /// <summary>
        /// Получает новость по ее уникальному идентификатору.
        /// </summary>
        /// <param name="Id">Уникальный Id новости.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, содержащий DTO новости (NewsPostDto) или строку с ошибкой.</returns>
        Task<Result<NewsPostDto, string>> GetByIdAsync(Guid Id, CancellationToken token = default);

        /// <summary>
        /// Получает страницу (пагинацию) списка новостей.
        /// </summary>
        /// <param name="pageIndex">Индекс запрашиваемой страницы (начиная с 0).</param>
        /// <param name="pageSize">Количество элементов на странице.</param>
        /// <param name="descending">Направление сортировки (true - по убыванию, false - по возрастанию).</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, содержащий объект Paged<NewsPostDto> или строку с ошибкой.</returns>
        Task<Result<Paged<NewsPostDto>, string>> GetNewsPageAsync(int pageIndex, int pageSize, CancellationToken token = default);

        /// <summary>
        /// Определяет индекс страницы (pageIndex), на которой находится указанная новость.
        /// </summary>
        /// <param name="newsId">Id новости, индекс страницы которой нужно найти.</param>
        /// <param name="pageSize">Количество элементов на странице (должно совпадать с тем, что используется в GetNewsPageAsync).</param>
        /// <param name="descending">Направление сортировки, используемое для пагинации.</param>
        /// <param name="token">Токен отмены.</param>
        /// <returns>Result, содержащий индекс страницы (int) или строку с ошибкой.</returns>
        Task<Result<int, string>> GetNewsPageIndexAsync(Guid newsId, int pageSize, CancellationToken token = default);

        #endregion
    }


}
