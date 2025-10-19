using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SixLabors.ImageSharp.Formats.Png;
using ZXing.ImageSharp.Rendering;

namespace TallerBicicleta.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;
        private readonly string _smtpHost;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;
        private readonly string _senderName;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;

            // Configuración de SMTP desde appsettings.json
            _smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "smtp.gmail.com";
            _smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            _senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "";
            _senderPassword = _configuration["EmailSettings:SenderPassword"] ?? "";
            _senderName = _configuration["EmailSettings:SenderName"] ?? "Taller ProyectoWeb";
        }

        /// <summary>
        /// Envía una factura por correo electrónico con código de barras
        /// </summary>
        public async Task<bool> EnviarFacturaPorCorreoAsync(Factura factura, string emailDestino)
        {
            try
            {
                // Validar configuración de email
                if (string.IsNullOrEmpty(_senderEmail) || string.IsNullOrEmpty(_senderPassword))
                {
                    _logger.LogWarning("Configuración de email no encontrada. No se puede enviar el correo.");
                    return false;
                }

                // Generar código de barras
                var codigoBarrasBase64 = GenerarCodigoBarras(factura.NumeroFactura);

                // Crear mensaje
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_senderName, _senderEmail));
                message.To.Add(new MailboxAddress(factura.ClienteNombre, emailDestino));
                message.Subject = $"Factura {factura.NumeroFactura} - Taller";

                // Crear cuerpo HTML
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = CrearPlantillaFacturaHtml(factura, codigoBarrasBase64)
                };

                message.Body = bodyBuilder.ToMessageBody();

                // Enviar email
                using (var client = new SmtpClient())
                {
                    await client.ConnectAsync(_smtpHost, _smtpPort, SecureSocketOptions.StartTls);
                    await client.AuthenticateAsync(_senderEmail, _senderPassword);
                    await client.SendAsync(message);
                    await client.DisconnectAsync(true);
                }

                _logger.LogInformation("Factura {factura.NumeroFactura} enviada por correo a {emailDestino}", factura.NumeroFactura, emailDestino);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar factura {factura.NumeroFactura} por correo", factura.NumeroFactura);
                return false;
            }
        }

        /// <summary>
        /// Genera un código de barras en formato Base64
        /// </summary>
        private string GenerarCodigoBarras(string texto)
        {
            try
            {
                var writer = new ZXing.ImageSharp.BarcodeWriter<Rgba32>
                {
                    Format = BarcodeFormat.CODE_128,
                    Options = new EncodingOptions
                    {
                        Height = 100,
                        Width = 300,
                        Margin = 10,
                        PureBarcode = true
                    },
                    Renderer = new ImageSharpRenderer<Rgba32>()
                };

                using var image = writer.Write(texto);
                using var stream = new MemoryStream();

                image.Save(stream, new PngEncoder());
                var bytes = stream.ToArray();
                return Convert.ToBase64String(bytes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar código de barras");
                return string.Empty;
            }
        }

        /// <summary>
        /// Crea la plantilla HTML para el email de la factura
        /// </summary>
        private string CrearPlantillaFacturaHtml(Factura factura, string codigoBarrasBase64)
        {
            var html = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Factura {factura.NumeroFactura}</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .container {{
            background-color: white;
            border-radius: 10px;
            box-shadow: 0 0 20px rgba(0,0,0,0.1);
            overflow: hidden;
        }}
        .header {{
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            padding: 30px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 28px;
        }}
        .content {{
            padding: 30px;
        }}
        .factura-info {{
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            margin-bottom: 20px;
        }}
        .factura-info p {{
            margin: 8px 0;
        }}
        .barcode {{
            text-align: center;
            margin: 20px 0;
            padding: 20px;
            background-color: white;
            border: 2px solid #667eea;
            border-radius: 8px;
        }}
        .barcode img {{
            max-width: 100%;
            height: auto;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
        }}
        th {{
            background-color: #667eea;
            color: white;
            padding: 12px;
            text-align: left;
        }}
        td {{
            padding: 10px;
            border-bottom: 1px solid #ddd;
        }}
        .total-section {{
            background-color: #f8f9fa;
            padding: 20px;
            border-radius: 8px;
            margin-top: 20px;
        }}
        .total-row {{
            display: flex;
            justify-content: space-between;
            margin: 10px 0;
            font-size: 18px;
        }}
        .total-row.final {{
            font-weight: bold;
            font-size: 24px;
            color: #667eea;
            border-top: 2px solid #667eea;
            padding-top: 10px;
        }}
        .estado {{
            display: inline-block;
            padding: 8px 16px;
            border-radius: 20px;
            font-weight: bold;
            margin-top: 10px;
        }}
        .estado.pagada {{
            background-color: #28a745;
            color: white;
        }}
        .estado.pendiente {{
            background-color: #ffc107;
            color: #333;
        }}
        .footer {{
            text-align: center;
            padding: 20px;
            background-color: #f8f9fa;
            color: #666;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔧 FACTURA DE SERVICIO</h1>
            <p>Taller ProyectoWeb</p>
        </div>

        <div class='content'>
            <div class='barcode'>
                <h2 style='color: #667eea; margin-top: 0;'>{factura.NumeroFactura}</h2>
                {(string.IsNullOrEmpty(codigoBarrasBase64) ? "" : $"<img src='data:image/png;base64,{codigoBarrasBase64}' alt='Código de Barras' />")}
            </div>

            <div class='factura-info'>
                <h3 style='color: #667eea; margin-top: 0;'>Información de la Factura</h3>
                <p><strong>Fecha de Emisión:</strong> {factura.FechaEmision.ToLocalTime():dd/MM/yyyy HH:mm}</p>
                <p><strong>Cliente:</strong> {factura.ClienteNombre}</p>
                <p><strong>Empleado:</strong> {factura.EmpleadoNombre}</p>
                <p><strong>Estado:</strong> <span class='estado {(factura.Pagada ? "pagada" : "pendiente")}'>{(factura.Pagada ? "PAGADA" : "PENDIENTE")}</span></p>
            </div>

            <h3 style='color: #667eea;'>Servicio Realizado</h3>
            <div style='background-color: #f8f9fa; padding: 15px; border-radius: 8px; margin-bottom: 20px;'>
                <p style='margin: 0;'><strong>{factura.ServicioNombre}</strong></p>
                <p style='margin: 5px 0 0 0; text-align: right; font-size: 18px; color: #667eea;'><strong>${factura.PrecioServicio:N2}</strong></p>
            </div>

            {(factura.Detalles != null && factura.Detalles.Count != 0 ? $@"
            <h3 style='color: #667eea;'>Productos Utilizados</h3>
            <table>
                <thead>
                    <tr>
                        <th>Producto</th>
                        <th style='text-align: center;'>Cantidad</th>
                        <th style='text-align: right;'>Precio Unit.</th>
                        <th style='text-align: right;'>Subtotal</th>
                    </tr>
                </thead>
                <tbody>
                    {string.Join("", factura.Detalles.Select(d => $@"
                    <tr>
                        <td>{d.ProductoNombre}</td>
                        <td style='text-align: center;'>{d.Cantidad}</td>
                        <td style='text-align: right;'>${d.PrecioUnitario:N2}</td>
                        <td style='text-align: right;'>${d.Subtotal:N2}</td>
                    </tr>"))}
                </tbody>
            </table>" : "")}

            <div class='total-section'>
                <div class='total-row'>
                    <span>Subtotal Servicio:</span>
                    <span>${factura.PrecioServicio:N2}</span>
                </div>
                <div class='total-row'>
                    <span>Subtotal Productos:</span>
                    <span>${factura.SubtotalProductos:N2}</span>
                </div>
                <div class='total-row final'>
                    <span>TOTAL:</span>
                    <span>${factura.Total:N2}</span>
                </div>
                {(!factura.Pagada ? $@"
                <div class='total-row' style='color: #dc3545;'>
                    <span>Saldo Pendiente:</span>
                    <span>${factura.Saldo:N2}</span>
                </div>" : "")}
            </div>

            <div style='margin-top: 30px; padding: 20px; background-color: #e7f3ff; border-left: 4px solid #667eea; border-radius: 4px;'>
                <p style='margin: 0;'><strong>📧 ¿Necesitas ayuda?</strong></p>
                <p style='margin: 5px 0 0 0;'>Contáctanos al correo: {_senderEmail}</p>
            </div>
        </div>

        <div class='footer'>
            <p>Gracias por confiar en nuestro taller</p>
            <p>Este es un correo automático, por favor no responder</p>
        </div>
    </div>
</body>
</html>";

            return html;
        }
    }
}