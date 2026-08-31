using CorrigindoSimuladoWebApp.Compartilhado.Apresentacao;
using CorrigindoSimuladoWebApp.Compartilhado.Infraestrutura;

var builder = WebApplication.CreateBuilder(args);
//Configurar a INFRAESTRUTURA (arquivos, banco de dados, logs, cachês, etc..)
builder.Services.AdicionarCamadaInfraEstrutura();

//Configura MVC / APRESENTAÇÃO
builder.Services.AdicionarCamadaApresentacao();

var app = builder.Build();

//Middleware
app.UseRouting();
app.MapDefaultControllerRoute();

app.UseStaticFiles();

app.Run();
