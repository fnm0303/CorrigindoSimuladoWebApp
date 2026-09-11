using System.ComponentModel.DataAnnotations;

public record ListarProvaViewModel(
    int Id,
    string Nome,
    string NomeTurma
);

public record CadastrarProvaViewModel(
    [Required(ErrorMessage = "O nome da prova é obrigatório.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres.")]
    string? Nome,

    [Required(ErrorMessage = "A quantidade de questões é obrigatória.")]
    [Range(1, 100, ErrorMessage = "A prova deve ter entre 1 e 100 questões.")]
    int? QuantidadeQuestoes,

    [Required(ErrorMessage = "O gabarito é obrigatório.")]
    [RegularExpression(@"^[a-eA-E]+$", ErrorMessage = "O gabarito deve conter apenas as letras A, B, C, D ou E.")]
    string? GabaritoCorreto,

    [Required(ErrorMessage = "Selecione uma turma.")]
    int TurmaId,

    List<SelecionarTurmaViewModel>? TurmasDisponiveis
);

public record EditarProvaViewModel(
    int Id,

    [Required(ErrorMessage = "O nome da prova é obrigatório.")]
    string? Nome,

    [Required(ErrorMessage = "A quantidade de questões é obrigatória.")]
    [Range(1, 100, ErrorMessage = "A prova deve ter entre 1 e 100 questões.")]
    int? QuantidadeQuestoes,

    [Required(ErrorMessage = "O gabarito é obrigatório.")]
    string? GabaritoCorreto,

    [Required(ErrorMessage = "Selecione uma turma.")]
    int TurmaId,

    List<SelecionarTurmaViewModel>? TurmasDisponiveis
);