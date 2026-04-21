namespace GestionDocumentosMVC.Models
{
    public class DocumentViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Extension { get; set; }
        public System.DateTime UploadDate { get; set; }
        public int Version { get; set; }
    }
}