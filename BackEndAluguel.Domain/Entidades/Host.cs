namespace BackEndAluguel.Domain.Entidades;

/// <summary>
/// Representa um administrador (locador/host) da plataforma GestAluguel.
/// Cada host é um usuário com acesso total ao painel de gerenciamento.
/// A conta deve ser confirmada por e-mail antes de ser utilizada.
/// </summary>
public class Host : EntidadeBase
{
    // ─── Propriedades ──────────────────────────────────────────────────────

    /// <summary>Nome completo do host.</summary>
    public string NomeCompleto { get; private set; } = string.Empty;

    /// <summary>CPF do host (somente dígitos, 11 caracteres).</summary>
    public string Cpf { get; private set; } = string.Empty;

    /// <summary>Data de nascimento do host.</summary>
    public DateOnly DataNascimento { get; private set; }

    /// <summary>Endereço de e-mail do host. Utilizado como login.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Hash da senha do host gerado via PBKDF2-SHA256.</summary>
    public string SenhaHash { get; private set; } = string.Empty;

    /// <summary>Indica se o e-mail foi confirmado. A conta só pode ser usada após confirmação.</summary>
    public bool EmailConfirmado { get; private set; }

    /// <summary>Token único para confirmação de e-mail. Nulo após a confirmação.</summary>
    public string? TokenConfirmacao { get; private set; }

    /// <summary>Data de expiração do token de confirmação de e-mail.</summary>
    public DateTime? TokenExpiracaoConfirmacao { get; private set; }

    // ─── Construtor ────────────────────────────────────────────────────────

    /// <summary>
    /// Construtor protegido exigido pelo Entity Framework Core.
    /// Use o construtor público para criar instâncias desta entidade.
    /// </summary>
    protected Host() { }

    /// <summary>
    /// Cria um novo host com os dados fornecidos.
    /// A conta começa como não confirmada e recebe um token de confirmação de e-mail.
    /// </summary>
    /// <param name="nomeCompleto">Nome completo do host.</param>
    /// <param name="cpf">CPF (com ou sem formatação — será limpo automaticamente).</param>
    /// <param name="dataNascimento">Data de nascimento do host.</param>
    /// <param name="email">E-mail do host. Será armazenado em minúsculas.</param>
    /// <param name="senhaHash">Senha já criptografada via PBKDF2.</param>
    public Host(string nomeCompleto, string cpf, DateOnly dataNascimento, string email, string senhaHash)
    {
        ValidarNome(nomeCompleto);
        ValidarCpf(cpf);
        ValidarEmail(email);
        ValidarSenhaHash(senhaHash);

        NomeCompleto = nomeCompleto.Trim();
        Cpf = new string(cpf.Where(char.IsDigit).ToArray());
        DataNascimento = dataNascimento;
        Email = email.Trim().ToLowerInvariant();
        SenhaHash = senhaHash;
        EmailConfirmado = false;
        GerarTokenConfirmacao();
    }

    // ─── Métodos de domínio ────────────────────────────────────────────────

    /// <summary>
    /// Confirma o e-mail do host, limpando o token de confirmação.
    /// </summary>
    public void ConfirmarEmail()
    {
        EmailConfirmado = true;
        TokenConfirmacao = null;
        TokenExpiracaoConfirmacao = null;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Gera um novo token de confirmação de e-mail com expiração de 48 horas.
    /// Útil para reenvio de e-mail de confirmação.
    /// </summary>
    public void GerarTokenConfirmacao()
    {
        TokenConfirmacao = Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N");
        TokenExpiracaoConfirmacao = DateTime.UtcNow.AddHours(48);
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Verifica se o token de confirmação informado é válido e não expirou.
    /// </summary>
    public bool TokenConfirmacaoValido(string token)
        => TokenConfirmacao == token
           && TokenExpiracaoConfirmacao.HasValue
           && TokenExpiracaoConfirmacao.Value > DateTime.UtcNow;

    /// <summary>
    /// Atualiza os dados pessoais do host.
    /// </summary>
    public void Atualizar(string nomeCompleto, DateOnly dataNascimento)
    {
        ValidarNome(nomeCompleto);
        NomeCompleto = nomeCompleto.Trim();
        DataNascimento = dataNascimento;
        MarcarComoAtualizado();
    }

    /// <summary>
    /// Atualiza o hash da senha do host.
    /// </summary>
    public void AtualizarSenha(string novaSenhaHash)
    {
        ValidarSenhaHash(novaSenhaHash);
        SenhaHash = novaSenhaHash;
        MarcarComoAtualizado();
    }

    // ─── Validações privadas ───────────────────────────────────────────────

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome completo do host é obrigatório.", nameof(nome));
    }

    private static void ValidarCpf(string cpf)
    {
        var digitos = new string(cpf.Where(char.IsDigit).ToArray());
        if (digitos.Length != 11)
            throw new ArgumentException("O CPF deve conter exatamente 11 dígitos.", nameof(cpf));
    }

    private static void ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException("O e-mail informado é inválido.", nameof(email));
    }

    private static void ValidarSenhaHash(string senhaHash)
    {
        if (string.IsNullOrWhiteSpace(senhaHash))
            throw new ArgumentException("O hash da senha não pode ser vazio.", nameof(senhaHash));
    }
}

