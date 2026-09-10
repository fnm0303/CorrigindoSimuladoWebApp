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
    public ActionResult Listar()
    {
        List<ListarAlunoViewModel> viewModels = new List<ListarAlunoViewModel>();

        foreach (Aluno a in repositorioAluno.SelecionarTodos())
        {
            ListarAlunoViewModel viewModel = new ListarAlunoViewModel(
                a.Id,
                a.NumeroDeMatricula,
                a.Nome,
                a.Turma.Nome
            );

            viewModels.Add(viewModel);
        }
        return View(viewModels);
    }

    [HttpGet]
    public ActionResult Cadastrar()
    {
        CadastrarAlunoViewModel viewModel = new(
            null,
            null,
            0,
            ObterTurmasDisponiveis()
        );

        return View(viewModel);
    }

    [HttpPost]
    public ActionResult Cadastrar(CadastrarAlunoViewModel viewModel)
    {
        if (viewModel.NumeroDeMatricula.HasValue)
        {
            bool matriculaExiste = repositorioAluno.SelecionarTodos()
            .Any(a => a.NumeroDeMatricula == viewModel.NumeroDeMatricula.Value);

            if (matriculaExiste)
                ModelState.AddModelError(nameof(viewModel.NumeroDeMatricula), "Esta matrícula já está cadastrada no sistema.");
        }

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
            viewModel.NumeroDeMatricula!.Value,
            viewModel.Nome ?? string.Empty,
            turmaSelecionada!
        );

        repositorioAluno.Cadastrar(aluno);
        return RedirectToAction(nameof(IndexAlunos));
    }

    [HttpGet]
    public ActionResult Editar(int id)
    {
        Aluno? alunoSelecionado = repositorioAluno.SelecionarPorId(id);

        if (alunoSelecionado == null)
            return NotFound();

        EditarAlunoViewModel viewModel = new(
            alunoSelecionado.Id,
            alunoSelecionado.NumeroDeMatricula,
            alunoSelecionado.Nome,
            alunoSelecionado.Turma.Id,
            ObterTurmasDisponiveis()
        );

        return View(viewModel);
    }

    [HttpPost]
    public ActionResult Editar(int id, EditarAlunoViewModel viewModel)
    {
        if (viewModel.NumeroDeMatricula.HasValue)
        {
            bool matriculaEmUsoPorOutro = repositorioAluno.SelecionarTodos()
                .Any(a => a.NumeroDeMatricula == viewModel.NumeroDeMatricula.Value && a.Id != id);

            if (matriculaEmUsoPorOutro)
            {
                ModelState.AddModelError(
                    nameof(viewModel.NumeroDeMatricula),
                    "Esta matrícula já está cadastrada para outro aluno."
                );
            }
        }
        Turma? turmaSelecionada = repositorioTurma.SelecionarPorId(viewModel.TurmaId);

        if (turmaSelecionada == null)
            ModelState.AddModelError(nameof(viewModel.TurmaId), "Selecione uma turma válida.");

        if (!ModelState.IsValid)
        {
            viewModel = viewModel with
            {
                TurmasDisponiveis = ObterTurmasDisponiveis()
            };

            return View(viewModel);
        }
        Aluno alunoAtualizado = new(
            viewModel.NumeroDeMatricula!.Value,
            viewModel.Nome ?? string.Empty,
            turmaSelecionada!
        );

        bool conseguiuEditar = repositorioAluno.Editar(id, alunoAtualizado);

        if (!conseguiuEditar)
            return NotFound();

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
