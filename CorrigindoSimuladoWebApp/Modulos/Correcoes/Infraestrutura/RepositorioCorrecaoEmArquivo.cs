using CorrigindoSimuladoWebApp.Compartilhado.Infraestrutura.Arquivos;
using CorrigindoSimuladoWebApp.Modulos.Correcoes.Dominio;
namespace CorrigindoSimuladoWebApp.Modulos.Correcoes.Infraestrutura;

public sealed class RepositorioCorrecaoEmArquivo : RepositorioBaseEmArquivo<Correcao>
{
    public RepositorioCorrecaoEmArquivo(ContextoJson contexto) : base(contexto)
    {
    }

    protected override List<Correcao> ObterRegistros()
    {
        return contexto.Correcoes;
    }
}
