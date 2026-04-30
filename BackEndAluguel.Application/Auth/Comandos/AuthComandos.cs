using BackEndAluguel.Application.Auth.DTOs;
using MediatR;

namespace BackEndAluguel.Application.Auth.Comandos;

/// <summary>
/// Comando CQRS para autenticacao do administrador (host/locador).
/// Valida e-mail e senha contra o banco de dados.
/// </summary>
/// <param name="Email">E-mail cadastrado do host.</param>
/// <param name="Senha">Senha em texto plano (verificada via hash PBKDF2).</param>
public record LoginHostComando(
    string Email,
    string Senha
) : IRequest<TokenResultadoDto>;

/// <summary>
/// Comando CQRS para registro de um novo host/locador na plataforma.
/// Cria a conta com e-mail pendente de confirmação e envia o link por e-mail.
/// </summary>
/// <param name="NomeCompleto">Nome completo do host.</param>
/// <param name="Cpf">CPF do host (com ou sem formatação).</param>
/// <param name="DataNascimento">Data de nascimento do host.</param>
/// <param name="Email">E-mail do host (utilizado como login).</param>
/// <param name="Senha">Senha em texto plano (será criptografada antes de armazenar).</param>
public record RegistrarHostComando(
    string NomeCompleto,
    string Cpf,
    DateOnly DataNascimento,
    string Email,
    string Senha
) : IRequest<HostDto>;

/// <summary>
/// Comando CQRS para confirmação de e-mail do host.
/// Recebe o token enviado por e-mail e ativa a conta.
/// </summary>
/// <param name="Token">Token único recebido no e-mail de confirmação.</param>
public record ConfirmarEmailHostComando(
    string Token
) : IRequest<bool>;

/// <summary>
/// Comando CQRS para autenticacao de um inquilino.
/// Utiliza CPF + data de nascimento como credenciais (segundo fator de identidade).
/// </summary>
/// <param name="Cpf">CPF do inquilino (com ou sem formatacao).</param>
/// <param name="DataNascimento">Data de nascimento do inquilino.</param>
public record LoginInquilinoComando(
    string Cpf,
    DateOnly DataNascimento
) : IRequest<TokenResultadoDto>;


