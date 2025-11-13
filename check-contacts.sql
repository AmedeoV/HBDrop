-- Check contacts imported
SELECT COUNT(*) as total_contacts, 
       COUNT("PhoneNumber") as with_phone, 
       COUNT(*) - COUNT("PhoneNumber") as without_phone 
FROM "Contacts" 
WHERE "UserId" = 'cce6ecbc-1fa3-4ee1-9f4c-232dae5b1e0e';

-- Show sample of imported contacts
SELECT "Name", "PhoneNumber", "IsGroup" 
FROM "Contacts" 
WHERE "UserId" = 'cce6ecbc-1fa3-4ee1-9f4c-232dae5b1e0e' 
LIMIT 10;
