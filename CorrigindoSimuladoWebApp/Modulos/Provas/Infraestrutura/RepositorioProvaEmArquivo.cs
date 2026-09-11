using CorrigindoSimuladoWebApp.Compartilhado.Infraestrutura.Arquivos;
using CorrigindoSimuladoWebApp.Modulos.Provas.Dominio;

namespace CorrigindoSimuladoWebApp.Modulos.Provas.Infraestrutura;

public sealed class RepositorioProvaEmArquivo : RepositorioBaseEmArquivo<Prova>
{
    public RepositorioProvaEmArquivo(ContextoJson contexto) : base(contexto)
    {
    }
    protected override List<Prova> ObterRegistros()
    {
        return contexto.Provas;
    }
}
