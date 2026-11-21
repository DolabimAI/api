using MediatR;

namespace DolApp.Application.UseCases.Clothes.AnalyzeClothing
{
    /// POST /api/clothes/analyze isteği. JSON değil, multipart/form-data: fotoğraf "image" alanında gelir.
    /// Controller yüklenen dosyadan doldurur; kullanıcı token'dan bulunur (ICurrentUser).

    public class AnalyzeClothingRequest : IRequest<AnalyzeClothingResponse>
    {
        /// Fotoğrafın içeriği. Dosya gönderilmediyse null (validator required_fields döner).
        public Stream? ImageStream { get; set; }

        /// Telefonun gönderdiği dosya adı. Yalnızca Python'a iletilir ve loga yazılır; tür buna göre belirlenmez.
        public string FileName { get; set; } = string.Empty;

        /// Bayt cinsinden boyut (10 MB sınırı için).
        public long FileLength { get; set; }
    }
}
