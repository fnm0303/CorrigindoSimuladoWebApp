using CorrigindoSimuladoWebApp.Compartilhado.Infraestrutura.Arquivos;
using CorrigindoSimuladoWebApp.Modulos.Alunos.Infraestrutura;
using CorrigindoSimuladoWebApp.Modulos.Correcoes.Infraestrutura;
using CorrigindoSimuladoWebApp.Modulos.Provas.Infraestrutura;
using CorrigindoSimuladoWebApp.Modulos.Turmas.Infraestrutura;

namespace CorrigindoSimuladoWebApp.Compartilhado.Infraestrutura;

public static class InjecaoDeDependencia
{
    public static void AdicionarCamadaInfraEstrutura(this IServiceCollection services)
    {
        services.AddScoped(services =>
        {
            ContextoJson contexto = new ContextoJson();

            contexto.Carregar();

            return contexto;
        });

        services.AddScoped<RepositorioTurmaEmArquivo>();
        services.AddScoped<RepositorioAlunoEmArquivo>();
        services.AddScoped<RepositorioProvaEmArquivo>();
        services.AddScoped<RepositorioCorrecaoEmArquivo>();
    }
}
