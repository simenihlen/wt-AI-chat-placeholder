namespace Testrepository.Server.Exceptions
{
    public class NoSuchUserException : Exception
    {
        public NoSuchUserException(string userId)
            : base($"User with ID '{userId}' does not exist.") { }
    }

    public class NoSuchSessionException : Exception
    {
        public NoSuchSessionException(int sessionId)
            : base($"Session with ID '{sessionId}' does not exist.") { }
    }

    public class InvalidPromptException : Exception
    {
        public InvalidPromptException(string prompt)
            : base($"Prompt '{prompt}' is invalid.") { }
    }
    public class NoSuchStoryException: Exception
    {
        public NoSuchStoryException(string storyId)
            : base($"Story with ID '{storyId}' does not exist.") { }
    }
    public class NoSuchProjectException: Exception
    {
        public NoSuchProjectException(int projectId)
            : base($"Project with ID '{projectId}' does not exist.") { }
    }
    public class  NoTitleException : Exception
    {
        public NoTitleException(string title)
            : base($"'{title}' is an invalid title") { }
    }
}