namespace CorrigindoSimuladoWebApp.Compartilhado.Dominio;

public abstract class EntidadeBase
{
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public abstract void Atualizar(EntidadeBase entidadeAtualizada);

}
