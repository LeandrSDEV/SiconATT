using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using NPOI.HSSF.UserModel; // Para arquivos .xls
using Servidor.Data;
using Servidor.Models;
using Servidor.Models.Enums;

namespace Servidor.Controllers
{
    public class ConvenioController : Controller
    {
        private readonly BancoContext _context;

        public ConvenioController(BancoContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new EnumModel();
            var statusList = Enum.GetValues(typeof(Status))
                                 .Cast<Status>()
                                 .Select(s => new SelectListItem
                                 {
                                     Value = ((int)s).ToString(),
                                     Text = s.ToString()
                                 }).ToList();
            ViewBag.Statuses = statusList;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessarArquivo(IFormFile arquivoTxt, IFormFile arquivoExcel, EnumModel status)
        {
            if (arquivoTxt == null || arquivoTxt.Length == 0 && arquivoExcel == null || arquivoExcel.Length == 0)
            {
                TempData["Mensagem"] = "Erro nos arquivos enviado.";
                return RedirectToAction("Index");
            }

            var registros = new List<ContrachequeModel>();
            var registros2 = new List<AdministrativoModel>();

            try
            {
                using (var reader = new StreamReader(arquivoTxt.OpenReadStream(), Encoding.UTF8))
                {
                    while (!reader.EndOfStream)
                    {
                        var linha = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(linha) || !linha.StartsWith("F", StringComparison.OrdinalIgnoreCase))
                            continue;

                        var colunas = linha.Split(';').Select(c => c.Trim()).ToArray();
                        
                        if (status.StatusSelecionado == Status.ABARE)
                        {
                            var contracheque = new ContrachequeModel
                            {
                                Ccoluna1 = colunas[7],
                                Ccoluna2 = colunas[3],
                                Ccoluna3 = colunas[4],
                                Ccoluna4 = colunas[5],
                                Ccoluna5 = "Rua A",
                                Ccoluna6 = "S/N",
                                Ccoluna7 = "CASA",
                                Ccoluna8 = "CENTRO",
                                Ccoluna9 = "ABARE",
                                Ccoluna10 = "BA",
                                Ccoluna11 = "99999999",
                                Ccoluna12 = colunas[11],
                                Ccoluna13 = colunas[12],
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
                            if (colunas[16].Trim().Equals("Cargo Comissionado", StringComparison.OrdinalIgnoreCase))
                            {
                                contracheque.Ccoluna16 = "7";
                            }
                            if (colunas[16].Trim().Equals("Cargo Efetivo", StringComparison.OrdinalIgnoreCase))
                            {
                                contracheque.Ccoluna16 = "2";
                            }
                            if (colunas[16].Trim().Equals("SERVIDOR EFETIVO CEDIDO DE OUTRA ENTIDADE", StringComparison.OrdinalIgnoreCase))
                            {
                                contracheque.Ccoluna16 = "33";
                            }
                            if (colunas[16].Trim().Equals("EFETIVO CEDIDO LAGOA DOS GATOS", StringComparison.OrdinalIgnoreCase))
                            {
                                contracheque.Ccoluna16 = "33";
                            }
                            if (colunas[16].Trim().Equals("ELETIVOS", StringComparison.OrdinalIgnoreCase))
                            {
                                contracheque.Ccoluna16 = "13";
                            }
                            if (colunas[16].Trim().Equals("Contratados", StringComparison.OrdinalIgnoreCase))
                            {
                                contracheque.Ccoluna16 = "5";
                            }
                            if (colunas[16].Trim().Equals("PENSIONISTA", StringComparison.OrdinalIgnoreCase))
                            {
                                contracheque.Ccoluna16 = "1";
                            }
                            if (colunas[16].Trim().Equals("INATIVOS", StringComparison.OrdinalIgnoreCase))
                            {
                                contracheque.Ccoluna16 = "14";
                            }

                            registros.Add(contracheque);
                        }
                        if (status.StatusSelecionado == Status.CUPIRA)
                        {
                            var contracheque = new ContrachequeModel
                            {
                                Ccoluna1 = colunas[7],
                                Ccoluna2 = colunas[3],
                                Ccoluna3 = colunas[4],
                                Ccoluna4 = colunas[5],
                                Ccoluna5 = "Rua A",
                                Ccoluna6 = "S/N",
                                Ccoluna7 = "CASA",
                                Ccoluna8 = "CENTRO",
                                Ccoluna9 = "CUPIRA",
                                Ccoluna10 = "AL",
                                Ccoluna11 = "99999999",
                                Ccoluna12 = colunas[11],
                                Ccoluna13 = colunas[12],
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
                            registros.Add(contracheque);
                        }

                    }
                }

                if (registros.Any())
                {
                    _context.Contracheque.AddRange(registros);
                    await _context.SaveChangesAsync();
                    TempData["Mensagem"] = $"{registros.Count} registros salvos com sucesso!";
                }
                else
                {
                    TempData["Mensagem"] = "Nenhum dado válido encontrado no arquivo TXT.";
                }
            }
            catch (Exception ex)
            {
                TempData["Mensagem"] = $"Erro ao processar o arquivo TXT: {ex.Message}";
            }

            try
            {
                using (var stream = arquivoExcel.OpenReadStream())
                {
                    var workbook = new HSSFWorkbook(stream); // Para arquivos .xls
                    var sheet = workbook.GetSheetAt(0); // Pega a primeira aba

                    for (int rowIdx = 1; rowIdx <= sheet.LastRowNum; rowIdx++) // Começa da linha 1 para ignorar cabeçalho
                    {
                        var row = sheet.GetRow(rowIdx);
                        if (row == null) continue; // Ignora linhas vazias

                        var administrativo = new AdministrativoModel
                        {
                            Acoluna1 = row.GetCell(2)?.ToString() ?? "",
                            Acoluna2 = row.GetCell(3)?.ToString() ?? "", // Coluna 4
                            Acoluna3 = row.GetCell(4)?.ToString() ?? "", // Coluna 5
                            Acoluna4 = row.GetCell(12)?.ToString() ?? "", // Coluna 13
                            Acoluna5 = row.GetCell(13)?.ToString() ?? "", // Coluna 14
                            Acoluna6 = row.GetCell(14)?.ToString() ?? "", // Coluna 15
                        };
                        if (administrativo.Acoluna5 == "Contratado")
                        {
                            administrativo.Acoluna5 = "5";
                        }
                        if (administrativo.Acoluna5 == "Comissionado")
                        {
                            administrativo.Acoluna5 = "7";
                        }
                        if (administrativo.Acoluna5 == "Agente politico")
                        {
                            administrativo.Acoluna5 = "13";
                        }
                        if (administrativo.Acoluna5 == "Efetivo")
                        {
                            administrativo.Acoluna5 = "2";
                        }
                        if (administrativo.Acoluna5 == "Inativo")
                        {
                            administrativo.Acoluna5 = "14";
                        }
                        if (administrativo.Acoluna5 == "Pensionista")
                        {
                            administrativo.Acoluna5 = "1";
                        }
                        if (administrativo.Acoluna5 == "Cedido")
                        {
                            administrativo.Acoluna5 = "33";
                        }
                        if (administrativo.Acoluna5 == "Eletivo")
                        {
                            administrativo.Acoluna5 = "13";
                        }
                        registros2.Add(administrativo);
                    }
                }

                if (registros2.Any())
                {
                    _context.Administrativo.AddRange(registros2);
                    await _context.SaveChangesAsync();
                    TempData["Mensagem"] = $"{registros2.Count} registros salvos com sucesso!";
                }
                else
                {
                    TempData["Mensagem"] = "Nenhum dado válido encontrado no arquivo Excel.";
                }
            }
            catch (Exception ex)
            {
                TempData["Mensagem"] = $"Erro ao processar o arquivo Excel: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
