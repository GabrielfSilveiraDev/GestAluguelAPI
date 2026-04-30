using BackEndAluguel.Domain.Enumeradores;

namespace BackEndAluguel.Domain.Entidades;

/// <summary>
/// Representa um dependente vinculado a um inquilino.
/// </summary>
public class Dependente : EntidadeBase
{
    public string NomeCompleto { get; private set; } = string.Empty;
    public string Cpf { get; private set; } = string.Empty;
    public string Rg { get; private set; } = string.Empty;
    public string OrgaoEmissor { get; private set; } = string.Empty;
    public DateOnly DataNascimento { get; private set; }
    public string Telefone { get; private set; } = string.Empty;
    public EstadoCivil EstadoCivil { get; private set; }
    public Guid InquilinoId { get; private set; }
    public Inquilino? Inquilino { get; private set; }

    protected Dependente() { }

    public Dependente(
        string nomeCompleto,
        string cpf,
        string rg,
        string orgaoEmissor,
        DateOnly dataNascimento,
        string telefone,
        EstadoCivil estadoCivil,
        Guid inquilinoId)
    {
        ValidarNome(nomeCompleto);
        ValidarCpf(cpf);
        ValidarRg(rg);
        ValidarOrgaoEmissor(orgaoEmissor);
        ValidarTelefone(telefone);

        NomeCompleto = nomeCompleto.Trim();
        Cpf = LimparCpf(cpf);
        Rg = rg.Trim();
        OrgaoEmissor = orgaoEmissor.Trim();
        DataNascimento = dataNascimento;
        Telefone = telefone.Trim();
        EstadoCivil = estadoCivil;
        InquilinoId = inquilinoId;
    }

    public void Atualizar(
        string nomeCompleto,
        string rg,
        string orgaoEmissor,
        DateOnly dataNascimento,
        string telefone,
        EstadoCivil estadoCivil)
    {
        ValidarNome(nomeCompleto);
        ValidarRg(rg);
        ValidarOrgaoEmissor(orgaoEmissor);
        ValidarTelefone(telefone);

        NomeCompleto = nomeCompleto.Trim();
        Rg = rg.Trim();
        OrgaoEmissor = orgaoEmissor.Trim();
        DataNascimento = dataNascimento;
        Telefone = telefone.Trim();
        EstadoCivil = estadoCivil;
        MarcarComoAtualizado();
    }

    private static string LimparCpf(string cpf)
        => new(cpf.Where(char.IsDigit).ToArray());

    private static void ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome completo do dependente nao pode ser vazio.", nameof(nome));
    }

    private static void ValidarCpf(string cpf)
    {
        var digitos = new string(cpf.Where(char.IsDigit).ToArray());
        if (digitos.Length != 11)
            throw new ArgumentException("O CPF do dependente deve conter 11 digitos numericos.", nameof(cpf));
    }

    private static void ValidarRg(string rg)
    {
        if (string.IsNullOrWhiteSpace(rg))
            throw new ArgumentException("O RG do dependente e obrigatorio.", nameof(rg));
    }

    private static void ValidarOrgaoEmissor(string orgaoEmissor)
    {
        if (string.IsNullOrWhiteSpace(orgaoEmissor))
            throw new ArgumentException("O orgao emissor do RG do dependente e obrigatorio.", nameof(orgaoEmissor));
    }

    private static void ValidarTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone))
            throw new ArgumentException("O telefone do dependente e obrigatorio.", nameof(telefone));
    }
}

