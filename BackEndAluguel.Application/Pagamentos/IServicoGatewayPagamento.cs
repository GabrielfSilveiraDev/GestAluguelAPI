using BackEndAluguel.Application.Pagamentos.DTOs;

namespace BackEndAluguel.Application.Pagamentos;

/// <summary>
/// Interface que define o contrato do servico de gateway de pagamento.
/// Seguindo o principio de inversao de dependencia (DIP - SOLID), a camada Application
/// depende apenas desta abstracao; a implementacao concreta (Asaas) fica na Infrastructure.
/// </summary>
public interface IServicoGatewayPagamento
{
    /// <summary>
    /// Registra uma subconta (Host/Locador) na plataforma Asaas.
    /// O retorno contem o <c>walletId</c>, necessario para configurar o split de pagamentos PIX.
    /// </summary>
    /// <param name="dados">Dados do Host a ser cadastrado.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado com o walletId e informacoes da conta criada.</returns>
    Task<SubcontaResultadoDto> CriarSubcontaAsync(CriarSubcontaDto dados, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gera uma cobranca via PIX com split de pagamento para o walletId do Host.
    /// O Asaas cria a cobranca e direciona o valor automaticamente para a carteira do locador.
    /// O retorno contem o codigo PIX copia-e-cola e a URL do QR Code.
    /// </summary>
    /// <param name="dados">Dados da cobranca incluindo o walletId do Host para split.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Resultado com o codigo PIX e QR Code para pagamento.</returns>
    Task<CobrancaPixResultadoDto> CriarCobrancaPixAsync(CriarCobrancaPixDto dados, CancellationToken cancellationToken = default);
}

