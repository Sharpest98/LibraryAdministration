namespace LibraryAdministration.Exceptions
{
    public class BookIsAlreadyTakenException: Exception
    {
        public BookIsAlreadyTakenException(int id) : base($"Impossible to take the book with id: {id}. The book is already taken.")
        {
            
        }
    }
}
