namespace GestionDocumentosMVC.Models
{
    public class SharedDocumentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public string OwnerName { get; set; }
        public string Permission { get; set; }
        public int Version { get; set; }
    }
}