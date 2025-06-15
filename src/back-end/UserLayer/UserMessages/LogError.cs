namespace UserMessages;

public record LogError(
    string ErrorMessage,
    string StackTrace,
    DateTime Timestamp);