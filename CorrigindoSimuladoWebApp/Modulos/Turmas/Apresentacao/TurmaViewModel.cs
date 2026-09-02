using System.ComponentModel.DataAnnotations;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Dominio;

public record ListarTurmaViewModel(
    int Id,
    string Nome,
    TipoCurso Curso,
    int AnoLetivo
);