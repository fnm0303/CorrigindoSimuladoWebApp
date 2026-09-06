using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using CorrigindoSimuladoWebApp.Compartilhado.Dominio;

namespace CorrigindoSimuladoWebApp.Modulos.Turmas.Dominio;

public enum TipoCurso
{
    [Display(Name = "Ensino Fundamental")]
    EnsinoFundamental,
    [Display(Name = "Ensino Médio")]
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
