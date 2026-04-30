using BackEndAluguel.Api.Modelos;
using BackEndAluguel.Application.Contratos.Comandos;
using BackEndAluguel.Application.Contratos.Consultas;
using BackEndAluguel.Application.Contratos.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BackEndAluguel.Api.Controllers;

/// <summary>
/// Controller responsavel pelo gerenciamento de contratos assinados dos inquilinos.
/// Permite o upload de arquivos PDF ou imagem e a consulta dos contratos existentes.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ContratosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;

    /// <summary>Inicializa o controller com mediator e configuracao.</summary>
    public ContratosController(IMediator mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    /// <summary>Lista todos os contratos de um inquilino.</summary>
    [HttpGet("inquilino/{inquilinoId:guid}")]
    [ProducesResponseType(typeof(RespostaApi<IEnumerable<ContratoInquilinoDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarPorInquilino(Guid inquilinoId, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ListarContratosPorInquilinoConsulta(inquilinoId), cancellationToken);
        return Ok(RespostaApi<IEnumerable<ContratoInquilinoDto>>.Ok(resultado));
    }

    /// <summary>Busca um contrato pelo ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RespostaApi<ContratoInquilinoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new ObterContratoPorIdConsulta(id), cancellationToken);
        if (resultado is null)
            return NotFound(RespostaErro.Criar($"Contrato com id '{id}' nao encontrado."));
        return Ok(RespostaApi<ContratoInquilinoDto>.Ok(resultado));
    }

    /// <summary>
    /// Faz o upload de um contrato (PDF ou imagem) e registra os metadados no banco.
    /// Tipos permitidos: application/pdf, image/jpeg, image/png.
    /// Tamanho maximo: 10 MB.
    /// </summary>
    [HttpPost("inquilino/{inquilinoId:guid}/upload")]
    [ProducesResponseType(typeof(RespostaApi<ContratoInquilinoDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(10 * 1024 * 1024)] // Limite de 10 MB
    public async Task<IActionResult> Upload(
        Guid inquilinoId,
        IFormFile arquivo,
        [FromForm] string? descricao,
        CancellationToken cancellationToken)
    {
        // Valida o tipo MIME permitido
        var tiposPermitidos = new[] { "application/pdf", "image/jpeg", "image/png", "image/jpg" };
        if (!tiposPermitidos.Contains(arquivo.ContentType.ToLower()))
            return BadRequest(RespostaErro.Criar("Tipo de arquivo nao permitido. Envie um PDF ou imagem (JPEG, PNG)."));

        if (arquivo.Length == 0)
            return BadRequest(RespostaErro.Criar("O arquivo nao pode estar vazio."));

        // Determina a pasta de armazenamento a partir da configuracao
        var pastaRaiz = _configuration["Contratos:PastaArmazenamento"] ?? "wwwroot/contratos";
        var pastaInquilino = Path.Combine(pastaRaiz, inquilinoId.ToString());
        Directory.CreateDirectory(pastaInquilino);

        // Gera um nome unico para o arquivo (evita colisoes)
        var extensao = Path.GetExtension(arquivo.FileName);
        var nomeArquivoSalvo = $"{Guid.NewGuid()}{extensao}";
        var caminhoRelativo = Path.Combine(inquilinoId.ToString(), nomeArquivoSalvo);
        var caminhoCompleto = Path.Combine(pastaRaiz, caminhoRelativo);

        // Salva o arquivo no disco
        await using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
            await arquivo.CopyToAsync(stream, cancellationToken);

        // Registra os metadados no banco via comando CQRS
        var comando = new RegistrarContratoComando(
            inquilinoId,
            arquivo.FileName,
            caminhoRelativo,
            arquivo.ContentType,
            arquivo.Length,
            descricao);

        var resultado = await _mediator.Send(comando, cancellationToken);
        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Id },
            RespostaApi<ContratoInquilinoDto>.Ok(resultado, "Contrato enviado com sucesso."));
    }

    /// <summary>
    /// Faz o download do arquivo de contrato pelo ID.
    /// </summary>
    [HttpGet("{id:guid}/download")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(Guid id, CancellationToken cancellationToken)
    {
        var contrato = await _mediator.Send(new ObterContratoPorIdConsulta(id), cancellationToken);
        if (contrato is null)
            return NotFound(RespostaErro.Criar($"Contrato com id '{id}' nao encontrado."));

        // Reconstroi o caminho completo a partir do caminho relativo persistido
        var pastaRaiz = _configuration["Contratos:PastaArmazenamento"] ?? "wwwroot/contratos";
        var caminhoCompleto = Path.Combine(pastaRaiz, contrato.CaminhoArquivo);

        if (!System.IO.File.Exists(caminhoCompleto))
            return NotFound(RespostaErro.Criar("Arquivo fisico nao encontrado no servidor."));

        var bytes = await System.IO.File.ReadAllBytesAsync(caminhoCompleto, cancellationToken);
        return File(bytes, contrato.TipoConteudo, contrato.NomeOriginalArquivo);
    }

    /// <summary>Atualiza a descricao de um contrato existente.</summary>
    [HttpPatch("{id:guid}/descricao")]
    [ProducesResponseType(typeof(RespostaApi<ContratoInquilinoDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarDescricao(Guid id, [FromBody] AtualizarDescricaoContratoCorpo corpo, CancellationToken cancellationToken)
    {
        var resultado = await _mediator.Send(new AtualizarDescricaoContratoComando(id, corpo.Descricao), cancellationToken);
        return Ok(RespostaApi<ContratoInquilinoDto>.Ok(resultado, "Descricao atualizada com sucesso."));
    }

    /// <summary>Remove um contrato e o arquivo fisico do servidor.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(RespostaErro), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id, CancellationToken cancellationToken)
    {
        await _mediator.Send(new RemoverContratoComando(id), cancellationToken);
        return NoContent();
    }
}

/// <summary>Corpo para atualizacao da descricao do contrato.</summary>
public record AtualizarDescricaoContratoCorpo(string? Descricao);

