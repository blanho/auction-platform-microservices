using SkiaSharp;
using Storage.Infrastructure.Processing;
using Xunit;

namespace Storage.Application.Tests;

public class ImageProcessorTests
{
    [Theory]
    [InlineData(SKEncodedImageFormat.Png, "image/png")]
    [InlineData(SKEncodedImageFormat.Png, "IMAGE/PNG")]
    [InlineData(SKEncodedImageFormat.Jpeg, "image/jpeg")]
    [InlineData(SKEncodedImageFormat.Webp, "image/webp")]
    public async Task ProducesBoundedJpegAndPreservesAspectRatio(SKEncodedImageFormat format, string type)
    {
        var result = await ImageProcessor.ProcessAsync(ImageBytes(800, 400, format), type, default);
        Assert.Equal((800, 400), (result.Width, result.Height));
        using var thumbnail = DecodeThumbnail(result);
        Assert.Equal((320, 160), (thumbnail.Width, thumbnail.Height));
        Assert.StartsWith("data:image/jpeg;base64,", result.ThumbnailDataUrl);
    }

    [Fact]
    public async Task SmallImageIsNotUpscaledAndGifIsSupported()
    {
        var gif = Convert.FromBase64String("R0lGODlhAQABAIAAAAAAAP///yH5BAEAAAAALAAAAAABAAEAAAIBRAA7");
        var result = await ImageProcessor.ProcessAsync(gif, "image/gif", default);
        using var thumbnail = DecodeThumbnail(result);
        Assert.Equal((1, 1), (thumbnail.Width, thumbnail.Height));
    }

    [Theory]
    [InlineData(1, 0, 1, 2, 3)]
    [InlineData(2, 1, 0, 3, 2)]
    [InlineData(3, 3, 2, 1, 0)]
    [InlineData(4, 2, 3, 0, 1)]
    [InlineData(5, 0, 2, 1, 3)]
    [InlineData(6, 2, 0, 3, 1)]
    [InlineData(7, 3, 1, 2, 0)]
    [InlineData(8, 1, 3, 0, 2)]
    public async Task ExifOrientationIsAppliedAndRemoved(int orientation, int tl, int tr, int bl, int br)
    {
        SKColor[] colors = [SKColors.Red, SKColors.Lime, SKColors.Blue, SKColors.Yellow];
        using var bitmap = new SKBitmap(80, 40);
        for (var y = 0; y < 40; y++)
        {
            for (var x = 0; x < 80; x++)
            {
                bitmap.SetPixel(x, y, colors[(y < 20 ? 0 : 2) + (x < 40 ? 0 : 1)]);
            }
        }
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, 100);
        var jpeg = encoded.ToArray();
        byte[] exif = [0x45, 0x78, 0x69, 0x66, 0, 0, 0x49, 0x49, 42, 0, 8, 0, 0, 0,
            1, 0, 0x12, 1, 3, 0, 1, 0, 0, 0, (byte)orientation, 0, 0, 0, 0, 0, 0, 0];
        byte[] input = [.. jpeg[..2], 0xff, 0xe1, 0, 34, .. exif, .. jpeg[2..]];
        var result = await ImageProcessor.ProcessAsync(input, "image/jpeg", default);
        Assert.Equal(orientation >= 5 ? (40, 80) : (80, 40), (result.Width, result.Height));
        using var thumbnail = DecodeThumbnail(result);
        Near(colors[tl], thumbnail.GetPixel(5, 5));
        Near(colors[tr], thumbnail.GetPixel(thumbnail.Width - 6, 5));
        Near(colors[bl], thumbnail.GetPixel(5, thumbnail.Height - 6));
        Near(colors[br], thumbnail.GetPixel(thumbnail.Width - 6, thumbnail.Height - 6));
        using var data = SKData.CreateCopy(Convert.FromBase64String(result.ThumbnailDataUrl.Split(',')[1]));
        using var codec = SKCodec.Create(data);
        Assert.Equal(SKEncodedOrigin.TopLeft, codec.EncodedOrigin);
    }

    [Fact]
    public async Task CorruptMismatchedAndOversizedImagesAreRejected()
    {
        await Assert.ThrowsAsync<StorageJobValidationException>(() => ImageProcessor.ProcessAsync([1, 2, 3], "image/png", default));
        await Assert.ThrowsAsync<StorageJobValidationException>(() => ImageProcessor.ProcessAsync(ImageBytes(2, 2), "image/jpeg", default));
        await Assert.ThrowsAsync<StorageJobValidationException>(() => ImageProcessor.ProcessAsync(ImageBytes(4001, 4000), "image/png", default));
        await Assert.ThrowsAsync<StorageJobValidationException>(() => ImageProcessor.ProcessAsync(ImageBytes(2, 2), "text/plain", default));
    }

    [Fact]
    public async Task CanceledProcessingDoesNotProduceAResult()
    {
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ImageProcessor.ProcessAsync(
            ImageBytes(2, 2), "image/png", new CancellationToken(true)));
    }

    internal static byte[] ImageBytes(int width, int height, SKEncodedImageFormat format = SKEncodedImageFormat.Png)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.CornflowerBlue);
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(format, 90);
        return encoded.ToArray();
    }

    private static SKBitmap DecodeThumbnail(ProcessedImage image) =>
        SKBitmap.Decode(Convert.FromBase64String(image.ThumbnailDataUrl.Split(',')[1]));

    private static void Near(SKColor expected, SKColor actual)
    {
        Assert.InRange(Math.Abs(expected.Red - actual.Red), 0, 25);
        Assert.InRange(Math.Abs(expected.Green - actual.Green), 0, 25);
        Assert.InRange(Math.Abs(expected.Blue - actual.Blue), 0, 25);
    }
}
