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
    ADD COLUMN IF NOT EXISTS leisure_nearest_meters numeric(10,2);

CREATE INDEX IF NOT EXISTS ix_search_listing_agent_id ON search_listing (agent_id);
CREATE INDEX IF NOT EXISTS ix_search_listing_building_type ON search_listing (building_type);
CREATE INDEX IF NOT EXISTS ix_search_listing_price_amount ON search_listing (price_amount);
CREATE INDEX IF NOT EXISTS ix_search_listing_published_date ON search_listing (published_date);
CREATE INDEX IF NOT EXISTS ix_search_listing_infrastructure_score ON search_listing (infrastructure_score);
