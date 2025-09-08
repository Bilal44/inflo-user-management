using System;
using System.Net;

namespace UserManagement.Services.Exceptions;

public class ApiException(HttpStatusCode statusCode, string errorMessage) : Exception
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public string ErrorMessage { get; } = errorMessage;
}
