using BackEndAluguel.Domain.Entidades;
using BackEndAluguel.Domain.Enumeradores;
using BackEndAluguel.Infrastructure.Contexto;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BackEndAluguel.Tests.Infraestrutura;

/// <summary>
/// Testes de integração para o <see cref="AluguelDbContext"/>.
/// Usa o provedor InMemory do EF Core para validar as configurações de mapeamento,
/// relacionamentos e persistência sem necessidade de um banco real.
/// </summary>
public class AluguelDbContextTestes : IDisposable
{
    private readonly AluguelDbContext _contexto;

    /// <summary>
    /// Inicializa o contexto InMemory com um banco isolado por teste.
    /// </summary>
    public AluguelDbContextTestes()
    {
        var opcoes = new DbContextOptionsBuilder<AluguelDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _contexto = new AluguelDbContext(opcoes);
    }

    // =====================================================
    // Testes de Apartamento
    // =====================================================

    /// <summary>
    /// Verifica que é possível persistir e recuperar um apartamento do banco InMemory.
    /// </summary>
    [Fact]
    public async Task Apartamento_PersistirERecuperar_DeveRetornarDadosCorretos()
    {
        // Arrange
        var apartamento = new Apartamento("101", "A");

        // Act
        _contexto.Apartamentos.Add(apartamento);
        await _contexto.SaveChangesAsync();

        var recuperado = await _contexto.Apartamentos.FindAsync(apartamento.Id);

        // Assert
        recuperado.Should().NotBeNull();
        recuperado!.Numero.Should().Be("101");
        recuperado.Bloco.Should().Be("A");
        recuperado.Ocupado.Should().BeFalse();
    }

    /// <summary>
    /// Verifica que múltiplos apartamentos são listados corretamente.
    /// </summary>
    [Fact]
    public async Task Apartamentos_ListarTodos_DeveRetornarTodosOsRegistros()
    {
        // Arrange
        _contexto.Apartamentos.AddRange(
            new Apartamento("101", "A"),
            new Apartamento("102", "A"),
            new Apartamento("201", "B"));
        await _contexto.SaveChangesAsync();

        // Act
        var todos = await _contexto.Apartamentos.ToListAsync();

        // Assert
        todos.Should().HaveCount(3);
    }

    // =====================================================
    // Testes de Inquilino
    // =====================================================

    /// <summary>
    /// Verifica que é possível persistir um inquilino vinculado a um apartamento.
    /// </summary>
    [Fact]
    public async Task Inquilino_PersistirComApartamento_DeveCarregarRelacionamento()
    {
        // Arrange
        var apartamento = new Apartamento("101", "A");
        _contexto.Apartamentos.Add(apartamento);
        await _contexto.SaveChangesAsync();

        var inquilino = new Inquilino(
            "João da Silva", "12345678901", 2,
            new DateOnly(2024, 1, 1), new DateOnly(2026, 12, 31),
            1500m, apartamento.Id,
            new DateOnly(1990, 5, 15),
            new List<int> { 30, 60, 90 });

        // Act
        _contexto.Inquilinos.Add(inquilino);
        await _contexto.SaveChangesAsync();

        var recuperado = await _contexto.Inquilinos
            .Include(i => i.Apartamento)
            .FirstOrDefaultAsync(i => i.Id == inquilino.Id);

        // Assert
        recuperado.Should().NotBeNull();
        recuperado!.NomeCompleto.Should().Be("João da Silva");
        recuperado.Cpf.Should().Be("12345678901");
        recuperado.DiasAlertaVencimento.Should().Equal(new List<int> { 30, 60, 90 });
        recuperado.Apartamento.Should().NotBeNull();
        recuperado.Apartamento!.Numero.Should().Be("101");
    }

    // =====================================================
    // Testes de Fatura
    // =====================================================

    /// <summary>
    /// Verifica que é possível persistir uma fatura e carregar o inquilino associado.
    /// </summary>
    [Fact]
    public async Task Fatura_PersistirComInquilino_DeveCarregarRelacionamento()
    {
        // Arrange
        var apartamento = new Apartamento("101", "A");
        _contexto.Apartamentos.Add(apartamento);
        await _contexto.SaveChangesAsync();

        var inquilino = new Inquilino(
            "Maria Santos", "98765432100", 1,
            new DateOnly(2024, 1, 1), new DateOnly(2026, 12, 31),
            1200m, apartamento.Id);
        _contexto.Inquilinos.Add(inquilino);
        await _contexto.SaveChangesAsync();

        var fatura = new Fatura("05/2025", 1200m, 75m, 110m,
            new DateOnly(2025, 5, 10), inquilino.Id, codigoPix: "PIX_CODE_123");

        // Act
        _contexto.Faturas.Add(fatura);
        await _contexto.SaveChangesAsync();

        var recuperada = await _contexto.Faturas
            .Include(f => f.Inquilino)
            .FirstOrDefaultAsync(f => f.Id == fatura.Id);

        // Assert
        recuperada.Should().NotBeNull();
        recuperada!.MesReferencia.Should().Be("05/2025");
        recuperada.ValorAluguel.Should().Be(1200m);
        recuperada.ValorAgua.Should().Be(75m);
        recuperada.ValorLuz.Should().Be(110m);
        recuperada.Status.Should().Be(StatusFatura.Pendente);
        recuperada.CodigoPix.Should().Be("PIX_CODE_123");
        recuperada.Inquilino.Should().NotBeNull();
        recuperada.Inquilino!.NomeCompleto.Should().Be("Maria Santos");
    }

    /// <summary>
    /// Verifica que registrar pagamento persiste o status Pago corretamente.
    /// </summary>
    [Fact]
    public async Task Fatura_RegistrarPagamento_DevePersistitStatusPago()
    {
        // Arrange
        var apartamento = new Apartamento("201", "B");
        _contexto.Apartamentos.Add(apartamento);

        var inquilino = new Inquilino("Pedro Costa", "11122233344", 1,
            new DateOnly(2024, 1, 1), new DateOnly(2026, 12, 31), 1000m, apartamento.Id);
        _contexto.Inquilinos.Add(inquilino);
        await _contexto.SaveChangesAsync();

        var fatura = new Fatura("06/2025", 1000m, 0m, 0m, new DateOnly(2025, 6, 10), inquilino.Id);
        _contexto.Faturas.Add(fatura);
        await _contexto.SaveChangesAsync();

        // Act
        fatura.RegistrarPagamento(new DateOnly(2025, 6, 5));
        await _contexto.SaveChangesAsync();

        var recuperada = await _contexto.Faturas.FindAsync(fatura.Id);

        // Assert
        recuperada!.Status.Should().Be(StatusFatura.Pago);
        recuperada.DataPagamento.Should().Be(new DateOnly(2025, 6, 5));
    }

    // =====================================================
    // Testes de relacionamentos em cascata
    // =====================================================

    /// <summary>
    /// Verifica que ao remover um inquilino suas faturas são removidas em cascata.
    /// </summary>
    [Fact]
    public async Task RemoverInquilino_DevRemoverFaturasEmCascata()
    {
        // Arrange
        var apartamento = new Apartamento("301", "C");
        _contexto.Apartamentos.Add(apartamento);

        var inquilino = new Inquilino("Ana Lima", "55566677788", 1,
            new DateOnly(2024, 1, 1), new DateOnly(2026, 12, 31), 900m, apartamento.Id);
        _contexto.Inquilinos.Add(inquilino);
        await _contexto.SaveChangesAsync();

        _contexto.Faturas.Add(new Fatura("03/2025", 900m, 0m, 0m, new DateOnly(2025, 3, 10), inquilino.Id));
        _contexto.Faturas.Add(new Fatura("04/2025", 900m, 0m, 0m, new DateOnly(2025, 4, 10), inquilino.Id));
        await _contexto.SaveChangesAsync();

        // Act
        _contexto.Inquilinos.Remove(inquilino);
        await _contexto.SaveChangesAsync();

        // Assert
        var faturasRestantes = await _contexto.Faturas
            .Where(f => f.InquilinoId == inquilino.Id)
            .ToListAsync();

        faturasRestantes.Should().BeEmpty();
    }

    /// <summary>
    /// Libera os recursos do contexto após cada teste.
    /// </summary>
    public void Dispose()
    {
        _contexto.Dispose();
        GC.SuppressFinalize(this);
    }
}

