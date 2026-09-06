using System.ComponentModel.DataAnnotations;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Dominio;

public record ListarTurmaViewModel(
    int Id,
    string Nome,
    TipoCurso Curso,
    int AnoLetivo
);

public record CadastrarTurmaViewModel(
    [Required(ErrorMessage = "O campo \"Nome\" é obrigatório.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "O campo \"Nome\" deve ter entre 2 e 100 caracteres")]
    string? Nome,

    [Required(ErrorMessage = "Selecione o curso.")]
    TipoCurso? Curso,

    [Required(ErrorMessage = "O campo é obrigatório.")]
    [Range(2000, 2100, ErrorMessage = "Informe um ano válido entre 2000 e 2100")]
    int AnoLetivo
);