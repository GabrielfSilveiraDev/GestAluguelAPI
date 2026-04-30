using BackEndAluguel.Domain.Enumeradores;

namespace BackEndAluguel.Application.Inquilinos.DTOs;

/// <summary>
/// DTO de resposta para o Inquilino com todos os dados pessoais e contratuais.
/// </summary>
public record InquilinoDto(
    Guid Id,
    string NomeCompleto,
    string Cpf,
    DateOnly DataNascimento,
    string Rg,
    string OrgaoEmissor,
    string Telefone,
    EstadoCivil EstadoCivil,
    string EstadoCivilDescricao,
    int QuantidadeMoradores,
    DateOnly DataEntrada,
    DateOnly DataVencimentoContrato,
    decimal ValorAluguel,
    decimal Garagem,
    Guid ApartamentoId,
    /// <summary>Número do apartamento (ex: "101"). Nulo se o apartamento não foi carregado.</summary>
    string? NumeroApartamento,
    /// <summary>Bloco do apartamento (ex: "A"). Nulo se não houver bloco ou não foi carregado.</summary>
    string? BlocoApartamento,
    List<int> DiasAlertaVencimento,
    DateTime CriadoEm
);

public record DependenteDto(
    /// <summary>Identificador único do dependente.</summary>
    Guid Id,
    /// <summary>Nome completo do dependente.</summary>
    string NomeCompleto,
    /// <summary>CPF do dependente (somente dígitos).</summary>
    string Cpf,
    /// <summary>RG do dependente.</summary>
    string Rg,
    /// <summary>Órgão emissor do RG.</summary>
    string OrgaoEmissor,
    /// <summary>Data de nascimento do dependente.</summary>
    DateOnly DataNascimento,
    /// <summary>Telefone de contato do dependente.</summary>
    string Telefone,
    /// <summary>Estado civil do dependente.</summary>
    EstadoCivil EstadoCivil,
    /// <summary>Descrição do estado civil do dependente.</summary>
    string EstadoCivilDescricao,
    /// <summary>Identificador do inquilino ao qual o dependente está vinculado.</summary>
    Guid InquilinoId,
    /// <summary>Data de criação do registro.</summary>
    DateTime CriadoEm
);
