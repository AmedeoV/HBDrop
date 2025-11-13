-- Check user and contact message hours
SELECT 
    'User' as type,
    "UserName",
    "DefaultTimeZoneId",
    "DefaultMessageHour"
FROM "AspNetUsers"
WHERE "Email" = 'amedeo.vertullo@gmail.com';

SELECT 
    'Contacts' as type,
    "Name",
    "TimeZoneId",
    "PreferredMessageHour"
FROM "Contacts"
WHERE "UserId" = 'cce6ecbc-1fa3-4ee1-9f4c-232dae5b1e0e'
ORDER BY "Name"
LIMIT 10;
