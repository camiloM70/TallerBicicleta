using Stripe;

namespace TallerBicicleta.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PagoController(
        StripePaymentService stripeService,
        FacturaService facturaService,
        AbonoService abonoService,
        ILogger<PagoController> logger) : ControllerBase
    {
        private readonly StripePaymentService _stripeService = stripeService;
        private readonly FacturaService _facturaService = facturaService;
        private readonly AbonoService _abonoService = abonoService;
        private readonly ILogger<PagoController> _logger = logger;

        [HttpPost("crear-sesion/{facturaId}")]
        public async Task<IActionResult> CrearSesionPago(string facturaId, [FromBody] DatosPagoRequest request)
        {
            try
            {
                _logger.LogInformation("Creando sesión de pago para factura: {facturaId}", facturaId);

                var factura = await _facturaService.ObtenerFacturaPorIdAsync(facturaId);
                if (factura == null)
                {
                    _logger.LogWarning("Factura no encontrada: {facturaId}", facturaId);
                    return NotFound(new { mensaje = "Factura no encontrada" });
                }

                if (factura.Pagada)
                {
                    _logger.LogWarning("Factura ya pagada: {facturaId}",facturaId);
                    return BadRequest(new { mensaje = "Esta factura ya está pagada" });
                }

                // Crear URLs de éxito y cancelación
                var baseUrl = $"{Request.Scheme}://{Request.Host}";
                var urlExito = $"{baseUrl}/pago/exitoso?session_id={{CHECKOUT_SESSION_ID}}";
                var urlCancelacion = $"{baseUrl}/pago/cancelado?facturaId={facturaId}";

                _logger.LogInformation("URLs creadas - Éxito: {urlExito}, Cancelación: {urlCancelacion}",urlExito,urlCancelacion);
                _logger.LogInformation("Correo cliente: {request.CorreoCliente}",request.CorreoCliente);

                var urlPago = await _stripeService.CrearSesionPagoAsync(
                    factura,
                    request.CorreoCliente,
                    urlExito,
                    urlCancelacion
                );

                _logger.LogInformation("Sesión creada exitosamente, URL: {urlPago}", urlPago);
                return Ok(new { url = urlPago });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"EXCEPCIÓN al crear sesión de pago: {Message}", ex.Message);
                _logger.LogError(ex,"StackTrace: {StackTrace}", ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _logger.LogError(ex,"InnerException: {InnerException.Message}", ex.InnerException.Message);
                }
                return StatusCode(500, new { mensaje = $"Error al procesar el pago: {ex.Message}" });
            }
        }

        [HttpGet("verificar/{sesionId}")]
        public async Task<IActionResult> VerificarPago(string sesionId)
        {
            try
            {
                var (exitoso, estado, monto, facturaId) = await _stripeService.VerificarEstadoPagoAsync(sesionId);

                return Ok(new
                {
                    exitoso,
                    estado,
                    monto,
                    facturaId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error al verificar pago: {Message}", ex.Message);
                return StatusCode(500, new { mensaje = $"Error al verificar pago: {ex.Message}" });
            }
        }

        /// <summary>
        /// Webhook de Stripe para notificaciones de pagos
        /// </summary>
        [HttpPost("webhook")]
        public async Task<IActionResult> WebhookStripe()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var firmaStripe = Request.Headers["Stripe-Signature"].ToString();

            try
            {
                var evento = _stripeService.VerificarWebhook(json, firmaStripe);
                if (evento == null)
                {
                    return BadRequest(new { mensaje = "Firma de webhook inválida" });
                }

                _logger.LogInformation("Webhook recibido: {evento.Type}", evento.Type);

                // Procesar diferentes tipos de eventos
                switch (evento.Type)
                {
                    case "checkout.session.completed":
                        await ProcesarPagoExitoso(evento);
                        break;

                    case "checkout.session.expired":
                        _logger.LogInformation("Sesión de pago expiró: {evento.Id}", evento.Id);
                        break;

                    case "payment_intent.payment_failed":
                        _logger.LogWarning("Pago fallido: {evento.Id}",evento.Id);
                        break;
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error procesando webhook: {Message}", ex.Message);
                return StatusCode(500);
            }
        }

        private async Task ProcesarPagoExitoso(Event evento)
        {
            try
            {
                if (evento.Data.Object is not Stripe.Checkout.Session sesion) return;

                var facturaId = sesion.Metadata["facturaId"];
                var monto = (double)(sesion.AmountTotal ?? 0) / 100;

                _logger.LogInformation("Procesando pago exitoso para factura: {FacturaId}, monto: {Monto}", facturaId, monto);

                // Registrar el abono
                var abono = new Abono
                {
                    Id = Guid.NewGuid().ToString(),
                    FacturaId = facturaId,
                    Monto = monto,
                    FechaAbono = DateTime.Now,
                    MetodoPago = "Stripe - " + (sesion.PaymentMethodTypes?.FirstOrDefault() ?? "Tarjeta"),
                    Observaciones = $"Pago procesado vía Stripe. Sesión: {sesion.Id}"
                };

                await _abonoService.RegistrarAbonoAsync(abono);

                _logger.LogInformation("Abono registrado exitosamente para factura {facturaId}", facturaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,"Error al procesar pago exitoso: {Message}", ex.Message);
            }
        }

        [HttpGet("public-key")]
        public IActionResult ObtenerPublicKey()
        {
            return Ok(new { publicKey = _stripeService.ObtenerPublicKey() });
        }
    }

    public class DatosPagoRequest
    {
        public string CorreoCliente { get; set; } = "";
    }
}