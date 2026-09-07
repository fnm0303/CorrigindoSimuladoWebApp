using CorrigindoSimuladoWebApp.Compartilhado.Infraestrutura.Arquivos;
using CorrigindoSimuladoWebApp.Modulos.Alunos.Dominio;

namespace CorrigindoSimuladoWebApp.Modulos.Alunos.Infraestrutura;

public sealed class RepositorioAlunoEmArquivo : RepositorioBaseEmArquivo<Aluno>
{
    public RepositorioAlunoEmArquivo(ContextoJson contexto) : base(contexto)
    {
    }
    protected override List<Aluno> ObterRegistros()
    {
        return contexto.Alunos;
    }
}
