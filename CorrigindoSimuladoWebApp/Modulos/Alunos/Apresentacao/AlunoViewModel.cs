
using System.ComponentModel.DataAnnotations;

public record ListarAlunoViewModel(
    Guid Id,
    int NumeroDeMatricula,
    string Nome,
    string NomeTurma
);

public record SelecionarTurmaViewModel(Guid Id, string Nome);
public record CadastrarAlunoViewModel(
    [Required(ErrorMessage = "O campo é obrigatório.")]
    [Range(1000000, 9999999, ErrorMessage ="A matrícula deve conter exatamente 7 dígitos.")]
    int? NumeroDeMatricula,

    [Required(ErrorMessage = "O campo \"Nome\" é obrigatório.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "O campo \"Nome\" deve ter entre 2 e 100 caracteres")]
    string? Nome,

    Guid TurmaId,

    List<SelecionarTurmaViewModel>? TurmasDisponiveis
);

public record EditarAlunoViewModel(
    Guid Id,
    [Required(ErrorMessage = "O campo é obrigatório.")]
    [Range(1000000, 9999999, ErrorMessage ="A matrícula deve conter exatamente 7 dígitos.")]
    int? NumeroDeMatricula,

    [Required(ErrorMessage = "O campo \"Nome\" é obrigatório.")]
    [StringLength(100, MinimumLength = 3,
        ErrorMessage = "O campo \"Nome\" deve ter entre 2 e 100 caracteres")]
    string? Nome,

    Guid TurmaId,

    List<SelecionarTurmaViewModel>? TurmasDisponiveis
);

public record ExcluirAlunoViewModel(
    Guid Id,
    string Nome
);