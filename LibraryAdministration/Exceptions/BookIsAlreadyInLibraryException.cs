namespace LibraryAdministration.Exceptions
{
    public class BookIsAlreadyInLibraryException:Exception
    {
        public BookIsAlreadyInLibraryException(int id) :base($"Book with id: {id} is already in the library")
        {
            
        }
    }
}
