# MiniStravaAPI

- uruchamianie dockera: docker-compose up --build

## Aktualny adres

- https://align-presented-hosted-bids.trycloudflare.com/

## Endpointy API

### Auth / konto
- **POST** `/api/register` – rejestracja
- **POST** `/api/login` – logowanie (JWT)
- **POST** `/api/change-password` – zmiana hasła (wymaga JWT)

### Reset hasła
- **POST** `/api/password/forgot` – generuje token resetu (DEV: może zwracać token)
- **POST** `/api/password/reset` – resetuje hasło tokenem

### Profil
- **GET** `/api/me` – pobierz profil (JWT)
- **PUT** `/api/me` – zaktualizuj profil (JWT)

### Aktywności
- **GET** `/api/activities` – lista Twoich aktywności (JWT, z query do filtrów/sortu jeśli wdrożone)
- **POST** `/api/activities` – utwórz aktywność (JWT)
- **GET** `/api/activities/{id:guid}` – szczegóły aktywności (JWT)
- **PUT** `/api/activities/{id:guid}` – edycja (JWT)
- **DELETE** `/api/activities/{id:guid}` – usuń (JWT)
- **POST** `/api/activities/{id:guid}/trackpoints` – dodaj trackpointy (JWT)
- **GET** `/api/activities/{id:guid}/export/gpx` – eksport GPX (JWT)

### Statystyki / ranking
- **GET** `/api/stats/me` – statystyki użytkownika (JWT)
- **GET** `/api/rankings/weekly` – ranking tygodniowy (JWT)

### Admin (wymaga JWT + rola Admin)
- **GET** `/api/admin/users` – lista użytkowników
- **GET** `/api/admin/activities` – lista aktywności (filtry)
- **DELETE** `/api/admin/activities/{id:guid}` – usuń aktywność
- **GET** `/api/admin/stats` – statystyki globalne

### Synchronizacja (jeśli używana w appce)
- **GET** `/api/sync` – lista sesji sync (JWT)
- **POST** `/api/sync/start` – start sesji sync (JWT)
- **POST** `/api/sync/{sessionId:guid}/finish` – zakończ sesję sync (JWT)

### Swagger / OpenAPI
- Swagger UI: **GET** `/api/documentation`
- JSON spec: **GET** `/api/documentation/v1/swagger.json`

## Dokumentacja Open API
- [dokumentacja](./openapi.json)
