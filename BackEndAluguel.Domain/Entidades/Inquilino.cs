using BackEndAluguel.Domain.Enumeradores;

namespace BackEndAluguel.Domain.Entidades;

/// <summary>
/// Entidade que representa um inquilino (locatário) de um apartamento.
/// O inquilino é também um morador e, portanto, compartilha os mesmos campos
/// pessoais obrigatórios que os dependentes (RG, órgão emissor, telefone, estado civil).
///
/// Relacionamentos:
/// - N Inquilinos para 1 Apartamento (chave estrangeira ApartamentoId).
/// - 1 Inquilino para N Faturas: cada inquilino pode ter múltiplas faturas mensais geradas.
/// </summary>
public class Inquilino : EntidadeBase
{
    /// <summary>Nome completo do inquilino.</summary>
    public string NomeCompleto { get; private set; } = string.Empty;

    /// <summary>CPF do inquilino, armazenado somente com dígitos (11 caracteres).</summary>
    public string Cpf { get; private set; } = string.Empty;

    /// <summary>RG do inquilino.</summary>
    public string Rg { get; private set; } = string.Empty;

    /// <summary>Órgão emissor do RG (ex: SSP-SP).</summary>
    public string OrgaoEmissor { get; private set; } = string.Empty;

    /// <summary>
    /// Data de nascimento do inquilino.
    /// Utilizada como segundo fator de autenticação no login do inquilino.
    /// </summary>
    public DateOnly DataNascimento { get; private set; }

    /// <summary>Telefone de contato do inquilino.</summary>
    public string Telefone { get; private set; } = string.Empty;

    /// <summary>Estado civil do inquilino.</summary>
    public EstadoCivil EstadoCivil { get; private set; }

    /// <summary>
    /// Quantidade total de moradores que residirão no apartamento,
    /// incluindo o próprio inquilino.
    /// </summary>
    public int QuantidadeMoradores { get; private set; }

    /// <summary>
    /// Data em que o inquilino iniciou a ocupação do apartamento.
    /// </summary>
    public DateOnly DataEntrada { get; private set; }

    /// <summary>
    /// Data de vencimento do contrato de locação.
    /// </summary>
    public DateOnly DataVencimentoContrato { get; private set; }

    /// <summary>
    /// Valor fixo mensal do aluguel definido em contrato (em reais).
    /// </summary>
    public decimal ValorAluguel { get; private set; }

    /// <summary>
    /// Valor mensal cobrado pela vaga de garagem (0 se nao possui garagem).
    /// </summary>
    public decimal Garagem { get; private set; }

    /// <summary>
    /// Configuração dos alertas de vencimento de contrato em dias (ex: 30, 60, 90).
    /// Permite notificar o inquilino/administrador com antecedência sobre o fim do contrato.
    /// Armazenado como uma lista de inteiros representando os dias antes do vencimento.
    /// </summary>
    public List<int> DiasAlertaVencimento { get; private set; } = new();

    // =============================================================
    // RELACIONAMENTO: N Inquilinos -> 1 Apartamento
    // Vários inquilinos (histórico) podem estar vinculados a
    // um único apartamento. A chave estrangeira ApartamentoId
    // referencia a tabela Apartamentos.
    // =============================================================

    /// <summary>
    /// Identificador do apartamento ao qual este inquilino está vinculado.
    /// Chave estrangeira para a entidade Apartamento.
    /// </summary>
    public Guid ApartamentoId { get; private set; }

    /// <summary>
    /// Propriedade de navegação para o apartamento vinculado.
    /// Permite acessar os dados do apartamento diretamente através do inquilino.
    /// </summary>
    public Apartamento? Apartamento { get; private set; }

    // =============================================================
    // RELACIONAMENTO: 1 Inquilino -> N Faturas
    // Um inquilino pode ter uma fatura gerada por mês de referência.
    // O relacionamento é Um-para-Muitos (1:N).
    // =============================================================

    /// <summary>
    /// Coleção de faturas mensais geradas para este inquilino.
    /// Representa o relacionamento Um-para-Muitos (1:N) entre Inquilino e Fatura.
    /// </summary>
    public ICollection<Fatura> Faturas { get; private set; } = new List<Fatura>();

    /// <summary>
    /// Dependentes vinculados a este inquilino.
    /// </summary>
    public ICollection<Dependente> Dependentes { get; private set; } = new List<Dependente>();

    /// <summary>
    /// Construtor protegido requerido pelo Entity Framework Core.
    /// </summary>
    protected Inquilino() { }

    /// <summary>
    /// Cria um novo inquilino com dados básicos (sem RG, órgão emissor, telefone e estado civil).
    /// Útil para cenários simplificados e testes.
    /// </summary>
    public Inquilino(
        string nomeCompleto,
        string cpf,
        int quantidadeMoradores,
        DateOnly dataEntrada,
        DateOnly dataVencimentoContrato,
        decimal valorAluguel,
        Guid apartamentoId,
        DateOnly dataNascimento = default,
        List<int>? diasAlertaVencimento = null,
        decimal garagem = 0m)
    {
        ValidarNome(nomeCompleto);
        ValidarCpf(cpf);
        ValidarQuantidadeMoradores(quantidadeMoradores);
        ValidarValorAluguel(valorAluguel);
        ValidarDatas(dataEntrada, dataVencimentoContrato);

        NomeCompleto = nomeCompleto.Trim();
        Cpf = LimparCpf(cpf);
        QuantidadeMoradores = quantidadeMoradores;
        DataEntrada = dataEntrada;
        DataVencimentoContrato = dataVencimentoContrato;
        ValorAluguel = valorAluguel;
        ApartamentoId = apartamentoId;
        DataNascimento = dataNascimento;
        DiasAlertaVencimento = diasAlertaVencimento ?? new List<int> { 30, 60, 90 };
        Garagem = garagem < 0 ? 0 : garagem;
    }

    /// <summary>
    /// Cria um novo inquilino com todos os dados pessoais e contratuais necessários.
    /// Como todo inquilino é também um morador, os campos RG, órgão emissor, telefone
    /// e estado civil são obrigatórios, assim como nos dependentes.
    /// </summary>
    public Inquilino(
        string nomeCompleto,
        string cpf,
        int quantidadeMoradores,
        DateOnly dataEntrada,
        DateOnly dataVencimentoContrato,
        decimal valorAluguel,
        Guid apartamentoId,
        DateOnly dataNascimento,
        string rg,
        string orgaoEmissor,
        string telefone,
        EstadoCivil estadoCivil,
        List<int>? diasAlertaVencimento = null,
        decimal garagem = 0m)
    {
        ValidarNome(nomeCompleto);
        ValidarCpf(cpf);
        ValidarQuantidadeMoradores(quantidadeMoradores);
        ValidarValorAluguel(valorAluguel);
        ValidarDatas(dataEntrada, dataVencimentoContrato);
        ValidarRg(rg);
        ValidarOrgaoEmissor(orgaoEmissor);
        ValidarTelefone(telefone);

        NomeCompleto = nomeCompleto.Trim();
        Cpf = LimparCpf(cpf);
        QuantidadeMoradores = quantidadeMoradores;
        DataEntrada = dataEntrada;
        DataVencimentoContrato = dataVencimentoContrato;
        ValorAluguel = valorAluguel;
        ApartamentoId = apartamentoId;
        DataNascimento = dataNascimento;
        Rg = rg.Trim();
        OrgaoEmissor = orgaoEmissor.Trim();
        Telefone = telefone.Trim();
        EstadoCivil = estadoCivil;
        DiasAlertaVencimento = diasAlertaVencimento ?? new List<int> { 30, 60, 90 };
        Garagem = garagem < 0 ? 0 : garagem;
    }

    /// <summary>Atualiza as informações básicas do inquilino (sem RG, órgão emissor, telefone e estado civil).</summary>
    public void Atualizar(
        string nomeCompleto,
        int quantidadeMoradores,
        DateOnly dataVencimentoContrato,
        decimal valorAluguel,
        List<int>? diasAlertaVencimento = null,
        decimal garagem = 0m)
    {
        ValidarNome(nomeCompleto);
        ValidarQuantidadeMoradores(quantidadeMoradores);
        ValidarValorAluguel(valorAluguel);

        NomeCompleto = nomeCompleto.Trim();
        QuantidadeMoradores = quantidadeMoradores;
        DataVencimentoContrato = dataVencimentoContrato;
        ValorAluguel = valorAluguel;
        Garagem = garagem < 0 ? 0 : garagem;

        if (diasAlertaVencimento != null)
            DiasAlertaVencimento = diasAlertaVencimento;

        MarcarComoAtualizado();
    }

    /// <summary>Atualiza as informações do inquilino.</summary>
    public void Atualizar(
        string nomeCompleto,
        int quantidadeMoradores,
        DateOnly dataVencimentoContrato,
        decimal valorAluguel,
        string rg,
        string orgaoEmissor,
        string telefone,
        EstadoCivil estadoCivil,
        List<int>? diasAlertaVencimento = null,
        decimal garagem = 0m)
    {
        ValidarNome(nomeCompleto);
        ValidarQuantidadeMoradores(quantidadeMoradores);
        ValidarValorAluguel(valorAluguel);
        ValidarRg(rg);
        ValidarOrgaoEmissor(orgaoEmissor);
        ValidarTelefone(telefone);

        NomeCompleto = nomeCompleto.Trim();
        QuantidadeMoradores = quantidadeMoradores;
        DataVencimentoContrato = dataVencimentoContrato;
        ValorAluguel = valorAluguel;
        Rg = rg.Trim();
        OrgaoEmissor = orgaoEmissor.Trim();
        Telefone = telefone.Trim();
        EstadoCivil = estadoCivil;
        Garagem = garagem < 0 ? 0 : garagem;

        if (diasAlertaVencimento != null)
            DiasAlertaVencimento = diasAlertaVencimento;

        MarcarComoAtualizado();
    }

    /// <summary>
    /// Verifica se o contrato do inquilino vence dentro de um número específico de dias.
    /// Utilizado para disparar alertas de vencimento.
    /// </summary>
    /// <param name="diasAntecedencia">Quantidade de dias para verificar antecedência.</param>
    /// <returns>Verdadeiro se o contrato vence dentro dos dias especificados.</returns>
    public bool ContratoVenceEm(int diasAntecedencia)
    {
        var dataLimite = DateOnly.FromDateTime(DateTime.Today.AddDays(diasAntecedencia));
        return DataVencimentoContrato <= dataLimite && DataVencimentoContrato >= DateOnly.FromDateTime(DateTime.Today);
    }

    /// <summary>
    /// Verifica se algum alerta de vencimento está ativo com base na configuração de alertas.
    /// </summary>
    /// <returns>Lista de dias configurados onde o alerta está ativo no momento.</returns>
    public IEnumerable<int> ObterAlertasAtivos()
    {
        return DiasAlertaVencimento.Where(ContratoVenceEm);
    }

    /// <summary>
    /// Remove formatação do CPF, mantendo apenas os dígitos.
    /// </summary>
    private static string LimparCpf(string cpf)
        => new(cpf.Where(char.IsDigit).ToArray());

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome completo do inquilino não pode ser vazio.", nameof(nome));
    }

    private static void ValidarCpf(string cpf)
    {
        var digitos = new string(cpf.Where(char.IsDigit).ToArray());
        if (digitos.Length != 11)
            throw new ArgumentException("O CPF deve conter 11 dígitos numéricos.", nameof(cpf));
    }

    private static void ValidarRg(string rg)
    {
        if (string.IsNullOrWhiteSpace(rg))
            throw new ArgumentException("O RG do inquilino é obrigatório.", nameof(rg));
    }

    private static void ValidarOrgaoEmissor(string orgaoEmissor)
    {
        if (string.IsNullOrWhiteSpace(orgaoEmissor))
            throw new ArgumentException("O órgão emissor do RG é obrigatório.", nameof(orgaoEmissor));
    }

    private static void ValidarTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("O telefone do inquilino é obrigatório.", nameof(telefone));
    }

    private static void ValidarQuantidadeMoradores(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentException("A quantidade de moradores deve ser maior que zero.", nameof(quantidade));
    }

    private static void ValidarValorAluguel(decimal valor)
    {
        if (valor <= 0)
            throw new ArgumentException("O valor do aluguel deve ser maior que zero.", nameof(valor));
    }

    private static void ValidarDatas(DateOnly dataEntrada, DateOnly dataVencimento)
    {
        if (dataEntrada >= dataVencimento)
            throw new ArgumentException("A data de entrada deve ser anterior à data de vencimento do contrato.");
    }
}

