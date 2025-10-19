namespace TallerBicicleta.Services
{
    public class CodigoBarrasService
    {
        private readonly ILogger<CodigoBarrasService> _logger;

        public CodigoBarrasService(ILogger<CodigoBarrasService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Genera un c�digo de barras en formato Base64 (PNG) usando ImageSharp para ser multiplataforma.
        /// </summary>
        public string GenerarCodigoBarrasBase64(string texto)
        {
            try
            {
                var writer = new BarcodeWriterPixelData
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = 100,
                        Width = 400,
                        Margin = 10,
                        PureBarcode = false
                    }
                };

                var pixelData = writer.Write(texto);

                // Crea una imagen ImageSharp a partir de los bytes de p�xeles (RGBA)
                using var image = Image.LoadPixelData<Rgba32>(pixelData.Pixels, pixelData.Width, pixelData.Height);

                using var ms = new MemoryStream();
                image.SaveAsPng(ms);
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar c�digo de barras");
                return string.Empty;
            }
        }
    }

}
