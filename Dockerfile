# ---------- שלב 1: בנייה ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# מעתיקים קודם רק את קבצי הפרויקט ומשחזרים חבילות, כך ש-Docker ישתמש במטמון כשרק הקוד משתנה
COPY Gmach.Core/Gmach.Core.csproj Gmach.Core/
COPY Gmach.Data/Gmach.Data.csproj Gmach.Data/
COPY Gmach.Service/Gmach.Service.csproj Gmach.Service/
COPY Gmach.API/Gmach.API.csproj Gmach.API/
RUN dotnet restore Gmach.API/Gmach.API.csproj

COPY Gmach.Core/ Gmach.Core/
COPY Gmach.Data/ Gmach.Data/
COPY Gmach.Service/ Gmach.Service/
COPY Gmach.API/ Gmach.API/
RUN dotnet publish Gmach.API/Gmach.API.csproj -c Release -o /app --no-restore

# ---------- שלב 2: הרצה (תמונה קטנה, בלי ה-SDK) ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app .

# Render מעביר את הפורט במשתנה PORT. אם הוא חסר, משתמשים ב-10000.
ENV ASPNETCORE_ENVIRONMENT=Production
CMD ["sh", "-c", "ASPNETCORE_URLS=http://0.0.0.0:${PORT:-10000} dotnet Gmach.API.dll"]
