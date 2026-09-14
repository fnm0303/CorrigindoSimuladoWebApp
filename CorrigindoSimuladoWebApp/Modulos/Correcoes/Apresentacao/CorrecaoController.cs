using CorrigindoSimuladoWebApp.Modulos.Alunos.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Alunos.Infraestrutura;
using CorrigindoSimuladoWebApp.Modulos.Correcoes.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Correcoes.Infraestrutura;
using CorrigindoSimuladoWebApp.Modulos.Provas.Dominio;
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
    public ActionResult Cadastrar(int provaId)
    {
        // 1. Busca a Prova selecionada
        Prova? prova = repositorioProva.SelecionarPorId(provaId);

        if (prova == null)
            return RedirectToAction("SelecionarProva");

        // 2. Filtra os alunos que pertencem SOMENTE à turma vinculada a esta prova
        List<SelecionarItemViewModel> alunosDaTurma = repositorioAluno.SelecionarTodos()
            .Where(a => a.Turma.Id == prova.Turma.Id)
            .Select(a => new SelecionarItemViewModel(a.Id, a.Nome))
            .ToList();

        // 3. Monta a ViewModel
        CadastrarCorrecaoViewModel viewModel = new(
            prova.Id,
            prova.Nome,
            prova.QuantidadeQuestoes,
            null,
            null,
            alunosDaTurma
        );

        return View("/Modulos/Correcoes/Apresentacao/Views/Cadastrar.cshtml", viewModel);
    }

    [HttpPost]
    public ActionResult Cadastrar(CadastrarCorrecaoViewModel viewModel)
    {
        Prova? provaSelecionada = repositorioProva.SelecionarPorId(viewModel.ProvaId);
        Aluno? alunoSelecionado = viewModel.AlunoId.HasValue ? repositorioAluno.SelecionarPorId(viewModel.AlunoId.Value) : null;

        if (provaSelecionada == null || alunoSelecionado == null)
            return NotFound();

        // VALIDAÇÃO 1: Impede que o aluno tenha duas notas na mesma prova
        bool alunoJaFezAProva = repositorioCorrecao.SelecionarTodos()
            .Any(c => c.Prova.Id == provaSelecionada.Id && c.Aluno.Id == alunoSelecionado.Id);

        if (alunoJaFezAProva)
            ModelState.AddModelError(nameof(viewModel.AlunoId), "Este aluno já possui uma correção registrada para esta prova.");

        // VALIDAÇÃO 2: O gabarito tem que ter o mesmo tamanho da quantidade de questões da prova
        if (!string.IsNullOrEmpty(viewModel.GabaritoAluno))
        {
            string gabaritoLimpo = viewModel.GabaritoAluno.Replace(" ", "").Trim();

            if (gabaritoLimpo.Length != provaSelecionada.QuantidadeQuestoes)
            {
                ModelState.AddModelError(
                    nameof(viewModel.GabaritoAluno),
                    $"O gabarito deve conter exatamente {provaSelecionada.QuantidadeQuestoes} letras. Você digitou {gabaritoLimpo.Length}."
                );
            }
        }

        // Se houver erros (validação do C# ou as validações acima)
        if (!ModelState.IsValid)
        {
            // Recarrega os alunos da turma para o dropdown não sumir na tela de erro
            viewModel = viewModel with
            {
                AlunosDisponiveis = repositorioAluno.SelecionarTodos()
                    .Where(a => a.Turma.Id == provaSelecionada.Turma.Id)
                    .Select(a => new SelecionarItemViewModel(a.Id, a.Nome)).ToList()
            };

            return View("/Modulos/Correcoes/Apresentacao/Views/Cadastrar.cshtml", viewModel);
        }

        // Salva a correção (A sua entidade Correcao já calcula os acertos automaticamente no construtor)
        Correcao novaCorrecao = new Correcao(provaSelecionada, alunoSelecionado, viewModel.GabaritoAluno!);
        repositorioCorrecao.Cadastrar(novaCorrecao);

        // Redireciona para a listagem (Tabela)
        return RedirectToAction(nameof(Listar));
    }

    [HttpGet]
    public ActionResult SelecionarProva()
    {
        // 1. Busca todas as provas do arquivo JSON
        var provasCadastradas = repositorioProva.SelecionarTodos();

        // 2. Cria a lista de ViewModels apenas com os dados necessários para a tela (Id e Nome)
        List<SelecionarItemViewModel> provasDisponiveis = new List<SelecionarItemViewModel>();

        foreach (var p in provasCadastradas)
        {
            provasDisponiveis.Add(new SelecionarItemViewModel(p.Id, p.Nome));
        }

        // 3. Envia a lista para a View através da ViewBag
        ViewBag.Provas = provasDisponiveis;

        // Retorna o arquivo SelecionarProva.cshtml
        return View("/Modulos/Correcoes/Apresentacao/Views/SelecionarProva.cshtml");
    }

    [HttpGet]
    public ActionResult DetalharCorrecao(int id)
    {
        // 1. Busca a correção no banco/arquivo
        Correcao? correcao = repositorioCorrecao.SelecionarPorId(id);

        if (correcao == null)
            return NotFound();

        // 2. Removemos os espaços em branco para garantir que as posições batam perfeitamente
        string gabaritoAluno = correcao.GabaritoAluno.Replace(" ", "").ToUpper();
        string gabaritoProva = correcao.Prova.GabaritoCorreto.Replace(" ", "").ToUpper();

        List<DetalheQuestaoViewModel> questoes = new List<DetalheQuestaoViewModel>();

        // 3. Montamos a lista comparando questão por questão
        for (int i = 0; i < correcao.Prova.QuantidadeQuestoes; i++)
        {
            char respostaAluno = gabaritoAluno[i];
            char respostaCorreta = gabaritoProva[i];
            bool acertou = respostaAluno == respostaCorreta;

            questoes.Add(new DetalheQuestaoViewModel(
                NumeroQuestao: i + 1,
                RespostaAluno: respostaAluno,
                RespostaCorreta: respostaCorreta,
                Acertou: acertou
            ));
        }

        // 4. Construímos a ViewModel principal
        DetalhesCorrecaoViewModel viewModel = new DetalhesCorrecaoViewModel(
            correcao.Id,
            correcao.Prova.Nome,
            correcao.Aluno.Nome,
            correcao.NumeroAcertos,
            correcao.Prova.QuantidadeQuestoes,
            questoes
        );

        return View("/Modulos/Correcoes/Apresentacao/Views/DetalharCorrecao.cshtml", viewModel);
    }

    private List<SelecionarItemViewModel>? ObterAlunosDisponiveis()
    {
        List<SelecionarItemViewModel> viewModels = new();

        foreach (Aluno a in repositorioAluno.SelecionarTodos())
        {
            SelecionarItemViewModel viewModel = new(a.Id, a.Nome);

            viewModels.Add(viewModel);
        }

        return viewModels;
    }
}
