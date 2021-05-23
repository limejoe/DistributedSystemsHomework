namespace FrontendService.Contracts
{
    public class CreateAuthor
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public byte Age { get; set; }
        public string Biography { get; set; }
    }
}