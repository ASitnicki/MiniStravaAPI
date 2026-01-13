# MiniStravaAPI

uruchamianie dockera: docker-compose up --build

# Dostêpne endpointy API:

- POST /api/register — Rejestracja u¿ytkownika (body: RegisterRequest)
- POST /api/login — Logowanie u¿ytkownika, zwraca JWT (body: LoginRequest, response: LoginResponse)
- GET /api/test1 — Endpoint testowy, zwraca listê u¿ytkowników (wymaga JWT)
- GET /api/test2 — Endpoint testowy, zwraca listê u¿ytkowników (bez autoryzacji)
- GET /WeatherForecast — Przyk³adowy endpoint z template ASP.NET (demo)
- GET /api/activities — Lista aktywnoœci zalogowanego u¿ytkownika (JWT wymagany)
- GET /api/activities/{id}?includeTrackPoints={true|false} — Szczegó³y aktywnoœci (opcjonalnie z trackpointami) (JWT wymagany)
- POST /api/activities — Utworzenie aktywnoœci (JWT wymagany, body: CreateActivityRequest)
- PUT /api/activities/{id} — Aktualizacja aktywnoœci (JWT wymagany, body: UpdateActivityRequest)
- DELETE /api/activities/{id} — Usuniêcie aktywnoœci (JWT wymagany)
- POST /api/activities/{id}/trackpoints — Dodanie paczki punktów GPS do aktywnoœci (bez duplikatów po Sequence) (JWT wymagany, body: AddTrackPointsRequest)
- GET /api/sync — Lista sesji synchronizacji u¿ytkownika (JWT wymagany)
- POST /api/sync/start — Start sesji synchronizacji (JWT wymagany)
- POST /api/sync/{sessionId}/finish — Zakoñczenie sesji synchronizacji (JWT wymagany, body: FinishSyncSessionRequest)