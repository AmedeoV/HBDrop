-- Migration: Add CustomEvents table
-- This adds support for custom events like Father's Day, Mother's Day, Anniversaries, etc.

CREATE TABLE IF NOT EXISTS "CustomEvents" (
    "Id" SERIAL PRIMARY KEY,
    "ContactId" INTEGER NOT NULL,
    "EventName" VARCHAR(200) NOT NULL,
    "EventMonth" INTEGER NOT NULL CHECK ("EventMonth" >= 1 AND "EventMonth" <= 12),
    "EventDay" INTEGER NOT NULL CHECK ("EventDay" >= 1 AND "EventDay" <= 31),
    "EventYear" INTEGER NULL CHECK ("EventYear" >= 1900 AND "EventYear" <= 2100),
    "CustomMessage" VARCHAR(1000) NULL,
    "GroupId" INTEGER NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "IsEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT "FK_CustomEvents_Contacts_ContactId" FOREIGN KEY ("ContactId") 
        REFERENCES "Contacts" ("Id") ON DELETE CASCADE
);

-- Create indexes for better query performance
CREATE INDEX IF NOT EXISTS "IX_CustomEvents_ContactId" ON "CustomEvents" ("ContactId");
CREATE INDEX IF NOT EXISTS "IX_CustomEvents_GroupId" ON "CustomEvents" ("GroupId");
CREATE INDEX IF NOT EXISTS "IX_CustomEvents_EventMonth_EventDay" ON "CustomEvents" ("EventMonth", "EventDay");
CREATE INDEX IF NOT EXISTS "IX_CustomEvents_IsEnabled" ON "CustomEvents" ("IsEnabled");
