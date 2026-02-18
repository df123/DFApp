using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace DFApp.Migrations
{
    [DbContext(typeof(DFApp.EntityFrameworkCore.DFAppDbContext))]
    [Migration("20260217065648_RebuildElectricVehicleTables")]
    partial class RebuildElectricVehicleTables
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            modelBuilder.Entity("DFApp.ElectricVehicle.ElectricVehicle", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT")
                        .HasColumnName("ConcurrencyStamp");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("CreationTime");

                    b.Property<Guid?>("CreatorId")
                        .HasColumnType("TEXT")
                        .HasColumnName("CreatorId");

                    b.Property<DateTime?>("DeletionTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("DeletionTime");

                    b.Property<Guid?>("DeleterId")
                        .HasColumnType("TEXT")
                        .HasColumnName("DeleterId");

                    b.Property<string>("ExtraProperties")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("ExtraProperties");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("IsDeleted")
                        .HasDefaultValue(false);

                    b.Property<string>("LicensePlate")
                        .HasMaxLength(20)
                        .HasColumnType("TEXT")
                        .HasColumnName("LicensePlate");

                    b.Property<string>("Model")
                        .HasMaxLength(100)
                        .HasColumnType("TEXT")
                        .HasColumnName("Model");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT")
                        .HasColumnName("Name");

                    b.Property<DateTime?>("PurchaseDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("PurchaseDate");

                    b.Property<decimal>("TotalMileage")
                        .HasColumnType("TEXT")
                        .HasColumnName("TotalMileage")
                        .HasDefaultValue(0m);

                    b.Property<DateTime?>("LastModificationTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("LastModificationTime");

                    b.Property<Guid?>("LastModifierId")
                        .HasColumnType("TEXT")
                        .HasColumnName("LastModifierId");

                    b.HasKey("Id");

                    b.ToTable("AppElectricVehicle", (string)null);
                });

            modelBuilder.Entity("DFApp.ElectricVehicle.ElectricVehicleCost", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Amount")
                        .HasColumnType("TEXT")
                        .HasColumnName("Amount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT")
                        .HasColumnName("ConcurrencyStamp");

                    b.Property<DateTime>("CostDate")
                        .HasColumnType("TEXT")
                        .HasColumnName("CostDate");

                    b.Property<int>("CostType")
                        .HasColumnType("INTEGER")
                        .HasColumnName("CostType");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("CreationTime");

                    b.Property<Guid?>("CreatorId")
                        .HasColumnType("TEXT")
                        .HasColumnName("CreatorId");

                    b.Property<DateTime?>("DeletionTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("DeletionTime");

                    b.Property<Guid?>("DeleterId")
                        .HasColumnType("TEXT")
                        .HasColumnName("DeleterId");

                    b.Property<string>("ExtraProperties")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("ExtraProperties");

                    b.Property<bool>("IsBelongToSelf")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("IsBelongToSelf")
                        .HasDefaultValue(true);

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("IsDeleted")
                        .HasDefaultValue(false);

                    b.Property<DateTime?>("LastModificationTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("LastModificationTime");

                    b.Property<Guid?>("LastModifierId")
                        .HasColumnType("TEXT")
                        .HasColumnName("LastModifierId");

                    b.Property<string>("Remark")
                        .HasMaxLength(500)
                        .HasColumnType("TEXT")
                        .HasColumnName("Remark");

                    b.Property<Guid>("VehicleId")
                        .HasColumnType("TEXT")
                        .HasColumnName("VehicleId");

                    b.HasKey("Id");

                    b.HasIndex("CostDate");

                    b.HasIndex("CostType");

                    b.HasIndex("VehicleId");

                    b.ToTable("AppElectricVehicleCost", (string)null);
                });

            modelBuilder.Entity("DFApp.ElectricVehicle.ElectricVehicleChargingRecord", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("Cost")
                        .HasColumnType("TEXT")
                        .HasColumnName("Cost");

                    b.Property<string>("ChargingDate")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("ChargingDate");

                    b.Property<string>("ChargingStation")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT")
                        .HasColumnName("ChargingStation");

                    b.Property<decimal>("Electricity")
                        .HasColumnType("TEXT")
                        .HasColumnName("Electricity");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT")
                        .HasColumnName("ConcurrencyStamp");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("CreationTime");

                    b.Property<Guid?>("CreatorId")
                        .HasColumnType("TEXT")
                        .HasColumnName("CreatorId");

                    b.Property<DateTime?>("DeletionTime")
                        .HasColumnType("TD:EXT")
                        .HasColumnName("DeletionTime");

                    b.Property<Guid?>("DeleterId")
                        .HasColumnType("TEXT")
                        .HasColumnName("DeleterId");

                    b.Property<string>("ExtraProperties")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("ExtraProperties");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("IsDeleted")
                        .HasDefaultValue(false);

                    b.Property<DateTime?>("LastModificationTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("LastModificationTime");

                    b.Property<Guid?>("LastModifierId")
                        .HasColumnType("TEXT")
                        .HasColumnName("LastModifierId");

                    b.Property<Guid>("VehicleId")
                        .HasColumnType("TEXT")
                        .HasColumnName("VehicleId");

                    b.HasKey("Id");

                    b.HasIndex("ChargingDate");

                    b.HasIndex("VehicleId");

                    b.ToTable("AppElectricVehicleChargingRecord", (string)null);
                });

            modelBuilder.Entity("DFApp.ElectricVehicle.GasolinePrice", b =>
                {
                    b.Property<Guid>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT")
                        .HasColumnName("ConcurrencyStamp");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("CreationTime");

                    b.Property<Guid?>("CreatorId")
                        .HasColumnType("TEXT")
                        .HasColumnName("CreatorId");

                    b.Property<DateTime>("Date")
                        .HasColumnType("TEXT")
                        .HasColumnName("Date");

                    b.Property<DateTime?>("DeletionTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("DeletionTime");

                    b.Property<Guid?>("DeleterId")
                        .HasColumnType("TEXT")
                        .HasColumnName("DeleterId");

                    b.Property<string>("ExtraProperties")
                        .IsRequired()
                        .HasColumnType("TEXT")
                        .HasColumnName("ExtraProperties");

                    b.Property<bool>("IsDeleted")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER")
                        .HasColumnName("IsDeleted")
                        .HasDefaultValue(false);

                    b.Property<DateTime?>("LastModificationTime")
                        .HasColumnType("TEXT")
                        .HasColumnName("LastModificationTime");

                    b.Property<Guid?>("LastModifierId")
                        .HasColumnType("TEXT")
                        .HasColumnName("LastModifierId");

                    b.Property<decimal?>("Price0H")
                        .HasColumnType("TEXT")
                        .HasColumnName("Price0H");

                    b.Property<decimal?>("Price89H")
                        .HasColumnType("TEXT")
                        .HasColumnName("Price89H");

                    b.Property<decimal?>("Price90H")
                        .HasColumnType("TEXT")
                        .HasColumnName("Price90H");

                    b.Property<decimal?>("Price92H")
                        .HasColumnType("TEXT")
                        .HasColumnName("Price92H");

                    b.Property<decimal?>("Price93H")
                        .HasColumnType("TEXT")
                        .HasColumnName("Price93H");

                    b.Property<decimal?>("Price95H")
                        .HasColumnType("TEXT")
                        .HasColumnName("Price95H");

                    b.Property<decimal?>("Price97H")
                        .HasColumnType("TEXT")
                        .HasColumnName("Price97H");

                    b.Property<decimal?>("Price98H")
                        .HasColumnType("TEXT")
                        .HasColumnName("Price98H");

                    b.Property<string>("Province")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("TEXT")
                        .HasColumnName("Province");

                    b.HasKey("Id");

                    b.HasIndex("Date");

                    b.HasIndex("Province");

                    b.ToTable("AppGasolinePrice", (string)null);
                });

            modelBuilder.Entity("DFApp.ElectricVehicle.ElectricVehicleCost", b =>
                {
                    b.HasOne("DFApp.ElectricVehicle.ElectricVehicle", "Vehicle")
                        .WithMany("Costs")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Vehicle");
                });

            modelBuilder.Entity("DFApp.ElectricVehicle.ElectricVehicleChargingRecord", b =>
                {
                    b.HasOne("DFApp.ElectricVehicle.ElectricVehicle", "Vehicle")
                        .WithMany("ChargingRecords")
                        .HasForeignKey("VehicleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Vehicle");
                });

            modelBuilder.Entity("DFApp.ElectricVehicle.ElectricVehicle", b =>
                {
                    b.Navigation("Costs");

                    b.Navigation("ChargingRecords");
                });
#pragma warning restore 612, 618
        }
    }
}
