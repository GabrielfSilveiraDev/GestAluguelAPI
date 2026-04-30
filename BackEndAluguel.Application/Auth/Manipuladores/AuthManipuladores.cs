using BackEndAluguel.Application.Auth.Comandos;
using BackEndAluguel.Application.Auth.DTOs;
using BackEndAluguel.Application.Comum.Excecoes;
using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using MediatR;

namespace BackEndAluguel.Application.Auth.Manipuladores;

/// <summary>
/// Manipulador CQRS para autenticacao do administrador (host/locador).
/// Valida e-mail e senha contra o banco de dados e retorna um token JWT.
/// </summary>
public class LoginHostManipulador : IRequestHandler<LoginHostComando, TokenResultadoDto>
{
    private readonly IHostRepositorio _hostRepositorio;
    private readonly ISenhaServico _senhaServico;
    private readonly IJwtServico _jwtServico;

    public LoginHostManipulador(
        IHostRepositorio hostRepositorio,
        ISenhaServico senhaServico,
        IJwtServico jwtServico)
    {
        _hostRepositorio = hostRepositorio;
        _senhaServico = senhaServico;
        _jwtServico = jwtServico;
    }

    /// <summary>
    /// Autentica o host pelo e-mail e senha.
    /// Retorna token JWT apenas se o e-mail estiver confirmado.
    /// </summary>
    public async Task<TokenResultadoDto> Handle(LoginHostComando request, CancellationToken cancellationToken)
    {
        var emailLimpo = request.Email.Trim().ToLowerInvariant();
        var host = await _hostRepositorio.ObterPorEmailAsync(emailLimpo, cancellationToken);

        if (host is null || !_senhaServico.VerificarSenha(request.Senha, host.SenhaHash))
            throw new RegraDeNegocioExcecao("E-mail ou senha inválidos.");

        if (!host.EmailConfirmado)
            throw new RegraDeNegocioExcecao("Conta não confirmada. Verifique seu e-mail e clique no link de confirmação.");

        var (token, expiracao) = _jwtServico.GerarTokenHost(host.Id, host.NomeCompleto, host.Email);
        return new TokenResultadoDto(token, "Host", host.NomeCompleto, null, host.Id, expiracao);
    }
}

/// <summary>
/// Manipulador CQRS para registro de um novo host/locador.
/// Valida dados, cria a conta com senha criptografada, envia e-mail de confirmação.
/// </summary>
public class RegistrarHostManipulador : IRequestHandler<RegistrarHostComando, HostDto>
{
    private readonly IHostRepositorio _hostRepositorio;
    private readonly ISenhaServico _senhaServico;
    private readonly IEmailServico _emailServico;

    public RegistrarHostManipulador(
        IHostRepositorio hostRepositorio,
        ISenhaServico senhaServico,
        IEmailServico emailServico)
    {
        _hostRepositorio = hostRepositorio;
        _senhaServico = senhaServico;
        _emailServico = emailServico;
    }

    /// <summary>
    /// Registra um novo host: valida unicidade de CPF/e-mail, criptografa a senha,
    /// persiste no banco e envia o e-mail de confirmação.
    /// </summary>
    public async Task<HostDto> Handle(RegistrarHostComando request, CancellationToken cancellationToken)
    {
        // Validações de unicidade
        var emailLimpo = request.Email.Trim().ToLowerInvariant();
        var cpfLimpo = new string(request.Cpf.Where(char.IsDigit).ToArray());

        if (await _hostRepositorio.ExisteEmailAsync(emailLimpo, cancellationToken))
            throw new RegraDeNegocioExcecao("Já existe uma conta cadastrada com este e-mail.");

        if (await _hostRepositorio.ExisteCpfAsync(cpfLimpo, cancellationToken))
            throw new RegraDeNegocioExcecao("Já existe uma conta cadastrada com este CPF.");

        // Validação mínima de senha
        if (string.IsNullOrWhiteSpace(request.Senha) || request.Senha.Length < 6)
            throw new RegraDeNegocioExcecao("A senha deve ter no mínimo 6 caracteres.");

        // Cria o host com senha criptografada
        var senhaHash = _senhaServico.HashearSenha(request.Senha);
        var host = new Host(request.NomeCompleto, cpfLimpo, request.DataNascimento, emailLimpo, senhaHash);

        await _hostRepositorio.AdicionarAsync(host, cancellationToken);
        await _hostRepositorio.SalvarAlteracoesAsync(cancellationToken);

        // Envia e-mail de confirmação (não bloqueia o registro em caso de falha de envio)
        try
        {
            await _emailServico.EnviarConfirmacaoContaAsync(
                host.Email, host.NomeCompleto, host.TokenConfirmacao!, cancellationToken);
        }
        catch
        {
            // Falha no envio de e-mail não deve impedir o registro
            // O usuário pode solicitar reenvio futuramente
        }

        return ConverterParaDto(host);
    }

    private static HostDto ConverterParaDto(Host host) => new(
        host.Id,
        host.NomeCompleto,
        host.Cpf,
        host.DataNascimento,
        host.Email,
        host.EmailConfirmado,
        host.CriadoEm
    );
}

/// <summary>
/// Manipulador CQRS para confirmação de e-mail do host.
/// Valida o token e ativa a conta se for válido e não expirado.
/// </summary>
public class ConfirmarEmailHostManipulador : IRequestHandler<ConfirmarEmailHostComando, bool>
{
    private readonly IHostRepositorio _hostRepositorio;

    public ConfirmarEmailHostManipulador(IHostRepositorio hostRepositorio)
    {
        _hostRepositorio = hostRepositorio;
    }

    /// <summary>
    /// Valida o token de confirmação e ativa a conta do host.
    /// </summary>
    public async Task<bool> Handle(ConfirmarEmailHostComando request, CancellationToken cancellationToken)
    {
        var host = await _hostRepositorio.ObterPorTokenConfirmacaoAsync(request.Token, cancellationToken);

        if (host is null)
            throw new RegraDeNegocioExcecao("Token de confirmação inválido ou já utilizado.");

        if (!host.TokenConfirmacaoValido(request.Token))
            throw new RegraDeNegocioExcecao("Token de confirmação expirado. Solicite um novo e-mail de confirmação.");

        host.ConfirmarEmail();
        _hostRepositorio.Atualizar(host);
        await _hostRepositorio.SalvarAlteracoesAsync(cancellationToken);

        return true;
    }
}

/// <summary>
/// Manipulador CQRS para autenticacao de inquilinos via CPF + data de nascimento.
/// </summary>
public class LoginInquilinoManipulador : IRequestHandler<LoginInquilinoComando, TokenResultadoDto>
{
    private readonly IInquilinoRepositorio _inquilinoRepositorio;
    private readonly IJwtServico _jwtServico;

    /// <summary>Inicializa o manipulador com o repositorio de inquilinos e o servico JWT.</summary>
    public LoginInquilinoManipulador(IInquilinoRepositorio inquilinoRepositorio, IJwtServico jwtServico)
    {
        _inquilinoRepositorio = inquilinoRepositorio;
        _jwtServico = jwtServico;
    }

    /// <summary>
    /// Autentica o inquilino pelo CPF e data de nascimento.
    /// Retorna um token JWT com InquilinoId em caso de sucesso.
    /// </summary>
    public async Task<TokenResultadoDto> Handle(LoginInquilinoComando request, CancellationToken cancellationToken)
    {
        var cpfLimpo = new string(request.Cpf.Where(char.IsDigit).ToArray());

        var inquilino = await _inquilinoRepositorio.ObterPorCpfEDataNascimentoAsync(
            cpfLimpo, request.DataNascimento, cancellationToken);

        if (inquilino is null)
            throw new RegraDeNegocioExcecao("CPF ou data de nascimento incorretos.");

        var (token, expiracao) = _jwtServico.GerarTokenInquilino(inquilino.Id, inquilino.NomeCompleto, inquilino.Cpf);
        return new TokenResultadoDto(token, "Inquilino", inquilino.NomeCompleto, inquilino.Id, null, expiracao);
    }
}


