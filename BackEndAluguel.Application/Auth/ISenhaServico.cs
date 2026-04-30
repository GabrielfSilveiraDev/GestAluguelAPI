namespace BackEndAluguel.Application.Auth;

/// <summary>
/// Contrato para hashing e verificação de senhas.
/// A implementação concreta fica na camada Infrastructure (PBKDF2-SHA256).
/// </summary>
public interface ISenhaServico
{
    /// <summary>
    /// Gera um hash seguro da senha em texto plano.
    /// </summary>
    /// <param name="senhaPlana">Senha em texto plano fornecida pelo usuário.</param>
    /// <returns>Hash da senha (base64, inclui salt).</returns>
    string HashearSenha(string senhaPlana);

    /// <summary>
    /// Verifica se a senha em texto plano corresponde ao hash armazenado.
    /// </summary>
    /// <param name="senhaPlana">Senha em texto plano a verificar.</param>
    /// <param name="hashArmazenado">Hash armazenado no banco de dados.</param>
    /// <returns>True se a senha for correta.</returns>
    bool VerificarSenha(string senhaPlana, string hashArmazenado);
}

