CREATE TABLE "AdditionalBirthdays" (
    "Id" SERIAL PRIMARY KEY,
    "ContactId" INTEGER NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "BirthMonth" INTEGER NOT NULL,
    "BirthDay" INTEGER NOT NULL,
    "BirthYear" INTEGER NULL,
    "Relationship" VARCHAR(50) NULL,
    "CustomMessage" VARCHAR(500) NULL,
    "SendTo" VARCHAR(20) NOT NULL,
    "SendToGroupId" INTEGER NULL,
    "TimeZoneId" VARCHAR(100) NULL,
    "MessageHour" INTEGER NULL,
    "IsEnabled" BOOLEAN NOT NULL,
    "CreatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    "UpdatedAt" TIMESTAMP WITH TIME ZONE NOT NULL,
    CONSTRAINT "FK_AdditionalBirthdays_Contacts_ContactId" FOREIGN KEY ("ContactId") REFERENCES "Contacts"("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AdditionalBirthdays_Contacts_SendToGroupId" FOREIGN KEY ("SendToGroupId") REFERENCES "Contacts"("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_AdditionalBirthdays_BirthMonth_BirthDay" ON "AdditionalBirthdays" ("BirthMonth", "BirthDay");
CREATE INDEX "IX_AdditionalBirthdays_ContactId" ON "AdditionalBirthdays" ("ContactId");
CREATE INDEX "IX_AdditionalBirthdays_ContactId_IsEnabled" ON "AdditionalBirthdays" ("ContactId", "IsEnabled");
CREATE INDEX "IX_AdditionalBirthdays_SendToGroupId" ON "AdditionalBirthdays" ("SendToGroupId");
