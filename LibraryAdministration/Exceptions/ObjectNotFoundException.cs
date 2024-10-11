namespace LibraryAdministration.Exceptions
{
    public class ObjectNotFoundException : Exception
    {
        public ObjectNotFoundException(string name, int id) : base($"Object '{name}' with id '{id}' not found")
        {

        }
    }
}
