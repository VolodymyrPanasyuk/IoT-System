using IoT_System.Domain.Entities.IoT;
using Microsoft.EntityFrameworkCore;

namespace IoT_System.Infrastructure.Contexts;

public class IoTDbContext : DbContext
{
    public IoTDbContext(DbContextOptions<IoTDbContext> options) : base(options)
    {
    }

    // IoT entities
    public DbSet<Device> Devices => Set<Device>();
    public DbSet<DeviceField> DeviceFields => Set<DeviceField>();
    public DbSet<FieldMapping> FieldMappings => Set<FieldMapping>();
    public DbSet<MeasurementDateMapping> MeasurementDateMappings => Set<MeasurementDateMapping>();
    public DbSet<DeviceMeasurement> DeviceMeasurements => Set<DeviceMeasurement>();
    public DbSet<FieldMeasurementValue> FieldMeasurementValues => Set<FieldMeasurementValue>();
    public DbSet<DeviceAccessPermission> DeviceAccessPermissions => Set<DeviceAccessPermission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.HasDefaultSchema("IoT.System");

        ConfigureDevice(builder);
        ConfigureDeviceField(builder);
        ConfigureFieldMapping(builder);
        ConfigureMeasurementDateMapping(builder);
        ConfigureDeviceMeasurement(builder);
        ConfigureFieldMeasurementValue(builder);
        ConfigureDeviceAccessPermission(builder);
    }

    private void ConfigureDevice(ModelBuilder builder)
    {
        builder.Entity<Device>(entity =>
        {
            entity.HasKey(d => d.Id);

            // Basic properties
            entity.Property(d => d.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(d => d.Description)
                .HasMaxLength(1000);

            // GPS coordinates with precision (6 decimal places ≈ 0.1 meters accuracy)
            entity.Property(d => d.Latitude)
                .HasPrecision(9, 6);

            entity.Property(d => d.Longitude)
                .HasPrecision(9, 6);

            // API authentication
            entity.Property(d => d.ApiKey)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(d => d.ApiKeyExpiresAt)
                .IsRequired(false);

            // Audit fields
            entity.Property(d => d.CreatedBy)
                .IsRequired();

            entity.Property(d => d.CreatedOn)
                .IsRequired();

            entity.Property(d => d.LastModifiedBy)
                .IsRequired(false);

            entity.Property(d => d.LastModifiedOn)
                .IsRequired(false);

            // Indexes
            entity.HasIndex(d => d.ApiKey)
                .IsUnique()
                .HasDatabaseName("IX_Devices_ApiKey");

            entity.HasIndex(d => new { d.IsActive, d.CreatedOn })
                .HasDatabaseName("IX_Devices_IsActive_CreatedOn");

            // Navigation properties
            entity.HasMany(d => d.Fields)
                .WithOne(f => f.Device)
                .HasForeignKey(f => f.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.Measurements)
                .WithOne(m => m.Device)
                .HasForeignKey(m => m.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(d => d.AccessPermissions)
                .WithOne(ap => ap.Device)
                .HasForeignKey(ap => ap.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureDeviceField(ModelBuilder builder)
    {
        builder.Entity<DeviceField>(entity =>
        {
            entity.HasKey(f => f.Id);

            // Field properties
            entity.Property(f => f.FieldName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(f => f.DisplayName)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(f => f.Description)
                .HasMaxLength(500);

            entity.Property(f => f.Unit)
                .HasMaxLength(50);

            entity.Property(f => f.DataType)
                .IsRequired();

            entity.Property(f => f.DisplayOrder)
                .IsRequired();

            entity.Property(f => f.IsActive)
                .IsRequired();

            // Threshold values with high precision for scientific measurements
            entity.Property(f => f.WarningMin)
                .HasPrecision(18, 6);

            entity.Property(f => f.WarningMax)
                .HasPrecision(18, 6);

            entity.Property(f => f.CriticalMin)
                .HasPrecision(18, 6);

            entity.Property(f => f.CriticalMax)
                .HasPrecision(18, 6);

            // Audit fields
            entity.Property(f => f.CreatedBy)
                .IsRequired();

            entity.Property(f => f.CreatedOn)
                .IsRequired();

            entity.Property(f => f.LastModifiedBy)
                .IsRequired(false);

            entity.Property(f => f.LastModifiedOn)
                .IsRequired(false);

            // Indexes
            entity.HasIndex(f => new { f.DeviceId, f.FieldName })
                .IsUnique()
                .HasDatabaseName("IX_DeviceFields_DeviceId_FieldName");

            entity.HasIndex(f => new { f.DeviceId, f.IsActive, f.DisplayOrder })
                .HasDatabaseName("IX_DeviceFields_DeviceId_IsActive_DisplayOrder");

            // Navigation properties
            entity.HasMany(f => f.Mappings)
                .WithOne(m => m.Field)
                .HasForeignKey(m => m.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(f => f.MeasurementValues)
                .WithOne(mv => mv.Field)
                .HasForeignKey(mv => mv.FieldId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private void ConfigureFieldMapping(ModelBuilder builder)
    {
        builder.Entity<FieldMapping>(entity =>
        {
            entity.HasKey(m => m.Id);

            // Mapping configuration
            entity.Property(m => m.DataFormat)
                .IsRequired();

            entity.Property(m => m.SourceFieldPath)
                .HasMaxLength(500);

            // TransformationPipeline stored as text (JSON)
            // Will be serialized/deserialized in application layer
            // Provider-independent: works with PostgreSQL, SQL Server, MySQL, SQLite
            entity.Property(m => m.TransformationPipeline)
                .IsRequired(false);

            entity.Property(m => m.IsActive)
                .IsRequired();

            // Audit fields
            entity.Property(m => m.CreatedBy)
                .IsRequired();

            entity.Property(m => m.CreatedOn)
                .IsRequired();

            entity.Property(m => m.LastModifiedBy)
                .IsRequired(false);

            entity.Property(m => m.LastModifiedOn)
                .IsRequired(false);

            // Indexes
            entity.HasIndex(m => new { m.FieldId, m.DataFormat, m.IsActive })
                .HasDatabaseName("IX_FieldMappings_FieldId_DataFormat_IsActive");
        });
    }

    private void ConfigureMeasurementDateMapping(ModelBuilder builder)
    {
        builder.Entity<MeasurementDateMapping>(entity =>
        {
            entity.HasKey(m => m.Id);

            // Mapping configuration
            entity.Property(m => m.DataFormat)
                .IsRequired();

            entity.Property(m => m.SourceFieldPath)
                .HasMaxLength(500);

            // TransformationPipeline stored as text (JSON)
            entity.Property(m => m.TransformationPipeline)
                .IsRequired(false);

            entity.Property(m => m.IsActive)
                .IsRequired();

            // Audit fields
            entity.Property(m => m.CreatedBy)
                .IsRequired();

            entity.Property(m => m.CreatedOn)
                .IsRequired();

            entity.Property(m => m.LastModifiedBy)
                .IsRequired(false);

            entity.Property(m => m.LastModifiedOn)
                .IsRequired(false);

            // Indexes
            // Note: Filtered index for unique constraint on active mappings
            // Will be created in migration with provider-specific syntax if needed
            entity.HasIndex(m => new { m.DeviceId, m.DataFormat, m.IsActive })
                .HasDatabaseName("IX_MeasurementDateMappings_DeviceId_DataFormat_IsActive");
        });
    }

    private void ConfigureDeviceMeasurement(ModelBuilder builder)
    {
        builder.Entity<DeviceMeasurement>(entity =>
        {
            entity.HasKey(m => m.Id);

            // Raw data storage (unlimited length)
            entity.Property(m => m.RawData)
                .IsRequired(false);

            entity.Property(m => m.DataFormat)
                .IsRequired();

            // Timestamps
            entity.Property(m => m.CreatedOn)
                .IsRequired();

            entity.Property(m => m.MeasurementDate)
                .IsRequired();

            // Parsing status
            entity.Property(m => m.ParsedSuccessfully)
                .IsRequired();

            entity.Property(m => m.ParsingErrors)
                .IsRequired(false);

            // Indexes for efficient time-series queries
            entity.HasIndex(m => new { m.DeviceId, m.MeasurementDate })
                .IsDescending(false, true) // DeviceId ASC, MeasurementDate DESC
                .HasDatabaseName("IX_DeviceMeasurements_DeviceId_MeasurementDate");

            entity.HasIndex(m => m.CreatedOn)
                .HasDatabaseName("IX_DeviceMeasurements_CreatedOn");

            entity.HasIndex(m => new { m.DeviceId, m.ParsedSuccessfully })
                .HasDatabaseName("IX_DeviceMeasurements_DeviceId_ParsedSuccessfully");

            // Navigation properties
            entity.HasMany(m => m.FieldValues)
                .WithOne(fv => fv.Measurement)
                .HasForeignKey(fv => fv.MeasurementId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureFieldMeasurementValue(ModelBuilder builder)
    {
        builder.Entity<FieldMeasurementValue>(entity =>
        {
            entity.HasKey(fv => fv.Id);

            // Value stored as string for flexibility
            entity.Property(fv => fv.Value)
                .IsRequired()
                .HasMaxLength(500);

            // Indexes for efficient queries
            entity.HasIndex(fv => new { fv.MeasurementId, fv.FieldId })
                .HasDatabaseName("IX_FieldMeasurementValues_MeasurementId_FieldId");

            entity.HasIndex(fv => fv.FieldId)
                .HasDatabaseName("IX_FieldMeasurementValues_FieldId");
        });
    }

    private void ConfigureDeviceAccessPermission(ModelBuilder builder)
    {
        builder.Entity<DeviceAccessPermission>(entity =>
        {
            entity.HasKey(ap => ap.Id);

            // Permission configuration
            entity.Property(ap => ap.UserId)
                .IsRequired(false);

            entity.Property(ap => ap.RoleId)
                .IsRequired(false);

            entity.Property(ap => ap.GroupId)
                .IsRequired(false);

            entity.Property(ap => ap.PermissionType)
                .IsRequired();

            // Audit fields
            entity.Property(ap => ap.CreatedBy)
                .IsRequired();

            entity.Property(ap => ap.CreatedOn)
                .IsRequired();

            entity.Property(ap => ap.LastModifiedBy)
                .IsRequired(false);

            entity.Property(ap => ap.LastModifiedOn)
                .IsRequired(false);

            // Indexes for permission lookups
            entity.HasIndex(ap => new { ap.DeviceId, ap.UserId })
                .HasDatabaseName("IX_DeviceAccessPermissions_DeviceId_UserId");

            entity.HasIndex(ap => new { ap.DeviceId, ap.RoleId })
                .HasDatabaseName("IX_DeviceAccessPermissions_DeviceId_RoleId");

            entity.HasIndex(ap => new { ap.DeviceId, ap.GroupId })
                .HasDatabaseName("IX_DeviceAccessPermissions_DeviceId_GroupId");

            // Note: Validation that at least one of UserId, RoleId, or GroupId is set
            // will be enforced in application layer (DTO validation)
            // This ensures database provider independence
        });
    }
}