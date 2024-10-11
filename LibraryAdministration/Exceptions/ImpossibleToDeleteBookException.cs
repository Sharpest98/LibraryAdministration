namespace LibraryAdministration.Exceptions
{
    public class ImpossibleToDeleteBookException: Exception
    {
        public ImpossibleToDeleteBookException() : base("Impossible to delete book witch is taken by reader!") 
        {
            
        }
    }
}
