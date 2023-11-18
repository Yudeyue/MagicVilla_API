namespace MagicVilla_API.Authentication
{
    public interface IRefreshhandler
    {
        Task<string> GenerateToken(string username);
    }
}
