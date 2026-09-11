using CorrigindoSimuladoWebApp.Modulos.Alunos.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Alunos.Infraestrutura;
using CorrigindoSimuladoWebApp.Modulos.Correcoes.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Correcoes.Infraestrutura;
using CorrigindoSimuladoWebApp.Modulos.Provas.Infraestrutura;
using Microsoft.AspNetCore.Mvc;

namespace CorrigindoSimuladoWebApp.Modulos.Correcoes.Apresentacao;

public sealed class CorrecaoController : Controller
{
    private readonly RepositorioCorrecaoEmArquivo repositorioCorrecao;
    private readonly RepositorioProvaEmArquivo repositorioProva;
    private readonly RepositorioAlunoEmArquivo repositorioAluno;

    public CorrecaoController(
        RepositorioCorrecaoEmArquivo repositorioCorrecao,
        RepositorioProvaEmArquivo repositorioProva,
        RepositorioAlunoEmArquivo repositorioAluno)
    {
        this.repositorioCorrecao = repositorioCorrecao;
        this.repositorioProva = repositorioProva;
        this.repositorioAluno = repositorioAluno;
    }

    [HttpGet]
    public ActionResult IndexCorrecoes()
    {
        return View("/Modulos/Correcoes/Apresentacao/Views/IndexCorrecoes.cshtml");
    }

    [HttpGet]
    public ActionResult Listar()
    {
        List<ListarCorrecaoViewModel> viewModels = new List<ListarCorrecaoViewModel>();

        foreach (Correcao c in repositorioCorrecao.SelecionarTodos())
        {
            ListarCorrecaoViewModel viewModel = new ListarCorrecaoViewModel(
                c.Id,
                c.Prova.Nome,
                c.Aluno.Nome,
                c.NumeroAcertos
            );

            viewModels.Add(viewModel);
        }
        return View("/Modulos/Correcoes/Apresentacao/Views/Listar.cshtml", viewModels);
    }

    [HttpGet]
    public ActionResult Cadastrar()
    {
        CadastrarCorrecaoViewModel viewModel = new(
            0,
            null,
            1,
            null,
            null,
            ObterAlunosDisponiveis()
        );
        return View(viewModel);
    }

    private List<SelecionarAlunoViewModel>? ObterAlunosDisponiveis()
    {
        List<SelecionarAlunoViewModel> viewModels = new();

        foreach (Aluno a in repositorioAluno.SelecionarTodos())
        {
            SelecionarAlunoViewModel viewModel = new(a.Id, a.Nome);

            viewModels.Add(viewModel);
        }

        return viewModels;
    }
}
