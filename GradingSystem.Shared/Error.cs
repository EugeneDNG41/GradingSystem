using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GradingSystem.Shared;

public record Error
{
    public string ErrorCode { get; }
    public string Message { get; }
    public ErrorType Type { get; }
    public Error(string code, string message, ErrorType type)
    {
        ErrorCode = code;
        Message = message;
        Type = type;
    }
    public static Error Failure(string code, string message) =>
        new(code, message, ErrorType.Failure);
    public static Error NotFound(string code, string message) =>
        new(code, message, ErrorType.NotFound);
    public static Error BadRequest(string code, string message) =>
        new(code, message, ErrorType.BadRequest);
    public static Error Unauthorized(string code, string message) =>
        new(code, message, ErrorType.Unauthorized);
    public static Error Forbidden(string code, string message) =>
        new(code, message, ErrorType.Forbidden);

    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
    public static readonly Error NullValue = new(
        "General.Null",
        "Null value was provided",
        ErrorType.Failure);

}
