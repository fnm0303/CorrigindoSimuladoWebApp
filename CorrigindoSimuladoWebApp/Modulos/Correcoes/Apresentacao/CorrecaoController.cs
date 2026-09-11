using CorrigindoSimuladoWebApp.Modulos.Alunos.Infraestrutura;
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
}
