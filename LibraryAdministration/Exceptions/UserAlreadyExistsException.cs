namespace LibraryAdministration.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException(string username) :
            base($"User {username} already exists")
        { }
    }
}
