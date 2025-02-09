using Servidor.Models.Enums;
using Servidor.Models;
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
        var contracheque = ContrachequeHelper.CriarContracheque(colunas, "INDIAPORA");

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
