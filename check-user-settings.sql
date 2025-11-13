-- Check user's default settings
SELECT "Id", "DefaultTimeZoneId", "DefaultMessageHour" 
FROM "AspNetUsers" 
WHERE "Id" = 'cce6ecbc-1fa3-4ee1-9f4c-232dae5b1e0e';

-- Check contacts' timezone and message hour settings
SELECT "Id", "Name", "TimeZoneId", "PreferredMessageHour" 
FROM "Contacts" 
WHERE "UserId" = 'cce6ecbc-1fa3-4ee1-9f4c-232dae5b1e0e' 
LIMIT 10;
