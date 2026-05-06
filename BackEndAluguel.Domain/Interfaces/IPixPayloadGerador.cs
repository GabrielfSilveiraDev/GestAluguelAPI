namespace BackEndAluguel.Domain.Interfaces;

/// <summary>
/// Contrato para geração de payload PIX no padrão EMV (copia-e-cola) do Banco Central do Brasil.
/// Não requer nenhuma API externa — o código é gerado localmente.
/// </summary>
public interface IPixPayloadGerador
{
    /// <summary>
    /// Gera o código PIX copia-e-cola conforme especificação EMV do BACEN.
    /// </summary>
    /// <param name="chavePix">Chave PIX do recebedor (CPF, CNPJ, e-mail, telefone ou chave aleatória).</param>
    /// <param name="nomeRecebedor">Nome do recebedor (máx. 25 caracteres).</param>
    /// <param name="cidadeRecebedor">Cidade do recebedor (máx. 15 caracteres).</param>
    /// <param name="valor">Valor da transação. Se nulo, gera PIX sem valor fixo.</param>
    /// <param name="txId">Identificador da transação (máx. 25 chars, apenas alfanumérico). Se nulo, usa "***".</param>
    /// <returns>String do payload PIX pronto para copiar e pagar.</returns>
    string Gerar(string chavePix, string nomeRecebedor, string cidadeRecebedor,
                 decimal? valor = null, string? txId = null);
}

