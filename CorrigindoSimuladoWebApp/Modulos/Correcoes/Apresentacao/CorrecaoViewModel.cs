using System.ComponentModel.DataAnnotations;

namespace CorrigindoSimuladoWebApp.Modulos.Correcoes.Apresentacao;

// ViewModel genérico para preencher Dropdowns (combobox)
public record SelecionarAlunoViewModel(int Id, string Nome);

public record ListarCorrecaoViewModel(
    int Id,
    string NomeProva,
    string NomeAluno,
    int NumeroAcertos
);

public record CadastrarCorrecaoViewModel(
    int ProvaId,
    string? NomeProva,
    int QuantidadeQuestoes,

    [Required(ErrorMessage = "Selecione um aluno.")]
    int? AlunoId,

    [Required(ErrorMessage = "O gabarito do aluno é obrigatório.")]
    [RegularExpression(@"^[a-eA-E\s]+$", ErrorMessage = "O gabarito deve conter apenas as letras A, B, C, D ou E e espaços.")]
    string? GabaritoAluno,

    List<SelecionarAlunoViewModel>? AlunosDisponiveis
);