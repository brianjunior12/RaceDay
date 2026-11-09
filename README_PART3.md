# RaceDay – Part 3: MVC web application

ASP.NET Core MVC front end for RaceDay. It talks only to the Part 2 API: there is **no database access and no business logic** in this project.
Images (event banners and profile pictures) are uploaded to the API, which stores them in **Azure Blob Storage**. The MVC project never talks to Azure directly.

## Project structure (`part3/raceday_mvc`)
- `Controllers` – one controller per page group (Account, Home, Events, MyEnrolments, MyResults, Profile for participants; Dashboard, ManageEvents, ManageCategories, ManageEnrolments, ManageResults for organisers; Images shows pictures from the API)
- `Services/api_client.cs` – the only class that calls the API; keeps the API session cookie of the logged in user
- `Filters/role_required.cs` – keeps unauthenticated users and the wrong role out of protected pages
- `Views` – Razor pages (Bootstrap 5 + the RaceDay theme in `wwwroot/css/site.css`)
- `Dockerfile` – multi-stage build (SDK image to build, small ASP.NET runtime image to run, non-root user)

## How login and roles work
1. Register or log in on the MVC site. The MVC project calls `POST /api/auth/login` on the API.
2. The API starts a session and sends back a session cookie. The MVC project keeps that cookie, the user id, name and role in **its own session**.
3. Every later API call carries the cookie, so the API knows who is calling.
4. Organisers get the dashboard menu, participants get the events menu. Wrong role = "access denied", not logged in = login page.
5. Log out ends both sessions and redirects to the login page.

## Run it locally
1. Start the API (Part 2) with the `http` profile so it listens on `http://localhost:5080`.
2. Set the Azure Blob Storage connection string for the API (see below).
3. Run `raceday_mvc` (profile `http`) and open `http://localhost:5090`.

### Azure Blob Storage setup (API project)
Right-click `raceday_api` > **Manage User Secrets** and add:
```json
{
  "AzureBlobStorage": {
    "ConnectionString": "<your storage account connection string>",
    "ContainerName": "raceday-images"
  }
}
```
(Azure portal > Storage account > Security + networking > Access keys.) The API creates the private container the first time an image is uploaded. For local testing without an Azure account you can use the Azurite emulator with `UseDevelopmentStorage=true`.

## Run it with Docker
Start the API first (it runs on your computer), then use this one command from the repository root:

```bash
docker build -t raceday-mvc part3/raceday_mvc && docker run --rm -p 8080:8080 -e ApiSettings__BaseUrl=http://host.docker.internal:5080 raceday-mvc
```

Open `http://localhost:8080`. The container behaves exactly like the local run; only the address of the API differs (`host.docker.internal` is your computer as seen from inside the container).

## CI/CD
GitHub Actions (`.github/workflows/part2-ci.yml`) builds and tests the API, builds the MVC project and builds the Docker image on every push.

![Green build](../part2/docs/ci-green-build.png)  <!-- replace with your screenshot -->

## Demo video
YouTube (unlisted): <paste link>
