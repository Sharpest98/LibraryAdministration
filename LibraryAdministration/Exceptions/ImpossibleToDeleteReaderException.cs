namespace LibraryAdministration.Exceptions
{
    public class ImpossibleToDeleteReaderException: Exception
    {
        public ImpossibleToDeleteReaderException(): base("Impossible to delete reader with taken books from the library")
        {
            
        }
    }
}
