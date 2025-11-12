-- Add regional event fields to CustomEvents table
-- Migration: AddRegionalEventFields
-- Date: 2025-11-12

-- Add IsRegionalEvent column
ALTER TABLE "CustomEvents" 
ADD COLUMN IF NOT EXISTS "IsRegionalEvent" BOOLEAN NOT NULL DEFAULT false;

-- Add RegionalEventType column
ALTER TABLE "CustomEvents" 
ADD COLUMN IF NOT EXISTS "RegionalEventType" VARCHAR(100) NULL;

-- Add RegionalEventCountryCode column
ALTER TABLE "CustomEvents" 
ADD COLUMN IF NOT EXISTS "RegionalEventCountryCode" VARCHAR(10) NULL;

-- Create indexes for performance
CREATE INDEX IF NOT EXISTS "IX_CustomEvents_IsRegionalEvent" ON "CustomEvents" ("IsRegionalEvent");
CREATE INDEX IF NOT EXISTS "IX_CustomEvents_RegionalEventType" ON "CustomEvents" ("RegionalEventType");
CREATE INDEX IF NOT EXISTS "IX_CustomEvents_RegionalEventCountryCode" ON "CustomEvents" ("RegionalEventCountryCode");
