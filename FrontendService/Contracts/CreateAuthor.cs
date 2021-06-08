namespace FrontendService.Contracts
{
    public class CreateAuthor
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public uint Age { get; set; }
        public string Biography { get; set; }
    }
}