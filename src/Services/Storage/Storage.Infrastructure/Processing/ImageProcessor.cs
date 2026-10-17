using SkiaSharp;

namespace Storage.Infrastructure.Processing;

public sealed record ProcessedImage(int Width, int Height, string ThumbnailDataUrl);

public sealed class ImageProcessor
{
    public static bool Supports(string contentType) => contentType?.ToLowerInvariant() is "image/jpeg" or "image/png" or "image/gif" or "image/webp";

    public Task<ProcessedImage> ProcessAsync(byte[] bytes, string contentType, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (!Supports(contentType))
            throw new StorageJobValidationException("Only JPEG, PNG, GIF, and WebP images are supported.");
        using var data = SKData.CreateCopy(bytes);
        using var codec = SKCodec.Create(data)
            ?? throw new StorageJobValidationException("The image is corrupt or uses an unsupported format.");
        var actualType = codec.EncodedFormat switch
        {
            SKEncodedImageFormat.Jpeg => "image/jpeg",
            SKEncodedImageFormat.Png => "image/png",
            SKEncodedImageFormat.Gif => "image/gif",
            SKEncodedImageFormat.Webp => "image/webp",
            _ => null
        };
        if (!string.Equals(contentType, actualType, StringComparison.OrdinalIgnoreCase))
            throw new StorageJobValidationException("Image bytes do not match the declared content type.");
        var info = codec.Info;
        if (info.Width <= 0 || info.Height <= 0 || (long)info.Width * info.Height > StorageDefaults.Images.MaxPixels)
            throw new StorageJobValidationException("Image dimensions exceed the 16-megapixel limit.");
        using var bitmap = new SKBitmap(new SKImageInfo(info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul));
        if (codec.GetPixels(bitmap.Info, bitmap.GetPixels(), new SKCodecOptions(0)) != SKCodecResult.Success)
            throw new StorageJobValidationException("The image is corrupt or incomplete.");
        ct.ThrowIfCancellationRequested();
        var swapsAxes = codec.EncodedOrigin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or
            SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var width = swapsAxes ? info.Height : info.Width;
        var height = swapsAxes ? info.Width : info.Height;
        var scale = Math.Min(1d, (double)StorageDefaults.Images.ThumbnailSize / Math.Max(width, height));
        var thumbnailWidth = Math.Max(1, (int)Math.Round(width * scale));
        var thumbnailHeight = Math.Max(1, (int)Math.Round(height * scale));
        using var surface = SKSurface.Create(new SKImageInfo(thumbnailWidth, thumbnailHeight));
        var canvas = surface.Canvas;
        canvas.Clear(SKColors.White);
        canvas.Scale((float)thumbnailWidth / width, (float)thumbnailHeight / height);
        ApplyOrientation(canvas, codec.EncodedOrigin, info.Width, info.Height);
        using var decoded = SKImage.FromBitmap(bitmap);
        canvas.DrawImage(decoded, 0, 0, new SKSamplingOptions(SKFilterMode.Linear));
        using var thumbnail = surface.Snapshot();
        using var encoded = thumbnail.Encode(SKEncodedImageFormat.Jpeg, 80);
        ct.ThrowIfCancellationRequested();
        if (encoded.Size > StorageDefaults.Images.MaxThumbnailBytes)
            throw new StorageJobValidationException("The generated thumbnail exceeds the size limit.");
        // Encoding a fresh surface omits the source's EXIF/GPS and other metadata.
        return Task.FromResult(new ProcessedImage(width, height,
            "data:image/jpeg;base64," + Convert.ToBase64String(encoded.ToArray())));
    }

    private static void ApplyOrientation(SKCanvas canvas, SKEncodedOrigin origin, int width, int height)
    {
        switch (origin)
        {
            case SKEncodedOrigin.TopRight:
                canvas.Translate(width, 0); canvas.Scale(-1, 1); break;
            case SKEncodedOrigin.BottomRight:
                canvas.Translate(width, height); canvas.RotateDegrees(180); break;
            case SKEncodedOrigin.BottomLeft:
                canvas.Translate(0, height); canvas.Scale(1, -1); break;
            case SKEncodedOrigin.LeftTop:
                canvas.RotateDegrees(90); canvas.Scale(1, -1); break;
            case SKEncodedOrigin.RightTop:
                canvas.Translate(height, 0); canvas.RotateDegrees(90); break;
            case SKEncodedOrigin.RightBottom:
                canvas.Translate(height, width); canvas.RotateDegrees(90); canvas.Scale(-1, 1); break;
            case SKEncodedOrigin.LeftBottom:
                canvas.Translate(0, width); canvas.RotateDegrees(270); break;
        }
    }
}
