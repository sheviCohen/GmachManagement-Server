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

## המשאב המוגבל והטיפול במקביליות
ההלוואה היא המשאב המוגבל: רק לקוח אחד יכול לתפוס אותה. מחזור החיים: `Available → Reserved → Active → AwaitingRenewal → Available`.

לישות `Loan` יש שדה `Version` (Guid) שמוגדר כ-concurrency token ומוחלף בכל שמירה. אם שני לקוחות מגישים בקשה לאותה הלוואה בו-זמנית, השני מקבל `DbUpdateConcurrencyException`, שה-Repository הופך ל-`ConflictException`, וה-Middleware מחזיר **409 Conflict** עם הודעה ברורה. בנוסף, לקוח עם בקשה פתוחה או הלוואה פעילה לא יכול להגיש בקשה נוספת (409).

## הרצה מקומית
דרישות: ‏.NET 8 SDK ו-PostgreSQL.

1. צרו מסד נתונים ריק, למשל `gmach`.
2. הגדירו סודות (מהתיקייה הראשית):
```
dotnet user-secrets init --project Gmach.API
dotnet user-secrets set "ConnectionStrings:GmachDb" "Host=localhost;Port=5432;Database=gmach;Username=postgres;Password=YOUR_PASSWORD" --project Gmach.API
dotnet user-secrets set "Jwt:Key" "מפתח-אקראי-באורך-32-תווים-לפחות" --project Gmach.API
dotnet user-secrets set "Seed:AdminPassword" "YOUR_ADMIN_PASSWORD" --project Gmach.API
dotnet user-secrets set "Seed:UserPassword" "YOUR_USER_PASSWORD" --project Gmach.API
```
3. הרצה (ה-Migrations וה-Seed מתבצעים אוטומטית בעלייה):
```
dotnet run --project Gmach.API --launch-profile Gmach.API
```
4. Swagger: https://localhost:7092/swagger

## משתמשי דמו
| תפקיד | אימייל | סיסמה |
|---|---|---|
| Admin | admin@gmach.local | הערך של `Seed:AdminPassword` |
| User | user@gmach.local | הערך של `Seed:UserPassword` |

## בדיקות
```
dotnet test Gmach.Tests
```

## לוגים
NLog כותב לקונסולה ולקובץ `logs/gmach-<תאריך>.log`. כל בקשה נרשמת עם `CorrelationId`; סיסמאות וטוקנים אינם נרשמים.

## הערות
- חבילת AutoMapper 12.0.1 מציגה אזהרת אבטחה NU1903; גרסאות חדשות דורשות רישיון, ולכן נשארנו בגרסה זו.
- כל התאריכים נשמרים ב-UTC.
