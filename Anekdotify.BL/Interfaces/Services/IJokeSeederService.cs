namespace Anekdotify.BL.Interfaces.Services
{
    public interface IJokeSeederService
    {
        Task SeedAsync(int count);
    }
}
