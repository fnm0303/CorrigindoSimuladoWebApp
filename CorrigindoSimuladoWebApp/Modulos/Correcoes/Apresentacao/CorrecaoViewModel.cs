using System.ComponentModel.DataAnnotations;

namespace CorrigindoSimuladoWebApp.Modulos.Correcoes.Apresentacao;

// ViewModel genérico para preencher Dropdowns (combobox)
public record SelecionarItemViewModel(Guid Id, string Nome);

public record ListarCorrecaoViewModel(
    Guid Id,
    string NomeProva,
    string NomeAluno,
    int NumeroAcertos
);

public record CadastrarCorrecaoViewModel(
    Guid ProvaId,
    string? NomeProva,
    int QuantidadeQuestoes,

    [Required(ErrorMessage = "Selecione um aluno.")]
    Guid? AlunoId,

    [Required(ErrorMessage = "O gabarito do aluno é obrigatório.")]
    [RegularExpression(@"^[a-eA-E\s]+$", ErrorMessage = "O gabarito deve conter apenas as letras A, B, C, D ou E e espaços.")]
    string? GabaritoAluno,

    List<SelecionarItemViewModel>? AlunosDisponiveis
);

// ViewModel que representa cada linha da tabela de detalhes
public record DetalheQuestaoViewModel(
    int NumeroQuestao,
    char RespostaAluno,
    char RespostaCorreta,
    bool Acertou
);

// ViewModel que empacota tudo para mandar para a tela
public record DetalhesCorrecaoViewModel(
    Guid CorrecaoId,
    string NomeProva,
    string NomeAluno,
    int Acertos,
    int TotalQuestoes,
    List<DetalheQuestaoViewModel> Questoes
);

public record ListarProvasCorrigidasViewModel(
    Guid Id,
    string Nome,
    string NomeTurma
);

public record ExcluirCorrecaoViewModel(
    Guid Id,
    string NomeAluno
);