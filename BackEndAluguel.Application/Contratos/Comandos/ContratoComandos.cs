using BackEndAluguel.Application.Contratos.DTOs;
using MediatR;

namespace BackEndAluguel.Application.Contratos.Comandos;

/// <summary>
/// Comando CQRS para registrar os metadados de um contrato ja salvo no disco.
/// O upload do arquivo e realizado na camada Web (Controller) antes de chamar este comando.
/// </summary>
/// <param name="InquilinoId">Identificador do inquilino.</param>
/// <param name="NomeOriginalArquivo">Nome original do arquivo enviado pelo usuario.</param>
/// <param name="CaminhoArquivo">Caminho relativo no servidor onde o arquivo foi salvo.</param>
/// <param name="TipoConteudo">Tipo MIME do arquivo.</param>
/// <param name="TamanhoBytes">Tamanho do arquivo em bytes.</param>
/// <param name="Descricao">Descricao opcional do contrato.</param>
public record RegistrarContratoComando(
    Guid InquilinoId,
    string NomeOriginalArquivo,
    string CaminhoArquivo,
    string TipoConteudo,
    long TamanhoBytes,
    string? Descricao = null
) : IRequest<ContratoInquilinoDto>;

/// <summary>Comando CQRS para atualizar a descricao de um contrato existente.</summary>
public record AtualizarDescricaoContratoComando(Guid Id, string? Descricao) : IRequest<ContratoInquilinoDto>;

/// <summary>
/// Comando CQRS para remover um contrato do sistema.
/// O arquivo fisico tambem deve ser removido pelo handler.
/// </summary>
public record RemoverContratoComando(Guid Id) : IRequest<bool>;

