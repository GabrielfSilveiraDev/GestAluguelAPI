using BackEndAluguel.Application.Inquilinos.DTOs;
using BackEndAluguel.Domain.Enumeradores;
using MediatR;

namespace BackEndAluguel.Application.Inquilinos.Comandos;

/// <summary>
/// Comando CQRS para cadastro de um novo inquilino vinculado a um apartamento.
/// Todo inquilino é também um morador — campos de RG, órgão emissor, telefone e estado civil são obrigatórios.
/// </summary>
public record CriarInquilinoComando(
    string NomeCompleto,
    string Cpf,
    DateOnly DataNascimento,
    string Rg,
    string OrgaoEmissor,
    string Telefone,
    EstadoCivil EstadoCivil,
    int QuantidadeMoradores,
    DateOnly DataEntrada,
    DateOnly DataVencimentoContrato,
    decimal ValorAluguel,
    Guid ApartamentoId,
    decimal Garagem = 0m,
    List<int>? DiasAlertaVencimento = null
) : IRequest<InquilinoDto>;

/// <summary>
/// Comando CQRS para atualização dos dados de um inquilino existente.
/// </summary>
public record AtualizarInquilinoComando(
    Guid Id,
    string NomeCompleto,
    int QuantidadeMoradores,
    DateOnly DataVencimentoContrato,
    decimal ValorAluguel,
    string Rg,
    string OrgaoEmissor,
    string Telefone,
    EstadoCivil EstadoCivil,
    decimal Garagem = 0m,
    List<int>? DiasAlertaVencimento = null
) : IRequest<InquilinoDto>;

/// <summary>
/// Comando CQRS para remoção de um inquilino.
/// Retorna verdadeiro se a remoção foi bem-sucedida.
/// </summary>
/// <param name="Id">Identificador único do inquilino a ser removido.</param>
public record RemoverInquilinoComando(Guid Id) : IRequest<bool>;

/// <summary>
/// Comandos para gerenciamento de dependentes de um inquilino.
/// </summary>
public record AdicionarDependenteComando(
    Guid InquilinoId,
    string NomeCompleto,
    string Cpf,
    string Rg,
    string OrgaoEmissor,
    DateOnly DataNascimento,
    string Telefone,
    EstadoCivil EstadoCivil
) : IRequest<DependenteDto>;

public record AtualizarDependenteComando(
    Guid Id,
    string NomeCompleto,
    string Rg,
    string OrgaoEmissor,
    DateOnly DataNascimento,
    string Telefone,
    EstadoCivil EstadoCivil
) : IRequest<DependenteDto>;

public record RemoverDependenteComando(Guid Id) : IRequest<bool>;
