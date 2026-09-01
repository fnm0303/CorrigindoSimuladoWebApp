using CorrigindoSimuladoWebApp.Compartilhado.Dominio;

namespace CorrigindoSimuladoWebApp.Modulos.Turmas.Dominio;

public enum TipoCurso
{
    EnsinoFundamental,
    EnsinoMedio
}
public sealed class Turma : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;
    public TipoCurso Curso { get; set; }
    public int AnoLetivo { get; set; }

    public Turma() { }

    public Turma(string nome, TipoCurso curso, int anoLetivo) : this()
    {
        Nome = nome;
        Curso = curso;
        AnoLetivo = anoLetivo;
    }
    public override void Atualizar(EntidadeBase entidadeAtualizada)
    {
        Turma turmaAtualizada = (Turma)entidadeAtualizada;

        Nome = turmaAtualizada.Nome;
        Curso = turmaAtualizada.Curso;
        AnoLetivo = turmaAtualizada.AnoLetivo;
    }
}
