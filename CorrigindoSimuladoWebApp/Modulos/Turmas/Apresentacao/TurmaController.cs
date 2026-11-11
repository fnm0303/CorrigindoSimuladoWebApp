using CorrigindoSimuladoWebApp.Modulos.Alunos.Infraestrutura;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Infraestrutura;
using Microsoft.AspNetCore.Mvc;

namespace CorrigindoSimuladoWebApp.Modulos.Turmas.Apresentacao;

public sealed class TurmaController : Controller
{
    private readonly RepositorioTurmaEmArquivo repositorioTurma;
    private readonly RepositorioAlunoEmArquivo repositorioAluno;
    public TurmaController(RepositorioTurmaEmArquivo repositorioTurma, RepositorioAlunoEmArquivo repositorioAluno)
    {
        this.repositorioTurma = repositorioTurma;
        this.repositorioAluno = repositorioAluno;
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

        foreach (Turma t in repositorioTurma.SelecionarTodos())
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
    public ActionResult DetalhesAlunos(Guid id)
    {
        var turma = repositorioTurma.SelecionarPorId(id); // Ajuste para o método do seu repositório de turmas
        if (turma == null)
            return NotFound();

        // Busca todos os alunos e filtra os que pertencem a esta turma 
        // (ou use um método direto do seu repositório de alunos se já possuir, ex: SelecionarPorTurmaId(id))
        var alunosDaTurma = repositorioAluno.SelecionarTodos()
        .Where(a => a.Turma.Id == id)
        .OrderBy(a => a.Nome)
        .Select(a => new ListarAlunoViewModel(
            a.Id,
            a.NumeroDeMatricula,
            a.Nome,
            turma.Nome
        ))
        .ToList();

        ViewBag.NomeTurma = turma.Nome;
        ViewBag.TurmaId = turma.Id;

        return View(alunosDaTurma);
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
            viewModel.Curso!.Value,
            viewModel.AnoLetivo
        );

        repositorioTurma.Cadastrar(turma);

        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult Editar(Guid id)
    {
        Turma? turmaSelecionada = repositorioTurma.SelecionarPorId(id);

        if (turmaSelecionada == null)
            return NotFound();

        EditarTurmaViewModel viewModel = new(
            turmaSelecionada.Id,
            turmaSelecionada.Nome,
            turmaSelecionada.Curso,
            turmaSelecionada.AnoLetivo
        );

        return View(viewModel);
    }

    [HttpPost]
    public ActionResult Editar(Guid id, EditarTurmaViewModel viewModel)
    {
        if (!ModelState.IsValid)
            return View(viewModel);

        Turma turmaAtualizada = new(
            viewModel.Nome ?? string.Empty,
            viewModel.Curso!.Value,
            viewModel.AnoLetivo
        );

        bool conseguiuEditar = repositorioTurma.Editar(id, turmaAtualizada);

        if (!conseguiuEditar)
            return NotFound();

        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult Excluir(Guid id)
    {
        Turma? turmaSelecionada = repositorioTurma.SelecionarPorId(id);

        if (turmaSelecionada == null)
            return NotFound();

        ExcluirTurmaViewModel viewModel = new(
            turmaSelecionada.Id,
            turmaSelecionada.Nome
        );

        return View(viewModel);
    }

    [HttpPost]
    public ActionResult Excluir(ExcluirTurmaViewModel viewModel)
    {
        bool conseguiuExcluir = repositorioTurma.Excluir(viewModel.Id);

        if (!conseguiuExcluir)
            return NotFound();

        return RedirectToAction(nameof(Listar));
    }
}
