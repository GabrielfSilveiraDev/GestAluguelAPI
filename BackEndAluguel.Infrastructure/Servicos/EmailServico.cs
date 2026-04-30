using System.Net;
using System.Net.Mail;
using BackEndAluguel.Application.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace BackEndAluguel.Infrastructure.Servicos;

/// <summary>
/// Implementação do serviço de e-mail usando SMTP via System.Net.Mail.
/// Configurações lidas do appsettings.json (seção "Email").
/// </summary>
public class EmailServico : IEmailServico
{
    private readonly IConfiguration _configuracao;
    private readonly ILogger<EmailServico> _logger;

    public EmailServico(IConfiguration configuracao, ILogger<EmailServico> logger)
    {
        _configuracao = configuracao;
        _logger = logger;
    }

    /// <summary>
    /// Envia o e-mail de confirmação de conta com o link para ativar a conta.
    /// O link aponta para a URL do frontend configurada em "Email:UrlFrontEnd".
    /// </summary>
    public async Task EnviarConfirmacaoContaAsync(
        string destinatario,
        string nomeDestinatario,
        string tokenConfirmacao,
        CancellationToken cancellationToken = default)
    {
        var urlFrontEnd = _configuracao["Email:UrlFrontEnd"] ?? "http://localhost:3000";
        var linkConfirmacao = $"{urlFrontEnd}/confirmar-email?token={tokenConfirmacao}";

        var assunto = "GestAluguel — Confirme sua conta";
        var corpo = MontarCorpoEmail(nomeDestinatario, linkConfirmacao);

        await EnviarAsync(destinatario, assunto, corpo, cancellationToken);

        _logger.LogInformation("E-mail de confirmação enviado para {Email}", destinatario);
    }

    // ─── Métodos auxiliares ────────────────────────────────────────────────

    private async Task EnviarAsync(string destinatario, string assunto, string corpo, CancellationToken cancellationToken)
    {
        var host = _configuracao["Email:Smtp:Host"] ?? throw new InvalidOperationException("Email:Smtp:Host não configurado.");
        var porta = int.TryParse(_configuracao["Email:Smtp:Porta"], out var p) ? p : 587;
        var usuario = _configuracao["Email:Smtp:Usuario"] ?? throw new InvalidOperationException("Email:Smtp:Usuario não configurado.");
        var senha = _configuracao["Email:Smtp:Senha"] ?? throw new InvalidOperationException("Email:Smtp:Senha não configurada.");
        var remetente = _configuracao["Email:Remetente"] ?? usuario;
        var nomeRemetente = _configuracao["Email:NomeRemetente"] ?? "GestAluguel";
        var ssl = bool.TryParse(_configuracao["Email:Smtp:UsarSsl"], out var s) && s;

        using var client = new SmtpClient(host, porta)
        {
            Credentials = new NetworkCredential(usuario, senha),
            EnableSsl = ssl,
            DeliveryMethod = SmtpDeliveryMethod.Network
        };

        using var mensagem = new MailMessage
        {
            From = new MailAddress(remetente, nomeRemetente),
            Subject = assunto,
            Body = corpo,
            IsBodyHtml = true
        };
        mensagem.To.Add(destinatario);

        await client.SendMailAsync(mensagem, cancellationToken);
    }

    private static string MontarCorpoEmail(string nomeDestinatario, string linkConfirmacao) => $"""
        <!DOCTYPE html>
        <html lang="pt-BR">
        <head><meta charset="UTF-8"><title>Confirme sua conta</title></head>
        <body style="font-family: Arial, sans-serif; background: #f4f4f4; padding: 24px;">
          <div style="max-width: 560px; margin: 0 auto; background: #fff; border-radius: 8px; padding: 32px; box-shadow: 0 2px 8px rgba(0,0,0,0.08);">
            <h2 style="color: #1a73e8;">Bem-vindo ao GestAluguel!</h2>
            <p>Olá, <strong>{nomeDestinatario}</strong>!</p>
            <p>Seu cadastro foi realizado com sucesso. Para ativar sua conta e começar a usar a plataforma, clique no botão abaixo:</p>
            <div style="text-align: center; margin: 32px 0;">
              <a href="{linkConfirmacao}"
                 style="background: #1a73e8; color: #fff; padding: 14px 28px; border-radius: 6px; text-decoration: none; font-size: 16px; font-weight: bold;">
                Confirmar minha conta
              </a>
            </div>
            <p style="color: #666; font-size: 13px;">Este link é válido por <strong>48 horas</strong>.</p>
            <p style="color: #666; font-size: 13px;">Se você não criou esta conta, ignore este e-mail.</p>
            <hr style="border: none; border-top: 1px solid #eee; margin: 24px 0;">
            <p style="color: #aaa; font-size: 12px; text-align: center;">© {DateTime.UtcNow.Year} GestAluguel. Todos os direitos reservados.</p>
          </div>
        </body>
        </html>
        """;
}

