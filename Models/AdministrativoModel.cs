namespace Servidor.Models
{
    public class AdministrativoModel
    {
        public int Id { get; set; }
        public string Acoluna1 { get; set; }
        public string Acoluna2 { get; set; }
        public string Acoluna3 { get; set; }
        public string Acoluna4 { get; set; }
        public string Acoluna5 { get; set; }
        public string Acoluna6 { get; set; }

        public AdministrativoModel()
        {
        }

        public AdministrativoModel(int id, string acoluna1, string acoluna2, string acoluna3, string acoluna4, string acoluna5, string acoluna6)
        {
            Id = id;
            Acoluna1 = acoluna1;
            Acoluna2 = acoluna2;
            Acoluna3 = acoluna3;
            Acoluna4 = acoluna4;
            Acoluna5 = acoluna5;
            Acoluna6 = acoluna6;
        }
    }
}
