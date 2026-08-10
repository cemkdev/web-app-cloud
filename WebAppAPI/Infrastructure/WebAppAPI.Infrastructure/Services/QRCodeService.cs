using QRCoder;
using WebAppAPI.Application.Abstractions.Services;

namespace WebAppAPI.Infrastructure.Services
{
    public sealed class QRCodeService : IQRCodeService
    {
        public byte[] Generate(string content)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(content);

            using QRCodeGenerator generator = new();
            using QRCodeData data = generator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

            using PngByteQRCode qRCode = new(data);

            return qRCode.GetGraphic(
                10,
                [84, 99, 71],
                [240, 240, 240]);
        }
    }
}
