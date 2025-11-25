namespace CandidateSearchSystem.Contracts.Utils
{
    public readonly struct Unit { public static Unit Value => default; }

    /// <summary>
    /// Представляет результат операции, который может содержать либо успешное значение, либо ошибку.
    /// </summary>
    /// <typeparam name="TValue">Тип успешного значения (например, string, int, List<T>).</typeparam>
    /// <typeparam name="TError">Тип ошибки (например, string, enum, класс ErrorDetails).</typeparam>
    public class Result<TValue, TError>
    {
        // Свойство для хранения успешного значения
        private readonly TValue _value;

        // Свойство для хранения информации об ошибке
        private readonly TError _error;

        // Свойство, указывающее, успешен ли результат
        public bool IsSuccess { get; }

        // Свойство, указывающее, является ли результат ошибкой
        public bool IsFailure => !IsSuccess;

        /// <summary>
        /// Возвращает успешное значение.
        /// Вызывает исключение, если результат является ошибкой.
        /// </summary>
        public TValue Value
        {
            get
            {
                if (IsFailure)
                {
                    throw new InvalidOperationException("Невозможно получить 'Value' для результата с ошибкой.");
                }
                return _value;
            }
        }

        /// <summary>
        /// Возвращает информацию об ошибке.
        /// Вызывает исключение, если результат успешен.
        /// </summary>
        public TError Error
        {
            get
            {
                if (IsSuccess)
                {
                    throw new InvalidOperationException("Невозможно получить 'Error' для успешного результата.");
                }
                return _error;
            }
        }

        // Приватный конструктор для успешного результата
        protected Result(TValue value)
        {
            IsSuccess = true;
            _value = value;
            _error = default!;
        }

        // Приватный конструктор для результата с ошибкой
        protected Result(TError error)
        {
            IsSuccess = false;
            _error = error;
            _value = default!;
        }

        /// <summary>
        /// Создает успешный результат.
        /// </summary>
        public static Result<TValue, TError> Success(TValue value) => new(value);

        /// <summary>
        /// Создает результат с ошибкой.
        /// </summary>
        public static Result<TValue, TError> Failure(TError error) => new(error);

        // Опционально: можно добавить метод для обработки обоих случаев
        public TOut Match<TOut>(Func<TValue, TOut> onSuccess, Func<TError, TOut> onFailure)
        {
            return IsSuccess ? onSuccess(_value) : onFailure(_error);
        }

        public void Match(Action<TValue> onSuccess, Action<TError> onFailure)
        {
            if (IsSuccess)
            {
                onSuccess(_value!);
            }
            else
            {
                onFailure(_error!);
            }
        }
    }

    public class EmptyResult : Result<Unit, string>
    {
        public EmptyResult() : base(Unit.Value) { }
        public EmptyResult(string error) : base(error) { }

        public static EmptyResult Success() => new();
        public static new EmptyResult Failure(string error) => new(error);
    }

}
