using CorrigindoSimuladoWebApp.Modulos.Turmas.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Infraestrutura;
using Microsoft.AspNetCore.Mvc;

namespace CorrigindoSimuladoWebApp.Modulos.Turmas.Apresentacao;

public sealed class TurmaController : Controller
{
    private readonly RepositorioTurmaEmArquivo repositorio;
    public TurmaController(RepositorioTurmaEmArquivo repositorio)
    {
        this.repositorio = repositorio;
    }

    [HttpGet]
    public ActionResult IndexTurmas()
    {
        return View();
    }

    [HttpGet]
    public ActionResult Listar()
    {
        List<ListarTurmaViewModel> viewModels = new List<ListarTurmaViewModel>();

        foreach (Turma t in repositorio.SelecionarTodos())
        {
            ListarTurmaViewModel viewModel = new ListarTurmaViewModel(
                t.Id,
                t.Nome,
                t.Curso,
                t.AnoLetivo
            );

            viewModels.Add(viewModel);
        }
        return View(viewModels);
    }

    [HttpGet]
    public ActionResult Cadastrar()
    {
        CadastrarTurmaViewModel viewModel = new(
            null,
            null,
            DateTime.Now.Year
        );

        return View(viewModel);
    }

    [HttpPost]
    public ActionResult Cadastrar(CadastrarTurmaViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            // Se houver erro de digitação, devolve a mesma tela para o usuário corrigir
            return View(viewModel);
        }
        Turma? turma = new(
            viewModel.Nome ?? string.Empty,
            viewModel.Curso.Value,
            viewModel.AnoLetivo
        );

        repositorio.Cadastrar(turma);

        return RedirectToAction(nameof(IndexTurmas));
    }
}
