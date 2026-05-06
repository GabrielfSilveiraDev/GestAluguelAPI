using BackEndAluguel.Application.Comum.Excecoes;
using BackEndAluguel.Application.Configuracoes.DTOs;
using BackEndAluguel.Application.Faturas.Comandos;
using BackEndAluguel.Application.Faturas.Consultas;
using BackEndAluguel.Application.Faturas.DTOs;
using BackEndAluguel.Application.Pagamentos;
using BackEndAluguel.Application.Pagamentos.DTOs;
using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Enumeradores;
using BackEndAluguel.Domain.Interfaces;
using MediatR;

namespace BackEndAluguel.Application.Faturas.Manipuladores;

/// <summary>
/// Manipulador CQRS responsável por processar o comando <see cref="CriarFaturaComando"/>.
/// Realiza as seguintes operacoes automaticas:
/// 1. Busca a Configuracao global para obter KwhValor e ValorAgua padrao.
/// 2. Busca a fatura do mes anterior para preencher KwMesAnterior automaticamente.
/// 3. Calcula ValorLuz = KwConsumidos * KwhValor se nao informado manualmente.
/// 4. Usa ValorAgua da Configuracao se nao informado manualmente.
/// </summary>
public class CriarFaturaManipulador : IRequestHandler<CriarFaturaComando, FaturaDto>
{
    private readonly IFaturaRepositorio _faturaRepositorio;
    private readonly IInquilinoRepositorio _inquilinoRepositorio;
    private readonly IConfiguracaoRepositorio _configuracaoRepositorio;

    /// <summary>Inicializa o manipulador com os repositorios necessarios.</summary>
    public CriarFaturaManipulador(
        IFaturaRepositorio faturaRepositorio,
        IInquilinoRepositorio inquilinoRepositorio,
        IConfiguracaoRepositorio configuracaoRepositorio)
    {
        _faturaRepositorio = faturaRepositorio;
        _inquilinoRepositorio = inquilinoRepositorio;
        _configuracaoRepositorio = configuracaoRepositorio;
    }

    /// <summary>Processa a criacao da fatura com preenchimento automatico de kWh e valores de configuracao.</summary>
    public async Task<FaturaDto> Handle(CriarFaturaComando request, CancellationToken cancellationToken)
    {
        var inquilino = await _inquilinoRepositorio.ObterPorIdAsync(request.InquilinoId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Inquilino), request.InquilinoId);

        var jaExiste = await _faturaRepositorio.ExisteParaMesReferenciaAsync(
            request.InquilinoId, request.MesReferencia, cancellationToken);

        if (jaExiste)
            throw new RegraDeNegocioExcecao(
                $"Ja existe uma fatura para o mes '{request.MesReferencia}' do inquilino '{inquilino.NomeCompleto}'.");

        // Busca a configuracao global para KwhValor e ValorAgua
        var config = await _configuracaoRepositorio.ObterConfiguracaoAsync(cancellationToken);

        // Preenchimento automatico do KwMesAnterior:
        // Busca a fatura do mes anterior e usa o KwAtual como KwMesAnterior do mes atual
        decimal? kwMesAnterior = request.KwMesAnteriorManual;
        if (!kwMesAnterior.HasValue)
        {
            var mesAnteriorRef = CalcularMesAnterior(request.MesReferencia);
            var faturaAnterior = await _faturaRepositorio.ObterPorInquilinoEMesAsync(
                request.InquilinoId, mesAnteriorRef, cancellationToken);
            kwMesAnterior = faturaAnterior?.KwAtual;
        }

        // Determina o valor do kWh: usa o da config se nao houver override
        var kwhValor = config?.KwhValor;

        // Calcula ValorLuz automaticamente se nao foi fornecido manualmente
        decimal valorLuz = 0m;
        if (request.ValorLuzManual.HasValue)
        {
            valorLuz = request.ValorLuzManual.Value;
        }
        else if (request.KwAtual.HasValue && kwMesAnterior.HasValue && kwhValor.HasValue && kwhValor.Value > 0)
        {
            var consumido = request.KwAtual.Value - kwMesAnterior.Value;
            if (consumido > 0)
                valorLuz = consumido * kwhValor.Value;
        }

        // Usa ValorAgua da config se nao foi fornecido manualmente
        decimal valorAgua = request.ValorAguaManual ?? config?.ValorAgua ?? 0m;

        // Usa Garagem do inquilino se nao foi fornecido manualmente
        decimal valorGaragem = request.ValorGaragem ?? inquilino.Garagem;

        var fatura = new Fatura(
            request.MesReferencia,
            request.ValorAluguel,
            valorAgua,
            valorLuz,
            request.DataLimitePagamento,
            request.InquilinoId,
            kwMesAnterior,
            request.KwAtual,
            kwhValor,
            request.CodigoPix,
            valorGaragem);

        await _faturaRepositorio.AdicionarAsync(fatura, cancellationToken);
        await _faturaRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return ConverterParaDto(fatura);
    }

    /// <summary>Calcula o mes de referencia anterior no formato MM/AAAA.</summary>
    private static string CalcularMesAnterior(string mesReferencia)
    {
        var partes = mesReferencia.Split('/');
        var mes = int.Parse(partes[0]);
        var ano = int.Parse(partes[1]);
        var anterior = new DateTime(ano, mes, 1).AddMonths(-1);
        return $"{anterior.Month:D2}/{anterior.Year}";
    }

    /// <summary>Converte uma entidade Fatura para o DTO de resposta com todos os campos de kWh.</summary>
    internal static FaturaDto ConverterParaDto(Fatura f)
    {
        // Calcula o status efetivo em tempo real:
        // se a data limite passou e não está paga, considera Atrasado
        // independentemente do valor persistido no banco (que é atualizado pelo background service a cada 24h)
        var statusEfetivo = f.EstaVencida() ? StatusFatura.Atrasado : f.Status;

        var statusDescricao = statusEfetivo switch
        {
            StatusFatura.Pendente => "Pendente",
            StatusFatura.Atrasado => "Atrasado",
            StatusFatura.Pago => "Pago",
            _ => "Desconhecido"
        };

        return new FaturaDto(
            f.Id, f.MesReferencia, f.ValorAluguel, f.ValorAgua, f.ValorLuz, f.ValorGaragem,
            f.CalcularValorTotal(), f.DataLimitePagamento, f.DataPagamento,
            f.CodigoPix, statusEfetivo, statusDescricao, f.InquilinoId, f.CriadoEm,
            f.KwMesAnterior, f.KwAtual, f.KwConsumidos, f.KwhValor, f.CobrancaAsaasId,
            ApartamentoId: f.Inquilino?.ApartamentoId,
            NumeroApartamento: f.Inquilino?.Apartamento?.Numero,
            BlocoApartamento: string.IsNullOrWhiteSpace(f.Inquilino?.Apartamento?.Bloco)
                ? null : f.Inquilino!.Apartamento!.Bloco);
    }
}

/// <summary>
/// Manipulador CQRS para o comando <see cref="RegistrarPagamentoComando"/>.
/// </summary>
public class RegistrarPagamentoManipulador : IRequestHandler<RegistrarPagamentoComando, FaturaDto>
{
    private readonly IFaturaRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de faturas.
    /// </summary>
    public RegistrarPagamentoManipulador(IFaturaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa o registro de pagamento da fatura.
    /// </summary>
    public async Task<FaturaDto> Handle(RegistrarPagamentoComando request, CancellationToken cancellationToken)
    {
        var fatura = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Fatura), request.Id);

        fatura.RegistrarPagamento(request.DataPagamento);
        _repositorio.Atualizar(fatura);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return CriarFaturaManipulador.ConverterParaDto(fatura);
    }
}

/// <summary>
/// Manipulador CQRS para o comando <see cref="AtualizarValoresConsumoComando"/>.
/// </summary>
public class AtualizarValoresConsumoManipulador : IRequestHandler<AtualizarValoresConsumoComando, FaturaDto>
{
    private readonly IFaturaRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de faturas.
    /// </summary>
    public AtualizarValoresConsumoManipulador(IFaturaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa a atualização dos valores de água e luz de uma fatura não paga.
    /// </summary>
    public async Task<FaturaDto> Handle(AtualizarValoresConsumoComando request, CancellationToken cancellationToken)
    {
        var fatura = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Fatura), request.Id);

        fatura.AtualizarValoresConsumo(request.ValorAgua, request.ValorLuz);
        _repositorio.Atualizar(fatura);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return CriarFaturaManipulador.ConverterParaDto(fatura);
    }
}

/// <summary>
/// Manipulador CQRS para o comando <see cref="AtualizarCodigoPixComando"/>.
/// </summary>
public class AtualizarCodigoPixManipulador : IRequestHandler<AtualizarCodigoPixComando, FaturaDto>
{
    private readonly IFaturaRepositorio _repositorio;

    /// <summary>Inicializa o manipulador com o repositório de faturas.</summary>
    public AtualizarCodigoPixManipulador(IFaturaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>Processa a atualização do código PIX da fatura.</summary>
    public async Task<FaturaDto> Handle(AtualizarCodigoPixComando request, CancellationToken cancellationToken)
    {
        var fatura = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Fatura), request.Id);

        fatura.AtualizarCodigoPix(request.CodigoPix);
        _repositorio.Atualizar(fatura);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return CriarFaturaManipulador.ConverterParaDto(fatura);
    }
}

/// <summary>
/// Manipulador CQRS para o comando <see cref="AtualizarLeituraKwComando"/>.
/// Atualiza a leitura do kWh atual e recalcula o ValorLuz se possivel.
/// </summary>
public class AtualizarLeituraKwManipulador : IRequestHandler<AtualizarLeituraKwComando, FaturaDto>
{
    private readonly IFaturaRepositorio _repositorio;

    /// <summary>Inicializa o manipulador com o repositorio de faturas.</summary>
    public AtualizarLeituraKwManipulador(IFaturaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>Processa a atualizacao da leitura de kWh da fatura.</summary>
    public async Task<FaturaDto> Handle(AtualizarLeituraKwComando request, CancellationToken cancellationToken)
    {
        var fatura = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Fatura), request.Id);

        fatura.AtualizarLeituraKw(request.KwAtual, request.KwhValorOverride);
        _repositorio.Atualizar(fatura);
        await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return CriarFaturaManipulador.ConverterParaDto(fatura);
    }
}

/// <summary>
/// Manipulador CQRS para o comando <see cref="ProcessarFaturasVencidasComando"/>.
/// Varre o sistema marcando como Atrasadas todas as faturas com data limite ultrapassada.
/// </summary>
public class ProcessarFaturasVencidasManipulador : IRequestHandler<ProcessarFaturasVencidasComando, int>
{
    private readonly IFaturaRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de faturas.
    /// </summary>
    public ProcessarFaturasVencidasManipulador(IFaturaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa todas as faturas vencidas, alterando o status para Atrasado.
    /// </summary>
    /// <returns>Quantidade de faturas marcadas como atrasadas.</returns>
    public async Task<int> Handle(ProcessarFaturasVencidasComando request, CancellationToken cancellationToken)
    {
        var faturasVencidas = await _repositorio.ObterFaturasVencidasAsync(cancellationToken);
        var lista = faturasVencidas.ToList();

        foreach (var fatura in lista)
        {
            fatura.MarcarComoAtrasado();
            _repositorio.Atualizar(fatura);
        }

        if (lista.Count > 0)
            await _repositorio.SalvarAlteracoesAsync(cancellationToken);

        return lista.Count;
    }
}

/// <summary>
/// Manipulador CQRS para a consulta <see cref="ObterFaturaPorIdConsulta"/>.
/// </summary>
public class ObterFaturaPorIdManipulador : IRequestHandler<ObterFaturaPorIdConsulta, FaturaDto?>
{
    private readonly IFaturaRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de faturas.
    /// </summary>
    public ObterFaturaPorIdManipulador(IFaturaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa a consulta por ID.
    /// </summary>
    public async Task<FaturaDto?> Handle(ObterFaturaPorIdConsulta request, CancellationToken cancellationToken)
    {
        var fatura = await _repositorio.ObterPorIdAsync(request.Id, cancellationToken);
        return fatura is null ? null : CriarFaturaManipulador.ConverterParaDto(fatura);
    }
}

/// <summary>
/// Manipulador CQRS para a consulta <see cref="ListarFaturasPorInquilinoConsulta"/>.
/// </summary>
public class ListarFaturasPorInquilinoManipulador : IRequestHandler<ListarFaturasPorInquilinoConsulta, IEnumerable<FaturaDto>>
{
    private readonly IFaturaRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de faturas.
    /// </summary>
    public ListarFaturasPorInquilinoManipulador(IFaturaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa a listagem de faturas por inquilino.
    /// </summary>
    public async Task<IEnumerable<FaturaDto>> Handle(ListarFaturasPorInquilinoConsulta request, CancellationToken cancellationToken)
    {
        var faturas = await _repositorio.ObterPorInquilinoAsync(request.InquilinoId, cancellationToken);
        return faturas.Select(CriarFaturaManipulador.ConverterParaDto);
    }
}

/// <summary>
/// Manipulador CQRS para a consulta <see cref="ListarFaturasPorStatusConsulta"/>.
/// </summary>
public class ListarFaturasPorStatusManipulador : IRequestHandler<ListarFaturasPorStatusConsulta, IEnumerable<FaturaDto>>
{
    private readonly IFaturaRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de faturas.
    /// </summary>
    public ListarFaturasPorStatusManipulador(IFaturaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa a listagem de faturas filtrada por status.
    /// </summary>
    public async Task<IEnumerable<FaturaDto>> Handle(ListarFaturasPorStatusConsulta request, CancellationToken cancellationToken)
    {
        var faturas = await _repositorio.ObterPorStatusAsync(request.Status, cancellationToken);
        return faturas.Select(CriarFaturaManipulador.ConverterParaDto);
    }
}

/// <summary>
/// Manipulador CQRS para a consulta <see cref="ListarFaturasVencidasConsulta"/>.
/// </summary>
public class ListarFaturasVencidasManipulador : IRequestHandler<ListarFaturasVencidasConsulta, IEnumerable<FaturaDto>>
{
    private readonly IFaturaRepositorio _repositorio;

    /// <summary>
    /// Inicializa o manipulador com o repositório de faturas.
    /// </summary>
    public ListarFaturasVencidasManipulador(IFaturaRepositorio repositorio)
    {
        _repositorio = repositorio;
    }

    /// <summary>
    /// Processa a listagem de todas as faturas vencidas.
    /// </summary>
    public async Task<IEnumerable<FaturaDto>> Handle(ListarFaturasVencidasConsulta request, CancellationToken cancellationToken)
    {
        var faturas = await _repositorio.ObterFaturasVencidasAsync(cancellationToken);
        return faturas.Select(CriarFaturaManipulador.ConverterParaDto);
    }
}

/// <summary>
/// Manipulador CQRS para o comando <see cref="GerarCobrancaPixComando"/>.
/// Fluxo:
/// 1. Busca a fatura pelo ID.
/// 2. Verifica que a fatura nao esta paga.
/// 3. Busca o inquilino (pagador) e a configuracao global (WalletId do host).
/// 4. Chama o gateway Asaas para criar a cobrança PIX com split.
/// 5. Persiste o CobrancaAsaasId e CodigoPix na fatura.
/// 6. Retorna o PixCopiaCola e QrCode para exibição no frontend.
/// </summary>
public class GerarCobrancaPixManipulador : IRequestHandler<GerarCobrancaPixComando, CobrancaPixResultadoDto>
{
    private readonly IFaturaRepositorio _faturaRepositorio;
    private readonly IInquilinoRepositorio _inquilinoRepositorio;
    private readonly IConfiguracaoRepositorio _configuracaoRepositorio;
    private readonly IServicoGatewayPagamento _gatewayPagamento;

    /// <summary>Inicializa o manipulador com os repositorios e o gateway de pagamento.</summary>
    public GerarCobrancaPixManipulador(
        IFaturaRepositorio faturaRepositorio,
        IInquilinoRepositorio inquilinoRepositorio,
        IConfiguracaoRepositorio configuracaoRepositorio,
        IServicoGatewayPagamento gatewayPagamento)
    {
        _faturaRepositorio = faturaRepositorio;
        _inquilinoRepositorio = inquilinoRepositorio;
        _configuracaoRepositorio = configuracaoRepositorio;
        _gatewayPagamento = gatewayPagamento;
    }

    /// <summary>Gera a cobrança PIX via Asaas e persiste o resultado na fatura.</summary>
    public async Task<CobrancaPixResultadoDto> Handle(GerarCobrancaPixComando request, CancellationToken cancellationToken)
    {
        var fatura = await _faturaRepositorio.ObterPorIdAsync(request.FaturaId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Fatura), request.FaturaId);

        if (fatura.Status == StatusFatura.Pago)
            throw new RegraDeNegocioExcecao("Nao e possivel gerar cobranca PIX para uma fatura ja paga.");

        var inquilino = await _inquilinoRepositorio.ObterPorIdAsync(fatura.InquilinoId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Inquilino), fatura.InquilinoId);

        var config = await _configuracaoRepositorio.ObterConfiguracaoAsync(cancellationToken);

        if (string.IsNullOrEmpty(config?.WalletIdAsaas))
            throw new RegraDeNegocioExcecao(
                "O WalletId do Asaas nao esta configurado. " +
                "Registre a subconta do locador via POST /api/configuracoes/asaas/subconta antes de gerar cobranças PIX.");

        var dados = new CriarCobrancaPixDto(
            FaturaId: fatura.Id,
            CpfInquilino: inquilino.Cpf,
            NomeInquilino: inquilino.NomeCompleto,
            Valor: fatura.CalcularValorTotal(),
            Descricao: $"Aluguel {fatura.MesReferencia} - {inquilino.NomeCompleto}",
            WalletIdHost: config.WalletIdAsaas,
            PercentualSplit: request.PercentualSplit);

        var resultado = await _gatewayPagamento.CriarCobrancaPixAsync(dados, cancellationToken);

        // Persiste o ID da cobrança e o copia-e-cola PIX na fatura
        fatura.RegistrarCobrancaAsaas(resultado.CobrancaId);
        if (!string.IsNullOrEmpty(resultado.PixCopiaCola))
            fatura.AtualizarCodigoPix(resultado.PixCopiaCola);

        _faturaRepositorio.Atualizar(fatura);
        await _faturaRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return resultado;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// WhatsApp & PIX Nativo
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Manipulador CQRS para <see cref="ObterLinkWhatsappConsulta"/>.
/// Gera o link wa.me com a mensagem padrão formatada e o código PIX embutido,
/// sem depender de nenhuma API externa — usa apenas wa.me (gratuito).
/// </summary>
public class ObterLinkWhatsappManipulador : IRequestHandler<ObterLinkWhatsappConsulta, WhatsAppLinkDto>
{
    private readonly IFaturaRepositorio _faturaRepositorio;
    private readonly IInquilinoRepositorio _inquilinoRepositorio;
    private readonly IConfiguracaoRepositorio _configuracaoRepositorio;
    private readonly IPixPayloadGerador _pixGerador;

    public ObterLinkWhatsappManipulador(
        IFaturaRepositorio faturaRepositorio,
        IInquilinoRepositorio inquilinoRepositorio,
        IConfiguracaoRepositorio configuracaoRepositorio,
        IPixPayloadGerador pixGerador)
    {
        _faturaRepositorio = faturaRepositorio;
        _inquilinoRepositorio = inquilinoRepositorio;
        _configuracaoRepositorio = configuracaoRepositorio;
        _pixGerador = pixGerador;
    }

    public async Task<WhatsAppLinkDto> Handle(ObterLinkWhatsappConsulta request, CancellationToken cancellationToken)
    {
        var fatura = await _faturaRepositorio.ObterPorIdAsync(request.FaturaId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Fatura), request.FaturaId);

        var inquilino = await _inquilinoRepositorio.ObterPorIdAsync(fatura.InquilinoId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Inquilino), fatura.InquilinoId);

        var config = await _configuracaoRepositorio.ObterConfiguracaoAsync(cancellationToken);

        // Gera PIX nativo se a chave estiver configurada
        string? codigoPix = null;
        if (config is not null &&
            !string.IsNullOrWhiteSpace(config.ChavePix) &&
            !string.IsNullOrWhiteSpace(config.NomeRecebedorPix) &&
            !string.IsNullOrWhiteSpace(config.CidadeRecebedorPix))
        {
            var txId = $"FAT{fatura.Id.ToString("N")[..20]}";
            codigoPix = _pixGerador.Gerar(
                config.ChavePix,
                config.NomeRecebedorPix,
                config.CidadeRecebedorPix,
                fatura.CalcularValorTotal(),
                txId);
        }
        // Se já houver um código PIX salvo na fatura, usa ele como fallback
        codigoPix ??= fatura.CodigoPix;

        // Formata a mensagem usando o template configurado
        var template = config?.MensagemPadraoWhatsapp
            ?? "Olá {inquilino}, segue sua fatura de {mesReferencia}.\nValor: R$ {valorTotal}\nVencimento: {dataVencimento}\nPIX: {codigoPix}";

        var mensagem = template
            .Replace("{inquilino}", inquilino.NomeCompleto)
            .Replace("{mesReferencia}", fatura.MesReferencia)
            .Replace("{valorTotal}", fatura.CalcularValorTotal().ToString("F2", System.Globalization.CultureInfo.GetCultureInfo("pt-BR")))
            .Replace("{dataVencimento}", fatura.DataLimitePagamento.ToString("dd/MM/yyyy"))
            .Replace("{codigoPix}", codigoPix ?? "(PIX não configurado)");

        // Número do inquilino — normaliza para formato whatsapp (somente dígitos, DDI 55 se necessário)
        var telefone = new string(inquilino.Telefone.Where(char.IsDigit).ToArray());
        if (!telefone.StartsWith("55") && telefone.Length <= 11)
            telefone = "55" + telefone;

        // Link wa.me com mensagem pré-preenchida
        var mensagemEncoded = Uri.EscapeDataString(mensagem);
        var link = $"https://wa.me/{telefone}?text={mensagemEncoded}";

        return new WhatsAppLinkDto(link, mensagem, codigoPix, inquilino.Telefone);
    }
}

/// <summary>
/// Manipulador CQRS para <see cref="GerarPixNativoConsulta"/>.
/// Gera apenas o payload PIX EMV (copia-e-cola) para uma fatura usando a ChavePix configurada.
/// </summary>
public class GerarPixNativoManipulador : IRequestHandler<GerarPixNativoConsulta, string>
{
    private readonly IFaturaRepositorio _faturaRepositorio;
    private readonly IConfiguracaoRepositorio _configuracaoRepositorio;
    private readonly IPixPayloadGerador _pixGerador;

    public GerarPixNativoManipulador(
        IFaturaRepositorio faturaRepositorio,
        IConfiguracaoRepositorio configuracaoRepositorio,
        IPixPayloadGerador pixGerador)
    {
        _faturaRepositorio = faturaRepositorio;
        _configuracaoRepositorio = configuracaoRepositorio;
        _pixGerador = pixGerador;
    }

    public async Task<string> Handle(GerarPixNativoConsulta request, CancellationToken cancellationToken)
    {
        var fatura = await _faturaRepositorio.ObterPorIdAsync(request.FaturaId, cancellationToken)
            ?? throw new EntidadeNaoEncontradaExcecao(nameof(Fatura), request.FaturaId);

        var config = await _configuracaoRepositorio.ObterConfiguracaoAsync(cancellationToken);

        if (config is null ||
            string.IsNullOrWhiteSpace(config.ChavePix) ||
            string.IsNullOrWhiteSpace(config.NomeRecebedorPix) ||
            string.IsNullOrWhiteSpace(config.CidadeRecebedorPix))
            throw new RegraDeNegocioExcecao(
                "Dados PIX nao configurados. Configure via PUT /api/configuracoes/pix antes de gerar o codigo.");

        var txId = $"FAT{fatura.Id.ToString("N")[..20]}";
        return _pixGerador.Gerar(
            config.ChavePix,
            config.NomeRecebedorPix,
            config.CidadeRecebedorPix,
            fatura.CalcularValorTotal(),
            txId);
    }
}

