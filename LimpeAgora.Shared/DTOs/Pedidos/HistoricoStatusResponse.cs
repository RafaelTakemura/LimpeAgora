using LimpeAgora.Shared.Enums;

namespace LimpeAgora.Shared.DTOs.Pedidos;

public class HistoricoStatusResponse
{
    public Guid Id { get; set; }
    public StatusPedido StatusAnterior { get; set; }
    public string StatusAnteriorDescricao => StatusAnterior.ToString();
    public StatusPedido StatusNovo { get; set; }
    public string StatusNovoDescricao => StatusNovo.ToString();
    public Guid UsuarioResponsavelId { get; set; }
    public string NomeUsuarioResponsavel { get; set; } = string.Empty;
    public DateTime AlteradoEm { get; set; }
}
