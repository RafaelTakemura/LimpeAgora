using LimpeAgora.Shared.Enums;

namespace LimpeAgoraApi.Entities;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string NomeCompleto { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefone { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Cliente;
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Relacionamentos de navegação
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    public ICollection<HistoricoStatusPedido> HistoricosStatus { get; set; } = new List<HistoricoStatusPedido>();
}
