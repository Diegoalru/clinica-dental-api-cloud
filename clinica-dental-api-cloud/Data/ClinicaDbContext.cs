using clinica_dental_api_cloud.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace clinica_dental_api_cloud.Data;

public partial class ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : DbContext(options)
{
    public virtual DbSet<Appointment> Appointments { get; set; }

    public virtual DbSet<AppointmentLog> AppointmentLogs { get; set; }

    public virtual DbSet<Dentist> Dentists { get; set; }

    public virtual DbSet<DentistBlock> DentistBlocks { get; set; }

    public virtual DbSet<DentistSchedule> DentistSchedules { get; set; }

    public virtual DbSet<DentistService> DentistServices { get; set; }

    public virtual DbSet<MedicalRecord> MedicalRecords { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Patient> Patients { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Specialty> Specialties { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("btree_gist");

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.IdAppointment).HasName("appointments_pkey");

            entity.ToTable("appointments");

            entity.HasIndex(e => new { e.IdDentist, e.StartsAt }, "idx_appointments_dentist_start");

            entity.HasIndex(e => new { e.IdPatient, e.StartsAt }, "idx_appointments_patient_start");

            entity.Property(e => e.IdAppointment)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_appointment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.EndsAt).HasColumnName("ends_at");
            entity.Property(e => e.IdDentist).HasColumnName("id_dentist");
            entity.Property(e => e.IdPatient).HasColumnName("id_patient");
            entity.Property(e => e.IdService).HasColumnName("id_service");
            entity.Property(e => e.PriceSnapshot)
                .HasPrecision(10, 2)
                .HasColumnName("price_snapshot");
            entity.Property(e => e.StartsAt).HasColumnName("starts_at");
            entity.Property(e => e.Status)
                .HasMaxLength(15)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdPatientNavigation).WithMany(p => p.Appointments)
                .HasForeignKey(d => d.IdPatient)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("appointments_id_patient_fkey");

            entity.HasOne(d => d.DentistService).WithMany(p => p.Appointments)
                .HasForeignKey(d => new { d.IdDentist, d.IdService })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("appointments_id_dentist_id_service_fkey");
        });

        modelBuilder.Entity<AppointmentLog>(entity =>
        {
            entity.HasKey(e => e.IdLog).HasName("appointment_logs_pkey");

            entity.ToTable("appointment_logs");

            entity.HasIndex(e => e.IdAppointment, "idx_appointment_logs_appointment");

            entity.Property(e => e.IdLog)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_log");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("changed_at");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.IdAppointment).HasColumnName("id_appointment");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(15)
                .HasColumnName("new_status");
            entity.Property(e => e.PreviousStatus)
                .HasMaxLength(15)
                .HasColumnName("previous_status");
            entity.Property(e => e.Remarks)
                .HasMaxLength(500)
                .HasColumnName("remarks");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.AppointmentLogs)
                .HasForeignKey(d => d.ChangedBy)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("appointment_logs_changed_by_fkey");

            entity.HasOne(d => d.IdAppointmentNavigation).WithMany(p => p.AppointmentLogs)
                .HasForeignKey(d => d.IdAppointment)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("appointment_logs_id_appointment_fkey");
        });

        modelBuilder.Entity<Dentist>(entity =>
        {
            entity.HasKey(e => e.IdDentist).HasName("dentists_pkey");

            entity.ToTable("dentists");

            entity.HasIndex(e => e.IdUser, "dentists_id_user_key").IsUnique();

            entity.HasIndex(e => e.LicenseNumber, "dentists_license_number_key").IsUnique();

            entity.Property(e => e.IdDentist)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_dentist");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IdSpecialty).HasColumnName("id_specialty");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.LicenseNumber)
                .HasMaxLength(30)
                .HasColumnName("license_number");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasDefaultValueSql("'ACTIVE'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdSpecialtyNavigation).WithMany(p => p.Dentists)
                .HasForeignKey(d => d.IdSpecialty)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("dentists_id_specialty_fkey");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Dentist)
                .HasForeignKey<Dentist>(d => d.IdUser)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("dentists_id_user_fkey");
        });

        modelBuilder.Entity<DentistBlock>(entity =>
        {
            entity.HasKey(e => e.IdBlock).HasName("dentist_blocks_pkey");

            entity.ToTable("dentist_blocks");

            entity.Property(e => e.IdBlock)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_block");
            entity.Property(e => e.EndsAt).HasColumnName("ends_at");
            entity.Property(e => e.IdDentist).HasColumnName("id_dentist");
            entity.Property(e => e.Reason)
                .HasMaxLength(255)
                .HasColumnName("reason");
            entity.Property(e => e.StartsAt).HasColumnName("starts_at");

            entity.HasOne(d => d.IdDentistNavigation).WithMany(p => p.DentistBlocks)
                .HasForeignKey(d => d.IdDentist)
                .HasConstraintName("dentist_blocks_id_dentist_fkey");
        });

        modelBuilder.Entity<DentistSchedule>(entity =>
        {
            entity.HasKey(e => e.IdSchedule).HasName("dentist_schedules_pkey");

            entity.ToTable("dentist_schedules");

            entity.HasIndex(e => new { e.IdDentist, e.DayOfWeek, e.StartTime },
                "dentist_schedules_id_dentist_day_of_week_start_time_key").IsUnique();

            entity.Property(e => e.IdSchedule)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_schedule");
            entity.Property(e => e.DayOfWeek).HasColumnName("day_of_week");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IdDentist).HasColumnName("id_dentist");
            entity.Property(e => e.StartTime).HasColumnName("start_time");

            entity.HasOne(d => d.IdDentistNavigation).WithMany(p => p.DentistSchedules)
                .HasForeignKey(d => d.IdDentist)
                .HasConstraintName("dentist_schedules_id_dentist_fkey");
        });

        modelBuilder.Entity<DentistService>(entity =>
        {
            entity.HasKey(e => new { e.IdDentist, e.IdService }).HasName("dentist_services_pkey");

            entity.ToTable("dentist_services");

            entity.Property(e => e.IdDentist).HasColumnName("id_dentist");
            entity.Property(e => e.IdService).HasColumnName("id_service");

            entity.HasOne(d => d.IdDentistNavigation).WithMany(p => p.DentistServices)
                .HasForeignKey(d => d.IdDentist)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("dentist_services_id_dentist_fkey");

            entity.HasOne(d => d.IdServiceNavigation).WithMany(p => p.DentistServices)
                .HasForeignKey(d => d.IdService)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("dentist_services_id_service_fkey");
        });

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.IdRecord).HasName("medical_records_pkey");

            entity.ToTable("medical_records");

            entity.HasIndex(e => e.IdAppointment, "medical_records_id_appointment_key").IsUnique();

            entity.Property(e => e.IdRecord)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_record");
            entity.Property(e => e.ClinicalNotes).HasColumnName("clinical_notes");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Diagnosis).HasColumnName("diagnosis");
            entity.Property(e => e.IdAppointment).HasColumnName("id_appointment");
            entity.Property(e => e.PhysicalExam).HasColumnName("physical_exam");
            entity.Property(e => e.Prescriptions).HasColumnName("prescriptions");
            entity.Property(e => e.Symptoms).HasColumnName("symptoms");
            entity.Property(e => e.TreatmentPlan).HasColumnName("treatment_plan");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdAppointmentNavigation).WithOne(p => p.MedicalRecord)
                .HasForeignKey<MedicalRecord>(d => d.IdAppointment)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("medical_records_id_appointment_fkey");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.IdNotification).HasName("notifications_pkey");

            entity.ToTable("notifications");

            entity.HasIndex(e => e.ScheduledFor, "idx_notifications_pending")
                .HasFilter("((status)::text = 'PENDING'::text)");

            entity.Property(e => e.IdNotification)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_notification");
            entity.Property(e => e.Channel)
                .HasMaxLength(10)
                .HasDefaultValueSql("'EMAIL'::character varying")
                .HasColumnName("channel");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IdAppointment).HasColumnName("id_appointment");
            entity.Property(e => e.Recipient)
                .HasMaxLength(255)
                .HasColumnName("recipient");
            entity.Property(e => e.RetryCount).HasColumnName("retry_count");
            entity.Property(e => e.ScheduledFor).HasColumnName("scheduled_for");
            entity.Property(e => e.SentAt).HasColumnName("sent_at");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasDefaultValueSql("'PENDING'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");

            entity.HasOne(d => d.IdAppointmentNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.IdAppointment)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("notifications_id_appointment_fkey");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.IdPatient).HasName("patients_pkey");

            entity.ToTable("patients");

            entity.HasIndex(e => e.IdUser, "patients_id_user_key").IsUnique();

            entity.HasIndex(e => e.Identification, "patients_identification_key").IsUnique();

            entity.Property(e => e.IdPatient)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_patient");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DataConsentAt).HasColumnName("data_consent_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EmergencyContact)
                .HasMaxLength(200)
                .HasColumnName("emergency_contact");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.Identification)
                .HasMaxLength(30)
                .HasColumnName("identification");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasDefaultValueSql("'ACTIVE'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Patient)
                .HasForeignKey<Patient>(d => d.IdUser)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("patients_id_user_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("roles_pkey");

            entity.ToTable("roles");

            entity.HasIndex(e => e.Name, "roles_name_key").IsUnique();

            entity.Property(e => e.IdRole)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_role");
            entity.Property(e => e.Description)
                .HasMaxLength(200)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(30)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.IdService).HasName("services_pkey");

            entity.ToTable("services");

            entity.HasIndex(e => e.Name, "services_name_key").IsUnique();

            entity.Property(e => e.IdService)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_service");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.DurationMinutes).HasColumnName("duration_minutes");
            entity.Property(e => e.Name)
                .HasMaxLength(120)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasDefaultValueSql("'ACTIVE'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Specialty>(entity =>
        {
            entity.HasKey(e => e.IdSpecialty).HasName("specialties_pkey");

            entity.ToTable("specialties");

            entity.HasIndex(e => e.Name, "specialties_name_key").IsUnique();

            entity.Property(e => e.IdSpecialty)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_specialty");
            entity.Property(e => e.Description)
                .HasMaxLength(300)
                .HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.IdUser)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_user");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(150)
                .HasColumnName("full_name");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .HasDefaultValueSql("'ACTIVE'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("users_id_role_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}