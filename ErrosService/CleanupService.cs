using Servidor.Data;

public class CleanupService
{
    private readonly BancoContext _context;

    public CleanupService(BancoContext context)
    {
        _context = context;
    }

    public async Task LimparTabelasAsync()
    {
        // Remove todos os registros da tabela Contracheque
        _context.Contracheque.RemoveRange(_context.Contracheque);

        // Remove todos os registros da tabela Administrativo
        _context.Administrativo.RemoveRange(_context.Administrativo);

        // Salva as alterações no banco de dados
        await _context.SaveChangesAsync();

        Console.WriteLine("Tabelas Contracheque e Administrativo limpas com sucesso.");
    }
}
