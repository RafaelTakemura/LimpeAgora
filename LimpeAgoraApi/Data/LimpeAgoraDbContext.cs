using LimpeAgoraApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace LimpeAgoraApi.Data;

public class LimpeAgoraDbContext : DbContext
{
    public LimpeAgoraDbContext(DbContextOptions<LimpeAgoraDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<ItemPedido> ItensPedido => Set<ItemPedido>();
    public DbSet<HistoricoStatusPedido> HistoricosStatusPedido => Set<HistoricoStatusPedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Mapeamento: Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("Usuarios");
            entity.HasKey(u => u.Id);

            entity.Property(u => u.NomeCompleto)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.HasIndex(u => u.Email)
                .IsUnique();

            entity.Property(u => u.Telefone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(u => u.SenhaHash)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.Perfil)
                .IsRequired();

            entity.Property(u => u.Ativo)
                .IsRequired();

            entity.Property(u => u.CriadoEm)
                .IsRequired();
        });

        // Mapeamento: Categoria
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias");
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(c => c.Descricao)
                .HasMaxLength(500);

            entity.Property(c => c.Ativa)
                .IsRequired();

            entity.Property(c => c.CriadoEm)
                .IsRequired();

            entity.HasIndex(c => c.Nome);
        });

        // Mapeamento: Produto
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.ToTable("Produtos");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Nome)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(p => p.Descricao)
                .HasMaxLength(500);

            entity.Property(p => p.Apresentacao)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(p => p.Preco)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            entity.Property(p => p.QuantidadeEstoque)
                .IsRequired();

            entity.Property(p => p.ImagemUrl)
                .HasMaxLength(500);

            entity.Property(p => p.Ativo)
                .IsRequired();

            entity.Property(p => p.CriadoEm)
                .IsRequired();

            entity.Property(p => p.AtualizadoEm)
                .IsRequired();

            entity.HasIndex(p => p.Nome);
            entity.HasIndex(p => p.CategoriaId);

            entity.HasOne(p => p.Categoria)
                .WithMany(c => c.Produtos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeamento: Pedido
        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.ToTable("Pedidos");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.ValorTotal)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            entity.Property(p => p.Status)
                .IsRequired();

            entity.Property(p => p.Observacao)
                .HasMaxLength(500);

            entity.Property(p => p.DataPedido)
                .IsRequired();

            entity.Property(p => p.AtualizadoEm)
                .IsRequired();

            entity.HasIndex(p => p.UsuarioId);
            entity.HasIndex(p => p.Status);
            entity.HasIndex(p => p.DataPedido);

            entity.HasOne(p => p.Usuario)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeamento: ItemPedido
        modelBuilder.Entity<ItemPedido>(entity =>
        {
            entity.ToTable("ItensPedido");
            entity.HasKey(i => i.Id);

            entity.Ignore(i => i.Subtotal);

            entity.Property(i => i.NomeProduto)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(i => i.Quantidade)
                .IsRequired();

            entity.Property(i => i.PrecoUnitario)
                .IsRequired()
                .HasColumnType("decimal(10,2)");

            entity.HasOne(i => i.Pedido)
                .WithMany(p => p.Itens)
                .HasForeignKey(i => i.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Produto)
                .WithMany(p => p.ItensPedido)
                .HasForeignKey(i => i.ProdutoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Mapeamento: HistoricoStatusPedido
        modelBuilder.Entity<HistoricoStatusPedido>(entity =>
        {
            entity.ToTable("HistoricosStatusPedido");
            entity.HasKey(h => h.Id);

            entity.Property(h => h.StatusAnterior)
                .IsRequired();

            entity.Property(h => h.StatusNovo)
                .IsRequired();

            entity.Property(h => h.AlteradoEm)
                .IsRequired();

            entity.HasOne(h => h.Pedido)
                .WithMany(p => p.HistoricosStatus)
                .HasForeignKey(h => h.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(h => h.UsuarioResponsavel)
                .WithMany(u => u.HistoricosStatus)
                .HasForeignKey(h => h.UsuarioResponsavelId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
