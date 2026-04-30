namespace BackEndAluguel.Application.Configuracoes.DTOs;

/// <summary>
/// DTO de leitura da configuracao global do sistema.
/// </summary>
public record ConfiguracaoDto(
    /// <summary>Identificador unico da configuracao.</summary>
    Guid Id,
    /// <summary>Valor do kWh em reais (usado para calcular o custo de energia).</summary>
    decimal KwhValor,
    /// <summary>Valor fixo mensal da agua cobrado de todos os inquilinos.</summary>
    decimal ValorAgua,
    /// <summary>Data da ultima atualizacao da configuracao.</summary>
    DateTime? AtualizadoEm,
    /// <summary>
    /// WalletId do Host na plataforma Asaas.
    /// Preenchido apos o registro da subconta via endpoint de integracao Asaas.
    /// </summary>
    string? WalletIdAsaas = null
);

