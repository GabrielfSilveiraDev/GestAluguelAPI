using BackEndAluguel.Application.Apartamentos.Comandos;
using BackEndAluguel.Application.Apartamentos.Manipuladores;
using BackEndAluguel.Application.Comum.Excecoes;
using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace BackEndAluguel.Tests.Aplicacao;

/// <summary>
/// Testes unitários para os manipuladores CQRS de Apartamento.
/// Usa Moq para simular o repositório e validar o comportamento dos handlers.
/// </summary>
public class ApartamentoManipuladorTestes
{
    private readonly Mock<IApartamentoRepositorio> _repositorioMock;

    /// <summary>
    /// Inicializa os mocks antes de cada teste.
    /// </summary>
    public ApartamentoManipuladorTestes()
    {
        _repositorioMock = new Mock<IApartamentoRepositorio>();
    }

    // =====================================================
    // Testes de CriarApartamentoManipulador
    // =====================================================

    /// <summary>
    /// Verifica que criar um apartamento com dados únicos retorna o DTO corretamente.
    /// </summary>
    [Fact]
    public async Task CriarApartamento_ApartamentoNaoExistente_DeveCriarERetornarDto()
    {
        // Arrange
        _repositorioMock.Setup(r => r.ExisteAsync("101", "A", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _repositorioMock.Setup(r => r.AdicionarAsync(It.IsAny<Apartamento>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var manipulador = new CriarApartamentoManipulador(_repositorioMock.Object);
        var comando = new CriarApartamentoComando("101", "A");

        // Act
        var resultado = await manipulador.Handle(comando, CancellationToken.None);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Numero.Should().Be("101");
        resultado.Bloco.Should().Be("A");
        resultado.Ocupado.Should().BeFalse();

        _repositorioMock.Verify(r => r.AdicionarAsync(It.IsAny<Apartamento>(), It.IsAny<CancellationToken>()), Times.Once);
        _repositorioMock.Verify(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Verifica que criar apartamento duplicado lança RegraDeNegocioExcecao.
    /// </summary>
    [Fact]
    public async Task CriarApartamento_ApartamentoJaExistente_DeveLancarRegraDeNegocioExcecao()
    {
        // Arrange
        _repositorioMock.Setup(r => r.ExisteAsync("101", "A", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var manipulador = new CriarApartamentoManipulador(_repositorioMock.Object);
        var comando = new CriarApartamentoComando("101", "A");

        // Act
        var acao = async () => await manipulador.Handle(comando, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<RegraDeNegocioExcecao>()
            .WithMessage("*101*");

        _repositorioMock.Verify(r => r.AdicionarAsync(It.IsAny<Apartamento>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // =====================================================
    // Testes de AtualizarApartamentoManipulador
    // =====================================================

    /// <summary>
    /// Verifica que atualizar apartamento existente retorna DTO atualizado.
    /// </summary>
    [Fact]
    public async Task AtualizarApartamento_ApartamentoExistente_DeveAtualizarERetornarDto()
    {
        // Arrange
        var apartamentoExistente = new Apartamento("101", "A");
        _repositorioMock.Setup(r => r.ObterPorIdAsync(apartamentoExistente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apartamentoExistente);
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var manipulador = new AtualizarApartamentoManipulador(_repositorioMock.Object);
        var comando = new AtualizarApartamentoComando(apartamentoExistente.Id, "202", "B");

        // Act
        var resultado = await manipulador.Handle(comando, CancellationToken.None);

        // Assert
        resultado.Numero.Should().Be("202");
        resultado.Bloco.Should().Be("B");
    }

    /// <summary>
    /// Verifica que atualizar apartamento não encontrado lança EntidadeNaoEncontradaExcecao.
    /// </summary>
    [Fact]
    public async Task AtualizarApartamento_ApartamentoNaoEncontrado_DeveLancarEntidadeNaoEncontradaExcecao()
    {
        // Arrange
        _repositorioMock.Setup(r => r.ObterPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Apartamento?)null);

        var manipulador = new AtualizarApartamentoManipulador(_repositorioMock.Object);
        var comando = new AtualizarApartamentoComando(Guid.NewGuid(), "202", "B");

        // Act
        var acao = async () => await manipulador.Handle(comando, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<EntidadeNaoEncontradaExcecao>();
    }

    // =====================================================
    // Testes de RemoverApartamentoManipulador
    // =====================================================

    /// <summary>
    /// Verifica que remover apartamento desocupado funciona corretamente.
    /// </summary>
    [Fact]
    public async Task RemoverApartamento_ApartamentoDesocupado_DeveRemoverERetornarVerdadeiro()
    {
        // Arrange
        var apartamento = new Apartamento("101", "A");
        _repositorioMock.Setup(r => r.ObterPorIdAsync(apartamento.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apartamento);
        _repositorioMock.Setup(r => r.SalvarAlteracoesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var manipulador = new RemoverApartamentoManipulador(_repositorioMock.Object);
        var comando = new RemoverApartamentoComando(apartamento.Id);

        // Act
        var resultado = await manipulador.Handle(comando, CancellationToken.None);

        // Assert
        resultado.Should().BeTrue();
        _repositorioMock.Verify(r => r.Remover(apartamento), Times.Once);
    }

    /// <summary>
    /// Verifica que tentativa de remover apartamento ocupado lança RegraDeNegocioExcecao.
    /// </summary>
    [Fact]
    public async Task RemoverApartamento_ApartamentoOcupado_DeveLancarRegraDeNegocioExcecao()
    {
        // Arrange
        var apartamento = new Apartamento("101", "A");
        apartamento.MarcarComoOcupado();

        _repositorioMock.Setup(r => r.ObterPorIdAsync(apartamento.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(apartamento);

        var manipulador = new RemoverApartamentoManipulador(_repositorioMock.Object);
        var comando = new RemoverApartamentoComando(apartamento.Id);

        // Act
        var acao = async () => await manipulador.Handle(comando, CancellationToken.None);

        // Assert
        await acao.Should().ThrowAsync<RegraDeNegocioExcecao>()
            .WithMessage("*ocupado*");

        _repositorioMock.Verify(r => r.Remover(It.IsAny<Apartamento>()), Times.Never);
    }
}

