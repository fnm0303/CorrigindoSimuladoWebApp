using CorrigindoSimuladoWebApp.Modulos.Correcoes.Dominio;
using OpenCvSharp;

namespace CorrigindoSimuladoWebApp.Modulos.Correcoes.Infraestrutura;

public class LeitorDeGabaritoOmr : ILeitorDeGabarito
{
    // ============================================================
    // CONFIGURAÇÕES GERAIS
    // ============================================================

    private const int LarguraPadrao = 1000;
    private const int AlturaPadrao = 1414;

    // Área analisada no interior de cada bolinha.
    private const int TamanhoRoi = 19;
    private const int RaioMascara = 6;

    // Busca pequenos deslocamentos causados por foto/scanner.
    private const int BuscaXY = 2;
    private const int BuscaYQuestao = 0;

    // Limites de segurança para considerar uma marca válida.
    private const double MinScoreQuestao = 0.45;
    private const double MinContrasteQuestao = 0.12;
    private const double MinMargemQuestao = 0.12;

    private const double MinScoreMatricula = 0.45;
    private const double MinMargemMatricula = 0.10;

    private const double InclinacaoXPorLinha = -0.50;

    // ============================================================
    // COORDENADAS DA FOLHA PADRÃO 1000 x 1414
    //
    // IMPORTANTE:
    // Trabalhamos com o CENTRO das bolinhas, e não com o canto
    // superior esquerdo do Rect.
    // ============================================================

    // Questões 01-25
    private static readonly double[] XQuestoesEsquerda =
    {
        274, 303, 332, 359, 387
    };

    // Questões 26-50
    private static readonly double[] XQuestoesDireita =
    {
        622, 648, 677, 705, 733
    };

    // Centro vertical da primeira questão.
    private const double YPrimeiraQuestaoEsquerda = 580.0;
    private const double YPrimeiraQuestaoDireita = 584.5;

    // Espaçamento médio real da folha.
    // Não usamos mais 30.
    private const double StepYQuestoes = 28.1;

    // Matrícula - 7 colunas
    private static readonly double[] XMatricula =
    {
        714, 745, 776, 806, 837, 868, 899
    };

    // Linha 0 da matrícula.
    private const double YPrimeiraMatricula = 171.0;

    // Espaçamento vertical aproximado.
    private const double StepYMatricula = 26.6;


    // ============================================================
    // MÉTODO PÚBLICO
    // ============================================================

    public async Task<ResultadoLeituraGabarito> LerImagemAsync(Stream streamImagem)
    {
        if (streamImagem == null)
            throw new ArgumentNullException(nameof(streamImagem));

        // Mantemos a assinatura assíncrona da arquitetura atual.
        return await Task.Run(() => ProcessarImagemOpenCv(streamImagem));
    }


    // ============================================================
    // PROCESSAMENTO PRINCIPAL
    // ============================================================

    private ResultadoLeituraGabarito ProcessarImagemOpenCv(Stream stream)
    {
        var resultado = new ResultadoLeituraGabarito();

        try
        {
            // ----------------------------------------------------
            // 1. Ler Stream
            // ----------------------------------------------------

            using var memoryStream = new MemoryStream();

            stream.CopyTo(memoryStream);

            byte[] imageBytes = memoryStream.ToArray();

            using Mat imagemOriginal =
                Cv2.ImDecode(imageBytes, ImreadModes.Color);

            if (imagemOriginal.Empty())
                throw new Exception("Não foi possível decodificar a imagem recebida.");


            // ----------------------------------------------------
            // 2. Corrigir orientação/perspectiva e padronizar
            // ----------------------------------------------------

            using Mat imagemNormalizada =
                NormalizarImagem(imagemOriginal);


            // ----------------------------------------------------
            // 3. Converter para escala de cinza
            // ----------------------------------------------------

            using Mat imagemCinzaOriginal = new Mat();

            Cv2.CvtColor(
                imagemNormalizada,
                imagemCinzaOriginal,
                ColorConversionCodes.BGR2GRAY
            );


            // ----------------------------------------------------
            // 4. Corrigir polaridade
            //
            // Esperamos folha clara com tinta escura.
            // Caso a imagem venha como negativo, invertimos.
            // ----------------------------------------------------

            using Mat imagemCinza = CorrigirPolaridade(
                imagemCinzaOriginal
            );


            // ----------------------------------------------------
            // 5. Melhorar contraste local
            //
            // Ajuda em fotos onde uma parte da folha está mais
            // escura ou mais clara.
            // ----------------------------------------------------

            using Mat imagemPreparada = MelhorarContraste(
                imagemCinza
            );


            // ----------------------------------------------------
            // 6. Matrícula
            // ----------------------------------------------------

            resultado.NumeroMatricula =
                LerMatricula(imagemPreparada);


            // ----------------------------------------------------
            // 7. Questões 01-50
            // ----------------------------------------------------

            resultado.LetrasGabarito =
                Ler50Questoes(imagemPreparada);


            resultado.Sucesso = true;

            return resultado;
        }
        catch (Exception ex)
        {
            resultado.Sucesso = false;

            resultado.MensagemErro =
                $"Erro ao ler gabarito: {ex.Message}";

            return resultado;
        }
    }


    // ============================================================
    // NORMALIZAÇÃO DA IMAGEM
    // ============================================================

    private Mat NormalizarImagem(Mat imagemOriginal)
    {
        using Mat imagemParaProcessar = imagemOriginal.Clone();

        Mat imagemOrientada = imagemParaProcessar.Clone();

        // --------------------------------------------------------
        // Tenta encontrar os quatro cantos da folha.
        //
        // Se conseguir, aplica transformação de perspectiva.
        // Se não conseguir, cai para Resize padrão.
        // --------------------------------------------------------

        try
        {
            using Mat cinza = new Mat();

            Cv2.CvtColor(
                imagemOrientada,
                cinza,
                ColorConversionCodes.BGR2GRAY
            );

            if (TentarEncontrarFolha(cinza, out Point2f[] pontos))
            {
                Point2f[] destino =
                {
                    new Point2f(0, 0),
                    new Point2f(LarguraPadrao - 1, 0),
                    new Point2f(LarguraPadrao - 1, AlturaPadrao - 1),
                    new Point2f(0, AlturaPadrao - 1)
                };

                using Mat matriz =
                    Cv2.GetPerspectiveTransform(pontos, destino);

                var resultado = new Mat();

                Cv2.WarpPerspective(
                    imagemOrientada,
                    resultado,
                    matriz,
                    new Size(LarguraPadrao, AlturaPadrao),
                    InterpolationFlags.Linear,
                    BorderTypes.Constant,
                    Scalar.White
                );

                return resultado;
            }

            // ----------------------------------------------------
            // Fallback:
            // se não detectar os quatro cantos, usamos o mesmo
            // padrão de tamanho da sua aplicação.
            // ----------------------------------------------------

            var redimensionada = new Mat();

            Cv2.Resize(
                imagemOrientada,
                redimensionada,
                new Size(LarguraPadrao, AlturaPadrao),
                0,
                0,
                InterpolationFlags.Area
            );

            return redimensionada;
        }
        finally
        {
            imagemOrientada.Dispose();
        }
    }


    // ============================================================
    // DETECTAR OS QUATRO CANTOS DA FOLHA
    // ============================================================

    private bool TentarEncontrarFolha(
        Mat cinza,
        out Point2f[] pontosOrdenados)
    {
        pontosOrdenados = Array.Empty<Point2f>();

        using var suavizada = new Mat();

        Cv2.GaussianBlur(
            cinza,
            suavizada,
            new Size(5, 5),
            0
        );

        using var bordas = new Mat();

        Cv2.Canny(
            suavizada,
            bordas,
            50,
            150
        );

        Cv2.FindContours(
            bordas,
            out Point[][] contornos,
            out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple
        );

        double areaImagem =
            cinza.Width * cinza.Height;

        double maiorArea = 0;

        Point2f[]? melhor = null;

        foreach (Point[] contorno in contornos)
        {
            if (contorno.Length < 4)
                continue;

            double area = Math.Abs(
                Cv2.ContourArea(contorno)
            );

            // Ignora retângulos pequenos:
            // caixas de nome, matrícula etc.
            if (area < areaImagem * 0.60)
                continue;

            double perimetro =
                Cv2.ArcLength(contorno, true);

            Point[] aproximado =
                Cv2.ApproxPolyDP(
                    contorno,
                    0.02 * perimetro,
                    true
                );

            if (aproximado.Length != 4)
                continue;

            if (area <= maiorArea)
                continue;

            maiorArea = area;

            melhor = aproximado
                .Select(p => new Point2f(p.X, p.Y))
                .ToArray();
        }

        if (melhor == null)
            return false;

        pontosOrdenados =
            OrdenarPontos(melhor);

        return true;
    }


    // ============================================================
    // ORDENAR CANTOS:
    // TL, TR, BR, BL
    // ============================================================

    private Point2f[] OrdenarPontos(Point2f[] pontos)
    {
        var resultado = new Point2f[4];

        Point2f superiorEsquerdo =
            pontos.OrderBy(p => p.X + p.Y).First();

        Point2f inferiorDireito =
            pontos.OrderByDescending(p => p.X + p.Y).First();

        Point2f superiorDireito =
            pontos.OrderBy(p => p.Y - p.X).First();

        Point2f inferiorEsquerdo =
            pontos.OrderByDescending(p => p.Y - p.X).First();

        resultado[0] = superiorEsquerdo;
        resultado[1] = superiorDireito;
        resultado[2] = inferiorDireito;
        resultado[3] = inferiorEsquerdo;

        return resultado;
    }


    // ============================================================
    // POLARIDADE
    // ============================================================

    private Mat CorrigirPolaridade(Mat imagemCinza)
    {
        var resultado = imagemCinza.Clone();

        double media =
            Cv2.Mean(imagemCinza).Val0;

        // Folha normal:
        // fundo claro -> média alta
        //
        // Imagem negativa:
        // fundo escuro -> média baixa
        //
        // Após normalização, essa heurística funciona bem
        // para o formulário inteiro.

        if (media < 127)
        {
            Cv2.BitwiseNot(
                resultado,
                resultado
            );
        }

        return resultado;
    }


    // ============================================================
    // CONTRASTE
    // ============================================================

    private Mat MelhorarContraste(Mat imagemCinza)
    {
        var resultado = new Mat();

        using var clahe =
            Cv2.CreateCLAHE(
                clipLimit: 2.0,
                tileGridSize: new Size(8, 8)
            );

        clahe.Apply(
            imagemCinza,
            resultado
        );

        return resultado;
    }


    // ============================================================
    // MATRÍCULA
    // ============================================================

    private string LerMatricula(Mat imagem)
    {
        char[] matricula = new char[7];

        for (int coluna = 0; coluna < 7; coluna++)
        {
            double x = XMatricula[coluna];

            double melhorScore = -1;

            int melhorDigito = -1;

            double segundoMelhorScore = -1;

            // ----------------------------------------------------
            // Cada coluna possui os números 0 a 9.
            // ----------------------------------------------------

            for (int digito = 0; digito <= 9; digito++)
            {
                double y =
                    YPrimeiraMatricula +
                    (digito * StepYMatricula);

                double score =
                    ObterMelhorScoreNaVizinhanca(
                        imagem,
                        x,
                        y,
                        BuscaXY
                    );

                if (score > melhorScore)
                {
                    segundoMelhorScore = melhorScore;

                    melhorScore = score;

                    melhorDigito = digito;
                }
                else if (score > segundoMelhorScore)
                {
                    segundoMelhorScore = score;
                }
            }


            // ----------------------------------------------------
            // Segurança:
            // não aceita um dígito apenas porque ele tem o maior
            // score. Ele precisa ter uma marca realmente forte.
            // ----------------------------------------------------

            bool marcaValida =
                melhorDigito >= 0 &&
                melhorScore >= MinScoreMatricula &&
                (melhorScore - segundoMelhorScore)
                    >= MinMargemMatricula;

            if (marcaValida)
            {
                matricula[coluna] =
                    (char)('0' + melhorDigito);
            }
            else
            {
                // ? significa:
                // não foi possível determinar com segurança.
                //
                // Isso é propositalmente melhor do que colocar
                // "0" automaticamente.
                matricula[coluna] = '?';
            }
        }

        return new string(matricula);
    }


    // ============================================================
    // 50 QUESTÕES
    // ============================================================

    private string Ler50Questoes(Mat imagem)
    {
        char[] gabarito = new char[50];

        const string alternativas = "ABCDE";

        // ============================================================
        // QUESTÕES 01-25
        // ============================================================

        for (int q = 0; q < 25; q++)
        {
            double y =
                YPrimeiraQuestaoEsquerda +
                (q * StepYQuestoes);

            // Corrige a inclinação horizontal da folha.
            double deslocamentoX =
                q * InclinacaoXPorLinha;

            double[] xCorrigidos =
                XQuestoesEsquerda
                    .Select(x => x + deslocamentoX)
                    .ToArray();

            gabarito[q] =
                IdentificarAlternativa(
                    imagem,
                    xCorrigidos,
                    y,
                    alternativas
                );
        }


        // ============================================================
        // QUESTÕES 26-50
        // ============================================================

        for (int q = 0; q < 25; q++)
        {
            double y =
                YPrimeiraQuestaoDireita +
                (q * StepYQuestoes);

            double deslocamentoX =
                q * InclinacaoXPorLinha;

            double[] xCorrigidos =
                XQuestoesDireita
                    .Select(x => x + deslocamentoX)
                    .ToArray();

            gabarito[q + 25] =
                IdentificarAlternativa(
                    imagem,
                    xCorrigidos,
                    y,
                    alternativas
                );
        }

        return new string(gabarito);
    }


    // ============================================================
    // IDENTIFICAR ALTERNATIVA
    // ============================================================

    private char IdentificarAlternativa(
    Mat imagem,
    double[] xAlternativas,
    double yNominal,
    string alternativas)
    {
        double[] scores = new double[5];

        // ============================================================
        // Calcula um score robusto para cada alternativa.
        //
        // Não procuramos "o melhor pixel".
        // Fazemos uma média de uma pequena vizinhança.
        //
        // Isso evita que a borda impressa da bolinha gere falso
        // positivo.
        // ============================================================

        for (int i = 0; i < 5; i++)
        {
            scores[i] =
                ObterMelhorScoreNaVizinhanca(
                    imagem,
                    xAlternativas[i],
                    yNominal,
                    BuscaXY
                );
        }

        // ============================================================
        // Maior score
        // ============================================================

        int indiceMaior = 0;

        double maiorScore = scores[0];
        double segundoMaior = 0;

        for (int i = 1; i < scores.Length; i++)
        {
            if (scores[i] > maiorScore)
            {
                segundoMaior = maiorScore;
                maiorScore = scores[i];
                indiceMaior = i;
            }
            else if (scores[i] > segundoMaior)
            {
                segundoMaior = scores[i];
            }
        }

        // ============================================================
        // Mediana das 5 alternativas
        // ============================================================

        double mediana =
            CalcularMediana(scores);

        double contraste =
            maiorScore - mediana;

        double margem =
            maiorScore - segundoMaior;

        // ============================================================
        // REGRA 1:
        // Questão claramente em branco.
        //
        // Na sua folha vazia existe a impressão das letras/bordas,
        // mas não existe preenchimento real no centro.
        // ============================================================

        const double limiteBranco = 0.32;

        if (maiorScore < limiteBranco)
        {
            return ' ';
        }

        // ============================================================
        // REGRA 2:
        // Score ainda baixo e sem contraste suficiente.
        //
        // Isso também deve ser considerado vazio, em vez de "?".
        // ============================================================

        if (maiorScore < MinScoreQuestao &&
            contraste < MinContrasteQuestao)
        {
            return ' ';
        }

        // ============================================================
        // REGRA 3:
        // Existe alguma marca, mas ela é fraca.
        // ============================================================

        if (maiorScore < MinScoreQuestao)
        {
            return '?';
        }

        // ============================================================
        // REGRA 4:
        // Existem duas alternativas muito próximas.
        //
        // Exemplo:
        // A = 0.83
        // B = 0.79
        //
        // Não devemos adivinhar.
        // ============================================================

        if (margem < MinMargemQuestao)
        {
            return '?';
        }

        // ============================================================
        // RESPOSTA IDENTIFICADA
        // ============================================================

        return alternativas[indiceMaior];
    }

    // ============================================================
    // BUSCAR MELHOR SCORE AO REDOR DA COORDENADA
    // ============================================================

    private double ObterMelhorScoreNaVizinhanca(
    Mat imagem,
    double x,
    double y,
    int busca)
    {
        double soma = 0;
        int quantidade = 0;

        int centroX = (int)Math.Round(x);
        int centroY = (int)Math.Round(y);

        for (int dx = -busca; dx <= busca; dx++)
        {
            for (int dy = -busca; dy <= busca; dy++)
            {
                int novoX =
                    centroX + dx;

                int novoY =
                    centroY + dy;

                double score =
                    CalcularScorePreenchimento(
                        imagem,
                        novoX,
                        novoY
                    );

                soma += score;
                quantidade++;
            }
        }

        if (quantidade == 0)
            return 0;

        return soma / quantidade;
    }

    // ============================================================
    // SCORE DA BOLINHA
    // ============================================================
    //
    // A grande diferença em relação ao código anterior:
    //
    // NÃO contamos a borda inteira da bolinha.
    //
    // Analisamos somente o círculo central.
    //
    // Bolinha vazia:
    //     A/B/C/D/E impressos -> score baixo
    //
    // Bolinha marcada:
    //     preenchimento real   -> score alto
    //
    // ============================================================

    private double CalcularScorePreenchimento(
    Mat imagem,
    int centroX,
    int centroY)
    {
        int metade =
            TamanhoRoi / 2;

        int x =
            centroX - metade;

        int y =
            centroY - metade;

        // ============================================================
        // Verificação de limites
        // ============================================================

        if (x < 0 ||
            y < 0 ||
            x + TamanhoRoi > imagem.Width ||
            y + TamanhoRoi > imagem.Height)
        {
            return 0;
        }


        using var roi =
            new Mat(
                imagem,
                new Rect(
                    x,
                    y,
                    TamanhoRoi,
                    TamanhoRoi
                )
            );


        // ============================================================
        // MÁSCARA DO CENTRO
        //
        // Aqui analisamos somente o interior da bolinha.
        // A borda impressa fica fora dessa região.
        // ============================================================

        using var mascaraCentro =
            new Mat(
                roi.Size(),
                MatType.CV_8UC1,
                Scalar.Black
            );

        Cv2.Circle(
            mascaraCentro,
            new Point(metade, metade),
            RaioMascara,
            Scalar.White,
            -1
        );


        // ============================================================
        // MÁSCARA DO FUNDO
        //
        // Usa os quatro cantos da ROI para saber qual é o nível de
        // iluminação local da folha.
        //
        // Isso ajuda muito em fotos com sombra.
        // ============================================================

        using var mascaraFundo =
            new Mat(
                roi.Size(),
                MatType.CV_8UC1,
                Scalar.Black
            );

        int tamanhoCanto = 4;

        // Canto superior esquerdo
        Cv2.Rectangle(
            mascaraFundo,
            new Rect(
                0,
                0,
                tamanhoCanto,
                tamanhoCanto
            ),
            Scalar.White,
            -1
        );

        // Canto superior direito
        Cv2.Rectangle(
            mascaraFundo,
            new Rect(
                TamanhoRoi - tamanhoCanto,
                0,
                tamanhoCanto,
                tamanhoCanto
            ),
            Scalar.White,
            -1
        );

        // Canto inferior esquerdo
        Cv2.Rectangle(
            mascaraFundo,
            new Rect(
                0,
                TamanhoRoi - tamanhoCanto,
                tamanhoCanto,
                tamanhoCanto
            ),
            Scalar.White,
            -1
        );

        // Canto inferior direito
        Cv2.Rectangle(
            mascaraFundo,
            new Rect(
                TamanhoRoi - tamanhoCanto,
                TamanhoRoi - tamanhoCanto,
                tamanhoCanto,
                tamanhoCanto
            ),
            Scalar.White,
            -1
        );


        // ============================================================
        // MÉDIA DO FUNDO
        // ============================================================

        double intensidadeFundo =
            Cv2.Mean(
                roi,
                mascaraFundo
            ).Val0;


        // ============================================================
        // MÉDIA DO CENTRO DA BOLINHA
        // ============================================================

        double intensidadeCentro =
            Cv2.Mean(
                roi,
                mascaraCentro
            ).Val0;


        // ============================================================
        // SCORE RELATIVO
        //
        // Fundo claro + centro escuro = score alto
        //
        // Fundo claro + centro praticamente igual = score baixo
        //
        // Exemplo:
        //
        // fundo = 250
        // centro = 220
        // score = 0.12  -> provavelmente vazio
        //
        // fundo = 250
        // centro = 40
        // score = 0.84  -> preenchido
        // ============================================================

        if (intensidadeFundo <= 1)
            return 0;

        double score =
            (
                intensidadeFundo -
                intensidadeCentro
            )
            /
            intensidadeFundo;


        // ============================================================
        // Limitar entre 0 e 1
        // ============================================================

        if (score < 0)
            score = 0;

        if (score > 1)
            score = 1;

        return score;
    }

    // ============================================================
    // MEDIANA
    // ============================================================

    private double CalcularMediana(double[] valores)
    {
        double[] ordenados =
            (double[])valores.Clone();

        Array.Sort(ordenados);

        return ordenados[ordenados.Length / 2];
    }
}