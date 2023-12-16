namespace Orleans.EventSourcing.EventStorage.Marten;

/// <summary>
/// Exception for throwing from Redis event storage.
/// </summary>
[GenerateSerializer]
public class MartenEventStorageException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="MartenEventStorageException"/>.
    /// </summary>
    public MartenEventStorageException()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="MartenEventStorageException"/>.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public MartenEventStorageException(string message) : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="MartenEventStorageException"/>.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="inner">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
    public MartenEventStorageException(string message, Exception inner) : base(message, inner)
    {
    }
}