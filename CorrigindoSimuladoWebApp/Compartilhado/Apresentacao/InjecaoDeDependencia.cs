namespace CorrigindoSimuladoWebApp.Compartilhado.Apresentacao;

public static class InjecaoDeDependencia
{
    public static void AdicionarCamadaApresentacao(this IServiceCollection services)
    {
        //Razor = CSHTML
        services.AddControllersWithViews().AddRazorOptions(options =>
        {
            //resetar o mecanismo de busca de Views
            options.ViewLocationFormats.Clear();

            //Configurar localização das views compartilhadas
            options.ViewLocationFormats.Add("/Compartilhado/Apresentacao/Views/{0}.cshtml");

            //Configurar localização das views dos módulos
            options.ViewLocationFormats.Add("/Modulos/{1}s/Apresentacao/Views/{0}.cshtml");
        });
    }
}
