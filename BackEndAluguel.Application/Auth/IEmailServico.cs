namespace BackEndAluguel.Application.Auth;

/// <summary>
/// Contrato para envio de e-mails transacionais.
/// A implementação concreta fica na camada Infrastructure (SMTP via System.Net.Mail).
/// </summary>
public interface IEmailServico
{
    /// <summary>
    /// Envia o e-mail de confirmação de conta para o host recém-registrado.
    /// </summary>
    /// <param name="destinatario">Endereço de e-mail do destinatário.</param>
    /// <param name="nomeDestinatario">Nome do destinatário para personalização.</param>
    /// <param name="tokenConfirmacao">Token único de confirmação de conta.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    Task EnviarConfirmacaoContaAsync(
        string destinatario,
        string nomeDestinatario,
        string tokenConfirmacao,
        CancellationToken cancellationToken = default);
}

