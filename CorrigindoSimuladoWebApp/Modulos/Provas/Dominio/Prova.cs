using CorrigindoSimuladoWebApp.Compartilhado.Dominio;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Dominio;

namespace CorrigindoSimuladoWebApp.Modulos.Provas.Dominio;

public sealed class Prova : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;
    public int QuantidadeQuestoes { get; set; }
    public string GabaritoCorreto { get; set; } = string.Empty;
    public Turma Turma { get; set; } = null!;

    public Prova() { }

    public Prova(string nome, int quantidadeQuestoes, string gabaritoCorreto, Turma turma) : this()
    {
        Nome = nome;
        QuantidadeQuestoes = quantidadeQuestoes;
        GabaritoCorreto = gabaritoCorreto;
        Turma = turma;
    }
    public override void Atualizar(EntidadeBase entidadeAtualizada)
    {
        Prova provaAtualizada = (Prova)entidadeAtualizada;

        Nome = provaAtualizada.Nome;
        QuantidadeQuestoes = provaAtualizada.QuantidadeQuestoes;
        GabaritoCorreto = provaAtualizada.GabaritoCorreto;
        Turma = provaAtualizada.Turma;
    }
}
