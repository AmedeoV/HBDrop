-- Add GifUrl column to Birthday, AdditionalBirthday, and CustomEvent tables

ALTER TABLE "Birthdays" 
ADD COLUMN "GifUrl" VARCHAR(1000) NULL;

ALTER TABLE "AdditionalBirthdays" 
ADD COLUMN "GifUrl" VARCHAR(1000) NULL;

ALTER TABLE "CustomEvents" 
ADD COLUMN "GifUrl" VARCHAR(1000) NULL;
