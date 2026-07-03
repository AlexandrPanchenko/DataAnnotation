namespace JetFlight.Shared.Exceptions;

public class BadRequestException(string message) : Exception(message)
{
}
