using System.Security.Cryptography;
using BackEndAluguel.Application.Auth;

namespace BackEndAluguel.Infrastructure.Servicos;

/// <summary>
/// Implementação de hashing e verificação de senhas usando PBKDF2-SHA256.
/// Armazena o salt embutido no hash em Base64 — sem dependências externas.
/// Formato: Base64(salt[16] + hash[32]) = 48 bytes = 64 caracteres Base64.
/// </summary>
public class SenhaServico : ISenhaServico
{
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;
    private const int Iteracoes = 100_000;

    /// <summary>
    /// Gera um hash PBKDF2-SHA256 da senha em texto plano com salt aleatório.
    /// </summary>
    public string HashearSenha(string senhaPlana)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(senhaPlana, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        var resultado = new byte[TamanhoSalt + TamanhoHash];
        Buffer.BlockCopy(salt, 0, resultado, 0, TamanhoSalt);
        Buffer.BlockCopy(hash, 0, resultado, TamanhoSalt, TamanhoHash);

        return Convert.ToBase64String(resultado);
    }

    /// <summary>
    /// Verifica se a senha em texto plano corresponde ao hash armazenado.
    /// Usa comparação de tempo constante para evitar ataques de timing.
    /// </summary>
    public bool VerificarSenha(string senhaPlana, string hashArmazenado)
    {
        byte[] bytes;
        try { bytes = Convert.FromBase64String(hashArmazenado); }
        catch { return false; }

        if (bytes.Length != TamanhoSalt + TamanhoHash)
            return false;

        var salt = bytes[..TamanhoSalt];
        var hashEsperado = bytes[TamanhoSalt..];
        var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(senhaPlana, salt, Iteracoes, HashAlgorithmName.SHA256, TamanhoHash);

        return CryptographicOperations.FixedTimeEquals(hashEsperado, hashCalculado);
    }
}


