using BackEndAluguel.Domain.Entidades;

namespace BackEndAluguel.Domain.Interfaces;

public interface IDependenteRepositorio : IRepositorio<Dependente>
{
    Task<IEnumerable<Dependente>> ObterPorInquilinoAsync(Guid inquilinoId, CancellationToken cancellationToken = default);
    Task<bool> ExistePorCpfAsync(string cpf, CancellationToken cancellationToken = default);
}

