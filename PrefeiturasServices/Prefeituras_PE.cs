using Servidor.Models.Enums;
using Servidor.Models;
//======================================    CUPIRA    ============================================\\

public class CupiraService
{
    private static readonly Dictionary<string, string> Vinculo = new()
    {
        { "Cargo Comissionado", "7" },
        { "Cargo Efetivo", "2" },
        { "SERVIDOR EFETIVO CEDIDO DE OUTRA ENTIDADE", "33" },
        { "EFETIVO CEDIDO LAGOA DOS GATOS", "33" },
        { "ELETIVOS", "13" },
        { "Contratados", "5" },
        { "PENSIONISTA", "1" },
        { "INATIVOS", "28" }
    };

    public Task<List<ContrachequeModel>> ProcessarArquivoAsync(string[] colunas, Status status)
    {
        var contracheque = ContrachequeHelper.CriarContracheque(colunas, "CUPIRA");

        if (contracheque.Ccoluna1 == "PREFEITURA MUNICIPAL DE CUPIRA")
        {
            contracheque.Ccoluna21 = "1";
        }

        if (Vinculo.ContainsKey(colunas[16].Trim()))
        {
            contracheque.Ccoluna16 = Vinculo[colunas[16].Trim()];
        }

        switch (contracheque.Ccoluna16)
        {
            case "7":
            case "13":
            case "5":
            case "28":
            case "1":
                contracheque.Ccoluna18 = "943";
                break;
            case "33":
            case "2":
                contracheque.Ccoluna18 = "926";
                break;
            default:
                contracheque.Ccoluna18 = "ERRO";
                break;
        }


        return Task.FromResult(new List<ContrachequeModel> { contracheque });
    }
}
