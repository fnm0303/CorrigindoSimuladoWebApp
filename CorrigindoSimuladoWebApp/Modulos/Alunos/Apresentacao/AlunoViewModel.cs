
using System.ComponentModel.DataAnnotations;

public record ListarAlunoViewModel(
    int Id,
    int NumeroDeMatricula,
    string Nome,
    string NomeTurma
);

public record SelecionarTurmaViewModel(int Id, string Nome);
public record CadastrarAlunoViewModel(
    [Required(ErrorMessage = "O campo é obrigatório.")]
    [Range(1000000, 9999999, ErrorMessage ="A matrícula deve conter exatamente 7 dígitos.")]
    int NumeroDeMatricula,

    [Required(ErrorMessage = "O campo \"Nome\" é obrigatório.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "O campo \"Nome\" deve ter entre 2 e 100 caracteres")]
    string? Nome,

    [Range(1, int.MaxValue, ErrorMessage = "O campo \"Turma\" é obrigatório.")]
    int TurmaId,

    List<SelecionarTurmaViewModel>? TurmasDisponiveis
);