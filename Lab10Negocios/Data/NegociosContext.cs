using System;
using System.Collections.Generic;
using Lab10Negocios.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab10Negocios.Data;

public partial class NegociosContext : DbContext
{
    public NegociosContext(DbContextOptions<NegociosContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Cargo> Cargos { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Cliente> Clientes { get; set; }

    public virtual DbSet<Detallepedido> Detallepedidos { get; set; }

    public virtual DbSet<Distrito> Distritos { get; set; }

    public virtual DbSet<Empleado> Empleados { get; set; }

    public virtual DbSet<Pais> Pais { get; set; }

    public virtual DbSet<Pedido> Pedidos { get; set; }

    public virtual DbSet<Producto> Productos { get; set; }

    public virtual DbSet<Proveedor> Proveedores { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cargo>(entity =>
        {
            entity.HasKey(e => e.IdCargo).HasName("PK__CARGO__3D0E29B884BAECA5");

            entity.ToTable("CARGO");

            entity.Property(e => e.IdCargo).HasColumnName("idCargo");
            entity.Property(e => e.NomCargo)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PK__CATEGORI__A3C02A10DA83A50F");

            entity.ToTable("CATEGORIA");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.NombreCategoria)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.HasKey(e => e.IdCliente).HasName("PK__CLIENTE__D5946642F3628086");

            entity.ToTable("CLIENTE");

            entity.Property(e => e.IdCliente)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("A")
                .IsFixedLength();
            entity.Property(e => e.Idpais).HasColumnName("idpais");
            entity.Property(e => e.NombreCia)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(24)
                .IsUnicode(false);

            entity.HasOne(d => d.IdpaisNavigation).WithMany(p => p.Clientes)
                .HasForeignKey(d => d.Idpais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CLIENTE__idpais__3B75D760");
        });

        modelBuilder.Entity<Detallepedido>(entity =>
        {
            entity.HasKey(e => new { e.IdPedido, e.IdProducto }).HasName("PK__DETALLEP__8DABD4E216BD1022");

            entity.ToTable("DETALLEPEDIDO");

            entity.Property(e => e.PrecioUnidad).HasColumnType("decimal(10, 0)");

            entity.HasOne(d => d.IdPedidoNavigation).WithMany(p => p.Detallepedidos)
                .HasForeignKey(d => d.IdPedido)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DETALLEPE__IdPed__52593CB8");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.Detallepedidos)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DETALLEPE__IdPro__534D60F1");
        });

        modelBuilder.Entity<Distrito>(entity =>
        {
            entity.HasKey(e => e.IdDistrito).HasName("PK__DISTRITO__494092A87D5D82DD");

            entity.ToTable("DISTRITO");

            entity.Property(e => e.IdDistrito).HasColumnName("idDistrito");
            entity.Property(e => e.NomDistrito)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.IdEmpleado).HasName("PK__EMPLEADO__CE6D8B9E811E5420");

            entity.ToTable("EMPLEADO");

            entity.Property(e => e.ApeEmpleado)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.DirEmpleado)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Estado)
                .HasMaxLength(1)
                .IsUnicode(false)
                .IsFixedLength();
            entity.Property(e => e.FonoEmpleado)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("fonoEmpleado");
            entity.Property(e => e.IdCargo).HasColumnName("idCargo");
            entity.Property(e => e.IdDistrito).HasColumnName("idDistrito");
            entity.Property(e => e.NomEmpleado)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.IdCargoNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdCargo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EMPLEADO__idCarg__4AB81AF0");

            entity.HasOne(d => d.IdDistritoNavigation).WithMany(p => p.Empleados)
                .HasForeignKey(d => d.IdDistrito)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__EMPLEADO__idDist__49C3F6B7");
        });

        modelBuilder.Entity<Pais>(entity =>
        {
            entity.HasKey(e => e.Idpais).HasName("PK__PAIS__AD357A25E36D1B8D");

            entity.ToTable("PAIS");

            entity.Property(e => e.NombrePais)
                .HasMaxLength(40)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Pedido>(entity =>
        {
            entity.HasKey(e => e.IdPedido).HasName("PK__PEDIDO__9D335DC3E9F71340");

            entity.ToTable("PEDIDO");

            entity.Property(e => e.IdPedido).ValueGeneratedNever();
            entity.Property(e => e.Cargo).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.CiudadDestinatario)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.CodPostalDestinatario)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.Destinatario)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.DireccionDestinatario)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Envio)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("0")
                .IsFixedLength();
            entity.Property(e => e.FechaEntrega).HasColumnType("datetime");
            entity.Property(e => e.FechaEnvio).HasColumnType("datetime");
            entity.Property(e => e.FechaPedido)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.IdCliente)
                .HasMaxLength(5)
                .IsUnicode(false);
            entity.Property(e => e.PaisDestinatario)
                .HasMaxLength(15)
                .IsUnicode(false);
            entity.Property(e => e.RegionDestinatario)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.IdClienteNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdCliente)
                .HasConstraintName("FK__PEDIDO__IdClient__4D94879B");

            entity.HasOne(d => d.IdEmpleadoNavigation).WithMany(p => p.Pedidos)
                .HasForeignKey(d => d.IdEmpleado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PEDIDO__IdEmplea__4E88ABD4");
        });

        modelBuilder.Entity<Producto>(entity =>
        {
            entity.HasKey(e => e.IdProducto).HasName("PK__PRODUCTO__098892104675D9A6");

            entity.ToTable("PRODUCTO");

            entity.Property(e => e.NombreProducto)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.PrecioUnidad).HasColumnType("decimal(10, 0)");
            entity.Property(e => e.Umedida)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("umedida");

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdCategoria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTO__IdCate__4316F928");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.Productos)
                .HasForeignKey(d => d.IdProveedor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PRODUCTO__IdProv__4222D4EF");
        });

        modelBuilder.Entity<Proveedor>(entity =>
        {
            entity.HasKey(e => e.IdProveedor).HasName("PK__PROVEEDO__E8B631AF1E4850E8");

            entity.ToTable("PROVEEDOR");

            entity.Property(e => e.CargoContacto)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(60)
                .IsUnicode(false);
            entity.Property(e => e.Fax)
                .HasMaxLength(24)
                .IsUnicode(false);
            entity.Property(e => e.Idpais).HasColumnName("idpais");
            entity.Property(e => e.NombreCia)
                .HasMaxLength(40)
                .IsUnicode(false);
            entity.Property(e => e.NombreContacto)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(24)
                .IsUnicode(false);

            entity.HasOne(d => d.IdpaisNavigation).WithMany(p => p.Proveedores)
                .HasForeignKey(d => d.Idpais)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__PROVEEDOR__idpai__3F466844");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
