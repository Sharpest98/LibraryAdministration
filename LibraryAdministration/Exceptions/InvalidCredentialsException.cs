namespace LibraryAdministration.Exceptions
{
    public class InvalidCredentialsException :  Exception
    {
        public InvalidCredentialsException() :base($"Failed to login! Invalid Credentials!")
        {
            
        }
    }
}
