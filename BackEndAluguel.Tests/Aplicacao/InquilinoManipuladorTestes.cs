using BackEndAluguel.Application.Comum.Excecoes;
using BackEndAluguel.Application.Inquilinos.Comandos;
using BackEndAluguel.Application.Inquilinos.Manipuladores;
using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BackEndAluguel.Tests.Aplicacao;

/// <summary>
/// Testes unitários para os manipuladores CQRS de Inquilino.
/// Usa Moq para simular os repositórios e validar o comportamento dos handlers.
/// </summary>
public class InquilinoManipuladorTestes
{
    private readonly Mock<IInquilinoRepositorio> _inquilinoRepositorioMock;
    private readonly Mock<IApartamentoRepositorio> _apartamentoRepositorioMock;

    private static readonly Guid ApartamentoId = Guid.NewGuid();
    private static readonly DateOnly DataEntrada = new(2024, 1, 1);
    private static readonly DateOnly DataVencimento = new(2026, 12, 31);
    private static readonly DateOnly DataNascimento = new(1990, 5, 15);

    /// <summary>
    /// Inicializa os mocks antes de cada teste.
    /// </summary>
    public InquilinoManipuladorTestes()
    {
        _inquilinoRepositorioMock = new Mock<IInquilinoRepositorio>();
        _apartamentoRepositorioMock = new Mock<IApartamentoRepositorio>();
    }

    // =====================================================
    // Testes de CriarInquilinoManipulador
    // =====================================================

    /// <summary>
    /// Verifica que criar inquilino com dados válidos retorna DTO e marca o apartamento como ocupado.
    /// </summary>
    [Fact]
    public async Task CriarInquilino_DadosValidos_DeveCriarVincularApartamentoERetornarDto()
    {
        // Arrange
        var apartamento = new Apartamento("101", "A");

        _inquilinoRepositorioMock.Setup(r => r.ExistePorCpfAsync("12345678901", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _apartamentoRepositorioMock.Setup(r => r.ObterPorIdAsync(ApartamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apartamento);
        _inquilinoRepositorioMock.Setup(r => r.AdicionarAsync(It.IsAny<Inquilino>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _inquilinoRepositorioMock.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var manipulador = new CriarInquilinoManipulador(_inquilinoRepositorioMock.Object, _apartamentoRepositorioMock.Object);
        var comando = new CriarInquilinoComando("João da Silva", "12345678901", DataNascimento, "12345678", "SSP-SP", "11999990000", Domain.Enumeradores.EstadoCivil.Solteiro, 2, DataEntrada, DataVencimento, 1500m, ApartamentoId);

        // Act
        var resultado = await manipulador.Handle(comando, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.NomeCompleto.Should().Be("João da Silva");
        resultado.Cpf.Should().Be("12345678901");
        resultado.ValorAluguel.Should().Be(1500m);
        resultado.ApartamentoId.Should().Be(ApartamentoId);

        // Deve ter marcado o apartamento como ocupado
        apartamento.Ocupado.Should().BeTrue();
        _apartamentoRepositorioMock.Verify(r => r.Atualizar(apartamento), Times.Once);
    }

    /// <summary>
    /// Verifica que tentar cadastrar inquilino com CPF duplicado lança RegraDeNegocioExcecao.
    /// </summary>
    [Fact]
    public async Task CriarInquilino_CpfJaCadastrado_DeveLancarRegraDeNegocioExcecao()
    {
        // Arrange
        _inquilinoRepositorioMock.Setup(r => r.ExistePorCpfAsync("12345678901", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var manipulador = new CriarInquilinoManipulador(_inquilinoRepositorioMock.Object, _apartamentoRepositorioMock.Object);
        var comando = new CriarInquilinoComando("João", "12345678901", DataNascimento, "12345678", "SSP-SP", "11999990000", Domain.Enumeradores.EstadoCivil.Solteiro, 1, DataEntrada, DataVencimento, 1000m, ApartamentoId);

        // Act
        var acao = async () => await manipulador.Handle(comando, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<RegraDeNegocioExcecao>()
            .WithMessage("*CPF*");

        _inquilinoRepositorioMock.Verify(r => r.AdicionarAsync(It.IsAny<Inquilino>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Verifica que tentar criar inquilino com apartamento inexistente lança EntidadeNaoEncontradaExcecao.
    /// </summary>
    [Fact]
    public async Task CriarInquilino_ApartamentoNaoEncontrado_DeveLancarEntidadeNaoEncontradaExcecao()
    {
        // Arrange
        _inquilinoRepositorioMock.Setup(r => r.ExistePorCpfAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _apartamentoRepositorioMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Apartamento?)null);

        var manipulador = new CriarInquilinoManipulador(_inquilinoRepositorioMock.Object, _apartamentoRepositorioMock.Object);
        var comando = new CriarInquilinoComando("João", "12345678901", DataNascimento, "12345678", "SSP-SP", "11999990000", Domain.Enumeradores.EstadoCivil.Solteiro, 1, DataEntrada, DataVencimento, 1000m, Guid.NewGuid());

        // Act
        var acao = async () => await manipulador.Handle(comando, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<EntidadeNaoEncontradaExcecao>();
    }

    // =====================================================
    // Testes de RemoverInquilinoManipulador
    // =====================================================

    /// <summary>
    /// Verifica que remover inquilino existente libera o apartamento.
    /// </summary>
    [Fact]
    public async Task RemoverInquilino_InquilinoExistente_DeveRemoverELiberarApartamento()
    {
        // Arrange
        var apartamento = new Apartamento("101", "A");
        apartamento.MarcarComoOcupado();

        var inquilino = new Inquilino("João", "12345678901", 1, DataEntrada, DataVencimento, 1000m, apartamento.Id);

        _inquilinoRepositorioMock.Setup(r => r.ObterPorIdAsync(inquilino.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inquilino);
        _apartamentoRepositorioMock.Setup(r => r.ObterPorIdAsync(inquilino.ApartamentoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apartamento);
        _inquilinoRepositorioMock.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var manipulador = new RemoverInquilinoManipulador(_inquilinoRepositorioMock.Object, _apartamentoRepositorioMock.Object);
        var comando = new RemoverInquilinoComando(inquilino.Id);

        // Act
        var resultado = await manipulador.Handle(comando, CancellationToken.None);

        // Assert
        resultado.Should().BeTrue();
        apartamento.Ocupado.Should().BeFalse();
        _inquilinoRepositorioMock.Verify(r => r.Remover(inquilino), Times.Once);
        _apartamentoRepositorioMock.Verify(r => r.Atualizar(apartamento), Times.Once);
    }

    // =====================================================
    // Testes de AtualizarInquilinoManipulador
    // =====================================================

    /// <summary>
    /// Verifica que atualizar inquilino existente aplica e persiste as alterações.
    /// </summary>
    [Fact]
    public async Task AtualizarInquilino_InquilinoExistente_DeveAtualizarERetornarDto()
    {
        // Arrange
        var inquilino = new Inquilino("João", "12345678901", 1, DataEntrada, DataVencimento, 1000m, ApartamentoId);

        _inquilinoRepositorioMock.Setup(r => r.ObterPorIdAsync(inquilino.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inquilino);
        _inquilinoRepositorioMock.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var manipulador = new AtualizarInquilinoManipulador(_inquilinoRepositorioMock.Object);
        var comando = new AtualizarInquilinoComando(inquilino.Id, "Pedro Santos", 3, new DateOnly(2027, 6, 30), 1800m, "87654321", "SSP-RJ", "21988887777", Domain.Enumeradores.EstadoCivil.Casado);

        // Act
        var resultado = await manipulador.Handle(comando, CancellationToken.None);

        // Assert
        resultado.NomeCompleto.Should().Be("Pedro Santos");
        resultado.QuantidadeMoradores.Should().Be(3);
        resultado.ValorAluguel.Should().Be(1800m);
        _inquilinoRepositorioMock.Verify(r => r.Atualizar(inquilino), Times.Once);
    }
}

