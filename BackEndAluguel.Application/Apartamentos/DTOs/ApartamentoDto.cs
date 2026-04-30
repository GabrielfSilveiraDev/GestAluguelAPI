namespace BackEndAluguel.Application.Apartamentos.DTOs;

/// <summary>
/// Objeto de Transferência de Dados (DTO) para exposição das informações de um apartamento.
/// Utilizado como resultado de consultas (Queries) no padrão CQRS.
/// </summary>
public record ApartamentoDto(
    /// <summary>Identificador único do apartamento.</summary>
    Guid Id,
    /// <summary>Número do apartamento.</summary>
    string Numero,
    /// <summary>Bloco ou torre do apartamento.</summary>
    string? Bloco,
    /// <summary>Indica se o apartamento está atualmente ocupado.</summary>
    bool Ocupado,
    /// <summary>Data de criação do registro.</summary>
    DateTime CriadoEm
);
