using CorrigindoSimuladoWebApp.Compartilhado.Infraestrutura.Arquivos;

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

        //incluir services.AddScoped<RepositorioMódulo>();

    }
}
