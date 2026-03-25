ALTER TABLE poi
    ADD COLUMN IF NOT EXISTS subcategory_code character varying(120),
    ADD COLUMN IF NOT EXISTS source character varying(50);

CREATE INDEX IF NOT EXISTS ix_poi_subcategory_code ON poi (subcategory_code);

ALTER TABLE infrastructure_score
    ADD COLUMN IF NOT EXISTS detailed_summary_json jsonb NOT NULL DEFAULT '{}'::jsonb;
