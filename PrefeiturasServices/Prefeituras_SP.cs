using Servidor.Models.Enums;
using Servidor.Models;

public class ContrachequeSP
{
    public static ContrachequeModel CriarContracheque(string[] colunas, string municipio)
    {
        return new ContrachequeModel
        {
            Ccoluna1 = colunas[7],
            Ccoluna2 = colunas[3],
            Ccoluna3 = colunas[4],
            Ccoluna4 = colunas[5],
            Ccoluna5 = "Rua A",
            Ccoluna6 = "S/N",
            Ccoluna7 = "CASA",
            Ccoluna8 = "CENTRO",
            Ccoluna9 = municipio,
            Ccoluna10 = "MS",
            Ccoluna11 = "99999999",
            Ccoluna12 = "0",
            Ccoluna13 = "0",
            Ccoluna14 = "99999999999",
            Ccoluna15 = colunas[9],
            Ccoluna16 = colunas[16],
            Ccoluna17 = "0",
            Ccoluna18 = colunas[18],
            Ccoluna19 = "0",
            Ccoluna20 = "Teste@gmail.com",
            Ccoluna21 = colunas[19],
            Ccoluna22 = "0",
            Ccoluna23 = colunas[10],
            Ccoluna24 = "0",
            Ccoluna25 = "0"
        };
    }
}
//======================================    CUPIRA    ============================================\\

public class IndiaporaService
{
    private static readonly Dictionary<string, string> Vinculo = new()
    {
        { "Diretor sem v?nculo empregat?cio para o qual a empresa/entidade tenha optado por recolhimento do FGTS.", "29" },
        { "Servidor regido pelo Regime Jur?dico ?nico ( Federal,Estadual e Municipal) e militar", "2" },
        { "Servidor P?blico n?o-efetivo (demiss?vel ad nutum ou admitido por legisla??o especial, n?o regido pela CLT ).", "5" },
        { "ESTAGIARIOS", "8" },
        { "Tempor?rios", "11" },
        { "CONSELHEIROS TUTELARES", "17" }
    };

    public Task<List<ContrachequeModel>> ProcessarArquivoAsync(string[] colunas, Status status)
    {       
        var contracheque = ContrachequeSP.CriarContracheque(colunas, "INDIAPORA");

        if (contracheque.Ccoluna1 == "MUNICIPIO DE INDIAPORA")
        {
            contracheque.Ccoluna21 = "1";
        }

        if (Vinculo.ContainsKey(colunas[16].Trim()))
        {
            contracheque.Ccoluna16 = Vinculo[colunas[16].Trim()];
        }

        switch (contracheque.Ccoluna16)
        {
            case "29":
            case "8":
            case "5":
            case "2":
            case "11":
            case "17":
                contracheque.Ccoluna18 = "294";
                break;          
            default:
                contracheque.Ccoluna18 = "ERRO";
                break;
        }


        return Task.FromResult(new List<ContrachequeModel> { contracheque });
    }
}
