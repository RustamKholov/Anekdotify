using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.Common
{
    public class OperationResult
    {
        public bool IsSuccess { get; protected set; }
        public string? ErrorMessage { get; protected set; }
        public bool IsAlreadyExists { get; protected set; }
        public bool IsNotFound { get; protected set; }

        protected OperationResult() { }

        public static OperationResult Success() => new OperationResult { IsSuccess = true };
        public static OperationResult Fail(string errorMessage) => new OperationResult { IsSuccess = false, ErrorMessage = errorMessage };
        public static OperationResult AlreadyExists(string errorMessage) => new OperationResult { IsSuccess = false, IsAlreadyExists = true, ErrorMessage = errorMessage };
        public static OperationResult NotFound(string errorMessage) => new OperationResult { IsSuccess = false, IsNotFound = true, ErrorMessage = errorMessage };

    }

    public class OperationResult<T> : OperationResult
    {
        public T? Value { get; private set; }

        private OperationResult() { }

        public static OperationResult<T> Success(T value) => new OperationResult<T> { IsSuccess = true, Value = value };
        public static OperationResult<T> Fail(T value, string errorMessage) => new OperationResult<T> { IsSuccess = false, ErrorMessage = errorMessage, Value = value };
        public static OperationResult<T> AlreadyExists(T value, string errorMessage) => new OperationResult<T> { IsSuccess = false, IsAlreadyExists = true, ErrorMessage = errorMessage, Value = value };
        public static OperationResult<T> NotFound(T value, string errorMessage) => new OperationResult<T> { IsSuccess = false, IsNotFound = true, ErrorMessage = errorMessage, Value = value };
    }
    
}