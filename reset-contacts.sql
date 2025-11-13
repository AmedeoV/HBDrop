-- Delete all contacts and related data for user
DELETE FROM "Birthdays" WHERE "ContactId" IN (
    SELECT "Id" FROM "Contacts" WHERE "UserId" = 'cce6ecbc-1fa3-4ee1-9f4c-232dae5b1e0e'
);

DELETE FROM "Contacts" WHERE "UserId" = 'cce6ecbc-1fa3-4ee1-9f4c-232dae5b1e0e';

-- Show counts
SELECT 'Contacts deleted' as status;
SELECT COUNT(*) as remaining_contacts FROM "Contacts" WHERE "UserId" = 'cce6ecbc-1fa3-4ee1-9f4c-232dae5b1e0e';
