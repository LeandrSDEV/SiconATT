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
        private readonly AbareService _abareservice;
        private readonly CupiraService _cupiraservice;
        private readonly CansancaoService _cansancaoservice;
        private readonly MatriculaService _matriculaservice;
        private readonly SecretariaService _secretariaservice;
        private readonly ServidorService _servidorService;
        private readonly CategoriaService _categoriaService;
        private readonly CleanupService _cleanupService;

        public ConvenioController(BancoContext context, AbareService abareservice, 
                                  CupiraService cupiraservice, CansancaoService cansancaoservice, 
                                  MatriculaService matriculaservice, SecretariaService secretariaservice,
                                  ServidorService servidorService, CategoriaService categoriaService,
                                  CleanupService cleanupService)
        {
            _context = context;
            _abareservice = abareservice;
            _cupiraservice = cupiraservice;
            _cansancaoservice = cansancaoservice;
            _matriculaservice = matriculaservice;
            _secretariaservice = secretariaservice;
            _servidorService = servidorService;
            _categoriaService = categoriaService;
            _cleanupService = cleanupService;
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
                            var contracheques = await _abareservice.ProcessarArquivoAsync(colunas, Status.ABARE);
                            registros.AddRange(contracheques);
                        }
                        if (status.StatusSelecionado == Status.CUPIRA)
                        {
                            var contracheques = await _cupiraservice.ProcessarArquivoAsync(colunas, Status.CUPIRA);
                            registros.AddRange(contracheques);
                        }
                        if (status.StatusSelecionado == Status.CANSANCAO)
                        {
                            var contracheques = await _cansancaoservice.ProcessarArquivoAsync(colunas, Status.CANSANCAO);
                            registros.AddRange(contracheques);
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

                         //Mapeamento de valores para Acoluna5
                        var Vinculo = new Dictionary<string, string>
                        {
                            { "Contratado", "5" },
                            { "Comissionado", "7" },
                            { "Agente politico", "13" },
                            { "Efetivo", "2" },
                            { "Inativo", "14" },
                            { "Pensionista", "1" },
                            { "Cedido", "33" },
                            { "Eletivo", "13" },
                            { "Temporário", "11"},
                            { "Aguardando Especificar", "14" },
                            { "Conselheiro Tutelar", "17"},
                            { "Estatutário", "10"},
                            { "Militar", "14"},
                            { "Celetista", "9"}
                        };                      

                         //Atualiza Acoluna5 com base no mapeamento
                        if (Vinculo.ContainsKey(administrativo.Acoluna5))
                        {
                            administrativo.Acoluna5 = Vinculo[administrativo.Acoluna5];
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

            await _matriculaservice.GerarMatriculasAsync();
            await _servidorService.GerarEncontradoAsync();
            await _secretariaservice.GerarSecretariasAsync();
            await _categoriaService.GerarVinculoAsync();

            await _cleanupService.LimparTabelasAsync();

            return RedirectToAction("Index");
        }
    }
}
