ALTER TABLE search_listing
    ADD COLUMN IF NOT EXISTS agent_id uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    ADD COLUMN IF NOT EXISTS listing_details_type character varying(100),
    ADD COLUMN IF NOT EXISTS education_score numeric(10,4),
    ADD COLUMN IF NOT EXISTS healthcare_score numeric(10,4),
    ADD COLUMN IF NOT EXISTS transport_score numeric(10,4),
    ADD COLUMN IF NOT EXISTS commerce_score numeric(10,4),
    ADD COLUMN IF NOT EXISTS leisure_score numeric(10,4),
    ADD COLUMN IF NOT EXISTS education_count integer,
    ADD COLUMN IF NOT EXISTS healthcare_count integer,
    ADD COLUMN IF NOT EXISTS transport_count integer,
    ADD COLUMN IF NOT EXISTS commerce_count integer,
    ADD COLUMN IF NOT EXISTS leisure_count integer,
    ADD COLUMN IF NOT EXISTS education_nearest_meters numeric(10,2),
    ADD COLUMN IF NOT EXISTS healthcare_nearest_meters numeric(10,2),
    ADD COLUMN IF NOT EXISTS transport_nearest_meters numeric(10,2),
    ADD COLUMN IF NOT EXISTS commerce_nearest_meters numeric(10,2),
    ADD COLUMN IF NOT EXISTS leisure_nearest_meters numeric(10,2),
    ADD COLUMN IF NOT EXISTS pharmacy_count_primary integer,
    ADD COLUMN IF NOT EXISTS store_count_primary integer,
    ADD COLUMN IF NOT EXISTS stops_count_primary integer,
    ADD COLUMN IF NOT EXISTS stop_groups_count_primary integer,
    ADD COLUMN IF NOT EXISTS stop_platforms_count_primary integer,
    ADD COLUMN IF NOT EXISTS parking_count_primary integer,
    ADD COLUMN IF NOT EXISTS school_count_primary integer,
    ADD COLUMN IF NOT EXISTS kindergarten_count_primary integer,
    ADD COLUMN IF NOT EXISTS clinic_count_primary integer,
    ADD COLUMN IF NOT EXISTS hospital_count_primary integer,
    ADD COLUMN IF NOT EXISTS nearest_pharmacy_walk_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_store_walk_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_stop_walk_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_stop_group_walk_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_stop_group_drive_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_stop_group_drive_minutes_estimated boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS nearest_stop_group_name character varying(300),
    ADD COLUMN IF NOT EXISTS nearest_stop_group_platform_count integer,
    ADD COLUMN IF NOT EXISTS nearest_parking_walk_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_parking_drive_minutes_estimated boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS nearest_school_walk_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_kindergarten_walk_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_clinic_drive_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_clinic_drive_minutes_estimated boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS nearest_hospital_drive_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_hospital_drive_minutes_estimated boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS parks_count_primary integer,
    ADD COLUMN IF NOT EXISTS nearest_park_walk_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS fitness_count_primary integer,
    ADD COLUMN IF NOT EXISTS nearest_fitness_walk_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_fitness_drive_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS museums_count_primary integer,
    ADD COLUMN IF NOT EXISTS nearest_museum_drive_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS theaters_count_primary integer,
    ADD COLUMN IF NOT EXISTS nearest_theater_drive_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS cinemas_count_primary integer,
    ADD COLUMN IF NOT EXISTS nearest_cinema_drive_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_metro_walk_minutes numeric(10,2),
    ADD COLUMN IF NOT EXISTS nearest_metro_name character varying(300),
    ADD COLUMN IF NOT EXISTS metro_found_in_primary_radius boolean NOT NULL DEFAULT false;

CREATE INDEX IF NOT EXISTS ix_search_listing_agent_id ON search_listing (agent_id);
CREATE INDEX IF NOT EXISTS ix_search_listing_building_type ON search_listing (building_type);
CREATE INDEX IF NOT EXISTS ix_search_listing_price_amount ON search_listing (price_amount);
CREATE INDEX IF NOT EXISTS ix_search_listing_published_date ON search_listing (published_date);
CREATE INDEX IF NOT EXISTS ix_search_listing_infrastructure_score ON search_listing (infrastructure_score);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_store_walk_minutes ON search_listing (nearest_store_walk_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_pharmacy_walk_minutes ON search_listing (nearest_pharmacy_walk_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_stop_walk_minutes ON search_listing (nearest_stop_walk_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_parking_walk_minutes ON search_listing (nearest_parking_walk_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_school_walk_minutes ON search_listing (nearest_school_walk_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_kindergarten_walk_minutes ON search_listing (nearest_kindergarten_walk_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_clinic_drive_minutes ON search_listing (nearest_clinic_drive_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_hospital_drive_minutes ON search_listing (nearest_hospital_drive_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_park_walk_minutes ON search_listing (nearest_park_walk_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_fitness_walk_minutes ON search_listing (nearest_fitness_walk_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_museum_drive_minutes ON search_listing (nearest_museum_drive_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_theater_drive_minutes ON search_listing (nearest_theater_drive_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_cinema_drive_minutes ON search_listing (nearest_cinema_drive_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_nearest_metro_walk_minutes ON search_listing (nearest_metro_walk_minutes);
CREATE INDEX IF NOT EXISTS ix_search_listing_metro_found_in_primary_radius ON search_listing (metro_found_in_primary_radius);
