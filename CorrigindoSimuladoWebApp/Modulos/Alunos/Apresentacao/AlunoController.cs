using CorrigindoSimuladoWebApp.Modulos.Alunos.Infraestrutura;
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
}
