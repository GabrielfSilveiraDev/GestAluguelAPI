using System.Text;
using BackEndAluguel.Domain.Interfaces;

namespace BackEndAluguel.Infrastructure.Servicos;

/// <summary>
/// Implementação do gerador de payload PIX no padrão EMV do Banco Central do Brasil.
/// Gera o código "copia-e-cola" sem depender de qualquer API externa.
///
/// Referência: https://www.bcb.gov.br/content/estabilidadefinanceira/pix/Regulamento_Pix/II-ManualdePadroesparaIniciacaodoPix-versao3.pdf
/// </summary>
public class PixPayloadGerador : IPixPayloadGerador
{
    /// <summary>
    /// Gera o payload PIX EMV completo com CRC16-CCITT.
    /// </summary>
    public string Gerar(string chavePix, string nomeRecebedor, string cidadeRecebedor,
                        decimal? valor = null, string? txId = null)
    {
        // Normaliza campos
        nomeRecebedor = Normalizar(nomeRecebedor, 25);
        cidadeRecebedor = Normalizar(cidadeRecebedor, 15);
        txId = string.IsNullOrWhiteSpace(txId) ? "***" : NormalizarTxId(txId, 25);

        // Merchant Account Info (ID 26)
        var merchantAccountInfo = MontarCampo("00", "BR.GOV.BCB.PIX") +
                                  MontarCampo("01", chavePix.Trim());
        var campoMerchant = MontarCampo("26", merchantAccountInfo);

        // Additional Data Field Template (ID 62) → txid (ID 05)
        var additionalData = MontarCampo("05", txId);
        var campo62 = MontarCampo("62", additionalData);

        // Monta payload sem CRC
        var payload = new StringBuilder();
        payload.Append(MontarCampo("00", "01"));       // Payload format indicator
        payload.Append(campoMerchant);                  // Merchant account info
        payload.Append(MontarCampo("52", "0000"));      // Merchant category code
        payload.Append(MontarCampo("53", "986"));       // Transaction currency (BRL)

        if (valor.HasValue && valor.Value > 0)
            payload.Append(MontarCampo("54", valor.Value.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)));

        payload.Append(MontarCampo("58", "BR"));        // Country code
        payload.Append(MontarCampo("59", nomeRecebedor));
        payload.Append(MontarCampo("60", cidadeRecebedor));
        payload.Append(campo62);                         // Additional data
        payload.Append("6304");                          // CRC placeholder (sem valor ainda)

        // Calcula e adiciona CRC16
        var crc = CalcularCrc16(payload.ToString());
        payload.Append(crc.ToString("X4"));

        return payload.ToString();
    }

    // ─── Helpers ────────────────────────────────────────────────────────────────

    private static string MontarCampo(string id, string valor)
    {
        var len = Encoding.UTF8.GetByteCount(valor);
        return $"{id}{len:D2}{valor}";
    }

    private static string Normalizar(string texto, int maxLen)
    {
        // Remove acentos e caracteres especiais (PIX aceita apenas ASCII imprimível)
        var normalized = texto.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
                != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        var result = sb.ToString().Normalize(NormalizationForm.FormC);
        // Mantém apenas ASCII imprimível
        result = new string(result.Where(c => c >= 0x20 && c <= 0x7E).ToArray());
        return result.Length > maxLen ? result[..maxLen] : result;
    }

    private static string NormalizarTxId(string txId, int maxLen)
    {
        var clean = new string(txId.Where(char.IsLetterOrDigit).ToArray());
        return clean.Length > maxLen ? clean[..maxLen] : clean;
    }

    /// <summary>
    /// CRC-16/CCITT-FALSE: poly=0x1021, init=0xFFFF, refin=false, refout=false, xorout=0x0000.
    /// </summary>
    private static ushort CalcularCrc16(string payload)
    {
        var bytes = Encoding.UTF8.GetBytes(payload);
        ushort crc = 0xFFFF;
        foreach (var b in bytes)
        {
            crc ^= (ushort)(b << 8);
            for (var i = 0; i < 8; i++)
            {
                if ((crc & 0x8000) != 0)
                    crc = (ushort)((crc << 1) ^ 0x1021);
                else
                    crc <<= 1;
            }
        }
        return crc;
    }
}

