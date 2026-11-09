# RaceDay – Part 3

RaceDay is an ASP.NET Core event-management solution. Part 2 provides the REST API and automated tests. Part 3 adds an ASP.NET Core MVC frontend that consumes the API through `RaceDayApiClient` and does not access the database directly.

## Solution structure
- `RaceDay.Api` – Part 2 REST API and data layer.
- `RaceDay.Api.Tests` – API integration and authorisation tests.
- `RaceDay.Mvc` – Part 3 MVC user interface.
- `.github/workflows/dotnet.yml` – GitHub Actions restore, build and test pipeline.
- `Dockerfile` – container image for the MVC application.

## Run locally
1. Install the .NET 8 SDK.
2. Configure the API database connection string in `RaceDay.Api/appsettings.json` or user secrets.
3. Start the API: `dotnet run --project RaceDay.Api`.
4. In another terminal, start the MVC app: `dotnet run --project RaceDay.Mvc`.
5. If the API uses a different address, update `Api:BaseUrl` in `RaceDay.Mvc/appsettings.json`.

## Docker
Build the MVC image from the repository root:

```bash
docker build -t raceday-mvc .
```

Run it (set the API URL to an address reachable from the container):

```bash
docker run --rm -p 8080:8080 -e Api__BaseUrl=http://host.docker.internal:5000/ raceday-mvc
```

Open `http://localhost:8080`.

## CI/CD
GitHub Actions runs `dotnet restore`, `dotnet build`, and `dotnet test` for the complete solution on pushes and pull requests.

## Current implementation notes
The MVC application includes registration/login through the Part 2 API, role-specific navigation, organiser event creation/listing/deletion, participant event listing, enrolment history, results history, and profile display. The current Part 2 API does not expose Azure Blob Storage upload endpoints, event banner fields, category-enrolment discovery, profile updates, or organiser result-entry UI. Those features require extending the API and database schema; the MVC client intentionally does not bypass the API or access the database directly. Configure and test the API/database before demonstration.

## Demonstration checklist
- Show registration and login for both roles.
- Show the organiser dashboard and event creation.
- Show the participant event list, enrolments and results.
- Show the GitHub Actions successful build/test run.
- Demonstrate the Docker build and run commands.
