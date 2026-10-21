using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CorrigindoSimuladoWebApp.Modulos.Correcoes.Dominio;
using OpenCvSharp;

namespace CorrigindoSimuladoWebApp.Modulos.Correcoes.Infraestrutura;

public class LeitorDeGabaritoOmr : ILeitorDeGabarito
{
    // ============================================================
    // TAMANHO PADRÃO DA FOLHA
    // ============================================================

    private const int LarguraPadrao = 1000;
    private const int AlturaPadrao = 1414;

    // ============================================================
    // MARCADORES DE REGISTRO
    // Devem ser impressos no formulário:
    // 10mm x 10mm
    // centro aproximadamente 10mm de cada borda.
    // ============================================================

    private const double MarcadorDestinoEsquerdaX = 67;
    private const double MarcadorDestinoDireitaX = 946;
    private const double MarcadorDestinoSuperiorY = 67;
    private const double MarcadorDestinoInferiorY = 1347;

    // ============================================================
    // CONFIGURAÇÃO DO FORMULÁRIO
    //
    // Estes valores foram calibrados para o modelo do gabarito
    // que você acabou de enviar.
    //
    // Como trabalhamos com imagem normalizada, não dependem da
    // resolução original da foto.
    // ============================================================

    // ------------------------------------------------------------
    // RESPOSTAS - COLUNA ESQUERDA
    // Questões 01 a 25
    // ------------------------------------------------------------

    private static readonly double[] RespostasEsquerdaX =
    {
        0.253,
        0.282,
        0.312,
        0.341,
        0.371
    };

    // ------------------------------------------------------------
    // RESPOSTAS - COLUNA DIREITA
    // Questões 26 a 50
    // ------------------------------------------------------------

    private static readonly double[] RespostasDireitaX =
    {
        0.620,
        0.650,
        0.680,
        0.710,
        0.740
    };

    // ------------------------------------------------------------
    // Primeira linha de respostas
    // ------------------------------------------------------------

    private const double RespostaPrimeiraY = 0.4111;

    // ------------------------------------------------------------
    // Espaçamento vertical aproximado do formulário
    //
    // Como a folha é corrigida geometricamente antes da leitura,
    // podemos trabalhar com uma grade regular.
    // ------------------------------------------------------------

    private const double RespostaStepY = 0.0208;

    // ============================================================
    // MATRÍCULA
    // ============================================================

    private static readonly double[] MatriculaX =
    {
        0.646,
        0.679,
        0.711,
        0.744,
        0.778,
        0.811,
        0.844
    };

    private const double MatriculaPrimeiraY = 0.1430;

    private const double MatriculaStepY = 0.0198;

    // ============================================================
    // SCORE OMR
    // ============================================================

    // Abaixo disso consideramos a alternativa vazia.
    private const double LimiteBranco = 0.18;

    // Acima disso existe uma marca potencial.
    private const double LimiteMarcacao = 0.30;

    // Diferença mínima entre a primeira e segunda alternativa.
    private const double MargemMinima = 0.07;

    // ============================================================
    // ROI DAS BOLINHAS
    // ============================================================

    private const int TamanhoRoi = 31;

    // Centro analisado
    private const int RaioCentro = 5;

    // Anel utilizado para medir o fundo local
    private const int RaioFundoInterno = 10;
    private const int RaioFundoExterno = 13;

    // ============================================================
    // MÉTODO PÚBLICO
    // ============================================================

    public async Task<ResultadoLeituraGabarito> LerImagemAsync(
        Stream streamImagem)
    {
        if (streamImagem == null)
            throw new ArgumentNullException(nameof(streamImagem));

        return await Task.Run(
            () => ProcessarImagemOpenCv(streamImagem));
    }

    // ============================================================
    // PROCESSAMENTO PRINCIPAL
    // ============================================================

    private ResultadoLeituraGabarito ProcessarImagemOpenCv(
        Stream stream)
    {
        var resultado = new ResultadoLeituraGabarito();

        try
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            byte[] imageBytes = memoryStream.ToArray();

            using Mat imagemOriginal = Cv2.ImDecode(
                imageBytes,
                ImreadModes.Color);

            if (imagemOriginal.Empty())
                throw new InvalidOperationException(
                    "Não foi possível abrir a imagem enviada.");

            // 1. Corrigir orientação.
            using Mat imagemOrientada =
                CorrigirOrientacao(imagemOriginal);

            // 2. Detectar marcadores e corrigir perspectiva.
            using Mat imagemNormalizada =
                NormalizarPagina(imagemOrientada);

            // 3. Criar imagem em escala de cinza.
            using Mat imagemCinza = new Mat();

            Cv2.CvtColor(
                imagemNormalizada,
                imagemCinza,
                ColorConversionCodes.BGR2GRAY);

            // 4. Criar debug colorido a partir da folha normalizada.
            using Mat imagemDebug = imagemNormalizada.Clone();

            // 5. Ler matrícula e respostas usando a imagem cinza.
            resultado.NumeroMatricula =
                LerMatricula(imagemCinza, imagemDebug);

            resultado.LetrasGabarito =
                Ler50Questoes(imagemCinza, imagemDebug);

            // 6. Salvar a imagem de depuração.
            Cv2.ImWrite(
                "debug_gabarito_desenhado.jpg",
                imagemDebug);

            resultado.Sucesso = true;
        }
        catch (Exception ex)
        {
            resultado.Sucesso = false;
            resultado.MensagemErro =
                $"Erro ao ler gabarito: {ex.Message}";
        }

        return resultado;
    }

    // ============================================================
    // ORIENTAÇÃO
    // ============================================================

    private Mat CorrigirOrientacao(Mat imagem)
    {
        if (imagem.Width <= imagem.Height)
        {
            return imagem.Clone();
        }

        using var transposta = new Mat();

        Cv2.Transpose(
            imagem,
            transposta);

        var resultado = new Mat();

        Cv2.Flip(
            transposta,
            resultado,
            FlipMode.Y);

        return resultado;
    }

    // ============================================================
    // NORMALIZAR PÁGINA
    // ============================================================

    private Mat NormalizarPagina(Mat imagem)
    {
        // --------------------------------------------------------
        // Primeiro tentamos os 4 marcadores.
        // --------------------------------------------------------

        using Mat cinza = new Mat();

        Cv2.CvtColor(
            imagem,
            cinza,
            ColorConversionCodes.BGR2GRAY);

        if (!TentarEncontrarMarcadores(
                cinza,
                out Point2f[] marcadores))
        {
            throw new InvalidOperationException(
                "Não foi possível detectar os quatro marcadores " +
                "de registro. Verifique se os quatro aparecem " +
                "na imagem e se a folha inteira está enquadrada.");
        }

        return AplicarPerspectivaPorMarcadores(imagem, marcadores);

    }

    // ============================================================
    // DETECTAR OS 4 MARCADORES
    // ============================================================

    private bool TentarEncontrarMarcadores(
        Mat cinza,
        out Point2f[] pontos)
    {
        pontos = Array.Empty<Point2f>();

        using var suavizada = new Mat();

        Cv2.GaussianBlur(
            cinza,
            suavizada,
            new Size(5, 5),
            0);

        using var binarizada = new Mat();

        Cv2.Threshold(
            suavizada,
            binarizada,
            0,
            255,
            ThresholdTypes.BinaryInv |
            ThresholdTypes.Otsu);

        Cv2.FindContours(
            binarizada,
            out Point[][] contornos,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        var candidatos = new List<MarcadorCandidato>();

        double areaImagem =
            cinza.Width *
            cinza.Height;

        foreach (Point[] contorno in contornos)
        {
            double area = Math.Abs(Cv2.ContourArea(contorno));

            if (area <= 0)
                continue;

            double proporcaoArea = area / areaImagem;

            // Os marcadores precisam ser pequenos em relação
            // à página, mas não microscópicos.
            if (proporcaoArea < 0.0002 || proporcaoArea > 0.01)
                continue;


            Rect rect = Cv2.BoundingRect(contorno);

            if (rect.Width <= 0 || rect.Height <= 0)
            {
                continue;
            }

            double proporcao = (double)rect.Width / rect.Height;

            // Deve se parecer com um quadrado.
            if (proporcao < 0.70 || proporcao > 1.30)
                continue;


            double preenchimento = area / (rect.Width * rect.Height);

            if (preenchimento < 0.50)
                continue;

            Point2f centro =
                new Point2f(
                    rect.X + rect.Width / 2.0f,
                    rect.Y + rect.Height / 2.0f);

            candidatos.Add(new MarcadorCandidato
            {
                Centro = centro,
                Area = area,
                Preenchimento = preenchimento
            });
        }

        if (candidatos.Count < 4)
            return false;

        double margemX = cinza.Width * 0.08;
        double margemY = cinza.Height * 0.08;

        MarcadorCandidato? superiorEsquerdo =
            MelhorMarcador(
                candidatos,
                0,
                0);

        MarcadorCandidato? superiorDireito =
            MelhorMarcador(
                candidatos,
                cinza.Width - 1,
                0);

        MarcadorCandidato? inferiorDireito =
            MelhorMarcador(
                candidatos,
                cinza.Width - 1,
                cinza.Height - 1);

        MarcadorCandidato? inferiorEsquerdo =
            MelhorMarcador(
                candidatos,
                0,
                cinza.Height - 1);

        if (superiorEsquerdo == null ||
            superiorDireito == null ||
            inferiorDireito == null ||
            inferiorEsquerdo == null)
        {
            return false;
        }

        // Os quatro pontos precisam ser distintos.
        var lista = new[]
        {
            superiorEsquerdo,
            superiorDireito,
            inferiorDireito,
            inferiorEsquerdo
        };

        if (lista
            .GroupBy(x =>
                $"{Math.Round(x.Centro.X)}-" +
                $"{Math.Round(x.Centro.Y)}")
            .Count() != 4)
        {
            return false;
        }

        // Confirma a ordem geométrica dos pontos.
        if (superiorEsquerdo.Centro.X >=
                superiorDireito.Centro.X ||
            inferiorEsquerdo.Centro.X >=
                inferiorDireito.Centro.X ||
            superiorEsquerdo.Centro.Y >=
                inferiorEsquerdo.Centro.Y ||
            superiorDireito.Centro.Y >=
                inferiorDireito.Centro.Y)
        {
            return false;
        }

        pontos = new[]
        {
            superiorEsquerdo.Centro,
            superiorDireito.Centro,
            inferiorDireito.Centro,
            inferiorEsquerdo.Centro
        };

        return true;
    }

    // ============================================================
    // ENCONTRAR MELHOR MARCADOR PARA UM CANTO
    // ============================================================

    private MarcadorCandidato? MelhorMarcador(
        List<MarcadorCandidato> candidatos,
        double alvoX,
        double alvoY)
    {
        MarcadorCandidato? melhor = null;

        double menorDistancia = double.MaxValue;

        foreach (MarcadorCandidato candidato
                 in candidatos)
        {
            double dx = candidato.Centro.X - alvoX;

            double dy = candidato.Centro.Y - alvoY;

            double distancia =
                Math.Sqrt(
                    dx * dx +
                    dy * dy);

            if (distancia < menorDistancia)
            {
                menorDistancia = distancia;

                melhor = candidato;
            }
        }

        return melhor;
    }

    // ============================================================
    // PERSPECTIVA USANDO MARCADORES
    // ============================================================

    private Mat AplicarPerspectivaPorMarcadores(
        Mat imagem,
        Point2f[] marcadores)
    {
        Point2f[] destino =
        {
            // Superior esquerdo
            new Point2f(
                (float)MarcadorDestinoEsquerdaX,
                (float)MarcadorDestinoSuperiorY),

            // Superior direito
            new Point2f(
                (float)MarcadorDestinoDireitaX,
                (float)MarcadorDestinoSuperiorY),

            // Inferior direito
            new Point2f(
                (float)MarcadorDestinoDireitaX,
                (float)MarcadorDestinoInferiorY),

            // Inferior esquerdo
            new Point2f(
                (float)MarcadorDestinoEsquerdaX,
                (float)MarcadorDestinoInferiorY)
        };

        return AplicarTransformacaoPerspectiva(
            imagem,
            marcadores,
            destino);
    }

    // ============================================================
    // PERSPECTIVA GENÉRICA
    // ============================================================

    private Mat AplicarPerspectiva(
        Mat imagem,
        Point2f[] pontos)
    {
        Point2f[] destino =
        {
            new Point2f(0, 0),

            new Point2f(
                LarguraPadrao - 1,
                0),

            new Point2f(
                LarguraPadrao - 1,
                AlturaPadrao - 1),

            new Point2f(
                0,
                AlturaPadrao - 1)
        };

        return AplicarTransformacaoPerspectiva(
            imagem,
            pontos,
            destino);
    }

    private Mat AplicarTransformacaoPerspectiva(
        Mat imagem,
        Point2f[] origem,
        Point2f[] destino)
    {
        using Mat matriz =
            Cv2.GetPerspectiveTransform(
                origem,
                destino);

        var resultado = new Mat();

        Cv2.WarpPerspective(
            imagem,
            resultado,
            matriz,
            new Size(
                LarguraPadrao,
                AlturaPadrao),
            InterpolationFlags.Linear,
            BorderTypes.Constant,
            Scalar.White);

        return resultado;
    }

    // ============================================================
    // TENTAR ENCONTRAR CANTOS DA FOLHA
    // ============================================================

    private bool TentarEncontrarCantosFolha(
        Mat cinza,
        out Point2f[] pontosOrdenados)
    {
        pontosOrdenados = Array.Empty<Point2f>();

        using var suavizada = new Mat();

        Cv2.GaussianBlur(
            cinza,
            suavizada,
            new Size(5, 5),
            0);

        using var bordas = new Mat();

        Cv2.Canny(
            suavizada,
            bordas,
            50,
            150);

        Cv2.FindContours(
            bordas,
            out Point[][] contornos,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple);

        double areaImagem = cinza.Width * cinza.Height;

        double maiorArea = 0;

        Point2f[]? melhor = null;

        foreach (Point[] contorno in contornos)
        {
            if (contorno.Length < 4)
                continue;

            double area =
                Math.Abs(
                    Cv2.ContourArea(contorno));

            if (area <
                areaImagem * 0.55)
            {
                continue;
            }

            double perimetro =
                Cv2.ArcLength(
                    contorno,
                    true);

            Point[] aproximado =
                Cv2.ApproxPolyDP(
                    contorno,
                    0.02 *
                    perimetro,
                    true);

            if (aproximado.Length != 4)
                continue;

            if (area <= maiorArea)
                continue;

            maiorArea = area;

            melhor = aproximado
                    .Select(
                        p => new Point2f(
                            p.X,
                            p.Y))
                    .ToArray();
        }

        if (melhor == null)
            return false;

        pontosOrdenados = OrdenarPontos(melhor);

        return true;
    }

    // ============================================================
    // ORDENAR PONTOS
    // TL, TR, BR, BL
    // ============================================================

    private Point2f[] OrdenarPontos(
        Point2f[] pontos)
    {
        var resultado = new Point2f[4];

        resultado[0] =
            pontos
                .OrderBy(
                    p => p.X + p.Y)
                .First();

        resultado[2] =
            pontos
                .OrderByDescending(
                    p => p.X + p.Y)
                .First();

        resultado[1] =
            pontos
                .OrderBy(
                    p => p.Y - p.X)
                .First();

        resultado[3] =
            pontos
                .OrderByDescending(
                    p => p.Y - p.X)
                .First();

        return resultado;
    }

    // ============================================================
    // MATRÍCULA
    // ============================================================

    private string LerMatricula(
    Mat imagemCinza,
    Mat imagemDebug)
    {
        char[] matricula = new char[MatriculaX.Length];

        for (int col = 0; col < MatriculaX.Length; col++)
        {
            double centroX =
                MatriculaX[col] * imagemCinza.Cols;

            double maiorScore = -1;
            double segundoMaiorScore = -1;
            int melhorDigito = -1;

            for (int digito = 0; digito <= 9; digito++)
            {
                double yNormalizado =
                    MatriculaPrimeiraY +
                    digito * MatriculaStepY;

                double centroY =
                    yNormalizado * imagemCinza.Rows;

                double score = CalcularScorePreenchimento(
                    imagemCinza,
                    centroX,
                    centroY);

                int x = (int)Math.Round(centroX);
                int y = (int)Math.Round(centroY);

                Cv2.Circle(
                    imagemDebug,
                    new Point(x, y),
                    5,
                    new Scalar(0, 0, 255),
                    1);

                if (score > maiorScore)
                {
                    segundoMaiorScore = maiorScore;
                    maiorScore = score;
                    melhorDigito = digito;
                }
                else if (score > segundoMaiorScore)
                {
                    segundoMaiorScore = score;
                }
            }

            if (melhorDigito < 0 ||
                maiorScore < LimiteMarcacao)
            {
                matricula[col] = '?';
            }
            else if (
                maiorScore - segundoMaiorScore < MargemMinima)
            {
                matricula[col] = '?';
            }
            else
            {
                matricula[col] = (char)('0' + melhorDigito);
            }
        }

        return new string(matricula);
    }

    private string Ler50Questoes(
    Mat imagemCinza,
    Mat imagemDebug)
    {
        char[] respostas = new char[50];

        const string alternativas = "ABCDE";

        for (int q = 0; q < 25; q++)
        {
            double yNormalizado =
                RespostaPrimeiraY + q * RespostaStepY;

            double centroY =
                yNormalizado * imagemCinza.Rows;

            respostas[q] = IdentificarAlternativa(
                imagemCinza,
                imagemDebug,
                RespostasEsquerdaX,
                centroY,
                alternativas);

            respostas[q + 25] = IdentificarAlternativa(
                imagemCinza,
                imagemDebug,
                RespostasDireitaX,
                centroY,
                alternativas);
        }

        return new string(respostas);
    }

    private char IdentificarAlternativa(
        Mat imagemCinza,
        Mat imagemDebug,
        double[] coordenadasX,
        double centroY,
        string alternativas)
    {
        double[] scores = new double[coordenadasX.Length];

        for (int i = 0; i < coordenadasX.Length; i++)
        {
            double centroX =
                coordenadasX[i] * imagemCinza.Cols;

            scores[i] = CalcularScorePreenchimento(
                imagemCinza,
                centroX,
                centroY);

            int x = (int)Math.Round(centroX);
            int y = (int)Math.Round(centroY);

            // Marca o centro estimado da bolinha.
            Cv2.Circle(
                imagemDebug,
                new Point(x, y),
                RaioCentro,
                new Scalar(0, 255, 0),
                1);

            // Contorno externo da região analisada.
            Cv2.Rectangle(
                imagemDebug,
                new Rect(
                    x - TamanhoRoi / 2,
                    y - TamanhoRoi / 2,
                    TamanhoRoi,
                    TamanhoRoi),
                new Scalar(0, 255, 0),
                1);
        }

        int melhor = Array.IndexOf(scores, scores.Max());

        if (melhor < 0 ||
            scores[melhor] < LimiteMarcacao)
        {
            return ' ';
        }

        double segundoMaior = scores
            .Where((score, indice) => indice != melhor)
            .DefaultIfEmpty(0)
            .Max();

        if (scores[melhor] - segundoMaior < MargemMinima)
        {
            // Duas alternativas com scores muito próximos:
            // marcação ambígua.
            return '?';
        }

        return alternativas[melhor];
    }
    // ============================================================
    // SCORE DE PREENCHIMENTO
    //
    // Não contamos a borda da bolinha.
    //
    // Centro:
    //     mede o preenchimento.
    //
    // Anel:
    //     mede o fundo local.
    //
    // Assim funciona melhor com lápis, caneta, sombra e fotografia.
    // ============================================================

    private double CalcularScorePreenchimento(
        Mat imagem,
        double centroX,
        double centroY)
    {
        int cx = (int)Math.Round(centroX);

        int cy = (int)Math.Round(centroY);

        int metade = TamanhoRoi / 2;

        int x = cx - metade;

        int y = cy - metade;

        if (
            x < 0 ||
            y < 0 ||
            x + TamanhoRoi >
                imagem.Width ||
            y + TamanhoRoi >
                imagem.Height)
        {
            return 0;
        }

        using var roi = new Mat(
                imagem,
                new Rect(
                    x,
                    y,
                    TamanhoRoi,
                    TamanhoRoi));

        // --------------------------------------------------------
        // Máscara do centro
        // --------------------------------------------------------

        using var mascaraCentro = new Mat(
                TamanhoRoi,
                TamanhoRoi,
                MatType.CV_8UC1,
                Scalar.Black);

        Cv2.Circle(
            mascaraCentro,
            new Point(
                metade,
                metade),
            RaioCentro,
            Scalar.White,
            -1);

        // --------------------------------------------------------
        // Máscara do fundo
        //
        // Primeiro cria um círculo maior.
        // Depois remove a região interna.
        // --------------------------------------------------------

        using var mascaraFundo =
            new Mat(
                TamanhoRoi,
                TamanhoRoi,
                MatType.CV_8UC1,
                Scalar.Black);

        Cv2.Circle(
            mascaraFundo,
            new Point(
                metade,
                metade),
            RaioFundoExterno,
            Scalar.White,
            -1);

        Cv2.Circle(
            mascaraFundo,
            new Point(
                metade,
                metade),
            RaioFundoInterno,
            Scalar.Black,
            -1);

        // --------------------------------------------------------
        // Fundo local
        // --------------------------------------------------------

        double fundo =
            Cv2.Mean(
                roi,
                mascaraFundo)
                .Val0;

        // --------------------------------------------------------
        // Centro
        // --------------------------------------------------------

        double centro =
            Cv2.Mean(
                roi,
                mascaraCentro)
                .Val0;

        if (fundo <= 1)
            return 0;

        // --------------------------------------------------------
        // Quanto mais escuro o centro em relação ao fundo,
        // maior o preenchimento.
        // --------------------------------------------------------

        double score = (fundo - centro) / fundo;

        if (score < 0)
            score = 0;

        if (score > 1)
            score = 1;

        return score;
    }

    // ============================================================
    // CANDIDATO A MARCADOR
    // ============================================================

    private sealed class MarcadorCandidato
    {
        public Point2f Centro { get; set; }

        public double Area { get; set; }

        public double Preenchimento { get; set; }
    }
}