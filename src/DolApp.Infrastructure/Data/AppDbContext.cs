using System;
using System.Collections.Generic;
using DolApp.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DolApp.Infrastructure.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<City> Cities { get; set; }

    public virtual DbSet<Clothe> Clothes { get; set; }

    public virtual DbSet<ClothingStatus> ClothingStatuses { get; set; }

    public virtual DbSet<Color> Colors { get; set; }

    public virtual DbSet<Comparison> Comparisons { get; set; }

    public virtual DbSet<ComparisonSession> ComparisonSessions { get; set; }

    public virtual DbSet<District> Districts { get; set; }

    public virtual DbSet<LabelChange> LabelChanges { get; set; }

    public virtual DbSet<LabelChangeStage> LabelChangeStages { get; set; }

    public virtual DbSet<LabelField> LabelFields { get; set; }

    public virtual DbSet<LearnedPickMode> LearnedPickModes { get; set; }

    public virtual DbSet<Method> Methods { get; set; }

    public virtual DbSet<ModelPrediction> ModelPredictions { get; set; }

    public virtual DbSet<OutfitChange> OutfitChanges { get; set; }

    public virtual DbSet<OutfitTemplate> OutfitTemplates { get; set; }

    public virtual DbSet<ResultStatus> ResultStatuses { get; set; }

    public virtual DbSet<Season> Seasons { get; set; }

    public virtual DbSet<Style> Styles { get; set; }

    public virtual DbSet<Subtype> Subtypes { get; set; }

    public virtual DbSet<TemplateRequirement> TemplateRequirements { get; set; }

    public virtual DbSet<Thickness> Thicknesses { get; set; }

    public virtual DbSet<TimesOfDay> TimesOfDays { get; set; }

    public virtual DbSet<Tone> Tones { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Wardrobe> Wardrobes { get; set; }

    public virtual DbSet<WardrobeSummary> WardrobeSummaries { get; set; }

    public virtual DbSet<WeatherCondition> WeatherConditions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresEnum("auth", "aal_level", new[] { "aal1", "aal2", "aal3" })
            .HasPostgresEnum("auth", "code_challenge_method", new[] { "s256", "plain" })
            .HasPostgresEnum("auth", "factor_status", new[] { "unverified", "verified" })
            .HasPostgresEnum("auth", "factor_type", new[] { "totp", "webauthn", "phone", "recovery_code" })
            .HasPostgresEnum("auth", "oauth_authorization_status", new[] { "pending", "approved", "denied", "expired" })
            .HasPostgresEnum("auth", "oauth_client_type", new[] { "public", "confidential" })
            .HasPostgresEnum("auth", "oauth_registration_type", new[] { "dynamic", "manual" })
            .HasPostgresEnum("auth", "oauth_response_type", new[] { "code" })
            .HasPostgresEnum("auth", "one_time_token_type", new[] { "confirmation_token", "reauthentication_token", "recovery_token", "email_change_token_new", "email_change_token_current", "phone_change_token" })
            .HasPostgresEnum("realtime", "action", new[] { "INSERT", "UPDATE", "DELETE", "TRUNCATE", "ERROR" })
            .HasPostgresEnum("realtime", "equality_op", new[] { "eq", "neq", "lt", "lte", "gt", "gte", "in", "like", "ilike", "is", "match", "imatch", "isdistinct" })
            .HasPostgresEnum("storage", "buckettype", new[] { "STANDARD", "ANALYTICS", "VECTOR" })
            .HasPostgresExtension("extensions", "pg_stat_statements")
            .HasPostgresExtension("extensions", "pgcrypto")
            .HasPostgresExtension("extensions", "uuid-ossp")
            .HasPostgresExtension("vault", "supabase_vault");

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("categories_pkey");

            entity.ToTable("categories", tb => tb.HasComment("6 ana kategori. Kullanıcı seçmez; C# alt türden yazar."));

            entity.HasIndex(e => e.Value, "categories_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("cities_pkey");

            entity.ToTable("cities", tb => tb.HasComment("81 il, plaka kodu ve il merkezi koordinatı."));

            entity.HasIndex(e => e.PlateCode, "cities_plate_code_key").IsUnique();

            entity.HasIndex(e => e.Value, "cities_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Latitude)
                .HasPrecision(8, 5)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(8, 5)
                .HasColumnName("longitude");
            entity.Property(e => e.PlateCode).HasColumnName("plate_code");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Clothe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("clothes_pkey");

            entity.ToTable("clothes", tb => tb.HasComment("Dolaptaki her parça. Fotoğraf yüklenince taslak, Kaydet ile onayli."));

            entity.HasIndex(e => new { e.WardrobeId, e.StatusId }, "clothes_wardrobe_status_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedAt).HasColumnName("approved_at");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.ImagePath).HasColumnName("image_path");
            entity.Property(e => e.IsPatterned).HasColumnName("is_patterned");
            entity.Property(e => e.MainColorId).HasColumnName("main_color_id");
            entity.Property(e => e.MaxTemp).HasColumnName("max_temp");
            entity.Property(e => e.MinTemp).HasColumnName("min_temp");
            entity.Property(e => e.RawColor).HasColumnName("raw_color");
            entity.Property(e => e.SeasonIds).HasColumnName("season_ids");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StyleIds).HasColumnName("style_ids");
            entity.Property(e => e.SubtypeId).HasColumnName("subtype_id");
            entity.Property(e => e.ThicknessId).HasColumnName("thickness_id");
            entity.Property(e => e.ToneId).HasColumnName("tone_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.WardrobeId).HasColumnName("wardrobe_id");

            entity.HasOne(d => d.Category).WithMany(p => p.Clothes)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("clothes_category_id_fkey");

            entity.HasOne(d => d.MainColor).WithMany(p => p.Clothes)
                .HasForeignKey(d => d.MainColorId)
                .HasConstraintName("clothes_main_color_id_fkey");

            entity.HasOne(d => d.Status).WithMany(p => p.Clothes)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("clothes_status_id_fkey");

            entity.HasOne(d => d.Subtype).WithMany(p => p.Clothes)
                .HasForeignKey(d => d.SubtypeId)
                .HasConstraintName("clothes_subtype_id_fkey");

            entity.HasOne(d => d.Thickness).WithMany(p => p.Clothes)
                .HasForeignKey(d => d.ThicknessId)
                .HasConstraintName("clothes_thickness_id_fkey");

            entity.HasOne(d => d.Tone).WithMany(p => p.Clothes)
                .HasForeignKey(d => d.ToneId)
                .HasConstraintName("clothes_tone_id_fkey");

            entity.HasOne(d => d.Wardrobe).WithMany(p => p.Clothes)
                .HasForeignKey(d => d.WardrobeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("clothes_wardrobe_id_fkey");
        });

        modelBuilder.Entity<ClothingStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("clothing_statuses_pkey");

            entity.ToTable("clothing_statuses", tb => tb.HasComment("Parça durumu: taslak, onayli, silindi. Dolap ve kombine yalnız onayli girer."));

            entity.HasIndex(e => e.Value, "clothing_statuses_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Color>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("colors_pkey");

            entity.ToTable("colors", tb => tb.HasComment("13 ana renk. Nötr/canlı grubu Python'dadır."));

            entity.HasIndex(e => e.Value, "colors_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Comparison>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comparisons_pkey");

            entity.ToTable("comparisons", tb => tb.HasComment("Her iki kartlı karşılaştırma. Sonucu yalnız C# yazar."));

            entity.HasIndex(e => new { e.SessionId, e.SequenceNo }, "comparisons_session_id_sequence_no_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Accessories).HasColumnName("accessories");
            entity.Property(e => e.CompletedAt).HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FinalOutfit).HasColumnName("final_outfit");
            entity.Property(e => e.LearnedOutfit).HasColumnName("learned_outfit");
            entity.Property(e => e.LearnedPickModeId).HasColumnName("learned_pick_mode_id");
            entity.Property(e => e.LeftMethodId).HasColumnName("left_method_id");
            entity.Property(e => e.ResultId).HasColumnName("result_id");
            entity.Property(e => e.RuleOutfit).HasColumnName("rule_outfit");
            entity.Property(e => e.SelectedAt).HasColumnName("selected_at");
            entity.Property(e => e.SequenceNo).HasColumnName("sequence_no");
            entity.Property(e => e.SessionId).HasColumnName("session_id");

            entity.HasOne(d => d.LearnedPickMode).WithMany(p => p.Comparisons)
                .HasForeignKey(d => d.LearnedPickModeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comparisons_learned_pick_mode_id_fkey");

            entity.HasOne(d => d.LeftMethod).WithMany(p => p.Comparisons)
                .HasForeignKey(d => d.LeftMethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comparisons_left_method_id_fkey");

            entity.HasOne(d => d.Result).WithMany(p => p.Comparisons)
                .HasForeignKey(d => d.ResultId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comparisons_result_id_fkey");

            entity.HasOne(d => d.Session).WithMany(p => p.Comparisons)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comparisons_session_id_fkey");
        });

        modelBuilder.Entity<ComparisonSession>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("comparison_sessions_pkey");

            entity.ToTable("comparison_sessions", tb => tb.HasComment("Bir kombin formu gönderimi ve o anki hava."));

            entity.HasIndex(e => e.UserId, "comparison_sessions_user_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DistrictId).HasColumnName("district_id");
            entity.Property(e => e.EventStyleId).HasColumnName("event_style_id");
            entity.Property(e => e.FeelsLikeTemp)
                .HasPrecision(4, 1)
                .HasColumnName("feels_like_temp");
            entity.Property(e => e.TargetDate).HasColumnName("target_date");
            entity.Property(e => e.TimeOfDayId).HasColumnName("time_of_day_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.WantsAccessories).HasColumnName("wants_accessories");
            entity.Property(e => e.WeatherConditionId).HasColumnName("weather_condition_id");

            entity.HasOne(d => d.City).WithMany(p => p.ComparisonSessions)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comparison_sessions_city_id_fkey");

            entity.HasOne(d => d.District).WithMany(p => p.ComparisonSessionDistricts)
                .HasForeignKey(d => d.DistrictId)
                .HasConstraintName("comparison_sessions_district_id_fkey");

            entity.HasOne(d => d.EventStyle).WithMany(p => p.ComparisonSessions)
                .HasForeignKey(d => d.EventStyleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comparison_sessions_event_style_id_fkey");

            entity.HasOne(d => d.TimeOfDay).WithMany(p => p.ComparisonSessions)
                .HasForeignKey(d => d.TimeOfDayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comparison_sessions_time_of_day_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.ComparisonSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comparison_sessions_user_id_fkey");

            entity.HasOne(d => d.WeatherCondition).WithMany(p => p.ComparisonSessions)
                .HasForeignKey(d => d.WeatherConditionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("comparison_sessions_weather_condition_id_fkey");

            entity.HasOne(d => d.DistrictNavigation).WithMany(p => p.ComparisonSessionDistrictNavigations)
                .HasPrincipalKey(p => new { p.CityId, p.Id })
                .HasForeignKey(d => new { d.CityId, d.DistrictId })
                .HasConstraintName("comparison_sessions_city_id_district_id_fkey");
        });

        modelBuilder.Entity<District>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("districts_pkey");

            entity.ToTable("districts", tb => tb.HasComment("İlçeler ve ilçe merkezi koordinatı. value il içinde tekildir."));

            entity.HasIndex(e => new { e.CityId, e.Id }, "districts_city_id_id_key").IsUnique();

            entity.HasIndex(e => new { e.CityId, e.Value }, "districts_city_id_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CityId).HasColumnName("city_id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Latitude)
                .HasPrecision(8, 5)
                .HasColumnName("latitude");
            entity.Property(e => e.Longitude)
                .HasPrecision(8, 5)
                .HasColumnName("longitude");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.City).WithMany(p => p.Districts)
                .HasForeignKey(d => d.CityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("districts_city_id_fkey");
        });

        modelBuilder.Entity<LabelChange>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("label_changes_pkey");

            entity.ToTable("label_changes", tb => tb.HasComment("Etiket düzeltme logu. Güncellenmez, silinmez."));

            entity.HasIndex(e => e.ClothingId, "label_changes_clothing_idx");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ClothingId).HasColumnName("clothing_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.FieldId).HasColumnName("field_id");
            entity.Property(e => e.NewValue).HasColumnName("new_value");
            entity.Property(e => e.OldValue).HasColumnName("old_value");
            entity.Property(e => e.StageId).HasColumnName("stage_id");

            entity.HasOne(d => d.Clothing).WithMany(p => p.LabelChanges)
                .HasForeignKey(d => d.ClothingId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("label_changes_clothing_id_fkey");

            entity.HasOne(d => d.Field).WithMany(p => p.LabelChanges)
                .HasForeignKey(d => d.FieldId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("label_changes_field_id_fkey");

            entity.HasOne(d => d.Stage).WithMany(p => p.LabelChanges)
                .HasForeignKey(d => d.StageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("label_changes_stage_id_fkey");
        });

        modelBuilder.Entity<LabelChangeStage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("label_change_stages_pkey");

            entity.ToTable("label_change_stages", tb => tb.HasComment("Düzeltmenin yapıldığı ekran: onay ekranı ya da parça detayı."));

            entity.HasIndex(e => e.Value, "label_change_stages_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<LabelField>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("label_fields_pkey");

            entity.ToTable("label_fields", tb => tb.HasComment("Düzeltilebilen etiket alanları (label_changes)."));

            entity.HasIndex(e => e.Value, "label_fields_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<LearnedPickMode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("learned_pick_modes_pkey");

            entity.ToTable("learned_pick_modes", tb => tb.HasComment("Öğrenen kartın nasıl seçildiği. is_random analizde model seçimlerini rastgele kartlardan ayırır."));

            entity.HasIndex(e => e.Value, "learned_pick_modes_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.IsRandom).HasColumnName("is_random");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Method>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("methods_pkey");

            entity.ToTable("methods", tb => tb.HasComment("Kombin ya da parça seçen yöntemler. Kullanıcıya gösterilmez."));

            entity.HasIndex(e => e.Value, "methods_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<ModelPrediction>(entity =>
        {
            entity.HasKey(e => e.ClothingId).HasName("model_predictions_pkey");

            entity.ToTable("model_predictions", tb => tb.HasComment("Modelin ve renk analizinin ilk cevabı. Güncellenmez."));

            entity.Property(e => e.ClothingId)
                .ValueGeneratedNever()
                .HasColumnName("clothing_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.IsPatterned).HasColumnName("is_patterned");
            entity.Property(e => e.MainColorId).HasColumnName("main_color_id");
            entity.Property(e => e.RawColor).HasColumnName("raw_color");
            entity.Property(e => e.SubtypeConfidence)
                .HasPrecision(4, 3)
                .HasColumnName("subtype_confidence");
            entity.Property(e => e.SubtypeId).HasColumnName("subtype_id");
            entity.Property(e => e.ToneId).HasColumnName("tone_id");

            entity.HasOne(d => d.Clothing).WithOne(p => p.ModelPrediction)
                .HasForeignKey<ModelPrediction>(d => d.ClothingId)
                .HasConstraintName("model_predictions_clothing_id_fkey");

            entity.HasOne(d => d.MainColor).WithMany(p => p.ModelPredictions)
                .HasForeignKey(d => d.MainColorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("model_predictions_main_color_id_fkey");

            entity.HasOne(d => d.Subtype).WithMany(p => p.ModelPredictions)
                .HasForeignKey(d => d.SubtypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("model_predictions_subtype_id_fkey");

            entity.HasOne(d => d.Tone).WithMany(p => p.ModelPredictions)
                .HasForeignKey(d => d.ToneId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("model_predictions_tone_id_fkey");
        });

        modelBuilder.Entity<OutfitChange>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("outfit_changes_pkey");

            entity.ToTable("outfit_changes", tb => tb.HasComment("Seçilen kartta her değiştir/kaldır. Güncellenmez, silinmez."));

            entity.HasIndex(e => new { e.ComparisonId, e.OrderNo }, "outfit_changes_comparison_id_order_no_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.ComparisonId).HasColumnName("comparison_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.MethodId).HasColumnName("method_id");
            entity.Property(e => e.NewItemId).HasColumnName("new_item_id");
            entity.Property(e => e.OldItemId).HasColumnName("old_item_id");
            entity.Property(e => e.OrderNo).HasColumnName("order_no");

            entity.HasOne(d => d.Category).WithMany(p => p.OutfitChanges)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("outfit_changes_category_id_fkey");

            entity.HasOne(d => d.Comparison).WithMany(p => p.OutfitChanges)
                .HasForeignKey(d => d.ComparisonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("outfit_changes_comparison_id_fkey");

            entity.HasOne(d => d.Method).WithMany(p => p.OutfitChanges)
                .HasForeignKey(d => d.MethodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("outfit_changes_method_id_fkey");

            entity.HasOne(d => d.NewItem).WithMany(p => p.OutfitChangeNewItems)
                .HasForeignKey(d => d.NewItemId)
                .HasConstraintName("outfit_changes_new_item_id_fkey");

            entity.HasOne(d => d.OldItem).WithMany(p => p.OutfitChangeOldItems)
                .HasForeignKey(d => d.OldItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("outfit_changes_old_item_id_fkey");
        });

        modelBuilder.Entity<OutfitTemplate>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("outfit_templates_pkey");

            entity.ToTable("outfit_templates", tb => tb.HasComment("Kombin şablonları: ust_alt, tek_parca."));

            entity.HasIndex(e => e.Value, "outfit_templates_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<ResultStatus>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("result_statuses_pkey");

            entity.ToTable("result_statuses", tb => tb.HasComment("Karşılaştırmanın 14 olası sonucu. C# ve Python result_key ile arar."));

            entity.HasIndex(e => e.ResultKey, "result_statuses_result_key_key").IsUnique();

            entity.HasIndex(e => e.Value, "result_statuses_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Acceptance).HasColumnName("acceptance");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.InAcceptanceAnalysis).HasColumnName("in_acceptance_analysis");
            entity.Property(e => e.InPreferenceAnalysis).HasColumnName("in_preference_analysis");
            entity.Property(e => e.ResultKey).HasColumnName("result_key");
            entity.Property(e => e.Selected).HasColumnName("selected");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Season>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("seasons_pkey");

            entity.ToTable("seasons", tb => tb.HasComment("4 mevsim; yalnız etiket, öneride kullanılmaz."));

            entity.HasIndex(e => e.Value, "seasons_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Style>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("styles_pkey");

            entity.ToTable("styles", tb => tb.HasComment("4 tarz; kombin formundaki etkinlik türü de buradan."));

            entity.HasIndex(e => e.Value, "styles_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Subtype>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("subtypes_pkey");

            entity.ToTable("subtypes", tb => tb.HasComment("35 alt tür ve varsayılanları. FashionCLIP etiketleri value ile birebir aynıdır."));

            entity.HasIndex(e => e.Value, "subtypes_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AvoidWhenWet).HasColumnName("avoid_when_wet");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.DefaultSeasonIds).HasColumnName("default_season_ids");
            entity.Property(e => e.DefaultStyleIds).HasColumnName("default_style_ids");
            entity.Property(e => e.DefaultThicknessId).HasColumnName("default_thickness_id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.MaxTemp).HasColumnName("max_temp");
            entity.Property(e => e.MinTemp).HasColumnName("min_temp");
            entity.Property(e => e.PreferredWhenHot).HasColumnName("preferred_when_hot");
            entity.Property(e => e.PreferredWhenWet).HasColumnName("preferred_when_wet");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Category).WithMany(p => p.Subtypes)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("subtypes_category_id_fkey");

            entity.HasOne(d => d.DefaultThickness).WithMany(p => p.Subtypes)
                .HasForeignKey(d => d.DefaultThicknessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("subtypes_default_thickness_id_fkey");
        });

        modelBuilder.Entity<TemplateRequirement>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("template_requirements_pkey");

            entity.ToTable("template_requirements", tb => tb.HasComment("Şablon başına gereken en az onaylı parça (dolap şartı)."));

            entity.HasIndex(e => new { e.TemplateId, e.CategoryId }, "template_requirements_template_id_category_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.MinRequired).HasColumnName("min_required");
            entity.Property(e => e.TemplateId).HasColumnName("template_id");

            entity.HasOne(d => d.Category).WithMany(p => p.TemplateRequirements)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("template_requirements_category_id_fkey");

            entity.HasOne(d => d.Template).WithMany(p => p.TemplateRequirements)
                .HasForeignKey(d => d.TemplateId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("template_requirements_template_id_fkey");
        });

        modelBuilder.Entity<Thickness>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("thicknesses_pkey");

            entity.ToTable("thicknesses", tb => tb.HasComment("3 kalınlık; yalnız etiket, öneride kullanılmaz."));

            entity.HasIndex(e => e.Value, "thicknesses_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<TimesOfDay>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("times_of_day_pkey");

            entity.ToTable("times_of_day", tb => tb.HasComment("4 vakit ve hava hesabının saat aralığı (end_hour dahil)."));

            entity.HasIndex(e => e.Value, "times_of_day_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.EndHour).HasColumnName("end_hour");
            entity.Property(e => e.IsDaytime).HasColumnName("is_daytime");
            entity.Property(e => e.StartHour).HasColumnName("start_hour");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<Tone>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tones_pkey");

            entity.ToTable("tones", tb => tb.HasComment("3 ton."));

            entity.HasIndex(e => e.Value, "tones_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users", tb => tb.HasComment("Profil, onam ve silme bilgisi. id Supabase giriş hesabından gelir; hesap silinince satır kimliksiz kalır."));

            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.BirthDate).HasColumnName("birth_date");
            entity.Property(e => e.ConsentAt).HasColumnName("consent_at");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DefaultCityId).HasColumnName("default_city_id");
            entity.Property(e => e.DefaultDistrictId).HasColumnName("default_district_id");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");
            entity.Property(e => e.Email).HasColumnName("email");
            entity.Property(e => e.FirstName).HasColumnName("first_name");
            entity.Property(e => e.IsConsentAccepted).HasColumnName("is_consent_accepted");
            entity.Property(e => e.IsDeleted).HasColumnName("is_deleted");
            entity.Property(e => e.IsOnboardingAccepted).HasColumnName("is_onboarding_accepted");
            entity.Property(e => e.LastName).HasColumnName("last_name");
            entity.Property(e => e.OnboardingAcceptedAt).HasColumnName("onboarding_accepted_at");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.DefaultCity).WithMany(p => p.Users)
                .HasForeignKey(d => d.DefaultCityId)
                .HasConstraintName("users_default_city_id_fkey");

            entity.HasOne(d => d.DefaultDistrict).WithMany(p => p.UserDefaultDistricts)
                .HasForeignKey(d => d.DefaultDistrictId)
                .HasConstraintName("users_default_district_id_fkey");

            entity.HasOne(d => d.District).WithMany(p => p.UserDistricts)
                .HasPrincipalKey(p => new { p.CityId, p.Id })
                .HasForeignKey(d => new { d.DefaultCityId, d.DefaultDistrictId })
                .HasConstraintName("users_default_city_id_default_district_id_fkey");
        });

        modelBuilder.Entity<Wardrobe>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("wardrobes_pkey");

            entity.ToTable("wardrobes", tb => tb.HasComment("Her kullanıcının tek dolabı."));

            entity.HasIndex(e => e.UserId, "wardrobes_user_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.Wardrobe)
                .HasForeignKey<Wardrobe>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("wardrobes_user_id_fkey");
        });

        modelBuilder.Entity<WardrobeSummary>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("wardrobe_summary")
                .HasAnnotation("Npgsql:StorageParameter:security_invoker", "true");

            entity.Property(e => e.AccessoryCount).HasColumnName("accessory_count");
            entity.Property(e => e.BottomCount).HasColumnName("bottom_count");
            entity.Property(e => e.OnePieceCount).HasColumnName("one_piece_count");
            entity.Property(e => e.OuterCount).HasColumnName("outer_count");
            entity.Property(e => e.ShoesCount).HasColumnName("shoes_count");
            entity.Property(e => e.TopCount).HasColumnName("top_count");
            entity.Property(e => e.TotalCount).HasColumnName("total_count");
            entity.Property(e => e.WardrobeId).HasColumnName("wardrobe_id");
        });

        modelBuilder.Entity<WeatherCondition>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("weather_conditions_pkey");

            entity.ToTable("weather_conditions", tb => tb.HasComment("Open-Meteo kodlarının Türkçe hava durumuna tek çeviri yeri."));

            entity.HasIndex(e => e.Severity, "weather_conditions_severity_key").IsUnique();

            entity.HasIndex(e => e.Value, "weather_conditions_value_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApiCodes).HasColumnName("api_codes");
            entity.Property(e => e.DisplayName).HasColumnName("display_name");
            entity.Property(e => e.IsSunny).HasColumnName("is_sunny");
            entity.Property(e => e.IsWet).HasColumnName("is_wet");
            entity.Property(e => e.Severity).HasColumnName("severity");
            entity.Property(e => e.Value).HasColumnName("value");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
