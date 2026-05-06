using BackEndAluguel.Application.Comum.Excecoes;
using BackEndAluguel.Application.Configuracoes.Comandos;
using BackEndAluguel.Application.Configuracoes.Consultas;
using BackEndAluguel.Application.Configuracoes.DTOs;
using BackEndAluguel.Application.Pagamentos;
using BackEndAluguel.Application.Pagamentos.DTOs;
using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using MediatR;

namespace BackEndAluguel.Application.Configuracoes.Manipuladores;

/// <summary>
/// Manipulador CQRS para a consulta <see cref="ObterConfiguracaoConsulta"/>.
/// Retorna a configuracao global unica do sistema.
/// </summary>
public class ObterConfiguracaoManipulador : IRequestHandler<ObterConfiguracaoConsulta, ConfiguracaoDto?>
{
    private readonly IConfiguracaoRepositorio _repositorio;

    /// <summary>Inicializa o manipulador com o repositorio de configuracoes.</summary>
    public ObterConfiguracaoManipulador(IConfiguracaoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>Processa a consulta retornando a configuracao atual.</summary>
    public async Task<ConfiguracaoDto?> Handle(ObterConfiguracaoConsulta request, CancellationToken cancellationToken)
    {
        var config = await _repositorio.ObterConfiguracaoAsync(cancellationToken);
        return config is null ? null : ConverterParaDto(config);
    }

    /// <summary>Converte a entidade Configuracao para DTO de resposta.</summary>
    internal static ConfiguracaoDto ConverterParaDto(Configuracao c)
        => new(c.Id, c.KwhValor, c.ValorAgua, c.AtualizadoEm, c.WalletIdAsaas,
               c.NumeroWhatsappLocador, c.MensagemPadraoWhatsapp,
               c.ChavePix, c.NomeRecebedorPix, c.CidadeRecebedorPix);
}

/// <summary>
/// Manipulador CQRS para o comando <see cref="AtualizarConfiguracaoComando"/>.
/// Atualiza os valores globais de kWh e agua do sistema.
/// </summary>
public class AtualizarConfiguracaoManipulador : IRequestHandler<AtualizarConfiguracaoComando, ConfiguracaoDto>
{
    private readonly IConfiguracaoRepositorio _repositorio;

    /// <summary>Inicializa o manipulador com o repositorio de configuracoes.</summary>
    public AtualizarConfiguracaoManipulador(IConfiguracaoRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>Processa a atualizacao da configuracao global do sistema (cria se nao existir — upsert).</summary>
    public async Task<ConfiguracaoDto> Handle(AtualizarConfiguracaoComando request, CancellationToken cancellationToken)
    {
        var config = await _repositorio.ObterConfiguracaoAsync(cancellationToken);

        if (config is null)
        {
            // Primeira configuracao: cria o registro singleton
            config = new Configuracao(request.KwhValor, request.ValorAgua);
            await _repositorio.AdicionarAsync(config, cancellationToken);
        }
        else
        {
            config.Atualizar(request.KwhValor, request.ValorAgua);
            _repositorio.Atualizar(config);
        }

        await _repositorio.SalvarAlteracoesAsync(cancellationToken);
        return ObterConfiguracaoManipulador.ConverterParaDto(config);
    }
}

/// <summary>
/// Manipulador CQRS para o comando <see cref="CriarSubcontaAsaasComando"/>.
/// Registra o locador (host) na plataforma Asaas, obtendo o WalletId para uso no split de pagamentos PIX.
/// O WalletId e armazenado na Configuracao global para uso automatico na geracao de cobranças.
/// </summary>
public class CriarSubcontaAsaasManipulador : IRequestHandler<CriarSubcontaAsaasComando, SubcontaResultadoDto>
{
    private readonly IConfiguracaoRepositorio _configuracaoRepositorio;
    private readonly IServicoGatewayPagamento _gatewayPagamento;

    /// <summary>Inicializa o manipulador com os servicos necessarios.</summary>
    public CriarSubcontaAsaasManipulador(
        IConfiguracaoRepositorio configuracaoRepositorio,
        IServicoGatewayPagamento gatewayPagamento)
    {
        _configuracaoRepositorio = configuracaoRepositorio;
        _gatewayPagamento = gatewayPagamento;
    }

    /// <summary>
    /// Registra a subconta no Asaas e persiste o WalletId retornado na configuracao global.
    /// </summary>
    public async Task<SubcontaResultadoDto> Handle(CriarSubcontaAsaasComando request, CancellationToken cancellationToken)
    {
        var dados = new CriarSubcontaDto(
            Nome: request.Nome,
            Email: request.Email,
            CpfCnpj: request.CpfCnpj,
            TipoPessoa: request.TipoPessoa,
            Telefone: request.Telefone,
            Site: request.Site);

        var resultado = await _gatewayPagamento.CriarSubcontaAsync(dados, cancellationToken);

        // Persiste o WalletId na configuracao global para uso futuro no split PIX
        var config = await _configuracaoRepositorio.ObterConfiguracaoAsync(cancellationToken)
            ?? throw new RegraDeNegocioExcecao("Configuracao global nao encontrada. Contate o suporte.");

        config.AtualizarWalletIdAsaas(resultado.WalletId);
        _configuracaoRepositorio.Atualizar(config);
        await _configuracaoRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return resultado;
    }
}

/// <summary>
/// Manipulador CQRS para o comando <see cref="AtualizarWhatsappComando"/>.
/// Salva o número de WhatsApp do locador e o template de mensagem padrão.
/// </summary>
public class AtualizarWhatsappManipulador : IRequestHandler<AtualizarWhatsappComando, ConfiguracaoDto>
{
    private readonly IConfiguracaoRepositorio _repositorio;

    public AtualizarWhatsappManipulador(IConfiguracaoRepositorio repositorio) => _repositorio = repositorio;

    public async Task<ConfiguracaoDto> Handle(AtualizarWhatsappComando request, CancellationToken cancellationToken)
    {
        var config = await _repositorio.ObterConfiguracaoAsync(cancellationToken)
            ?? throw new RegraDeNegocioExcecao("Configuracao global nao encontrada. Use PUT /api/configuracoes para criar.");

        config.AtualizarWhatsapp(request.NumeroWhatsapp, request.MensagemPadrao);
        _repositorio.Atualizar(config);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return ObterConfiguracaoManipulador.ConverterParaDto(config);
    }
}

/// <summary>
/// Manipulador CQRS para o comando <see cref="AtualizarPixNativoComando"/>.
/// Salva os dados de PIX nativo (chave, nome e cidade) para geração de código sem gateway.
/// </summary>
public class AtualizarPixNativoManipulador : IRequestHandler<AtualizarPixNativoComando, ConfiguracaoDto>
{
    private readonly IConfiguracaoRepositorio _repositorio;

    public AtualizarPixNativoManipulador(IConfiguracaoRepositorio repositorio) => _repositorio = repositorio;

    public async Task<ConfiguracaoDto> Handle(AtualizarPixNativoComando request, CancellationToken cancellationToken)
    {
        var config = await _repositorio.ObterConfiguracaoAsync(cancellationToken)
            ?? throw new RegraDeNegocioExcecao("Configuracao global nao encontrada. Use PUT /api/configuracoes para criar.");

        config.AtualizarPix(request.ChavePix, request.NomeRecebedor, request.CidadeRecebedor);
        _repositorio.Atualizar(config);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return ObterConfiguracaoManipulador.ConverterParaDto(config);
    }
}
