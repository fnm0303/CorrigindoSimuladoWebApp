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
}
