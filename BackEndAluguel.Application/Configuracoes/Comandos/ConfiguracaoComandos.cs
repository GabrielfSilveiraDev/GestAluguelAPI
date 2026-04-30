using BackEndAluguel.Application.Configuracoes.DTOs;
using BackEndAluguel.Application.Pagamentos.DTOs;
using MediatR;

namespace BackEndAluguel.Application.Configuracoes.Comandos;

/// <summary>
/// Comando CQRS para atualizar os valores globais de configuracao do sistema.
/// </summary>
/// <param name="KwhValor">Novo valor do kWh em reais.</param>
/// <param name="ValorAgua">Novo valor fixo mensal da agua.</param>
public record AtualizarConfiguracaoComando(
    decimal KwhValor,
    decimal ValorAgua
) : IRequest<ConfiguracaoDto>;

/// <summary>
/// Comando CQRS para registrar o locador (host) na plataforma Asaas como subconta.
/// Apos o cadastro, o WalletId retornado e salvo automaticamente na Configuracao global
/// para uso nos splits de pagamento PIX.
/// </summary>
/// <param name="Nome">Nome completo ou razao social do host.</param>
/// <param name="Email">E-mail do host para acesso a subconta Asaas.</param>
/// <param name="CpfCnpj">CPF ou CNPJ do host (somente digitos).</param>
/// <param name="TipoPessoa">"FISICA" ou "JURIDICA".</param>
/// <param name="Telefone">Telefone de contato (opcional).</param>
/// <param name="Site">Site do host (opcional).</param>
public record CriarSubcontaAsaasComando(
    string Nome,
    string Email,
    string CpfCnpj,
    string TipoPessoa,
    string? Telefone = null,
    string? Site = null
) : IRequest<SubcontaResultadoDto>;
