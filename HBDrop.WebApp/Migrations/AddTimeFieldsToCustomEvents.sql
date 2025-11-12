-- Add TimeZoneId and MessageHour columns to CustomEvents table
-- Migration: AddTimeFieldsToCustomEvents
-- Date: 2025-11-12

-- Add TimeZoneId column
ALTER TABLE "CustomEvents" 
ADD COLUMN IF NOT EXISTS "TimeZoneId" VARCHAR(100) NULL;

-- Add MessageHour column
ALTER TABLE "CustomEvents" 
ADD COLUMN IF NOT EXISTS "MessageHour" INTEGER NULL
CHECK ("MessageHour" IS NULL OR ("MessageHour" >= 0 AND "MessageHour" <= 23));

-- Create index on TimeZoneId for performance
CREATE INDEX IF NOT EXISTS "IX_CustomEvents_TimeZoneId" ON "CustomEvents" ("TimeZoneId");

-- Create index on MessageHour for performance
CREATE INDEX IF NOT EXISTS "IX_CustomEvents_MessageHour" ON "CustomEvents" ("MessageHour");
