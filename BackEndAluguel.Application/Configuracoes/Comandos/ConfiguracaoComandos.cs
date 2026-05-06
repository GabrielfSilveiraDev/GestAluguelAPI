using BackEndAluguel.Application.Configuracoes.DTOs;
using BackEndAluguel.Application.Pagamentos.DTOs;
using MediatR;

namespace BackEndAluguel.Application.Configuracoes.Comandos;

/// <summary>
/// Comando CQRS para atualizar os valores globais de configuracao do sistema.
/// </summary>
public record AtualizarConfiguracaoComando(
    decimal KwhValor,
    decimal ValorAgua
) : IRequest<ConfiguracaoDto>;

/// <summary>
/// Comando CQRS para registrar o locador (host) na plataforma Asaas como subconta.
/// </summary>
public record CriarSubcontaAsaasComando(
    string Nome,
    string Email,
    string CpfCnpj,
    string TipoPessoa,
    string? Telefone = null,
    string? Site = null
) : IRequest<SubcontaResultadoDto>;

/// <summary>
/// Comando CQRS para configurar a integração com WhatsApp.
/// </summary>
/// <param name="NumeroWhatsapp">Número no formato internacional sem '+' (ex: 5511999999999).</param>
/// <param name="MensagemPadrao">
/// Template. Placeholders disponíveis: {inquilino}, {mesReferencia}, {valorTotal}, {dataVencimento}, {codigoPix}.
/// </param>
public record AtualizarWhatsappComando(
    string NumeroWhatsapp,
    string MensagemPadrao
) : IRequest<ConfiguracaoDto>;

/// <summary>
/// Comando CQRS para configurar os dados PIX nativos do locador (sem gateway externo).
/// </summary>
/// <param name="ChavePix">Chave PIX (CPF, CNPJ, e-mail, telefone ou chave aleatória).</param>
/// <param name="NomeRecebedor">Nome do recebedor (máx. 25 chars).</param>
/// <param name="CidadeRecebedor">Cidade do recebedor (máx. 15 chars).</param>
public record AtualizarPixNativoComando(
    string ChavePix,
    string NomeRecebedor,
    string CidadeRecebedor
) : IRequest<ConfiguracaoDto>;

