using CorrigindoSimuladoWebApp.Compartilhado.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Alunos.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Provas.Dominio;

namespace CorrigindoSimuladoWebApp.Modulos.Correcoes.Dominio;

public sealed class Correcao : EntidadeBase
{
    public Prova Prova { get; set; } = null!;
    public Aluno Aluno { get; set; } = null!;
    public string GabaritoAluno { get; set; } = string.Empty;
    public int NumeroAcertos { get; set; }

    public Correcao() { }

    public Correcao(Prova prova, Aluno aluno, string gabaritoAluno) : this()
    {
        Prova = prova;
        Aluno = aluno;

        // Padroniza a digitação do aluno (ex: "a b c" vira "ABC")
        GabaritoAluno = gabaritoAluno.Replace(" ", "").ToUpper().Trim();

        // Executa a regra de negócio de correção no momento da criação
        NumeroAcertos = CalcularAcertos();
    }

    private int CalcularAcertos()
    {
        int totalAcertos = 0;

        // Garante que o loop não quebre se o aluno deixar questões em branco
        int totalComparacao = Math.Min(Prova.GabaritoCorreto.Length, GabaritoAluno.Length);

        for (int i = 0; i < totalComparacao; i++)
        {
            if (Prova.GabaritoCorreto[i] == GabaritoAluno[i])
            {
                totalAcertos++;
            }
        }

        return totalAcertos;
    }

    public override void Atualizar(EntidadeBase entidadeAtualizada)
    {
        Correcao correcaoAtualizada = (Correcao)entidadeAtualizada;

        Prova = correcaoAtualizada.Prova;
        Aluno = correcaoAtualizada.Aluno;
        GabaritoAluno = correcaoAtualizada.GabaritoAluno;
        NumeroAcertos = correcaoAtualizada.NumeroAcertos;
    }
}
