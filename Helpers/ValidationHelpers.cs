using System;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace MassTransit
{
    public static class ValidationHelpers
    {
        public static void EnsureNamingConventionForCommand(string commandClassName)
        {
            if (commandClassName == null)
                throw new ArgumentNullException($"{nameof(commandClassName)}");
            if (!commandClassName.EndsWith("Command") && !commandClassName.EndsWith("Query"))
                throw new InvalidCastException(
                    $"ConsumeContext<´> Send Endpoints should only be based on commands (class must be suffixed with `Command or `Query). Events have no send endpoints. You tried to use: {commandClassName}. Case Sensitive!");
        }

        public static void EnsureNamingConventionForMessage(string messageClassName)
        {
            var acceptedClassSuffices = new[] { "Command", "Event", "Query" };
            if (!messageClassName.EndsWithAnyOf(acceptedClassSuffices))
                throw new InvalidCastException(
                    $"Bus Send Endpoints should only be based on commands (class must be suffixed with `Command). Events have no send endpoints. You tried to use: {messageClassName}");
        }

        private static bool EndsWithAnyOf(this string source, IEnumerable<string> acceptableEndings)
        {
            foreach (var acceptableEnding in acceptableEndings)
            {
                if (source.EndsWith(acceptableEnding))
                    return true;
            }
            return false;
        }

    }
}
