using CorrigindoSimuladoWebApp.Compartilhado.Infraestrutura.Arquivos;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Dominio;

namespace CorrigindoSimuladoWebApp.Modulos.Turmas.Infraestrutura;

public sealed class RepositorioTurmaEmArquivo : RepositorioBaseEmArquivo<Turma>
{
    public RepositorioTurmaEmArquivo(ContextoJson contexto) : base(contexto)
    {
    }

    protected override List<Turma> ObterRegistros()
    {
        return contexto.Turmas;
    }
}
