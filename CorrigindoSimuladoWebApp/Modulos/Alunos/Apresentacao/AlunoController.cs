using CorrigindoSimuladoWebApp.Modulos.Alunos.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Alunos.Infraestrutura;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Infraestrutura;
using Microsoft.AspNetCore.Mvc;

namespace CorrigindoSimuladoWebApp.Modulos.Alunos.Apresentacao;

public sealed class AlunoController : Controller
{
    private readonly RepositorioAlunoEmArquivo repositorioAluno;
    private readonly RepositorioTurmaEmArquivo repositorioTurma;

    public AlunoController(RepositorioAlunoEmArquivo repositorioAluno, RepositorioTurmaEmArquivo repositorioTurma)
    {
        this.repositorioAluno = repositorioAluno;
        this.repositorioTurma = repositorioTurma;
    }

    [HttpGet]
    public ActionResult IndexAlunos()
    {
        return View();
    }

    [HttpGet]
    public ActionResult Cadastrar()
    {
        CadastrarAlunoViewModel viewModel = new(
            0000000,
            null,
            0,
            ObterTurmasDisponiveis()
        );

        return View(viewModel);
    }

    [HttpPost]
    public ActionResult Cadastrar(CadastrarAlunoViewModel viewModel)
    {
        Turma? turmaSelecionada = repositorioTurma.SelecionarPorId(viewModel.TurmaId);

        if (turmaSelecionada == null)
            ModelState.AddModelError(nameof(viewModel.TurmaId), "Selecione uma turma válida");

        if (!ModelState.IsValid)
        {
            viewModel = viewModel with
            {
                TurmasDisponiveis = ObterTurmasDisponiveis()
            };
            return View(viewModel);
        }

        Aluno aluno = new(
            viewModel.NumeroDeMatricula,
            viewModel.Nome ?? string.Empty,
            turmaSelecionada!
        );

        repositorioAluno.Cadastrar(aluno);
        return RedirectToAction(nameof(IndexAlunos));
    }

    private List<SelecionarTurmaViewModel> ObterTurmasDisponiveis()
    {
        List<SelecionarTurmaViewModel> viewModels = new();

        foreach (Turma t in repositorioTurma.SelecionarTodos())
        {
            SelecionarTurmaViewModel viewModel = new(t.Id, t.Nome);

            viewModels.Add(viewModel);
        }

        return viewModels;
    }
}
