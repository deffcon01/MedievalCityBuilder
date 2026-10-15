using System;

namespace MedievalCityBuilder.Persistence
{
    /// <summary>
    /// Thrown when a save string cannot be parsed into a valid <see cref="WorldSaveData"/>.
    /// Every deserialization failure is reported through this single exception type so callers
    /// do not need to distinguish between malformed JSON, missing fields and invalid values.
    /// </summary>
    public sealed class SaveFormatException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SaveFormatException"/> class with a message.
        /// </summary>
        /// <param name="message">A human-readable description of the format problem.</param>
        public SaveFormatException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveFormatException"/> class with a message and an inner exception.
        /// </summary>
        /// <param name="message">A human-readable description of the format problem.</param>
        /// <param name="innerException">The original exception that triggered this format error.</param>
        public SaveFormatException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}