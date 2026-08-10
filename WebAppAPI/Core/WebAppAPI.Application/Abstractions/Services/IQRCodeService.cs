namespace WebAppAPI.Application.Abstractions.Services
{
    public interface IQRCodeService
    {
        byte[] Generate(string content);
    }
}
