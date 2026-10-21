# GmachManagement

מערכת לניהול גמ"ח הלוואות ללא ריבית. ‏.NET 8 Web API, ‏EF Core + PostgreSQL, ‏JWT.

## תיאור המערכת
הגמ"ח מחזיק מאגר הלוואות (סכום + תאריך החזר). לקוח מגיש בקשה להלוואה אחת מהמאגר; מנהל מאשר או דוחה (עם סיבה), רושם קבלת כסף, וקובע תאריך חדש כדי להחזיר את ההלוואה למאגר. לכל לקוח יש ציון אמינות הנראה למנהל בלבד (+10 על החזר בזמן, ‏-20 על איחור, מינימום 0; ערכים ב-`appsettings.json`).

תפקידים: `User` (הרשמה פתוחה) ו-`Admin` (מנהל יחיד מה-Seed).

## ארכיטקטורה
| פרויקט | תפקיד |
|---|---|
| Gmach.Core | ישויות, DTOs, ממשקים, חריגים (ללא תלויות) |
| Gmach.Data | EF Core, ‏Migrations, ‏Repositories, ‏Seed |
| Gmach.Service | לוגיקה עסקית, ‏AutoMapper, ‏JWT, ‏BCrypt |
| Gmach.API | ‏Controllers, ‏Middleware, ‏Swagger, ‏NLog |
| Gmach.Tests | ‏xUnit + Moq + SQLite בזיכרון |
| GmachManagement-Client | ‏React (ריפוזיטורי נפרד) |

## המשאב המוגבל והטיפול במקביליות
ההלוואה היא המשאב המוגבל: רק לקוח אחד יכול לתפוס אותה. מחזור החיים: `Available → Reserved → Active → AwaitingRenewal → Available`.

לישות `Loan` יש שדה `Version` (Guid) שמוגדר כ-concurrency token ומוחלף בכל שמירה. אם שני לקוחות מגישים בקשה לאותה הלוואה בו-זמנית, השני מקבל `DbUpdateConcurrencyException`, שה-Repository הופך ל-`ConflictException`, וה-Middleware מחזיר **409 Conflict** עם הודעה ברורה. בנוסף, לקוח עם בקשה פתוחה או הלוואה פעילה לא יכול להגיש בקשה נוספת (409).

## כתובות חיות (Render)
| רכיב | כתובת |
|---|---|
| לקוח (React) | https://gmach-client.onrender.com |
| שרת (API) + Swagger | https://gmach-api.onrender.com/swagger |

להתחברות השתמשו במשתמשי הדמו שבהמשך.

**שימו לב (חבילה חינמית):** השרת "נרדם" אחרי כ-15 דקות ללא פעילות, ולכן הפנייה הראשונה יכולה להימשך כדקה. מומלץ לפתוח קודם את כתובת ה-Swagger, להמתין לטעינה, ורק אז להיכנס ללקוח. בסיס הנתונים החינמי ב-Render תקף כ-30 יום.

## הרצה מקומית
דרישות: ‏.NET 8 SDK ו-PostgreSQL.

1. צרו מסד נתונים ריק, למשל `gmach`.
2. הגדירו סודות (מהתיקייה הראשית):
```
dotnet user-secrets init --project Gmach.API
dotnet user-secrets set "ConnectionStrings:GmachDb" "Host=localhost;Port=5432;Database=gmach;Username=postgres;Password=YOUR_PASSWORD" --project Gmach.API
dotnet user-secrets set "Jwt:Key" "any-random-text-of-at-least-32-characters" --project Gmach.API
dotnet user-secrets set "Seed:AdminPassword" "sC134679" --project Gmach.API
dotnet user-secrets set "Seed:UserPassword" "sC147369" --project Gmach.API
```
3. הרצה (ה-Migrations וה-Seed מתבצעים אוטומטית בעלייה):
```
dotnet run --project Gmach.API --launch-profile Gmach.API
```
4. Swagger: https://localhost:7092/swagger

## משתמשי דמו
| תפקיד | אימייל | סיסמה |
|---|---|---|
| Admin | admin@gmach.local | sC134679 |
| User | user@gmach.local | sC147369 |

## לקוח (React)
הלקוח נמצא בריפוזיטורי נפרד: https://github.com/sheviCohen/GmachManagement-Client

הרצה מקומית (השרת חייב לרוץ במקביל):
```
git clone https://github.com/sheviCohen/GmachManagement-Client.git
cd GmachManagement-Client
npm install
npm run dev
```
הלקוח נפתח בכתובת http://localhost:5173, והשרת מאשר אותו דרך הגדרת `Cors:AllowedOrigins`.

## פריסה
השרת נפרס ב-Render כ-Web Service (Docker, לפי ה-`Dockerfile`) מול PostgreSQL של Render; הלקוח נפרס כ-Static Site. ההגדרות הרגישות (מחרוזת חיבור, מפתח JWT, סיסמאות Seed) נשמרות כמשתני סביבה ב-Render ולא בקוד, למשל `ConnectionStrings__GmachDb`, ‏`Jwt__Key`, ‏`Cors__AllowedOrigins__0` (כתובת הלקוח).

## בדיקות
```
dotnet test Gmach.Tests
```

## לוגים
NLog כותב לקונסולה ולקובץ `logs/gmach-<תאריך>.log`. כל בקשה נרשמת עם `CorrelationId`; סיסמאות וטוקנים אינם נרשמים.

## הערות
- חבילת AutoMapper 12.0.1 מציגה אזהרת אבטחה NU1903; גרסאות חדשות דורשות רישיון, ולכן נשארנו בגרסה זו.
- כל התאריכים נשמרים ב-UTC.
