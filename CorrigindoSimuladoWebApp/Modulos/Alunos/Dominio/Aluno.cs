using CorrigindoSimuladoWebApp.Compartilhado.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Dominio;

namespace CorrigindoSimuladoWebApp.Modulos.Alunos.Dominio;

public sealed class Aluno : EntidadeBase
{
    public int NumeroDeMatricula { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Turma Turma { get; set; } = null!;
    public Aluno() { }

    public Aluno(int nroDeMatricula, string nome, Turma turma) : this()
    {
        NumeroDeMatricula = nroDeMatricula;
        Nome = nome;
        Turma = turma;
    }
    public override void Atualizar(EntidadeBase entidadeAtualizada)
    {
        Aluno alunoAtualizado = (Aluno)entidadeAtualizada;

        NumeroDeMatricula = alunoAtualizado.NumeroDeMatricula;
        Nome = alunoAtualizado.Nome;
        Turma = alunoAtualizado.Turma;
    }
}
