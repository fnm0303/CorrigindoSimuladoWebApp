using CorrigindoSimuladoWebApp.Compartilhado.Dominio;
using CorrigindoSimuladoWebApp.Compartilhado.Infraestrutura.Arquivos;

public abstract class RepositorioBaseEmArquivo<TEntidade> where TEntidade : EntidadeBase
{
    protected readonly ContextoJson contexto;
    protected readonly List<TEntidade> registros;

    protected RepositorioBaseEmArquivo(ContextoJson contexto)
    {
        this.contexto = contexto;
        registros = ObterRegistros();
    }

    public void Cadastrar(TEntidade novoRegistro)
    {
        int ultimoId = 0;

        foreach (TEntidade r in registros)
        {
            if (r.Id > ultimoId)
                ultimoId = r.Id;
        }

        novoRegistro.Id = ultimoId + 1;

        registros.Add(novoRegistro);

        contexto.Salvar();
    }

    public bool Editar(int idSelecionado, TEntidade entidadeAtualizada)
    {
        TEntidade? entidadeSelecionada = SelecionarPorId(idSelecionado);

        if (entidadeSelecionada == null)
            return false;

        entidadeSelecionada.Atualizar(entidadeAtualizada);

        contexto.Salvar();

        return true;
    }

    public bool Excluir(int idSelecionado)
    {
        TEntidade? registro = SelecionarPorId(idSelecionado);

        if (registro == null)
            return false;

        bool conseguiuRemover = registros.Remove(registro);

        if (!conseguiuRemover)
            return false;

        contexto.Salvar();

        return true;
    }

    public TEntidade? SelecionarPorId(int idSelecionado)
    {
        foreach (TEntidade o in registros)
        {
            if (o.Id == idSelecionado)
                return o;
        }

        return null;
    }

    public List<TEntidade> SelecionarTodos()
    {
        return registros;
    }

    protected abstract List<TEntidade> ObterRegistros();
}
