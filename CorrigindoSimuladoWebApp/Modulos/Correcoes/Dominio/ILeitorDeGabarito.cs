namespace CorrigindoSimuladoWebApp.Modulos.Correcoes.Dominio;

public interface ILeitorDeGabarito
{
    // Recebe a imagem e devolve o que conseguiu ler
    Task<ResultadoLeituraGabarito> LerImagemAsync(Stream imagem);
}

public class ResultadoLeituraGabarito
{
    public string NumeroMatricula { get; set; } = string.Empty;
    public string LetrasGabarito { get; set; } = string.Empty;
    public bool Sucesso { get; set; }
    public string MensagemErro { get; set; } = string.Empty;
}
