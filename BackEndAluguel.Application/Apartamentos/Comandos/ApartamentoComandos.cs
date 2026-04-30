using BackEndAluguel.Application.Apartamentos.DTOs;
using MediatR;

namespace BackEndAluguel.Application.Apartamentos.Comandos;

/// <summary>
/// Comando CQRS para criação de um novo apartamento.
/// Segue o padrão Command do CQRS — representa uma intenção de alterar o estado do sistema.
/// Retorna o DTO do apartamento criado após execução bem-sucedida.
/// </summary>
/// <param name="Numero">Número do apartamento (ex: "101").</param>
/// <param name="Bloco">Bloco ou torre (ex: "A").</param>
public record CriarApartamentoComando(
    string Numero,
    string? Bloco = null
) : IRequest<ApartamentoDto>;

/// <summary>
/// Comando CQRS para atualização dos dados de um apartamento existente.
/// </summary>
/// <param name="Id">Identificador único do apartamento a ser atualizado.</param>
/// <param name="Numero">Novo número do apartamento.</param>
/// <param name="Bloco">Novo bloco do apartamento.</param>
public record AtualizarApartamentoComando(
    Guid Id,
    string Numero,
    string? Bloco = null
) : IRequest<ApartamentoDto>;

/// <summary>
/// Comando CQRS para remoção de um apartamento existente.
/// Retorna verdadeiro se a remoção foi bem-sucedida.
/// </summary>
/// <param name="Id">Identificador único do apartamento a ser removido.</param>
public record RemoverApartamentoComando(Guid Id) : IRequest<bool>;

