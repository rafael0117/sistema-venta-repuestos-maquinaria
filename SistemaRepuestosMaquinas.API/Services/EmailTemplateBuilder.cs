using System.Globalization;
using System.Net;

namespace SistemaRepuestosMaquinas.API.Services;

public static class EmailTemplateBuilder
{
    public static string BuildPasswordResetHtml(string fullName, string resetUrl)
    {
        var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(fullName) ? "cliente" : fullName);
        var safeUrl = WebUtility.HtmlEncode(resetUrl);

        return $$"""
<!doctype html>
<html lang="es">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>Recuperación de contraseña</title>
</head>
<body style="margin:0;padding:0;background:#f4f6fb;font-family:Segoe UI,Arial,sans-serif;color:#1f2937;">
  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="padding:24px 0;">
    <tr>
      <td align="center">
        <table role="presentation" width="620" cellspacing="0" cellpadding="0" style="max-width:620px;background:#ffffff;border-radius:16px;overflow:hidden;border:1px solid #e5e7eb;">
          <tr>
            <td style="background:linear-gradient(90deg,#0f172a,#1d4ed8);padding:22px 28px;color:#fff;">
              <h1 style="margin:0;font-size:20px;">🔐 Recuperación de contraseña</h1>
              <p style="margin:8px 0 0 0;font-size:13px;opacity:.9;">Sistema Repuestos Maquinaria Pro</p>
            </td>
          </tr>
          <tr>
            <td style="padding:28px;">
              <p style="margin:0 0 14px 0;">Hola <strong>{{safeName}}</strong>,</p>
              <p style="margin:0 0 14px 0;line-height:1.55;">Recibimos una solicitud para restablecer tu contraseña. Usa el botón de abajo para crear una nueva clave.</p>
              <p style="margin:0 0 20px 0;line-height:1.55;"><strong>Importante:</strong> este enlace expira en 30 minutos.</p>

              <table role="presentation" cellspacing="0" cellpadding="0" style="margin:0 0 20px 0;">
                <tr>
                  <td style="border-radius:8px;background:#2563eb;">
                    <a href="{{safeUrl}}" style="display:inline-block;padding:12px 20px;color:#fff;text-decoration:none;font-weight:600;">Restablecer contraseña</a>
                  </td>
                </tr>
              </table>

              <p style="margin:0 0 10px 0;font-size:13px;color:#6b7280;">Si no solicitaste este cambio, puedes ignorar este mensaje.</p>
              <p style="margin:0;font-size:12px;color:#9ca3af;word-break:break-all;">Enlace directo: {{safeUrl}}</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
""";
    }

    public static string BuildOrderConfirmationHtml(OrderEmailData data)
    {
        var culture = CultureInfo.GetCultureInfo("es-PE");
        var safeClient = WebUtility.HtmlEncode(data.ClientName);
        var safeOrderId = WebUtility.HtmlEncode(data.OrderId.ToString(culture));
        var safeAddress = WebUtility.HtmlEncode(data.DeliveryAddress);
        var safePayment = WebUtility.HtmlEncode(data.PaymentMethod);

        var rows = string.Join(string.Empty, data.Items.Select(item =>
        {
            var name = WebUtility.HtmlEncode(item.ProductName);
            var qty = item.Quantity.ToString(culture);
            var unit = item.UnitPrice.ToString("C2", culture);
            var subtotal = item.SubTotal.ToString("C2", culture);
            return $"<tr><td style='padding:10px;border-bottom:1px solid #e5e7eb;'>{name}</td><td style='padding:10px;border-bottom:1px solid #e5e7eb;text-align:center;'>{qty}</td><td style='padding:10px;border-bottom:1px solid #e5e7eb;text-align:right;'>{unit}</td><td style='padding:10px;border-bottom:1px solid #e5e7eb;text-align:right;'>{subtotal}</td></tr>";
        }));

        var total = data.Total.ToString("C2", culture);
        var orderDate = data.OrderDateLocal.ToString("dd/MM/yyyy HH:mm", culture);

        return $$"""
<!doctype html>
<html lang="es">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width,initial-scale=1" />
  <title>Confirmación de pedido</title>
</head>
<body style="margin:0;padding:0;background:#f4f6fb;font-family:Segoe UI,Arial,sans-serif;color:#1f2937;">
  <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="padding:24px 0;">
    <tr>
      <td align="center">
        <table role="presentation" width="700" cellspacing="0" cellpadding="0" style="max-width:700px;background:#ffffff;border-radius:16px;overflow:hidden;border:1px solid #e5e7eb;">
          <tr>
            <td style="background:linear-gradient(90deg,#0f172a,#2563eb);padding:22px 28px;color:#fff;">
              <h1 style="margin:0;font-size:21px;">✅ Pedido confirmado</h1>
              <p style="margin:8px 0 0 0;font-size:13px;opacity:.9;">Gracias por tu compra en Repuestos Maquinaria Pro</p>
            </td>
          </tr>
          <tr>
            <td style="padding:28px;">
              <p style="margin:0 0 8px 0;">Hola <strong>{{safeClient}}</strong>,</p>
              <p style="margin:0 0 18px 0;line-height:1.55;">Tu pago fue aprobado y registramos correctamente tu pedido <strong>#{{safeOrderId}}</strong>.</p>

              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="margin-bottom:18px;background:#f8fafc;border:1px solid #e5e7eb;border-radius:10px;">
                <tr><td style="padding:14px 16px;font-size:14px;"><strong>Fecha:</strong> {{orderDate}}</td></tr>
                <tr><td style="padding:0 16px 14px 16px;font-size:14px;"><strong>Dirección de entrega:</strong> {{safeAddress}}</td></tr>
                <tr><td style="padding:0 16px 14px 16px;font-size:14px;"><strong>Método de pago:</strong> {{safePayment}}</td></tr>
              </table>

              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="border:1px solid #e5e7eb;border-radius:10px;overflow:hidden;">
                <thead>
                  <tr style="background:#f1f5f9;">
                    <th style="padding:10px;text-align:left;font-size:13px;">Producto</th>
                    <th style="padding:10px;text-align:center;font-size:13px;">Cant.</th>
                    <th style="padding:10px;text-align:right;font-size:13px;">P. Unit.</th>
                    <th style="padding:10px;text-align:right;font-size:13px;">Subtotal</th>
                  </tr>
                </thead>
                <tbody>
                  {{rows}}
                </tbody>
                <tfoot>
                  <tr>
                    <td colspan="3" style="padding:12px 10px;text-align:right;font-weight:700;">Total</td>
                    <td style="padding:12px 10px;text-align:right;font-weight:700;color:#0f172a;">{{total}}</td>
                  </tr>
                </tfoot>
              </table>

              <p style="margin:18px 0 0 0;font-size:12px;color:#6b7280;">Este correo confirma tu compra. Si tienes dudas, responde este mensaje y te ayudaremos.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>
""";
    }
}

public record OrderEmailData(
    string ClientName,
    int OrderId,
    DateTime OrderDateLocal,
    string DeliveryAddress,
    string PaymentMethod,
    decimal Total,
    IReadOnlyCollection<OrderEmailItemData> Items);

public record OrderEmailItemData(string ProductName, int Quantity, decimal UnitPrice, decimal SubTotal);
