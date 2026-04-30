using BackEndAluguel.Domain.Entidades;
using FluentAssertions;

namespace BackEndAluguel.Tests.Dominio;

/// <summary>
/// Testes unitários para a entidade <see cref="Inquilino"/>.
/// Valida criação, validações e regras de negócio do inquilino.
/// </summary>
public class InquilinoTestes
{
    // Dados auxiliares reutilizáveis nos testes
    private static readonly Guid ApartamentoId = Guid.NewGuid();
    private static readonly DateOnly DataEntrada = new(2024, 1, 1);
    private static readonly DateOnly DataVencimento = new(2025, 12, 31);

    /// <summary>
    /// Cria um inquilino válido para ser usado nos testes.
    /// </summary>
    private static Inquilino CriarInquilinoValido()
        => new("João da Silva", "12345678901", 2, DataEntrada, DataVencimento, 1500.00m, ApartamentoId);

    // =====================================================
    // Testes de criação
    // =====================================================

    /// <summary>
    /// Verifica que um inquilino é criado corretamente com dados válidos.
    /// </summary>
    [Fact]
    public void CriarInquilino_ComDadosValidos_DeveCriarComSucesso()
    {
        // Arrange & Act
        var inquilino = CriarInquilinoValido();

        // Assert
        inquilino.NomeCompleto.Should().Be("João da Silva");
        inquilino.Cpf.Should().Be("12345678901");
        inquilino.QuantidadeMoradores.Should().Be(2);
        inquilino.DataEntrada.Should().Be(DataEntrada);
        inquilino.DataVencimentoContrato.Should().Be(DataVencimento);
        inquilino.ValorAluguel.Should().Be(1500.00m);
        inquilino.ApartamentoId.Should().Be(ApartamentoId);
        inquilino.DiasAlertaVencimento.Should().Equal(new List<int> { 30, 60, 90 });
    }

    /// <summary>
    /// Verifica que o CPF com formatação é limpo para somente dígitos.
    /// </summary>
    [Fact]
    public void CriarInquilino_CpfComFormatacao_DeveLimparParaSomentedigitos()
    {
        // Arrange & Act
        var inquilino = new Inquilino("Maria", "123.456.789-01", 1, DataEntrada, DataVencimento, 1000m, ApartamentoId);

        // Assert
        inquilino.Cpf.Should().Be("12345678901");
    }

    /// <summary>
    /// Verifica que alertas personalizados são respeitados.
    /// </summary>
    [Fact]
    public void CriarInquilino_ComAlertasPersonalizados_DeveUsarAlertasInformados()
    {
        // Arrange
        var alertas = new List<int> { 15, 45 };

        // Act
        var inquilino = new Inquilino("Maria", "12345678901", 1, DataEntrada, DataVencimento, 1000m, ApartamentoId, default, alertas);

        // Assert
        inquilino.DiasAlertaVencimento.Should().Equal(alertas);
    }

    // =====================================================
    // Testes de validação
    // =====================================================

    /// <summary>
    /// Verifica que nome vazio lança exceção.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CriarInquilino_ComNomeVazio_DeveLancarExcecao(string nome)
    {
        var acao = () => new Inquilino(nome, "12345678901", 1, DataEntrada, DataVencimento, 1000m, ApartamentoId);
        acao.Should().Throw<ArgumentException>().WithMessage("*nome*");
    }

    /// <summary>
    /// Verifica que CPF com menos de 11 dígitos lança exceção.
    /// </summary>
    [Theory]
    [InlineData("1234")]
    [InlineData("123456789012")]
    public void CriarInquilino_ComCpfInvalido_DeveLancarExcecao(string cpf)
    {
        var acao = () => new Inquilino("João", cpf, 1, DataEntrada, DataVencimento, 1000m, ApartamentoId);
        acao.Should().Throw<ArgumentException>().WithMessage("*CPF*");
    }

    /// <summary>
    /// Verifica que quantidade de moradores zero lança exceção.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CriarInquilino_ComQuantidadeMoradoresInvalida_DeveLancarExcecao(int quantidade)
    {
        var acao = () => new Inquilino("João", "12345678901", quantidade, DataEntrada, DataVencimento, 1000m, ApartamentoId);
        acao.Should().Throw<ArgumentException>().WithMessage("*moradores*");
    }

    /// <summary>
    /// Verifica que valor de aluguel zero ou negativo lança exceção.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void CriarInquilino_ComValorAluguelInvalido_DeveLancarExcecao(decimal valor)
    {
        var acao = () => new Inquilino("João", "12345678901", 1, DataEntrada, DataVencimento, valor, ApartamentoId);
        acao.Should().Throw<ArgumentException>().WithMessage("*aluguel*");
    }

    /// <summary>
    /// Verifica que data de entrada posterior ao vencimento lança exceção.
    /// </summary>
    [Fact]
    public void CriarInquilino_DataEntradaPosteriorAoVencimento_DeveLancarExcecao()
    {
        var acao = () => new Inquilino("João", "12345678901", 1,
            new DateOnly(2025, 12, 31), new DateOnly(2024, 1, 1), 1000m, ApartamentoId);

        acao.Should().Throw<ArgumentException>().WithMessage("*data de entrada*");
    }

    // =====================================================
    // Testes de alertas de vencimento
    // =====================================================

    /// <summary>
    /// Verifica que ContratoVenceEm retorna verdadeiro quando dentro do prazo.
    /// </summary>
    [Fact]
    public void ContratoVenceEm_ContratoDentroDosPrazo_DeveRetornarVerdadeiro()
    {
        // Arrange — vence daqui a 20 dias
        var vencimento = DateOnly.FromDateTime(DateTime.Today.AddDays(20));
        var inquilino = new Inquilino("João", "12345678901", 1,
            DateOnly.FromDateTime(DateTime.Today.AddDays(-30)), vencimento, 1000m, ApartamentoId);

        // Act & Assert
        inquilino.ContratoVenceEm(30).Should().BeTrue();
        inquilino.ContratoVenceEm(10).Should().BeFalse();
    }

    // =====================================================
    // Testes de atualização
    // =====================================================

    /// <summary>
    /// Verifica que a atualização aplica os novos dados corretamente.
    /// </summary>
    [Fact]
    public void Atualizar_ComDadosValidos_DeveAtualizarPropriedades()
    {
        // Arrange
        var inquilino = CriarInquilinoValido();

        // Act
        inquilino.Atualizar("Pedro dos Santos", 3, new DateOnly(2026, 6, 30), 1800m, new List<int> { 15, 30 });

        // Assert
        inquilino.NomeCompleto.Should().Be("Pedro dos Santos");
        inquilino.QuantidadeMoradores.Should().Be(3);
        inquilino.ValorAluguel.Should().Be(1800m);
        inquilino.DiasAlertaVencimento.Should().Equal(new List<int> { 15, 30 });
        inquilino.AtualizadoEm.Should().NotBeNull();
    }
}

