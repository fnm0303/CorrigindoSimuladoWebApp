using CorrigindoSimuladoWebApp.Modulos.Provas.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Provas.Infraestrutura;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Infraestrutura;
using Microsoft.AspNetCore.Mvc;

namespace CorrigindoSimuladoWebApp.Modulos.Provas.Apresentacao;

public sealed class ProvaController : Controller
{
    private readonly RepositorioProvaEmArquivo repositorioProva;
    private readonly RepositorioTurmaEmArquivo repositorioTurma;
    public ProvaController(RepositorioProvaEmArquivo repositorioProva, RepositorioTurmaEmArquivo repositorioTurma)
    {
        this.repositorioProva = repositorioProva;
        this.repositorioTurma = repositorioTurma;
    }

    [HttpGet]
    public ActionResult IndexProvas()
    {
        return View();
    }

    [HttpGet]
    public ActionResult Listar()
    {
        List<ListarProvaViewModel> viewModels = new List<ListarProvaViewModel>();

        foreach (Prova p in repositorioProva.SelecionarTodos())
        {
            ListarProvaViewModel viewModel = new ListarProvaViewModel(
                p.Id,
                p.Nome,
                p.Turma.Nome
            );
            viewModels.Add(viewModel);
        }
        return View(viewModels);
    }
}
