using CorrigindoSimuladoWebApp.Modulos.Provas.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Provas.Infraestrutura;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Dominio;
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

    [HttpGet]
    public ActionResult Cadastrar()
    {
        CadastrarProvaViewModel viewModel = new(
            null,
            null,
            null,
            0,
            ObterTurmasDisponiveis()
        );
        return View(viewModel);
    }

    [HttpPost]
    public ActionResult Cadastrar(CadastrarProvaViewModel viewModel)
    {
        Turma? turmaSelecionada = repositorioTurma.SelecionarPorId(viewModel.TurmaId);

        if (turmaSelecionada == null)
            ModelState.AddModelError(nameof(viewModel.TurmaId), "Selecione uma turma válida");

        // Validação de Negócio: O gabarito deve ter o mesmo tamanho da quantidade de questões
        if (viewModel.QuantidadeQuestoes.HasValue && !string.IsNullOrEmpty(viewModel.GabaritoCorreto))
        {
            string gabaritoLimpo = viewModel.GabaritoCorreto.Replace(" ", "").Trim();

            if (gabaritoLimpo.Length != viewModel.QuantidadeQuestoes.Value)
            {
                ModelState.AddModelError(
                    nameof(viewModel.GabaritoCorreto),
                    $"O gabarito deve conter exatamente {viewModel.QuantidadeQuestoes} letras."
                );
            }
        }

        if (!ModelState.IsValid)
        {
            viewModel = viewModel with
            {
                TurmasDisponiveis = ObterTurmasDisponiveis()
            };
            return View(viewModel);
        }

        Prova prova = new(
            viewModel.Nome ?? string.Empty,
            viewModel.QuantidadeQuestoes!.Value,
            viewModel.GabaritoCorreto.Replace(" ", "").ToUpper().Trim(), // Padroniza o gabarito
            turmaSelecionada!
        );

        repositorioProva.Cadastrar(prova);
        return RedirectToAction(nameof(IndexProvas));
    }

    [HttpGet]
    public ActionResult Editar(int id)
    {
        Prova? provaSelecionada = repositorioProva.SelecionarPorId(id);

        if (provaSelecionada == null)
            return NotFound();

        EditarProvaViewModel viewModel = new(
            provaSelecionada.Id,
            provaSelecionada.Nome,
            provaSelecionada.QuantidadeQuestoes,
            provaSelecionada.GabaritoCorreto,
            provaSelecionada.Turma.Id,
            ObterTurmasDisponiveis()
        );

        return View(viewModel);
    }

    [HttpPost]
    public ActionResult Editar(int id, EditarProvaViewModel viewModel)
    {
        Turma? turmaSelecionada = repositorioTurma.SelecionarPorId(viewModel.TurmaId);

        if (turmaSelecionada == null)
            ModelState.AddModelError(nameof(viewModel.TurmaId), "Selecione uma turma válida");

        // Validação de Negócio: O gabarito deve ter o mesmo tamanho da quantidade de questões
        if (viewModel.QuantidadeQuestoes.HasValue && !string.IsNullOrEmpty(viewModel.GabaritoCorreto))
        {
            string gabaritoLimpo = viewModel.GabaritoCorreto.Replace(" ", "").Trim();

            if (gabaritoLimpo.Length != viewModel.QuantidadeQuestoes.Value)
            {
                ModelState.AddModelError(
                    nameof(viewModel.GabaritoCorreto),
                    $"O gabarito deve conter exatamente {viewModel.QuantidadeQuestoes} letras."
                );
            }
        }

        if (!ModelState.IsValid)
        {
            viewModel = viewModel with
            {
                TurmasDisponiveis = ObterTurmasDisponiveis()
            };
            return View(viewModel);
        }

        Prova provaAtualizada = new(
            viewModel.Nome ?? string.Empty,
            viewModel.QuantidadeQuestoes!.Value,
            viewModel.GabaritoCorreto.Replace(" ", "").ToUpper().Trim(), // Padroniza o gabarito
            turmaSelecionada!
        );

        bool conseguiuEditar = repositorioProva.Editar(id, provaAtualizada);

        if (!conseguiuEditar)
            return NotFound();

        return RedirectToAction(nameof(IndexProvas));
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
