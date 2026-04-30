using BackEndAluguel.Domain.Entidades;
using FluentAssertions;

namespace BackEndAluguel.Tests.Dominio;

/// <summary>
/// Testes unitários para a entidade <see cref="Apartamento"/>.
/// Valida as regras de negócio, criação, atualização e estado do apartamento.
/// </summary>
public class ApartamentoTestes
{
    // =====================================================
    // Testes de criação
    // =====================================================

    /// <summary>
    /// Verifica que um apartamento é criado corretamente com número e bloco válidos.
    /// </summary>
    [Fact]
    public void CriarApartamento_ComDadosValidos_DeveCriarComSucesso()
    {
        // Arrange & Act
        var apartamento = new Apartamento("101", "A");

        // Assert
        apartamento.Numero.Should().Be("101");
        apartamento.Bloco.Should().Be("A");
        apartamento.Ocupado.Should().BeFalse();
        apartamento.Id.Should().NotBeEmpty();
        apartamento.CriadoEm.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Verifica que o número e o bloco são normalizados para maiúsculas.
    /// </summary>
    [Fact]
    public void CriarApartamento_DeveNormalizarNumeroEBlocoParaMaiusculas()
    {
        // Arrange & Act
        var apartamento = new Apartamento("101a", "bloco b");

        // Assert
        apartamento.Numero.Should().Be("101A");
        apartamento.Bloco.Should().Be("BLOCO B");
    }

    /// <summary>
    /// Verifica que uma excecao e lancada ao criar apartamento com numero vazio.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CriarApartamento_ComNumeroVazio_DeveLancarExcecao(string? numero)
    {
        // Act
#pragma warning disable CS8604
        var acao = () => new Apartamento(numero, "A");
#pragma warning restore CS8604

        // Assert
        acao.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Verifica que bloco vazio ou nulo e aceito (Bloco e opcional).
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CriarApartamento_ComBlocoVazioOuNulo_DeveCriarComSucesso(string? bloco)
    {
        // Bloco e opcional — nao deve lancar excecao
        var acao = () => new Apartamento("101", bloco);
        acao.Should().NotThrow();
    }

    // =====================================================
    // Testes de estado de ocupação
    // =====================================================

    /// <summary>
    /// Verifica que o apartamento é marcado como ocupado corretamente.
    /// </summary>
    [Fact]
    public void MarcarComoOcupado_DeveAlterarStatusParaOcupado()
    {
        // Arrange
        var apartamento = new Apartamento("101", "A");

        // Act
        apartamento.MarcarComoOcupado();

        // Assert
        apartamento.Ocupado.Should().BeTrue();
        apartamento.AtualizadoEm.Should().NotBeNull();
    }

    /// <summary>
    /// Verifica que o apartamento é marcado como desocupado corretamente.
    /// </summary>
    [Fact]
    public void MarcarComoDesocupado_DeveAlterarStatusParaDesocupado()
    {
        // Arrange
        var apartamento = new Apartamento("101", "A");
        apartamento.MarcarComoOcupado();

        // Act
        apartamento.MarcarComoDesocupado();

        // Assert
        apartamento.Ocupado.Should().BeFalse();
    }

    // =====================================================
    // Testes de atualização
    // =====================================================

    /// <summary>
    /// Verifica que a atualização altera os dados e registra a data de atualização.
    /// </summary>
    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarPropriedades()
    {
        // Arrange
        var apartamento = new Apartamento("101", "A");

        // Act
        apartamento.Atualizar("202", "B");

        // Assert
        apartamento.Numero.Should().Be("202");
        apartamento.Bloco.Should().Be("B");
        apartamento.AtualizadoEm.Should().NotBeNull();
    }
}

