using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Enumeradores;
using FluentAssertions;

namespace BackEndAluguel.Tests.Dominio;

/// <summary>
/// Testes unitários para a entidade <see cref="Fatura"/>.
/// Valida criação, cálculo de total, registro de pagamento e regras de status.
/// </summary>
public class FaturaTestes
{
    private static readonly Guid InquilinoId = Guid.NewGuid();

    /// <summary>
    /// Cria uma fatura válida para testes.
    /// </summary>
    private static Fatura CriarFaturaValida()
        => new("05/2025", 1500m, 80m, 120m, new DateOnly(2025, 5, 10), InquilinoId, codigoPix: "00020126580014br.gov.bcb.pix");

    // =====================================================
    // Testes de criação
    // =====================================================

    /// <summary>
    /// Verifica que uma fatura é criada corretamente com status Pendente.
    /// </summary>
    [Fact]
    public void CriarFatura_ComDadosValidos_DeveCriarComStatusPendente()
    {
        // Arrange & Act
        var fatura = CriarFaturaValida();

        // Assert
        fatura.MesReferencia.Should().Be("05/2025");
        fatura.ValorAluguel.Should().Be(1500m);
        fatura.ValorAgua.Should().Be(80m);
        fatura.ValorLuz.Should().Be(120m);
        fatura.Status.Should().Be(StatusFatura.Pendente);
        fatura.DataPagamento.Should().BeNull();
        fatura.InquilinoId.Should().Be(InquilinoId);
    }

    /// <summary>
    /// Verifica que o cálculo do valor total está correto.
    /// </summary>
    [Fact]
    public void CalcularValorTotal_DeveRetornarSomaDeTodosOsValores()
    {
        // Arrange
        var fatura = CriarFaturaValida();

        // Act
        var total = fatura.CalcularValorTotal();

        // Assert
        total.Should().Be(1700m); // 1500 + 80 + 120
    }

    // =====================================================
    // Testes de validações de criação
    // =====================================================

    /// <summary>
    /// Verifica que mês de referência em formato inválido lança exceção.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("2025-05")]
    [InlineData("5/2025")]
    [InlineData("13/2025")]
    public void CriarFatura_ComMesReferenciaInvalido_DeveLancarExcecao(string mesReferencia)
    {
        var acao = () => new Fatura(mesReferencia, 1000m, 0m, 0m, new DateOnly(2025, 5, 10), InquilinoId);
        acao.Should().Throw<ArgumentException>().WithMessage("*mes de referencia*");
    }

    /// <summary>
    /// Verifica que valor de aluguel zero lança exceção.
    /// </summary>
    [Fact]
    public void CriarFatura_ComValorAluguelZero_DeveLancarExcecao()
    {
        var acao = () => new Fatura("05/2025", 0m, 0m, 0m, new DateOnly(2025, 5, 10), InquilinoId);
        acao.Should().Throw<ArgumentException>().WithMessage("*aluguel*");
    }

    /// <summary>
    /// Verifica que valor negativo de água lança exceção.
    /// </summary>
    [Fact]
    public void CriarFatura_ComValorAguaNegativo_DeveLancarExcecao()
    {
        var acao = () => new Fatura("05/2025", 1000m, -10m, 0m, new DateOnly(2025, 5, 10), InquilinoId);
        acao.Should().Throw<ArgumentException>().WithMessage("*agua*");
    }

    // =====================================================
    // Testes de registro de pagamento
    // =====================================================

    /// <summary>
    /// Verifica que o registro de pagamento altera o status para Pago.
    /// </summary>
    [Fact]
    public void RegistrarPagamento_DeveAlterarStatusParaPago()
    {
        // Arrange
        var fatura = CriarFaturaValida();
        var dataPagamento = new DateOnly(2025, 5, 8);

        // Act
        fatura.RegistrarPagamento(dataPagamento);

        // Assert
        fatura.Status.Should().Be(StatusFatura.Pago);
        fatura.DataPagamento.Should().Be(dataPagamento);
        fatura.AtualizadoEm.Should().NotBeNull();
    }

    /// <summary>
    /// Verifica que tentar pagar uma fatura já paga lança exceção.
    /// </summary>
    [Fact]
    public void RegistrarPagamento_FaturaJaPaga_DeveLancarExcecao()
    {
        // Arrange
        var fatura = CriarFaturaValida();
        fatura.RegistrarPagamento(new DateOnly(2025, 5, 8));

        // Act
        var acao = () => fatura.RegistrarPagamento(new DateOnly(2025, 5, 9));

        // Assert
        acao.Should().Throw<InvalidOperationException>().WithMessage("*ja foi paga*");
    }

    // =====================================================
    // Testes de marcação de atraso
    // =====================================================

    /// <summary>
    /// Verifica que uma fatura pendente pode ser marcada como atrasada.
    /// </summary>
    [Fact]
    public void MarcarComoAtrasado_FaturaPendente_DeveAlterarStatusParaAtrasado()
    {
        // Arrange
        var fatura = CriarFaturaValida();

        // Act
        fatura.MarcarComoAtrasado();

        // Assert
        fatura.Status.Should().Be(StatusFatura.Atrasado);
    }

    /// <summary>
    /// Verifica que tentar marcar como atrasada uma fatura paga lança exceção.
    /// </summary>
    [Fact]
    public void MarcarComoAtrasado_FaturaJaPaga_DeveLancarExcecao()
    {
        // Arrange
        var fatura = CriarFaturaValida();
        fatura.RegistrarPagamento(new DateOnly(2025, 5, 8));

        // Act
        var acao = () => fatura.MarcarComoAtrasado();

        // Assert
        acao.Should().Throw<InvalidOperationException>().WithMessage("*paga*");
    }

    // =====================================================
    // Testes de verificação de vencimento
    // =====================================================

    /// <summary>
    /// Verifica que EstaVencida retorna verdadeiro para fatura com data passada e não paga.
    /// </summary>
    [Fact]
    public void EstaVencida_FaturaComDataPassadaNaoPaga_DeveRetornarVerdadeiro()
    {
        // Arrange — data limite no passado
        var fatura = new Fatura("01/2020", 1000m, 0m, 0m, new DateOnly(2020, 1, 10), InquilinoId);

        // Act & Assert
        fatura.EstaVencida().Should().BeTrue();
    }

    /// <summary>
    /// Verifica que EstaVencida retorna falso para fatura paga mesmo com data no passado.
    /// </summary>
    [Fact]
    public void EstaVencida_FaturaPaga_DeveRetornarFalso()
    {
        // Arrange
        var fatura = new Fatura("01/2020", 1000m, 0m, 0m, new DateOnly(2020, 1, 10), InquilinoId);
        fatura.RegistrarPagamento(new DateOnly(2020, 1, 8));

        // Act & Assert
        fatura.EstaVencida().Should().BeFalse();
    }

    // =====================================================
    // Testes de atualização de valores
    // =====================================================

    /// <summary>
    /// Verifica que é possível atualizar valores de consumo antes do pagamento.
    /// </summary>
    [Fact]
    public void AtualizarValoresConsumo_FaturaNaoPaga_DeveAtualizarCorretamente()
    {
        // Arrange
        var fatura = CriarFaturaValida();

        // Act
        fatura.AtualizarValoresConsumo(95m, 140m);

        // Assert
        fatura.ValorAgua.Should().Be(95m);
        fatura.ValorLuz.Should().Be(140m);
        fatura.CalcularValorTotal().Should().Be(1735m); // 1500 + 95 + 140
    }

    /// <summary>
    /// Verifica que não é possível atualizar valores de uma fatura já paga.
    /// </summary>
    [Fact]
    public void AtualizarValoresConsumo_FaturaJaPaga_DeveLancarExcecao()
    {
        // Arrange
        var fatura = CriarFaturaValida();
        fatura.RegistrarPagamento(new DateOnly(2025, 5, 8));

        // Act
        var acao = () => fatura.AtualizarValoresConsumo(95m, 140m);

        // Assert
        acao.Should().Throw<InvalidOperationException>().WithMessage("*paga*");
    }
}

