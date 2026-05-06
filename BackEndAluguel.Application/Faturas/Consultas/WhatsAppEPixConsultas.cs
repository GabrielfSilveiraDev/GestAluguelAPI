using BackEndAluguel.Application.Configuracoes.DTOs;
using MediatR;

namespace BackEndAluguel.Application.Faturas.Consultas;

/// <summary>
/// Consulta CQRS para gerar o link WhatsApp (wa.me) com a mensagem padrão e o código PIX
/// embutidos, para envio ao inquilino via fatura.
/// </summary>
/// <param name="FaturaId">Identificador único da fatura.</param>
public record ObterLinkWhatsappConsulta(Guid FaturaId) : IRequest<WhatsAppLinkDto>;

/// <summary>
/// Consulta CQRS para gerar o código PIX copia-e-cola nativo de uma fatura
/// usando a ChavePix configurada pelo locador (sem gateway externo).
/// </summary>
/// <param name="FaturaId">Identificador único da fatura.</param>
public record GerarPixNativoConsulta(Guid FaturaId) : IRequest<string>;

